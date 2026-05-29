using System.Collections.Generic;

namespace CoLab.Telemetry
{
    /// <summary>
    /// class container untuk disimpan dalam format JSON
    /// </summary>
    public class TelemetryDataContainer
    {
        public List<TelemetryDataLog> telemetryDataLogs;

        public TelemetryDataContainer()
        {
            this.telemetryDataLogs = new List<TelemetryDataLog>();
        }

        public void Add(TelemetryDataLog log)
        {
            telemetryDataLogs ??= new List<TelemetryDataLog>();

            telemetryDataLogs.Add(log);
        }
    }
}