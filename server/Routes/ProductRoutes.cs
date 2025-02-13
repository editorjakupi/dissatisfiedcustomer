using Npgsql;

namespace server;

public class ProductRoutes{

    public record Product(int id, string name, string description);
    public static async Task<List<Product>> GetProducts(int company_id, NpgsqlDataSource db)
    {
        List<Product> list = new();

        await using var query = db.CreateCommand("select id, name, description from product where company_id = $1");
        query.Parameters.AddWithValue(company_id);
        await using var reader = await query.ExecuteReaderAsync();

        while(await reader.ReadAsync()) {
        list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
    }
    return list;
    }
}