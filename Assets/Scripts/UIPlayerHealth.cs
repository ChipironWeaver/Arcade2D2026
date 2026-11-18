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
    
    private void UpdateHealth()
    {
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
    }
    
    public void SetColor(Color color)
    {
        foreach (Image image in _coloredImages)
        {
            image.color = color;
        }
    }
}
