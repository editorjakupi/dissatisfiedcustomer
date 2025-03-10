using Microsoft.AspNetCore.Http.HttpResults;

using Npgsql;

namespace server;

public class FeedbackRoutes
{
    public static async Task<Results<Created, BadRequest<string>>>
    PostFeedback(FeedbackDTO feedback, NpgsqlDataSource db){
        
    }
}