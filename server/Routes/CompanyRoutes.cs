using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Cms;
using System.Data.SqlTypes;
using System.Data;

namespace server;

public class CompanyRoutes
{

    public static async Task<Results<Created, BadRequest<string>>>
    PostCompany(CompanyDTO company, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("INSERT INTO company (company_name, company_phone, company_email) VALUES($1, $2, $3)");
        cmd.Parameters.AddWithValue(company.name);
        cmd.Parameters.AddWithValue(company.phone);
        cmd.Parameters.AddWithValue(company.email);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch
        {
            return TypedResults.BadRequest("Failed to add company: " + company);
        }
    }

    public static async Task<Results<NoContent, NotFound>>
    DeleteCompany(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("DELETE FROM company WHERE id = $1");
        cmd.Parameters.AddWithValue(id);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected > 0)
        {
            return TypedResults.NoContent();
        }
        else
        {
            return TypedResults.NotFound();
        }
    }

    public static async Task<Results<Ok<Company>, NotFound>>
    GetCompany(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("SELECT * FROM company_view WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return TypedResults.NotFound();
        }
        var result = new Company(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4)
        );

        return TypedResults.Ok(result);

    }

    public static async Task<Results<Ok<List<Company>>, NotFound>>
    GetCompanies(NpgsqlDataSource db)
    {// AND users.role_id = 3 
        int? adm = null;
        var result = new List<Company>();
        try
        {
            using var cmd = db.CreateCommand("SELECT * FROM company_view");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
    
                result.Add(new Company(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)
                ));
                

            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Fetching companies: {ex.Message}");
        }
        return TypedResults.Ok(result);

    }

    public static async Task<Results<Ok<string>, BadRequest<string>>>
    PutCompany(int id, CompanyDTO company, NpgsqlDataSource db){
        string nameQuery = "company_name";
        string phoneQuery = "company_phone";
        string emailQuery = "company_email";
        string query;

        if (!string.IsNullOrWhiteSpace(company.name))
        {
            nameQuery = "@company_name";
        }

        if (!string.IsNullOrWhiteSpace(company.phone))
        {
            phoneQuery = "@company_phone";
        }

        if (!string.IsNullOrWhiteSpace(company.email))
        {
            emailQuery = "@company_email";
        }

        query = "UPDATE company SET company_name = " + nameQuery + ", company_phone = " + phoneQuery + ", company_email = " + emailQuery + " WHERE id = @id";
        using var cmd = db.CreateCommand((query));
        cmd.Parameters.AddWithValue("@company_name", company.name);
        cmd.Parameters.AddWithValue("@company_phone", company.phone);
        cmd.Parameters.AddWithValue("@company_email", company.email);
        cmd.Parameters.AddWithValue("@id", id);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                return TypedResults.Ok("Company Updated!");
            }
            else
            {
                return TypedResults.BadRequest("Company with id:" + id + " not found!");
            }


        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest("Company failed to update, " + ex);
        }
    }


}