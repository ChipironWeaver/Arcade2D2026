using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class DamageSource : MonoBehaviour
{
    [HorizontalLine,Header("Header Player Damager"), Space(7)]
    [SerializeField] private bool _canDamagePlayer;
    [SerializeField,Tag,ShowIf("_canDamagePlayer")] 
    private string _playerTag = "Player";
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private List<GameObject> _objectBlacklist;
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private bool _useTargetXPosition;
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private bool _useTargetYPosition;
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private Vector2 _positionOffset;
    
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private bool _overrideXPosition;
    [SerializeField,ShowIf(EConditionOperator.And, "_canDamagePlayer" , "_overrideXPosition")] 
    private float _xPosition;
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private bool _overrideYPosition;
    [SerializeField,ShowIf(EConditionOperator.And, "_canDamagePlayer" , "_overrideYPosition")] 
    private float _yPosition;
    
    [SerializeField,ShowIf("_canDamagePlayer")] 
    private int _damageAmount;

    [HorizontalLine,Header("Environement"), Space(7)] 
    [SerializeField,Tag] private string _environementTag;
    
    [HorizontalLine,Header("Power Up"), Space(7)]
    [SerializeField,Tag] private string _powerUpTag;
    
    
    public UnityEvent onPlayerHit;
    public UnityEvent<PowerUp.PowerUp> onPowerUpHit = new();

    private List<GameObject> _hitObjects = new();

    public void OnEnable()
    {
        _hitObjects.Clear();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (_objectBlacklist.Contains(other.gameObject)) return;
        if(_hitObjects.Contains(other.gameObject)) return;
        if (other.CompareTag(_playerTag) && _canDamagePlayer)
        {
            if (other.TryGetComponent<PlayerHealthController>(out var player))
            {
                Vector2 position = transform.position;
                
                if(_useTargetXPosition) position.x = player.transform.position.x;
                if(_useTargetYPosition) position.y = player.transform.position.y;
                
                position += _positionOffset;
                
                if(_overrideXPosition) position.x = _xPosition;
                if(_overrideYPosition) position.y = _yPosition;
                
                player.Damage(_damageAmount,position);
                
                onPlayerHit.Invoke();
            }
        }
        else if (other.CompareTag(_powerUpTag))
        {
            if (other.TryGetComponent(out PowerUp.PowerUp powerUp))
            {
                onPowerUpHit.Invoke(powerUp);
            }
        }
    }
}
