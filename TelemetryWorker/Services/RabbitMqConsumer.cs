using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelemetryWorker.Models;

namespace TelemetryWorker.Services
{
    public class RabbitMqConsumer : IAsyncDisposable
    {
        private IConnection? _connection;
        private IChannel? _channel;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "telemetry",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    Console.WriteLine($"Odebrano: {message}");
                }
                catch
                {

                }
            };

        }
    }
}
