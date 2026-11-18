using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerHealthController _playerHealthController;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private List<SpriteRenderer> _coloredSprites;
    public UnityEvent<PlayerController> onPlayerDeath = new();
    public UnityEvent<PlayerController> onPlayerDamage = new();
    public bool isAlive = true;

    public void Start()
    {
        _playerHealthController.onPlayerDamage.AddListener(PlayerDamage);
        _playerHealthController.onPlayerDeath.AddListener(PlayerDeath);
        FreezePlayer(true);
    }
    
    public void ResetPlayer()
    {
        FreezePlayer(false);
        isAlive = true;
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
        isAlive = false;
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
    
    public void SetColor(Color color)
    {
        foreach (SpriteRenderer sprite in _coloredSprites)
        {
            sprite.color = color;
        }
    }
}