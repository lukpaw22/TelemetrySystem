using System;
using System.Collections.Generic;
using System.Text;
using TelemetryWorker.Models;
using TelemetryWorker.Validation;

namespace TelemetryWorker.Services
{
    public class TelemetryProcessor
    {
        private readonly TelemetryValidator _validator;
        private readonly InfluxService _influx;

        public TelemetryProcessor(TelemetryValidator validator, InfluxService inlux)
        {
            _validator = validator;
            _influx = inlux;
        }

        public async Task ProcessAsync(TelemetryMessage msg)
        {
            if (!_validator.IsValid(msg))
                throw new Exception("Validation failed");
            if (!_validator.ValidateHash(msg))
                throw new Exception("Invalid hash");
            await _influx.WriteAsync(msg);
        }
    }
}
