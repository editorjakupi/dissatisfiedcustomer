using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

using Npgsql;

namespace server;

public class LoginRoute
{
    public record HashDTO(string Password);
    public static IResult
        HashPassword(HashDTO dto, PasswordHasher<string> hasher)
    {
        string hash = hasher.HashPassword("", dto.Password);
        return Results.Ok(hash);
    }
    
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
               reader.GetInt32(5),
               reader.GetInt32(6)
           ));
       }
       return users;
   }
    
   public static async Task<IResult> LoginUser(HttpContext context, NpgsqlDataSource db, PasswordHasher<string> hasher)
   {
       try
       {
           var request = await context.Request.ReadFromJsonAsync<LoginRequest>();
           if (request == null) return Results.BadRequest("Invalid request");

           await using var connection = await db.OpenConnectionAsync();
           await using var command = new NpgsqlCommand("SELECT * FROM userxcompany WHERE email = @email", connection);
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
               reader.GetInt32(5),
               reader.GetInt32(6)
           );

           string storedHashedPassword = reader.GetString(3); // Get hashed password from DB

           // Verify the password
           var verifyResult = hasher.VerifyHashedPassword("", storedHashedPassword, request.password);
           if (verifyResult == PasswordVerificationResult.Failed)
               return Results.Unauthorized();

           // Create session user data and store it in the session
           var sessionUser = new
           {
               userId = user.id,
               name = user.name,
               email = user.email,
               phonenumber = user.phonenumber,
               role_id = user.role_id,
               companyId = user.companyId
           };

           context.Session.SetString("user", System.Text.Json.JsonSerializer.Serialize(sessionUser));
           Console.WriteLine($"Session set: {context.Session.GetString("user")}");

           // Return the user session data as a response
           return Results.Ok(sessionUser);
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Error: {ex.Message}");
           return Results.StatusCode(500);
       }
   }

    public static IResult GetSessionUser(HttpContext context)
    {
        var userJson = context.Session.GetString("user");
        if (string.IsNullOrEmpty(userJson))
        {
            Console.WriteLine("No user session found.");
            return Results.Json(null); // Explicitly return JSON null
        }

        Console.WriteLine($"Session retrieved: {userJson}");
        var user = System.Text.Json.JsonSerializer.Deserialize<object>(userJson);
        return Results.Json(user); // Ensure proper JSON response
    }



   
   public static IResult LogoutUser(HttpContext context)
   {
       context.Session.Clear();
       return Results.Ok("Logged out");
   }
}