using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder.Extensions;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticAssets;


namespace server;

public static class TicketRoutes
{


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
            reader.GetString(7), // description
            reader.GetInt32(8) // company_id
        ));
        }

        return result;
    }

    public static async Task<IResult> UpdateTicketStatus(int ticketId, NpgsqlDataSource db)
    {
        try
        {
            using var cmd = db.CreateCommand("UPDATE tickets SET status_id = 3 WHERE id = @id");
            cmd.Parameters.AddWithValue("@id", ticketId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected == 1)
            {
                return Results.Ok();
            }
            else
            {
                return Results.NotFound();
            }

        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }
    public static async Task<Ticket?> GetTicket(int ticketId, NpgsqlDataSource db)
    {
        Ticket? result = null;

        var query = db.CreateCommand("SELECT * FROM tickets_all WHERE id = @ticketId");
        query.Parameters.AddWithValue("ticketId", ticketId);

        using var reader = await query.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            result = new Ticket(
                reader.GetInt32(reader.GetOrdinal("id")),               // id
                reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"), // date
                reader.GetString(reader.GetOrdinal("title")),           // title
                reader.GetString(reader.GetOrdinal("name")),   // categoryname
                reader.GetString(reader.GetOrdinal("email")),           // email
                reader.GetString(reader.GetOrdinal("status_name")),          // status
                reader.GetString(reader.GetOrdinal("case_number")),      // caseNumber
                reader.GetString(reader.GetOrdinal("title")),       // description
                reader.GetInt32(reader.GetOrdinal("company_id"))     // company_id
            );
        }

        return result; // Return the single ticket or null if not found
    }



    public static async Task<IResult>
        PutTicketStatus(int status, int ticket_id, NpgsqlDataSource db)
    {
        await using var cmd = db.CreateCommand("UPDATE tickets SET status_id = @status WHERE id = @ticket_id");
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@ticket_id", ticket_id);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return Results.NotFound("Ticket not found or status unchanged.");
            }
            return Results.Ok("Ticket status updated successfully.");
        }
        catch (Exception e)
        {
            return Results.BadRequest($"Failed to update ticket status: {e.Message}");
        }
    }


    //Skapa backend-endpoint för kundens chatt via token
    // Exempelmetod för att hämta ett ärende baserat på det genererade token (case_number)
    public static async Task<Ticket?> GetTicketByToken(string token, NpgsqlDataSource db)
    {
        Ticket? result = null;
        // Hämtar direkt från tabellen "tickets" i public-schemat
        using var cmd = db.CreateCommand("SELECT * FROM public.tickets WHERE case_number = $1");
        cmd.Parameters.AddWithValue(token);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            // Skapa ett Ticket-objekt från kolumnvärdena.
            // Notera: Om du inte brukar använda JOIN för att få med t.ex. category name och status,
            // sätts dessa fält till tomma strängar för nu, men du kan senare uppdatera detta.
            result = new Ticket(
                reader.GetInt32(reader.GetOrdinal("id")),
                reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"),
                reader.GetString(reader.GetOrdinal("title")),
                "", // Du kan till exempel hämta kategori från en JOIN, om så önskas
                reader.GetString(reader.GetOrdinal("user_email")),
                "", // Om status lagras separat via en JOIN, uppdatera denna rad
                reader.GetString(reader.GetOrdinal("case_number")),
                reader.GetString(reader.GetOrdinal("description")),
                reader.GetInt32(reader.GetOrdinal("company_id"))
            );
        }
        return result;
    }




    public static async Task<IResult>
        PutTicketCategory(int ticket_id, int category_id, NpgsqlDataSource db)
    {
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "UPDATE tickets SET category_id = @category_id WHERE id = @ticket_id";
        cmd.Parameters.AddWithValue("@category_id", category_id);
        cmd.Parameters.AddWithValue("@ticket_id", ticket_id);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return Results.NotFound("Category not found or status unchanged.");
            }
            return Results.Ok("Category status updated successfully.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error updating ticket category: {ex.Message}");
        }
    }

    public static async Task<IResult>
        PutTicketProduct(int ticket_id, int product_id, NpgsqlDataSource db)
    {
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "UPDATE tickets SET product_id = @product_id WHERE id = @ticket_id";
        cmd.Parameters.AddWithValue("@product_id", product_id);
        cmd.Parameters.AddWithValue("@ticket_id", ticket_id);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return Results.NotFound("Product not found or status unchanged.");
            }
            return Results.Ok("Product status updated successfully.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error updating ticket product: {ex.Message}");
        }
    }
}