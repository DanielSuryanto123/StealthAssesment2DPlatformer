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
        public string stageID;
        public float timestamp;
        public string eventType;
        public float duration;
        public string playerId;
        public PositionDataModel playerPositionDataModel;
        public PositionDataModel partnerPositionDataModel;
        public bool isTrapped;

        public TelemetryDataModel(StageID stageID, float timestamp, EventType eventType, float duration, PlayerID playerId, PositionDataModel playerPositionDataModel, PositionDataModel partnerPositionDataModel, bool isTrapped)
        {
            this.stageID = stageID.ToString();
            this.timestamp = timestamp;
            this.eventType = eventType.ToString();
            this.duration =  duration;
            this.playerId = playerId.ToString();
            this.playerPositionDataModel = playerPositionDataModel;
            this.partnerPositionDataModel = partnerPositionDataModel;
            this.isTrapped = isTrapped;
        }
    }
}