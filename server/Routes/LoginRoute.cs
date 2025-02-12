using Npgsql;

namespace server;

public class LoginRoute
{
    public static async Task<List<Users>> GetUser(NpgsqlDataSource db)
    {
        var users = new List<Users>();
        await using var cmd = db.CreateCommand("SELECT * FROM Users WHERE email = @email");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new Users{
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                email = reader.GetString(2),
                phonenumber = reader.GetString(3),
                role_id = reader.GetInt32(4)});
        }

        return users;
    }
}