using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;

public static class MessageRoutes
{
    public record MessageDTO(string Email, string Name, string Content);

    public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, NpgsqlDataSource db)
    {

        Console.WriteLine($"Received Message - Email: {message.Email}, Name: {message.Name}, Content: {message.Content}");

        // Validera inkommande data
        if (string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Content))
        {
            return TypedResults.BadRequest("Email, Name, and Content are required.");
        }

        using var conn = db.CreateConnection();
        await conn.OpenAsync();

        using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Hämta eller skapa användare
            int userId = await GetOrCreateUserIdAsync(message.Email, "No Name", conn, transaction);

            // Skapa nytt ärende (ticket)
            int ticketId = await CreateTicketAsync(userId, message.Name, conn, transaction);

            // Spara meddelandet
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "INSERT INTO messages (ticket_id, message, user_id) VALUES ($1, $2, $3)";
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(userId);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Message inserted successfully!");

            await transaction.CommitAsync();
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

    private static async Task<int> GetOrCreateUserIdAsync(string email, string name, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Kontrollera om användaren redan finns
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT id FROM users WHERE email = $1";
        cmd.Parameters.AddWithValue(email);

        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            return (int)result;
        }
        else
        {
            // Generera ett standardlösenord eller ett slumpmässigt lösenord
            string defaultPassword = GenerateRandomPassword();

            // Skapa ny användare inklusive 'name' och 'password'
            cmd.CommandText = "INSERT INTO users (email, name, password, role_id) VALUES ($1, $2, $3, $4) RETURNING id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(name);
            cmd.Parameters.AddWithValue(defaultPassword);
            cmd.Parameters.AddWithValue(1); // Antar att 1 är 'Customer' role_id

            return (int)await cmd.ExecuteScalarAsync();
        }
    }
    private static string GenerateRandomPassword()
    {
        // En enkel lösenordsgenerator för demonstration (använd en säkrare metod i produktion)
        return Guid.NewGuid().ToString().Substring(0, 8);
    }


    private static async Task<int> CreateTicketAsync(int userId, string title, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        // Skapa nytt ärende
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "INSERT INTO tickets (user_id, title, date) VALUES ($1, $2, $3) RETURNING id";
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(title);
        cmd.Parameters.AddWithValue(DateTime.UtcNow); // Insertar datum och tid.

        return (int)await cmd.ExecuteScalarAsync();
    }
}
