using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks the most recent checkpoint each player has activated, keyed by
/// player tag ("Player1" / "Player2"). PlayerRespawn asks this for where
/// to send a player back to after a hazard hit.
///
/// Setup:
/// - Put this on a persistent GameObject in the scene (e.g. "Managers").
/// - Set a default spawn position for each player in the Inspector for
///   the very start of the level, before any checkpoint is touched.
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [System.Serializable]
    public struct DefaultSpawn
    {
        public string playerTag;
        public Vector3 position;
    }

    [Tooltip("Starting respawn position for each player before any checkpoint is reached.")]
    [SerializeField] private List<DefaultSpawn> defaultSpawns = new List<DefaultSpawn>();

    private readonly Dictionary<string, Vector3> checkpoints = new Dictionary<string, Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var spawn in defaultSpawns)
        {
            checkpoints[spawn.playerTag] = spawn.position;
        }
    }

    public void SetCheckpoint(string playerTag, Vector3 position)
    {
        checkpoints[playerTag] = position;
    }

    public Vector3 GetRespawnPoint(string playerTag)
    {
        if (checkpoints.TryGetValue(playerTag, out Vector3 pos))
        {
            return pos;
        }

        Debug.LogWarning($"No checkpoint found for {playerTag}, respawning at world origin.");
        return Vector3.zero;
    }
}