using UnityEngine;

namespace BattleZoneMobile
{
    [RequireComponent(typeof(CharacterController))]
    public class ReliablePlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera movementCamera;
        [SerializeField] private FloatingJoystick movementJoystick;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float crouchSpeed = 2.35f;
        [SerializeField] private float rotationSharpness = 14f;
        [SerializeField] private float gravity = -28f;
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float jumpHeight = 1.35f;

        [Header("Crouch")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.12f;
        [SerializeField] private float heightLerpSpeed = 14f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundProbeDistance = 0.28f;

        [Header("Editor Debug")]
        [SerializeField] private bool showEditorDebug = true;

        private Vector2 keyboardInput;
        private Vector2 joystickInput;
        private Vector2 finalInput;
        private Vector3 finalMoveVector;
        private Vector3 positionBeforeMove;
        private Vector3 positionAfterMove;
        private Vector3 positionLateUpdate;
        private Vector3 characterControllerVelocity;
        private Vector3 latePositionDelta;
        private Vector3 emergencyPositionBefore;
        private Vector3 emergencyPositionAfter;
        private float verticalVelocity;
        private bool grounded;
        private bool sprintHeld;
        private bool mobileSprintHeld;
        private bool crouching;
        private bool jumpQueued;
        private int activeCharacterControllerCount;
        private int activeReliableMovementCount;
        private int activeOldMovementCount;
        private int activePlayerRootCount;
        private int lastLatePositionOverrideFrame = -1;
        private int lastEmergencyMoveFrame = -1;
        private ThirdPersonMobileController oldMovementController;
        private BattleRoyaleMatchFlow matchFlow;
        private bool gravitySuppressed;
        private bool wasGravitySuppressed;
        private string poseOwner = "ReliableGround";

        public Vector2 KeyboardInput => keyboardInput;
        public Vector2 JoystickInput => joystickInput;
        public Vector2 FinalInput => finalInput;
        public Vector3 FinalMoveVector => finalMoveVector;
        public Vector3 PositionBeforeMove => positionBeforeMove;
        public Vector3 PositionAfterMove => positionAfterMove;
        public Vector3 PositionLateUpdate => positionLateUpdate;
        public Vector3 CharacterControllerVelocity => characterControllerVelocity;
        public bool IsGrounded => grounded;
        public bool IsCrouching => crouching;
        public bool ControllerReady => enabled && characterController != null && characterController.enabled;
        public bool OwnsGroundMovement => enabled;
        public float DebugVerticalVelocity => verticalVelocity;
        public bool DebugGravitySuppressed => gravitySuppressed;
        public string DebugPoseOwner => poseOwner;

        public void ConfigureForRuntime(CharacterController controller, Camera camera, FloatingJoystick joystick)
        {
            characterController = controller;
            movementCamera = camera;
            movementJoystick = joystick;
            CacheControllerDefaults();
            RepairReferences();
        }

        public void SetSprint(bool value)
        {
            mobileSprintHeld = value;
        }

        public void ToggleCrouch()
        {
            crouching = !crouching;
        }

        public void Jump()
        {
            jumpQueued = true;
        }

        public void SetAuthoritativePose(Vector3 position, Quaternion rotation)
        {
            RepairReferences();
            bool controllerWasEnabled = characterController != null && characterController.enabled;
            if (characterController != null && controllerWasEnabled)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);

            if (characterController != null && controllerWasEnabled)
            {
                characterController.enabled = true;
            }

            verticalVelocity = ShouldSuppressGroundMovementForMatchFlow() ? 0f : groundedStickVelocity;
            finalMoveVector = Vector3.zero;
            positionBeforeMove = transform.position;
            positionAfterMove = transform.position;
            positionLateUpdate = transform.position;
            characterControllerVelocity = Vector3.zero;
        }

        public void ResetAfterMatchFlowPose()
        {
            verticalVelocity = groundedStickVelocity;
            jumpQueued = false;
            finalMoveVector = Vector3.zero;
            positionBeforeMove = transform.position;
            positionAfterMove = transform.position;
            positionLateUpdate = transform.position;
            characterControllerVelocity = Vector3.zero;
            gravitySuppressed = false;
            wasGravitySuppressed = false;
            poseOwner = "ReliableGround";
        }

        private void Awake()
        {
            RepairReferences();
            CacheControllerDefaults();
        }

        private void OnEnable()
        {
            RepairReferences();
            SetOldControllerGroundMovementBypass(true);
        }

        private void OnDisable()
        {
            SetOldControllerGroundMovementBypass(false);
        }

        private void Update()
        {
            RepairReferences();
            ReadInput();
            gravitySuppressed = ShouldSuppressGroundMovementForMatchFlow();
            poseOwner = gravitySuppressed ? "MatchFlowPose" : "ReliableGround";
            if (!gravitySuppressed && Input.GetKeyDown(KeyCode.F8))
            {
                EmergencyMoveForward();
            }

            if (gravitySuppressed)
            {
                SuppressGroundMovementForMatchFlowPose();
                wasGravitySuppressed = true;
                return;
            }

            if (wasGravitySuppressed)
            {
                ResetAfterMatchFlowPose();
            }

            UpdateCrouchHeight();
            MoveCharacter();
        }

