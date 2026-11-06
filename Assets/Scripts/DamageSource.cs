using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DamageSource : MonoBehaviour
{
    [SerializeField,Tag] private string _playerTag = "Player";
    [SerializeField] private List<GameObject> _objectBlacklist;
    
    [SerializeField] private bool _useTargetXPosition;
    [SerializeField] private bool _useTargetYPosition;
    
    [SerializeField] private Vector2 _positionOffset;
    
    [SerializeField] private bool _overrideXPosition;
    [SerializeField,ShowIf("_overrideXPosition")] private float _xPosition;
    [SerializeField] private bool _overrideYPosition;
    [SerializeField,ShowIf("_overrideYPosition")] private float _yPosition;
    
    [SerializeField] private int _damageAmount;
    
    public UnityEvent onTargetHit;

    void OnTriggerStay2D(Collider2D other)
    {
        if (_objectBlacklist.Contains(other.gameObject)) return;
        if (other.CompareTag(_playerTag))
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
                
                onTargetHit.Invoke();
            }
        }
    }
}
