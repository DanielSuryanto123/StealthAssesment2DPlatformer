using System;

namespace CoLab.Telemetry
{
    /// <summary>
    /// class ini digunakan sebagai representasi/data model yntuk disimpan dalam bentuk JSON
    /// data didapat dari convert isi class TelemetryData, terutama eventType dan playerID dari enum ke string
    /// </summary>
    [Serializable]
    public class TelemetryDataModel
    {
        // Nama field sengaja snake_case mengikuti gdd.md 4.1 / 4.2 karena JsonUtility memakai nama field apa adanya.
        public string stage_id;
        /// <summary>Detik sejak sesi dimulai (float, resolusi sub-milidetik). Bukan integer milidetik.</summary>
        public float timestamp;
        public string event_type;
        public float duration;
        public string player_id;
        public PositionDataModel player_pos;
        public PositionDataModel partner_pos;
        public bool isTrapped;

        public TelemetryDataModel(StageID stageID, float timestamp, EventType eventType, float duration, PlayerID playerId, PositionDataModel playerPositionDataModel, PositionDataModel partnerPositionDataModel, bool isTrapped)
        {
            this.stage_id = stageID.ToString();
            this.timestamp = timestamp;
            this.event_type = eventType.ToString();
            this.duration =  duration;
            this.player_id = playerId.ToString();
            this.player_pos = playerPositionDataModel;
            this.partner_pos = partnerPositionDataModel;
            this.isTrapped = isTrapped;
        }
    }
}