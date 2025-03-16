using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server
{
    public static class CaseRoutes
    {
        // Lägger till ett meddelande i ett ärende baserat på avsändarens e-postadress.
        // Hanterar både kund- och medarbetarmeddelanden.
        public static async Task<IResult> AddCaseMessageByEmail(string email, int ticketId, Message message, string senderType, NpgsqlDataSource db)
        {
            using var conn = db.CreateConnection(); // Skapar en anslutning till databasen.
            await conn.OpenAsync(); // Öppnar anslutningen.

            using var transaction = await conn.BeginTransactionAsync(); // Startar en transaktion.

            try
            {
                // Kontrollerar om ärendet är stängt eller löst.
                using var checkCmd = conn.CreateCommand();
                checkCmd.Transaction = transaction;
                checkCmd.CommandText = "SELECT status_id FROM tickets WHERE id = $1";
                checkCmd.Parameters.AddWithValue(ticketId);
                var statusResult = await checkCmd.ExecuteScalarAsync(); // Hämtar ärendets status.
                int status = Convert.ToInt32(statusResult);
                if (status == 3 || status == 4) // Anta att 3 = Lösta ärenden, 4 = Stängda ärenden.
                {
                    return TypedResults.BadRequest("Cannot add message to closed or resolved ticket.");
                }

                // Lägger till meddelandet i "messages"-tabellen.
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO messages (ticket_id, email, message, sender_type)
                    VALUES ($1, $2, $3, $4)";
                cmd.Parameters.AddWithValue(ticketId); // Kopplar till ärendets ID.
                cmd.Parameters.AddWithValue(email); // Avsändarens e-postadress.
                cmd.Parameters.AddWithValue(message.Content); // Meddelandets innehåll.
                cmd.Parameters.AddWithValue(senderType); // Typ av avsändare (kund eller medarbetare).
                await cmd.ExecuteNonQueryAsync(); // Kör insättningen.

                // Genomför transaktionen.
                await transaction.CommitAsync();

                return TypedResults.Ok("Message added successfully."); // Returnerar framgångsmeddelande.
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rullar tillbaka transaktionen vid fel.
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return TypedResults.NotFound(); // Returnerar fel om något går snett.
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open) // Stänger anslutningen om den är öppen.
                {
                    await conn.CloseAsync();
                }
            }
        }

        // Hämtar alla meddelanden kopplade till ett ärende.
        public static async Task<List<MessageDetails>> GetCaseMessagesByEmail(int ticketId, NpgsqlDataSource db)
        {
            var result = new List<MessageDetails>(); // Skapar en lista för att hålla meddelandena.

            using var conn = db.CreateConnection(); // Skapar en anslutning till databasen.
            await conn.OpenAsync(); // Öppnar anslutningen.

            using var cmd = conn.CreateCommand(); // Skapar ett SQL-kommando för att hämta meddelanden.
            cmd.CommandText = @"
                SELECT ticket_id, email, message, sender_type
                FROM messages
                WHERE ticket_id = $1";
            cmd.Parameters.AddWithValue(ticketId); // Kopplar till det angivna ärendet.

            using var reader = await cmd.ExecuteReaderAsync(); // Utför kommandot och läser resultaten.
            while (await reader.ReadAsync()) // Loopar igenom resultaten.
            {
                var senderType = reader.IsDBNull(3) ? "unknown" : reader.GetString(3); // Hämtar avsändartyp.
                result.Add(new MessageDetails(
                    0,                            // Default MessageId (0 om ej använt).
                    reader.GetString(1),          // Avsändarens e-postadress.
                    reader.GetString(2),          // Meddelandets innehåll.
                    senderType                    // Avsändartyp (kund eller medarbetare).
                ));
            }

            return result; // Returnerar listan med meddelanden.
        }
    }
}
