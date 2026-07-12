using UnityEngine;

namespace BattleZoneMobile
{
    public class HumanoidPlaceholderAnimator : MonoBehaviour
    {
        [Header("Body Parts")]
        [SerializeField] private Transform hips;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform weaponRig;

        [Header("Motion")]
        [SerializeField] private float walkCycleSpeed = 5.5f;
        [SerializeField] private float runCycleSpeed = 8.5f;
        [SerializeField] private float limbSwing = 24f;
        [SerializeField] private float runLimbSwing = 42f;
        [SerializeField] private float crouchDrop = 0.28f;

        private Vector3 hipsStart;
        private Vector3 torsoStart;
        private Vector3 headStart;
        private Vector3 weaponStart;
        private float speed;
        private float smoothedSpeed;
        private float verticalVelocity;
        private bool grounded = true;
        private bool crouching;
        private bool prone;
        private bool sprinting;
        private bool aiming;
        private bool falling;
        private float crouchBlend;
        private float proneBlend;
        private float aimBlend;
        private float sprintBlend;
        private float fallBlend;
        private float groundedBlend = 1f;
        private float cycle;
        private float fireKick;
        private float hitReaction;
        private float reloadTimer;
        private float switchTimer;
        private float deathTimer;
        private float dropBlend;
        private float parachuteBlend;
        private float landingTimer;
        private Vector3 dropVelocity;
        private bool dropActive;
        private bool parachuteActive;
        private bool dead;

        public void Configure(
            Transform hipsPart,
            Transform torsoPart,
            Transform headPart,
            Transform leftArmPart,
            Transform rightArmPart,
            Transform leftLegPart,
            Transform rightLegPart,
            Transform weaponPart)
        {
            hips = hipsPart;
            torso = torsoPart;
            head = headPart;
            leftArm = leftArmPart;
            rightArm = rightArmPart;
            leftLeg = leftLegPart;
            rightLeg = rightLegPart;
            weaponRig = weaponPart;

            CacheStarts();
        }

        private void Awake()
        {
            CacheStarts();
        }

        private void Update()
        {
            if (dead)
            {
                UpdateDeathPose();
                return;
            }

            smoothedSpeed = Mathf.Lerp(smoothedSpeed, speed, 1f - Mathf.Exp(-12f * Time.deltaTime));
            crouchBlend = Mathf.Lerp(crouchBlend, crouching ? 1f : 0f, 1f - Mathf.Exp(-12f * Time.deltaTime));
            proneBlend = Mathf.Lerp(proneBlend, prone ? 1f : 0f, 1f - Mathf.Exp(-10f * Time.deltaTime));
            aimBlend = Mathf.Lerp(aimBlend, aiming ? 1f : 0f, 1f - Mathf.Exp(-14f * Time.deltaTime));
            sprintBlend = Mathf.Lerp(sprintBlend, sprinting ? 1f : 0f, 1f - Mathf.Exp(-10f * Time.deltaTime));
            fallBlend = Mathf.Lerp(fallBlend, falling ? 1f : 0f, 1f - Mathf.Exp(-13f * Time.deltaTime));
            groundedBlend = Mathf.Lerp(groundedBlend, grounded ? 1f : 0f, 1f - Mathf.Exp(-18f * Time.deltaTime));
            dropBlend = Mathf.Lerp(dropBlend, dropActive ? 1f : 0f, 1f - Mathf.Exp(-10f * Time.deltaTime));
            parachuteBlend = Mathf.Lerp(parachuteBlend, parachuteActive ? 1f : 0f, 1f - Mathf.Exp(-9f * Time.deltaTime));

            float normalizedSpeed = Mathf.Clamp01(smoothedSpeed / 7.1f);
            float moveCycleWeight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 1f, normalizedSpeed));
            float cycleSpeed = Mathf.Lerp(walkCycleSpeed, runCycleSpeed, sprintBlend);
            cycle += Time.deltaTime * cycleSpeed * Mathf.Lerp(0.62f, 1.08f, moveCycleWeight);

