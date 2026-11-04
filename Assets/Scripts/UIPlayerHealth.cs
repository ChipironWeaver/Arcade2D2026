using System;
using TMPro;
using UnityEngine;

public class UIPlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerHealthController _targetPlayer;
    [SerializeField] private TextMeshProUGUI _overchargeText;

    public void Start()
    {
        if (!_targetPlayer)
        {
            enabled = false;
            return;
        }
        _targetPlayer.onPlayerDamage.AddListener(UpdateHealth);
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        _overchargeText.text = $"{_targetPlayer.currentOverCharge} %";
    }
}
