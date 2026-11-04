using System.Numerics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Vector2 = UnityEngine.Vector2;

public class PlayerHealthController : MonoBehaviour
{
    [ReadOnly] public float currentOverCharge;
    [Tooltip("The amount of knockback a player receives per 100 overchange")] 
    public float knockbackForcePer100;
    
    [HideInInspector] public UnityEvent onPlayerDeath;
    [HideInInspector] public UnityEvent onPlayerDamage;
    
    private Rigidbody2D _rb;
    private PlayerMovementController _playerMovementController;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMovementController = GetComponent<PlayerMovementController>();
    }

    public bool Damage(float damage, Vector2 attackPosition)
    {
        
        return true;
    }

    
    
    [Button]
    public void TestKnockback()
    {
        _playerMovementController.Knockback(knockbackForcePer100, new Vector2(1,1));
    }
}
