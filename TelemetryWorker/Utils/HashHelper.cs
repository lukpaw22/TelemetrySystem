using System.Security.Cryptography;
using System.Text;

namespace TelemetryWorker.Utils
{
    public static class HashHelper
    {
        public const string DefaultKey = "telemetry-secret-key";

        public static string ComputeHash(string input, string key = DefaultKey)
        {
            var keyBytes   = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(inputBytes);
            return Convert.ToBase64String(hash);
        }
    }
}
