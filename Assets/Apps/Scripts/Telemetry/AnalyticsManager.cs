using System.Collections;
using UnityEngine;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Gunakan script ini untuk mencatat telemetry dari action yang ada pada game
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance;
        
        private GameSession _gameSession;
        private SessionDataModel _sessionDataModel;
        
        private void Awake()
        {
            SetInstance();
        }
        
        private void SetInstance()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartSession(string player1ID, string player2ID)
        {
            _gameSession = new GameSession(startTime: Time.time);
            _sessionDataModel = new  SessionDataModel(_gameSession.SessionID, player1ID, player2ID);
        }

        public void EndSession()
        {
            string json = GetSessionJSON();
            _gameSession = null;
            _sessionDataModel = null;
        }

        public string GetSessionJSON()
        {
            if (_sessionDataModel == null)
            {
                return string.Empty;
            }

            return JsonUtility.ToJson(_sessionDataModel);
        }

        /// <summary>
        /// Function ini dipakai untuk mencatat data telemetry
        /// </summary>
        public void Record(StageID stageID, EventType eventType, float duration, PlayerID playerId, PositionDataModel playerPositionDataModel, PositionDataModel partnerPositionDataModel, bool isTrapped)
        {
            float timestamp = _gameSession.GetRelativeTimestamp(Time.time);
            _sessionDataModel.Add(new TelemetryDataModel(stageID, timestamp, eventType, duration, playerId, playerPositionDataModel, partnerPositionDataModel, isTrapped));
        }
    }
}
