using System.Text.Json.Serialization;

namespace SignalRApp.Models;

/// <summary>
/// Model payloadu webhooka wysyłanego przez InfluxDB po wyzwoleniu alertu.
/// InfluxDB wysyła POST na skonfigurowany endpoint z tym JSON-em.
/// </summary>
public class InfluxAlertPayload
{
    [JsonPropertyName("_check_id")]
    public string CheckId { get; set; } = string.Empty;

    [JsonPropertyName("_check_name")]
    public string CheckName { get; set; } = string.Empty;

    [JsonPropertyName("_level")]
    public string Level { get; set; } = string.Empty;   // ok | info | warn | crit

    [JsonPropertyName("_message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("_time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("_source_measurement")]
    public string Measurement { get; set; } = string.Empty;

    // Dowolne tagi/pola z pomiaru – InfluxDB dołącza je dynamicznie
    [JsonExtensionData]
    public Dictionary<string, object>? ExtraFields { get; set; }
}

/// <summary>
/// Uproszczony model alertu wysyłanego do klientów przez SignalR.
/// </summary>
public class AlertNotification
{
    public string Id          { get; set; } = Guid.NewGuid().ToString();
    public string CheckName   { get; set; } = string.Empty;
    public string Level       { get; set; } = string.Empty;
    public string Message     { get; set; } = string.Empty;
    public string Time        { get; set; } = string.Empty;
    public string Measurement { get; set; } = string.Empty;
}
