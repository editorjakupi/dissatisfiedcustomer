using Npgsql;
using server;

var builder = WebApplication.CreateBuilder(args);

// User & Password set by operationsystem environment variables PGUSER & PGPASSWORD
NpgsqlDataSource db = NpgsqlDataSource.Create("Host=localhost;Database=dissatisfiedcustomer");
builder.Services.AddSingleton<NpgsqlDataSource>(db);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("api/users/{id}", (int id) => LoginRoute.GetUser(id, db));
app.MapGet("/api/users", UserRoutes.GetUsers);
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapPost("/api/users", UserRoutes.PostUser);
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser);

//product APIs
//app.MapGet("/api/products", (int company_id) => ProductRoutes.GetProducts(company_id, db));
app.MapGet("/api/products/{company_id}", (int company_id) => ProductRoute.GetProducts(company_id, db));
app.MapGet("/api/product/{product_id}", (int product_id) => ProductRoute.GetProduct(product_id, db));
app.MapPost("/api/products", ProductRoute.PostProduct);
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct);
app.MapPut("/api/products/{id}", ProductRoute.UpdateProduct);

app.MapGet("/api/employees/{userId}", (int userId) => EmployeeRoute.GetEmployees(userId, db));
app.MapGet("/api/employee/{user_id}", (int user_id) => EmployeeRoute.GetEmployee(user_id, db));
app.MapPost("/api/employees", EmployeeRoute.PostEmployee);
//app.MapPut ///api/employees/{id}
app.MapDelete("/api/employees/{userId}/{companyId}", (int userId, int companyId) => EmployeeRoute.DeleteEmployee(userId, companyId, db));

app.MapPost("/api/login", LoginRoute.LoginUser);

app.MapPut("/api/users", UserRoutes.PutUsers);

// Meddelande-API:er
app.MapPost("/api/messages", MessageRoutes.PostMessage);



app.MapPost("/api/ticketform", TicketFormRoutes.PostTicketForm);
app.MapGet("/api/ticketform", (string caseNumber) => TicketFormRoutes.GetTicketForm(caseNumber, db));

// Category api:s
app.MapGet("/api/categories", CategoryRoutes.GetCategories);
// Lägg till den nya GET-endpointen för att visa ärendedetaljer och statusar
app.MapGet("/user/{id}/cases/{caseId}", (int id, int caseId, NpgsqlDataSource db) => CaseRoutes.GetCaseDetails(id, caseId, db));

app.Run();