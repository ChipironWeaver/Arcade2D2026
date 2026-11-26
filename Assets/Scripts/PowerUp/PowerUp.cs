using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace PowerUp
{
    public abstract class PowerUp : MonoBehaviour
    {
        public UnityEvent onActivate;
        public bool isUsed;
        public void Activate(PlayerController playerController)
        {
            Debug.Log($"PowerUp Activated: {gameObject.name}");
            if(isUsed) return;
            isUsed = true;
            ActivatePowerUp(playerController);
        }

        public void Reset()
        {
            isUsed = false;
            ResetPowerUp();
        }
        protected abstract void ActivatePowerUp(PlayerController player);
        protected abstract void ResetPowerUp();
    }
}
