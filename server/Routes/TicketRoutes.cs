using Npgsql;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{
    public static class TicketRoutes
    {
        public static async Task<IResult> GetTickets(NpgsqlDataSource db)
        {
            var result = new List<Ticket>();
            using var cmd = db.CreateCommand("SELECT id, date, title, email, status_name, case_number, description, company_id FROM tickets_with_status ORDER BY id ASC");
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new Ticket(
                    reader.GetInt32(0),
                    reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                    reader.GetString(2),
                    "", // category placeholder
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
                ));
            }
            return Results.Ok(result);
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
    }
}
