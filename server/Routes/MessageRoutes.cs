using Npgsql; // För att hantera databasoperationer med PostgreSQL.
using Microsoft.AspNetCore.Http.HttpResults; // Ger standardiserade HTTP-svar som Created och BadRequest.
using MailKit.Net.Smtp; // SMTP-klient för att skicka e-postmeddelanden.
using MimeKit; // För att skapa och hantera e-postinnehåll.
using MailKit.Security; // För säker e-postkommunikation, som TLS.
using System; // Standardfunktioner som undantagshantering och GUID-generering.
using System.Data.Common; // Generiska databasoperationer och anslutningar.
using Microsoft.AspNetCore.StaticAssets; // Hanterar statiska resurser 

namespace server; 

public static class MessageRoutes // Klass som hanterar kundmeddelanden och relaterad logik.
{
    // Hämtar kategorier och produkter för ett specifikt företag.
    // Kombinerar två datakällor: kategorier och produkter, till ett objekt.
    public static async Task<CatAndProd> GetCatAndProd(int company_id, NpgsqlDataSource db)
    {
        return new CatAndProd(
            CategoryRoutes.GetCategories(db).Result, // Hämtar kategorier från databasen.
            ProductRoute.GetProducts(company_id, db).Result // Hämtar produkter kopplade till det angivna företaget.
        );
    }