            float swingAmount = Mathf.Lerp(limbSwing, runLimbSwing, sprintBlend) * moveCycleWeight * groundedBlend;
            float swing = Mathf.Sin(cycle) * swingAmount;
            float counterSwing = Mathf.Sin(cycle + Mathf.PI) * swingAmount;
            float footPlant = Mathf.Sin(cycle * 2f);
            float idleBreath = Mathf.Sin(Time.time * 1.7f) * 0.012f * (1f - moveCycleWeight) * groundedBlend;
            float bob = (Mathf.Abs(footPlant) * moveCycleWeight * Mathf.Lerp(0.035f, 0.072f, sprintBlend) + idleBreath) * groundedBlend;
            float weaponSway = Mathf.Sin(cycle * 1.35f) * normalizedSpeed * Mathf.Lerp(0.045f, 0.015f, aimBlend);
            float crouch = Mathf.Lerp(0f, crouchDrop, crouchBlend) + Mathf.Lerp(0f, crouchDrop * 1.55f, proneBlend);
            float reloadPose = Mathf.Clamp01(reloadTimer / 0.7f);
            float switchPose = Mathf.Clamp01(switchTimer / 0.28f);
            float landingPose = Mathf.Clamp01(landingTimer / 0.55f);
            float jumpPose = Mathf.Clamp01(Mathf.InverseLerp(0.25f, 6f, verticalVelocity)) * (1f - groundedBlend);

            fireKick = Mathf.MoveTowards(fireKick, 0f, 7f * Time.deltaTime);
            hitReaction = Mathf.MoveTowards(hitReaction, 0f, 8f * Time.deltaTime);
            reloadTimer = Mathf.MoveTowards(reloadTimer, 0f, Time.deltaTime);
            switchTimer = Mathf.MoveTowards(switchTimer, 0f, Time.deltaTime);
            landingTimer = Mathf.MoveTowards(landingTimer, 0f, Time.deltaTime);

            if (hips != null)
            {
                hips.localPosition = Vector3.Lerp(hips.localPosition, hipsStart + new Vector3(0f, bob - crouch, 0f), 14f * Time.deltaTime);
            }

