using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using server; // Förutsätter att record-definitioner finns i samma namespace

namespace server
{
  public static class EmployeeRoute
  {
    // Befintlig metod hämtar employees via userId (används fortfarande för vissa interna fall)
    public static async Task<List<Employees>> GetEmployees(int userId, NpgsqlDataSource db)
    {
      var result = new List<Employees>();
      using var cmd = db.CreateCommand("SELECT * FROM employees WHERE user_id = $1");
      cmd.Parameters.AddWithValue(userId);
      using var reader = await cmd.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        result.Add(new Employees(
            reader.GetInt32(0), // id
            reader.GetInt32(1), // userId
            reader.GetInt32(2)  // companyId
        ));
      }
      return result;
    }

    // NY METOD: Hämta employee baserat på e-post istället för userId
    public static async Task<List<Users>> GetEmployeeByEmail(string email, NpgsqlDataSource db)
    {
      var result = new List<Users>();
      // Använder vyn "userxcompany" för att hämta både användarinformation och companyId
      using var cmd = db.CreateCommand("SELECT * FROM userxcompany WHERE email = $1");
      cmd.Parameters.AddWithValue(email);
      using var reader = await cmd.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        result.Add(new Users(
            reader.GetInt32(0),   // id
            reader.GetString(1),  // name
            reader.GetString(2),  // email
            reader.GetString(3),  // password
            reader.GetString(4),  // phonenumber
            reader.GetInt32(5),   // role_id
            reader.GetInt32(6)    // companyId
        ));
      }
      return result;
    }

    // NY METOD: Radera employee baserat på e-post
    // Ändrad returtyp till IResult så att vi kan returnera BadRequest<string>
    public static async Task<IResult> DeleteEmployeeByEmail(string email, NpgsqlDataSource db)
    {
      await using var conn = await db.OpenConnectionAsync();
      await using var transaction = await conn.BeginTransactionAsync();
      try
      {
        using var getCmd = conn.CreateCommand();
        getCmd.Transaction = transaction;
        getCmd.CommandText = "SELECT id FROM users WHERE email = $1";
        getCmd.Parameters.AddWithValue(email);
        var userIdObj = await getCmd.ExecuteScalarAsync();
        if (userIdObj == null)
          return TypedResults.NotFound();
        int userId = (int)userIdObj;

        using var deleteCmd = conn.CreateCommand();
        deleteCmd.Transaction = transaction;
        deleteCmd.CommandText = "DELETE FROM employees WHERE user_id = $1";
        deleteCmd.Parameters.AddWithValue(userId);
        int deleted = await deleteCmd.ExecuteNonQueryAsync();
        if (deleted == 0)
        {
          await transaction.RollbackAsync();
          return TypedResults.NotFound();
        }

        using var updateCmd = conn.CreateCommand();
        updateCmd.Transaction = transaction;
        updateCmd.CommandText = "UPDATE users SET role_id = 1 WHERE id = $1";
        updateCmd.Parameters.AddWithValue(userId);
        await updateCmd.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
        return TypedResults.NoContent();
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        return TypedResults.BadRequest("Error: " + ex.Message);
      }
    }

    // NY METOD: Skapa en ny anställdpost med existerande användar-id
    public static async Task<IResult> PostEmployee(Employees employee, NpgsqlDataSource db)
    {
      Console.WriteLine($"Received request: userId={employee.userId}, CompanyId={employee.companyId}");
      await using var conn = await db.OpenConnectionAsync();
      await using var transaction = await conn.BeginTransactionAsync();
      try
      {
        using var insertCmd = conn.CreateCommand();
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
        using var updateCmd = conn.CreateCommand();
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
        return TypedResults.BadRequest("Error: " + ex.Message);
      }
    }
  }
}
