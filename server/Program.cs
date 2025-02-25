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
app.MapDelete("/api/employees/{userId}", (int userId) => EmployeeRoute.DeleteEmployee(userId, db));
//app.MapPut ///api/employees/{id}

//Company Endpoints
app.MapPost("/api/company", CompanyRoutes.PostCompany);
app.MapDelete("/api/company/{id}", CompanyRoutes.DeleteCompany);
app.MapGet("/api/company/{id}", CompanyRoutes.GetCompany);
app.MapGet("/api/company/", CompanyRoutes.GetCompanies);
app.MapPut("/api/company/{id}", CompanyRoutes.PutCompany);

app.MapPost("/api/login", LoginRoute.LoginUser);

app.MapPut("/api/users", UserRoutes.PutUsers);


app.MapPost("/api/ticketform", TicketFormRoutes.PostTicketForm);
app.MapGet("/api/ticketform", (string caseNumber) => TicketFormRoutes.GetTicketForm(caseNumber, db));

// Category API:s
app.MapGet("/api/categories", CategoryRoutes.GetCategories);

// Meddelande-API:er
app.MapPost("/api/messages", MessageRoutes.PostMessage);

// Nya rutter för att hämta ärenden och ärendedetaljer för en specifik användare
app.MapGet("/api/user/{id}/cases", (int id, NpgsqlDataSource db) => CaseRoutes.GetUserCases(id, db));
app.MapGet("/api/user/{id}/cases/{caseId}", (int id, int caseId, NpgsqlDataSource db) => CaseRoutes.GetCaseDetails(id, caseId, db));
//Rutt för att lägga till ett meddelande till ett befintligtärende
app.MapPost("/api/user/{id}/cases/{caseId}/messages", (int id, int caseId, Message message, NpgsqlDataSource db) => CaseRoutes.AddCaseMessage(id, caseId, message, db));
// Rutt för att hitta meddelanden till ett specifiktärende
app.MapGet("/api/user/{id}/cases/{caseId}/messages", (int id, int caseId, NpgsqlDataSource db) => CaseRoutes.GetCaseMessages(id, caseId, db));

app.Run();
