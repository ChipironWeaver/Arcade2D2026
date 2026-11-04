using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField,Range(0,1)] private float _groundFriction;
    [SerializeField,Range(0,1)] private float _airFriction;
    [SerializeField,Range(0,1)] private float _xKnockbackDecayMultiplier;
    [SerializeField,Range(0,1)] private float _yKnockbackDecayMultiplier;
    [SerializeField] private Vector2 _knockbackMultiplier;
    
    [Header("Ground Check Parameter")]
    [SerializeField] private LayerMask _raycastGroundLayer;
    [SerializeField] private Vector2 _raycastGroundSize;
    [SerializeField] private float _raycastGroundDistance;
    
    [Header("Dash")]
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashTime;
    [SerializeField] private int _dashAmount;
    [SerializeField,CurveRange(0,0,1,1,EColor.Pink)] private AnimationCurve _dashCurve;

    [Header("Kayote and Input Buffer")] 
    [SerializeField] private float _inputBufferTime;
    
    
    [Header("Debug")]
    [SerializeField] private bool _debugUpdateDebugText;
    [SerializeField] private TextMeshProUGUI _debugIsGroundedText;
    [SerializeField] private TextMeshProUGUI _debugIsInputBufferingText;
    [SerializeField] private TextMeshProUGUI _debugInBufferTimerText;
    [SerializeField] private TextMeshProUGUI _debugVelocityText;
    [SerializeField] private TextMeshProUGUI _debugPlayerStateText;
    [SerializeField] private TextMeshProUGUI _debugDashAmountText;
    [SerializeField] private TextMeshProUGUI _debugDashTimeText;
    [SerializeField] private TextMeshProUGUI _debugKnockbackText;
    [SerializeField] private TextMeshProUGUI _debugRandom;

    [ReadOnly] public bool isGrounded;
    [ReadOnly] public PlayerState playerState = PlayerState.Base;
    
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private float _xMoveInputMemory = 1;
    
    private Vector2 _dashDirection;
    private int _currentDashAmount;
    private float _currentDashTime;
    
    private InputBuffer _jumpBuffer;
    private InputBuffer _dashBuffer;
    
    private float _gravityScaleMemory;

    private Vector2 _currentKnockback;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _jumpBuffer = new InputBuffer(_inputBufferTime);
        _jumpBuffer.inputCompleteEvent.AddListener(Jump);
        
        _dashBuffer = new InputBuffer(_inputBufferTime);
        _dashBuffer.inputCompleteEvent.AddListener(Jump);
        
        _gravityScaleMemory = _rb.gravityScale;
    }

    void FixedUpdate()
    {
        bool groundStateChange = isGrounded;
        isGrounded = IsGrounded();
        groundStateChange = isGrounded != groundStateChange;
        
        if (groundStateChange && isGrounded)
        {
            _currentDashAmount = _dashAmount;
            _jumpBuffer.TryInput();
        }
        
        Vector2 newVelocity = Vector2.zero;
        _rb.linearVelocityX -= _currentKnockback.x;
        float currentFriction = isGrounded ? _groundFriction : _airFriction;
        switch(playerState)
        {
            case PlayerState.Base:
                newVelocity.y = _rb.linearVelocity.y;
                float a = _rb.linearVelocity.x;
                float b = _moveSpeed * _moveInput.x;
                float c = Mathf.Abs(currentFriction * _moveSpeed * _moveInput.x);
                if(Mathf.Abs(_moveInput.x) > 0) newVelocity.x = Mathf.MoveTowards(a,b,c);
                else newVelocity.x = Mathf.MoveTowards(a,b,currentFriction * _moveSpeed / 2);
                
                break;
            case PlayerState.Dashing:
                newVelocity = Vector2.MoveTowards(_rb.linearVelocity ,_dashDirection * _dashForce , currentFriction * _dashForce);
                if (_currentDashTime > _dashTime)
                {
                    newVelocity.y *= 0.5f;
                    SwitchPlayerState(PlayerState.Base);
                }
                else _currentDashTime += Time.deltaTime;
                break;
            case PlayerState.Frozen:
                _currentKnockback = Vector2.zero;
                break;
        }

        newVelocity += _currentKnockback * _knockbackMultiplier;
        _currentKnockback *= new Vector2(_xKnockbackDecayMultiplier, _yKnockbackDecayMultiplier);
        _rb.linearVelocity = newVelocity;
        
        if(_debugUpdateDebugText) UpdateDebugText();
    }
    
    public void Knockback(float knockbackForce, Vector2 knockbackDirection)
    { 
        if (playerState == PlayerState.Frozen) return;
        _rb.linearVelocity = Vector2.zero;
       knockbackDirection.Normalize();
       _currentKnockback += knockbackDirection * knockbackForce;
    }
    
    public bool SwitchPlayerState (PlayerState newState, bool force = false)
    {
        switch (playerState)
        {
            case PlayerState.Frozen:
                if (!force) return false; 
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                break;
            case PlayerState.Dashing:
                _rb.gravityScale = _gravityScaleMemory;
                if(isGrounded) _currentDashAmount = _dashAmount;
                break;
        }
        switch (newState)
        {
            case PlayerState.Frozen:
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
            case PlayerState.Dashing:
                _rb.gravityScale = 0;
                break;
            case PlayerState.Base:
                if (_dashBuffer.TryInput())
                {
                    print("boom");
                }
                break;
        }

        playerState = newState;
        return true;
    }
    
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, _raycastGroundSize, 0, Vector2.down, _raycastGroundDistance,_raycastGroundLayer);
        return hit.collider;
    }

    private void UpdateDebugText()
    {
        if(_debugIsGroundedText) _debugIsGroundedText.text = $"Is Grounded: {IsGrounded()}";
        if(_debugIsInputBufferingText) _debugIsInputBufferingText.text = $"Is Jump Buffering: {_jumpBuffer.isPressed}";
        if(_debugInBufferTimerText) _debugInBufferTimerText.text = $"Input Jump Buffer Timer: {Time.unscaledTime - _jumpBuffer.lastPress :F}";
        if(_debugVelocityText) _debugVelocityText.text = $"Velocity: {_rb.linearVelocity.x :F} x , {_rb.linearVelocity.y :F} y";
        if(_debugPlayerStateText) _debugPlayerStateText.text = $"Player State: {playerState}";
        if(_debugDashAmountText) _debugDashAmountText.text = $"Dash Amount: {_currentDashAmount} / {_dashAmount}";
        if(_debugDashTimeText) _debugDashTimeText.text = $"Dash Time: {_currentDashTime} / {_dashTime}";
        if(_debugKnockbackText) _debugKnockbackText.text = $"Knockback: {_currentKnockback} ";
    }

    public void Jump()
    {
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);   
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        if (_moveInput.x != 0) _xMoveInputMemory = _moveInput.x;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && playerState == PlayerState.Base)
        {
            if (IsGrounded())
            {
                Jump();
            }
            else
            {
                _jumpBuffer.RegisterInput();
            }
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.performed && _currentDashAmount > 0 )
        {
            if (playerState == PlayerState.Base )
            {
                Dash();
            }
            else
            {
                _dashBuffer.RegisterInput();
            }
        }
    }

    public void Dash()
    {
        if(_currentDashAmount > 0)
        {
            if (Mathf.Approximately(_moveInput.x, 0) && Mathf.Approximately(_moveInput.y, 0))
            {
                _dashDirection = Vector2.zero;
                _dashDirection.x = _xMoveInputMemory;
                _dashDirection.Normalize();
            }
            else
            {
                _dashDirection = _moveInput.normalized;
            }

            _currentDashTime = 0f;
            _currentDashAmount--;
            SwitchPlayerState(PlayerState.Dashing);
        }
    }

    public void FreezePlayer(bool freeze)
    {
        SwitchPlayerState(freeze ? PlayerState.Frozen : PlayerState.Base);        
    }

    
}
public enum PlayerState
{
    Base,
    Frozen,
    Dashing,
    Attacking,
}