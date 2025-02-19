using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;

public static class EmployeeRoute
{
  public record Employees(int id, int userId, int companyId);
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
    using var cmd = db.CreateCommand("INSERT INTO employees (user_id, company_id) VALUES($1, $2)");
    cmd.Parameters.AddWithValue(employee.userId);
    cmd.Parameters.AddWithValue(employee.companyId);

    try
    {
      await cmd.ExecuteNonQueryAsync();
      return TypedResults.Created();
    }
    catch
    {
      return TypedResults.BadRequest("Failed to add: " + employee + " to employees!");
    }
  }
}