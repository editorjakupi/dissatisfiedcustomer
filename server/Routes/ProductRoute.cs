using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql.Replication.PgOutput.Messages;
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


    public static async Task<IResult>
        PostProduct(PostProductDTO product, NpgsqlDataSource db, HttpContext context)
    {
        if (context.Session.GetInt32("company") is int comapny_id)
        {
            Console.WriteLine($"Received request: {product.Name}, {product.Description}, {comapny_id}");

            using var cmd = db.CreateCommand("INSERT INTO product (name, description, company_id) VALUES($1, $2, $3)");
            cmd.Parameters.AddWithValue(product.Name);
            cmd.Parameters.AddWithValue(product.Description);
            cmd.Parameters.AddWithValue(comapny_id);

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
        
        Console.WriteLine("No Product Found");
        return Results.BadRequest();
    }

    public static async Task<Results<NoContent, NotFound>>
        DeleteProduct(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("DELETE FROM product WHERE id = $1");
        cmd.Parameters.AddWithValue(id);

        int affectedRows = await cmd.ExecuteNonQueryAsync();
        if (affectedRows > 0)
        {
            return TypedResults.NoContent();
        }
        else
        {
            return TypedResults.NotFound();
        }

    }

    public static async Task<IResult>
        UpdateProduct(int id, PutProductDTO product, NpgsqlDataSource db)
    {
        await using var connection = await db.OpenConnectionAsync();
        
        await using var updateCmd = new NpgsqlCommand(
            "UPDATE product SET name = @name, description = @description WHERE id = @id",
            connection);
        updateCmd.Parameters.AddWithValue("@name", product.name);
        updateCmd.Parameters.AddWithValue("@description", product.description);
        updateCmd.Parameters.AddWithValue("@id", product.id); // ID should be last
        
        await updateCmd.ExecuteNonQueryAsync();
        return Results.Ok("Product updated successfully");
    }

    public static async Task<List<Products>>
        GetProduct(int productId, NpgsqlDataSource db)
    {
        var result = new List<Products>();
        using var cmd = db.CreateCommand("SELECT * FROM product WHERE id = $1");
        cmd.Parameters.AddWithValue(productId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)));

        return result;
    }
}

