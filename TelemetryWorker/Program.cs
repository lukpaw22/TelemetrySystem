using System;
using System.Collections.Generic;
using System.Text;
using TelemetryWorker.Models;
using TelemetryWorker.Services;
using TelemetryWorker.Validation;

Console.WriteLine("Telemetry Worker Starting...");

var validator = new TelemetryValidator();
var influxService = new InfluxService("http://localhost:8181", "");

var sampleMessage = new TelemetryMessage
{
    Room = "TestRoom",
    Timestamp = DateTime.UtcNow,
    Temperature = 25.5,
    Hash = "test"
};

await influxService.WriteAsync(sampleMessage);
Console.WriteLine("Sample data written to InfluxDB");

var processor = new TelemetryProcessor(validator, influxService);
var consumer = new RabbitMqConsumer(processor);

consumer.Start();

Console.WriteLine("Press Ctrl+C to exit");
Console.ReadLine();