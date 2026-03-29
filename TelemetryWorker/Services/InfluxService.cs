using System;
using System.Collections.Generic;
using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using TelemetryWorker.Models;

namespace TelemetryWorker.Services
{
    public class InfluxService
    {
        private readonly InfluxxDBClient _client;
        private readonly string _bucket = "telemetry";
        private readonly string _org = "my-org";

        public InfluxService(string url, string token)
        {
            _client = InfluxDBClientFactory.Create(url, token);
        }
        public async Task WriteAsync(TelemetryMessage msg)
        {
            var point = PointData
            .Measurment("temperature")
            .Tag("room", msg.Room)
            .Field("temperature")
            .Timestamp(msg.Timestamp, WritePrecision.Ns);

            using var writeApi = _client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _bucket,  _org);
        }
    }
}
