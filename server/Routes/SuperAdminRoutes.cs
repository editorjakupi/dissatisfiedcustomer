using Npgsql;
namespace server;

public class SuperAdminRoutes
{
    public static async Task<IResult> PutAdmin(int userId, NpgsqlDataSource db)
    {
        Console.WriteLine($"Received request: UserId={userId} demotion.");
        
        await using var conn = await db.OpenConnectionAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            await using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = "UPDATE users SET role_id = 2 WHERE id = $1";
            updateCmd.Parameters.AddWithValue(userId);
            updateCmd.Transaction = transaction;
            
            int updated = await updateCmd.ExecuteNonQueryAsync();
            if (updated == 0)
            {
                await transaction.RollbackAsync();
                return TypedResults.BadRequest("Failed to demote users role!");
            }

            await transaction.CommitAsync();
            return TypedResults.Created();

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error demoting admin: {ex.Message}");
            return TypedResults.BadRequest("Failed to demote Admin!");
        }

    }
    public static async Task<List<Users>>
        GetAdmins(NpgsqlDataSource db)
    {
        var result = new List<Users>();
        using var cmd = db.CreateCommand("SELECT * FROM userxcompany WHERE role_id = 3");

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(
                new(
                    reader.GetInt32(0),
                    reader.GetString(2),
                    reader.GetString(1),
                    reader.GetString(3), // password
                    reader.GetString(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6)
                )
            );
        }
        return result;
    }
    
    public static async Task<List<Users>>
        GetAdmin(int userId, NpgsqlDataSource db)
    {
        List<Users> result = new();
        using var cmd = db.CreateCommand("SELECT userxcompany.id, userxcompany.name, userxcompany.email, userxcompany.password, userxcompany.phonenumber, userxcompany.role_id, userxcompany.companyId FROM employees JOIN userxcompany ON employees.user_id = userxcompany.id WHERE userxcompany.companyId = $1");
        cmd.Parameters.AddWithValue(userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(
                new(
                    reader.GetInt32(0),
                    reader.GetString(2),
                    reader.GetString(1),
                    reader.GetString(3), // password
                    reader.GetString(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6)
                )
            );
        }
        return result;
    }
}