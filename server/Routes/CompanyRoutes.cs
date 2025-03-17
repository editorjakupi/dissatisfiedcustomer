using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Cms;
using System.Data.SqlTypes;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Tls;
using Microsoft.VisualBasic;

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

     /*   using var cmd2 = db.CreateCommand("INSERT INTO employees (company_id, user_id) SELECT id, $1 FROM company " + 
                                        "WHERE company_name = $2 AND company_phone = $3 AND company_email = $4");
        cmd2.Parameters.AddWithValue(company.admin);
        cmd2.Parameters.AddWithValue(company.name);
        cmd2.Parameters.AddWithValue(company.phone);
        cmd2.Parameters.AddWithValue(company.email);
*/
        try
        {
            await cmd.ExecuteNonQueryAsync();

           /* try{

              //  await cmd2.ExecuteNonQueryAsync();
                await UserRoutes.PutPromoteAdmin(company.admin, db);

                return TypedResults.Created();
            }
            catch
            {
                return TypedResults.BadRequest("Failed to add admin: " + company.admin);

            }*/
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
    {
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

    public static async Task<IResult>
    PutCompany(int id, CompanyDTO company, NpgsqlDataSource db){
        using var cmd = db.CreateCommand("UPDATE company SET name = $1, phone = $2, email = $3, admin = $4 WHERE id = $5");
        cmd.Parameters.AddWithValue(company.name); 
        cmd.Parameters.AddWithValue(company.phone);
        cmd.Parameters.AddWithValue(company.email);
        cmd.Parameters.AddWithValue(id);

        using var cmd2 = db.CreateCommand("UPDATE employees SET user_id = $1 WHERE company_id = $2");
        cmd2.Parameters.AddWithValue(company.admin);
        cmd2.Parameters.AddWithValue(company.id);

        try{
            await cmd.ExecuteNonQueryAsync();
            await cmd2.ExecuteNonQueryAsync();
            //await UserRoutes.PutPromoteAdmin(company.admin, db);

            return Results.Ok("Company and admin updated successfully");
        }
        catch
        {
            return TypedResults.BadRequest("Failed to update company: " + company);
        }
    }

    public static async Task<Results<Ok<List<Admin>>, NotFound>>
    GetAdmins(NpgsqlDataSource db)
    {
        var result = new List<Admin>();
        try
        {
            using var cmd = db.CreateCommand("SELECT users.id, users.name FROM users " +
                                             "WHERE NOT EXISTS( " +
                                               "SELECT * FROM employees WHERE users.id = employees.user_id)");

            await using var reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync())

            result.Add(new Admin(
                reader.GetInt32(0),
                reader.GetString(1)
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error feching Admins: {ex.Message}");
        }
        return TypedResults.Ok(result);
    }
     
       /* string nameQuery = "company_name";
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
    }*/


}