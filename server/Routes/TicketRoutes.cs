using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder.Extensions;

namespace server;

public static class TicketRoutes
{
    public record Ticket(int id, string date, string title, string categoryname, string email, string status);

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
            reader.GetString(5)  // status
        ));
        }

        return result;
    }
}