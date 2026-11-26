using UnityEngine;

namespace PowerUp
{
    public class PowerUpCameraMove : PowerUp
    {
        [SerializeField] private float _cameraMoveMultiplier; 
        [SerializeField] private float _duration; 
        protected override void ActivatePowerUp(PlayerController player)
        {
            Actions.CameraMoveMultiplier.Invoke(_cameraMoveMultiplier, _duration);
        }

        protected override void ResetPowerUp()
        {
            
        }
    }
}