using System.Numerics;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _groundFriction;
    [SerializeField] private float _airFriction;
    
    [Header("Ground Check Parameter")]
    [SerializeField] private LayerMask _raycastGroundLayer;
    [SerializeField] private Vector2 _raycastGroundSize;
    [SerializeField] private float _raycastGroundDistance;
    
    [Header("Dash")]
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashTime;
    [SerializeField] private int _dashAmount;
    [SerializeField] private AnimationCurve _dashCurve;

    [Header("Kayote Input Buffer")] 
    [SerializeField] private float _inputBufferTime;
    
    
    [Header("Debug")]
    [SerializeField] private bool _debugUpdateDebugText;
    [SerializeField] private TextMeshProUGUI _debugIsGroundedText;
    [SerializeField] private TextMeshProUGUI _debugIsInputBufferingText;
    [SerializeField] private TextMeshProUGUI _debugInBufferTimerText;
    [SerializeField] private TextMeshProUGUI _debugVelocityText;
    [SerializeField] private TextMeshProUGUI _debugPlayerStateText;
    [SerializeField] private TextMeshProUGUI _debugRandom;

    public bool isGrounded;
    public PlayerState playerState = PlayerState.Base;
    
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    
    private Vector2 _dashDirection;
    private int _currentDashAmount;
    private float _currentDashTime;


    private InputBuffer _jumpBuffer;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _jumpBuffer = new InputBuffer(_inputBufferTime);
        _jumpBuffer.inputCompleteEvent.AddListener(Jump);
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
        
        switch(playerState)
        {
            case PlayerState.Base:
                newVelocity.y = _rb.linearVelocity.y;
                float currentFriction = isGrounded ? _groundFriction : _airFriction;
                newVelocity.x = Mathf.Lerp( _rb.linearVelocity.x, _moveSpeed * _moveInput.x,currentFriction);
                break;
        }
        
        
        _rb.linearVelocity = newVelocity;
        
        if(_debugUpdateDebugText) UpdateDebugText();
    }
    
    public bool SwitchPlayerState (PlayerState newState, bool force = false)
    {
        switch (playerState)
        {
            case PlayerState.Frozen:
                if (!force) return false; 
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                break;
        }
        switch (newState)
        {
            case PlayerState.Frozen:
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
        }
        return true;
    }
    
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, _raycastGroundSize, 0, Vector2.down, _raycastGroundDistance,_raycastGroundLayer);
        return hit.collider;
    }

    private void UpdateDebugText()
    {
        _debugIsGroundedText.text = $"Is Grounded: {IsGrounded()}";
        _debugIsInputBufferingText.text = $"Is Jump Buffering: {_jumpBuffer.isPressed}";
        _debugInBufferTimerText.text = $"Input Jump Buffer Timer: {Time.unscaledTime - _jumpBuffer.lastPress :F}";
        _debugVelocityText.text = $"Velocity: {_rb.linearVelocity.x :F} x , {_rb.linearVelocity.y :F} y";
        _debugPlayerStateText.text = $"Player State: {playerState}";
    }

    public void Jump()
    {
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);   
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
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
        Debug.Log("Attack");
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (playerState == PlayerState.Base && _currentDashAmount > 0)
        {
            Dash();
        }
    }

    public void Dash()
    {
        if(_currentDashAmount > 0)
        {
            _dashDirection = _moveInput.normalized;
            _currentDashAmount--;
        }
    }

    public void FreezePlayer(bool freeze)
    {
        SwitchPlayerState(freeze ? PlayerState.Frozen : PlayerState.Base);        
    }

    public enum PlayerState
    {
        Base,
        Frozen,
        Dashing,
        Attacking,
    }
}