using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManger : MonoBehaviourSingleton<GameManger>
{
    [Header("Player Color Setting")]
    [SerializeField] private Color _baseColor;
    [SerializeField] private float _hueOffsetPerPlayer;
    [Header("References")]
    [SerializeField] private LevelManager _activeLevel;
    [SerializeField] private CinemachineCamera _baseCamera;
    [SerializeField] private bool _changeLevelOnGameEnd;
    [SerializeField] private bool _autoRestart;
    
    [SerializeField] private List<LevelManager> _levels; 

    [SerializeField, ReadOnly] private List<PlayerController> _players = new(); // Players that are playing the game
    [SerializeField, ReadOnly]
    private Dictionary<PlayerController, bool> _playerStatus = new(); // Show the status of the player : true = alive / false = dead
    
    [SerializeField, ReadOnly]
    private List<PlayerController> _queuedPlayer = new(); // Players that want to join the game
    [SerializeField, ReadOnly]
    private List<PlayerController> _afkPlayers = new(); // Players that haven't played an input or had there device deconected.
    [SerializeField, ReadOnly]
    private List<PlayerController> _disabledPlayers = new(); // Players that got kicked for AFK or Device deconection.
    
    private Dictionary<PlayerInput, PlayerController> _playerInputControllerLink = new();// Link the player input to the controller
    
    private int _activePlayerCount;
    
    public bool isGameRunning = false;

    private void Awake()
    {
        PlayerDeviceStorage.OnPlayerConnected.AddListener(RegisterPlayer);
    }

    public void RegisterPlayer(PlayerInput player)
    {
        if (_playerInputControllerLink.ContainsKey(player)) return;
        
        PlayerController pc = player.GetComponent<PlayerController>();
        _playerInputControllerLink.Add(player, pc);
        _playerStatus.Add(pc, false);
        pc.onPlayerDeath.AddListener(PlayerDeath);
        
        Color.RGBToHSV(_baseColor,  out float h, out float s, out float v);
        float newH = h + (_hueOffsetPerPlayer / 255 * (_playerInputControllerLink.Count - 1));
        newH -= Mathf.Floor(newH);
        Debug.Log($"h: {newH}, s: {s}, v: {v}");
        Color color = Color.HSVToRGB( newH, s, v);
        pc.SetColor(color);
        
        if (isGameRunning)
        {
            _queuedPlayer.Add(pc);
        }
        else
        {
            _players.Add(pc);
            if (_players.Count >= 2)
            {
                DOTween.Sequence().AppendInterval(0.5f).OnComplete(() =>
                {
                    GlobalReset();
                    LevelStart();
                });
            }
            else if(_players.Count > 0)
            {
                Debug.Log($"{_players.Count} / 2 players to start the game");
                // prompt for other players to join
            }
        }
        
    }
    public void LevelStart()
    {
        foreach (PlayerController player in _queuedPlayer)
        {
            _players.Add(player);
            player.ResetPlayer();
            _playerStatus[player] = true;
        }
        _queuedPlayer.Clear();
        Debug.Log("Starting level");
        _activePlayerCount = _players.Count;
        
        isGameRunning = true;
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
        isGameRunning = false;
        GlobalReset();
        //ANIMATION
        if(_autoRestart && _players.Count >= 2) DOTween.Sequence().AppendInterval(0.5f).OnComplete(LevelStart);
        
    }

    public void GlobalReset()
    {
        _activeLevel.Stop();
        _baseCamera.Priority.Value = 2;
        _activeLevel.Reset();
        foreach (PlayerController player in _players)
        {
            player.ResetPlayer();
            _playerStatus[player] = true;
        }
        Debug.Log("Global Reset");
        _activePlayerCount = _players.Count;
    }
    
    public void SwitchLevel()
    {
        throw new NotImplementedException();
    }

    private void PlayerDeath(PlayerController player)
    {
        Debug.Log(_activePlayerCount);
        if (!_players.Contains(player)) return;
        if(_playerStatus[player])
        {
            Debug.Log("ggrrrr");
            _activePlayerCount--;
            _playerStatus[player] = false;
        }
        Debug.Log(_activePlayerCount);
        if (_activePlayerCount == 1)
        {
            PlayerController winner = null;
            foreach(PlayerController p in _players)
            {
                if (_playerStatus[p])
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
        }
    }
}