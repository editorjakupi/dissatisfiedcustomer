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


    public record PostUserDTO(string Name);
    public static async Task<Results<Created, BadRequest<string>>>
    PostUser(PostUserDTO user, NpgsqlDataSource db)
    {
        using var command = db.CreateCommand("insert into users(name) values($1)");
        command.Parameters.AddWithValue(user.Name);

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

}