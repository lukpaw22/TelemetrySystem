using System.Globalization;
using TelemetryWorker.Models;
using TelemetryWorker.Utils;

namespace TelemetryWorker.Validation
{
    public class TelemetryValidator
    {
        public bool IsValid(TelemetryMessage msg)
        {
            if (string.IsNullOrEmpty(msg.Room))
                return false;
            if (msg.Timestamp > DateTime.UtcNow.AddMinutes(5))
                return false;
            if (msg.Temperature < -100 || msg.Temperature > 200)
                return false;

            return true;
        }

        public bool ValidateHash(TelemetryMessage msg)
        {
            
            var temp = msg.Temperature.ToString(CultureInfo.InvariantCulture);
            var raw  = $"{msg.Room}{msg.Timestamp:o}{temp}";
            var computed = HashHelper.ComputeHash(raw);
            return computed == msg.Hash;
        }
    }
}
