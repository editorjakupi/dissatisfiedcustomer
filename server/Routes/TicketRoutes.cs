using Npgsql;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server;

public static class TicketRoutes
{
    // Hämtar biljetter baserat på en vyparameter (all, open, closed, pending).
    public static async Task<IResult> GetTickets(string? view, NpgsqlDataSource db, HttpContext context)
    {
        List<Ticket> result = new(); // Lista för att hålla biljetterna.
        NpgsqlCommand query;

        // Kollar om företags-ID finns i sessionen.
        if (context.Session.GetInt32("company") is int company_id)
        {
            // Bygger SQL-frågan baserat på vyparameter.
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

            using var reader = await query.ExecuteReaderAsync(); // Kör SQL-frågan och läser resultaten.
            while (await reader.ReadAsync()) // Läser varje rad från resultatet.
            {
                result.Add(new Ticket(
                    reader.GetInt32(0), // ID
                    reader.GetDateTime(1).ToString("yyyy-MM-dd"), // Datum
                    reader.GetString(2), // Titel
                    reader.GetString(3), // Kategorinamn
                    reader.GetString(4), // E-post
                    reader.GetString(5), // Status
                    reader.GetString(6), // Ärendenummer
                    reader.GetString(7), // Beskrivning
                    reader.GetInt32(8)  // Företags-ID
                ));
            }
            return Results.Ok(result); // Returnerar listan över biljetter.
        }

        Console.WriteLine("No Company Found"); // Loggar om inget företag hittas i sessionen.
        return Results.BadRequest();
    }

    // Hämtar en specifik biljett baserat på dess ID.
    public static async Task<Ticket?> GetTicket(int id, NpgsqlDataSource db)
    {
        Ticket? result = null;
        using var cmd = db.CreateCommand(@"
            SELECT id, date, title, email, status_name, case_number, description, company_id
            FROM public.tickets_with_status 
            WHERE id = $1");
        cmd.Parameters.AddWithValue(id);

        using var reader = await cmd.ExecuteReaderAsync(); // Utför SQL-frågan och hämtar resultatet.
        if (await reader.ReadAsync()) // Kontrollerar om data finns.
        {
            int? companyId = reader.IsDBNull(reader.GetOrdinal("company_id"))
                ? (int?)null
                : reader.GetInt32(reader.GetOrdinal("company_id"));

            // Skapar biljettobjektet med data från resultatet.
            result = new Ticket(
                reader.GetInt32(reader.GetOrdinal("id")),
                reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"),
                reader.GetString(reader.GetOrdinal("title")),
                "", // Kategori-platsinnehåll (placeholder)
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("status_name")),
                reader.GetString(reader.GetOrdinal("case_number")),
                reader.GetString(reader.GetOrdinal("description")),
                companyId
            );
        }
        return result; // Returnerar biljetten.
    }

    // Uppdaterar kategorin för en biljett.
    public static async Task<IResult> PutTicketCategory(int ticketId, string categoryName, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("UPDATE tickets SET category_name = $2 WHERE id = $1");
        cmd.Parameters.AddWithValue(ticketId);
        cmd.Parameters.AddWithValue(categoryName);
        await cmd.ExecuteNonQueryAsync(); // Uppdaterar databasen.
        return Results.Ok();
    }

    // Uppdaterar statusen för en biljett.
    public static async Task<IResult> PutTicketStatus(int ticketId, int status, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("UPDATE tickets SET status_id = $2 WHERE id = $1");
        cmd.Parameters.AddWithValue(ticketId);
        cmd.Parameters.AddWithValue(status);
        await cmd.ExecuteNonQueryAsync(); // Uppdaterar databasen.
        return Results.Ok();
    }

    // Uppdaterar produkten kopplad till en biljett.
    public static async Task<IResult> PutTicketProduct(int ticketId, string productName, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("UPDATE tickets SET product_name = $2 WHERE id = $1");
        cmd.Parameters.AddWithValue(ticketId);
        cmd.Parameters.AddWithValue(productName);
        await cmd.ExecuteNonQueryAsync(); // Uppdaterar databasen.
        return Results.Ok();
    }

