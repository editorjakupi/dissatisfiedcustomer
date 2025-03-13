using Npgsql;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server;

public static class TicketRoutes
{


    public static async Task<IResult>

    GetTickets(string? view, NpgsqlDataSource db, HttpContext context)
    {
        List<Ticket> result = new();
        NpgsqlCommand query;
        if (context.Session.GetInt32("company") is int company_id)
        {
            switch (view)
            {
                case "all":
                    query = db.CreateCommand("SELECT * FROM tickets_all WHERE company_id = $1");
                    break;
                case "open":
                    query = db.CreateCommand("SELECT * FROM tickets_open WHERE company_id = $1");
                    break;
                case "closed":
                    query = db.CreateCommand("SELECT * FROM tickets_closed WHERE company_id = $1");
                    break;
                case "pending":
                    query = db.CreateCommand("SELECT * FROM tickets_pending WHERE company_id = $1");
                    break;
                default:
                    query = db.CreateCommand("SELECT * FROM tickets_all WHERE company_id = $1");
                    break;
            }
            query.Parameters.AddWithValue(company_id);


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
            return Results.Ok(result);
        }

        Console.WriteLine("No Company Found");

        return Results.BadRequest();
    }

        public static async Task<Ticket?> GetTicket(int id, NpgsqlDataSource db)
        {
            Ticket? result = null;
            using var cmd = db.CreateCommand(@"
                SELECT id, date, title, email, status_name, case_number, description, company_id
                FROM public.tickets_with_status 
                WHERE id = $1");
            cmd.Parameters.AddWithValue(id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int? companyId = reader.IsDBNull(reader.GetOrdinal("company_id"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("company_id"));

                result = new Ticket(
                    reader.GetInt32(reader.GetOrdinal("id")),
                    reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"),
                    reader.GetString(reader.GetOrdinal("title")),
                    "", // category placeholder
                    reader.GetString(reader.GetOrdinal("email")),
                    reader.GetString(reader.GetOrdinal("status_name")),
                    reader.GetString(reader.GetOrdinal("case_number")),
                    reader.GetString(reader.GetOrdinal("description")),
                    companyId
                );
            }
            return result;
        }

        public static async Task<IResult> PutTicketCategory(int ticketId, string categoryName, NpgsqlDataSource db)
        {
            using var cmd = db.CreateCommand("UPDATE tickets SET category_name = $2 WHERE id = $1");
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(categoryName);
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok();
        }

        public static async Task<IResult> PutTicketStatus(int ticketId, int status, NpgsqlDataSource db)
        {
            using var cmd = db.CreateCommand("UPDATE tickets SET status_id = $2 WHERE id = $1");
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(status);
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok();
        }

        public static async Task<IResult> PutTicketProduct(int ticketId, string productName, NpgsqlDataSource db)
        {
            using var cmd = db.CreateCommand("UPDATE tickets SET product_name = $2 WHERE id = $1");
            cmd.Parameters.AddWithValue(ticketId);
            cmd.Parameters.AddWithValue(productName);
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok();
        }

        public static async Task<IResult> UpdateTicketStatus(int id, NpgsqlDataSource db)
        {
            using var cmd = db.CreateCommand("UPDATE tickets SET status_id = $2 WHERE id = $1");
            cmd.Parameters.AddWithValue(id);
            cmd.Parameters.AddWithValue(1); // exempelvis status_id = 1 (unread)
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok();
        }

        public static async Task<IResult> Feedback(int ticketId, NpgsqlDataSource db)
        {
            using var cmd = db.CreateCommand("SELECT feedback FROM tickets WHERE id = $1");
            cmd.Parameters.AddWithValue(ticketId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Results.Ok(result.ToString()) : Results.NotFound();
        }

        public static async Task<Ticket?> GetTicketByToken(string token, NpgsqlDataSource db)
        {
            Ticket? result = null;
            using var cmd = db.CreateCommand(@"
                SELECT id, date, title, email, status_name, case_number, description, company_id
                FROM public.tickets_with_status 
                WHERE case_number = $1");
            cmd.Parameters.AddWithValue(token);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int? companyId = reader.IsDBNull(reader.GetOrdinal("company_id"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("company_id"));
                result = new Ticket(
                    reader.GetInt32(reader.GetOrdinal("id")),
                    reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"),
                    reader.GetString(reader.GetOrdinal("title")),
                    "", // category placeholder
                    reader.GetString(reader.GetOrdinal("email")),
                    reader.GetString(reader.GetOrdinal("status_name")),
                    reader.GetString(reader.GetOrdinal("case_number")),
                    reader.GetString(reader.GetOrdinal("description")),
                    companyId
                );
            }
            return result;
        }
    


    public static async Task<IResult>
        PutTicketProducts(int ticket_id, int product_id, NpgsqlDataSource db)
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

    public static async Task<IResult> Feedbacks( NpgsqlDataSource db,HttpContext context)
    {
        if (context.Session.GetInt32("company") is int comapny_id)
        {
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"
            SELECT t.id, t.title, t.user_email, u.name as employee_name,
       f.rating, f.comment, f.date
FROM tickets t
         JOIN employees e ON t.employee_id = e.id
         JOIN users u ON e.user_id = u.id
         LEFT JOIN feedback f ON t.id = f.ticket_id
WHERE t.company_id = @companyId";

            cmd.Parameters.AddWithValue("companyId", comapny_id);

            using var reader = await cmd.ExecuteReaderAsync();
            var ticketFeedbackList = new List<TicketFeedback>();

            while (await reader.ReadAsync())
            {
                ticketFeedbackList.Add(new TicketFeedback(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetInt32(4), // Handle nullable rating
                    reader.IsDBNull(5) ? null : reader.GetString(5), // Handle nullable comment
                    reader.IsDBNull(6) ? null : reader.GetDateTime(6) // Handle nullable date
                ));
            }

            return Results.Ok(ticketFeedbackList);
        }
        Console.WriteLine("No Feedback Found");
        return Results.BadRequest();
        
        }
    }
