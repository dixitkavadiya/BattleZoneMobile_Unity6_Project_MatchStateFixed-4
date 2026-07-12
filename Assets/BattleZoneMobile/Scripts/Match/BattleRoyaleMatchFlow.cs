using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleZoneMobile
{
    public class BattleRoyaleMatchFlow : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private ThirdPersonMobileController controller;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private BotManager botManager;
        [SerializeField] private Transform waitingAreaPoint;
        [SerializeField] private GameObject planeVisual;
        [SerializeField] private GameObject parachuteVisual;
        [SerializeField] private LootItem[] airdropLootPrefabs;
        [SerializeField] private Material crateMaterial;
        [SerializeField] private Vector3[] routeStarts;
        [SerializeField] private Vector3[] routeEnds;
        [SerializeField] private Vector2 mapHalfExtents = new Vector2(250f, 250f);
        [SerializeField] private float routeAltitude = 138f;
        [SerializeField] private float routeOvershoot = 34f;
        [SerializeField, Range(0f, 1f)] private float proceduralRouteChance = 0.72f;

        [Header("Timings")]
        [SerializeField] private float waitingCountdownSeconds = 5f;
        [SerializeField] private float aircraftRouteSeconds = 10f;
        [SerializeField] private float autoJumpRoutePercent = 0.88f;
        [SerializeField] private float freeFallVerticalSpeed = 34f;
        [SerializeField] private float freeFallSteerSpeed = 18f;
        [SerializeField] private float freeFallGravity = 42f;
        [SerializeField] private float freeFallTerminalSpeed = 58f;
        [SerializeField] private float freeFallGlideAcceleration = 28f;
        [SerializeField] private float freeFallMaxHorizontalSpeed = 26f;
        [SerializeField] private float parachuteDeployAltitude = 54f;
        [SerializeField] private float parachuteDescentSpeed = 9.5f;
        [SerializeField] private float parachuteSteerSpeed = 11f;
        [SerializeField] private float parachuteGravity = 15f;
        [SerializeField] private float parachuteTerminalSpeed = 12f;
        [SerializeField] private float parachuteOpenDamping = 34f;
        [SerializeField] private float parachuteTurnResponse = 18f;
        [SerializeField] private float windStrength = 2.2f;
        [SerializeField] private float landingRecoverySeconds = 0.55f;

        private readonly WaitForSeconds countdownWait = new WaitForSeconds(1f);
        private bool jumpRequested;
        private bool parachuteRequested;
        private Vector3 lastJumpStart;
        private Vector3 lastRouteStart;
        private Vector3 lastRouteEnd;
        private LineRenderer flightPathLine;
        private Material flightPathMaterial;
        private GameObject routeStartMarker;
        private GameObject routeEndMarker;
        private ReliablePlayerMovement reliablePlayerMovement;
        private bool localPlayerHasLanded;

        public string CurrentPhase { get; private set; } = "Lobby";
        public bool ConsumesJumpButton => CurrentPhase == "Aircraft" || CurrentPhase == "Freefall" || CurrentPhase == "Parachute";
        public Vector3 LastRouteStart => lastRouteStart;
        public Vector3 LastRouteEnd => lastRouteEnd;
        public bool DebugLocalPlayerHasLanded => localPlayerHasLanded;

        public void ConfigureForRuntime(
            Transform playerTransform,
            ThirdPersonMobileController playerController,
            UIManager ui,
            GameObject plane,
            GameObject parachute,
            LootItem[] lootPrefabs,
            Material airdropMaterial,
            Vector3[] starts,
            Vector3[] ends,
            Transform waitingPoint = null,
            BotManager bots = null)
        {
            player = playerTransform;
            controller = playerController;
            uiManager = ui;
            planeVisual = plane;
            parachuteVisual = parachute;
            airdropLootPrefabs = lootPrefabs;
            crateMaterial = airdropMaterial;
            routeStarts = starts;
            routeEnds = ends;
            waitingAreaPoint = waitingPoint;
            botManager = bots;
            reliablePlayerMovement = playerTransform != null ? playerTransform.GetComponent<ReliablePlayerMovement>() : null;
            localPlayerHasLanded = false;
            SetVisualActive(planeVisual, false);
            SetVisualActive(parachuteVisual, false);
            EnsureFlightPathLine();
            SetFlightPathActive(false);
        }

        public void ConfigureBattleRoyaleDirector(Vector2 halfExtents, float altitude, float routeSeconds, float waitingSeconds)
        {
            mapHalfExtents = new Vector2(Mathf.Max(80f, halfExtents.x), Mathf.Max(80f, halfExtents.y));
            routeAltitude = Mathf.Clamp(altitude, 64f, 180f);
            aircraftRouteSeconds = Mathf.Max(6f, routeSeconds);
            waitingCountdownSeconds = Mathf.Clamp(waitingSeconds, 3f, 30f);
        }

        public void RequestJump()
        {
            if (CurrentPhase == "Aircraft")
            {
                jumpRequested = true;
                uiManager?.SetMatchAnnouncement("Jump confirmed");
            }
            else if (CurrentPhase == "Freefall")
            {
                parachuteRequested = true;
                uiManager?.SetMatchAnnouncement("Deploying parachute");
            }
        }

        public IEnumerator PlayOpeningSequence()
        {
            if (player == null || controller == null)
            {
                yield break;
            }

            jumpRequested = false;
            parachuteRequested = false;
            localPlayerHasLanded = false;
            controller.SetExternalMotionLock(false);
            controller.ControlsEnabled = true;

            yield return PlayWaitingArea();

            controller.SetExternalMotionLock(true);
            controller.ControlsEnabled = false;

            SelectFlightRoute(out Vector3 start, out Vector3 end);
            Vector3 routeDirection = end - start;
            routeDirection.y = 0f;
            if (routeDirection.sqrMagnitude < 0.1f)
            {
                routeDirection = Vector3.forward;
            }

            routeDirection.Normalize();
            Quaternion routeRotation = Quaternion.LookRotation(routeDirection, Vector3.up);
            lastRouteStart = start;
            lastRouteEnd = end;
            UpdateRouteMarkers(start, end);
            botManager?.BeginBattleRoyaleDrop(start, end);

            yield return PlayAircraftRoute(start, end, routeRotation);
            Vector3 jumpStart = lastJumpStart;
            yield return PlayPlayerDrop(jumpStart, routeDirection, routeRotation);

            SetFlightPathActive(false);
            uiManager?.SetFlightPathPreview(start, end, false);
            SetVisualActive(planeVisual, false);
            localPlayerHasLanded = true;
            CurrentPhase = "Combat";
            controller.SetExternalMotionLock(false);
            controller.ControlsEnabled = true;
            EnsureReliableGroundMovementEnabled();
            uiManager?.SetMatchPhase("Combat", "Loot, rotate, survive");
            uiManager?.SetMatchAnnouncement("Combat live");
            SpawnAirdrop(FindGroundPoint(player.position + new Vector3(Random.Range(-42f, 42f), 0f, Random.Range(38f, 66f))));
        }

        private IEnumerator PlayWaitingArea()
        {
            CurrentPhase = "Waiting";
            uiManager?.SetMatchPhase("Waiting Lobby", "Combat locked");
            Vector3 waitPosition = waitingAreaPoint != null ? waitingAreaPoint.position : player.position + Vector3.up * 0.2f;
            Quaternion waitRotation = waitingAreaPoint != null ? waitingAreaPoint.rotation : Quaternion.identity;
            TryApplyLocalPlayerPose(waitPosition, waitRotation);

            int countdown = Mathf.CeilToInt(waitingCountdownSeconds);
            for (int i = countdown; i > 0; i--)
            {
                uiManager?.SetMatchPhase("Waiting Lobby", $"Match starts in {i}", 1f - (i / (float)Mathf.Max(1, countdown)));
                uiManager?.SetMatchAnnouncement($"Waiting area | Match starts in {i}");
                yield return countdownWait;
            }
        }

        private IEnumerator PlayAircraftRoute(Vector3 start, Vector3 end, Quaternion routeRotation)
        {
            CurrentPhase = "Aircraft";
            EnsureFlightPathLine();
            UpdateFlightPathLine(start, end);
            SetFlightPathActive(true);
            uiManager?.SetFlightPathPreview(start, end, true);
            SetVisualActive(planeVisual, true);
            SetVisualActive(parachuteVisual, false);

            lastJumpStart = start;
            float elapsed = 0f;
            while (elapsed < aircraftRouteSeconds)
            {
                elapsed += Time.deltaTime;
                float routeT = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, aircraftRouteSeconds));
                Vector3 planePosition = Vector3.Lerp(start, end, routeT);
                AnimateRoutePreview(start, end, routeT);
                TryApplyLocalPlayerPose(planePosition + Vector3.down * 2.6f, routeRotation);
                uiManager?.SetMatchPhase("Aircraft", routeT < autoJumpRoutePercent ? "Choose jump timing" : "Auto jump window", routeT);

                if (routeT < autoJumpRoutePercent)
                {
                    uiManager?.SetMatchAnnouncement("Tap JUMP to drop");
                }
                else
                {
                    uiManager?.SetMatchAnnouncement("Auto jump active");
                }

                if (jumpRequested || routeT >= autoJumpRoutePercent)
                {
                    lastJumpStart = planePosition + Vector3.down * 3.2f;
                    break;
                }

                yield return null;
            }

            yield return null;
        }

        private IEnumerator PlayPlayerDrop(Vector3 dropStart, Vector3 routeDirection, Quaternion routeRotation)
        {
            CurrentPhase = "Freefall";
            SetVisualActive(parachuteVisual, false);
            Vector3 dropPosition = dropStart;
            Vector3 dropVelocity = routeDirection * 10f + Vector3.down * Mathf.Max(4f, freeFallVerticalSpeed * 0.2f);
            Vector3 wind = BuildDropWind(routeDirection);
            float announceTimer = 0f;
            controller.SetDropCameraMode(true, false);
            controller.SetDropAnimationState(true, false, dropVelocity);
            uiManager?.SetMatchPhase("Freefall", "Steer toward loot");

            while (true)
            {
                Vector3 ground = FindGroundPoint(dropPosition);
                float altitude = Mathf.Max(0f, dropPosition.y - ground.y);
                if (altitude <= parachuteDeployAltitude || parachuteRequested)
                {
                    break;
                }

                Vector3 steer = BuildSteerDirection(routeRotation, routeDirection);
                Vector3 desiredHorizontal = Vector3.ClampMagnitude(routeDirection * 7f + steer * freeFallSteerSpeed + wind, freeFallMaxHorizontalSpeed);
                Vector3 horizontal = new Vector3(dropVelocity.x, 0f, dropVelocity.z);
                horizontal = Vector3.MoveTowards(horizontal, desiredHorizontal, freeFallGlideAcceleration * Time.deltaTime);
                float vertical = Mathf.MoveTowards(dropVelocity.y, -freeFallTerminalSpeed, freeFallGravity * Time.deltaTime);
                dropVelocity = horizontal + Vector3.up * vertical;
                dropPosition += dropVelocity * Time.deltaTime;
                Quaternion dropRotation = BuildDropRotation(dropVelocity, routeRotation, false);
                TryApplyLocalPlayerPose(dropPosition, dropRotation, true);
                controller.SetDropAnimationState(true, false, dropVelocity);
                announceTimer += Time.deltaTime;
                if (announceTimer >= 0.35f)
                {
                    announceTimer = 0f;
                    uiManager?.SetMatchPhase("Freefall", $"ALT {Mathf.CeilToInt(altitude)}m");
                    uiManager?.SetMatchAnnouncement($"Free fall | ALT {Mathf.CeilToInt(altitude)}m");
                }

                yield return null;
            }

            CurrentPhase = "Parachute";
            SetVisualActive(parachuteVisual, true);
            RuntimeAudioBank.Instance?.PlayPickup(dropPosition);
            controller.SetDropCameraMode(true, true);
            controller.SetDropAnimationState(true, true, dropVelocity);
            uiManager?.SetMatchPhase("Parachute", "Glide to landing");

            while (true)
            {
                Vector3 ground = FindGroundPoint(dropPosition);
                float targetY = ground.y + 1.12f;
                float altitude = Mathf.Max(0f, dropPosition.y - ground.y);
                if (altitude <= 1.35f)
                {
                    dropPosition = new Vector3(dropPosition.x, targetY, dropPosition.z);
                    TryApplyLocalPlayerPose(dropPosition, routeRotation);
                    break;
                }

                Vector3 steer = BuildSteerDirection(routeRotation, routeDirection);
                Vector3 desiredHorizontal = Vector3.ClampMagnitude(steer * parachuteSteerSpeed + routeDirection * 2.8f + wind * 0.35f, parachuteSteerSpeed + 3f);
                Vector3 horizontal = new Vector3(dropVelocity.x, 0f, dropVelocity.z);
                horizontal = Vector3.MoveTowards(horizontal, desiredHorizontal, parachuteTurnResponse * Time.deltaTime);
                float targetVerticalSpeed = -Mathf.Min(parachuteDescentSpeed, parachuteTerminalSpeed);
                float vertical = Mathf.MoveTowards(dropVelocity.y, targetVerticalSpeed, (dropVelocity.y < targetVerticalSpeed ? parachuteGravity : parachuteOpenDamping) * Time.deltaTime);
                dropVelocity = horizontal + Vector3.up * vertical;
                dropPosition += dropVelocity * Time.deltaTime;
                if (dropPosition.y < targetY)
                {
                    dropPosition.y = targetY;
                }

                Quaternion chuteRotation = BuildDropRotation(dropVelocity, routeRotation, true);
                TryApplyLocalPlayerPose(dropPosition, chuteRotation, true);
                controller.SetDropAnimationState(true, true, dropVelocity);
                UpdateParachuteVisual(dropPosition, chuteRotation);

                announceTimer += Time.deltaTime;
                if (announceTimer >= 0.35f)
                {
                    announceTimer = 0f;
                    uiManager?.SetMatchPhase("Parachute", $"ALT {Mathf.CeilToInt(altitude)}m");
                    uiManager?.SetMatchAnnouncement($"Parachute | ALT {Mathf.CeilToInt(altitude)}m");
                }

                yield return null;
            }

            CurrentPhase = "Landing";
            SetVisualActive(parachuteVisual, false);
            controller.SetDropCameraMode(false, false);
            controller.SetDropAnimationState(false, false, Vector3.zero);
            SpawnLandingCue(dropPosition);
            controller.PlayLandingRecovery(landingRecoverySeconds);
            uiManager?.SetMatchPhase("Landing", "Recovered");
            uiManager?.SetMatchAnnouncement("Landed safely");
            yield return new WaitForSeconds(0.35f);
        }

        private bool TryApplyLocalPlayerPose(Vector3 position, Quaternion rotation, bool preserveCameraYaw = false)
        {
            if (controller == null || IsLocalPlayerPoseProtected())
            {
                return false;
            }

            controller.SetExternalPose(position, rotation, preserveCameraYaw);
            return true;
        }

        private bool IsLocalPlayerPoseProtected()
        {
            return localPlayerHasLanded || CurrentPhase == "Combat";
        }

        private void EnsureReliableGroundMovementEnabled()
        {
            ReliablePlayerMovement reliableMovement = ResolveReliablePlayerMovement();
            if (reliableMovement != null && !reliableMovement.enabled)
            {
                reliableMovement.enabled = true;
            }

            CharacterController characterController = player != null ? player.GetComponent<CharacterController>() : null;
            if (characterController != null && !characterController.enabled)
            {
                characterController.enabled = true;
            }
        }

        private ReliablePlayerMovement ResolveReliablePlayerMovement()
        {
            if (reliablePlayerMovement != null)
            {
                return reliablePlayerMovement;
            }

            if (player == null)
            {
                return null;
            }

            reliablePlayerMovement = player.GetComponent<ReliablePlayerMovement>();
            if (reliablePlayerMovement == null)
            {
                reliablePlayerMovement = player.GetComponentInChildren<ReliablePlayerMovement>(true);
            }

            return reliablePlayerMovement;
        }

        private Vector3 BuildSteerDirection(Quaternion routeRotation, Vector3 fallbackForward)
        {
            Vector2 input = controller != null ? controller.ReadExternalMovementInput() : Vector2.zero;
            Vector3 direction = routeRotation * new Vector3(input.x, 0f, input.y);
            if (direction.sqrMagnitude <= 0.0025f)
            {
                direction = fallbackForward * 0.18f;
            }

            direction.y = 0f;
            return direction.normalized;
        }

        private Vector3 BuildDropWind(Vector3 routeDirection)
        {
            Vector3 crossWind = Vector3.Cross(Vector3.up, routeDirection).normalized;
            float seed = Mathf.Sin((lastRouteStart.x + lastRouteEnd.z) * 0.013f);
            return (crossWind * seed + routeDirection * 0.22f).normalized * windStrength;
        }

        private static Quaternion BuildDropRotation(Vector3 velocity, Quaternion fallbackRotation, bool parachute)
        {
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (flatVelocity.sqrMagnitude < 0.04f)
            {
                return fallbackRotation;
            }

            Quaternion yawRotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);
            float pitch = parachute ? 0f : Mathf.Clamp(Mathf.Abs(velocity.y) * 0.82f, 24f, 68f);
            return yawRotation * Quaternion.Euler(pitch, 0f, 0f);
        }

        private void SelectFlightRoute(out Vector3 start, out Vector3 end)
        {
            int presetCount = Mathf.Min(routeStarts != null ? routeStarts.Length : 0, routeEnds != null ? routeEnds.Length : 0);
            bool useProcedural = presetCount <= 0 || Random.value < proceduralRouteChance;
            if (!useProcedural)
            {
                int routeIndex = Random.Range(0, presetCount);
                start = routeStarts[routeIndex];
                end = routeEnds[routeIndex];
                if (Random.value < 0.5f)
                {
                    Vector3 swap = start;
                    start = end;
                    end = swap;
                }

                start.y = routeAltitude;
                end.y = routeAltitude;
                return;
            }

            float angle = Random.Range(0f, 360f);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            float laneOffset = Random.Range(-Mathf.Min(mapHalfExtents.x, mapHalfExtents.y) * 0.42f, Mathf.Min(mapHalfExtents.x, mapHalfExtents.y) * 0.42f);
            Vector3 center = side * laneOffset;
            float distance = Mathf.Max(mapHalfExtents.x, mapHalfExtents.y) + routeOvershoot;
            start = center - direction * distance + Vector3.up * routeAltitude;
            end = center + direction * distance + Vector3.up * routeAltitude;
        }

        private int SelectRouteIndex()
        {
            int count = Mathf.Min(routeStarts != null ? routeStarts.Length : 0, routeEnds != null ? routeEnds.Length : 0);
            return count <= 0 ? 0 : Random.Range(0, count);
        }

        private void AnimateRoutePreview(Vector3 start, Vector3 end, float t)
        {
            if (planeVisual == null)
            {
                return;
            }

            Vector3 position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            planeVisual.transform.position = position;
            Vector3 direction = end - start;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.1f)
            {
                planeVisual.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private void UpdateParachuteVisual(Vector3 playerPosition, Quaternion rotation)
        {
            if (parachuteVisual == null)
            {
                return;
            }

            parachuteVisual.transform.SetPositionAndRotation(playerPosition + Vector3.up * 2.55f, rotation);
        }

        private void EnsureFlightPathLine()
        {
            if (flightPathLine != null)
            {
                return;
            }

            GameObject pathObject = new GameObject("Milestone13 Flight Path Preview");
            pathObject.transform.SetParent(transform, false);
            flightPathLine = pathObject.AddComponent<LineRenderer>();
            flightPathLine.useWorldSpace = true;
            flightPathLine.positionCount = 2;
            flightPathLine.startWidth = 1.35f;
            flightPathLine.endWidth = 1.35f;
            flightPathLine.receiveShadows = false;
            flightPathLine.shadowCastingMode = ShadowCastingMode.Off;
            flightPathMaterial = CreateLineMaterial(new Color(0.92f, 0.98f, 1f, 0.82f));
            flightPathLine.sharedMaterial = flightPathMaterial;
            GameObject markerRoot = new GameObject("Milestone18 Randomized Flight Path");
            markerRoot.transform.SetParent(transform, false);
            routeStartMarker = new GameObject("Milestone18 Flight Start Marker");
            routeStartMarker.transform.SetParent(markerRoot.transform, false);
            routeEndMarker = new GameObject("Milestone18 Flight End Marker");
            routeEndMarker.transform.SetParent(markerRoot.transform, false);
        }

        private void UpdateFlightPathLine(Vector3 start, Vector3 end)
        {
            if (flightPathLine == null)
            {
                return;
            }

            flightPathLine.SetPosition(0, start + Vector3.up * 0.15f);
            flightPathLine.SetPosition(1, end + Vector3.up * 0.15f);
        }

        private void SetFlightPathActive(bool active)
        {
            if (flightPathLine != null)
            {
                flightPathLine.gameObject.SetActive(active);
            }

            if (routeStartMarker != null)
            {
                routeStartMarker.SetActive(active);
            }

            if (routeEndMarker != null)
            {
                routeEndMarker.SetActive(active);
            }
        }

        private void UpdateRouteMarkers(Vector3 start, Vector3 end)
        {
            EnsureFlightPathLine();
            if (routeStartMarker != null)
            {
                routeStartMarker.transform.position = start;
            }

            if (routeEndMarker != null)
            {
                routeEndMarker.transform.position = end;
            }
        }

        private void SpawnAirdrop(Vector3 position)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Milestone13 Airdrop Crate";
            crate.transform.position = position + Vector3.up * 0.7f;
            crate.transform.localScale = new Vector3(2.2f, 1.4f, 2.2f);
            Renderer renderer = crate.GetComponent<Renderer>();
            if (renderer != null && crateMaterial != null)
            {
                renderer.material = crateMaterial;
            }

            for (int i = 0; i < 4; i++)
            {
                if (airdropLootPrefabs == null || airdropLootPrefabs.Length == 0)
                {
                    break;
                }

                LootItem prefab = airdropLootPrefabs[Mathf.Clamp(i, 0, airdropLootPrefabs.Length - 1)];
                if (prefab == null)
                {
                    continue;
                }

                Vector3 offset = Quaternion.Euler(0f, i * 90f, 0f) * Vector3.forward * 2.6f;
                LootItem loot = Instantiate(prefab, position + offset + Vector3.up * 0.9f, Quaternion.Euler(0f, i * 35f, 0f));
                loot.gameObject.SetActive(true);
            }
        }

        private void SpawnLandingCue(Vector3 position)
        {
            GameObject cue = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cue.name = "Milestone18 Landing Dust Cue";
            cue.transform.position = position + Vector3.up * 0.03f;
            cue.transform.localScale = new Vector3(1.9f, 0.035f, 1.9f);
            Renderer renderer = cue.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateLineMaterial(new Color(0.72f, 0.66f, 0.48f, 0.45f));
            }

            Collider collider = cue.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            cue.AddComponent<RuntimeEffectAutoDestroy>().Configure(1.1f);
        }

        private static Vector3 FindGroundPoint(Vector3 source)
        {
            Vector3 origin = new Vector3(source.x, 160f, source.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 320f, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return new Vector3(source.x, 0f, source.z);
        }

        private static Material CreateLineMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader)
            {
                name = "Milestone13 Flight Path Material"
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            return material;
        }

        private static void SetVisualActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