        private void LateUpdate()
        {
            positionLateUpdate = transform.position;
            latePositionDelta = positionLateUpdate - positionAfterMove;
            if ((positionAfterMove - positionBeforeMove).sqrMagnitude > 0.000001f && latePositionDelta.sqrMagnitude > 0.000001f)
            {
                lastLatePositionOverrideFrame = Time.frameCount;
            }

#if UNITY_EDITOR
            RefreshControllerCounts();
#endif
        }

        private void RepairReferences()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (movementCamera == null)
            {
                movementCamera = Camera.main;
                if (movementCamera == null)
                {
                    movementCamera = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
                }
            }

            if (movementJoystick == null)
            {
                movementJoystick = Object.FindAnyObjectByType<FloatingJoystick>(FindObjectsInactive.Include);
            }

            if (matchFlow == null)
            {
                matchFlow = Object.FindAnyObjectByType<BattleRoyaleMatchFlow>(FindObjectsInactive.Include);
            }

            SetOldControllerGroundMovementBypass(enabled);
        }

        private bool ShouldSuppressGroundMovementForMatchFlow()
        {
            if (matchFlow == null)
            {
                return false;
            }

            return matchFlow.MatchFlowOwnsLocalPlayerPose;
        }

        private void SuppressGroundMovementForMatchFlowPose()
        {
            verticalVelocity = 0f;
            jumpQueued = false;
            finalMoveVector = Vector3.zero;
            positionBeforeMove = transform.position;
            positionAfterMove = transform.position;
            characterControllerVelocity = Vector3.zero;
            grounded = characterController != null && characterController.enabled && characterController.isGrounded;
        }

        private void SetOldControllerGroundMovementBypass(bool active)
        {
            if (oldMovementController == null)
            {
                oldMovementController = GetComponent<ThirdPersonMobileController>();
            }

            if (oldMovementController != null)
            {
                oldMovementController.SetExternalGroundMovementDriver(active);
            }
        }

        private void CacheControllerDefaults()
        {
            if (characterController == null)
            {
                return;
            }

            standingHeight = Mathf.Max(0.5f, characterController.height);
            crouchingHeight = Mathf.Clamp(crouchingHeight, 0.5f, standingHeight);
        }

        private void ReadInput()
        {
            keyboardInput = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                keyboardInput.x -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                keyboardInput.x += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                keyboardInput.y -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                keyboardInput.y += 1f;
            }

            keyboardInput = Vector2.ClampMagnitude(keyboardInput, 1f);
            joystickInput = movementJoystick != null ? movementJoystick.Value : Vector2.zero;
            finalInput = Vector2.ClampMagnitude(keyboardInput + joystickInput, 1f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpQueued = true;
            }

            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                crouching = !crouching;
            }

            bool keyboardSprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            sprintHeld = mobileSprintHeld || keyboardSprintHeld;
        }

        private void UpdateCrouchHeight()
        {
            if (characterController == null)
            {
                return;
            }

            float targetHeight = crouching ? crouchingHeight : standingHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, heightLerpSpeed * Time.deltaTime);
            characterController.center = Vector3.up * (characterController.height * 0.5f);
        }

        private void MoveCharacter()
        {
            finalMoveVector = Vector3.zero;
            positionBeforeMove = transform.position;
            positionAfterMove = positionBeforeMove;
            characterControllerVelocity = characterController != null ? characterController.velocity : Vector3.zero;
            if (characterController == null || !characterController.enabled)
            {
                jumpQueued = false;
                return;
            }

            grounded = CheckGrounded();
            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            if (jumpQueued && grounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                crouching = false;
            }

            jumpQueued = false;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 planarMove = BuildCameraRelativeMove(finalInput);
            float speed = crouching ? crouchSpeed : sprintHeld && finalInput.y > 0.2f ? sprintSpeed : walkSpeed;
            finalMoveVector = planarMove * speed;

            if (finalMoveVector.sqrMagnitude > 0.0025f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(finalMoveVector.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSharpness * Time.deltaTime);
            }

            Vector3 motion = finalMoveVector;
            motion.y = verticalVelocity;
            CollisionFlags flags = characterController.Move(motion * Time.deltaTime);
            positionAfterMove = transform.position;
            characterControllerVelocity = characterController.velocity;
            if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
                grounded = true;
            }
        }

        private void EmergencyMoveForward()
        {
            RepairReferences();
            if (characterController == null || !characterController.enabled)
            {
                Debug.LogWarning("ReliablePlayerMovement F8 emergency move failed because CharacterController is missing or disabled.");
                return;
            }

            emergencyPositionBefore = transform.position;
            characterController.Move(transform.forward * 3f);
            emergencyPositionAfter = transform.position;
            characterControllerVelocity = characterController.velocity;
            lastEmergencyMoveFrame = Time.frameCount;
            Debug.Log($"ReliablePlayerMovement F8 emergency move: {FormatVector(emergencyPositionBefore)} -> {FormatVector(emergencyPositionAfter)}");
        }

        private Vector3 BuildCameraRelativeMove(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Transform basis = movementCamera != null ? movementCamera.transform : transform;
            Vector3 forward = basis.forward;
            Vector3 right = basis.right;
            forward.y = 0f;
            right.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = transform.forward;
            }

            if (right.sqrMagnitude <= 0.0001f)
            {
                right = transform.right;
            }

            forward.Normalize();
            right.Normalize();
            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }

        private bool CheckGrounded()
        {
            if (characterController == null)
            {
                return false;
            }

            if (characterController.isGrounded)
            {
                return true;
            }

            Vector3 origin = transform.position + Vector3.up * 0.12f;
            float radius = Mathf.Max(0.08f, characterController.radius * 0.82f);
            return Physics.SphereCast(origin, radius, Vector3.down, out _, groundProbeDistance, groundMask, QueryTriggerInteraction.Ignore);
        }

