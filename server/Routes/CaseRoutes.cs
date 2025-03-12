using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{
    public static class CaseRoutes
    {
        // Adds a message for a ticket based on the customer's email.
        // UPDATED: Now includes a validation step to check the ticket status before inserting a new message.
        // If the ticket’s status_id is 3 (Resolved) or 4 (Closed), we return a BadRequest.
        public static async Task<Results<Ok, NotFound, BadRequest<string>>> AddCaseMessageByEmail(string userEmail, int caseId, Message message, NpgsqlDataSource db)
        {
            // First check that the ticket exists for the given email.
            using var checkCmd = db.CreateCommand("SELECT COUNT(*) FROM tickets WHERE id = $1 AND user_email = $2");
            checkCmd.Parameters.AddWithValue(caseId);
            checkCmd.Parameters.AddWithValue(userEmail);
            var caseExists = (long)await checkCmd.ExecuteScalarAsync() > 0;
            if (!caseExists)
            {
                return TypedResults.NotFound();
            }

            // NEW: Check the current status of the ticket.
            await using var statusCmd = db.CreateCommand("SELECT status_id FROM tickets WHERE id = $1");
            statusCmd.Parameters.AddWithValue(caseId);
            var statusResult = await statusCmd.ExecuteScalarAsync();
            int statusId = Convert.ToInt32(statusResult);
            // In our schema: 3 = Resolved, 4 = Closed.
            if (statusId == 3 || statusId == 4)
            {
                return TypedResults.BadRequest("Cannot add message to closed or resolved ticket.");
            }

            // If ticket is active, proceed with inserting the message.
            using var cmd = db.CreateCommand("INSERT INTO messages (ticket_id, message, email) VALUES ($1, $2, $3)");
            cmd.Parameters.AddWithValue(caseId);
            cmd.Parameters.AddWithValue(message.Content);
            cmd.Parameters.AddWithValue(userEmail);
            await cmd.ExecuteNonQueryAsync();

            return TypedResults.Ok();
        }

        // Retrieves messages for a ticket based on the customer's email.
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
                    reader.GetString(1),  // Ensure the email is read as a string.
                    reader.GetString(2)
                ));
            }
            return result;
        }
    }
}
