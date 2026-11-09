using UnityEngine;
using UnityEngine.Events;

public class InputBuffer
{
    public UnityEvent inputCompleteEvent = new UnityEvent();
    private float _inputBufferTime;
    
    public float lastPress;
    public bool isPressed;

    public InputBuffer(float inputBufferTime)
    {
        _inputBufferTime = inputBufferTime;
    }

    public void RegisterInput()
    {
        lastPress = Time.unscaledTime;
        isPressed = true;
    }

    public bool TryInput()
    {
        if(!isPressed) return false;
        isPressed = false;
        if (lastPress + _inputBufferTime > Time.unscaledTime)
        {
            inputCompleteEvent?.Invoke();
            return true;
        }
        return false;
    }

    public void Reset()
    {
        isPressed = false;
        lastPress = 0f;
    }

}