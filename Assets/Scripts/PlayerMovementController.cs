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
    [Header("References"),HorizontalLine,Space(7)] 
    [SerializeField] private Transform _rotatingPart;
    [SerializeField] private Animator _animator;
    
    [Header("Movement Settings"),HorizontalLine,Space(7)]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField,Range(0,1)] private float _groundFriction;
    [SerializeField,Range(0,1)] private float _airFriction;
    [SerializeField,Range(0,1)] private float _xKnockbackDecayMultiplier;
    [SerializeField,Range(0,1)] private float _yKnockbackDecayMultiplier;
    [SerializeField] private Vector2 _knockbackMultiplier;
    
    [Header("Ground Check Parameter"),HorizontalLine,Space(7)]
    [SerializeField] private LayerMask _raycastGroundLayer;
    [SerializeField] private Vector2 _raycastGroundSize;
    [SerializeField] private float _raycastGroundDistance;

    [Header("Attack"),HorizontalLine,Space(7)] 
    [SerializeField] private float _attackTime;
    public DamageSource slashObject;
    [SerializeField] private float _attackCooldown;
    
    [Header("Dash"),HorizontalLine,Space(7)]
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashTime;
    [SerializeField] private int _dashAmount;
    [SerializeField,CurveRange(0,0,1,1,EColor.Pink)] private AnimationCurve _dashCurve;

    [Header("Kayote and Input Buffer"),HorizontalLine,Space(7)] 
    [SerializeField] private float _inputBufferTime;
    
    [Header("Debug"),HorizontalLine,Space(7)]
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
    
    private float _currentStateTime;
    private float _attackCooldownTimer;
    
    private InputBuffer _jumpBuffer;
    private InputBuffer _dashBuffer;
    private InputBuffer _attackBuffer;
    
    private float _gravityScaleMemory;

    private Vector2 _currentKnockback;

    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _jumpBuffer = new InputBuffer(_inputBufferTime);
        _jumpBuffer.inputCompleteEvent.AddListener(Jump);
        
        _dashBuffer = new InputBuffer(_inputBufferTime);
        _dashBuffer.inputCompleteEvent.AddListener(Dash);
        
        _attackBuffer = new InputBuffer(_inputBufferTime);
        _attackBuffer.inputCompleteEvent.AddListener(Attack);
    }

    void FixedUpdate()
    {
        bool groundStateChange = isGrounded;
        isGrounded = IsGrounded();
        groundStateChange = isGrounded != groundStateChange;
        
        if (groundStateChange && isGrounded)
        {
            _currentDashAmount = _dashAmount;
            if (!_dashBuffer.TryInput()) _jumpBuffer.TryInput();
        }

        if (_attackCooldownTimer < _attackCooldown)
        {
            _attackCooldownTimer += Time.deltaTime;
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
                
                _rotatingPart.localEulerAngles = new Vector3(0 , _xMoveInputMemory > 0 ? 0 : 180 ,0);
                break;
            case PlayerState.Dashing:
                newVelocity = Vector2.MoveTowards(_rb.linearVelocity ,_dashDirection * _dashForce , currentFriction * _dashForce);
                if (_currentStateTime > _dashTime)
                {
                    newVelocity.y *= 0.5f;
                    SwitchPlayerState(PlayerState.Base);
                }
                break;
            case PlayerState.Attacking:
                newVelocity.y = _rb.linearVelocity.y;
                newVelocity.x = Mathf.MoveTowards(_rb.linearVelocity.x,0 ,currentFriction * _moveSpeed);
                if (_currentStateTime > _attackTime) SwitchPlayerState(PlayerState.Base);
                break;
            case PlayerState.Frozen:
                _currentKnockback = Vector2.zero;
                break;
        }
        _currentStateTime += Time.deltaTime;
        
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
            case PlayerState.Attacking:
                slashObject.gameObject.SetActive(false);
                _attackCooldownTimer = 0f;
                break;
        }
        switch (newState)
        {
            case PlayerState.Frozen:
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
            case PlayerState.Dashing:
                _rb.gravityScale = 0;
                _rb.linearVelocityY = 0;
                break;
            case PlayerState.Base:
                if(_dashBuffer != null & _attackBuffer != null)
                {
                    if (!_dashBuffer.TryInput())
                    {
                        _attackBuffer.TryInput();
                    }
                }
                break;
            case PlayerState.Attacking:
                slashObject.gameObject.SetActive(true);
                break;
        }

        playerState = newState;
        _currentStateTime = 0f;
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
        if(_debugDashTimeText) _debugDashTimeText.text = $"Dash Time: {_currentStateTime} / {_dashTime}";
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
        if (_moveInput.x != 0)
        {
            _xMoveInputMemory = _moveInput.x;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsGrounded() && playerState == PlayerState.Base)
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
        if (context.performed)
        {
            if (playerState == PlayerState.Base && _attackCooldownTimer >= _attackCooldown)
            {
                Attack();
            }
            else
            {
                _attackBuffer.RegisterInput();
            }
        }
    }

    private void Attack()
    {
        if (playerState == PlayerState.Base)
        {
            SwitchPlayerState(PlayerState.Attacking);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (playerState == PlayerState.Base && _currentDashAmount > 0)
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
            _currentDashAmount--;
            SwitchPlayerState(PlayerState.Dashing);
        }
    }

    public void FreezePlayer(bool freeze)
    {
        SwitchPlayerState(freeze ? PlayerState.Frozen : PlayerState.Base, true);        
        slashObject.gameObject.SetActive(false);
    }


    public void Reset()
    {
        slashObject.gameObject.SetActive(false);
        SwitchPlayerState(PlayerState.Base);
        if(_rb)
        {
            _rb.linearVelocity = Vector2.zero;
            _gravityScaleMemory = _rb.gravityScale;
        }
        _currentKnockback = new Vector2(0, 0);
    }
}
public enum PlayerState
{
    Base,
    Frozen,
    Dashing,
    Attacking,
}