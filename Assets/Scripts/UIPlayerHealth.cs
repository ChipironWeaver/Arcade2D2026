using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIPlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerHealthController _targetPlayer;
    [SerializeField] private List<Image> _coloredImages;
    [FormerlySerializedAs("_overchargeText")] [SerializeField] private TextMeshProUGUI _targetText;
    
    public bool IsLinked => _targetPlayer;

    private void Start()
    {
        if(_targetPlayer) Link(_targetPlayer);
        else Unlink();
    }

    public void UpdateHealth()
    {
        if (!_targetPlayer) return;
        _targetText.text = $"{_targetPlayer.currentOverCharge} %";
    }

    public void Link(PlayerHealthController playerHealthController)
    {
        Unlink();
        _targetPlayer = playerHealthController;
        _targetPlayer.onPlayerDamage.AddListener(UpdateHealth);
        UpdateHealth();
    }
    
    public void Unlink()
    {
        if(_targetPlayer) _targetPlayer.onPlayerDamage.RemoveListener(UpdateHealth);
        SetColor(Color.clear);
        _targetText.text = "Press a key to join";
        _targetPlayer = null;
    }
    
    public void SetColor(Color color)
    {
        foreach (Image image in _coloredImages)
        {
            image.color = color;
        }
    }
}
