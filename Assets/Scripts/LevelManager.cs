using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelInstantiator _instantiator;
    [SerializeField] private CameraAutoScroll _cameraAutoScroll;
    [SerializeField] private List<Transform> _playersRespawnPositions;
    
    
    public Vector3 GetSpawnPosition(int index)
    {
        return _playersRespawnPositions[index % _playersRespawnPositions.Count].position;
    }

    public void OnDrawGizmos()
    {
        Color color = Color.green;
        foreach (Transform transform in _playersRespawnPositions)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h += 0.15f;
            color = Color.HSVToRGB(h, s, v);
        }
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void StartLevel()
    {
        _cameraAutoScroll.enabled = true;
        _instantiator.enabled = true;
    }
    public void Stop()
    {
        _cameraAutoScroll.enabled = false;
        _instantiator.enabled = false;
    }
}
