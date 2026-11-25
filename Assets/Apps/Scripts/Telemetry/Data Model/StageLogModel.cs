using System.Collections.Generic;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Satu baris tabel telemetry_stage_logs: satu pemain pada satu stage.
    /// Nama field mengikuti nama kolom karena JsonUtility memakai nama field apa adanya.
    /// </summary>
    public class StageLogModel
    {
        public string session_id;
        public string student_id;
        public string player_id;
        public string stage_id;
        public List<TelemetryDataModel> raw_logs;
    }
}