#if UNITY_EDITOR
        private void RefreshControllerCounts()
        {
            activeCharacterControllerCount = 0;
            activeReliableMovementCount = 0;
            activeOldMovementCount = 0;
            activePlayerRootCount = 0;

            CharacterController[] characterControllers = Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Exclude);
            for (int i = 0; i < characterControllers.Length; i++)
            {
                if (characterControllers[i] != null && characterControllers[i].gameObject.CompareTag("Player"))
                {
                    activeCharacterControllerCount++;
                }
            }

            ReliablePlayerMovement[] reliableControllers = Object.FindObjectsByType<ReliablePlayerMovement>(FindObjectsInactive.Exclude);
            for (int i = 0; i < reliableControllers.Length; i++)
            {
                if (reliableControllers[i] != null && reliableControllers[i].enabled && reliableControllers[i].gameObject.CompareTag("Player"))
                {
                    activeReliableMovementCount++;
                }
            }

            ThirdPersonMobileController[] oldControllers = Object.FindObjectsByType<ThirdPersonMobileController>(FindObjectsInactive.Exclude);
            for (int i = 0; i < oldControllers.Length; i++)
            {
                if (oldControllers[i] != null && oldControllers[i].enabled && oldControllers[i].gameObject.CompareTag("Player"))
                {
                    activeOldMovementCount++;
                }
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i] != null && allObjects[i].CompareTag("Player"))
                {
                    activePlayerRootCount++;
                }
            }
        }

        private void OnGUI()
        {
            if (!showEditorDebug)
            {
                return;
            }

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.Box(new Rect(12f, 12f, 620f, 310f), GUIContent.none);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(20f, 18f, 604f, 300f));
            string phase = matchFlow != null ? matchFlow.CurrentPhase : "None";
            bool landed = matchFlow != null && matchFlow.DebugLocalPlayerHasLanded;
            bool oldControllerEnabled = oldMovementController != null && oldMovementController.enabled;
            bool oldGroundBypassed = oldMovementController != null && oldMovementController.DebugExternalGroundMovementDriverActive;
            GUILayout.Label("Reliable Player Movement");
            GUILayout.Label($"match phase: {phase} | local landed: {landed}");
            GUILayout.Label($"pose owner: {poseOwner} | gravity suppressed: {gravitySuppressed} | vertical velocity: {verticalVelocity:0.00}");
            GUILayout.Label($"reliable owns ground: {OwnsGroundMovement} | old enabled: {oldControllerEnabled} | old ground bypassed: {oldGroundBypassed}");
            GUILayout.Label($"keyboard input: {keyboardInput.x:0.00}, {keyboardInput.y:0.00}");
            GUILayout.Label($"joystick input: {joystickInput.x:0.00}, {joystickInput.y:0.00}");
            GUILayout.Label($"final movement vector: {finalMoveVector.x:0.00}, {finalMoveVector.y:0.00}, {finalMoveVector.z:0.00}");
            GUILayout.Label($"before Move: {FormatVector(positionBeforeMove)}");
            GUILayout.Label($"after Move:  {FormatVector(positionAfterMove)}");
            GUILayout.Label($"LateUpdate:  {FormatVector(positionLateUpdate)}");
            GUILayout.Label($"CC velocity: {FormatVector(characterControllerVelocity)}");
            GUILayout.Label($"late delta:  {FormatVector(latePositionDelta)} | override frame: {lastLatePositionOverrideFrame}");
            GUILayout.Label($"controller enabled: {ControllerReady} | grounded: {grounded} | time scale: {Time.timeScale:0.00}");
            GUILayout.Label($"active Player roots: {activePlayerRootCount} | CharacterControllers: {activeCharacterControllerCount} | Reliable: {activeReliableMovementCount} | Old enabled: {activeOldMovementCount}");
            GUILayout.Label($"F8 emergency: frame {lastEmergencyMoveFrame} | {FormatVector(emergencyPositionBefore)} -> {FormatVector(emergencyPositionAfter)}");
            GUILayout.EndArea();
        }
#endif

        private static string FormatVector(Vector3 value)
        {
            return $"{value.x:0.00}, {value.y:0.00}, {value.z:0.00}";
        }
    }
}
