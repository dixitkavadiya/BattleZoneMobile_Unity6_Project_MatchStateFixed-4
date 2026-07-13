using UnityEngine;

namespace BattleZoneMobile
{
    public class GunFeelFeedbackController : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private ThirdPersonMobileController thirdPersonController;
        [SerializeField] private ReliablePlayerMovement reliableMovement;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float movementReferenceSpeed = 7f;

        public void Configure(
            UIManager hud,
            WeaponController weapons,
            ThirdPersonMobileController controller,
            ReliablePlayerMovement movement,
            CharacterController controllerComponent)
        {
            uiManager = hud;
            weaponController = weapons;
            thirdPersonController = controller;
            reliableMovement = movement;
            characterController = controllerComponent;
        }

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (weaponController == null)
            {
                weaponController = GetComponent<WeaponController>();
            }

            if (thirdPersonController == null)
            {
                thirdPersonController = GetComponent<ThirdPersonMobileController>();
            }

            if (reliableMovement == null)
            {
                reliableMovement = GetComponent<ReliablePlayerMovement>();
            }
        }

        private void LateUpdate()
        {
            if (uiManager == null || weaponController == null)
            {
                return;
            }

            Vector3 velocity = reliableMovement != null ? reliableMovement.CharacterControllerVelocity : characterController != null ? characterController.velocity : Vector3.zero;
            velocity.y = 0f;
            float movement01 = Mathf.Clamp01(velocity.magnitude / Mathf.Max(0.1f, movementReferenceSpeed));
            bool aiming = thirdPersonController != null && thirdPersonController.IsAiming;
            bool crouching = (thirdPersonController != null && thirdPersonController.IsCrouching) || (reliableMovement != null && reliableMovement.IsCrouching);
            bool prone = thirdPersonController != null && thirdPersonController.IsProne;
            uiManager.SetCrosshairFeedback(movement01, aiming, crouching, prone, weaponController.CurrentCrosshairBloom, weaponController.CurrentDefinition);
        }
    }
}
