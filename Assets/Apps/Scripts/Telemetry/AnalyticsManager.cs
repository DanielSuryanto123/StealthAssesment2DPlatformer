
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Records gameplay telemetry and sends session data to Supabase.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance;

        [Header("Supabase Configuration")]
        [SerializeField] private string projectURL =
            "https://wmljwcszcxdnockwfclr.supabase.co";

        [SerializeField] private string apiKey =
            "sb_publishable_uKnQtASk51qoTKR5mrUBZg_CUUqWMLi";

        private GameSession _gameSession;
        private SessionDataModel _sessionDataModel;

        private Transform _player1;
        private Transform _player2;

        // Stores each player's current trapped state.
        private bool _player1Trapped;
        private bool _player2Trapped;

        private void Awake()
        {
            SetInstance();
        }

        private void SetInstance()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        // ==========================================
        // SESSION MANAGEMENT
        // ==========================================

        public void StartSession(string player1ID, string player2ID)
        {
            _gameSession = new GameSession(startTime: Time.time);

            _sessionDataModel = new SessionDataModel(
                _gameSession.SessionID,
                player1ID,
                player2ID
            );

            _player1Trapped = false;
            _player2Trapped = false;
        }

        public void EndSession()
        {
            _gameSession = null;
            _sessionDataModel = null;

            _player1Trapped = false;
            _player2Trapped = false;
        }

        public string GetSessionJSON()
        {
            if (_sessionDataModel == null)
                return string.Empty;

            return JsonUtility.ToJson(_sessionDataModel);
        }

        // ==========================================
        // PLAYER MANAGEMENT
        // ==========================================

        public void SetPlayers(Transform p1, Transform p2)
        {
            _player1 = p1;
            _player2 = p2;
        }

        public static bool TryGetPlayerID(
            string playerTag,
            out PlayerID playerId)
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

        public static bool TryGetPlayerID(
            Transform actor,
            out PlayerID playerId)
        {
            if (actor != null &&
                TryGetPlayerID(actor.tag, out playerId))
            {
                return true;
            }

            playerId = PlayerID.PLAYER_1;
            return false;
        }

        private Transform GetPlayer(PlayerID playerId)
        {
            ref Transform cached = ref (
                playerId == PlayerID.PLAYER_1
                ? ref _player1
                : ref _player2
            );

            if (cached == null)
            {
                string tag = playerId == PlayerID.PLAYER_1
                    ? "Player1"
                    : "Player2";

                GameObject found =
                    GameObject.FindGameObjectWithTag(tag);

                cached = found != null ? found.transform : null;
            }

            return cached;
        }

        // ==========================================
        // TRAPPED STATE MANAGEMENT
        // ==========================================

        public void SetPlayerTrapped(
            PlayerID playerId,
            bool isTrapped)
        {
            if (playerId == PlayerID.PLAYER_1)
            {
                _player1Trapped = isTrapped;
            }
            else if (playerId == PlayerID.PLAYER_2)
            {
                _player2Trapped = isTrapped;
            }
        }

        public bool IsPlayerTrapped(PlayerID playerId)
        {
            if (playerId == PlayerID.PLAYER_1)
                return _player1Trapped;

            if (playerId == PlayerID.PLAYER_2)
                return _player2Trapped;

            return false;
        }

        // ==========================================
        // TELEMETRY LOGGING
        // ==========================================

        public void LogEvent(
            PlayerID playerId,
            EventType eventType,
            bool isTrapped = false,
            float duration = 0f)
        {
            Transform actor = GetPlayer(playerId);

            if (actor == null)
            {
                Debug.LogWarning(
                    $"AnalyticsManager: {playerId} tidak ditemukan di scene, " +
                    $"event {eventType} dilewati."
                );

                return;
            }

            PlayerID partnerId =
                playerId == PlayerID.PLAYER_1
                ? PlayerID.PLAYER_2
                : PlayerID.PLAYER_1;

            Transform partner = GetPlayer(partnerId);

            PositionDataModel playerPos =
                new PositionDataModel(
                    actor.position.x,
                    actor.position.y
                );

            PositionDataModel partnerPos = partner != null
                ? new PositionDataModel(
                    partner.position.x,
                    partner.position.y
                )
                : new PositionDataModel(0f, 0f);

            float loggedDuration = UsesDuration(eventType)
                ? Mathf.Max(0f, duration)
                : 0f;

            bool finalIsTrapped =
                isTrapped || IsPlayerTrapped(playerId);

            Record(
                GetCurrentStage(),
                eventType,
                loggedDuration,
                playerId,
                playerPos,
                partnerPos,
                finalIsTrapped
            );
        }

        public void LogEvent(
            Transform actor,
            EventType eventType,
            bool isTrapped = false,
            float duration = 0f)
        {
            if (TryGetPlayerID(actor, out PlayerID playerId))
            {
                LogEvent(
                    playerId,
                    eventType,
                    isTrapped,
                    duration
                );
            }
        }

        public void LogLeverPull(Transform actor)
        {
            LogEvent(actor, EventType.PULL_LEVER);
        }

        public void LogHazardEntered(
            Transform actor,
            bool isTrapped)
        {
            LogEvent(
                actor,
                EventType.HAZARD_ZONE_ENTERED,
                isTrapped
            );
        }

        // ==========================================
        // RECORD TELEMETRY DATA
        // ==========================================

        public void Record(
            StageID stageID,
            EventType eventType,
            float duration,
            PlayerID playerId,
            PositionDataModel playerPositionDataModel,
            PositionDataModel partnerPositionDataModel,
            bool isTrapped)
        {
            if (_gameSession == null ||
                _sessionDataModel == null)
            {
                Debug.LogWarning(
                    "AnalyticsManager: belum ada sesi aktif, " +
                    "membuat sesi otomatis dengan ID UNKNOWN. " +
                    "Mulai dari MainMenu untuk data riset yang valid."
                );

                StartSession("UNKNOWN_P1", "UNKNOWN_P2");
            }

            float timestamp =
                _gameSession.GetRelativeTimestamp(Time.time);

            float loggedDuration = UsesDuration(eventType)
                ? Mathf.Max(0f, duration)
                : 0f;

            _sessionDataModel.Add(
                new TelemetryDataModel(
                    stageID,
                    timestamp,
                    eventType,
                    loggedDuration,
                    playerId,
                    playerPositionDataModel,
                    partnerPositionDataModel,
                    isTrapped
                )
            );
        }

        // ==========================================
        // STAGE & DURATION HELPERS
        // ==========================================

        private static StageID GetCurrentStage()
        {
            string sceneName =
                SceneManager.GetActiveScene().name;

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

        // ==========================================
        // SUPABASE UPLOAD
        // ==========================================

        public IEnumerator SendData()
        {
            if (_sessionDataModel == null)
            {
                Debug.LogWarning(
                    "AnalyticsManager: Tidak ada data sesi untuk dikirim."
                );

                yield break;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError(
                    "AnalyticsManager: Supabase API key belum diisi."
                );

                yield break;
            }

            string url =
                projectURL + "/rest/v1/telemetry_sessions";

            string jsonData = GetSessionJSON();

            byte[] bodyRaw =
                Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request =
                   new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler =
                    new UploadHandlerRaw(bodyRaw);

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );

                request.SetRequestHeader(
                    "apikey",
                    apiKey
                );

                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + apiKey
                );

                request.SetRequestHeader(
                    "Prefer",
                    "return=minimal"
                );

                yield return request.SendWebRequest();

                if (request.result ==
                        UnityWebRequest.Result.Success)
                {
                    Debug.Log(
                        "SUKSES! Data telemetri berhasil dikirim ke Supabase."
                    );

                    EndSession();
                }
                else
                {
                    Debug.LogError(
                        "GAGAL mengirim data ke Supabase: " +
                        request.error
                    );

                    Debug.LogError(
                        "Status Code: " + request.responseCode
                    );

                    Debug.LogError(
                        "Pesan Error Server: " +
                        request.downloadHandler.text
                    );
                }
            }
        }
    }
}
