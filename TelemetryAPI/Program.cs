using Microsoft.AspNetCore.Builder;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run("http://localhost:3000");
    }
}