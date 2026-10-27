using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetGroupController : MonoBehaviour
{
    [Header("Target Group")]
    [SerializeField] private CinemachineTargetGroup targetGroup;

    [Header("Player Weights")]
    [SerializeField] private float normalWeight = 1f;
    [SerializeField] private float focusedWeight = 8f;
    [SerializeField] private float targetRadius = 1f;

    private Transform[] players;
    private Transform focusedPlayer;

    private void Awake()
    {
        if (targetGroup == null)
            targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
    }

    private void Start()
    {
        players = GetActivePlayers().ToArray();

        if (targetGroup == null)
        {
            Debug.LogError("TargetGroupController: CinemachineTargetGroup is missing.");
            return;
        }

        if (players.Length == 0)
        {
            Debug.LogError("TargetGroupController: No PlayerInput objects found.");
            return;
        }

        foreach (Transform player in players)
        {
            if (player != null && targetGroup.FindMember(player) == -1)
                targetGroup.AddMember(player, normalWeight, targetRadius);
        }

        FocusOnBothPlayers();

        Debug.Log($"TargetGroupController found {players.Length} players.");
    }

    public void FocusOnPlayer(Transform player)
    {
        if (targetGroup == null || player == null)
            return;

        focusedPlayer = player;
        UpdateTargetWeights();
    }

    public void FocusOnBothPlayers()
    {
        focusedPlayer = null;
        UpdateTargetWeights();
    }

    private void UpdateTargetWeights()
    {
        if (targetGroup == null || players == null)
            return;

        foreach (Transform player in players)
        {
            if (player == null)
                continue;

            int index = targetGroup.FindMember(player);

            if (index < 0)
                continue;

            CinemachineTargetGroup.Target target = targetGroup.m_Targets[index];

            target.weight = focusedPlayer == null || player == focusedPlayer
                ? (focusedPlayer == null ? normalWeight : focusedWeight)
                : 0f;

            targetGroup.m_Targets[index] = target;
        }
    }

    public List<Transform> GetActivePlayers()
    {
        return FindObjectsByType<PlayerInput>()
            .Select(input => input.transform)
            .ToList();
    }
}