    // Uppdaterar statusen för en biljett till ett standardvärde (exempelvis oläst).
    public static async Task<IResult> UpdateTicketStatus(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("UPDATE tickets SET status_id = $2 WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(1); // Exempel: status_id = 1 (oläst).
        await cmd.ExecuteNonQueryAsync(); // Uppdaterar databasen.
        return Results.Ok();
    }

    // Hämtar feedback för en specifik biljett.
    public static async Task<IResult> Feedback(int ticketId, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("SELECT feedback FROM tickets WHERE id = $1");
        cmd.Parameters.AddWithValue(ticketId);
        var result = await cmd.ExecuteScalarAsync(); // Hämtar feedback som scalar-värde.
        return result != null ? Results.Ok(result.ToString()) : Results.NotFound();
    }

    // Hämtar en biljett baserat på dess token (case_number).
    public static async Task<Ticket?> GetTicketByToken(string token, NpgsqlDataSource db)
    {
        Ticket? result = null;
        using var cmd = db.CreateCommand(@"
            SELECT id, date, title, email, status_name, case_number, description, company_id
            FROM public.tickets_with_status 
            WHERE case_number = $1");
        cmd.Parameters.AddWithValue(token);

        using var reader = await cmd.ExecuteReaderAsync(); // Utför SQL-frågan och hämtar resultatet.
        if (await reader.ReadAsync()) // Kontrollerar om data finns.
        {
            int? companyId = reader.IsDBNull(reader.GetOrdinal("company_id"))
                ? (int?)null
                : reader.GetInt32(reader.GetOrdinal("company_id"));

            // Skapar biljettobjektet med data från resultatet.
            result = new Ticket(
                reader.GetInt32(reader.GetOrdinal("id")),
                reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd"),
                reader.GetString(reader.GetOrdinal("title")),
                "", // Kategori-platsinnehåll (placeholder)
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("status_name")),
                reader.GetString(reader.GetOrdinal("case_number")),
                reader.GetString(reader.GetOrdinal("description")),
                companyId
            );
        }
        return result;
    }

    // Uppdaterar produkten kopplad till en biljett baserat på produkt-ID.
    public static async Task<IResult> PutTicketProducts(int ticket_id, int product_id, NpgsqlDataSource db)
    {
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE tickets SET product_id = @product_id WHERE id = @ticket_id";
        cmd.Parameters.AddWithValue("@product_id", product_id);
        cmd.Parameters.AddWithValue("@ticket_id", ticket_id);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync(); // Kör SQL-frågan.
            if (rowsAffected == 0)
            {
                return Results.NotFound("Product not found or status unchanged.");
            }
            return Results.Ok("Product status updated successfully."); // Returnerar framgångsmeddelande.
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error updating ticket product: {ex.Message}"); // Returnerar felmeddelande.
        }
    }

    // Hämtar feedback för biljetter kopplade till företaget i den aktuella sessionen.
    public static async Task<IResult> Feedbacks(NpgsqlDataSource db, HttpContext context)
    {
        if (context.Session.GetInt32("company") is int company_id)
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

            cmd.Parameters.AddWithValue("companyId", company_id);

            using var reader = await cmd.ExecuteReaderAsync();
            var ticketFeedbackList = new List<TicketFeedback>();

            while (await reader.ReadAsync())
            {
                ticketFeedbackList.Add(new TicketFeedback(
                    reader.GetInt32(0), // Ticket ID.
                    reader.GetString(1), // Title.
                    reader.GetString(2), // User email.
                    reader.GetString(3), // Employee name.
                    reader.IsDBNull(4) ? null : reader.GetInt32(4), // Rating (nullable).
                    reader.IsDBNull(5) ? null : reader.GetString(5), // Comment (nullable).
                    reader.IsDBNull(6) ? null : reader.GetDateTime(6) // Date (nullable).
                ));
            }

            return Results.Ok(ticketFeedbackList); // Returnerar listan med feedback.
        }

        Console.WriteLine("No Feedback Found"); // Loggar om ingen feedback hittas.
        return Results.BadRequest(); // Returnerar bad request om inget företags-ID hittas i sessionen.
    }
}
