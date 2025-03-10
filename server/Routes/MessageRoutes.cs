using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using System.Data.Common;
using Microsoft.AspNetCore.StaticAssets;

namespace server;

public static class MessageRoutes
{
    public static async Task<CatAndProd>
    GetCatAndProd(int company_id, NpgsqlDataSource db)
    {
        return new CatAndProd(
                
            CategoryRoutes.GetCategories(db).Result,
            ProductRoute.GetProducts(company_id, db).Result);
    }
    

    // Metod för att hantera POST /api/messages för kundens meddelande.
    // UPPDATERAD: Vi tar inte längre emot/skapar user_id, utan använder Email och genererar en token för kundsession
    public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, HttpContext context, NpgsqlDataSource db)
    {
        Console.WriteLine($"Received Message - Email: {message.Email}, Name: {message.Name}, Content: {message.Content}, CompanyID: {message.CompanyID}, CategoryID: {message.CategoryID}, ProductID: {message.ProductID}");

        // Validera inkommande data.
        if (string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Content) || message.CategoryID == null)
        {
            return TypedResults.BadRequest("Email, Name, Content, and Category are required.");
        }

        using var conn = db.CreateConnection();
        await conn.OpenAsync();

        // Starta en transaktion för att säkerställa dataintegritet.
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            int ticketId = await CreateTicketForCustomerAsync(message.Email, message.Name, message.Content, message.CompanyID, message.CategoryID, message.ProductID, conn, transaction);

            await using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "INSERT INTO messages (ticket_id, message, email) VALUES ($1, $2, $3)";
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(message.Email);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Message inserted successfully!");

            await transaction.CommitAsync();

            // Hämta det genererade tokenet från ärendet (lagrat i case_number).
            string token = GetTicketToken(ticketId, conn, transaction);

            // Skapa en session och sätt token som sessionsvärde.
            context.Session.SetString("UserSession", token);
            Console.WriteLine($"Session created with token: {token}");

            await SendConfirmationEmailAsync(message.Email, message.Name, message.Content, token);

            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (transaction != null && transaction.Connection != null)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"Rollback Exception: {rollbackEx.Message}");
                    Console.WriteLine($"Rollback Stack Trace: {rollbackEx.StackTrace}");
                }
            }
            return TypedResults.BadRequest($"An error occurred: {ex.Message}");
        }
        finally
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                await conn.CloseAsync();
            }
        }
    }

    // Skapar ett nytt ärende (ticket) för kund.
    // UPPDATERAD: Använder email istället för user_id, och genererar ett token (sparat i case_number).
    private static async Task<int> CreateTicketForCustomerAsync(string email, string title, string description, int company_id, int? category_id, int? product_id, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        // Generera ett unikt token via GenerateUniqueCaseNumber och använd det även som case_number.
        string token = GenerateUniqueCaseNumber();
        cmd.CommandText = "INSERT INTO tickets (user_email, title, description, case_number, company_id, date, status_id, category_id, product_id) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9) RETURNING id";
        cmd.Parameters.AddWithValue(email);
        cmd.Parameters.AddWithValue(title);
        cmd.Parameters.AddWithValue(description);
        cmd.Parameters.AddWithValue(token); // Sparar token som case_number.
        cmd.Parameters.AddWithValue(company_id);
        cmd.Parameters.AddWithValue(DateTime.UtcNow);
        cmd.Parameters.AddWithValue(1); // Sätter default status_id till 1 (Unread)
        cmd.Parameters.AddWithValue(category_id);
        cmd.Parameters.AddWithValue(product_id);

        var ticketId = (int)(await cmd.ExecuteScalarAsync() ?? throw new InvalidOperationException("Failed to create ticket."));
        Console.WriteLine($"Ticket created with token (case_number): {token}");
        return ticketId;
    }

    // Hjälpfunktion: Hämta token (case_number) för ett givet ticketId.
    private static string GetTicketToken(int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT case_number FROM tickets WHERE id = $1";
        cmd.Parameters.AddWithValue(ticketId);
        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? "";
    }

    // Genererar ett slumpmässigt lösenord (används inte direkt för kund, men finns för andra syften).
    public static string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    // Genererar ett unikt token / ärendenummer.
    private static string GenerateUniqueCaseNumber()
    {
        return $"CASE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    // Skickar bekräftelsemail med MailKit via Googles SMTP-server.
    // UPPDATERAD: Använder Gmail SMTP med baseUrl satt till din frontend (http://localhost:5173)
    private static async Task SendConfirmationEmailAsync(string email, string name, string content, string token)
    {
        string baseUrl = "http://localhost:5173"; // Uppdaterad bas-URL

        string messageBody = $"Dear {name ?? "Customer"},\n\nWe have received your message:\n\n\"{content}\"\n\n" +
                             $"To access your chat with customer support, please click the link below within the next hour:\n" +
                             $"{baseUrl}/tickets/view/{token}\n\n" +
                             $"Thank you for contacting us.\n\nBest regards,\nYour App Team";

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Your App Name", "dissatisfiedcustomer2025@gmail.com"));
        mimeMessage.To.Add(new MailboxAddress(name ?? "Customer", email));
        mimeMessage.Subject = "Chat Access Confirmation";
        mimeMessage.Body = new TextPart("plain") { Text = messageBody };

        try
        {
            using var smtpClient = new SmtpClient();
            // För testning: inaktivera certifikatvalidering
            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            smtpClient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtpClient.Authenticate("dissatisfiedcustomer2025@gmail.com", "yxel egbr xehm wdrt");
            await smtpClient.SendAsync(mimeMessage);
            await smtpClient.DisconnectAsync(true);
            Console.WriteLine("Confirmation email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            Console.WriteLine($"Email sending Stack Trace: {ex.StackTrace}");
            throw;
        }
    }
}
