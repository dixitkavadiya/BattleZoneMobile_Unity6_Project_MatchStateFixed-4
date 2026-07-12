using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public enum VehicleKind
    {
        Jeep,
        Buggy,
        Motorcycle,
        PickupTruck
    }

    public class VehicleController : MonoBehaviour, IDamageable
    {
        private static readonly List<VehicleController> ActiveVehicles = new List<VehicleController>();

        [Header("Identity")]
        [SerializeField] private VehicleKind kind = VehicleKind.Jeep;
        [SerializeField] private string displayName = "Jeep";

        [Header("Driving")]
        [SerializeField] private float maxSpeed = 18f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float brakeAcceleration = 24f;
        [SerializeField] private float turnSpeed = 78f;
        [SerializeField] private float steeringSharpness = 8.5f;
        [SerializeField] private float reverseSpeedRatio = 0.34f;
        [SerializeField] private float brakeTurnAssist = 1.18f;
        [SerializeField] private float coastDrag = 0.45f;
        [SerializeField] private float collisionDamageScale = 0.72f;
        [SerializeField] private float collisionBounce = 0.18f;
        [SerializeField] private float fuelBurnPerSecond = 2.1f;
        [SerializeField] private float suspensionHeight = 0.08f;
        [SerializeField] private float suspensionVisualTravel = 0.08f;
        [SerializeField] private float wheelSteerAngle = 28f;

        [Header("Runtime")]
        [SerializeField] private Transform seatPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private FloatingJoystick driveJoystick;
        [SerializeField] private MobileHoldButton throttleButton;
        [SerializeField] private MobileHoldButton brakeButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Transform bodyVisual;
        [SerializeField] private Transform[] wheelVisuals;

        [Header("Health")]
        [SerializeField] private float maxHealth = 320f;
        [SerializeField] private float maxFuel = 100f;

        private ThirdPersonMobileController driverController;
        private WeaponController driverWeapons;
        private Renderer[] renderers;
        private readonly RaycastHit[] groundProbeHits = new RaycastHit[8];
        private Vector3 bodyBaseLocalPosition;
        private Vector3[] wheelBaseLocalPositions;
        private float currentHealth;
        private float currentFuel;
        private float currentSpeed;
        private float currentSteer;
        private float wheelSpin;
        private bool destroyed;

        public bool IsAlive => !destroyed && currentHealth > 0f;
        public bool IsOccupied => driverController != null;
        public string DisplayName => displayName;
        public float Fuel01 => maxFuel <= 0f ? 0f : Mathf.Clamp01(currentFuel / maxFuel);

        public static VehicleController FindNearestAvailable(Vector3 position, float range)
        {
            VehicleController best = null;
            float bestDistance = range * range;
            for (int i = 0; i < ActiveVehicles.Count; i++)
            {
                VehicleController candidate = ActiveVehicles[i];
                if (candidate == null || !candidate.IsAlive || candidate.IsOccupied)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(candidate.transform.position - position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        public void ConfigureForRuntime(
            VehicleKind vehicleKind,
            string runtimeDisplayName,
            float runtimeMaxSpeed,
            FloatingJoystick joystick,
            MobileHoldButton throttle,
            MobileHoldButton brake,
            Text vehicleStatus)
        {
            kind = vehicleKind;
            displayName = runtimeDisplayName;
            maxSpeed = Mathf.Max(4f, runtimeMaxSpeed);
            driveJoystick = joystick;
            throttleButton = throttle;
            brakeButton = brake;
            statusText = vehicleStatus;

            switch (kind)
            {
                case VehicleKind.Buggy:
                    acceleration = 22f;
                    brakeAcceleration = 27f;
                    turnSpeed = 94f;
                    steeringSharpness = 10.5f;
                    suspensionVisualTravel = 0.12f;
                    wheelSteerAngle = 34f;
                    break;
                case VehicleKind.Motorcycle:
                    acceleration = 25f;
                    brakeAcceleration = 30f;
                    turnSpeed = 116f;
                    steeringSharpness = 12f;
                    suspensionVisualTravel = 0.09f;
                    wheelSteerAngle = 38f;
                    break;
                case VehicleKind.PickupTruck:
                    acceleration = 15f;
                    brakeAcceleration = 23f;
                    turnSpeed = 68f;
                    steeringSharpness = 7f;
                    suspensionVisualTravel = 0.1f;
                    wheelSteerAngle = 26f;
                    break;
                default:
                    acceleration = 18f;
                    brakeAcceleration = 24f;
                    turnSpeed = 80f;
                    steeringSharpness = 8.5f;
                    suspensionVisualTravel = 0.08f;
                    wheelSteerAngle = 28f;
                    break;
            }
        }

        public void ConfigureSeatPoints(Transform seat, Transform exit)
        {
            seatPoint = seat;
            exitPoint = exit;
        }

        public void ConfigureVisuals(Transform body, Transform[] wheels)
        {
            bodyVisual = body;
            wheelVisuals = wheels;
            bodyBaseLocalPosition = bodyVisual != null ? bodyVisual.localPosition : Vector3.zero;
            if (wheelVisuals == null)
            {
                wheelBaseLocalPositions = null;
                return;
            }

            wheelBaseLocalPositions = new Vector3[wheelVisuals.Length];
            for (int i = 0; i < wheelVisuals.Length; i++)
            {
                wheelBaseLocalPositions[i] = wheelVisuals[i] != null ? wheelVisuals[i].localPosition : Vector3.zero;
            }
        }

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            ResetVehicle();
        }

        private void OnEnable()
        {
            if (!ActiveVehicles.Contains(this))
            {
                ActiveVehicles.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveVehicles.Remove(this);
        }

        private void Update()
        {
            if (!IsOccupied)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeAcceleration * coastDrag * Time.deltaTime);
                currentSteer = Mathf.MoveTowards(currentSteer, 0f, steeringSharpness * Time.deltaTime);
                UpdateVehicleVisuals();
                UpdateStatus(false);
                return;
            }

            DrivePlayerVehicle();
            UpdateVehicleVisuals();
            UpdateStatus(true);
        }

        public void ResetVehicle()
        {
            currentHealth = maxHealth;
            currentFuel = maxFuel;
            currentSpeed = 0f;
            currentSteer = 0f;
            wheelSpin = 0f;
            destroyed = false;
        }

        public bool TryEnter(ThirdPersonMobileController controller, WeaponController weapons)
        {
            if (controller == null || !IsAlive || IsOccupied || seatPoint == null)
            {
                return false;
            }

            driverController = controller;
            driverWeapons = weapons;
            driverWeapons?.SetControlsEnabled(false);
            driverController.EnterVehicle(seatPoint);
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        public void Exit()
        {
            if (driverController == null)
            {
                return;
            }

            Vector3 exitPosition = exitPoint != null ? exitPoint.position : transform.position + transform.right * 2.2f + Vector3.up * 0.2f;
            Quaternion exitRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            driverController.ExitVehicle(exitPosition, exitRotation);
            driverWeapons?.SetControlsEnabled(true);
            driverController = null;
            driverWeapons = null;
            currentSpeed = 0f;
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
            {
                destroyed = true;
                Exit();
                TintVehicle(new Color(0.12f, 0.12f, 0.12f));
                RuntimeAudioBank.Instance?.PlayDeath(transform.position);
            }
        }

        public void BotDriveToward(Vector3 destination, float deltaTime)
        {
            if (!IsAlive || IsOccupied || currentFuel <= 0f)
            {
                return;
            }

            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 1f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * deltaTime);
            float distance = direction.magnitude;
            float desiredSpeed = maxSpeed * Mathf.Lerp(0.36f, 0.65f, Mathf.Clamp01(distance / 18f));
            currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * deltaTime);
            Vector3 move = transform.forward * currentSpeed * deltaTime;
            if (!Physics.Raycast(transform.position + Vector3.up * 0.7f, move.normalized, out RaycastHit hit, Mathf.Abs(currentSpeed) * deltaTime + 0.8f, ~0, QueryTriggerInteraction.Ignore) ||
                hit.collider.transform.root == transform.root)
            {
                transform.position += move;
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeAcceleration * 1.5f * deltaTime);
            }

            currentFuel = Mathf.Max(0f, currentFuel - fuelBurnPerSecond * 0.55f * deltaTime);
            SnapToGround();
        }

        private void DrivePlayerVehicle()
        {
            float throttle = throttleButton != null && throttleButton.IsPressed ? 1f : 0f;
            float brake = brakeButton != null && brakeButton.IsPressed ? 1f : 0f;
            float steer = 0f;

            if (driveJoystick != null)
            {
                Vector2 input = driveJoystick.Value;
                throttle = Mathf.Max(throttle, Mathf.Clamp01(input.y));
                brake = Mathf.Max(brake, Mathf.Clamp01(-input.y));
                steer = input.x;
            }

            if (Input.GetKey(KeyCode.W))
            {
                throttle = 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                brake = 1f;
            }

            steer += Input.GetAxisRaw("Horizontal");
            steer = Mathf.Clamp(steer, -1f, 1f);
            currentSteer = Mathf.MoveTowards(currentSteer, steer, steeringSharpness * Time.deltaTime);

            if (currentFuel <= 0f)
            {
                throttle = 0f;
            }

            float targetSpeed;
            float moveRate;
            if (throttle > 0.05f)
            {
                targetSpeed = maxSpeed * throttle;
                moveRate = acceleration;
            }
            else if (brake > 0.05f && currentSpeed > 0.65f)
            {
                targetSpeed = 0f;
                moveRate = brakeAcceleration * brakeTurnAssist;
            }
            else if (brake > 0.05f)
            {
                targetSpeed = -maxSpeed * reverseSpeedRatio * brake;
                moveRate = brakeAcceleration;
            }
            else
            {
                targetSpeed = 0f;
                moveRate = brakeAcceleration * coastDrag;
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, moveRate * Time.deltaTime);

            if (Mathf.Abs(currentSpeed) > 0.2f)
            {
                float speed01 = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(1f, maxSpeed));
                float steeringAuthority = Mathf.Lerp(1.1f, 0.58f, speed01);
                if (brake > 0.05f && currentSpeed > 0.65f)
                {
                    steeringAuthority *= brakeTurnAssist;
                }

                transform.Rotate(Vector3.up, currentSteer * turnSpeed * steeringAuthority * Mathf.Sign(currentSpeed) * Time.deltaTime, Space.World);
            }

            Vector3 move = transform.forward * currentSpeed * Time.deltaTime;
            if (move.sqrMagnitude <= 0.0001f)
            {
                SnapToGround();
                return;
            }

            if (!Physics.Raycast(transform.position + Vector3.up * 0.7f, move.normalized, out RaycastHit hit, Mathf.Abs(currentSpeed) * Time.deltaTime + 1f, ~0, QueryTriggerInteraction.Ignore) ||
                hit.collider.transform.root == transform.root)
            {
                transform.position += move;
            }
            else
            {
                TakeDamage(Mathf.Abs(currentSpeed) * collisionDamageScale, hit.point, hit.normal, gameObject);
                Vector3 slide = Vector3.ProjectOnPlane(move, hit.normal) * 0.35f;
                if (slide.sqrMagnitude > 0.0001f)
                {
                    transform.position += slide;
                }

                currentSpeed = -Mathf.Sign(currentSpeed) * Mathf.Min(Mathf.Abs(currentSpeed) * collisionBounce, maxSpeed * 0.16f);
            }

            currentFuel = Mathf.Max(0f, currentFuel - Mathf.Abs(currentSpeed / Mathf.Max(1f, maxSpeed)) * fuelBurnPerSecond * Time.deltaTime);
            SnapToGround();
        }

        private void UpdateVehicleVisuals()
        {
            wheelSpin += currentSpeed * 68f * Time.deltaTime;
            if (bodyVisual != null)
            {
                float speed01 = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(1f, maxSpeed));
                float lean = -currentSteer * speed01 * 5.5f;
                float pitch = Mathf.Clamp(-currentSpeed / Mathf.Max(1f, maxSpeed) * 2.2f, -2.2f, 2.2f);
                float bob = Mathf.Sin(Time.time * Mathf.Lerp(4f, 13f, speed01)) * suspensionVisualTravel * speed01;
                bodyVisual.localPosition = Vector3.Lerp(bodyVisual.localPosition, bodyBaseLocalPosition + Vector3.up * bob, 9f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Slerp(bodyVisual.localRotation, Quaternion.Euler(pitch, 0f, lean), 10f * Time.deltaTime);
            }

            if (wheelVisuals == null)
            {
                return;
            }

            for (int i = 0; i < wheelVisuals.Length; i++)
            {
                Transform wheel = wheelVisuals[i];
                if (wheel == null)
                {
                    continue;
                }

                bool frontWheel = kind == VehicleKind.Motorcycle ? i == 0 : i < 2;
                float steerVisual = frontWheel ? currentSteer * wheelSteerAngle : 0f;
                float compression = Mathf.Sin(Time.time * 16f + i * 1.7f) * suspensionVisualTravel * 0.42f * Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(1f, maxSpeed));
                if (wheelBaseLocalPositions != null && i < wheelBaseLocalPositions.Length)
                {
                    wheel.localPosition = Vector3.Lerp(wheel.localPosition, wheelBaseLocalPositions[i] + Vector3.up * compression, 12f * Time.deltaTime);
                }

                wheel.localRotation = Quaternion.Euler(wheelSpin, steerVisual, 90f);
            }
        }

        private void SnapToGround()
        {
            Vector3 origin = transform.position + Vector3.up * 8f;
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, groundProbeHits, 30f, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(groundProbeHits, 0, hitCount, RaycastHitDistanceComparer.Instance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = groundProbeHits[i];
                if (hit.collider != null && hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                float targetY = hit.point.y + suspensionHeight;
                float y = Application.isPlaying ? Mathf.Lerp(transform.position.y, targetY, 14f * Time.deltaTime) : targetY;
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                return;
            }
        }

        private void UpdateStatus(bool forceVisible)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.enabled = forceVisible;
            if (forceVisible)
            {
                statusText.text = $"{displayName} | HP {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)} | Fuel {Mathf.CeilToInt(Fuel01 * 100f)}%";
            }
        }

        private void TintVehicle(Color color)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer item in renderers)
            {
                if (item != null && item.material != null && item.material.HasProperty("_Color"))
                {
                    item.material.color = color;
                }
            }
        }

        private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();

            public int Compare(RaycastHit left, RaycastHit right)
            {
                return left.distance.CompareTo(right.distance);
            }
        }
    }
}
