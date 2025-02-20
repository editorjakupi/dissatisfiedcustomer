using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Cms;

namespace server;

public class CompanyRoutes{

    public record PostCompanyDTO(string name, string phone, string email);

    public static async Task<Results<Created, BadRequest<string>>>
    PostCompany(PostCompanyDTO company, NpgsqlDataSource db){
        using var cmd = db.CreateCommand("INSERT INTO company (company_name, company_phone, company_email) VALUES($1, $2, $3)");
        cmd.Parameters.AddWithValue(company.name);
        cmd.Parameters.AddWithValue(company.phone);
        cmd.Parameters.AddWithValue(company.email);

        try{
            await cmd.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch{
            return TypedResults.BadRequest("Failed to add company: " + company);
        }
    }

    public static async Task<Results<NoContent, NotFound>>
    DeleteCompany(int id, NpgsqlDataSource db){
        using var cmd = db.CreateCommand("DELETE FROM company WHERE id = $1");
        cmd.Parameters.AddWithValue(id);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if(rowsAffected > 0){
            return TypedResults.NoContent();
        }else{
            return TypedResults.NotFound();
        }
    }

    public static async Task<Results<Ok<Company>, NotFound>>
    GetCompany(int id, NpgsqlDataSource db){
        using var cmd = db.CreateCommand("SELECT * FROM company WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if(!await reader.ReadAsync()){
            return TypedResults.NotFound();
        }
            var result = new Company(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            );
            
        return TypedResults.Ok(result);
        
    }

    public static async Task<Results<Ok<List<Company>>, NotFound>>
    GetCompanies(NpgsqlDataSource db){
        var result = new List<Company>();
        try{
        using var cmd = db.CreateCommand("SELECT * FROM company");
        await using var reader = await cmd.ExecuteReaderAsync();
        while(await reader.ReadAsync()){
            result.Add(new Company(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            ));
        }
        }catch(Exception ex){
            Console.WriteLine($"Error Fetching companies: {ex.Message}");
        }
        return TypedResults.Ok(result);
        
    }

    public record Company(
    int id, 
    string name, 
    string phone, 
    string email
    );
}