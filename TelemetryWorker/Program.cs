using System;
using System.Collections.Generic;
using System.Text;
using TelemetryWorker.Services;
using TelemetryWorker.Validation;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<TelemetryValidator>();
builder.Services.AddSingleton<InfluxService>(_ =>
new InfluxService("http://localhost:8086", "TOKEN"));
builder.Services.AddSingleton<TelemetryProcessor>();
builder.Services.AddSingleton<RabbitMqConsumer>();

var app = builder.Build();

var consumer = app.Services.GetRequiredService<RabbitMqConsumer>();

app.Run();