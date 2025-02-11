using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;



class Program
{
    static async Task Main()
    {

        //var variableList = FileReader.Load("logIn.txt"); //laddar in variabler från .login.txt filen behöver en variabel exempel nedaför när vi har skapat vår db
        // DBConnectString=Host=localhost    Port = 5432  Username = postgre Password = databas123 Database = dissatisfiedcustommer SearchPath =public

        var variableList = FileReader.Load("logIn.txt");

        //DatabaseConnection database = new();

        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();



        app.MapGet("/", () => variableList["Password"]);

        app.Run();


    }
}