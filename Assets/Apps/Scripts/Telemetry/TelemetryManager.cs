using UnityEngine;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Gunakan script ini untuk mencatat telemetry dari action yang ada pada game
    /// </summary>
    public class TelemetryManager : MonoBehaviour
    {
        public static TelemetryManager Instance;
        private TelemetryDataContainer _telemetryDataContainer;
        
        private void Awake()
        {
            SetInstance();
            _telemetryDataContainer =  new TelemetryDataContainer();
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

        /// <summary>
        /// Function ini dipakai untuk mencatat data telemetry
        /// </summary>
        public void Record(string timestamp, EventType eventType, PlayerID playerId, Position playerPosition, Position partnerPosition, bool isTrapped)
        {
            _telemetryDataContainer.Add(new TelemetryDataLog(timestamp, eventType, playerId, playerPosition, partnerPosition, isTrapped));
        }
    }
}
