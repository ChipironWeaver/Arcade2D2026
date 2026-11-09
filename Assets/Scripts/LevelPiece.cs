using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelPiece : MonoBehaviour
{
    [Required] public Transform startPosition;
    [Required] public Transform endPosition;
    
    [Header("Scroll Adjustment OPTIONAL AND NOT NEEDED")]
    [SerializeField] private CameraAutoScroll _scroller;
    
    public void GoTo(Vector3 position)
    {
        transform.position = position + startPosition.localPosition * -1;
    }
    public Vector3 GetCenter()
    {
        return new Vector3(Mathf.Lerp(startPosition.position.x , endPosition.position.x,0.5f),
            Mathf.Lerp(startPosition.position.y , endPosition.position.y,0.5f));
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
        Vector2 difference = endPosition.position - startPosition.position;
        float magnitude = difference.magnitude;
        endPosition.position = startPosition.position;
        Vector3 direction = _scroller.scrollDirection.normalized;
        endPosition.position += direction * magnitude;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.limeGreen;
        Gizmos.DrawSphere(startPosition.position, 0.5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPosition.position, 0.5f);
        
        Gizmos.color = Color.lightCyan;
        Vector2 center = new Vector2(Mathf.Lerp(startPosition.position.x , endPosition.position.x,0.5f),
            Mathf.Lerp(startPosition.position.y , endPosition.position.y,0.5f));
        Vector2 size = new Vector2(Mathf.Abs(startPosition.position.x - endPosition.position.x),
            Mathf.Abs(startPosition.position.y - endPosition.position.y));
        
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.DrawLine(startPosition.position, endPosition.position);
    }
}
