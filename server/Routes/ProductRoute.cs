﻿using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using server.Records;
using DataReaderExtensions = System.Data.DataReaderExtensions;

namespace server;

public static class ProductRoute
{
    
    public static async Task<List<Products>>
        GetProducts(int companyId, NpgsqlDataSource db)
    {
        List<Products> result = new();
        using var cmd = db.CreateCommand("SELECT id, name, description FROM product Where company_id = $1");
        cmd.Parameters.AddWithValue(companyId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new (reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)));  
        }

        return result;
    }

    public record PostProductDTO(string Name, string Description);
    public static async Task<Results<Created, BadRequest<string>>>
        PostProduct(int companyId, PostProductDTO product, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("INSERT INTO product (name, desctiption, comoany_id) VALUES($1, $2, $3)");
        cmd.Parameters.AddWithValue(product.Name);
        cmd.Parameters.AddWithValue(product.Description);
        cmd.Parameters.AddWithValue(companyId);

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
        DeleteProduct(int companyId, int id, NpgsqlDataSource db)
    {
        using var cmd = db.CreateCommand("DELETE FROM product WHERE company_id = $1 AND id = $2");
        cmd.Parameters.AddWithValue(companyId);
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
}

