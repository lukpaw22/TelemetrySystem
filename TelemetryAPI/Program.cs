using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;

internal class Program
{
    private const int PORT = 8080;
    private static IConnection rabbitConnection;
    
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        rabbitConnection = await factory.CreateConnectionAsync();
        
        RegisterIncoming(app);

        app.Run($"http://localhost:{PORT}");
    }

    static void RegisterIncoming(WebApplication app)
    {
        app.MapPost("/load", async (ReadingRequest req) =>
        {
            var bytes = System.Convert.FromBase64String(req.Data);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            
            var channel = await rabbitConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "temperature-queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var props = new BasicProperties();
            props.ContentType = "text/json";
            
            await channel.BasicPublishAsync(
                "",
                "temperature-queue",
                mandatory: true,
                basicProperties: new BasicProperties(),
                body: System.Text.Encoding.UTF8.GetBytes(text)
            );
            
            channel.Dispose();
            
            return Results.Ok(new
            {
                Message = "Success",
                Status = 200,
            });
        });
    }
}

public record ReadingRequest(string Data);