using Microsoft.AspNetCore.SignalR;

namespace SignalRApp.Hubs;

/// <summary>
/// SignalR Hub – punkt połączenia dla klientów przeglądarki.
/// Klienci łączą się pod: ws://localhost:5050/alertHub
/// Nasłuchują na zdarzenie: "ReceiveAlert"
/// </summary>
public class AlertHub : Hub
{
    private readonly ILogger<AlertHub> _logger;

    public AlertHub(ILogger<AlertHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("[HUB] Klient połączony: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[HUB] Klient rozłączony: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
