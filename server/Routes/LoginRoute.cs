using Npgsql;

namespace server;

public class LoginRoute
{
   // public record GetUserDTO(string email);
    public static async Task<List<Users>> GetUser( int user, NpgsqlDataSource db)
    {
        var users = new List<Users>();
        await using var cmd = db.CreateCommand("SELECT * FROM Users WHERE id = $1");
        cmd.Parameters.AddWithValue(user);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new Users(
                reader.GetInt32(0),
                reader.GetString(1), 
                reader.GetString(2), 
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5)
                ));
        } 
        ;
        return users;
    }
}