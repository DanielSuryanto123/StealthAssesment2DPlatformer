using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Gunakan script ini untuk mencatat telemetry dari action yang ada pada game
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance;

        private string projectURL = "https://wmljwcszcxdnockwfclr.supabase.co";
        private string apiKey = "sb_publishable_uKnQtASk51qoTKR5mrUBZg_CUUqWMLi";
        
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

        public IEnumerator SendData()
        {
            string url = projectURL + "/rest/v1/telemetry_sessions";
            string jsonData = GetSessionJSON();
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Prefer", "return=minimal");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("GAGAL mengirim data ke Supabase: " + request.error);
                Debug.LogError("Pesan Error Server: " + request.downloadHandler.text);
            
                // Opsional: Buat logika UI untuk memberitahu mahasiswa agar jangan menutup game dulu 
                // dan berikan tombol "Coba Kirim Ulang" (Retry).
            }
            else
            {
                Debug.Log("SUKSES! Data telemetri berhasil diamankan di Supabase.");
            
                // Opsional: Bersihkan container agar siap untuk mahasiswa berikutnya
                EndSession();
            
                // Panggil fungsi untuk memunculkan Layar Game Over / Result Screen
                // ShowResultScreen(); 
            }
            
        }
    }
}
