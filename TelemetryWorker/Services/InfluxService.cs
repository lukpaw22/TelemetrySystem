using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using TelemetryWorker.Models;

namespace TelemetryWorker.Services
{
    public class InfluxService
    {
        private readonly InfluxDBClient _client;
        private readonly string _bucket = "telemetry";
        private readonly string _org = "my-org";

        public InfluxService(string url, string token)
        {
            // Nowy sposób tworzenia klienta (bez przestarzałego InfluxDBClientFactory)
            _client = new InfluxDBClient(url, token);
        }

        public async Task WriteAsync(TelemetryMessage msg)
        {
            var point = PointData
                .Measurement("temperature")
                .Tag("room", msg.Room)
                .Field("temperature", msg.Temperature)
                .Timestamp(msg.Timestamp, WritePrecision.Ns);

            // Bez "using" — WriteApiAsync nie implementuje IDisposable
            var writeApi = _client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _bucket, _org);
        }
    }
}
