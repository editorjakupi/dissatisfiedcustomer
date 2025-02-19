using Npgsql;

namespace server;

public class LoginRoute
{
   // public record GetUserDTO(string email);
   public static async Task<List<Users>> GetUser(int user, NpgsqlDataSource db)
   {
       var users = new List<Users>();
       await using var cmd = db.CreateCommand("SELECT * FROM users WHERE id = $1");
       cmd.Parameters.AddWithValue(user); // Correct way to add parameter with name
       
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
       return users;
   }
    
    public static async Task<IResult> LoginUser(HttpContext context, NpgsqlDataSource db)
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<LoginRequest>();
            if (request == null) return Results.BadRequest("Invalid request");

            await using var connection = await db.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", connection);
            command.Parameters.AddWithValue("@email", request.email);

            await using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows) return Results.Unauthorized();

            await reader.ReadAsync();
            var user = new Users(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5)
            );

            if (user.password != request.password) // Plain text comparison
                return Results.Unauthorized();

            return Results.Ok(new
            {
                id = user.id,
                name = user.name,
                email = user.email,
                phoneNumber = user.phonenumber,
                roleId = user.role_id
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.StatusCode(500);
        }
    }
}