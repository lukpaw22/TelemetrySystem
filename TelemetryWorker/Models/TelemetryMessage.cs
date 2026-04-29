using System;
using System.Collections.Generic;
using System.Text;


namespace TelemetryWorker.Models
{
    public class TelemetryMessage
    {
        public string Room { get; set; }
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public string Hash { get; set; }

    }
}
