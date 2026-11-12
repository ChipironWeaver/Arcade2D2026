using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraDeathZone : MonoBehaviour
{
    [SerializeField,Tag] private string _playerTag = "Player";
    [SerializeField] private Vector2 _margin;
    [SerializeField,Required] private Camera _camera;
    private BoxCollider2D _collider;
    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        UpdateColliderSize();
    }

    public void Update()
    {
        UpdateColliderSize();
    }

    [Button]
    public void UpdateColliderSize()
    {
        float normalizedAspectRatio = (float)Screen.width / Screen.height;
        float targetHeight = _camera.orthographicSize * 2;
        float targetWidth = targetHeight * normalizedAspectRatio;
        _collider.size = new Vector2(targetWidth, targetHeight) + _margin;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(_playerTag))
        {
            if (other.TryGetComponent<PlayerHealthController>(out var player))
            {
                player.KillPlayer();
            }
        }
    }
}
