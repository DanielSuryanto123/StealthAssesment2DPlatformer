using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using EventType = CoLab.Telemetry.EventType;

namespace CoLab.Telemetry.EvidenceTests
{
    /// <summary>
    /// Plays the real level scenes (players teleported, input injected) and checks the session JSON.
    /// Gameplay scripts live in Assembly-CSharp, so they are reached through reflection.
    /// Nothing is uploaded (the API key is blanked) unless TELEMETRY_UPLOAD=1 enables the end-to-end test.
    /// </summary>
    public class TelemetryEvidenceTests
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private AnalyticsManager analytics;
        private GameObject p1;
        private GameObject p2;
        private readonly List<string> errors = new List<string>();

        [SetUp]
        public void SetUp()
        {
            errors.Clear();
            Application.logMessageReceived += CollectError;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= CollectError;

            if (analytics == null || string.IsNullOrEmpty(analytics.GetSessionJSON()))
                return;

            // Saved so the evidence can be inspected after the run.
            SessionDataModel session = Session();
            session.raw_logs = Logs();
            string folder = Path.Combine(Application.dataPath, "../Logs/evidence");
            Directory.CreateDirectory(folder);
            string name = string.Concat(TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));
            File.WriteAllText(Path.Combine(folder, name + ".json"), JsonUtility.ToJson(session, true));

            foreach (string error in errors.Distinct())
                TestContext.WriteLine("Console error: " + error);

            var stopped = new[]
            {
                "JUMP_SUCCESS", "JUMP_FAIL", "LEVEL_RESTART",
                "HAZARD_ZONE_ENTERED", "INVALID_INTERACT", "RELEASE_LEVER"
            };
            string[] found = session.raw_logs.Select(log => log.event_type).Where(stopped.Contains).ToArray();

            // Test rows must never reach the upload queue of a real run.
            foreach (string path in QueuedFiles())
                File.Delete(path);

            analytics.EndSession();
            Object.DestroyImmediate(analytics.gameObject);
            AnalyticsManager.Instance = null;

