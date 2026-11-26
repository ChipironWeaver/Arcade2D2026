using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraAutoScroll : MonoBehaviour
{
    [SerializeField] private float _baseScrollSpeed;
    [SerializeField] private float _scrollSpeedIncrease;
    [FormerlySerializedAs("_scrollDirection")] public Vector2 scrollDirection;
    [SerializeField] private Vector2 _gizmoSize;
    
    [SerializeField,ReadOnly] private float _scrollSpeed;

    [SerializeField, Range(0, 1)] private float _scrollEaseDuration;
    [SerializeField] private Ease _scrollEaseIn;
    [SerializeField] private Ease _scrollEaseOut;
    private Vector3 _scrollDirectionNormalized;
    private float _scrollMultiplier = 1f;

    private Sequence _multSequence;
    
    private void Start()
    {
        Reset();
        Actions.CameraMoveMultiplier += HandleMoveMultiplier;
    }

    private void HandleMoveMultiplier(float multiplier, float duration)
    {
        if(_multSequence != null)
        {
            if (_multSequence.active)
            {
                _multSequence.Kill();
                _scrollMultiplier = 1;
            }
        }
        
        _multSequence = DOTween.Sequence();
        _multSequence.Append(DOTween.To(() => _scrollMultiplier, x => _scrollMultiplier = x,
            multiplier, duration * (_scrollEaseDuration / 2))).SetEase(_scrollEaseIn);
        _multSequence.AppendCallback(() => { _scrollMultiplier = multiplier; });
        _multSequence.AppendInterval(duration * (1 - _scrollEaseDuration));
        _multSequence.Append(DOTween.To(() => _scrollMultiplier, x => _scrollMultiplier = x,
            1, duration * (_scrollEaseDuration / 2))).SetEase(_scrollEaseOut);
        _multSequence.AppendCallback(() => { _scrollMultiplier = 1; });
    }

    private void FixedUpdate()
    {
        Vector3 additionalDirection = _scrollDirectionNormalized * (_scrollSpeed / 10000);
        transform.position += additionalDirection * _scrollMultiplier;
        _scrollSpeed += _scrollSpeedIncrease;
    }

    private void OnDrawGizmos()
    {
        float rotation = Vector2.Angle(Vector2.left, new Vector2(scrollDirection.y , scrollDirection.x).normalized);
        Vector3 eulerAngle = new Vector3(0, 0, rotation);
        Quaternion rotationQuaternion = Quaternion.Euler(eulerAngle);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero,rotationQuaternion,  Vector3.one);
        
        
        Gizmos.matrix = rotationMatrix;	
        Gizmos.color = Color.orange;
        Gizmos.DrawWireCube( new Vector3(0,-_gizmoSize.y/2), _gizmoSize);
        Gizmos.DrawWireCube( new Vector3(0,-_gizmoSize.y/2), new Vector2(0.1f, _gizmoSize.y));
    }

    public void Reset()
    {
        _scrollDirectionNormalized = scrollDirection.normalized;
        _scrollSpeed = _baseScrollSpeed;
        transform.position = Vector3.zero;
    }
}
