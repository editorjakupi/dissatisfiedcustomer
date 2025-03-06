using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data.Common;

namespace server;

public static class EmployeeRoute
{

  public static async Task<List<Employees>>
  GetEmployees(int userId, NpgsqlDataSource db)
  {
    var result = new List<Employees>();
    using var cmd = db.CreateCommand("SELECT * FROM employees WHERE user_id = $1");
    cmd.Parameters.AddWithValue(userId);

    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      result.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
    }
    return result;
  }

  public static async Task<Results<Created, BadRequest<string>>> 
    PostEmployee(Employees employee, NpgsqlDataSource db)
  {
    Console.WriteLine($"Received request: UserId={employee.userId}, CompanyId={employee.companyId}");

    await using var conn = await db.OpenConnectionAsync();
    await using var transaction = await conn.BeginTransactionAsync();

    try
    {
      // Insert employee
      await using var insertCmd = conn.CreateCommand();
      insertCmd.CommandText = "INSERT INTO employees (user_id, company_id) VALUES ($1, $2)";
      insertCmd.Parameters.AddWithValue(employee.userId);
      insertCmd.Parameters.AddWithValue(employee.companyId);
      insertCmd.Transaction = transaction;

      int inserted = await insertCmd.ExecuteNonQueryAsync();
      if (inserted == 0)
      {
        await transaction.RollbackAsync();
        return TypedResults.BadRequest("Failed to add employee!");
      }

      // Update user role
      await using var updateCmd = conn.CreateCommand();
      updateCmd.CommandText = "UPDATE users SET role_id = 2 WHERE id = $1";
      updateCmd.Parameters.AddWithValue(employee.userId);
      updateCmd.Transaction = transaction;

      int updated = await updateCmd.ExecuteNonQueryAsync();
      if (updated == 0)
      {
        await transaction.RollbackAsync();
        return TypedResults.BadRequest("Failed to update user role!");
      }

      await transaction.CommitAsync();
      return TypedResults.Created();
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      Console.WriteLine($"Error inserting employee: {ex.Message}");
      return TypedResults.BadRequest("Failed to add employee!");
    }
  }

  public static async Task<List<Users>>
    GetEmployee(int userId, NpgsqlDataSource db)
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

  public static async Task<Results<NoContent, NotFound>> DeleteEmployee(int id, NpgsqlDataSource db)
  {
    await using var conn = await db.OpenConnectionAsync();
    await using var transaction = await conn.BeginTransactionAsync();

    try
    {
      await using var deleteCmd = conn.CreateCommand();
      deleteCmd.CommandText = "DELETE FROM employees WHERE user_id = $1";
      deleteCmd.Parameters.AddWithValue(id);
      deleteCmd.Transaction = transaction;

      int deleted = await deleteCmd.ExecuteNonQueryAsync();
      if (deleted == 0)
      {
        await transaction.RollbackAsync();
        return TypedResults.NotFound();
      }

      await using var updateCmd = conn.CreateCommand();
      updateCmd.CommandText = "UPDATE users SET role_id = 1 WHERE id = $1";
      updateCmd.Parameters.AddWithValue(id);
      updateCmd.Transaction = transaction;
      await updateCmd.ExecuteNonQueryAsync();

      await transaction.CommitAsync();
      return TypedResults.NoContent();
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }
}