    // Hanterar inkommande meddelanden från kunder och tilldelar dem ett ärende.
    // Validerar data, garanterar dataintegritet med transaktioner och skickar en bekräftelse via e-post.
    public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, HttpContext context, NpgsqlDataSource db)
    {
        // Loggar inkommande meddelandedetaljer.
        Console.WriteLine($"Meddelande mottaget - E-post: {message.Email}, Namn: {message.Name}, Innehåll: {message.Content}, Företags-ID: {message.CompanyID}, Kategori-ID: {message.CategoryID}, Produkt-ID: {message.ProductID}");

        // Validerar att alla obligatoriska fält är korrekt ifyllda.
        if (string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Content) || message.CategoryID == null)
        {
            return TypedResults.BadRequest("E-post, Namn, Innehåll och Kategori krävs.");
        }

        using var conn = db.CreateConnection(); // Skapar en databasanslutning.
        await conn.OpenAsync(); // Öppnar anslutningen.

        // Påbörjar en transaktion för att säkerställa säkerhet och atomära operationer.
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Skapar ett nytt ärende eller hämtar ett befintligt kopplat till kundens meddelande.
            int ticketId = await CreateTicketForCustomerAsync(message.Email, message.Name, message.Content, message.CompanyID, message.CategoryID, message.ProductID, conn, transaction);

            // Kontrollerar att ärendet inte är stängt eller löst.
            int currentStatus = await GetTicketStatus(ticketId, conn, transaction);
            if (currentStatus == 3 || currentStatus == 4) // Status 3 = Resolved, 4 = Closed.
            {
                return TypedResults.BadRequest("Kan inte lägga till meddelande till ett stängt eller löst ärende.");
            }

            // Lägger till meddelandet i databasen.
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "INSERT INTO messages (ticket_id, message, email) VALUES ($1, $2, $3)";
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(message.Email);
            await cmd.ExecuteNonQueryAsync(); // Kör insättningen i databasen.

            // Bekräftar transaktionen om alla operationer lyckas.
            await transaction.CommitAsync();

            // Hämtar den unika token (case_number) kopplad till ärendet.
            string token = GetTicketToken(ticketId, conn, transaction);

            // Lagrar token i användarens session för framtida referens.
            context.Session.SetString("UserSession", token);
            Console.WriteLine($"Session skapad med token: {token}");

            // Skickar en bekräftelse via e-post till kunden.
            await SendConfirmationEmailAsync(message.Email, message.Name, message.Content, token);

            return TypedResults.Created();
        }
        catch (Exception ex) // Hanterar fel och loggar dem.
        {
            Console.WriteLine($"Undantag: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Rullar tillbaka transaktionen vid fel.
            if (transaction != null && transaction.Connection != null)
            {
                try { await transaction.RollbackAsync(); }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"Återställningsundantag: {rollbackEx.Message}");
                    Console.WriteLine($"Stack Trace: {rollbackEx.StackTrace}");
                }
            }
            return TypedResults.BadRequest($"Ett fel uppstod: {ex.Message}");
        }
        finally
        {
            // Stänger databasanslutningen när allt är klart.
            if (conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    // Skapar ett nytt ärende för kunden och returnerar det genererade ärende-ID:t.
    private static async Task<int> CreateTicketForCustomerAsync(string email, string title, string description, int company_id, int? category_id, int? product_id, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;

        string token = GenerateUniqueCaseNumber(); // Genererar ett unikt identifieringsnummer för ärendet.

        // Lägger till ett nytt ärende i databasen.
        cmd.CommandText = "INSERT INTO tickets (user_email, title, description, case_number, company_id, date, status_id, category_id, product_id) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9) RETURNING id";
        cmd.Parameters.AddWithValue(email);
        cmd.Parameters.AddWithValue(title);
        cmd.Parameters.AddWithValue(description);
        cmd.Parameters.AddWithValue(token);
        cmd.Parameters.AddWithValue(company_id);
        cmd.Parameters.AddWithValue(DateTime.UtcNow); // Sätter skapelsedatumet.
        cmd.Parameters.AddWithValue(1); // Standardstatus är "Unread".
        cmd.Parameters.AddWithValue(category_id);
        cmd.Parameters.AddWithValue(product_id);

        var scalar = await cmd.ExecuteScalarAsync(); // Hämtar det nya ärende-ID:t från databasen.
        if (scalar == null)
        {
            throw new InvalidOperationException("Misslyckades med att skapa ärende; returnerat ID är null.");
        }
        return (int)scalar;
    }

    // Hämtar den unika token (case_number) för ett angivet ärende-ID.
    private static string GetTicketToken(int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT case_number FROM tickets WHERE id = $1";
        cmd.Parameters.AddWithValue(ticketId);

        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? "";
    }

    // Hämtar aktuell status för ett ärende med hjälp av dess ID.
    private static async Task<int> GetTicketStatus(int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT status_id FROM tickets WHERE id = $1";
        cmd.Parameters.AddWithValue(ticketId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null)
        {
            throw new InvalidOperationException("Ingen status hittades för ärendet.");
        }
        return Convert.ToInt32(result);
    }

    // Genererar ett slumpmässigt lösenord.
    public static string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString().Substring(0, 8); // Returnerar de första 8 tecknen i en GUID som lösenord.
    }

    // Genererar ett unikt token för ett ärende.
    private static string GenerateUniqueCaseNumber()
    {
        return $"CASE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"; // Skapar ett identifieringsnummer i formatet CASE-XXXX.
    }

    // Skickar en bekräftelse via e-post till kunden med detaljer om ärendet och en länk för att komma åt det.
    private static async Task SendConfirmationEmailAsync(string email, string name, string content, string token)
    {
        string baseUrl = "http://localhost:5173"; // Frontend-URL för att komma åt ärendet.

        // Innehåll i e-postmeddelandet.
        string messageBody = $"Hej {name ?? "Kund"},\n\nVi har mottagit ditt meddelande:\n\n\"{content}\"\n\n" +
                                                         $"För att komma åt din chatt med kundsupport, vänligen klicka på länken nedan inom den närmaste timmen:\n" +
                             $"{baseUrl}/tickets/view/{token}\n\n" +
                             $"Tack för att du kontaktade oss.\n\nMed vänliga hälsningar,\nDitt App-team";

        // Skapa e-postmeddelandet.
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Ditt Appnamn", "dissatisfiedcustomer2025@gmail.com")); // Avsändare av e-post.
        mimeMessage.To.Add(new MailboxAddress(name ?? "Kund", email)); // Mottagare av e-post.
        mimeMessage.Subject = "Bekräftelse på din kontakt med kundsupport"; // Ämnesrad.
        mimeMessage.Body = new TextPart("plain") { Text = messageBody }; // Meddelandets innehåll.

        try
        {
            using var smtpClient = new SmtpClient();
            // För teständamål, inaktivera certifikatvalidering.
            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            smtpClient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls); // Anslut till SMTP-server med TLS.
            smtpClient.Authenticate("dissatisfiedcustomer2025@gmail.com", "yxel egbr xehm wdrt"); // Autentisering.
            await smtpClient.SendAsync(mimeMessage); // Skicka e-postmeddelandet.
            await smtpClient.DisconnectAsync(true); // Koppla från SMTP-servern.
            Console.WriteLine("Bekräftelsemeddelande skickades framgångsrikt!");
        }
        catch (Exception ex)
        {
            // Hantera eventuella fel som uppstår vid e-postsändning.
            Console.WriteLine($"E-postsändning misslyckades: {ex.Message}");
            throw; // Vidarebefordrar undantaget för vidare hantering.
        }
    }
}
