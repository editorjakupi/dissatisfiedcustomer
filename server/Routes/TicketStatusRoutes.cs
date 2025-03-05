using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;

namespace server;

public static class TicketStatusRoutes
{
  public static async Task<List<TicketStatus>>
  GetTicketStatus(NpgsqlDataSource db)
  {
    List<TicketStatus> result = new();

    using var query = db.CreateCommand("SELECT * FROM ticketstatus");
    using var reader = await query.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
      result.Add(new(reader.GetInt32(0), reader.GetString(1)));
    }
    return result;
  }
}