using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace TelemetryWorker.Utils
{
    public static class HashHelper
    {
        public static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
