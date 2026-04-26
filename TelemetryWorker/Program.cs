using System;
using System.Collections.Generic;
using System.Text;
using TelemetryWorker.Models;
using TelemetryWorker.Services;
using TelemetryWorker.Validation;

Console.WriteLine("Telemetry Worker Starting...");

var validator = new TelemetryValidator();
var influxService = new InfluxService("http://localhost:8086", "hn7oSytvk1sX1J7pUsb0BJ_z-F-_GKSW3QYJfnzm8I15RcPqMJcTvXIImIUs5WBvsjdoupz49EVpSCpjMQBDJQ==");

Console.WriteLine("Sample data written to InfluxDB");

var processor = new TelemetryProcessor(validator, influxService);
var consumer = new RabbitMqConsumer(processor);

consumer.Start();

Console.WriteLine("Press Ctrl+C to exit");
Console.ReadLine();