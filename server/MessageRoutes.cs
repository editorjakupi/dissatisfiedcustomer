using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Mail;

namespace server;

public static class MessageRoutes
{
    // Dataöverföringsobjekt för inkommande meddelanden
    public record MessageDTO(string Email, string Name, string Content, int CompanyID);

    // Metod för att hantera POST /api/messages
    public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, NpgsqlDataSource db)
    {

        Console.WriteLine($"Received Message - Email: {message.Email}, Name: {message.Name}, Content: {message.Content}, CompanyID: {message.CompanyID}");

        // Validera inkommande data
        if (string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Content))
        {
            return TypedResults.BadRequest("Email, Name, and Content are required.");
        }

        using var conn = db.CreateConnection();
        await conn.OpenAsync();

        // Starta en transaktion för att säkerställa dataintegritet
        using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Hämta eller skapa användaren i databasen
            (int userId, string generatedPassword) = await GetOrCreateUserIdAsync(message.Email, "No Name", conn, transaction);

            // Skapa ett nytt ärende (ticket) kopplat till användaren
            int ticketId = await CreateTicketAsync(userId, message.Name, message.CompanyID, conn, transaction);

            // Spara meddelandet i databasen
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "INSERT INTO messages (ticket_id, message, user_id) VALUES ($1, $2, $3)";
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(userId);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Message inserted successfully!");

            // Bekräfta transaktionen
            await transaction.CommitAsync();

            // Skicka bekräftelse via e-post med bifogat lösenord
            //await SendConfirmationEmailAsync(message.Email, message.Name, message.Content, generatedPassword);

            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return TypedResults.BadRequest($"An error occurred: {ex.Message}");
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    // Metod för att hämta eller skapa användare
    private static async Task<(int, string)> GetOrCreateUserIdAsync(string email, string name, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Kontrollera om användaren redan finns
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT id FROM users WHERE email = $1";
        cmd.Parameters.AddWithValue(email);

        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            // Användaren finns redan
            return ((int)result, null);
        }
        else
        {
            // Generera ett standardlösenord eller ett slumpmässigt lösenord
            string generatedPassword = GenerateRandomPassword();

            // Skapa ny användare inklusive 'name' och 'password'
            cmd.CommandText = "INSERT INTO users (email, name, password, role_id) VALUES ($1, $2, $3, $4) RETURNING id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(name);
            cmd.Parameters.AddWithValue(generatedPassword);
            cmd.Parameters.AddWithValue(1); // Antar att 1 är 'Customer' role_id

            var newUserId = (int)await cmd.ExecuteScalarAsync();
            return (newUserId, generatedPassword);
        }
    }

    // Metod för att generera ett slumpmässigt lösenord
    private static string GenerateRandomPassword()
    {
        // En enkel lösenordsgenerator för demonstration (man kan använda säkrare)
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    // Metod för att skapa ett nytt ärende (ticket)
    private static async Task<int> CreateTicketAsync(int userId, string title, int company_id, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Skapa nytt ärende
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "INSERT INTO tickets (user_id, title, company_id, date) VALUES ($1, $2, $3, $4) RETURNING id";
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(title);
        cmd.Parameters.AddWithValue(company_id);
        cmd.Parameters.AddWithValue(DateTime.UtcNow); // Insertar datum och tid.

        return (int)await cmd.ExecuteScalarAsync();
    }

    // Task 1.15: Metod för att skicka bekräftelse via e-post med enbart System.Net.Mail
    private static async Task SendConfirmationEmailAsync(string email, string name, string content, string password)
    {
        string messageBody = $"Dear {name},\n\nWe have received your message:\n\n\"{content}\"\n\nYour account has been created with the following password: {password}\n\nThank you for reaching out to us.\n\nBest regards,\nYour App Team";

        var message = new System.Net.Mail.MailMessage();
        message.From = new MailAddress("no-reply@yourapp.com", "Your App Name");
        message.To.Add(new MailAddress(email, name));
        message.Subject = "Message Received Confirmation";
        message.Body = messageBody;

        using var smtpClient = new System.Net.Mail.SmtpClient("smtp.your-email-provider.com", 587)
        {
            Credentials = new System.Net.NetworkCredential("your-email@yourapp.com", "your-email-password"),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(message);
    }
}
