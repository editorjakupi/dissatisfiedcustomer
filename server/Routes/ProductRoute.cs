using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql.Replication.PgOutput.Messages;
using server.Records;
using DataReaderExtensions = System.Data.DataReaderExtensions;

namespace server;

public static class ProductRoute
{
    
    public static async Task<List<Products>>
        GetProducts(int companyId, NpgsqlDataSource db)
    {
        var result = new List<Products>();
        using var cmd = db.CreateCommand("SELECT * FROM product WHERE company_id = $1");
        cmd.Parameters.AddWithValue(companyId);
        
        using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt32(3)));
            }
        return result;
    }

    public record PostProductDTO(string Name, string Description, int companyId);
    public static async Task<Results<Created, BadRequest<string>>>
        PostProduct(PostProductDTO product, NpgsqlDataSource db)
    {
        
        Console.WriteLine($"Received request: {product.Name}, {product.Description}, {product.companyId}");
        
        using var cmd = db.CreateCommand("INSERT INTO product (name, description, company_id) VALUES($1, $2, $3)");
        cmd.Parameters.AddWithValue(product.Name);
        cmd.Parameters.AddWithValue(product.Description);
        cmd.Parameters.AddWithValue(product.companyId);

        try
        {
            await cmd.ExecuteNonQueryAsync();   
            return TypedResults.Created();
        }
        catch
        {
            return TypedResults.BadRequest("Failed to create product " + product);
        }
    }

    public static async Task<Results<NoContent, NotFound>>
        DeleteProduct(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("DELETE FROM product WHERE id = $1");
        cmd.Parameters.AddWithValue(id);

        int affectedRows = await cmd.ExecuteNonQueryAsync();
        if(affectedRows > 0){
            return TypedResults.NoContent();
        }
        else
        {
            return TypedResults.NotFound();
        }
        
    }

    public record PutProductDTO(string Name, string Description);
    public static async Task<Results<Ok<string>, BadRequest<string>>>
        UpdateProduct(int id, PutProductDTO product, NpgsqlDataSource db)
    {
        string nameQuery = "";
        string descriptionQuery = "";
        string sqlquery;
        
        //if name is updated
        if(!string.IsNullOrWhiteSpace(product.Name))
            nameQuery = "SET name = \"" + product.Name + "\"";
            
        //if discription is updated
        if(!string.IsNullOrWhiteSpace(product.Description))
            descriptionQuery = "SET description = \"" + product.Description + "\"";
        
        //create query
        if(!nameQuery.Equals("")&&!descriptionQuery.Equals(""))
            sqlquery = "UPDATE product " + nameQuery + " AND " + descriptionQuery;
        else if(!nameQuery.Equals(""))
            sqlquery = "UPDATE product " + nameQuery;
        else if(!descriptionQuery.Equals(""))
            sqlquery = "UPDATE product " + descriptionQuery;
        else
        {
            return TypedResults.BadRequest("Update query failed");
        }
        sqlquery += " WHERE id = $1";

        using var cmd = db.CreateCommand((sqlquery));
        cmd.Parameters.AddWithValue(id);

        try
        {
            await cmd.ExecuteReaderAsync();
            return TypedResults.Ok("Product updated");

        }
        catch (Exception e)
        {
            return TypedResults.BadRequest("Product update failed, " +  e);
        }
    }
    
    public static async Task<List<Products>>
        GetProduct(int productId, NpgsqlDataSource db)
    {
        var result = new List<Products>();
        using var cmd = db.CreateCommand("SELECT * FROM product WHERE id = $1");
        cmd.Parameters.AddWithValue(productId);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new (
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)));
            
        return result;
    }
}

