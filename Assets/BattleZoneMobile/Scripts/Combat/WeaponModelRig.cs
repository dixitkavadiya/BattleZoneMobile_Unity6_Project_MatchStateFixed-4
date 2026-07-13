using System;
using UnityEngine;

namespace BattleZoneMobile
{
    [Serializable]
    public class WeaponModelEntry
    {
        public WeaponSlot slot;
        public GameObject modelRoot;
    }

    public class WeaponModelRig : MonoBehaviour
    {
        [SerializeField] private WeaponModelEntry[] models;
        [SerializeField] private float switchAnimationDuration = 0.16f;
        [SerializeField] private float swayAmount = 0.018f;
        [SerializeField] private float swayRotationAmount = 1.6f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 weaponKickOffset;
        private Vector3 weaponKickEuler;
        private float weaponReturnSpeed = 18f;
        private float switchTimer;

        public void Configure(params WeaponModelEntry[] runtimeModels)
        {
            models = runtimeModels;
            CacheBasePose();
            ShowWeapon(WeaponSlot.Pistol);
        }

        private void Awake()
        {
            CacheBasePose();
        }

        private void Update()
        {
            float switchLift = 0f;
            if (switchTimer > 0f)
            {
                switchTimer = Mathf.Max(0f, switchTimer - Time.deltaTime);
                float normalized = switchAnimationDuration <= 0f ? 1f : switchTimer / switchAnimationDuration;
                switchLift = Mathf.Sin(normalized * Mathf.PI) * -0.08f;
            }

            float sway = Mathf.Sin(Time.time * 5.6f) * swayAmount;
            float swayRoll = Mathf.Sin(Time.time * 4.2f) * swayRotationAmount;
            float kickBlend = 1f - Mathf.Exp(-Mathf.Max(0.1f, weaponReturnSpeed) * Time.deltaTime);
            weaponKickOffset = Vector3.Lerp(weaponKickOffset, Vector3.zero, kickBlend);
            weaponKickEuler = Vector3.Lerp(weaponKickEuler, Vector3.zero, kickBlend);

            Vector3 targetPosition = baseLocalPosition + new Vector3(sway * 0.45f, switchLift + sway * 0.25f, 0f) + weaponKickOffset;
            Quaternion targetRotation = baseLocalRotation * Quaternion.Euler(weaponKickEuler + new Vector3(0f, swayRoll * 0.35f, -swayRoll));
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 1f - Mathf.Exp(-16f * Time.deltaTime));
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }

        public void ShowWeapon(WeaponSlot slot)
        {
            if (models == null)
            {
                return;
            }

            for (int i = 0; i < models.Length; i++)
            {
                WeaponModelEntry entry = models[i];
                if (entry != null && entry.modelRoot != null)
                {
                    entry.modelRoot.SetActive(entry.slot == slot);
                }
            }

            switchTimer = switchAnimationDuration;
        }

        public void ApplyWeaponKick(float distance, float pitchKick, float yawKick, float returnSpeed)
        {
            weaponReturnSpeed = Mathf.Clamp(returnSpeed, 4f, 40f);
            weaponKickOffset += Vector3.back * Mathf.Clamp(distance, 0f, 0.2f);
            weaponKickEuler += new Vector3(-Mathf.Clamp(pitchKick, 0f, 12f), Mathf.Clamp(yawKick, -8f, 8f), 0f);
            weaponKickOffset = Vector3.ClampMagnitude(weaponKickOffset, 0.22f);
            weaponKickEuler = Vector3.ClampMagnitude(weaponKickEuler, 18f);
        }

        private void CacheBasePose()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
        }
    }
}
