using Npgsql;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Cms;
using Microsoft.Extensions.ObjectPool;

namespace server;

public class TicketFormRoutes
{

    public record TicketFormDTO(int TicketId, int ProductId, int CategoryId, string Content);

    public static async Task<Results<Created, BadRequest<string>>> PostTicketForm(TicketFormDTO ticketform, NpgsqlDataSource db)
    {
        using var command = db.CreateCommand("UPDATE tickets SET product_id = @product_id, category_id = @category_id, description = @description WHERE id = @id");
        command.Parameters.AddWithValue("@product_id", ticketform.ProductId);
        command.Parameters.AddWithValue("@category_id", ticketform.CategoryId);
        command.Parameters.AddWithValue("@description", ticketform.Content);
        command.Parameters.AddWithValue("@id", ticketform.TicketId);

        try
        {
            await command.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }
        catch
        {
            return TypedResults.BadRequest("Updating Ticket Failed");
        }
    }

    public static async Task<TicketForm> GetTicketForm(string caseNumber, NpgsqlDataSource db)
    {
        var result = new List<TicketForm>();
        await using var command = db.CreateCommand("SELECT id, company_id, title, description FROM tickets WHERE case_number = $1");
        command.Parameters.AddWithValue(caseNumber);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TicketForm(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3)));
        }

        return result[0];
    }

    public record TicketForm(
        int ticket_id,
        int company_id,
        string title,
        string description
        );
}