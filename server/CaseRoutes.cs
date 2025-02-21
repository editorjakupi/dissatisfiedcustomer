using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

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
}
