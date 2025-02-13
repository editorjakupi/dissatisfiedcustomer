using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;

public static class CategoryRoutes
{
  public record Category(int id, string name);
  public static async Task<List<Category>>
  GetCategories(NpgsqlDataSource db)
  {
    List<Category> result = new();

    using var query = db.CreateCommand("select id, name from category");
    using var reader = await query.ExecuteReaderAsync();
      
    while(await reader.ReadAsync()) {
      result.Add(new(reader.GetInt32(0), reader.GetString(1)));
    }
    return result;
    }
}