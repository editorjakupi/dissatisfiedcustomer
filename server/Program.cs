using DotNetEnv;
using app.Database;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;



class Program
{
    static async Task Main()
    {

        //Env.TraversePath().Load();//laddar in variabler från .env filen behöver en variabel exempel nedaför när vi har skapat vår db
        // DBConnectString="Host=localhost;Port=5432;Username=postgres;Password=pass123;Database=dbname;SearchPath=public"        

        var variableList = FileReader.Load("logIn.txt");

        //DatabaseConnection database = new();

        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();



        app.MapGet("/", () => variableList["Password"]);

        app.Run();


    }
}