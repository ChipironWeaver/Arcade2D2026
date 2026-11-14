using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerDeviceStorage : MonoBehaviour
{
    public static UnityEvent<PlayerInput> OnPlayerConnected = new();
    public static UnityEvent<PlayerInput> OnPlayerInputDisconnected = new();
    public static UnityEvent<PlayerInput> OnPlayerInputConnected = new();
    
    private List<PlayerInput> _playerInputs = new();
    private List<ReadOnlyArray<InputDevice>> _registeredPlayerInputs = new();
    
    
    public void PlayerJoined(PlayerInput player)
    {
        Debug.Log($"Player joined {player.gameObject.name}");
        Debug.Log($"Player joined {player.devices[0].name}");
        Debug.Log($"Player joined {player.devices.Count}");
        
        
        player.gameObject.name = $"Player [{player.devices[0].name}]";
        OnPlayerConnected?.Invoke(player);
        
        
        player.deviceLostEvent.AddListener(PlayerDisconnected);
        player.deviceRegainedEvent.AddListener(PlayerConnected);
        _playerInputs.Add(player);
        _registeredPlayerInputs.Add(player.devices);
    }

    public void PlayerDisconnected(PlayerInput player)
    {
        Debug.Log($"Player disconnected {player.gameObject.name}");
        OnPlayerInputDisconnected.Invoke(player);
    }

    public void PlayerConnected(PlayerInput player)
    {
        Debug.Log($"Player connected {player.gameObject.name}");
        OnPlayerInputConnected.Invoke(player);
    }
}
