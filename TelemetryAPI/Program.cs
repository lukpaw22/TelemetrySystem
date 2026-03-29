using System.Text;
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
        
        //Test2

        rabbitConnection = await factory.CreateConnectionAsync();
        
        RegisterMiddleware(app);
        RegisterIncoming(app);

        app.Run($"http://localhost:{PORT}");
    }

    static void RegisterMiddleware(WebApplication app)
    {
        app.Use( async (ctx, next) =>
        {
            ctx.Request.EnableBuffering();

            using (var reader = new StreamReader( ctx.Request.Body, encoding: Encoding.UTF8, 
                       detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                Console.WriteLine("Odebrano request! ------------------------------------");
                Console.WriteLine(body);
                Console.WriteLine("------------------------------------");

                ctx.Request.Body.Position = 0;
            }
            await next(ctx);
        });
    }

    static void RegisterIncoming(WebApplication app)
    {
        app.MapPost("/load", async (SensorReadingRequest req) =>
        {
            var bytes = Convert.FromBase64String(req.Data);
            var text = Encoding.UTF8.GetString(bytes);
            
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

public record SensorReadingRequest(string Data);