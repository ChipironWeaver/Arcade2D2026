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
    
    [HideInInspector] public UnityEvent onPlayerDeath = new();
    [HideInInspector] public UnityEvent onPlayerDamage = new();
    
    private PlayerMovementController _playerMovementController;

    private float _iFramesTimer;
    private bool _isInIFrames;

    public void Start()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        GiveIFrames(_spawnIFrames);
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
        _playerMovementController.SwitchPlayerState(PlayerState.Frozen, true);
        onPlayerDeath?.Invoke();
    }
    public bool Damage(float damage, Vector3 attackPosition, bool force = false)
    {
        if(_isInIFrames && !force) return false;
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
    
    public void TestDamage(InputAction.CallbackContext context)
    {
        if(context.performed) Damage(0, new Vector3(-1,-1,0) + transform.position);
    }

    [Button]
    public void ResetCharge()
    {
        currentOverCharge = 0;
    }
    [Button]
    public void DoDamage()
    {
        currentOverCharge += testDamage;
    }
}
