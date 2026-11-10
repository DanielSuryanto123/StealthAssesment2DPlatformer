using System;
using System.Collections.Generic;

namespace CoLab.Telemetry
{
    /// <summary>
    /// class container untuk disimpan dalam format JSON
    /// </summary>
    public class SessionDataModel
    {
        public string session_id;
        public string student_id_p1;
        public string student_id_p2;
        public List<TelemetryDataModel> raw_logs;

        public SessionDataModel(string sessionID, string studentIDP1, string studentIDP2)
        {
            this.student_id_p1 = studentIDP1;
            this.student_id_p2 = studentIDP2;
            this.raw_logs = new List<TelemetryDataModel>();
        }

        public void Add(TelemetryDataModel model)
        {
            raw_logs ??= new List<TelemetryDataModel>();

            raw_logs.Add(model);
        }
    }
}