using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerHealthController _playerHealthController;
    [SerializeField] private PlayerMovementController _playerMovementController;
    public UnityEvent<PlayerController> onPlayerDeath = new();
    public UnityEvent<PlayerController> onPlayerDamage = new();
    public bool isAlive = true;

    public void OnEnable()
    {
        _playerHealthController.onPlayerDamage.AddListener(PlayerDamage);
        _playerHealthController.onPlayerDeath.AddListener(PlayerDeath);
    }

    public void ResetPlayer()
    {
        FreezePlayer(false);
        _playerHealthController.Reset();
        _playerMovementController.Reset();
    }

    public void RespawnPlayer(Vector3 position)
    {
        transform.position = position;
        ResetPlayer();
    }

    private void PlayerDeath()
    {
        onPlayerDeath.Invoke(this);
        Debug.Log($"Player {gameObject.name} is dead");
        FreezePlayer(true);
    }

    private void PlayerDamage()
    {
        onPlayerDamage.Invoke(this);
    }
    
    public void FreezePlayer(bool freeze)
    {
        _playerHealthController.FreezePlayer(freeze);
        _playerMovementController.FreezePlayer(freeze);
    }
    
}