using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace server;

public static class MessageRoutes
{
    // Dataöverföringsobjekt för inkommande meddelanden
    public record MessageDTO(string Email, string Name, string Content);

    //Metod för att hantera POST /api/messages
    public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, NpgsqlDataSource db)
    {
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
            int userId = await GetOrCreateUserIdAsync(message.Email, message.Name, conn, transaction, out string generatedPassword);
            // Skapa ett nytt ärende (ticket) kopplat till användaren
            int ticketId = await CreateTicketAsync(userId, conn, transaction);

            // Spara meddelandet i databasen
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "INSERT INTO messages (ticket_id, message, user_id) VALUES ($1, $2, $3)";
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(userId);

            await cmd.ExecuteNonQueryAsync();
            // Bekräfta transaktionen
            await transaction.CommitAsync();

            // Skicka bekräftelse via e-post med bifogat lösenord
            await SendConfirmationEmailAsync(message.Email, message.Name, message.Content, generatedPassword);

            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            // Rulla tillbaka transaktionen vid fel
            await transaction.RollbackAsync();
            return TypedResults.BadRequest($"An error occurred: {ex.Message}");
        }
        finally
        {
            // Stäng databaskopplingen
            await conn.CloseAsync();
        }
    }

    // Metod för att hämta eller skapa användare
    private static async Task<int> GetOrCreateUserIdAsync(string email, string name, NpgsqlConnection conn, NpgsqlTransaction transaction, out string generatedPassword)
    {
        // Kontrollera om användaren redan finns
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT id FROM users WHERE email = $1";
        cmd.Parameters.AddWithValue(email);

        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            generatedPassword = null; // Lösenord genereras endast för nya användare
            // Användaren finns redan
            return (int)result;
        }
        else
        {
            // Generera ett standardlösenord eller ett slumpmässigt lösenord
            generatedPassword = GenerateRandomPassword();

            // Skapa ny användare inklusive 'name' och 'password'
            cmd.CommandText = "INSERT INTO users (email, name, password, role_id) VALUES ($1, $2, $3, $4) RETURNING id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(name);
            cmd.Parameters.AddWithValue(generatedPassword);
            cmd.Parameters.AddWithValue(1); // Antar att 1 är 'Customer' role_id

            return (int)await cmd.ExecuteScalarAsync();
        }
    }

    // Metod för att generera ett slumpmässigt lösenord
    private static string GenerateRandomPassword()
    {
        // Går att använda ännu säkrare metoder för generering av lösenord än detta
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    // Metod för att skapa ett nytt ärende (ticket)
    private static async Task<int> CreateTicketAsync(int userId, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "INSERT INTO tickets (user_id, title, date) VALUES ($1, $2, $3) RETURNING id";
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue("New Ticket");
        cmd.Parameters.AddWithValue(DateTime.UtcNow);

        return (int)await cmd.ExecuteScalarAsync();
    }

    // Metod för att skicka bekräftelse via e-post med bifogat lösenord
    private static async Task SendConfirmationEmailAsync(string email, string name, string content, string password)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Your App Name", "no-reply@yourapp.com"));
        message.To.Add(new MailboxAddress(name, email));
        message.Subject = "Message Received Confirmation";
        message.Body = new TextPart("plain")
        {
            Text = $"Dear {name},\n\nWe have received your message:\n\n\"{content}\"\n\nYour account has been created with the following password: {password}\n\nThank you for reaching out to us.\n\nBest regards,\nYour App Team"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.your-email-provider.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("your-email@yourapp.com", "your-email-password");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
