using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class GameManger : MonoBehaviourSingleton<GameManger>
{
    [SerializeField] private LevelManager _activeLevel;
    [SerializeField] private CinemachineCamera _baseCamera;
    [SerializeField] private bool _changeLevelOnGameEnd;
    [SerializeField] private List<PlayerController> _players;
    [SerializeField] private List<LevelManager> _levels;

    [SerializeField, ReadOnly]
    private Dictionary<PlayerController, bool> _activePlayers;

    public void OnEnable()
    {
        foreach (PlayerController player in _players)
        {
            player.onPlayerDeath.AddListener(PlayerDeath);
        }
    }

    public void LevelStart()
    {
        for (int i  = 0; i < _players.Count; i++)
        {
            _players[i].RespawnPlayer(_activeLevel.GetSpawnPosition(i));
        }
        _activeLevel.StartLevel();
    }

    public void GameEnd()
    {
        if(_changeLevelOnGameEnd) SwitchLevel();
        GlobalReset();
        //ANIMATION
        LevelStart();
    }

    public void GlobalReset()
    {
        _baseCamera.Priority.Value = 2;
        _activeLevel.Reset();
        foreach (PlayerController player in _players)
        {
            player.ResetPlayer();
        }
    }
    
    public void SwitchLevel()
    {
        throw new NotImplementedException();
    }

    private void PlayerDeath(PlayerController player)
    {
        throw new NotImplementedException();
    }
}