using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;
public static class UserRoutes
{
    public record User(int Id, string Name);

    public static async Task<List<User>>
    GetUsers(NpgsqlDataSource db)
    {
        List<User> result = new();

        using var query = db.CreateCommand("select id, name from users");
        using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }


    public record PostUserDTO(string Email, int Role_id);
    public static async Task<Results<Created, BadRequest<string>>>
    PostUser(PostUserDTO user, NpgsqlDataSource db)
    {
        string generatedPassword = MessageRoutes.GenerateRandomPassword();
        using var command = db.CreateCommand("insert into users(email, password, role_id) VALUES($1, $2, $3)");
        command.Parameters.AddWithValue(user.Email);
        command.Parameters.AddWithValue(generatedPassword);
        command.Parameters.AddWithValue(1); //Sätter automatiskt roll på användaren

        try
        {
            await command.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch
        {
            return TypedResults.BadRequest("Failed to create new user, might already exist?"); //Felhantering
        }
    }

    public static async Task<Results<NoContent, NotFound>> DeleteUser(int id, NpgsqlDataSource db) //Ta bort användare
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
}