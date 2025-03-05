using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using System.Data.Common;



namespace server;
public static class MessageRoutes
{

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
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Hämta eller skapa användaren i databasen
            (int userId, string generatedPassword) = await GetOrCreateUserIdAsync(message.Email, message.Name, conn, transaction);

            // Skapa ett nytt ärende (ticket) kopplat till användaren med ett unikt ärendenummer och beskrivning
            int ticketId = await CreateTicketAsync(userId, message.Name, message.Content, message.CompanyID, conn, transaction);

            // Spara meddelandet i databasen
            await using var cmd = conn.CreateCommand();
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

    // Metod för att hämta eller skapa användare
    private static async Task<(int, string)> GetOrCreateUserIdAsync(string email, string name, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Kontrollera om användaren redan finns
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT id FROM users WHERE email = $1";
        cmd.Parameters.AddWithValue(email);

        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            // Användaren finns redan
            return ((int)result, string.Empty); // Returnerar tom sträng om användaren redan finns
        }
        else
        {
            // Om namnet är null, använd en standardvärde
            string userName = name ?? "Unknown User";

            // Generera ett standardlösenord eller ett slumpmässigt lösenord
            string generatedPassword = GenerateRandomPassword();

            // Skapa ny användare inklusive 'name' och 'password'
            cmd.CommandText = "INSERT INTO users (email, name, password, role_id) VALUES ($1, $2, $3, $4) RETURNING id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(userName);
            cmd.Parameters.AddWithValue(generatedPassword);
            cmd.Parameters.AddWithValue(1); // Antar att 1 är 'Customer' role_id

            var newUserId = (int)(await cmd.ExecuteScalarAsync() ?? throw new InvalidOperationException("Failed to create user."));
            return (newUserId, generatedPassword);
        }
    }

    // Metod för att generera ett slumpmässigt lösenord
    public static string GenerateRandomPassword()
    {
        // En enkel lösenordsgenerator för demonstration (man kan använda säkrare)
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    // Metod för att generera ett unikt ärendenummer
    private static string GenerateUniqueCaseNumber()
    {
        // Generera ett unikt ärendenummer (du kan anpassa detta efter behov)
        return $"CASE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    // Metod för att skapa ett nytt ärende (ticket)
    private static async Task<int> CreateTicketAsync(int userId, string title, string description, int company_id, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Skapa nytt ärende med unikt ärendenummer och beskrivning
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        string caseNumber = GenerateUniqueCaseNumber(); // Generera unikt ärendenummer
        cmd.CommandText = "INSERT INTO tickets (user_id, title, description, case_number, company_id, date, status_id) VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING id";
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(title); // Title kan inte vara null eftersom den har NOT NULL constraint
        cmd.Parameters.AddWithValue(description); // Description kan inte vara null eftersom den har NOT NULL constraint
        cmd.Parameters.AddWithValue(caseNumber); // Lägg till det unika ärendenumret
        cmd.Parameters.AddWithValue(company_id);
        cmd.Parameters.AddWithValue(DateTime.UtcNow); // Insertar datum och tid.
        cmd.Parameters.AddWithValue(1); // Sätter default status_id till 1 (Unread)

        var ticketId = (int)(await cmd.ExecuteScalarAsync() ?? throw new InvalidOperationException("Failed to create ticket."));
        Console.WriteLine($"Ticket created with case number: {caseNumber}");
        return ticketId;
    }

    // Task 1.15: Metod för att skicka bekräftelse via e-post med enbart MailKit
    private static async Task SendConfirmationEmailAsync(string email, string name, string content, string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            password = "N/A"; // Om lösenordet är tomt, sätt det till "N/A"
        }

        string messageBody = $"Dear {name ?? "Customer"},\n\nWe have received your message:\n\n\"{content}\"\n\nYour account has been created with the following password: {password}\n\nThank you for reaching out to us.\n\nBest regards,\nYour App Team";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Your App Name", "no-reply@yourapp.com"));
        message.To.Add(new MailboxAddress(name ?? "Customer", email));
        message.Subject = "Message Received Confirmation";
        message.Body = new TextPart("plain") { Text = messageBody };

        try
        {
            using var smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls); // Använd dina SMTP-inställningar från Mailtrap
            smtpClient.Authenticate("your-mailtrap-username", "your-mailtrap-password"); // Använd dina autentiseringsuppgifter från Mailtrap
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            Console.WriteLine($"Email sending Stack Trace: {ex.StackTrace}");
            throw; // Kasta undantaget igen för att hantera det på högre nivå
        }
    }
}