            Assert.IsEmpty(found, "Stopped events were logged: " + string.Join(", ", found));
        }

        // ---------- AR1, AA1, AA2 ----------

        [UnityTest]
        public IEnumerator Ping_LogsNearestObject_AndResponseOnlyWhenPartnerApproaches()
        {
            yield return Load("Level1");
            yield return MoveTo(p2, new Vector2(510f, 500f));

            yield return Ping(p1);
            TelemetryDataModel ping = Single(EventType.PING_HERE, "PLAYER_1");
            Assert.IsNotEmpty(ping.object_id);
            Assert.Greater(ping.object_distance, 0f);

            yield return MoveTo(p2, new Vector2(509f, 500f));
            Assert.AreEqual(0, Count(EventType.PING_RESPONSE), "1 unit is below the threshold");

            yield return new WaitForSeconds(0.2f);
            yield return MoveTo(p2, new Vector2(505f, 500f));
            TelemetryDataModel response = Single(EventType.PING_RESPONSE, "PLAYER_2");
            Assert.Greater(response.duration, 0.1f);

            yield return MoveTo(p2, new Vector2(501f, 500f));
            Assert.AreEqual(1, Count(EventType.PING_RESPONSE), "one response per ping");

            Set(analytics, "pingResponseWindow", 0.3f);
            yield return MoveTo(p2, new Vector2(520f, 500f));
            yield return Ping(p1);
            yield return new WaitForSeconds(0.5f);
            yield return MoveTo(p2, new Vector2(505f, 500f));
            Assert.AreEqual(1, Count(EventType.PING_RESPONSE), "ignored ping must stay ignored");
        }

        [UnityTest]
        public IEnumerator Ping_IsAnsweredByInteractingNearIt()
        {
            yield return Load("Level2");
            Set(analytics, "pingResponseDistance", 100000f); // movement alone cannot answer

            Component lever = All("LeverController").First();
            GameObject puller = AllowedPlayer(lever);
            GameObject pinger = Partner(puller);

            yield return MoveTo(pinger, lever.transform.position);
            yield return Ping(pinger);
            yield return MoveTo(pinger, new Vector2(500f, 500f));
            yield return MoveTo(puller, lever);
            yield return Interact(puller);

            Single(EventType.PULL_LEVER, Id(puller));
            Single(EventType.PING_RESPONSE, Id(puller));
        }

        // ---------- AA3 ----------

        [UnityTest]
        public IEnumerator Portal_LogsEnterExitAndWait(
            [Values("Level1", "Level2", "Level3", "Level4", "Level5", "Level6", "Level7", "Level8")] string level)
        {
            yield return Load(level);
            Component portal = All("EndPortal").First(); // most levels carry two on one object
            Component tracker = p1.GetComponent(GameType("PlayerActivityTracker"));
            Assert.IsNotNull(tracker, "PlayerActivityTracker not attached");
            Set(tracker, "waitDurationThreshold", 0.2f);

            yield return MoveTo(p1, portal);
            Assert.AreEqual(ObjectId(portal), Single(EventType.PORTAL_ENTER, "PLAYER_1").object_id);

            p2.GetComponent<InputInjector>().move = 1f;
            yield return new WaitForSeconds(0.5f);
            p2.GetComponent<InputInjector>().move = 0f;
            yield return Frames();
            TelemetryDataModel wait = Single(EventType.WAIT_PARTNER, "PLAYER_1");
            Assert.Greater(wait.duration, 0.2f);
            Assert.AreEqual(ObjectId(portal), wait.object_id);

            yield return MoveTo(p1, new Vector2(500f, 500f));
            Assert.AreEqual(ObjectId(portal), Single(EventType.PORTAL_EXIT, "PLAYER_1").object_id);
        }

        [UnityTest]
        public IEnumerator Portal_SceneChangeIsNotLoggedAsLeaving()
        {
            yield return Load("Level1");
            Component portal = All("EndPortal").First();

            yield return MoveTo(p1, portal);
            yield return SceneManager.LoadSceneAsync("Level1"); // restart while standing in the portal
            yield return Frames();
            Assert.AreEqual(0, Count(EventType.PORTAL_EXIT));

            yield return Prepare();
            portal = All("EndPortal").First();
            yield return MoveTo(p1, portal);
            yield return MoveTo(p2, portal);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual("Level2", SceneManager.GetActiveScene().name);
            Single(EventType.PORTAL_ENTER, "PLAYER_2");
            Assert.AreEqual(0, Count(EventType.PORTAL_EXIT));
        }

        // ---------- Levers ----------

        [UnityTest]
        public IEnumerator DoorLever_LogsPull([Values("Level2", "Level8")] string level)
        {
            yield return Load(level);
            Component[] levers = All("LeverController");
            Assert.IsNotEmpty(levers);

            foreach (Component lever in levers)
            {
                GameObject puller = AllowedPlayer(lever);
                yield return MoveTo(puller, lever);
                yield return Interact(puller);
                Assert.AreEqual(1, At(EventType.PULL_LEVER, lever, Id(puller)), ObjectId(lever));
                yield return MoveTo(puller, Park(puller));
            }
        }

        // ---------- AR3 ----------

        [UnityTest]
        public IEnumerator SyncSwitch_LogsFailureAndFollowerSuccess([Values("Level3", "Level8")] string level)
        {
            yield return Load(level);
            Component[] switches = All("Level3SyncSwitch");
            Assert.IsNotEmpty(switches);

            // Every switch: press then leave before the partner presses.
            foreach (Component button in switches)
            {
                GameObject player = Tagged((string)Get(button, "requiredPlayerTag"));
                yield return MoveTo(player, button);
                yield return Interact(player);
                Assert.AreEqual(1, At(EventType.STEP_ON_BUTTON, button, Id(player)), ObjectId(button));
                yield return MoveTo(player, Park(player));
                Assert.AreEqual(1, At(EventType.STEP_OFF_BUTTON, button, Id(player)), ObjectId(button));
                Assert.AreEqual(1, At(EventType.SYNC_FAIL, button, Id(player)), ObjectId(button));
            }

            Assert.AreEqual(0, Count(EventType.SYNC_ACTION), "presses that do not overlap are not a sync");

            Component red = switches.First(s => (string)Get(s, "requiredPlayerTag") == "Player1");
            Component blue = switches.First(s => (string)Get(s, "requiredPlayerTag") == "Player2");
            yield return MoveTo(p1, red);
            yield return Interact(p1);
            yield return new WaitForSeconds(0.3f);
            yield return MoveTo(p2, blue);
            yield return Interact(p2);

            TelemetryDataModel sync = Single(EventType.SYNC_ACTION, "PLAYER_2");
            Assert.Greater(sync.duration, 0.3f);
            Assert.AreEqual(ObjectId(blue), sync.object_id);

            int stepOffs = Count(EventType.STEP_OFF_BUTTON);
            yield return MoveTo(p1, Park(p1));
            yield return MoveTo(p2, Park(p2));
            Assert.AreEqual(stepOffs, Count(EventType.STEP_OFF_BUTTON), "leaving after success is not a failure");
        }

        [UnityTest]
        public IEnumerator SyncSwitch_SceneChangeIsNotLoggedAsFailure()
        {
            yield return Load("Level3");
            Component button = All("Level3SyncSwitch").First();
            GameObject player = Tagged((string)Get(button, "requiredPlayerTag"));
            yield return MoveTo(player, button);
            yield return Interact(player);

            yield return SceneManager.LoadSceneAsync("Level3");
            yield return Frames();

            Assert.AreEqual(0, Count(EventType.SYNC_FAIL));
            Assert.AreEqual(0, Count(EventType.STEP_OFF_BUTTON));
        }

        [UnityTest]
        public IEnumerator BlindSync_LogsFailureAndFollowerSuccess()
        {
            yield return Load("Level4");
            Component manager = All("BlindSyncManager").Single();
            var red = (Component)Get(manager, "redSwitch");
            var blue = (Component)Get(manager, "blueSwitch");
            Set(manager, "syncWindowSeconds", 0.6f);

            yield return MoveTo(p1, red);
            yield return Interact(p1);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual(1, At(EventType.STEP_ON_BUTTON, red, "PLAYER_1"));
            Assert.AreEqual(1, At(EventType.SYNC_FAIL, red, "PLAYER_1"));
            Assert.AreEqual(1, At(EventType.STEP_OFF_BUTTON, red, "PLAYER_1"));

            yield return Interact(p1);
            yield return new WaitForSeconds(0.2f);
            yield return MoveTo(p2, blue);
            yield return Interact(p2);

            TelemetryDataModel sync = Single(EventType.SYNC_ACTION, "PLAYER_2");
            Assert.Greater(sync.duration, 0.2f);
            Assert.AreEqual(ObjectId(blue), sync.object_id);
        }

        // ---------- AR2, AR4 ----------

        [UnityTest]
        public IEnumerator Pitfall_LogsEarlyAttemptThenRescueByRescuer()
        {
            yield return Load("Level5");
            Component pit = All("PitfallTrigger").First();
            Component lever = All("RescueLever").Single();
            Component platform = All("RescuePlatform").Single();

            yield return MoveTo(p2, pit);
            Single(EventType.RESCUE_START, "PLAYER_2");
            yield return MoveTo(p2, Park(p2));

            yield return MoveTo(p1, lever);
            yield return Interact(p1);
            Assert.AreEqual(1, At(EventType.RESCUE_ATTEMPT_EARLY, lever, "PLAYER_1"));
            Assert.AreEqual(0, Count(EventType.RESCUE_TIME));

            yield return MoveTo(p2, platform);
            yield return Interact(p1);
            TelemetryDataModel rescue = Single(EventType.RESCUE_TIME, "PLAYER_1");
            Assert.Greater(rescue.duration, 0f);
            Assert.AreEqual(1, At(EventType.PULL_LEVER, lever, "PLAYER_1"));
            Assert.AreEqual(1, Count(EventType.RESCUE_ATTEMPT_EARLY));
        }

        [UnityTest]
        public IEnumerator Deadzone_LogsRescue([Values("Level6", "Level8")] string level)
        {
            yield return Load(level);
            Component[] levers = All("Level6DeadzoneLever");
            Assert.IsNotEmpty(levers);

            foreach (Component lever in levers)
            {
                var deadzone = (Component)Get(lever, "targetDeadzone");
                Assert.IsNotNull(deadzone, ObjectId(lever) + " has no deadzone");
                GameObject victim = Tagged((string)Get(deadzone, "playerTag"));
                GameObject rescuer = AllowedPlayer(lever);
                Assert.AreNotEqual(victim, rescuer, ObjectId(lever) + ": only the trapped player may pull");
                int starts = Count(EventType.RESCUE_START, Id(victim));

                yield return MoveTo(victim, deadzone);
                Assert.AreEqual(starts + 1, Count(EventType.RESCUE_START, Id(victim)), ObjectId(deadzone));

                yield return new WaitForSeconds(0.1f);
                int times = Count(EventType.RESCUE_TIME, Id(rescuer));
                yield return MoveTo(rescuer, lever);
                yield return Interact(rescuer);
                Assert.AreEqual(times + 1, Count(EventType.RESCUE_TIME, Id(rescuer)), ObjectId(lever));
                Assert.AreEqual(1, At(EventType.PULL_LEVER, lever, Id(rescuer)), ObjectId(lever));

                yield return MoveTo(victim, Park(victim));
                yield return MoveTo(rescuer, Park(rescuer));
            }
        }

        // ---------- Platform and puzzle buttons ----------

        [UnityTest]
        public IEnumerator PlatformButton_LogsOneCallPerPress([Values("Level6", "Level7", "Level8")] string level)
        {
            yield return Load(level);
            Component[] buttons = All("Level6PlatformCallButton")
                .Concat(All("Level7PlatformCallButton"))
                .Where(b => (Object)Get(b, "targetPlatform") != null)
                .ToArray();
            Assert.IsNotEmpty(buttons);

            foreach (Component button in buttons)
            {
                GameObject player = AllowedPlayer(button);
                int before = At(EventType.CALL_PLATFORM, button, Id(player));
                yield return MoveTo(player, button);
                yield return Interact(player);
                Assert.AreEqual(before + 1, At(EventType.CALL_PLATFORM, button, Id(player)), ObjectId(button));
                yield return MoveTo(player, Park(player));
            }

            Assert.AreEqual(buttons.Length, Count(EventType.CALL_PLATFORM));
        }

        [UnityTest]
        public IEnumerator PuzzleButton_LogsStepOnForAllowedPlayer()
        {
            yield return Load("Level7");
            Component[] buttons = All("Level7Button");
            Assert.IsNotEmpty(buttons);

            foreach (Component button in buttons)
            {
                string allowed = Get(button, "allowedPlayer").ToString();
                GameObject player = allowed == "Player2" ? p2 : p1;
                yield return MoveTo(player, button);
                yield return Interact(player);
                Assert.AreEqual(1, At(EventType.STEP_ON_BUTTON, button, Id(player)), ObjectId(button));
                yield return MoveTo(player, Park(player));
            }

            Assert.AreEqual(buttons.Length, Count(EventType.STEP_ON_BUTTON));
        }

        // ---------- End to end ----------

        /// <summary>Writes real rows (students TEST_E2E_*) to Supabase, so it only runs on request.</summary>
        [UnityTest]
        public IEnumerator PlayAllLevels_UploadsToSupabase()
        {
            if (Environment.GetEnvironmentVariable("TELEMETRY_UPLOAD") != "1")
                Assert.Ignore("Set TELEMETRY_UPLOAD=1 to upload test rows to Supabase.");

            yield return Load("Level1", upload: true);

            for (int level = 1; level <= 8; level++)
            {
                Assert.AreEqual("Level" + level, SceneManager.GetActiveScene().name);
                yield return MoveTo(p2, new Vector2(510f, 500f));
                yield return Ping(p1);
                yield return MoveTo(p2, new Vector2(503f, 500f));

                Component portal = All("EndPortal").First();
                yield return MoveTo(p1, portal);
                yield return MoveTo(p2, portal);
                yield return new WaitForSeconds(0.5f);

                if (level < 8)
                    yield return Prepare();
            }

            Assert.AreEqual("Result", SceneManager.GetActiveScene().name);

            float deadline = Time.realtimeSinceStartup + 60f;
            while (QueuedFiles().Length > 0 && Time.realtimeSinceStartup < deadline)
                yield return null;

            TestContext.WriteLine("Uploaded session " + Session().session_id);
            Assert.IsEmpty(QueuedFiles(), "Not accepted by Supabase: " + string.Join(" | ", errors.Distinct()));
            Assert.IsEmpty(Session().raw_logs, "Some logs were never queued");
        }

        // ---------- Helpers ----------

        private IEnumerator Load(string level, bool upload = false)
        {
            if (AnalyticsManager.Instance == null)
                new GameObject("EvidenceAnalytics").AddComponent<AnalyticsManager>();

            analytics = AnalyticsManager.Instance;

            if (!upload)
                Set(analytics, "apiKey", string.Empty);

            LogAssert.ignoreFailingMessages = true; // unrelated scene errors are reported, not fatal

            yield return SceneManager.LoadSceneAsync(level);
            yield return Frames();
            yield return Prepare();
            analytics.StartSession(upload ? "TEST_E2E_P1" : "TEST_P1", upload ? "TEST_E2E_P2" : "TEST_P2");
        }

        private IEnumerator Prepare()
        {
            p1 = GameObject.FindGameObjectWithTag("Player1");
            p2 = GameObject.FindGameObjectWithTag("Player2");
            Assert.IsNotNull(p1, "Player1 missing");
            Assert.IsNotNull(p2, "Player2 missing");
            analytics.SetPlayers(p1.transform, p2.transform);

            foreach (GameObject player in new[] { p1, p2 })
            {
                // Frozen so gravity and depenetration cannot move a teleported player.
                var body = player.GetComponent<Rigidbody2D>();
                body.constraints = RigidbodyConstraints2D.FreezeAll;
                body.sleepMode = RigidbodySleepMode2D.NeverSleep;
                player.AddComponent<InputInjector>();
                yield return MoveTo(player, Park(player));
            }
        }

        private Vector2 Park(GameObject player) =>
            player == p1 ? new Vector2(500f, 500f) : new Vector2(600f, 500f);

        private static IEnumerator MoveTo(GameObject player, Component target)
        {
            var collider = target.GetComponent<Collider2D>();
            Vector2 center = collider != null ? (Vector2)collider.bounds.center : (Vector2)target.transform.position;
            Vector2 offset = player.GetComponentInChildren<Collider2D>().bounds.center - player.transform.position;
            return MoveTo(player, center - offset);
        }

        private static IEnumerator MoveTo(GameObject player, Vector2 position)
        {
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            player.GetComponent<Rigidbody2D>().position = position;
            Physics2D.SyncTransforms();

            for (int i = 0; i < 3; i++)
                yield return new WaitForFixedUpdate();

            yield return Frames();
        }

        private static IEnumerator Frames()
        {
            yield return null;
            yield return null;
        }

        private static IEnumerator Interact(GameObject player)
        {
            player.GetComponent<InputInjector>().interact = true;
            return Frames();
        }

        private static IEnumerator Ping(GameObject player)
        {
            Set(player.GetComponent(GameType("PlayerController")), "canPing", true);
            player.GetComponent<InputInjector>().ping = true;
            return Frames();
        }

        private GameObject Tagged(string tag) => tag == "Player1" ? p1 : p2;

        private GameObject Partner(GameObject player) => player == p1 ? p2 : p1;

        private GameObject AllowedPlayer(Component target) => (bool)Get(target, "player2Only") ? p2 : p1;

        private string Id(GameObject player) => player == p1 ? "PLAYER_1" : "PLAYER_2";

        private SessionDataModel Session() =>
            JsonUtility.FromJson<SessionDataModel>(analytics.GetSessionJSON());

        // A finished stage is moved from memory to the upload queue on disk.
        private string[] QueuedFiles()
        {
            string queue = Path.Combine(Application.persistentDataPath, "telemetry_pending");

            return Directory.Exists(queue)
                ? Directory.GetFiles(queue, Session().session_id + "_*.json")
                : new string[0];
        }

        private List<TelemetryDataModel> Logs() =>
            QueuedFiles()
                .SelectMany(path => JsonUtility.FromJson<StageLogModel>(File.ReadAllText(path)).raw_logs)
                .Concat(Session().raw_logs)
                .ToList();

        private int Count(EventType type, string player = null) =>
            Logs().Count(l => l.event_type == type.ToString() && (player == null || l.player_id == player));

        private int At(EventType type, Component target, string player) =>
            Logs().Count(l => l.event_type == type.ToString() && l.player_id == player
                              && l.object_id == ObjectId(target));

        private TelemetryDataModel Single(EventType type, string player)
        {
            TelemetryDataModel[] matches = Logs()
                .Where(l => l.event_type == type.ToString() && l.player_id == player)
                .ToArray();
            Assert.AreEqual(1, matches.Length, $"{type} by {player}. Log: " +
                string.Join(", ", Logs().Select(l => l.event_type + "/" + l.player_id)));
            return matches[0];
        }

        private static string ObjectId(Component target) =>
            (string)typeof(AnalyticsManager)
                .GetMethod("GetObjectId", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { target });

        private static Type GameType(string name) => Type.GetType(name + ", Assembly-CSharp", true);

        private static Component[] All(string name) =>
            Object.FindObjectsByType(GameType(name)).Cast<Component>()
                .OrderBy(c => ObjectId(c)).ToArray();

        private static object Get(object target, string field) =>
            target.GetType().GetField(field, Fields).GetValue(target);

        private static void Set(object target, string field, object value)
        {
            // Walk base types: private fields are not visible through a subclass.
            Type type = target.GetType();
            while (type.GetField(field, Fields) == null)
                type = type.BaseType;
            type.GetField(field, Fields).SetValue(target, value);
        }

        private void CollectError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
                errors.Add(message);
        }
    }

    /// <summary>Injects input after InputHandler (-100) has read the keyboard and before gameplay polls it.</summary>
    [DefaultExecutionOrder(-50)]
    public class InputInjector : MonoBehaviour
    {
        public bool interact;
        public bool ping;
        public float move;

        private void Update()
        {
            Component input = GetComponent("InputHandler");

            if (interact) Apply(input, "InteractPressed", true);
            if (ping) Apply(input, "PingPressed", true);
            if (move != 0f) Apply(input, "MoveInput", new Vector2(move, 0f));

            interact = ping = false;
        }

        private static void Apply(Component input, string property, object value) =>
            input.GetType().GetProperty(property).SetValue(input, value);
    }
}
