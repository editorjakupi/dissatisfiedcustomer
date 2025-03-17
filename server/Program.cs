using Npgsql;
using server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Konfiguration av CORS-policy för att tillåta förfrågningar från lokalhost:5173.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader() // Tillåt alla headers.
              .AllowAnyMethod() // Tillåt alla HTTP-metoder.
              .AllowCredentials(); // Tillåt cookies (autentisering).
    });
});

// Konfiguration av datakällan för PostgreSQL med "Npgsql".
NpgsqlDataSource db = NpgsqlDataSource.Create("Host=localhost;Database=dissatisfiedcustomer");
builder.Services.AddSingleton(db); // Registrerar databasen som singleton.
builder.Services.AddSingleton<PasswordHasher<string>>(); // Registrerar password-hashing för autentisering.
builder.Services.AddDistributedMemoryCache(); // Aktiverar minnescache för sessionhantering.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // Inaktivitetstid för session: 1 timme.
    options.Cookie.HttpOnly = true; // Gör cookien otillgänglig för JavaScript.
    options.Cookie.IsEssential = true; // Markerar cookien som viktig.
    options.Cookie.SameSite = SameSiteMode.Lax; // Tillåter delad cookie för samma domän.
});

var app = builder.Build();
app.UseCors("AllowLocalhost5173"); // Använder CORS-policyn.
app.UseSession(); // Aktiverar sessionshantering.

app.MapGet("/", () => "Hello World!"); // Enkel test-GET endpoint.

// --- API-endpoints för användarhantering ---
app.MapGet("api/users/{id}", (int id) => LoginRoute.GetUser(id, db)); // Hämtar användare baserat på ID.
app.MapGet("/api/usersfromcompany", UserRoutes.GetUsersFromCompanys); // Hämtar användare kopplade till ett företag.
app.MapGet("/api/users", UserRoutes.GetUsers); // Hämtar alla användare.
app.MapPost("/api/users", UserRoutes.PostUser); // Skapar ny användare.
app.MapDelete("/api/users/{id}", UserRoutes.DeleteUser); // Raderar användare.
app.MapPut("/api/users/{userId}", UserRoutes.PutUsers); // Uppdaterar användarinformation.
app.MapPut("/api/promoteuser/{userId}", UserRoutes.PutPromoteAdmin); // Uppgraderar en användare till admin.

/* Tickets */
app.MapGet("/api/tickets", TicketRoutes.GetTickets); // Hämtar biljetter baserat på vy.
app.MapGet("/api/tickets/{id}", (int id) => TicketRoutes.GetTicket(id, db)); // Hämtar specifik biljett.
app.MapPut("/api/ticketscategory", TicketRoutes.PutTicketCategory); // Uppdaterar kategori för biljett.
app.MapPut("/api/ticketstatus", async (int ticketId, int status, NpgsqlDataSource db) =>
{
    return await TicketRoutes.PutTicketStatus(ticketId, status, db); // Uppdaterar biljettens status.
});
app.MapPut("/api/ticketsproduct", TicketRoutes.PutTicketProducts); // Uppdaterar produkt för biljett.
app.MapGet("/api/ticketstatus", TicketStatusRoutes.GetTicketStatus); // Hämtar alla biljettstatusar.
app.MapPut("/api/tickets/{id}", (int id) => TicketRoutes.UpdateTicketStatus(id, db)); // Återställer biljettens status (till exempel till oläst).

app.MapPost("/api/feedback", FeedbackRoutes.PostFeedback);

// --- Produktendpoints ---
app.MapGet("/api/products/{company_id}", (int company_id) => ProductRoute.GetProducts(company_id, db)); // Hämtar produkter kopplade till företag.
app.MapGet("/api/product/{product_id}", (int product_id) => ProductRoute.GetProduct(product_id, db)); // Hämtar en specifik produkt.
app.MapPost("/api/products", ProductRoute.PostProduct); // Skapar ny produkt.
app.MapDelete("/api/products/{id}", ProductRoute.DeleteProduct); // Raderar produkt.
app.MapPut("/api/products/{id}", ProductRoute.UpdateProduct); // Uppdaterar produktens information.

/* Employee */
app.MapGet("/api/employees/{userId}", (int userId) => EmployeeRoute.GetEmployees(userId, db)); // Hämtar anställda kopplade till användar-ID.
app.MapGet("/api/employee/{comapnyId}", (int comapnyId) => EmployeeRoute.GetEmployee(comapnyId, db)); // Hämtar anställda kopplade till företags-ID.
app.MapPost("/api/employees", EmployeeRoute.PostEmployee); // Skapar ny anställd.
app.MapDelete("/api/employees/{userId}", (int userId) => EmployeeRoute.DeleteEmployee(userId, db)); // Raderar anställd.

