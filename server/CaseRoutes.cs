using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using server; // Lägg till detta för att använda Message-klassen

namespace server;

public static class CaseRoutes
{
    public record CaseDetails(int CaseId, string Title, string Description, string CaseNumber, string Status, DateTime CreatedAt);

    public static async Task<List<CaseDetails>> GetUserCases(int userId, NpgsqlDataSource db)
    {
        var result = new List<CaseDetails>();
        using var cmd = db.CreateCommand("SELECT t.id, t.title, t.description, t.case_number, ts.status_name, t.date FROM tickets t JOIN ticketstatus ts ON t.status_id = ts.id WHERE t.user_id = $1");
        cmd.Parameters.AddWithValue(userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CaseDetails(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? "N/A" : reader.GetString(3), // Hantera nullvärden för case_number
                reader.GetString(4),
                reader.GetDateTime(5)
            ));
        }

        return result;
    }

    public static async Task<Results<Ok<CaseDetails>, NotFound>> GetCaseDetails(int userId, int caseId, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("SELECT t.id, t.title, t.description, t.case_number, ts.status_name, t.date FROM tickets t JOIN ticketstatus ts ON t.status_id = ts.id WHERE t.user_id = $1 AND t.id = $2");
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(caseId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return TypedResults.NotFound();
        }

        var caseDetails = new CaseDetails(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? "N/A" : reader.GetString(3), // Hantera nullvärden för case_number
            reader.GetString(4),
            reader.GetDateTime(5)
        );

        return TypedResults.Ok(caseDetails);
    }


    // Metod för att lägga till ett meddelande till ett befintligt ärende
    public static async Task<Results<Ok, NotFound>> AddCaseMessage(int userId, int caseId, Message message, NpgsqlDataSource db)
    {
        // Kontrollera om ärendet existerar
        using var checkCmd = db.CreateCommand("SELECT COUNT(*) FROM tickets WHERE id = $1 AND user_id = $2");
        checkCmd.Parameters.AddWithValue(caseId);
        checkCmd.Parameters.AddWithValue(userId);

        var caseExists = (long)await checkCmd.ExecuteScalarAsync() > 0;
        if (!caseExists)
        {
            return TypedResults.NotFound();
        }

        // Lägg till meddelandet i tabellen 'messages'
        using var cmd = db.CreateCommand("INSERT INTO messages (ticket_id, message, user_id) VALUES ($1, $2, $3)");
        cmd.Parameters.AddWithValue(caseId);
        cmd.Parameters.AddWithValue(message.Content);
        cmd.Parameters.AddWithValue(message.UserId);

        await cmd.ExecuteNonQueryAsync();
        return TypedResults.Ok();
    }


    public record MessageDetails(int MessageId, int UserId, string Content);

    public static async Task<List<MessageDetails>> GetCaseMessages(int userId, int caseId, NpgsqlDataSource db)
    {
        var result = new List<MessageDetails>();
        using var cmd = db.CreateCommand("SELECT id, user_id, message FROM messages WHERE ticket_id = $1 AND user_id = $2");
        cmd.Parameters.AddWithValue(caseId);
        cmd.Parameters.AddWithValue(userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new MessageDetails(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2)
            ));
        }

        return result;
    }

    

   
    

}
