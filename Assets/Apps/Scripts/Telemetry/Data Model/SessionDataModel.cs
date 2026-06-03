using System;
using System.Collections.Generic;

namespace CoLab.Telemetry
{
    /// <summary>
    /// class container untuk disimpan dalam format JSON
    /// </summary>
    public class SessionDataModel
    {
        public string sessionID;
        public string player1ID;
        public string player2ID;
        public List<TelemetryDataModel> telemetryDataLogs;

        public SessionDataModel(string sessionID, string player1ID, string player2ID)
        {
            this.player1ID = player1ID;
            this.player2ID = player2ID;
            this.telemetryDataLogs = new List<TelemetryDataModel>();
        }

        public void Add(TelemetryDataModel model)
        {
            telemetryDataLogs ??= new List<TelemetryDataModel>();

            telemetryDataLogs.Add(model);
        }
    }
}