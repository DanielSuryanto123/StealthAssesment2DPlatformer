using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using CoLab.Telemetry;

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
            Assert.AreEqual("Player1", sessionModel.player1ID);
            Assert.AreEqual("Player2", sessionModel.player2ID);
            Assert.IsNotNull(sessionModel.telemetryDataLogs);
            Assert.AreEqual(0, sessionModel.telemetryDataLogs.Count);
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
            Assert.AreEqual(1, sessionModel.telemetryDataLogs.Count);

            var entry = sessionModel.telemetryDataLogs[0];
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

        private static T GetPrivateField<T>(object instance, string fieldName)
            where T : class
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(instance) as T;
        }
    }
}
