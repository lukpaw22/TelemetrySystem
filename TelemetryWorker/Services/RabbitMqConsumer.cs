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
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "telemetry",
                durable: false,
                exclusive: false,
                autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: {json}");
                try
                {
                    var message = JsonSerializer.Deserialize<TelemetryMessage>(json);
                    Console.WriteLine(message);
                    if (message != null)
                    {
                        await _processor.ProcessAsync(message);
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    throw e;
                }
            };
            channel.BasicConsume(queue: "telemetry",
                                 autoAck: false,
                                 consumer: consumer);

            Console.ReadLine();
        }
    }
}