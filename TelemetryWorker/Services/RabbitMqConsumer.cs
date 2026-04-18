using System;
using System.Collections.Generic;
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

        public RabbitMqConsumer(TelemetryProcessor processor)
        {
            _processor = processor;
        }

        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "telemetry",
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var message = JsonSerializer.Deserialize<TelemetryMessage>(json);
                    if (message != null)
                    {
                        await _processor.ProcessAsync(message);
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            channel.BasicConsume(queue: "telemetry",
                                 autoAck: false,
                                 consumer: consumer);
        }
    }
}