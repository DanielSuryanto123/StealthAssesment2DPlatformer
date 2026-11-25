
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        // shortcut: provisional thresholds, replace once the research team sets N, X and the radius.
        [Header("Perspective Taking Thresholds")]
        [SerializeField] private float pingResponseWindow = 5f;
        [SerializeField] private float pingResponseDistance = 2f;
        [SerializeField] private float nearbyObjectRadius = 3f;

        private GameSession _gameSession;
        private SessionDataModel _sessionDataModel;

        private Transform _player1;
        private Transform _player2;

        // Stores each player's current trapped state.
        private bool _player1Trapped;
        private bool _player2Trapped;

        private struct PendingPing
        {
            public bool active;
            public float time;
            public Vector2 position;
            public float partnerDistance;
        }

        // Indexed by the PlayerID of the pinger; a new ping replaces the old window.
        private readonly PendingPing[] _pendingPings = new PendingPing[2];

        private void Awake()
        {
            SetInstance();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += ClearPendingPings;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= ClearPendingPings;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            for (int i = 0; i < _pendingPings.Length; i++)
            {
                PendingPing ping = _pendingPings[i];

                if (!IsOpen(ping))
                    continue;

                PlayerID responderId = Other((PlayerID)i);
                Transform responder = GetPlayer(responderId);

                if (responder != null &&
                    ping.partnerDistance - Vector2.Distance(responder.position, ping.position)
                        >= pingResponseDistance)
                {
                    LogPingResponse((PlayerID)i, responderId);
                }
            }
        }

        private void ClearPendingPings(Scene scene, LoadSceneMode mode)
        {
            System.Array.Clear(_pendingPings, 0, _pendingPings.Length);
        }

        private bool IsOpen(PendingPing ping)
        {
            return ping.active && Time.time - ping.time <= pingResponseWindow;
        }

        private void LogPingResponse(PlayerID pingerId, PlayerID responderId)
        {
            float responseTime = Time.time - _pendingPings[(int)pingerId].time;
            _pendingPings[(int)pingerId].active = false;

            LogEvent(responderId, EventType.PING_RESPONSE, false, responseTime);
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Duplikat dari scene lain masih menerima callback sebelum dihancurkan.
            // Key kosong berarti upload dimatikan (dipakai test): log tetap di memori.
            if (Instance != this || string.IsNullOrWhiteSpace(apiKey))
                return;

            // Restart memuat stage yang sama, jadi log semua percobaan tetap terkumpul.
            string currentStage = scene.name.StartsWith("Level")
                ? StageOf(scene.name).ToString()
                : null;

            FlushStages(currentStage);
            StartCoroutine(SendPending());
        }

        private void OnApplicationQuit()
        {
            // Stage yang belum selesai dikirim saat game dibuka lagi.
            FlushStages(null);
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
            System.Array.Clear(_pendingPings, 0, _pendingPings.Length);
        }

        public void EndSession()
        {
            _gameSession = null;
            _sessionDataModel = null;
            System.Array.Clear(_pendingPings, 0, _pendingPings.Length);

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
            float duration = 0f,
            Component target = null)
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

            PlayerID partnerId = Other(playerId);

            Transform partner = GetPlayer(partnerId);

            bool isPing =
                eventType == EventType.PING_HELP ||
                eventType == EventType.PING_HERE;

            if (target == null && (isPing || eventType == EventType.WAIT_PARTNER))
            {
                target = FindNearestTarget(actor.position);

                // A wait only belongs to a joint point the player is actually standing at.
                if (!isPing && target != null &&
                    Vector2.Distance(actor.position, target.transform.position) > nearbyObjectRadius)
                {
                    target = null;
                }
            }

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
                StageOf(SceneManager.GetActiveScene().name),
                eventType,
                loggedDuration,
                playerId,
                playerPos,
                partnerPos,
                finalIsTrapped,
                GetObjectId(target),
                target != null
                    ? Vector2.Distance(actor.position, target.transform.position)
                    : 0f
            );

            if (isPing && partner != null)
            {
                _pendingPings[(int)playerId] = new PendingPing
                {
                    active = true,
                    time = Time.time,
                    position = actor.position,
                    partnerDistance = Vector2.Distance(partner.position, actor.position)
                };
            }
            else if (IsInteraction(eventType) && target != null &&
                     IsOpen(_pendingPings[(int)partnerId]) &&
                     Vector2.Distance(target.transform.position, _pendingPings[(int)partnerId].position)
                         <= nearbyObjectRadius)
            {
                LogPingResponse(partnerId, playerId);
            }
        }

        public void LogEvent(
            Transform actor,
            EventType eventType,
            bool isTrapped = false,
            float duration = 0f,
            Component target = null)
        {
            if (TryGetPlayerID(actor, out PlayerID playerId))
            {
                LogEvent(
                    playerId,
                    eventType,
                    isTrapped,
                    duration,
                    target
                );
            }
        }

        private static PlayerID Other(PlayerID playerId)
        {
            return playerId == PlayerID.PLAYER_1
                ? PlayerID.PLAYER_2
                : PlayerID.PLAYER_1;
        }

        private static bool IsInteraction(EventType eventType)
        {
            return eventType == EventType.STEP_ON_BUTTON
                || eventType == EventType.PULL_LEVER
                || eventType == EventType.CALL_PLATFORM
                || eventType == EventType.RESCUE_ATTEMPT_EARLY;
        }

        private static Component FindNearestTarget(Vector2 position)
        {
            return FindObjectsByType<MonoBehaviour>()
                .Where(behaviour => behaviour is ITelemetryTarget)
                .OrderBy(behaviour => Vector2.Distance(position, behaviour.transform.position))
                .FirstOrDefault();
        }

        // Hierarchy path with sibling indices: stays unique when scene objects share a name.
        private static string GetObjectId(Component target)
        {
            if (target == null)
                return string.Empty;

            Transform node = target.transform;
            string path = $"{node.name}[{node.GetSiblingIndex()}]";

            while (node.parent != null)
            {
                node = node.parent;
                path = $"{node.name}[{node.GetSiblingIndex()}]/{path}";
            }

            return path;
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
            bool isTrapped,
            string objectId = "",
            float objectDistance = 0f)
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
                    isTrapped,
                    objectId,
                    objectDistance
                )
            );
        }

        // ==========================================
        // STAGE & DURATION HELPERS
        // ==========================================

        private static StageID StageOf(string sceneName)
        {
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
                || eventType == EventType.LEVEL_RESTART
                || eventType == EventType.PING_RESPONSE
                || eventType == EventType.SYNC_ACTION;
        }

        // ==========================================
        // SUPABASE UPLOAD
        // ==========================================

        private static string PendingPath =>
            Path.Combine(Application.persistentDataPath, "telemetry_pending");

        /// <summary>Satu baris per pemain per stage, kecuali stage yang masih dimainkan.</summary>
        public List<StageLogModel> BuildStageRows(string keepStageId)
        {
            var rows = new List<StageLogModel>();

            if (_sessionDataModel == null)
                return rows;

            var finishedStages = _sessionDataModel.raw_logs
                .Select(log => log.stage_id)
                .Where(stageId => stageId != keepStageId)
                .Distinct();

            foreach (string stageId in finishedStages)
            {
                foreach (PlayerID playerId in new[] { PlayerID.PLAYER_1, PlayerID.PLAYER_2 })
                {
                    rows.Add(new StageLogModel
                    {
                        session_id = _sessionDataModel.session_id,
                        student_id = playerId == PlayerID.PLAYER_1
                            ? _sessionDataModel.student_id_p1
                            : _sessionDataModel.student_id_p2,
                        player_id = playerId.ToString(),
                        stage_id = stageId,
                        raw_logs = _sessionDataModel.raw_logs
                            .Where(log => log.stage_id == stageId &&
                                          log.player_id == playerId.ToString())
                            .ToList()
                    });
                }
            }

            return rows;
        }

        // Tulis ke disk dulu; log baru dilepas dari memori setelah file tersimpan.
        private void FlushStages(string keepStageId)
        {
            foreach (StageLogModel row in BuildStageRows(keepStageId))
            {
                string path = Path.Combine(
                    PendingPath,
                    $"{row.session_id}_{row.stage_id}_{row.player_id}.json"
                );

                try
                {
                    Directory.CreateDirectory(PendingPath);
                    File.WriteAllText(path, JsonUtility.ToJson(row));

                    _sessionDataModel.raw_logs.RemoveAll(log =>
                        log.stage_id == row.stage_id &&
                        log.player_id == row.player_id);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("AnalyticsManager: Gagal menyimpan " + path + ": " + e.Message);
                }
            }
        }

        /// <summary>Mengirim semua stage yang tersisa di memori dan antrean di disk.</summary>
        public IEnumerator SendData()
        {
            FlushStages(null);
            return SendPending();
        }

        private IEnumerator SendPending()
        {
            if (!Directory.Exists(PendingPath))
                yield break;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError(
                    "AnalyticsManager: Supabase API key belum diisi."
                );

                yield break;
            }

            string url =
                projectURL + "/rest/v1/telemetry_stage_logs";

            foreach (string path in Directory.GetFiles(PendingPath, "*.json"))
            {
                // Bisa sudah dikirim oleh coroutine dari pergantian scene sebelumnya.
                if (!File.Exists(path))
                    continue;

                using (UnityWebRequest request =
                       new UnityWebRequest(url, "POST"))
                {
                    request.uploadHandler =
                        new UploadHandlerRaw(File.ReadAllBytes(path));

                    request.downloadHandler =
                        new DownloadHandlerBuffer();

                    request.timeout = 15;

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

                    // 409 = baris ini sudah ada di server.
                    if (request.result == UnityWebRequest.Result.Success ||
                        request.responseCode == 409)
                    {
                        File.Delete(path);
                        Debug.Log(
                            "SUKSES! Terkirim ke Supabase: " + Path.GetFileName(path)
                        );
                    }
                    else
                    {
                        // File dibiarkan; dicoba lagi pada pergantian scene berikutnya.
                        Debug.LogError(
                            "GAGAL mengirim " + Path.GetFileName(path) +
                            " (" + request.responseCode + "): " +
                            request.error + " " + request.downloadHandler.text
                        );
                    }
                }
            }
        }
    }
}
