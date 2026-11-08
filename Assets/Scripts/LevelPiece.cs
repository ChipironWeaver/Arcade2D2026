using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelPiece : MonoBehaviour
{
    [SerializeField,Required] private Transform _startPosition;
    [Required] public Transform endPosition;
    
    [Header("Scroll Adjustment OPTIONAL AND NOT NEEDED")]
    [SerializeField] private CameraAutoScroll _scroller;
    
    public void GoTo(Vector3 position)
    {
        transform.position = position + _startPosition.localPosition * -1;
    }
    public Vector3 GetCenter()
    {
        return new Vector3(Mathf.Lerp(_startPosition.position.x , endPosition.position.x,0.5f),
            Mathf.Lerp(_startPosition.position.y , endPosition.position.y,0.5f));
    }
    [Button] private void GoTo00()
    {
        GoTo(Vector3.zero);
    }
    [Button] private void UpdateEndPositionBasedOnScrollDirection()
    {
        if (!_scroller)
        {
            Debug.LogWarning("Scroller field is missing");
            return;
        }
        Vector2 difference = endPosition.position - _startPosition.position;
        float magnitude = difference.magnitude;
        endPosition.position = _startPosition.position;
        Vector3 direction = _scroller.scrollDirection.normalized;
        endPosition.position += direction * magnitude;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.limeGreen;
        Gizmos.DrawSphere(_startPosition.position, 0.5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPosition.position, 0.5f);
        
        Gizmos.color = Color.lightCyan;
        Vector2 center = new Vector2(Mathf.Lerp(_startPosition.position.x , endPosition.position.x,0.5f),
            Mathf.Lerp(_startPosition.position.y , endPosition.position.y,0.5f));
        Vector2 size = new Vector2(Mathf.Abs(_startPosition.position.x - endPosition.position.x),
            Mathf.Abs(_startPosition.position.y - endPosition.position.y));
        
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.DrawLine(_startPosition.position, endPosition.position);
    }
}
