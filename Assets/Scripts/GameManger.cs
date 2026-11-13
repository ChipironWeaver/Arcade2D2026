using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManger : MonoBehaviourSingleton<GameManger>
{
    [SerializeField] private LevelManager _activeLevel;
    [SerializeField] private CinemachineCamera _baseCamera;
    [SerializeField] private bool _changeLevelOnGameEnd;
    [SerializeField] private List<PlayerController> _players;
    [SerializeField] private List<LevelManager> _levels;

    [SerializeField, ReadOnly]
    private Dictionary<PlayerController, bool> _activePlayers;
    private int _activePlayerCount;

    // public void Start()
    // {
    //     foreach (PlayerController player in _players)
    //     {
    //         _activePlayers.Add(player, true);
    //     }
    //     _activePlayerCount = _players.Count;
    //     LevelStart();
    // }
    // public void OnEnable()
    // {
    //     foreach (PlayerController player in _players)
    //     {
    //         player.onPlayerDeath.AddListener(PlayerDeath);
    //     }
    // }

    public void PlayerJoined(PlayerInput player)
    {
        
    }

    public void PlayerLeft(PlayerInput player)
    {
        
    }
    
    public void LevelStart()
    {
        for (int i  = 0; i < _players.Count; i++)
        {
            _players[i].RespawnPlayer(_activeLevel.GetSpawnPosition(i));
        }
        _activeLevel.StartLevel();
        _baseCamera.Priority.Value = 0;
    }

    public void GameEnd()
    {
        if(_changeLevelOnGameEnd) SwitchLevel();
        GlobalReset();
        //ANIMATION
        DOTween.Sequence().AppendInterval(0.5f).OnComplete(LevelStart);

    }

    public void GlobalReset()
    {
        _activeLevel.Stop();
        _baseCamera.Priority.Value = 2;
        _activeLevel.Reset();
        foreach (PlayerController player in _players)
        {
            player.ResetPlayer();
        }
        foreach (PlayerController player in _players)
        {
            _activePlayers[player] = true;
        }
        _activePlayerCount = _players.Count;
    }
    
    public void SwitchLevel()
    {
        
    }

    private void PlayerDeath(PlayerController player)
    {
        if(_activePlayers[player])
        {
            _activePlayerCount--;
            _activePlayers[player] = false;
        }

        if (_activePlayerCount == 1)
        {
            PlayerController winner = null;
            foreach(PlayerController p in  _players)
            {
                if (_activePlayers[p])
                {
                    winner = p;
                    break;
                }
            }
            if (winner)
            {
                Debug.Log($"The winner is {winner.gameObject.name}");
                GameEnd();
            }
            else
            {
                Debug.Log("Please don't even show this message");
            }
        }
    }
}