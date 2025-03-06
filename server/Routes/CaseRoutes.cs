using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using server;

namespace server;

public static class CaseRoutes
{
   /*  Avnänds inte på nåt sätt i projektet för att 
   
   
   // Hämtar ärenden baserat på kundens e-post
    public static async Task<List<CaseDetails>> GetUserCasesByEmail(string userEmail, NpgsqlDataSource db)
    {
        var result = new List<CaseDetails>();
        using var cmd = db.CreateCommand("SELECT t.id, t.title, t.description, t.case_number, ts.status_name, t.date " +
                                          "FROM tickets t JOIN ticketstatus ts ON t.status_id = ts.id " +
                                          "WHERE t.user_email = $1");
        cmd.Parameters.AddWithValue(userEmail);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CaseDetails(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? "N/A" : reader.GetString(3),
                reader.GetString(4),
                reader.GetDateTime(5)
            ));
        }
        return result;
    }

    // Hämtar detaljer för ett specifikt ärende baserat på kundens e-post och caseId
    public static async Task<Results<Ok<CaseDetails>, NotFound>> GetCaseDetailsByEmail(string userEmail, int caseId, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("SELECT t.id, t.title, t.description, t.case_number, ts.status_name, t.date " +
                                          "FROM tickets t JOIN ticketstatus ts ON t.status_id = ts.id " +
                                          "WHERE t.user_email = $1 AND t.id = $2");
        cmd.Parameters.AddWithValue(userEmail);
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
            reader.IsDBNull(3) ? "N/A" : reader.GetString(3),
            reader.GetString(4),
            reader.GetDateTime(5)
        );
        return TypedResults.Ok(caseDetails);
    } */

    // Lägger till ett meddelande för ett ärende via kundens e-post
    public static async Task<Results<Ok, NotFound>> AddCaseMessageByEmail(string userEmail, int caseId, Message message, NpgsqlDataSource db)
    {
        using var checkCmd = db.CreateCommand("SELECT COUNT(*) FROM tickets WHERE id = $1 AND user_email = $2");
        checkCmd.Parameters.AddWithValue(caseId);
        checkCmd.Parameters.AddWithValue(userEmail);
        var caseExists = (long)await checkCmd.ExecuteScalarAsync() > 0;
        if (!caseExists)
        {
            return TypedResults.NotFound();
        }
        using var cmd = db.CreateCommand("INSERT INTO messages (ticket_id, message, email) VALUES ($1, $2, $3)");
        cmd.Parameters.AddWithValue(caseId);
        cmd.Parameters.AddWithValue(message.Content);
        cmd.Parameters.AddWithValue(userEmail);
        await cmd.ExecuteNonQueryAsync();
        return TypedResults.Ok();
    }

    // Hämtar meddelanden för ett ärende via kundens e-post
    public static async Task<List<MessageDetails>> GetCaseMessagesByEmail(string userEmail, int caseId, NpgsqlDataSource db)
    {
        var result = new List<MessageDetails>();
        using var cmd = db.CreateCommand("SELECT id, email, message FROM messages WHERE ticket_id = $1 AND email = $2");
        cmd.Parameters.AddWithValue(caseId);
        cmd.Parameters.AddWithValue(userEmail);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new MessageDetails(
                reader.GetInt32(0),
                reader.GetString(1),  // Ändrat från GetInt32 till GetString
                reader.GetString(2)
            ));
        }
        return result;
    }
}
