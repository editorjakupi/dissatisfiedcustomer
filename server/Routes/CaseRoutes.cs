using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server
{
    public static class CaseRoutes
    {
        // Adds a message to a ticket's case using the email address of the sender.
        // Handles both customer and employee messages.
        public static async Task<IResult> AddCaseMessageByEmail(string email, int ticketId, Message message, string senderType, NpgsqlDataSource db)
        {
            using var conn = db.CreateConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Kontrollera om ticket är stängd eller löst
                using var checkCmd = conn.CreateCommand();
                checkCmd.Transaction = transaction;
                checkCmd.CommandText = "SELECT status_id FROM tickets WHERE id = $1";
                checkCmd.Parameters.AddWithValue(ticketId);
                var statusResult = await checkCmd.ExecuteScalarAsync();
                int status = Convert.ToInt32(statusResult);
                if (status == 3 || status == 4)  // Antag att 3 = Resolved och 4 = Closed
                {
                    return TypedResults.BadRequest("Cannot add message to closed or resolved ticket.");
                }

                // Insert the message into the messages table.
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO messages (ticket_id, email, message, sender_type)
                    VALUES ($1, $2, $3, $4)";
                cmd.Parameters.AddWithValue(ticketId);
                cmd.Parameters.AddWithValue(email);
                cmd.Parameters.AddWithValue(message.Content);
                cmd.Parameters.AddWithValue(senderType);
                await cmd.ExecuteNonQueryAsync();

                // Commit the transaction.
                await transaction.CommitAsync();

                return TypedResults.Ok("Message added successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return TypedResults.NotFound();
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    await conn.CloseAsync();
                }
            }
        }

        // Retrieves all messages associated with a ticket's case.
        public static async Task<List<MessageDetails>> GetCaseMessagesByEmail(int ticketId, NpgsqlDataSource db)
        {
            var result = new List<MessageDetails>();

            using var conn = db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT ticket_id, email, message, sender_type
                FROM messages
                WHERE ticket_id = $1";
            cmd.Parameters.AddWithValue(ticketId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var senderType = reader.IsDBNull(3) ? "unknown" : reader.GetString(3);
                result.Add(new MessageDetails(
                    0,                            // Default MessageId
                    reader.GetString(1),          // Email (UserEmail)
                    reader.GetString(2),          // Message content (Content)
                    senderType                    // Sender type (SenderType)
                ));
            }

            return result;
        }
    }
}
