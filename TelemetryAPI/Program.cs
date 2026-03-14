using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;

internal class Program
{
    private const int PORT = 8080;
    private static ConnectionFactory rabbitFactory;
    
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        
        rabbitFactory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        rabbitFactory = new ConnectionFactory();
        
        RegisterIncoming(app);

        app.Run($"http://localhost:{PORT}");
    }

    static void RegisterIncoming(WebApplication app)
    {
        app.MapPost("/load", async (ReadingRequest req) =>
        {
            var bytes = System.Convert.FromBase64String(req.Data);
            var text = System.Text.Encoding.UTF8.GetString(bytes);

            var conn = await rabbitFactory.CreateConnectionAsync();
            var channel = await conn.CreateChannelAsync();

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
            conn.Dispose();
            
            return Results.Ok(new
            {
                Message = text,
            });
        });
    }
}

public record ReadingRequest(string Data);