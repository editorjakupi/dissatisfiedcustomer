using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace server
{
    public static class UserRoutes
    {
        public static async Task<List<Users>> GetUsersFromCompanys(NpgsqlDataSource db)
        {
            List<Users> result = new();
            try
            {
                using var query = db.CreateCommand("select * from userxcompany");
                using var reader = await query.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(
                        new Users(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3), // password
                            reader.GetString(4),
                            reader.GetInt32(5),
                            reader.GetInt32(6)
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Fetching users: {ex.Message}");
            }
            return result;
        }

        public static async Task<List<UserDTO>> GetUsers(NpgsqlDataSource db)
        {
            List<UserDTO> result = new();
            try
            {
                using var query = db.CreateCommand("select * from users");
                using var reader = await query.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(
                        new UserDTO(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3), // password
                            reader.GetString(4),
                            reader.GetInt32(5)
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Fetching users: {ex.Message}");
            }
            return result;
        }

        public static async Task<Results<Created<string>, BadRequest<string>>>
            PostUser(PostUserDTO user, NpgsqlDataSource db, PasswordHasher<string> hasher)
        {
            Console.WriteLine($"Received request: {user.Name}, {user.Email}, {user.Password}, {user.Phonenumber}");
            string generatedPassword = MessageRoutes.GenerateRandomPassword();
            string hashedPassword = hasher.HashPassword("", generatedPassword);
            using var command = db.CreateCommand(
                "INSERT INTO users(name, email, password, phonenumber, role_id) VALUES($1, $2, $3, $4, $5) RETURNING id"
            );
            command.Parameters.AddWithValue(user.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue(user.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue(hashedPassword ?? (object)DBNull.Value);
            command.Parameters.AddWithValue(user.Phonenumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue(1);

            try
            {
                var userId = await command.ExecuteScalarAsync();
                if (userId is int id)
                {
                    Console.WriteLine($"User created with ID: {id}" + $" User Password: {generatedPassword}");
                    await SendConfirmationEmailAsync(user.Email ?? "", user.Name ?? "", generatedPassword);
                    return TypedResults.Created($"/api/users/{id}", id.ToString());
                }
                return TypedResults.BadRequest("Failed to retrieve user ID.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return TypedResults.BadRequest("Failed to create new user, might already exist?");
            }
        }


        public static async Task<Results<NoContent, NotFound>> DeleteUser(int id, NpgsqlDataSource db)
        {
            using var command = db.CreateCommand("DELETE FROM users WHERE id = $1");
            command.Parameters.AddWithValue(id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                return TypedResults.NoContent();
            }
            else
            {
                return TypedResults.NotFound();
            }
        }

        public static async Task<IResult> PutUsers(Users updatedUser, NpgsqlDataSource db, PasswordHasher<string> hasher)
        {
            if (updatedUser == null)
                return Results.BadRequest("Invalid user data");

            await using var connection = await db.OpenConnectionAsync();

            // Fetch existing user
            await using var selectCmd = new NpgsqlCommand(
                "SELECT id, name, email, password, phonenumber, role_id, companyid FROM userxcompany WHERE id = @id",
                connection);
            selectCmd.Parameters.AddWithValue("@id", updatedUser.id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Results.NotFound("User not found");

            var existingUser = new Users(
                reader.GetInt32(0),
                reader.GetString(2),
                reader.GetString(1),
                reader.GetString(3), // stored hashed password
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6)
            );

            await reader.CloseAsync();

            // Hash the new password if provided, otherwise keep the existing
            string newPasswordHash = string.IsNullOrEmpty(updatedUser.password)
                ? existingUser.password
                : hasher.HashPassword("", updatedUser.password);

            var newUser = existingUser with
            {
                name = !string.IsNullOrEmpty(updatedUser.name) ? updatedUser.name : existingUser.name,
                email = !string.IsNullOrEmpty(updatedUser.email) ? updatedUser.email : existingUser.email,
                phonenumber = !string.IsNullOrEmpty(updatedUser.phonenumber)
                    ? updatedUser.phonenumber
                    : existingUser.phonenumber,
                password = newPasswordHash
            };

            // Update user in database
            await using var updateCmd = new NpgsqlCommand(
                "UPDATE users SET name = @name, email = @email, phonenumber = @phonenumber, password = @password WHERE id = @id",
                connection);
            updateCmd.Parameters.AddWithValue("@name", newUser.name);
            updateCmd.Parameters.AddWithValue("@email", newUser.email);
            updateCmd.Parameters.AddWithValue("@phonenumber", newUser.phonenumber);
            updateCmd.Parameters.AddWithValue("@password", newUser.password);
            updateCmd.Parameters.AddWithValue("@id", newUser.id);

            await updateCmd.ExecuteNonQueryAsync();

        return Results.Ok("User updated successfully");
    }
    
    public static async Task<IResult> PutUserForSAdmin(PutUserDTO postUser, NpgsqlDataSource db)
    {
        await using var connection = await db.OpenConnectionAsync();
        
        await using var updateCmd = new NpgsqlCommand(
            "UPDATE users SET name = @name, email = @email, phonenumber = @phonenumber WHERE id = @id",
            connection);
        updateCmd.Parameters.AddWithValue("@name", postUser.name);
        updateCmd.Parameters.AddWithValue("@email", postUser.email);
        updateCmd.Parameters.AddWithValue("@phonenumber", postUser.phonenumber);
        updateCmd.Parameters.AddWithValue("@id", postUser.id); // ID should be last
        
        await updateCmd.ExecuteNonQueryAsync();
        return Results.Ok("User updated successfully");
    }
    
    

        public static async Task<IResult> PutPromoteAdmin(int userId, NpgsqlDataSource db)
        {
            Console.WriteLine($"Received request: UserId={userId} promote to admin.");

            await using var conn = await db.OpenConnectionAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                await using var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = "UPDATE users SET role_id = 3 WHERE id = $1";
                updateCmd.Parameters.AddWithValue(userId);
                updateCmd.Transaction = transaction;

                int updated = await updateCmd.ExecuteNonQueryAsync();
                if (updated == 0)
                {
                    await transaction.RollbackAsync();
                    return TypedResults.BadRequest("Failed to promote to admin role!");
                }

                await transaction.CommitAsync();
                return TypedResults.Created();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error promote to admin: {ex.Message}");
                return TypedResults.BadRequest("Failed to promote to Admin!");
            }

        }

        private static async Task SendConfirmationEmailAsync(string email, string name, string content)
        {
            string baseUrl = "http://localhost:5173"; // Uppdaterad bas-URL

            string messageBody = $"Welcome {name ?? "Customer"},\n\n Your password is:\n\n\"{content}\"\n\n" +
                                 $"To access your account, please click the link below:\n" +
                                 $"{baseUrl}\n\n" +
                                 $"Thank you for joining us.\n\nBest regards,\nYour App Team";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Your App Name", "dissatisfiedcustomer2025@gmail.com"));
            mimeMessage.To.Add(new MailboxAddress(name ?? "Customer", email));
            mimeMessage.Subject = "Chat Access Confirmation";
            mimeMessage.Body = new TextPart("plain") { Text = messageBody };

            try
            {
                using var smtpClient = new SmtpClient();
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
