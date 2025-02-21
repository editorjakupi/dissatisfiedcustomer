using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;

namespace server;

public static class TicketRoutes
{
    public record Ticket(int id, string date, string title, string categoryname, string email, string status, string caseNumber, string description);

    public static async Task<List<Ticket>>

    GetTickets(string? view, NpgsqlDataSource db)
    {
        List<Ticket> result = new();
        NpgsqlCommand query;    
        switch (view)
        {
            case "all":
                query = db.CreateCommand("SELECT * FROM tickets_all");
                break;
            case "open":
                query = db.CreateCommand("SELECT * FROM tickets_open");
                break;
            case "closed":
                query = db.CreateCommand("SELECT * FROM tickets_closed");
                break;
            case "pending":
                query = db.CreateCommand("SELECT * FROM tickets_pending");
                break;
            default:
                query = db.CreateCommand("SELECT * FROM tickets_all");
                break;
        }
        using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new(
            reader.GetInt32(0), // id
            reader.GetDateTime(1).ToString("yyyy-MM-dd"), // date
            reader.GetString(2), // title
            reader.GetString(3), // category_name
            reader.GetString(4), // email
            reader.GetString(5),  // status
            reader.GetString(6), // casenumber
            reader.GetString(7) // description
        ));
        }

        return result;
    }

    public static async Task<List<Ticket>>
    GetTicket(int id, NpgsqlDataSource db)
    {
        List<Ticket> result = new();

        await using var query = db.CreateCommand("SELECT * FROM tickets_all WHERE id = ($1)");
        query.Parameters.AddWithValue(id);
        await using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new(
                reader.GetInt32(0), // id
                reader.GetDateTime(1).ToString("yyyy-MM-dd"), // date
                reader.GetString(2), // title
                reader.GetString(3), // category name
                reader.GetString(4), // email
                reader.GetString(5), // status
                reader.GetString(6), // casenumber
                reader.GetString(7) // description
            ));
        }
        return result;
    }

    public static async Task<IResult>
        PutTicketStatus(int status, int ticket_id, NpgsqlDataSource db)
    {
        await using var cmd = new NpgsqlCommand("UPDATE tickets SET status_id = $1 WHERE id = $2");
        cmd.Parameters.AddWithValue(status);
        cmd.Parameters.AddWithValue(ticket_id);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok("Ticket status updated");
        }
        catch (Exception e)
        {
            return Results.BadRequest($"Failed to update ticket status: {e.Message}");
        }
    }

    public static async Task<IResult>
        PutTicketCategory(int ticket_id, int category_id, NpgsqlDataSource db)
    {
        try
        {
            await using var conn = await db.OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE tickets SET category_id = @category_id WHERE id = @ticket_id";
            cmd.Parameters.AddWithValue("@category_id", category_id);
            cmd.Parameters.AddWithValue("@ticket_id", ticket_id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected == 0)
            {
                return Results.BadRequest("Ticket not found or category not updated.");
            }
            
            return Results.Ok("Ticket category updated successfully.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error updating ticket category: {ex.Message}");
        }
    }
}