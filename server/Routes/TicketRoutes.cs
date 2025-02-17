using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder.Extensions;

namespace server;

public static class TicketRoutes {
    public record Ticket(string date, string title, string categoryname, string email, string status);

    public static async Task<List<Ticket>>
    GetTickets(NpgsqlDataSource db) 
    {
        List<Ticket> result = new();

        using var query = db.CreateCommand(@"SELECT 
                                                t.date, 
                                                t.title, 
                                                c.name, 
                                                u.email, 
                                                ts.status_name 
                                            FROM tickets t
                                            JOIN category c on t.category_id = c.id
                                            JOIN users u on t.user_id = u.id
                                            JOIN ticketstatus ts ON t.status_id = ts.id;");
        using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {  
            result.Add(new(
            reader.GetDateTime(0).ToString("yyyy-MM-dd"), // date
            reader.GetString(1), // title
            reader.GetString(2), // category_name
            reader.GetString(3), // email
            reader.GetString(4)  // status
        ));
        }

        return result;
    }
}