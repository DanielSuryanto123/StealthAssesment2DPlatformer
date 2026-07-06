using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetGroupController : MonoBehaviour
{
    private Transform[] _players;
    private CinemachineTargetGroup _targetGroup;

    private void Awake()
    {
        _targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
    }

    private void Start()
    {
        _players = GetActivePlayers().ToArray();

        AssignPlayersToTarget();
        Debug.Log(_players.Length);
    }

    private void AssignPlayersToTarget()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            _targetGroup.AddMember(_players[i], 1, 1);
            // _targetGroup.m_Targets[i].target = _players[i];
        }
    }

    public List<Transform> GetActivePlayers()
    {
        var playerInputs = FindObjectsByType<PlayerInput>();
        List<Transform> playersTransform = new List<Transform>();
        
        foreach (PlayerInput input in playerInputs)
        {
            playersTransform.Add(input.transform);
        }

        return playersTransform;
    }
}
