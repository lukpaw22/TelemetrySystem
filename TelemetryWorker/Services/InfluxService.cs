using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TelemetryWorker.Models;

namespace TelemetryWorker.Services
{
    public class InfluxService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _bucket;
        private readonly string _org;

        public InfluxService(string url, string token, string bucket = "telemetry", string org = "my-org")
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");
            _url = url.TrimEnd('/');
            _bucket = bucket;
            _org = org;
        }

        public async Task WriteAsync(TelemetryMessage msg)
        {
            var line = FormatLineProtocol(msg);
            Console.WriteLine($"Sending: {line}");

            var content = new StringContent(line, Encoding.UTF8, "text/plain");

            var response = await _httpClient.PostAsync(
                $"{_url}/api/v2/write?bucket={_bucket}&org={_org}&precision=s",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {response.StatusCode} - {responseBody}");

            response.EnsureSuccessStatusCode();
        }

        private string FormatLineProtocol(TelemetryMessage msg)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); ;
            return $"temperature,room={msg.Room} value={msg.Temperature.ToString(CultureInfo.InvariantCulture)} {msg.Timestamp}";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}