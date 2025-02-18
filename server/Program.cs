using Npgsql;
using server;

var builder = WebApplication.CreateBuilder(args);

// User & Password set by operationsystem environment variables PGUSER & PGPASSWORD
NpgsqlDataSource db = NpgsqlDataSource.Create("Host=localhost;Database=dissatisfiedcustomer");
builder.Services.AddSingleton<NpgsqlDataSource>(db);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("api/login/{id}", (int id) => LoginRoute.GetUser(id, db));
app.MapGet("/api/users", UserRoutes.GetUsers);
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapPost("/api/users", UserRoutes.PostUser);
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser);

app.MapGet("/api/products/{company_id}",(int company_id) => ProductRoute.GetProducts(company_id, db));
app.MapPost("/api/products", ProductRoute.PostProduct);
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct);

app.MapGet("/api/employees/{userId}",(int userId) => EmployeeRoute.GetEmployees(userId, db));

app.MapPost("/api/login", async (HttpContext context, NpgsqlDataSource db) =>
{
    try
    {
        var request = await context.Request.ReadFromJsonAsync<LoginRequest>();
        if (request == null) return Results.BadRequest("Invalid request");

        await using var connection = await db.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", connection);
        command.Parameters.AddWithValue("@email", request.email);

        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return Results.Unauthorized();

        await reader.ReadAsync();
        var user = new Users(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetInt32(5)
        );

        if (user.password != request.password) // Plain text comparison
            return Results.Unauthorized();

        return Results.Ok(new
        {
            id = user.id,
            name = user.name,
            email = user.email,
            phoneNumber = user.phonenumber,
            roleId = user.role_id
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.StatusCode(500);
    }
});


// Meddelande-API:er
app.MapPost("/api/messages", MessageRoutes.PostMessage);

app.Run();