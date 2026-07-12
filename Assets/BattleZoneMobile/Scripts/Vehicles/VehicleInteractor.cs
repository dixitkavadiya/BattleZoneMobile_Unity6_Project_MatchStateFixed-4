using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class VehicleInteractor : MonoBehaviour
    {
        [SerializeField] private ThirdPersonMobileController controller;
        [SerializeField] private WeaponController weapons;
        [SerializeField] private Text promptText;
        [SerializeField] private float enterRange = 4.2f;

        private VehicleController currentVehicle;
        private VehicleController nearestVehicle;

        public void ConfigureForRuntime(ThirdPersonMobileController playerController, WeaponController weaponController, Text prompt)
        {
            controller = playerController;
            weapons = weaponController;
            promptText = prompt;
        }

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ThirdPersonMobileController>();
            }

            if (weapons == null)
            {
                weapons = GetComponent<WeaponController>();
            }
        }

        private void Update()
        {
            if (currentVehicle != null)
            {
                SetPrompt("Tap EXIT to leave vehicle");
                return;
            }

            nearestVehicle = VehicleController.FindNearestAvailable(transform.position, enterRange);
            if (nearestVehicle != null)
            {
                SetPrompt($"Tap DRIVE to enter {nearestVehicle.DisplayName}");
            }
            else
            {
                SetPrompt(string.Empty);
            }
        }

        public void ToggleVehicle()
        {
            if (currentVehicle != null)
            {
                currentVehicle.Exit();
                currentVehicle = null;
                return;
            }

            nearestVehicle = VehicleController.FindNearestAvailable(transform.position, enterRange);
            if (nearestVehicle != null && nearestVehicle.TryEnter(controller, weapons))
            {
                currentVehicle = nearestVehicle;
            }
        }

        public void ForceExitVehicle()
        {
            if (currentVehicle != null)
            {
                currentVehicle.Exit();
                currentVehicle = null;
            }

            weapons?.SetControlsEnabled(true);
        }

        private void SetPrompt(string value)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.text = value;
        }
    }
}
