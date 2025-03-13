using Npgsql;
using server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

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

NpgsqlDataSource db = NpgsqlDataSource.Create("Host=localhost;Database=dissatisfiedcustomer");
builder.Services.AddSingleton(db);
builder.Services.AddSingleton<PasswordHasher<string>>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
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
app.MapPut("/api/ticketstatus", async (int ticketId, int status, NpgsqlDataSource db) =>
{
    return await TicketRoutes.PutTicketStatus(ticketId, status, db);
});
app.MapPut("/api/ticketsproduct", TicketRoutes.PutTicketProducts);
app.MapGet("/api/ticketstatus", TicketStatusRoutes.GetTicketStatus);
app.MapPut("/api/tickets/{id}", (int id) => TicketRoutes.UpdateTicketStatus(id, db));

// Produkt endpoints
app.MapGet("/api/products/{company_id}", (int company_id) => ProductRoute.GetProducts(company_id, db));
app.MapGet("/api/product/{product_id}", (int product_id) => ProductRoute.GetProduct(product_id, db));
app.MapPost("/api/products", ProductRoute.PostProduct);
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct);
app.MapPut("/api/products/{id}", ProductRoute.UpdateProduct);

/* Employee */
app.MapGet("/api/employees/{userId}", (int userId) => EmployeeRoute.GetEmployees(userId, db));
app.MapGet("/api/employee/{comapnyId}", (int comapnyId) => EmployeeRoute.GetEmployee(comapnyId, db));
app.MapPost("/api/employees", EmployeeRoute.PostEmployee);
app.MapDelete("/api/employees/{userId}", (int userId) => EmployeeRoute.DeleteEmployee(userId, db));

/* Login / Session endpoints */

//Company Endpoints
app.MapPost("/api/company", CompanyRoutes.PostCompany);
app.MapDelete("/api/company/{id}", CompanyRoutes.DeleteCompany);
app.MapGet("/api/company/{id}", CompanyRoutes.GetCompany);
app.MapGet("/api/company/", CompanyRoutes.GetCompanies);
app.MapPut("/api/company/{id}", CompanyRoutes.PutCompany);
app.MapGet("/api/company/admins",CompanyRoutes.GetAdmins);

/* Login */
app.MapPost("/api/login", LoginRoute.LoginUser);
app.MapGet("/api/session", LoginRoute.GetSessionUser);
app.MapPost("/api/logout", LoginRoute.LogoutUser);

/* Message endpoints */
app.MapPost("/api/messages", async (MessageDTO message, HttpContext context, NpgsqlDataSource db) =>
{
    return await MessageRoutes.PostMessage(message, context, db);
}).AllowAnonymous();

/* Ticket Form endpoints */
app.MapPost("/api/ticketform", TicketFormRoutes.PostTicketForm);
app.MapGet("/api/ticketform", (string caseNumber) => TicketFormRoutes.GetTicketForm(caseNumber, db));

/* Category endpoints */
app.MapGet("/api/categories", CategoryRoutes.GetCategories);

// --- CASE ENDPOINTS ---
// Employee endpoint: används av anställda (via dashboard)
app.MapPost("/api/tickets/handle/{ticketId}/messages", async (int ticketId, Message message, HttpContext context, NpgsqlDataSource db) =>
{
    string senderType = "employee";
    string employeeEmail = "customerservice@company.com";
    message = new Message(employeeEmail, message.Content);
    var result = await CaseRoutes.AddCaseMessageByEmail(employeeEmail, ticketId, message, senderType, db);
    return result;
});

// Customer endpoint: används av kunder via token
app.MapPost("/api/tickets/view/{token}/messages", async (string token, Message message, HttpContext context, NpgsqlDataSource db) =>
{
    var ticket = await TicketRoutes.GetTicketByToken(token, db);
    if (ticket == null)
        return NotFound("Ticket not found.");
    string senderType = "customer";
    var result = await CaseRoutes.AddCaseMessageByEmail(ticket.Email, ticket.Id, message, senderType, db);
    return result;
});

// GET endpoint för meddelanden – baserat på ticketId
app.MapGet("/api/tickets/{ticketId}/messages", async (int ticketId, NpgsqlDataSource db) =>
{
    var msgs = await CaseRoutes.GetCaseMessagesByEmail(ticketId, db);
    return Ok(msgs);
});

// Ticket retrieval via token (för kundens chattvy)
app.MapGet("/api/tickets/view/{token}", async (string token, NpgsqlDataSource db, HttpContext context) =>
{
    context.Session.SetString("UserSession", token);
    Console.WriteLine($"Session created with token: {token}");
    var ticket = await TicketRoutes.GetTicketByToken(token, db);
    return ticket != null ? Ok(ticket) : NotFound();
});

//Super-Admin API calls
app.MapGet("/api/adminlist", SuperAdminRoutes.GetAdmins);
app.MapGet("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.GetAdmin(userId, db));
app.MapPut("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.PutAdmin(userId, db));
app.MapDelete("/api/company/admins/{userId}", (int userId) => SuperAdminRoutes.DeleteAdmin(userId, db));


app.MapPut("/api/putuser/{userId}", UserRoutes.PutUserForSAdmin);

app.MapGet("/api/tickets/feedback", TicketRoutes.Feedbacks);

app.MapPost("/api/password/hash", LoginRoute.HashPassword);

app.Run();
