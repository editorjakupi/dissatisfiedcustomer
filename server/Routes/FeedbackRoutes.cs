using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace server;

public class FeedbackRoutes
{
    public static async Task<Results<Created, BadRequest<string>>>
    PostFeedback(FeedbackDTO feedback, NpgsqlDataSource db){
        using var cmd = db.CreateCommand("INSERT INTO feedback (ticket_id, rating, comment, date) VALUES($1, $2, $3, $4)");
        cmd.Parameters.AddWithValue(feedback.ticket_id);
        cmd.Parameters.AddWithValue(feedback.rating);
        cmd.Parameters.AddWithValue(feedback.comment);
        cmd.Parameters.AddWithValue(DateTime.UtcNow);

        try{
            await cmd.ExecuteNonQueryAsync();
            return TypedResults.Created();
        }catch{
            return TypedResults.BadRequest("Failed to send feedback " + cmd.CommandText + " " + feedback);
        }
    }
}