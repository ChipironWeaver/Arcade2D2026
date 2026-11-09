using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

public class PlayerHealthController : MonoBehaviour
{
    [ReadOnly] public float currentOverCharge;
    [Tooltip("The amount of knockback a player receives per 100 overchange")] 
    [SerializeField] private float _knockbackForcePer100;
    
    [Header("IFrames")]
    [SerializeField] private float _damageIFrames;
    [SerializeField] private float _spawnIFrames;
    

    [Header("Test")] public float testDamage;
    
    public UnityEvent onPlayerDeath = new();
    public UnityEvent onPlayerDamage = new();
    public bool isFrozen = false;
    
    private PlayerMovementController _playerMovementController;

    private float _iFramesTimer;
    private bool _isInIFrames;

    public void Start()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        Reset();
    }

    public void Update()
    {
        if (_iFramesTimer > 0)
        {
            _iFramesTimer -= Time.deltaTime;
            if (_iFramesTimer <= 0)
            {
                _isInIFrames = false;
            }
        }
    }

    public void KillPlayer()
    {
        onPlayerDeath?.Invoke();
    }
    public bool Damage(float damage, Vector3 attackPosition, bool force = false)
    {
        if((_isInIFrames && !force) | isFrozen) return false;
        currentOverCharge += damage;
        Vector2 direction = transform.position - attackPosition;
        Debug.Log($"Damage: {currentOverCharge} / {direction}");
        float currentKnockback = 0;
        currentKnockback = _knockbackForcePer100 * Mathf.Sqrt(currentOverCharge / 100);
        _playerMovementController.Knockback(currentKnockback, direction);
        onPlayerDamage?.Invoke();
        GiveIFrames(_damageIFrames);
        return true;
    }
    public void GiveIFrames(float amount , bool force = false)
    {
        if (_iFramesTimer > 0 && !force) return;
        _iFramesTimer = amount;
        _isInIFrames = true;
    }

    public void Reset()
    {
        GiveIFrames(_spawnIFrames);
        currentOverCharge = 0;
        FreezePlayer(false);
    }

    public void FreezePlayer(bool freeze)
    {
        isFrozen = freeze;
        if (freeze)
        {
        }
        else
        {
            
        }
    }
    
    // TESTING
    
    
    private void TestDamage(InputAction.CallbackContext context)
    {
        if(context.performed) Damage(0, new Vector3(-1,-1,0) + transform.position);
    }

    [Button]
    private void ResetCharge()
    {
        currentOverCharge = 0;
    }
    [Button]
    private void DoDamage()
    {
        currentOverCharge += testDamage;
    }

    
}
