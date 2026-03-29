using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRApp.Hubs;
using SignalRApp.Models;

namespace SignalRApp.Controllers;

/// <summary>
/// Kontroler przyjmujący webhooki z InfluxDB.
///
/// InfluxDB wysyła POST na: http://localhost:5050/api/webhook/influx
/// po wyzwoleniu reguły alertowej.
///
/// Endpoint odbiera payload, mapuje go na AlertNotification
/// i rozsyła do wszystkich połączonych klientów przez SignalR.
/// </summary>
[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<AlertHub> _hubContext;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IHubContext<AlertHub> hubContext,
                              ILogger<WebhookController> logger)
    {
        _hubContext = hubContext;
        _logger     = logger;
    }

    /// <summary>
    /// Odbiera webhook z InfluxDB i rozgłasza alert do klientów SignalR.
    /// POST /api/webhook/influx
    /// </summary>
    [HttpPost("influx")]
    public async Task<IActionResult> ReceiveInfluxAlert([FromBody] InfluxAlertPayload payload)
    {
        _logger.LogInformation("[WEBHOOK] Odebrano alert z InfluxDB: {CheckName} poziom={Level}",
                                payload.CheckName, payload.Level);

        // Mapowanie payloadu InfluxDB → model wysyłany do UI
        var notification = new AlertNotification
        {
            CheckName   = payload.CheckName,
            Level       = payload.Level,
            Message     = payload.Message,
            Time        = string.IsNullOrEmpty(payload.Time)
                              ? DateTime.UtcNow.ToString("o")
                              : payload.Time,
            Measurement = payload.Measurement
        };

        // Rozgłoszenie do WSZYSTKICH połączonych klientów
        await _hubContext.Clients.All.SendAsync("ReceiveAlert", notification);

        _logger.LogInformation("[WEBHOOK] Alert rozgłoszony do klientów SignalR.");

        return Ok(new { status = "received" });
    }

    /// <summary>
    /// Endpoint testowy – pozwala ręcznie wysłać alert bez InfluxDB.
    /// POST /api/webhook/test
    /// Body: { "message": "Test alertu", "level": "crit" }
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTestAlert([FromBody] TestAlertRequest request)
    {
        var notification = new AlertNotification
        {
            CheckName   = "TEST",
            Level       = request.Level ?? "warn",
            Message     = request.Message ?? "Testowy alert",
            Time        = DateTime.UtcNow.ToString("o"),
            Measurement = "test"
        };

        await _hubContext.Clients.All.SendAsync("ReceiveAlert", notification);

        _logger.LogInformation("[WEBHOOK] Wysłano testowy alert: {Message}", notification.Message);

        return Ok(new { status = "sent", notification });
    }
}

public class TestAlertRequest
{
    public string? Message { get; set; }
    public string? Level   { get; set; }
}
