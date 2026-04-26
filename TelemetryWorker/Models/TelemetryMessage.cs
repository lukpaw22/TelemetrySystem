using System;
using System.Collections.Generic;
using System.Text;


namespace TelemetryWorker.Models
{
    public class TelemetryMessage
    {
        public required string Room { get; set; }
        public long Timestamp { get; set; }
        public double Temperature { get; set; }
        public required string Hash { get; set; }

    }
}