            if (torso != null)
            {
                torso.localPosition = Vector3.Lerp(torso.localPosition, torsoStart + new Vector3(hitReaction * 0.025f, -crouch * 0.35f, 0f), 14f * Time.deltaTime);
                float torsoPitch = Mathf.Lerp(0f, 8f, crouchBlend) + Mathf.Lerp(0f, 28f, proneBlend) + Mathf.Lerp(0f, -2f, aimBlend) + jumpPose * -8f + fallBlend * 10f + landingPose * 18f + hitReaction * 8f;
                torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(torsoPitch, aimBlend * 5f, -swing * 0.08f + hitReaction * 5f), 12f * Time.deltaTime);
            }

            if (head != null)
            {
                head.localPosition = Vector3.Lerp(head.localPosition, headStart + new Vector3(0f, -crouch * 0.2f, 0f), 14f * Time.deltaTime);
                head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(-4f * aimBlend - jumpPose * 5f + fallBlend * 3f + hitReaction * 4f, hitReaction * -10f, 0f), 12f * Time.deltaTime);
            }

            float leftArmX = Mathf.Lerp(counterSwing - reloadPose * 25f, -58f - reloadPose * 18f, aimBlend) - hitReaction * 10f;
            float rightArmX = Mathf.Lerp(swing - fireKick * 20f, -62f - fireKick * 16f + switchPose * 24f, aimBlend) + hitReaction * 8f;
            leftArmX = Mathf.Lerp(leftArmX, -18f, fallBlend * 0.6f);
            rightArmX = Mathf.Lerp(rightArmX, -18f, fallBlend * 0.6f);
            SetPartRotation(leftArm, leftArmX, 0f, Mathf.Lerp(-reloadPose * 18f, -8f, aimBlend));
            SetPartRotation(rightArm, rightArmX, 0f, Mathf.Lerp(reloadPose * 15f, 8f, aimBlend));

            float plantedLeft = Mathf.Abs(Mathf.Sin(cycle)) < 0.24f ? 0.45f : 1f;
            float plantedRight = Mathf.Abs(Mathf.Sin(cycle + Mathf.PI)) < 0.24f ? 0.45f : 1f;
            float leftLegX = Mathf.Lerp(swing * 0.85f * plantedLeft, 54f + swing * 0.18f, proneBlend);
            float rightLegX = Mathf.Lerp(counterSwing * 0.85f * plantedRight, 54f + counterSwing * 0.18f, proneBlend);
            leftLegX = Mathf.Lerp(leftLegX, Mathf.Lerp(-18f, 22f, fallBlend), Mathf.Clamp01(jumpPose + fallBlend));
            rightLegX = Mathf.Lerp(rightLegX, Mathf.Lerp(22f, -14f, fallBlend), Mathf.Clamp01(jumpPose + fallBlend));
            SetPartRotation(leftLeg, leftLegX, 0f, -8f * proneBlend - fallBlend * 6f);
            SetPartRotation(rightLeg, rightLegX, 0f, 8f * proneBlend + fallBlend * 6f);

            if (weaponRig != null)
            {
                Vector3 target = weaponStart + new Vector3(Mathf.Lerp(weaponSway, 0.08f + weaponSway, aimBlend), -crouchBlend * 0.08f - proneBlend * 0.16f - reloadPose * 0.07f, Mathf.Lerp(-fireKick * 0.04f, 0.06f - fireKick * 0.04f, aimBlend));
                weaponRig.localPosition = Vector3.Lerp(weaponRig.localPosition, target, 16f * Time.deltaTime);
                weaponRig.localRotation = Quaternion.Slerp(weaponRig.localRotation, Quaternion.Euler(Mathf.Lerp(8f + switchPose * 38f, -2f - fireKick * 12f, aimBlend), reloadPose * 18f, weaponSway * 120f), 14f * Time.deltaTime);
            }

            ApplyAerialPose(dropBlend, parachuteBlend, landingPose);
        }

        public void TriggerFire()
        {
            fireKick = 1f;
        }

        public void TriggerReload()
        {
            reloadTimer = 0.7f;
        }

        public void TriggerHit(float amount = 1f)
        {
            hitReaction = Mathf.Clamp01(hitReaction + Mathf.Clamp01(amount * 0.08f));
        }

        public void TriggerLanding()
        {
            landingTimer = 0.55f;
            dropActive = false;
            parachuteActive = false;
        }

        public void TriggerWeaponSwitch()
        {
            switchTimer = 0.28f;
        }

        public void TriggerDeath()
        {
            dead = true;
            deathTimer = 0f;
            fireKick = 0f;
            reloadTimer = 0f;
            switchTimer = 0f;
        }

        public void Revive()
        {
            dead = false;
            deathTimer = 0f;
            hitReaction = 0f;
            smoothedSpeed = 0f;
            dropBlend = 0f;
            parachuteBlend = 0f;
            landingTimer = 0f;
            dropVelocity = Vector3.zero;
            dropActive = false;
            parachuteActive = false;
            if (hips != null)
            {
                hips.localPosition = hipsStart;
                hips.localRotation = Quaternion.identity;
            }

            if (torso != null)
            {
                torso.localPosition = torsoStart;
                torso.localRotation = Quaternion.identity;
            }

            if (head != null)
            {
                head.localPosition = headStart;
                head.localRotation = Quaternion.identity;
            }

            if (weaponRig != null)
            {
                weaponRig.localPosition = weaponStart;
                weaponRig.localRotation = Quaternion.identity;
            }
        }

        public void SetState(float movementSpeed, bool isGrounded, bool isCrouching, bool isSprinting, bool isAiming)
        {
            SetState(movementSpeed, 0f, isGrounded, isCrouching, false, isSprinting, isAiming, false);
        }

        public void SetState(float movementSpeed, float currentVerticalVelocity, bool isGrounded, bool isCrouching, bool isProne, bool isSprinting, bool isAiming, bool isFalling)
        {
            speed = movementSpeed;
            verticalVelocity = currentVerticalVelocity;
            grounded = isGrounded;
            crouching = isCrouching;
            prone = isProne;
            sprinting = isSprinting;
            aiming = isAiming;
            falling = isFalling;
        }

        public void SetDropState(bool active, bool parachute, Vector3 velocity)
        {
            dropActive = active;
            parachuteActive = active && parachute;
            dropVelocity = velocity;
            if (active)
            {
                aiming = false;
                prone = false;
                crouching = false;
                grounded = false;
            }
        }

        private void CacheStarts()
        {
            if (hips != null) hipsStart = hips.localPosition;
            if (torso != null) torsoStart = torso.localPosition;
            if (head != null) headStart = head.localPosition;
            if (weaponRig != null) weaponStart = weaponRig.localPosition;
        }

        private void ApplyAerialPose(float aerialBlend, float chuteBlend, float landingPose)
        {
            float activeBlend = Mathf.Clamp01(aerialBlend);
            if (activeBlend <= 0.01f && landingPose <= 0.01f)
            {
                return;
            }

            float speedTilt = Mathf.Clamp(dropVelocity.magnitude / 42f, 0f, 1f);
            float sway = Mathf.Sin(Time.time * 4.4f) * Mathf.Lerp(2f, 6f, speedTilt);
            float skydiveBlend = activeBlend * (1f - chuteBlend);
            float parachutePose = activeBlend * chuteBlend;

            if (hips != null)
            {
                Vector3 aerialOffset = new Vector3(0f, -0.08f * skydiveBlend - 0.2f * parachutePose - 0.18f * landingPose, -0.1f * skydiveBlend);
                hips.localPosition = Vector3.Lerp(hips.localPosition, hipsStart + aerialOffset, 12f * Time.deltaTime);
            }

            if (torso != null)
            {
                float pitch = 68f * skydiveBlend + 10f * parachutePose + 20f * landingPose;
                torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(pitch, sway * 0.25f, -sway), 12f * Time.deltaTime);
            }

            if (head != null)
            {
                head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(-18f * skydiveBlend - 8f * parachutePose, sway * 0.35f, 0f), 12f * Time.deltaTime);
            }

            SetPartRotation(leftArm, Mathf.Lerp(-16f, -112f, parachutePose) + 8f * skydiveBlend, 0f, Mathf.Lerp(-72f, -18f, parachutePose));
            SetPartRotation(rightArm, Mathf.Lerp(-16f, -112f, parachutePose) + 8f * skydiveBlend, 0f, Mathf.Lerp(72f, 18f, parachutePose));
            SetPartRotation(leftLeg, Mathf.Lerp(34f, 18f, parachutePose) + 20f * landingPose, 0f, -16f * skydiveBlend - 8f * landingPose);
            SetPartRotation(rightLeg, Mathf.Lerp(34f, 18f, parachutePose) + 20f * landingPose, 0f, 16f * skydiveBlend + 8f * landingPose);

            if (weaponRig != null)
            {
                weaponRig.localPosition = Vector3.Lerp(weaponRig.localPosition, weaponStart + new Vector3(0.12f * skydiveBlend, -0.24f * activeBlend, 0.08f), 12f * Time.deltaTime);
                weaponRig.localRotation = Quaternion.Slerp(weaponRig.localRotation, Quaternion.Euler(28f * activeBlend, 12f * skydiveBlend, -18f * activeBlend), 12f * Time.deltaTime);
            }
        }

        private void UpdateDeathPose()
        {
            deathTimer += Time.deltaTime;
            float blend = 1f - Mathf.Exp(-7f * Time.deltaTime);
            float fall = Mathf.Clamp01(deathTimer * 2.3f);

            if (hips != null)
            {
                hips.localPosition = Vector3.Lerp(hips.localPosition, hipsStart + new Vector3(0f, -0.76f, 0.24f), blend);
                hips.localRotation = Quaternion.Slerp(hips.localRotation, Quaternion.Euler(76f * fall, 0f, 18f), blend);
            }

            if (torso != null)
            {
                torso.localPosition = Vector3.Lerp(torso.localPosition, torsoStart + new Vector3(0f, -0.12f, 0f), blend);
                torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(34f, -18f, 7f), blend);
            }

            if (head != null)
            {
                head.localPosition = Vector3.Lerp(head.localPosition, headStart + new Vector3(0f, -0.05f, 0f), blend);
                head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(24f, 28f, -12f), blend);
            }

            SetPartRotation(leftArm, -24f, 0f, -62f);
            SetPartRotation(rightArm, -18f, 0f, 66f);
            SetPartRotation(leftLeg, 12f, 0f, -18f);
            SetPartRotation(rightLeg, -18f, 0f, 18f);

            if (weaponRig != null)
            {
                weaponRig.localPosition = Vector3.Lerp(weaponRig.localPosition, weaponStart + new Vector3(0.18f, -0.46f, 0.12f), blend);
                weaponRig.localRotation = Quaternion.Slerp(weaponRig.localRotation, Quaternion.Euler(58f, 24f, -42f), blend);
            }
        }

        private static void SetPartRotation(Transform part, float x, float y, float z)
        {
            if (part == null)
            {
                return;
            }

            part.localRotation = Quaternion.Slerp(part.localRotation, Quaternion.Euler(x, y, z), 14f * Time.deltaTime);
        }
    }
}
