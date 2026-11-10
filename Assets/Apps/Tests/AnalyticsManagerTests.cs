using System.Collections;
using System.Reflection;
using CoLab.Telemetry;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoLab.Telemetry.Tests
{
    public class AnalyticsManagerTests
    {
        private GameObject _gameObject;
        private AnalyticsManager _analyticsManager;

        [SetUp]
        public void SetUp()
        {
            AnalyticsManager.Instance = null;
            _gameObject = new GameObject("AnalyticsManagerTests");
            _analyticsManager = _gameObject.AddComponent<AnalyticsManager>();
        }

        [TearDown]
        public void TearDown()
        {
            AnalyticsManager.Instance = null;
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void StartSession_CreatesSessionDataModel()
        {
            _analyticsManager.StartSession("Player1", "Player2");

            var sessionModel = GetPrivateField<SessionDataModel>(_analyticsManager, "_sessionDataModel");

            Assert.IsNotNull(sessionModel);
            Assert.AreEqual("Player1", sessionModel.student_id_p1);
            Assert.AreEqual("Player2", sessionModel.student_id_p2);
            Assert.IsNotNull(sessionModel.raw_logs);
            Assert.AreEqual(0, sessionModel.raw_logs.Count);
        }

        [Test]
        public void Record_AddsTelemetryEntryToSessionDataModel()
        {
            _analyticsManager.StartSession("Player1", "Player2");

            var playerPosition = new PositionDataModel(1.5f, -2.5f);
            var partnerPosition = new PositionDataModel(0.0f, 3.0f);

            _analyticsManager.Record(
                StageID.STAGE_1,
                EventType.MOVE_START,
                0.75f,
                PlayerID.PLAYER_1,
                playerPosition,
                partnerPosition,
                true);

            var sessionModel = GetPrivateField<SessionDataModel>(_analyticsManager, "_sessionDataModel");
            Assert.IsNotNull(sessionModel);
            Assert.AreEqual(1, sessionModel.raw_logs.Count);

            var entry = sessionModel.raw_logs[0];
            Assert.AreEqual(StageID.STAGE_1.ToString(), entry.stageID);
            Assert.AreEqual(EventType.MOVE_START.ToString(), entry.eventType);
            Assert.AreEqual(PlayerID.PLAYER_1.ToString(), entry.playerId);
            Assert.AreEqual(0.75f, entry.duration);
            Assert.AreEqual(true, entry.isTrapped);
            Assert.AreEqual(playerPosition.x, entry.playerPositionDataModel.x);
            Assert.AreEqual(playerPosition.y, entry.playerPositionDataModel.y);
            Assert.AreEqual(partnerPosition.x, entry.partnerPositionDataModel.x);
            Assert.AreEqual(partnerPosition.y, entry.partnerPositionDataModel.y);
            Assert.GreaterOrEqual(entry.timestamp, 0f);
        }

        [Test]
        public void GetSessionJSON_ReturnsSerializedSessionData()
        {
            _analyticsManager.StartSession("Player1", "Player2");
            _analyticsManager.Record(
                StageID.STAGE_1,
                EventType.MOVE_START,
                0.75f,
                PlayerID.PLAYER_1,
                new PositionDataModel(1.5f, -2.5f),
                new PositionDataModel(0f, 3f),
                true);

            string json = _analyticsManager.GetSessionJSON();

            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);
            Assert.That(json, Does.Contain("Player1"));
            Assert.That(json, Does.Contain("Player2"));
            Assert.That(json, Does.Contain(StageID.STAGE_1.ToString()));
            Assert.That(json, Does.Contain(EventType.MOVE_START.ToString()));
            Assert.That(json, Does.Contain(PlayerID.PLAYER_1.ToString()));
        }

        [Test]
        public void EndSession_ClearsInternalState()
        {
            _analyticsManager.StartSession("Player1", "Player2");
            _analyticsManager.EndSession();

            Assert.IsNull(GetPrivateField<GameSession>(_analyticsManager, "_gameSession"));
            Assert.IsNull(GetPrivateField<SessionDataModel>(_analyticsManager, "_sessionDataModel"));
        }

        [Test]
        public void GetSessionJSON_ReturnsEmptyString_WhenNoSessionActive()
        {
            string json = _analyticsManager.GetSessionJSON();
            Assert.AreEqual(string.Empty, json);
        }

        [UnityTest]
        public IEnumerator SendData_SuccessfullyPushesData()
        {
            // 1. Setup Sesi dan Data Telemetri
            _analyticsManager.StartSession("TestPlayer1", "TestPlayer2");
            _analyticsManager.Record(
                StageID.STAGE_1,
                EventType.MOVE_START,
                1.0f,
                PlayerID.PLAYER_1,
                new PositionDataModel(0, 0),
                new PositionDataModel(0, 0),
                false
            );
            // 2. Jalankan Coroutine SendData dan tunggu hingga selesai
            yield return _analyticsManager.SendData();
            // 3. Verifikasi Efek Samping: 
            // Jika sukses, SendData() akan memanggil EndSession() di blok 'else',
            // yang berarti _sessionDataModel akan diubah kembali menjadi null.
            var sessionModel = GetPrivateField<SessionDataModel>(_analyticsManager, "_sessionDataModel");
            Assert.IsNull(sessionModel, "Data model harus dibersihkan (null) setelah pengiriman sukses.");
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
            where T : class
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(instance) as T;
        }
    }

    public class GameSessionTests
    {
        [Test]
        public void Constructor_InitializesSessionIDAndStartTime()
        {
            float startTime = 10.0f;
            var session = new GameSession(startTime);

            Assert.IsNotNull(session.SessionID);
            Assert.IsTrue(session.SessionID.StartsWith("ses"));
            Assert.AreEqual(17, session.SessionID.Length); // "ses" + "yyyyMMddHHmmss" = 17 karakter
        }

        [Test]
        public void GetRelativeTimestamp_ReturnsCorrectRelativeTime()
        {
            float startTime = 15.0f;
            var session = new GameSession(startTime);

            Assert.AreEqual(0.0f, session.GetRelativeTimestamp(15.0f));
            Assert.AreEqual(10.0f, session.GetRelativeTimestamp(25.0f));
            Assert.AreEqual(-5.0f, session.GetRelativeTimestamp(10.0f));
        }
    }
}
