using System;

namespace CoLab.Telemetry
{
    /// <summary>
    /// class ini digunakan sebagai representasi/data model yntuk disimpan dalam bentuk JSON
    /// data didapat dari convert isi class TelemetryData, terutama eventType dan playerID dari enum ke string
    /// </summary>
    [Serializable]
    public class TelemetryDataLog
    {
        public string timestamp;
        public string eventType;
        public string playerId;
        public Position playerPosition;
        public Position partnerPosition;
        public bool isTrapped;

        public TelemetryDataLog(string timestamp, EventType eventType, PlayerID playerId, Position playerPosition, Position partnerPosition, bool isTrapped)
        {
            this.timestamp = timestamp;
            this.eventType = eventType.ToString();
            this.playerId = playerId.ToString();
            this.playerPosition = playerPosition;
            this.partnerPosition = partnerPosition;
            this.isTrapped = isTrapped;
        }
    }
}