/* Login / Session endpoints */
app.MapPost("/api/login", LoginRoute.LoginUser); // Hanterar inloggning.
app.MapGet("/api/session", LoginRoute.GetSessionUser); // Hämtar sessionens användarinformation.
app.MapPost("/api/logout", LoginRoute.LogoutUser); // Hanterar utloggning.


/* Ticket Form endpoints */
app.MapPost("/api/ticketform", TicketFormRoutes.PostTicketForm); // Skapar nytt ärende från formulär.
app.MapGet("/api/ticketform", (string caseNumber) => TicketFormRoutes.GetTicketForm(caseNumber, db)); // Hämtar formulärdata.

/* Category endpoints */
app.MapGet("/api/categories", CategoryRoutes.GetCategories); // Hämtar kategorier.

/* Case endpoints */
// Anställda lägger till meddelanden i ärenden via dashboard.
app.MapPost("/api/tickets/handle/{ticketId}/messages", async (int ticketId, Message message, HttpContext context, NpgsqlDataSource db) =>
{
    string senderType = "employee";
    string employeeEmail = "customerservice@company.com"; // Standard e-post för anställda.
    message = new Message(employeeEmail, message.Content);
    var result = await CaseRoutes.AddCaseMessageByEmail(employeeEmail, ticketId, message, senderType, db);
    return result;
});


/* Message endpoints */
app.MapGet("/api/demoinfo/{company_id}", (int company_id) => MessageRoutes.GetCatAndProd(company_id, db)); // Hämtar kategorier och produkter för demo.
app.MapPost("/api/messages", async (MessageDTO message, HttpContext context, NpgsqlDataSource db) =>
{
    return await MessageRoutes.PostMessage(message, context, db); // Lägger till nytt meddelande.
}).AllowAnonymous(); // Endast anonym åtkomst.


// Kund skickar meddelanden via token.
app.MapPost("/api/tickets/view/{token}/messages", async (string token, Message message, HttpContext context, NpgsqlDataSource db) =>
{
    var ticket = await TicketRoutes.GetTicketByToken(token, db);
    if (ticket == null)
        return NotFound("Ticket not found."); // Returnerar fel om biljetten inte finns.
    string senderType = "customer";
    var result = await CaseRoutes.AddCaseMessageByEmail(ticket.Email, ticket.Id, message, senderType, db);
    return result;
});

// Hämtar alla meddelanden kopplade till ett ärende.
app.MapGet("/api/tickets/{ticketId}/messages", async (int ticketId, NpgsqlDataSource db) =>
{
    var msgs = await CaseRoutes.GetCaseMessagesByEmail(ticketId, db);
    return Ok(msgs); // Returnerar meddelanden.
});

// Hämtar biljett via token för kundens chattvy.
app.MapGet("/api/tickets/view/{token}", async (string token, NpgsqlDataSource db, HttpContext context) =>
{
    context.Session.SetString("UserSession", token); // Skapar session med token.
    Console.WriteLine($"Session created with token: {token}");
    var ticket = await TicketRoutes.GetTicketByToken(token, db);
    return ticket != null ? Ok(ticket) : NotFound(); // Returnerar biljett eller NotFound.
});

/* SuperAdmin API calls */
app.MapGet("/api/adminlist", SuperAdminRoutes.GetAdmins); // Hämtar alla administratörer.
app.MapGet("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.GetAdmin(userId, db)); // Hämtar en administratör.
app.MapPut("/api/adminlist/{userId}", (int userId) => SuperAdminRoutes.PutAdmin(userId, db)); // Uppdaterar administratörsinformation.
app.MapDelete("/api/company/admins/{userId}", (int userId) => SuperAdminRoutes.DeleteAdmin(userId, db)); // Raderar administratör.
app.MapPut("/api/putuser/{userId}", UserRoutes.PutUserForSAdmin); // Uppdaterar användarinformation för superadmin.
app.MapGet("/api/tickets/feedback", TicketRoutes.Feedbacks); // Hämtar feedback kopplat till biljetter.
app.MapPost("/api/password/hash", LoginRoute.HashPassword); // Genererar hash för lösenord.

app.Run(); // Startar applikationen.
