using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelemetryWorker.Models;

namespace TelemetryWorker.Services
{
    public class RabbitMqConsumer
    {
        private readonly TelemetryProcessor _processor;

        // Nazwy kolejek i exchange jako stałe — łatwo znaleźć i zmienić
        public const string MainQueue  = "telemetry";
        public const string DlqName    = "telemetry-dlq";
        public const string DlxName    = "telemetry-dlx";

        public RabbitMqConsumer(TelemetryProcessor processor)
        {
            _processor = processor;
        }

        public async Task StartAsync()
        {
            var factory    = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel    = await connection.CreateChannelAsync();

            // ── Krok 1: Dead-Letter Exchange ────────────────────────────────
            // Exchange, na który trafią wiadomości odrzucone przez NACK
            await channel.ExchangeDeclareAsync(
                exchange: DlxName,
                type: ExchangeType.Direct,
                durable: true);

            // ── Krok 2: Dead-Letter Queue ───────────────────────────────────
            // Tu lądują wszystkie błędne / nieprzeszłe walidacji wiadomości
            await channel.QueueDeclareAsync(
                queue: DlqName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Powiąż DLQ z DLX (routing key = nazwa kolejki głównej)
            await channel.QueueBindAsync(
                queue: DlqName,
                exchange: DlxName,
                routingKey: MainQueue);

            // ── Krok 3: Kolejka główna z DLQ ───────────────────────────────
            // x-dead-letter-exchange: dokąd trafia NACK-owana wiadomość
            // x-dead-letter-routing-key: jaki routing key dostaje po odrzuceniu
            await channel.QueueDeclareAsync(
                queue: MainQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange",    DlxName   },
                    { "x-dead-letter-routing-key", MainQueue }
                });

            // ── Krok 4: Konsument ───────────────────────────────────────────
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var message = JsonSerializer.Deserialize<TelemetryMessage>(json);
                    await _processor.ProcessAsync(message!);

                    // Potwierdzenie poprawnego przetworzenia
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    Console.WriteLine($"[Worker] ✓ Przetworzono: {message!.Room} {message.Temperature}°C");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Worker] ✗ Błąd: {ex.Message} — wiadomość trafia do DLQ");
                    // requeue: false → wiadomość idzie do DLQ (nie wraca do głównej kolejki)
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                queue: MainQueue,
                autoAck: false,
                consumer: consumer);

            Console.WriteLine($"[Worker] Nasłuchiwanie na '{MainQueue}' | DLQ: '{DlqName}'");
        }
    }
}
