using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

        private Transform _player1;
        private Transform _player2;

        private bool _player1Trapped;
        private bool _player2Trapped;
        private bool _subscribedToSceneLoaded;

        private void Awake()
        {
            SetInstance();
        }

        private void OnDestroy()
        {
            if (_subscribedToSceneLoaded)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _subscribedToSceneLoaded = false;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Status trapped tidak boleh terbawa ke level berikutnya.
            _player1Trapped = false;
            _player2Trapped = false;
        }

        /// <summary>
        /// Menandai pemain sedang terjebak (mis. di pitfall/deadzone) sampai di-rescue.
        /// LogEvent otomatis memakai status ini untuk field isTrapped.
        /// </summary>
        public void SetPlayerTrapped(PlayerID id, bool trapped)
        {
            if (id == PlayerID.PLAYER_1)
            {
                _player1Trapped = trapped;
            }
            else
            {
                _player2Trapped = trapped;
            }
        }

        public bool IsPlayerTrapped(PlayerID id)
        {
            return id == PlayerID.PLAYER_1 ? _player1Trapped : _player2Trapped;
        }

        private void SetInstance()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                _subscribedToSceneLoaded = true;
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
        /// Mengisi referensi pemain secara eksplisit. Kalau tidak dipanggil, pemain dicari lewat tag
        /// "Player1"/"Player2" secara otomatis.
        /// </summary>
        public void SetPlayers(Transform p1, Transform p2)
        {
            _player1 = p1;
            _player2 = p2;
        }

        /// <summary>
        /// Mapping tag "Player1"/"Player2" ke <see cref="PlayerID"/>.
        /// </summary>
        public static bool TryGetPlayerID(string playerTag, out PlayerID playerId)
        {
            switch (playerTag)
            {
                case "Player1":
                    playerId = PlayerID.PLAYER_1;
                    return true;
                case "Player2":
                    playerId = PlayerID.PLAYER_2;
                    return true;
                default:
                    playerId = PlayerID.PLAYER_1;
                    return false;
            }
        }

        public static bool TryGetPlayerID(Transform actor, out PlayerID playerId)
        {
            if (actor != null && TryGetPlayerID(actor.tag, out playerId))
            {
                return true;
            }

            playerId = PlayerID.PLAYER_1;
            return false;
        }

        /// <summary>
        /// Helper untuk kode gameplay: posisi pemain, posisi partner, dan stage diisi otomatis.
        /// Aturan GDD 4.2.4: duration hanya dipakai untuk WAIT_PARTNER, RESCUE_TIME, LEVEL_RESTART.
        /// </summary>
        public void LogEvent(PlayerID playerId, EventType eventType, bool isTrapped = false, float duration = 0f)
        {
            Transform actor = GetPlayer(playerId);
            if (actor == null)
            {
                Debug.LogWarning($"AnalyticsManager: {playerId} tidak ditemukan di scene, event {eventType} dilewati.");
                return;
            }

            PlayerID partnerId = playerId == PlayerID.PLAYER_1 ? PlayerID.PLAYER_2 : PlayerID.PLAYER_1;
            Transform partner = GetPlayer(partnerId);

            PositionDataModel playerPos = new PositionDataModel(actor.position.x, actor.position.y);
            PositionDataModel partnerPos = partner != null
                ? new PositionDataModel(partner.position.x, partner.position.y)
                : new PositionDataModel(0f, 0f);

            float loggedDuration = UsesDuration(eventType) ? Mathf.Max(0f, duration) : 0f;

            Record(GetCurrentStage(), eventType, loggedDuration, playerId, playerPos, partnerPos, isTrapped || IsPlayerTrapped(playerId));
        }

        public void LogEvent(Transform actor, EventType eventType, bool isTrapped = false, float duration = 0f)
        {
            if (TryGetPlayerID(actor, out PlayerID playerId))
            {
                LogEvent(playerId, eventType, isTrapped, duration);
            }
        }

        public void LogLeverPull(Transform actor)
        {
            LogEvent(actor, EventType.PULL_LEVER);
        }

        public void LogHazardEntered(Transform actor, bool isTrapped)
        {
            LogEvent(actor, EventType.HAZARD_ZONE_ENTERED, isTrapped);
        }

        /// <summary>
        /// Function ini dipakai untuk mencatat data telemetry
        /// </summary>
        public void Record(StageID stageID, EventType eventType, float duration, PlayerID playerId, PositionDataModel playerPositionDataModel, PositionDataModel partnerPositionDataModel, bool isTrapped)
        {
            if (_gameSession == null || _sessionDataModel == null)
            {
                // Terjadi kalau level dimainkan langsung dari editor tanpa lewat MainMenu (input ID pemain).
                Debug.LogWarning("AnalyticsManager: belum ada sesi aktif, membuat sesi otomatis dengan ID UNKNOWN. " +
                                 "Mulai dari MainMenu untuk data riset yang valid.");
                StartSession("UNKNOWN_P1", "UNKNOWN_P2");
            }

            float timestamp = _gameSession.GetRelativeTimestamp(Time.time);
            _sessionDataModel.Add(new TelemetryDataModel(stageID, timestamp, eventType, duration, playerId, playerPositionDataModel, partnerPositionDataModel, isTrapped));
        }

        private Transform GetPlayer(PlayerID playerId)
        {
            ref Transform cached = ref (playerId == PlayerID.PLAYER_1 ? ref _player1 : ref _player2);

            // `== null` (bukan `is null`) agar objek Unity yang sudah di-destroy saat pindah scene ikut terdeteksi.
            if (cached == null)
            {
                string tag = playerId == PlayerID.PLAYER_1 ? "Player1" : "Player2";
                GameObject found = GameObject.FindGameObjectWithTag(tag);
                cached = found != null ? found.transform : null;
            }

            return cached;
        }

        /// <summary>
        /// Stage diturunkan dari nama scene ("Level6" menjadi STAGE_6). Scene non-level jatuh ke STAGE_1.
        /// </summary>
        private static StageID GetCurrentStage()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int level = 0;
            foreach (char c in sceneName)
            {
                if (char.IsDigit(c))
                {
                    level = level * 10 + (c - '0');
                }
            }

            if (level >= 1 && level <= 8)
            {
                return (StageID)(level - 1);
            }

            return StageID.STAGE_1;
        }

        private static bool UsesDuration(EventType eventType)
        {
            return eventType == EventType.WAIT_PARTNER
                   || eventType == EventType.RESCUE_TIME
                   || eventType == EventType.LEVEL_RESTART;
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
