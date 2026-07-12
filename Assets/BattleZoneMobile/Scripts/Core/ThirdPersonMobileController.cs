using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonMobileController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private FloatingJoystick movementJoystick;
        [SerializeField] private MobileLookArea lookArea;
        [SerializeField] private bool keyboardAndMouseFallback = true;

        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float cameraDistance = 4.8f;
        [SerializeField] private float aimCameraDistance = 3.1f;
        [SerializeField] private float standingCameraHeight = 1.6f;
        [SerializeField] private float crouchingCameraHeight = 1.05f;
        [SerializeField] private float shoulderOffset = 0.58f;
        [SerializeField] private float cameraCollisionRadius = 0.2f;
        [SerializeField] private LayerMask cameraCollisionMask = ~0;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float cameraFollowSharpness = 18f;
        [SerializeField] private float cameraZoomSharpness = 12f;
        [SerializeField] private float normalFieldOfView = 62f;
        [SerializeField] private float aimFieldOfView = 48f;
        [SerializeField] private float skydivingFieldOfView = 72f;
        [SerializeField] private float parachuteFieldOfView = 66f;
        [SerializeField] private float skydivingCameraDistance = 7.4f;
        [SerializeField] private float parachuteCameraDistance = 6.2f;
        [SerializeField] private float aimSensitivityScale = 0.72f;
        [SerializeField] private float scopeSensitivityScale = 0.52f;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 6.8f;
        [SerializeField] private float crouchSpeed = 2.4f;
        [SerializeField] private float proneSpeed = 1.25f;
        [SerializeField] private float acceleration = 14f;
        [SerializeField] private float sprintAcceleration = 18f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float airAcceleration = 6.5f;
        [SerializeField] private float airDeceleration = 4.8f;
        [SerializeField] private float jumpHeight = 1.3f;
        [SerializeField] private float jumpForwardBoost = 0.45f;
        [SerializeField] private float landingVelocityRetention = 0.84f;
        [SerializeField] private float jumpBufferTime = 0.14f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpCutGravityMultiplier = 1.75f;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float rotationSharpness = 16f;
        [SerializeField] private float footstepInterval = 0.38f;
        [SerializeField] private float inputDeadZone = 0.08f;

        [Header("Crouch")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.15f;
        [SerializeField] private float proneHeight = 0.72f;
        [SerializeField] private float crouchLerpSpeed = 12f;

        [Header("Optional")]
        [SerializeField] private Animator animator;
        [SerializeField] private HumanoidPlaceholderAnimator humanoidAnimator;

        [Header("Animator Diagnostics")]
        [SerializeField] private bool showAnimatorDebug = false;

        private CharacterController characterController;
        private readonly HashSet<int> animatorParameterHashes = new HashSet<int>();
        private readonly RaycastHit[] surfaceProbeHits = new RaycastHit[4];
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float yaw;
        private float pitch = 12f;
        private float recoilPitch;
        private float recoilRecoverySpeed = 16f;
        private float currentCameraDistance;
        private float currentShoulderOffset;
        private float animationMoveSpeed;
        private float animationMoveSpeedVelocity;
        private float sensitivityScale = 1f;
        private bool rightShoulder = true;
        private bool sprintHeld;
        private bool aimHeld;
        private bool aimButtonHeld;
        private bool crouching;
        private bool prone;
        private bool jumpQueued;
        private bool jumpHeld;
        private float jumpHoldTimer;
        private float jumpBufferTimer;
        private float coyoteTimer;
        private bool controlsEnabled = true;
        private bool externalMotionLock;
        [SerializeField] private bool externalGroundMovementDriverActive;
        private bool dropCameraActive;
        private bool parachuteCameraActive;
        private bool vehicleMode;
        private Transform vehicleSeat;
        private bool grounded;
        private bool sprinting;
        private float footstepTimer;
        private float landingRecoveryUntilTime;
        private float nextDependencyRepairTime;
        private Vector2 debugKeyboardInput;
        private Vector2 debugJoystickInput;
        private Vector2 debugFinalMoveInput;
        private float debugMoveSpeed;
        private float debugAnimatorSpeed;
        private float debugAnimatorVerticalVelocity;
        private bool debugAnimatorFalling;
        private bool animatorParameterCacheValid;
        private ReliablePlayerMovement reliableMovementOverride;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int SprintingHash = Animator.StringToHash("Sprinting");
        private static readonly int CrouchingHash = Animator.StringToHash("Crouching");
        private static readonly int ProneHash = Animator.StringToHash("Prone");
        private static readonly int AimingHash = Animator.StringToHash("Aiming");
        private static readonly int FallingHash = Animator.StringToHash("Falling");
        private static readonly int FireHash = Animator.StringToHash("Fire");
        private static readonly int ReloadHash = Animator.StringToHash("Reload");
        private static readonly int LandingHash = Animator.StringToHash("Landing");

        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set => controlsEnabled = value;
        }

        public bool IsAiming => aimHeld;
        public bool IsProne => prone;
        public bool IsInVehicle => vehicleMode;
        public bool IsExternalMotionLocked => externalMotionLock;
        public float CurrentYaw => yaw;
        public Vector2 DebugKeyboardInput => debugKeyboardInput;
        public Vector2 DebugJoystickInput => debugJoystickInput;
        public Vector2 DebugFinalMoveInput => debugFinalMoveInput;
        public float DebugMoveSpeed => debugMoveSpeed;
        public bool DebugGrounded => grounded;
        public bool DebugControlsEnabled => controlsEnabled && !externalMotionLock && !vehicleMode;
        public bool DebugHasJoystick => movementJoystick != null;
        public bool DebugHasCharacterController => characterController != null && characterController.enabled;
        public bool DebugExternalGroundMovementDriverActive => externalGroundMovementDriverActive;

        public void ConfigureForRuntime(FloatingJoystick joystick, MobileLookArea lookInput, Camera camera, Transform pivot)
        {
            movementJoystick = joystick;
            lookArea = lookInput;
            playerCamera = camera;
            cameraPivot = pivot;
            EnsureRuntimeMovementDependencies(true);
        }

        public void SetHumanoidAnimator(HumanoidPlaceholderAnimator placeholderAnimator)
        {
            humanoidAnimator = placeholderAnimator;
        }

        public void SetUnityAnimator(Animator visualAnimator)
        {
            animator = visualAnimator;
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            animatorParameterCacheValid = false;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            ClampMovementSettings();

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (cameraPivot == null)
            {
                GameObject pivot = new GameObject("Camera Pivot");
                pivot.transform.SetParent(transform);
                pivot.transform.localPosition = Vector3.up * standingCameraHeight;
                cameraPivot = pivot.transform;
            }

            yaw = transform.eulerAngles.y;
            currentCameraDistance = cameraDistance;
            currentShoulderOffset = shoulderOffset;
            characterController.height = standingHeight;
            characterController.center = Vector3.up * (standingHeight * 0.5f);
            EnsureRuntimeMovementDependencies(true);
        }

        private void Update()
        {
            EnsureRuntimeMovementDependencies(false);
            RefreshMovementDebugSnapshot();

            if (vehicleMode)
            {
                UpdateVehicleSeatPose();
                UpdateCamera();
                return;
            }

            if (!controlsEnabled)
            {
                if (!externalMotionLock && !externalGroundMovementDriverActive)
                {
                    ApplyGravityOnly();
                }
                else
                {
                    ReadLook();
                    horizontalVelocity = Vector3.zero;
                    verticalVelocity = groundedStickVelocity;
                }

                UpdateCamera();
                return;
            }

            ReadLook();
            if (externalGroundMovementDriverActive)
            {
                UpdateExternalGroundMovementDriverState();
            }
            else
            {
                Move();
                UpdateCrouch();
            }

            UpdateCamera();
            UpdateAnimator();
        }

        public void Jump()
        {
            if (prone)
            {
                prone = false;
                crouching = false;
                return;
            }

            jumpQueued = true;
            jumpHoldTimer = 0.18f;
        }

        public void SetSprint(bool value)
        {
            sprintHeld = value;
        }

        public void SetAim(bool value)
        {
            aimButtonHeld = value;
        }

        public void SetSensitivity(float normalizedValue)
        {
            sensitivityScale = Mathf.Lerp(0.55f, 1.75f, Mathf.Clamp01(normalizedValue));
        }

        public void SetAimSensitivity(float normalizedValue)
        {
            aimSensitivityScale = Mathf.Lerp(0.42f, 0.95f, Mathf.Clamp01(normalizedValue));
        }

        public void SetScopeSensitivity(float normalizedValue)
        {
            scopeSensitivityScale = Mathf.Lerp(0.26f, 0.78f, Mathf.Clamp01(normalizedValue));
        }

        public void SetCameraCollisionMask(LayerMask mask)
        {
            cameraCollisionMask = mask;
        }

        public void SetAimFieldOfView(float fov)
        {
            aimFieldOfView = Mathf.Clamp(fov, 32f, normalFieldOfView);
        }

        public void PlayLandingRecovery(float seconds)
        {
            landingRecoveryUntilTime = Time.time + Mathf.Clamp(seconds, 0f, 1.5f);
            sprintHeld = false;
            TriggerLandingAnimation();
        }

        public void TriggerFireAnimation()
        {
            humanoidAnimator?.TriggerFire();
            SetAnimatorTrigger(FireHash);
        }

        public void TriggerReloadAnimation()
        {
            humanoidAnimator?.TriggerReload();
            SetAnimatorTrigger(ReloadHash);
        }

        public void TriggerWeaponSwitchAnimation()
        {
            humanoidAnimator?.TriggerWeaponSwitch();
        }

        public void TriggerLandingAnimation()
        {
            humanoidAnimator?.TriggerLanding();
            SetAnimatorTrigger(LandingHash);
        }

        public void SetDropCameraMode(bool active, bool parachute)
        {
            dropCameraActive = active;
            parachuteCameraActive = active && parachute;
            if (active)
            {
                aimHeld = false;
                aimButtonHeld = false;
                pitch = Mathf.Clamp(pitch, -4f, 34f);
            }
            else
            {
                humanoidAnimator?.SetDropState(false, false, Vector3.zero);
            }
        }

        public void SetDropAnimationState(bool active, bool parachute, Vector3 velocity)
        {
            humanoidAnimator?.SetDropState(active, parachute, velocity);
        }

        public void AddRecoil(float pitchKick, float yawKick)
        {
            AddRecoil(pitchKick, yawKick, 16f);
        }

        public void AddRecoil(float pitchKick, float yawKick, float recoverySpeed)
        {
            recoilPitch = Mathf.Clamp(recoilPitch + Mathf.Abs(pitchKick), 0f, 8f);
            yaw += yawKick;
            recoilRecoverySpeed = Mathf.Clamp(recoverySpeed, 8f, 28f);
        }

        public void SetCrouch(bool value)
        {
            crouching = value;
            if (crouching)
            {
                prone = false;
            }
        }

        public void ToggleCrouch()
        {
            crouching = !crouching;
            if (crouching)
            {
                prone = false;
            }
        }

        public void ToggleProne()
        {
            prone = !prone;
            if (prone)
            {
                crouching = false;
                sprintHeld = false;
            }
        }

        public void ToggleShoulder()
        {
            rightShoulder = !rightShoulder;
        }

        public void SetExternalGroundMovementDriver(bool active)
        {
            externalGroundMovementDriverActive = active;
            if (externalGroundMovementDriverActive)
            {
                horizontalVelocity = Vector3.zero;
                verticalVelocity = groundedStickVelocity;
                sprinting = false;
                jumpQueued = false;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                footstepTimer = 0f;
            }
        }

        public void SetExternalMotionLock(bool locked)
        {
            externalMotionLock = locked;
            if (externalMotionLock)
            {
                horizontalVelocity = Vector3.zero;
                verticalVelocity = groundedStickVelocity;
                sprintHeld = false;
                aimHeld = false;
                aimButtonHeld = false;
                jumpQueued = false;
                if (!dropCameraActive)
                {
                    humanoidAnimator?.SetDropState(false, false, Vector3.zero);
                }
            }
        }

        public Vector2 ReadExternalMovementInput()
        {
            return ReadMovementInput();
        }

        public void RefreshMovementDebugSnapshot()
        {
            Vector2 input = ReadMovementInput();
            bool keyboardSprint = keyboardAndMouseFallback && ReadKey(KeyCode.LeftShift);
            debugMoveSpeed = ResolveTargetMoveSpeed(input, keyboardSprint);
        }

        public void SetExternalPose(Vector3 position, Quaternion rotation)
        {
            SetExternalPose(position, rotation, false);
        }

        public void SetExternalPose(Vector3 position, Quaternion rotation, bool preserveCameraYaw)
        {
            transform.SetPositionAndRotation(position, rotation);
            if (!preserveCameraYaw)
            {
                yaw = rotation.eulerAngles.y;
            }

            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickVelocity;
        }

        private bool TryGetReliableMovementOverride(out ReliablePlayerMovement reliableMovement)
        {
            if (reliableMovementOverride == null)
            {
                reliableMovementOverride = GetComponent<ReliablePlayerMovement>();
            }

            reliableMovement = reliableMovementOverride;
            return reliableMovement != null && reliableMovement.enabled;
        }

        private bool IsReliableMovementDrivingRoot()
        {
            return TryGetReliableMovementOverride(out _);
        }

        public void ResetController(Vector3 position, Quaternion rotation)
        {
            bool reliablePoseApplied = TryGetReliableMovementOverride(out ReliablePlayerMovement reliableMovement);
            if (reliablePoseApplied)
            {
                reliableMovement.SetAuthoritativePose(position, rotation);
            }
            else
            {
                characterController.enabled = false;
                transform.SetParent(null, true);
                transform.SetPositionAndRotation(position, rotation);
                characterController.enabled = true;
            }

            yaw = rotation.eulerAngles.y;
            pitch = 12f;
            recoilPitch = 0f;
            recoilRecoverySpeed = 16f;
            currentCameraDistance = cameraDistance;
            currentShoulderOffset = shoulderOffset;
            verticalVelocity = groundedStickVelocity;
            horizontalVelocity = Vector3.zero;
            animationMoveSpeed = 0f;
            animationMoveSpeedVelocity = 0f;
            sprintHeld = false;
            aimHeld = false;
            aimButtonHeld = false;
            crouching = false;
            prone = false;
            jumpQueued = false;
            jumpHeld = false;
            jumpHoldTimer = 0f;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            controlsEnabled = true;
            vehicleMode = false;
            vehicleSeat = null;
            externalMotionLock = false;
            dropCameraActive = false;
            parachuteCameraActive = false;
            characterController.enabled = true;
            humanoidAnimator?.Revive();
        }

        public void EnterVehicle(Transform seat)
        {
            if (seat == null)
            {
                return;
            }

            vehicleSeat = seat;
            vehicleMode = true;
            controlsEnabled = false;
            externalMotionLock = false;
            sprintHeld = false;
            aimHeld = false;
            aimButtonHeld = false;
            crouching = false;
            prone = false;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickVelocity;
            characterController.enabled = false;
            transform.SetParent(seat, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void ExitVehicle(Vector3 position, Quaternion rotation)
        {
            transform.SetParent(null, true);
            vehicleSeat = null;
            vehicleMode = false;
            controlsEnabled = true;
            externalMotionLock = false;
            characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            characterController.enabled = true;
            yaw = rotation.eulerAngles.y;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickVelocity;
        }

        private void ReadLook()
        {
            Vector2 look = lookArea != null ? lookArea.ConsumeLookDelta() : Vector2.zero;

            if (keyboardAndMouseFallback && ReadMouseButton(1))
            {
                look += new Vector2(ReadLegacyAxisRaw("Mouse X"), ReadLegacyAxisRaw("Mouse Y")) * 16f;
            }

            float zoomScale = aimHeld && aimFieldOfView <= 38f ? scopeSensitivityScale : aimHeld ? aimSensitivityScale : 1f;
            float scaledSensitivity = lookSensitivity * sensitivityScale * zoomScale;
            yaw += look.x * scaledSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * scaledSensitivity, minPitch, maxPitch);

            recoilPitch = Mathf.MoveTowards(recoilPitch, 0f, recoilRecoverySpeed * Time.deltaTime);
        }

        private void Move()
        {
            if (characterController == null)
            {
                return;
            }

            if (!characterController.enabled && !vehicleMode)
            {
                characterController.enabled = true;
            }

            Vector2 input = ReadMovementInput();
            bool wasGrounded = grounded;
            bool keyboardSprint = false;

            if (keyboardAndMouseFallback)
            {
                if (ReadKeyDown(KeyCode.Space))
                {
                    Jump();
                }

                jumpHeld = ReadKey(KeyCode.Space) || jumpHoldTimer > 0f;
                keyboardSprint = ReadKey(KeyCode.LeftShift);
                if (ReadKeyDown(KeyCode.LeftControl))
                {
                    ToggleCrouch();
                }

                if (ReadKeyDown(KeyCode.Z))
                {
                    ToggleProne();
                }

                if (ReadKeyDown(KeyCode.V))
                {
                    ToggleShoulder();
                }
            }
            else
            {
                jumpHeld = jumpHoldTimer > 0f;
            }

            jumpHoldTimer = Mathf.Max(0f, jumpHoldTimer - Time.deltaTime);

            aimHeld = aimButtonHeld || (keyboardAndMouseFallback && ReadMouseButton(1));

            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 desiredDirection = yawRotation * new Vector3(input.x, 0f, input.y);
            desiredDirection = Vector3.ClampMagnitude(desiredDirection, 1f);
            float inputStrength = Mathf.Clamp01(input.magnitude);

            sprinting = (sprintHeld || keyboardSprint) && input.y > 0.25f && !aimHeld && !prone;
            if (Time.time < landingRecoveryUntilTime)
            {
                sprinting = false;
            }

            float targetSpeed = ResolveTargetMoveSpeed(input, keyboardSprint);
            if (Time.time < landingRecoveryUntilTime)
            {
                targetSpeed = Mathf.Min(targetSpeed, walkSpeed * 0.42f);
            }
            debugMoveSpeed = targetSpeed;

            Vector3 desiredVelocity = desiredDirection * (targetSpeed * inputStrength);

            grounded = characterController.isGrounded;
            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            if (grounded)
            {
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
            }

            if (jumpQueued)
            {
                jumpBufferTimer = jumpBufferTime;
            }
            else
            {
                jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
            }

            if (jumpBufferTimer > 0f && coyoteTimer > 0f && !crouching && !prone)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (desiredDirection.sqrMagnitude > 0.05f)
                {
                    horizontalVelocity += desiredDirection.normalized * jumpForwardBoost;
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, sprintSpeed * 1.08f);
                }

                grounded = false;
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
            }

            float moveAcceleration = SelectAccelerationRate(desiredVelocity, sprinting);
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredVelocity, 1f - Mathf.Exp(-moveAcceleration * Time.deltaTime));

            jumpQueued = false;
            float gravityScale = !grounded && verticalVelocity > 0f && !jumpHeld ? jumpCutGravityMultiplier : 1f;
            verticalVelocity += gravity * gravityScale * Time.deltaTime;

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
            grounded = characterController.isGrounded;
            if (!wasGrounded && grounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
                horizontalVelocity *= landingVelocityRetention;
                TriggerLandingAnimation();
            }

            UpdateFootsteps();
            RotateCharacter(desiredDirection);
        }

        private void UpdateExternalGroundMovementDriverState()
        {
            if (characterController == null)
            {
                return;
            }

            if (!characterController.enabled && !vehicleMode)
            {
                characterController.enabled = true;
            }

            Vector2 input = ReadMovementInput();
            bool keyboardSprint = keyboardAndMouseFallback && ReadKey(KeyCode.LeftShift);
            aimHeld = aimButtonHeld || (keyboardAndMouseFallback && ReadMouseButton(1));
            sprinting = false;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickVelocity;
            jumpQueued = false;
            jumpHeld = false;
            jumpHoldTimer = 0f;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            footstepTimer = 0f;
            grounded = characterController.isGrounded;
            debugMoveSpeed = ResolveTargetMoveSpeed(input, keyboardSprint);

            if (keyboardAndMouseFallback && ReadKeyDown(KeyCode.V))
            {
                ToggleShoulder();
            }
        }

        private Vector2 ReadMovementInput()
        {
            debugJoystickInput = movementJoystick != null ? movementJoystick.Value : Vector2.zero;
            debugKeyboardInput = keyboardAndMouseFallback ? ReadKeyboardMovementInput() : Vector2.zero;

            Vector2 selected = debugJoystickInput.sqrMagnitude >= debugKeyboardInput.sqrMagnitude
                ? debugJoystickInput
                : debugKeyboardInput;

            debugFinalMoveInput = ApplyInputDeadZone(Vector2.ClampMagnitude(selected, 1f));
            return debugFinalMoveInput;
        }

        private Vector2 ReadKeyboardMovementInput()
        {
            Vector2 axisInput = new Vector2(ReadLegacyAxisRaw("Horizontal"), ReadLegacyAxisRaw("Vertical"));
            Vector2 keyInput = Vector2.zero;

            if (ReadKey(KeyCode.A) || ReadKey(KeyCode.LeftArrow))
            {
                keyInput.x -= 1f;
            }

            if (ReadKey(KeyCode.D) || ReadKey(KeyCode.RightArrow))
            {
                keyInput.x += 1f;
            }

            if (ReadKey(KeyCode.S) || ReadKey(KeyCode.DownArrow))
            {
                keyInput.y -= 1f;
            }

            if (ReadKey(KeyCode.W) || ReadKey(KeyCode.UpArrow))
            {
                keyInput.y += 1f;
            }

            keyInput = Vector2.ClampMagnitude(keyInput, 1f);
            return axisInput.sqrMagnitude >= keyInput.sqrMagnitude ? Vector2.ClampMagnitude(axisInput, 1f) : keyInput;
        }

        private float ResolveTargetMoveSpeed(Vector2 input, bool keyboardSprint)
        {
            bool wantsSprint = (sprintHeld || keyboardSprint) && input.y > 0.25f && !aimHeld && !prone;
            return prone ? proneSpeed : crouching ? crouchSpeed : wantsSprint ? sprintSpeed : walkSpeed;
        }

        private void EnsureRuntimeMovementDependencies(bool force)
        {
            if (!force && Time.unscaledTime < nextDependencyRepairTime)
            {
                return;
            }

            nextDependencyRepairTime = Time.unscaledTime + 0.5f;

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (characterController != null)
            {
                if (!vehicleMode && !characterController.enabled)
                {
                    characterController.enabled = true;
                }

                characterController.detectCollisions = true;
                characterController.minMoveDistance = 0f;
            }

            if (movementJoystick == null)
            {
                movementJoystick = Object.FindAnyObjectByType<FloatingJoystick>(FindObjectsInactive.Include);
            }

            if (lookArea == null)
            {
                lookArea = Object.FindAnyObjectByType<MobileLookArea>(FindObjectsInactive.Include);
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (cameraPivot == null)
            {
                Transform existingPivot = transform.Find("CameraPivot") ?? transform.Find("Camera Pivot");
                if (existingPivot != null)
                {
                    cameraPivot = existingPivot;
                }
                else
                {
                    GameObject pivot = new GameObject("CameraPivot");
                    pivot.transform.SetParent(transform);
                    pivot.transform.localPosition = Vector3.up * standingCameraHeight;
                    cameraPivot = pivot.transform;
                }
            }

            gameObject.isStatic = false;
            ClampMovementSettings();
        }

        private void ClampMovementSettings()
        {
            walkSpeed = Mathf.Max(0.5f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed + 0.2f, sprintSpeed);
            crouchSpeed = Mathf.Clamp(crouchSpeed, 0.2f, walkSpeed);
            proneSpeed = Mathf.Clamp(proneSpeed, 0.1f, crouchSpeed);
            acceleration = Mathf.Max(1f, acceleration);
            sprintAcceleration = Mathf.Max(acceleration, sprintAcceleration);
            deceleration = Mathf.Max(1f, deceleration);
            airAcceleration = Mathf.Max(0.2f, airAcceleration);
            airDeceleration = Mathf.Max(0.2f, airDeceleration);
            jumpHeight = Mathf.Max(0.15f, jumpHeight);
            gravity = gravity >= -0.1f ? -30f : gravity;
            groundedStickVelocity = Mathf.Min(-0.1f, groundedStickVelocity);
            rotationSharpness = Mathf.Max(1f, rotationSharpness);
            standingHeight = Mathf.Max(1f, standingHeight);
            crouchingHeight = Mathf.Clamp(crouchingHeight, 0.5f, standingHeight - 0.05f);
            proneHeight = Mathf.Clamp(proneHeight, 0.35f, crouchingHeight - 0.05f);
        }

        private static float ReadLegacyAxisRaw(string axisName)
        {
            try
            {
                return Input.GetAxisRaw(axisName);
            }
            catch (System.Exception)
            {
                return 0f;
            }
        }

        private static bool ReadKey(KeyCode key)
        {
            try
            {
                return Input.GetKey(key);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool ReadKeyDown(KeyCode key)
        {
            try
            {
                return Input.GetKeyDown(key);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool ReadMouseButton(int button)
        {
            try
            {
                return Input.GetMouseButton(button);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private Vector2 ApplyInputDeadZone(Vector2 rawInput)
        {
            float magnitude = rawInput.magnitude;
            if (magnitude <= inputDeadZone)
            {
                return Vector2.zero;
            }

            float remappedMagnitude = Mathf.InverseLerp(inputDeadZone, 1f, Mathf.Min(1f, magnitude));
            return rawInput.normalized * remappedMagnitude;
        }

        private float SelectAccelerationRate(Vector3 desiredVelocity, bool isSprinting)
        {
            bool hasInput = desiredVelocity.sqrMagnitude > 0.0025f;
            if (!grounded)
            {
                return hasInput ? airAcceleration : airDeceleration;
            }

            if (!hasInput)
            {
                return deceleration;
            }

            float directionDot = horizontalVelocity.sqrMagnitude > 0.0025f
                ? Vector3.Dot(horizontalVelocity.normalized, desiredVelocity.normalized)
                : 1f;

            if (directionDot < 0.25f)
            {
                return deceleration;
            }

            return isSprinting ? sprintAcceleration : acceleration;
        }

        private void UpdateFootsteps()
        {
            Vector3 flatVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            if (!grounded || flatVelocity.magnitude < 0.45f)
            {
                footstepTimer = 0f;
                return;
            }

            float cadence = prone ? footstepInterval * 2.2f : crouching ? footstepInterval * 1.45f : sprinting ? footstepInterval * 0.68f : footstepInterval;
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= cadence)
            {
                footstepTimer = 0f;
                RuntimeAudioBank.Instance?.PlayFootstep(transform.position, DetectFootstepSurface());
            }
        }

        private string DetectFootstepSurface()
        {
            int hitCount = Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.35f, Vector3.down, surfaceProbeHits, 1.4f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = surfaceProbeHits[i];
                if (hit.collider == null || hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                string objectName = hit.collider.name;
                if (objectName.Contains("Water") || objectName.Contains("River") || objectName.Contains("Lake"))
                {
                    return "Water";
                }

                if (objectName.Contains("Road") || objectName.Contains("Bridge") || objectName.Contains("Sidewalk") || objectName.Contains("Crosswalk") || objectName.Contains("Dirt"))
                {
                    return "Road";
                }

                if (objectName.Contains("Floor") || objectName.Contains("Interior") || objectName.Contains("Building") || objectName.Contains("Warehouse") || objectName.Contains("Barracks") || objectName.Contains("Factory"))
                {
                    return "Building";
                }
            }

            return "Grass";
        }

        private void ApplyGravityOnly()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            verticalVelocity += gravity * Time.deltaTime;
            characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void RotateCharacter(Vector3 desiredDirection)
        {
            Vector3 facingDirection = desiredDirection.sqrMagnitude > 0.0025f ? desiredDirection : transform.forward;

            if (aimHeld)
            {
                facingDirection = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            }

            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude < 0.0025f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }

        private void UpdateCrouch()
        {
            float targetHeight = prone ? proneHeight : crouching ? crouchingHeight : standingHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchLerpSpeed * Time.deltaTime);
            characterController.center = Vector3.up * (characterController.height * 0.5f);
        }

        private void UpdateCamera()
        {
            if (playerCamera == null)
            {
                return;
            }

            float cameraHeight = dropCameraActive ? standingCameraHeight + (parachuteCameraActive ? 0.95f : 0.55f) : prone ? crouchingCameraHeight * 0.65f : crouching ? crouchingCameraHeight : standingCameraHeight;
            Vector3 pivotPosition = transform.position + Vector3.up * cameraHeight;
            cameraPivot.position = Vector3.Lerp(cameraPivot.position, pivotPosition, 1f - Mathf.Exp(-cameraFollowSharpness * Time.deltaTime));

            float targetDistance = dropCameraActive ? (parachuteCameraActive ? parachuteCameraDistance : skydivingCameraDistance) : aimHeld ? aimCameraDistance : cameraDistance;
            if (aimHeld && aimFieldOfView <= 38f)
            {
                targetDistance = Mathf.Min(targetDistance, 2.65f);
            }
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetDistance, 1f - Mathf.Exp(-cameraZoomSharpness * Time.deltaTime));
            float targetShoulderOffset = dropCameraActive ? 0f : (rightShoulder ? 1f : -1f) * (aimHeld ? shoulderOffset * 0.72f : shoulderOffset);
            currentShoulderOffset = Mathf.Lerp(currentShoulderOffset, targetShoulderOffset, 1f - Mathf.Exp(-cameraFollowSharpness * Time.deltaTime));

            Quaternion cameraRotation = Quaternion.Euler(Mathf.Clamp(pitch - recoilPitch, minPitch, maxPitch), yaw, 0f);
            Vector3 shoulderPosition = cameraPivot.position + cameraRotation * Vector3.right * currentShoulderOffset;
            Vector3 desiredPosition = shoulderPosition - cameraRotation * Vector3.forward * currentCameraDistance;
            Vector3 toDesired = desiredPosition - shoulderPosition;

            if (toDesired.sqrMagnitude > 0.0001f &&
                Physics.SphereCast(shoulderPosition, cameraCollisionRadius, toDesired.normalized, out RaycastHit hit, currentCameraDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = shoulderPosition + toDesired.normalized * Mathf.Max(0.35f, hit.distance - 0.18f);
            }

            playerCamera.transform.SetPositionAndRotation(desiredPosition, cameraRotation);
            float targetFov = dropCameraActive ? (parachuteCameraActive ? parachuteFieldOfView : skydivingFieldOfView) : aimHeld ? aimFieldOfView : normalFieldOfView;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, 1f - Mathf.Exp(-cameraZoomSharpness * Time.deltaTime));
        }

        private void UpdateAnimator()
        {
            Vector3 flatVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            animationMoveSpeed = Mathf.SmoothDamp(animationMoveSpeed, flatVelocity.magnitude, ref animationMoveSpeedVelocity, grounded ? 0.075f : 0.14f);
            float animatorVerticalVelocity = characterController.velocity.y;
            bool falling = !grounded && animatorVerticalVelocity < -0.35f;
            debugAnimatorSpeed = animationMoveSpeed;
            debugAnimatorVerticalVelocity = animatorVerticalVelocity;
            debugAnimatorFalling = falling;

            if (animator != null)
            {
                animator.applyRootMotion = false;
                SetAnimatorFloat(SpeedHash, animationMoveSpeed, 0.08f);
                SetAnimatorFloat(VerticalVelocityHash, animatorVerticalVelocity, 0.06f);
                SetAnimatorBool(GroundedHash, grounded);
                SetAnimatorBool(SprintingHash, sprinting);
                SetAnimatorBool(CrouchingHash, crouching);
                SetAnimatorBool(ProneHash, prone);
                SetAnimatorBool(AimingHash, aimHeld);
                SetAnimatorBool(FallingHash, falling);
            }

            if (humanoidAnimator != null)
            {
                humanoidAnimator.SetState(animationMoveSpeed, animatorVerticalVelocity, grounded, crouching, prone, sprinting, aimHeld, falling);
            }
        }

        private void CacheAnimatorParameters()
        {
            animatorParameterHashes.Clear();
            if (animator != null)
            {
                AnimatorControllerParameter[] parameters = animator.parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    animatorParameterHashes.Add(parameters[i].nameHash);
                }
            }

            animatorParameterCacheValid = true;
        }

        private bool HasAnimatorParameter(int parameterHash)
        {
            if (!animatorParameterCacheValid)
            {
                CacheAnimatorParameters();
            }

            return animatorParameterHashes.Contains(parameterHash);
        }

        private void SetAnimatorFloat(int parameterHash, float value, float dampTime)
        {
            if (animator != null && HasAnimatorParameter(parameterHash))
            {
                animator.SetFloat(parameterHash, value, dampTime, Time.deltaTime);
            }
        }

        private void SetAnimatorBool(int parameterHash, bool value)
        {
            if (animator != null && HasAnimatorParameter(parameterHash))
            {
                animator.SetBool(parameterHash, value);
            }
        }

        private void SetAnimatorTrigger(int parameterHash)
        {
            if (animator != null && HasAnimatorParameter(parameterHash))
            {
                animator.SetTrigger(parameterHash);
            }
        }

        private void UpdateVehicleSeatPose()
        {
            if (vehicleSeat == null)
            {
                return;
            }

            transform.SetPositionAndRotation(vehicleSeat.position, vehicleSeat.rotation);
            yaw = vehicleSeat.eulerAngles.y;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickVelocity;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showAnimatorDebug)
            {
                return;
            }

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.Box(new Rect(650f, 12f, 360f, 174f), GUIContent.none);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(660f, 20f, 340f, 158f));
            GUILayout.Label("Animator Diagnostics");
            GUILayout.Label($"visual Animator: {(animator != null ? animator.gameObject.name : "None")}");
            GUILayout.Label($"root motion: {(animator != null && animator.applyRootMotion ? "ON" : "OFF")}");
            GUILayout.Label($"speed: {debugAnimatorSpeed:0.00} | vertical: {debugAnimatorVerticalVelocity:0.00}");
            GUILayout.Label($"grounded: {grounded} | falling: {debugAnimatorFalling}");
            GUILayout.Label($"sprint: {sprinting} | crouch: {crouching} | prone: {prone} | aim: {aimHeld}");
            GUILayout.EndArea();
        }
#endif
    }
}
