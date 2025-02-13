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
app.MapPost("/api/users", UserRoutes.PostUser);
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser);


// Meddelande-API:er
app.MapPost("/api/messages", MessageRoutes.PostMessage);

app.Run();