using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;
public static class UserRoutes
{
    public static async Task<List<Users>>
        GetUsers(NpgsqlDataSource db)
    {
        List<Users> result = new();

        using var query = db.CreateCommand("select * from users");
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
                    reader.GetInt32(5)
                )
            );
        }

        return result;
    }

    public record PostUserDTO(string Name, string Email, string Password);
    public static async Task<Results<Created, BadRequest<string>>>
    PostUser(PostUserDTO user, NpgsqlDataSource db)
    {
        using var command = db.CreateCommand("insert into users(name, email, password) VALUES($1, $2, $3)");
        command.Parameters.AddWithValue(user.Name);
        command.Parameters.AddWithValue(user.Email);
        command.Parameters.AddWithValue(user.Password);

        try
        {
            await command.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch
        {
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
            reader.GetInt32(5)
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