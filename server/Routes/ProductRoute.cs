﻿using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using server.Records;
using DataReaderExtensions = System.Data.DataReaderExtensions;

namespace server;

public static class ProductRoute
{

    public record PostProductDTO(string Name, string Description);

    public static async Task<Results<Created, BadRequest<string>>>
        PostProduct(PostProductDTO product, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("INSERT INTO products (name, desctiption) VALUES ($1,$2)");
        cmd.Parameters.AddWithValue("$1", product.Name);
        cmd.Parameters.AddWithValue("$2", product.Description);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch
        {
            return TypedResults.BadRequest("Failed to create product");
        }
    }

    public static async Task<Results<NoContent, NotFound>>
        DeleteProduct(int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("DELETE FROM products WHERE id = $1");
        cmd.Parameters.AddWithValue("$1", id);

        int affectedRows = await cmd.ExecuteNonQueryAsync();
        if(affectedRows > 0){
            return TypedResults.NoContent();
        }
        else
        {
            return TypedResults.NotFound();
        }
        
    }

    public static async Task<List<Products>>
        GetProducts(NpgsqlDataSource db)
    {
        List<Products> result = new();
        using var cmd = db.CreateCommand("SELECT * FROM products");
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
          result.Add(new Products(reader.GetInt32(0),
              reader.GetString(1),
              reader.GetString(2),
              reader.GetInt32(3)));  
        }

        return result;
        
    }
}

