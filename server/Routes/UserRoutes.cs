using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace server;
public static class UserRoutes
{
    public static async Task<List<Users>>
        GetUsers(NpgsqlDataSource db)
    {
        List<Users> result = new();
        try
        {
            using var query = db.CreateCommand("select * from userxcompany");
            using var reader = await query.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(
                    new(
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

    public record PostUserDTO(string Name, string Email, string Password, string Phonenumber);

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
                Console.WriteLine($"User created with ID: {id}" + "Users new password: " + generatedPassword);
                return TypedResults.Created($"/api/users/{id}", id.ToString());
            }
            return TypedResults.BadRequest("Failed to retrieve user ID.");
        }
        catch(Exception ex)
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

    public static async Task<IResult> PutUsers(Users updatedUser, NpgsqlDataSource db)
    {
        if (updatedUser == null)
            return Results.BadRequest("Invalid user data");

        await using var connection = await db.OpenConnectionAsync();

        // Fetch existing user
        await using var selectCmd = new NpgsqlCommand(
            "SELECT id, name, email, password, phonenumber, role_id FROM users WHERE id = @id",
            connection);
        selectCmd.Parameters.AddWithValue("@id", updatedUser.id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return Results.NotFound("User not found");

        var existingUser = new Users(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3), // password
            reader.GetString(4),
            reader.GetInt32(5),
            reader.GetInt32(6)
        );

        await reader.CloseAsync();

        // Only update the password if a new one is provided
        var newPassword = string.IsNullOrEmpty(updatedUser.password) ? existingUser.password : updatedUser.password;

        var newUser = existingUser with
        {
            name = !string.IsNullOrEmpty(updatedUser.name) ? updatedUser.name : existingUser.name,
            email = !string.IsNullOrEmpty(updatedUser.email) ? updatedUser.email : existingUser.email,
            phonenumber = !string.IsNullOrEmpty(updatedUser.phonenumber)
                ? updatedUser.phonenumber
                : existingUser.phonenumber,
            password = newPassword
        };

        // Update user in database
        await using var updateCmd = new NpgsqlCommand(
            "UPDATE users SET name = @name, email = @email, phonenumber = @phonenumber, password = @password WHERE id = @id",
            connection);
        updateCmd.Parameters.AddWithValue("@id", newUser.id);
        updateCmd.Parameters.AddWithValue("@name", newUser.name);
        updateCmd.Parameters.AddWithValue("@email", newUser.email);
        updateCmd.Parameters.AddWithValue("@phonenumber", newUser.phonenumber);
        updateCmd.Parameters.AddWithValue("@password", newUser.password);

        await updateCmd.ExecuteNonQueryAsync();

        return Results.Ok(newUser);
    }  


}