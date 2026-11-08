using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    [SerializeField,Required] private Transform _firstStartPosition;
    [SerializeField] private List<LevelPiece> _levelPiecesPrefab = new();
    [SerializeField,Required] private CameraAutoScroll _cameraAutoScroll;
    [SerializeField] private bool _autoAddLevelPieces;
    [SerializeField] private int _levelPiecesDistance;
    
    [Header("Read only")]
    [SerializeField,ReadOnly] private List<LevelPiece> _activeLevelPieces = new();
    [SerializeField,ReadOnly] private Vector3 _currentCenterPiece;
    [SerializeField,ReadOnly] private List<int> _activeLevelPiecesType = new();
    [SerializeField,ReadOnly] private Dictionary<int,List<LevelPiece>> _levelPieceCulling = new();

    private int _currentLevelIndex;
    private Vector2 _currentCenterPosition;
    public void Start()
    {
        if(_autoAddLevelPieces)
        {
            if (_levelPiecesDistance <= 0)
            {
                Debug.LogWarning("Level piece distance can't be 0 or less");
                enabled = false;
            }
            for (int i = 0; i < _levelPiecesDistance; i++)
            {
                AddNewLevelPiece();
            }

            _currentLevelIndex = 0;
            _currentCenterPosition = _activeLevelPieces[0].GetCenter();
        }
    }

    public void Update()
    {
        if(_autoAddLevelPieces)
        {
            float distance = _cameraAutoScroll.transform.position.x - _currentCenterPosition.x;
            if (distance > 0)
            {
                SmartAdd();   
            }
        }
    }

    private void SmartAdd()
    {
            _currentLevelIndex++;
            
            
            int checkIndex = _currentLevelIndex - (_levelPiecesDistance + 1);
            Debug.Log($"checkIndex: {checkIndex} currentLevelIndex: {_currentLevelIndex}");
            if (checkIndex > 0)
            {
                LevelPiece discard = _activeLevelPieces[0];
                int discardType = _activeLevelPiecesType[0];
                _activeLevelPieces.RemoveAt(0);
                _activeLevelPiecesType.RemoveAt(0);
                
                if (_levelPieceCulling.ContainsKey(discardType))
                {
                    _levelPieceCulling[discardType].Add(discard);
                }
                else
                {
                    List<LevelPiece> levelPieces = new();
                    levelPieces.Add(discard);
                    _levelPieceCulling.Add(discardType, levelPieces);
                }
                
                _currentLevelIndex--;
                discard.gameObject.SetActive(false); 
            }
            
            AddNewLevelPiece();
            _currentCenterPosition = _activeLevelPieces[_currentLevelIndex].GetCenter();
    }
    
    [Button]
    private void AddNewLevelPiece()
    {
        if (_levelPiecesPrefab.Count == 0)
        {
            Debug.LogWarning("Level piece prefab is empty");
            return;
        }
        int pieceIndex = Random.Range(0, _levelPiecesPrefab.Count);
        LevelPiece levelPiece = null;
        if (_levelPieceCulling.ContainsKey(pieceIndex))
        {
            if (_levelPieceCulling[pieceIndex].Count > 0)
            {
                levelPiece = _levelPieceCulling[pieceIndex][0];
                levelPiece.gameObject.SetActive(true);
                _levelPieceCulling[pieceIndex].RemoveAt(0);
            }
        }
        
        if(!levelPiece) levelPiece = Instantiate(_levelPiecesPrefab[pieceIndex], transform, true);
        
        Vector3 goToPosition = _firstStartPosition.position;
        if (_activeLevelPieces.Count >= 1)
        {
            goToPosition = _activeLevelPieces[^1].endPosition.position;
        }
        
        levelPiece.GoTo(goToPosition);
        _activeLevelPieces.Add(levelPiece);
        _activeLevelPiecesType.Add(pieceIndex);
    }
    
    [Button]
    public void AlignFirstPosition()
    {
        if (!_cameraAutoScroll)
        {
            Debug.LogWarning("Scroller field is missing");
            return;
        }
        
        float magnitude = _firstStartPosition.position.magnitude;
        Vector3 direction = _cameraAutoScroll.scrollDirection.normalized;
        _firstStartPosition.position = direction * magnitude;
    }
    
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.lightSkyBlue;
        Gizmos.DrawSphere(_firstStartPosition.position, 0.5f);
    }
}
