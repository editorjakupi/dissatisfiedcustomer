using Npgsql;
using server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

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
builder.Services.AddSingleton<PasswordHasher<string>>();
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
app.MapGet("/api/usersfromcompany", UserRoutes.GetUsersFromCompanys);
app.MapGet("/api/users", UserRoutes.GetUsers);
app.MapPost("/api/users", UserRoutes.PostUser);
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser);
app.MapPut("/api/users/{userId}", UserRoutes.PutUsers);
app.MapPut("/api/promoteuser/{userId}", UserRoutes.PutPromoteAdmin);

/* Tickets */
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapGet("/api/tickets/{id}", (int id) => TicketRoutes.GetTicket(id, db));
app.MapPut("/api/ticketscategory", TicketRoutes.PutTicketCategory);
app.MapPut("/api/ticketstatus", TicketRoutes.PutTicketStatus);
app.MapPut("/api/ticketsproduct", TicketRoutes.PutTicketProduct);
app.MapGet("/api/ticketstatus", TicketStatusRoutes.GetTicketStatus);
app.MapPut("/api/tickets/{id}", (int id) => TicketRoutes.UpdateTicketStatus(id, db));

app.MapPost("/api/feedback", FeedbackRoutes.PostFeedback);

// Produkt endpoints
app.MapGet("/api/products/{company_id}", (int company_id) => ProductRoute.GetProducts(company_id, db));
app.MapGet("/api/product/{product_id}", (int product_id) => ProductRoute.GetProduct(product_id, db));
app.MapPost("/api/products", ProductRoute.PostProduct);
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct);
app.MapPut("/api/products/{id}", ProductRoute.UpdateProduct);

/* Employee */
app.MapGet("/api/employees/{userId}", (int userId) => EmployeeRoute.GetEmployees(userId, db));
app.MapGet("/api/employee/{user_id}", (int user_id) => EmployeeRoute.GetEmployee(user_id, db));
app.MapPost("/api/employees", EmployeeRoute.PostEmployee);
app.MapDelete("/api/employees/{userId}", (int userId) => EmployeeRoute.DeleteEmployee(userId, db));

/* Login / Session endpoints */

//Company Endpoints
app.MapPost("/api/company", CompanyRoutes.PostCompany);
app.MapDelete("/api/company/{id}", CompanyRoutes.DeleteCompany);
app.MapGet("/api/company/{id}", CompanyRoutes.GetCompany);
app.MapGet("/api/company/", CompanyRoutes.GetCompanies);
app.MapPut("/api/company/{id}", CompanyRoutes.PutCompany);

/* Login */
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

//Super-Admin API calls
app.MapGet("/api/adminlist", SuperAdminRoutes.GetAdmins);
app.MapGet("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.GetAdmin(userId, db));
app.MapPut("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.PutAdmin(userId, db));


app.MapPost("/api/password/hash", LoginRoute.HashPassword);

app.Run();
