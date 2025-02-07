using DotNetEnv;
using app.Database;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;






class Program
{
    static async Task Main()
    {

        Env.TraversePath().Load();//laddar in variabler från .env filen behöver en variabel exempel nedaför när vi har skapat vår db
        // DBConnectString="Host=localhost;Port=5432;Username=postgres;Password=pass123;Database=dbname;SearchPath=public"        
        DatabaseConnection database = new();

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();






        app.MapGet("/", () => "Hello World!");

        app.Run();


    }
}