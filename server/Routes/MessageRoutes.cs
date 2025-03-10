using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;

namespace server
{
    public static class MessageRoutes
    {
        // Data Transfer Object (DTO) for incoming customer messages.
        // Customers provide their email, and we use that directly (no user account is created).
       

        // Method to handle POST /api/messages for a customer message.
        // UPDATED: We no longer receive/create a user_id; instead, we use Email and generate a token for the customer session.
        public static async Task<Results<Created, BadRequest<string>>> PostMessage(MessageDTO message, HttpContext context, NpgsqlDataSource db)
        {
            Console.WriteLine($"Received Message - Email: {message.Email}, Name: {message.Name}, Content: {message.Content}, CompanyID: {message.CompanyID}");

            // Validate required data.
            if (string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Content))
                return TypedResults.BadRequest("Email, Name, and Content are required.");

            using var conn = db.CreateConnection();
            await conn.OpenAsync();

            // Start a transaction to ensure data integrity.
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // I PostMessage-metoden, efter att vi har skapat ticketen:
                int ticketId = await CreateTicketForCustomerAsync(message.Email, message.Name, message.Content, message.CompanyID, conn, transaction);

                // Hämtar nu aktuell status (status_id) — enligt er databasschema: 
                // status_id 3 = Resolved, status_id 4 = Closed.
                int currentStatus = await GetTicketStatus(ticketId, conn, transaction);
                if (currentStatus == 3 || currentStatus == 4)
                {
                    // Om status är Resolved eller Closed, returneras ett fel.
                    return TypedResults.BadRequest("Cannot add message to closed or resolved ticket.");
                }


                // Insert the customer’s message into the database.
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO messages (ticket_id, message, email) VALUES ($1, $2, $3)";
                cmd.Parameters.AddWithValue(ticketId);
                cmd.Parameters.AddWithValue(message.Content);
                cmd.Parameters.AddWithValue(message.Email);
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("Message inserted successfully!");

                // Commit the transaction.
                await transaction.CommitAsync();

                // Retrieve the generated token (recorded as case_number) from the ticket.
                string token = GetTicketToken(ticketId, conn, transaction);

                // Create a session by storing the token.
                context.Session.SetString("UserSession", token);
                Console.WriteLine($"Session created with token: {token}");

                // Send a confirmation email to the customer using MailKit with Gmail SMTP.
                await SendConfirmationEmailAsync(message.Email, message.Name, message.Content, token);

                return TypedResults.Created();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (transaction != null && transaction.Connection != null)
                {
                    try { await transaction.RollbackAsync(); }
                    catch (Exception rollbackEx)
                    {
                        Console.WriteLine($"Rollback Exception: {rollbackEx.Message}");
                        Console.WriteLine($"Rollback Stack Trace: {rollbackEx.StackTrace}");
                    }
                }
                return TypedResults.BadRequest($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        // Creates a new ticket (case) for the customer.
        // UPDATED: Uses email instead of user_id and generates a token stored in case_number.
        private static async Task<int> CreateTicketForCustomerAsync(string email, string title, string description, int company_id, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            // Generate a unique token using GenerateUniqueCaseNumber() and use it as the case_number.
            string token = GenerateUniqueCaseNumber();
            cmd.CommandText = "INSERT INTO tickets (user_email, title, description, case_number, company_id, date, status_id) VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING id";
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(title);
            cmd.Parameters.AddWithValue(description);
            cmd.Parameters.AddWithValue(token);   // Save token as case_number.
            cmd.Parameters.AddWithValue(company_id);
            cmd.Parameters.AddWithValue(DateTime.UtcNow);
            cmd.Parameters.AddWithValue(1);         // Default status_id is 1 (Unread)

            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar == null)
                throw new InvalidOperationException("Failed to create ticket; returned ID is null.");

            int ticketId = (int)scalar;
            Console.WriteLine($"Ticket created with token (case_number): {token}");
            return ticketId;
        }

        // Retrieves the token (case_number) for a given ticketId.
        private static string GetTicketToken(int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "SELECT case_number FROM tickets WHERE id = $1";
            cmd.Parameters.AddWithValue(ticketId);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "";
        }

        // Retrieves the status of a ticket given its ticketId.
        // This method ensures that no new messages can be added if the status is "Closed" or "Resolved".
        // Ny metod för att hämta ticketstatus (status_id) baserat på ticketId.
        private static async Task<int> GetTicketStatus(int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "SELECT status_id FROM tickets WHERE id = $1";
            cmd.Parameters.AddWithValue(ticketId);
            var result = await cmd.ExecuteScalarAsync();
            if (result == null)
            {
                throw new InvalidOperationException("No status found for ticket.");
            }
            return Convert.ToInt32(result);
        }


        // Generates a random password. (Exposed for external use.)
        public static string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        // Generates a unique token/case number.
        private static string GenerateUniqueCaseNumber()
        {
            return $"CASE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        // Sends a confirmation email using MailKit with Gmail SMTP.
        // UPDATED: Uses baseUrl set to the frontend (http://localhost:5173).
        private static async Task SendConfirmationEmailAsync(string email, string name, string content, string token)
        {
            string baseUrl = "http://localhost:5173"; // Frontend base URL

            string messageBody = $"Dear {name ?? "Customer"},\n\nWe have received your message:\n\n\"{content}\"\n\n" +
                                 $"To access your chat with customer support, please click the link below within the next hour:\n" +
                                 $"{baseUrl}/tickets/view/{token}\n\n" +
                                 $"Thank you for contacting us.\n\nBest regards,\nYour App Team";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Your App Name", "dissatisfiedcustomer2025@gmail.com"));
            mimeMessage.To.Add(new MailboxAddress(name ?? "Customer", email));
            mimeMessage.Subject = "Chat Access Confirmation";
            mimeMessage.Body = new TextPart("plain") { Text = messageBody };

            try
            {
                using var smtpClient = new SmtpClient();
                // For testing purposes, disable certificate validation.
                smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                smtpClient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtpClient.Authenticate("dissatisfiedcustomer2025@gmail.com", "yxel egbr xehm wdrt");
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                Console.WriteLine("Confirmation email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                Console.WriteLine($"Email sending Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
