using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatRecoilApplicator : MonoBehaviour
    {
        [Header("Channels")]
        [SerializeField] private ThirdPersonMobileController cameraRecoilReceiver;
        [SerializeField] private Transform weaponRecoilTransform;
        [SerializeField] private UIManager uiManager;

        private Vector3 weaponBaseLocalPosition;
        private Quaternion weaponBaseLocalRotation = Quaternion.identity;
        private Vector3 weaponKickOffset;
        private Vector3 weaponKickEuler;
        private float crosshairBloom;
        private float weaponReturnSpeed = 18f;
        private float crosshairRecoverySpeed = 20f;

        public float CurrentCrosshairBloom => crosshairBloom;

        public void Configure(ThirdPersonMobileController cameraReceiver, Transform weaponTransform, UIManager hud)
        {
            cameraRecoilReceiver = cameraReceiver;
            weaponRecoilTransform = weaponTransform;
            uiManager = hud;
            CacheWeaponBasePose();
        }

        private void Awake()
        {
            CacheWeaponBasePose();
        }

        private void Update()
        {
            if (weaponRecoilTransform != null)
            {
                float weaponBlend = 1f - Mathf.Exp(-Mathf.Max(0.1f, weaponReturnSpeed) * Time.deltaTime);
                weaponKickOffset = Vector3.Lerp(weaponKickOffset, Vector3.zero, weaponBlend);
                weaponKickEuler = Vector3.Lerp(weaponKickEuler, Vector3.zero, weaponBlend);
                weaponRecoilTransform.localPosition = weaponBaseLocalPosition + weaponKickOffset;
                weaponRecoilTransform.localRotation = weaponBaseLocalRotation * Quaternion.Euler(weaponKickEuler);
            }

            crosshairBloom = Mathf.MoveTowards(crosshairBloom, 0f, Time.deltaTime * Mathf.Max(0.1f, crosshairRecoverySpeed));
        }

        public void Apply(AdvancedWeaponData data, bool aiming, float heat)
        {
            if (data == null || data.Recoil == null)
            {
                return;
            }

            CombatRecoilProfile recoil = data.Recoil;
            weaponReturnSpeed = recoil.weaponReturnSpeed;
            crosshairRecoverySpeed = recoil.crosshairRecoverySpeed;
            float aimScale = aiming ? 0.62f : 1f;
            float heatScale = 1f + Mathf.Clamp01(heat) * 0.35f;

            if (cameraRecoilReceiver != null)
            {
                float yaw = Random.Range(-recoil.cameraYawKick, recoil.cameraYawKick) * aimScale * heatScale;
                float pitch = recoil.cameraPitchKick * aimScale * heatScale;
                cameraRecoilReceiver.AddRecoil(pitch, yaw, recoil.cameraRecoverySpeed);
            }

            if (weaponRecoilTransform != null)
            {
                weaponKickOffset += Vector3.back * recoil.weaponKickDistance * heatScale;
                weaponKickEuler += new Vector3(-recoil.weaponPitchKick * heatScale, Random.Range(-0.5f, 0.5f) * recoil.weaponPitchKick, 0f);
            }

            crosshairBloom = Mathf.Max(crosshairBloom, recoil.crosshairBloomPerShot);
            uiManager?.PulseCrosshair();
        }

        private void CacheWeaponBasePose()
        {
            if (weaponRecoilTransform == null)
            {
                return;
            }

            weaponBaseLocalPosition = weaponRecoilTransform.localPosition;
            weaponBaseLocalRotation = weaponRecoilTransform.localRotation;
        }
    }
}
