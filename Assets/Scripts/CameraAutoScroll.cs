using System;
using NaughtyAttributes;
using UnityEngine;

public class CameraAutoScroll : MonoBehaviour
{
    [SerializeField] private float _baseScrollSpeed;
    [SerializeField] private float _scrollSpeedIncrease;
    [SerializeField] private Vector2 _scrollDirection;
    [SerializeField] private Vector2 _gizmoSize;
    
    [SerializeField,ReadOnly] private float _scrollSpeed;
    private Vector3 _scrollDirectionNormalized;

    private void Start()
    {
        _scrollDirectionNormalized = _scrollDirection.normalized;
        _scrollSpeed = _baseScrollSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 aditionalDirection = _scrollDirectionNormalized * (_scrollSpeed / 10000);
        transform.position += aditionalDirection;
        Debug.Log(aditionalDirection);
        _scrollSpeed += _scrollSpeedIncrease;
    }

    private void OnDrawGizmos()
    {
        float rotation = Vector2.Angle(Vector2.left, new Vector2(_scrollDirection.y , _scrollDirection.x).normalized);
        Vector3 eulerAngle = new Vector3(0, 0, rotation);
        Quaternion rotationQuaternion = Quaternion.Euler(eulerAngle);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero,rotationQuaternion,  Vector3.one);
        
        
        Gizmos.matrix = rotationMatrix;	
        Gizmos.color = Color.orange;
        Gizmos.DrawWireCube( new Vector3(0,-_gizmoSize.y/2), _gizmoSize);
    }
}
