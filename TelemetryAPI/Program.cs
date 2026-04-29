using System.Text;
using RabbitMQ.Client;

internal class Program
{
    private const int    PORT       = 8080;
    private const string QUEUE_NAME = "telemetry";
    private const string DLX_NAME   = "telemetry-dlx";
    private const string DLQ_NAME   = "telemetry-dlq";

    private static IConnection? _rabbitConnection;
    private static IChannel?    _channel;

    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS — wymagane żeby sensor_app.html (port 5050) mogła wysyłać do API (port 8080)
        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        var app = builder.Build();
        app.UseCors();

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _rabbitConnection = await factory.CreateConnectionAsync();
        _channel          = await _rabbitConnection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(DLX_NAME, ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(DLQ_NAME, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(DLQ_NAME, DLX_NAME, QUEUE_NAME);

        await _channel.QueueDeclareAsync(
            queue: QUEUE_NAME,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange",    DLX_NAME   },
                { "x-dead-letter-routing-key", QUEUE_NAME }
            });

        Console.WriteLine($"[API] Połączono z RabbitMQ. Kolejka '{QUEUE_NAME}' gotowa.");

        RegisterMiddleware(app);
        RegisterIncoming(app);

        app.Run($"http://localhost:{PORT}");
    }

    static void RegisterMiddleware(WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            ctx.Request.EnableBuffering();
            using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8,
                                detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"[API] Odebrano request ──────────────────────");
            Console.WriteLine(body);
            Console.WriteLine($"────────────────────────────────────────────");
            ctx.Request.Body.Position = 0;
            await next(ctx);
        });
    }

    static void RegisterIncoming(WebApplication app)
    {
        app.MapPost("/load", async (SensorReadingRequest req) =>
        {
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(req.Data);
            }
            catch
            {
                return Results.BadRequest(new { Error = "Nieprawidłowe kodowanie Base64" });
            }

            var text = Encoding.UTF8.GetString(bytes);

            await _channel!.BasicPublishAsync(
                exchange:        "",
                routingKey:      QUEUE_NAME,
                mandatory:       true,
                basicProperties: new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent },
                body:            Encoding.UTF8.GetBytes(text)
            );

            Console.WriteLine($"[API] Wiadomość opublikowana do kolejki '{QUEUE_NAME}'.");
            return Results.Ok(new { Message = "Success", Status = 200 });
        });
    }
}

public record SensorReadingRequest(string Data);
