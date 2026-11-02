using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    
    [Header("Ground Check Parameter")]
    [SerializeField] private LayerMask _raycastGroundLayer;
    [SerializeField] private Vector2 _raycastGroundSize;
    [SerializeField] private float _raycastGroundDistance;

    [Header("Kayote Input Buffer")] 
    [SerializeField] private float _inputBufferTime;
    
    
    [Header("Debug")]
    [SerializeField] private bool _debugUpdateDebugText;
    [SerializeField] private TextMeshProUGUI _debugIsGroundedText;
    [SerializeField] private TextMeshProUGUI _debugIsInputBufferingText;
    [SerializeField] private TextMeshProUGUI _debugInBufferTimerText;
    [SerializeField] private TextMeshProUGUI _debugVelocityText;

    public bool isGrounded;
    
    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private InputBuffer _jumpBuffer;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _jumpBuffer = new InputBuffer(_inputBufferTime);
        _jumpBuffer.inputCompleteEvent.AddListener(Jump);
    }

    void FixedUpdate()
    {
        bool justLanded = isGrounded;
        isGrounded = IsGrounded();
        justLanded = isGrounded != justLanded;
        
        if (justLanded) _jumpBuffer.TryInput();
        
        Vector2 newVelocity = _rb.linearVelocity;
        newVelocity.x = _moveInput.x * _moveSpeed;
        _rb.linearVelocity = newVelocity;
        
        if(_debugUpdateDebugText) UpdateDebugText();
    }
    
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, _raycastGroundSize, 0, Vector2.down, _raycastGroundDistance,_raycastGroundLayer);

        return hit.collider;
    }

    private void UpdateDebugText()
    {
        _debugIsGroundedText.text = $"Is Grounded: {IsGrounded()}";
        _debugIsInputBufferingText.text = $"Is Buffering: {_jumpBuffer.isPressed}";
        _debugInBufferTimerText.text = $"Input Buffer Timer: {Time.unscaledTime - _jumpBuffer.lastPress}";
        _debugVelocityText.text = $"Velocity: {_rb.linearVelocity.x} x , {_rb.linearVelocity.y} y";
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
        if (context.performed)
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
        Debug.Log("Dash");   
    }
}