using Npgsql;
using server; // Inkludera Records.cs och andra klasser via namespace "server"

var builder = WebApplication.CreateBuilder(args);

// Lägg till CORS-tjänster med stöd för credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// User & Password set by operationsystem environment variables PGUSER & PGPASSWORD
NpgsqlDataSource db = NpgsqlDataSource.Create("Host=localhost;Database=dissatisfiedcustomer");
builder.Services.AddSingleton<NpgsqlDataSource>(db);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // 1 timme sessionens varaktighet
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; // Alternativt SameSiteMode.None med Secure=true vid HTTPS
});


var app = builder.Build();
app.UseCors("AllowLocalhost5173");
app.UseSession();

app.MapGet("/", () => "Hello World!");

// Användar endpoints (för employees, admin, superadmin)
app.MapGet("api/users/{id}", (int id) => LoginRoute.GetUser(id, db));
app.MapGet("/api/users", UserRoutes.GetUsers);
app.MapPost("/api/users", UserRoutes.PostUser);
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser);
app.MapPut("/api/users", UserRoutes.PutUsers);

/* Tickets */
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapGet("/api/tickets/{id}", (int id) => TicketRoutes.GetTicket(id, db));
app.MapPut("/api/ticketscategory", TicketRoutes.PutTicketCategory);
app.MapPut("/api/ticketstatus", TicketRoutes.PutTicketStatus);
app.MapGet("/api/ticketstatus", TicketStatusRoutes.GetTicketStatus);
app.MapPut("/api/tickets/{id}", (int id) => TicketRoutes.UpdateTicketStatus(id, db));

// Produkt endpoints
app.MapGet("/api/products/{company_id}", (int company_id) => ProductRoute.GetProducts(company_id, db));
app.MapGet("/api/product/{product_id}", (int product_id) => ProductRoute.GetProduct(product_id, db));
app.MapPost("/api/products", ProductRoute.PostProduct);
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct);
app.MapPut("/api/products/{id}", ProductRoute.UpdateProduct);

/* Employee endpoints */
app.MapGet("/api/employees/{email}", (string email) => EmployeeRoute.GetEmployeeByEmail(email, db));
app.MapPost("/api/employees", EmployeeRoute.PostEmployee);
app.MapDelete("/api/employees/{email}", (string email) => EmployeeRoute.DeleteEmployeeByEmail(email, db));

/* Login / Session endpoints */
app.MapPost("/api/login", LoginRoute.LoginUser);
app.MapGet("/api/session", LoginRoute.GetSessionUser);
app.MapPost("/api/logout", LoginRoute.LogoutUser);

/* Message endpoints */
app.MapPost("/api/messages", async (MessageDTO message, HttpContext context, NpgsqlDataSource db) =>
{
    var result = await MessageRoutes.PostMessage(message, context, db);
    return result;
}).AllowAnonymous(); // Tillåt anonym åtkomst

/* Ticket Form endpoints */
app.MapPost("/api/ticketform", TicketFormRoutes.PostTicketForm);
app.MapGet("/api/ticketform", (string caseNumber) => TicketFormRoutes.GetTicketForm(caseNumber, db));

/* Category endpoints */
app.MapGet("/api/categories", CategoryRoutes.GetCategories);

/* Case endpoints (för support) - använder email istället för id */
// app.MapGet("/api/user/{email}/cases", (string email, NpgsqlDataSource db) => CaseRoutes.GetUserCasesByEmail(email, db));
// app.MapGet("/api/user/{email}/cases/{caseId}", (string email, int caseId, NpgsqlDataSource db) => CaseRoutes.GetCaseDetailsByEmail(email, caseId, db));
app.MapPost("/api/user/{email}/cases/{caseId}/messages", (string email, int caseId, Message message, NpgsqlDataSource db) => CaseRoutes.AddCaseMessageByEmail(email, caseId, message, db));
app.MapGet("/api/user/{email}/cases/{caseId}/messages", (string email, int caseId, NpgsqlDataSource db) => CaseRoutes.GetCaseMessagesByEmail(email, caseId, db));

/* Ticket endpoints (för support) - använder token istället för id */
app.MapGet("/api/tickets/view/{token}", async (string token, NpgsqlDataSource db, HttpContext context) =>
{
    // Skapa session när token används
    context.Session.SetString("UserSession", token);
    Console.WriteLine($"Session created with token: {token}");
    var ticket = await TicketRoutes.GetTicketByToken(token, db);
    return ticket != null ? Results.Ok(ticket) : Results.NotFound();
});

app.Run();
