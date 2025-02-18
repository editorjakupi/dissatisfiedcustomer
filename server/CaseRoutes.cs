using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;

public static class CaseRoutes
{
    public record CaseDetails(int CaseId, string Title, string Description, string CaseNumber, string Status, DateTime CreatedAt);

    // Method to get case details by user ID and case ID
    public static async Task<Results<Ok<CaseDetails>, NotFound>> GetCaseDetails(int userId, int caseId, NpgsqlDataSource db)
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT t.id, t.title, t.description, t.case_number, ts.status_name AS status, t.date 
            FROM tickets t
            JOIN ticketstatus ts ON t.status_id = ts.id
            WHERE t.user_id = $1 AND t.id = $2";
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(caseId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return TypedResults.NotFound();
        }

        var caseDetails = new CaseDetails(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? "N/A" : reader.GetString(3),  // Hantera NULL värde
            reader.GetString(4),
            reader.GetDateTime(5)
        );

        return TypedResults.Ok(caseDetails);
    }
}
