using UnityEngine;
using UnityEngine.AI;

namespace BattleZoneMobile
{
    [RequireComponent(typeof(Health))]
    public class BotAI : MonoBehaviour
    {
        public enum DifficultyPreset
        {
            Easy,
            Normal,
            Hard
        }

        private enum BotState
        {
            Patrol,
            Chase,
            TakeCover,
            Attack,
            Dead
        }

        [Header("References")]
        [SerializeField] private Transform eyePoint;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField] private Transform[] coverPoints;

        [Header("Movement")]
        [SerializeField] private float patrolRadius = 22f;
        [SerializeField] private float patrolSpeed = 2.2f;
        [SerializeField] private float chaseSpeed = 4.1f;
        [SerializeField] private float coverSpeed = 4.7f;
        [SerializeField] private float manualTurnSpeed = 540f;
        [SerializeField] private float waypointReachDistance = 1.8f;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 42f;
        [SerializeField] private float attackRange = 26f;
        [SerializeField] private float forgetRange = 58f;
        [SerializeField] private float combatMemoryDuration = 5.5f;
        [SerializeField] private float reactionTimeMin = 0.12f;
        [SerializeField] private float reactionTimeMax = 0.32f;
        [SerializeField] private float pathRefreshInterval = 0.24f;

        [Header("Loadout")]
        [SerializeField] private WeaponSlot equippedSlot = WeaponSlot.AssaultRifle;
        [SerializeField] private string equippedWeaponName = "Assault Rifle";
        [SerializeField] private float damage = 10f;
        [SerializeField] private float fireRate = 1.4f;
        [SerializeField] private float aimError = 1.4f;
        [SerializeField] private float aimLeadStrength = 0.34f;
        [SerializeField] private float firstShotDelay = 0.25f;
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 90;
        [SerializeField] private float reloadDuration = 1.8f;

        [Header("Visual")]
        [SerializeField] private Renderer[] tintRenderers;
        [SerializeField] private Color aliveColor = new Color(0.85f, 0.25f, 0.22f);
        [SerializeField] private Color alertColor = new Color(1f, 0.48f, 0.18f);
        [SerializeField] private Color deadColor = new Color(0.2f, 0.2f, 0.2f);

        private Transform player;
        private ThirdPersonMobileController playerController;
        private Health health;
        private NavMeshAgent agent;
        private GameManager gameManager;
        private Vector3 spawnPoint;
        private Vector3 currentWaypoint;
        private Vector3 coverDestination;
        private Vector3 flankDestination;
        private Vector3 lastKnownPlayerPosition;
        private Vector3 lastPlayerPosition;
        private Vector3 estimatedPlayerVelocity;
        private Vector3 lastAgentDestination;
        private Vector3 dropTarget;
        private Vector3 botDropVelocity;
        private Vector3 lastMovementSamplePosition;
        private readonly RaycastHit[] shotHits = new RaycastHit[16];
        private readonly RaycastHit[] sightHits = new RaycastHit[16];
        private readonly Collider[] awarenessColliders = new Collider[16];
        private BotState state;
        private float nextFireTime;
        private float coverUntilTime;
        private float nextFlankPickTime;
        private float reloadEndTime;
        private float nextHealTime;
        private float nextLootScanTime;
        private float nextVehicleCheckTime;
        private float nextCoverDecisionTime;
        private float nextPathRefreshTime;
        private float botVehicleUntilTime;
        private float investigateUntilTime;
        private float stuckTimer;
        private int currentAmmo;
        private int medkits = 1;
        private bool initialized;
        private bool combatLocked;
        private bool dropActive;
        private float dropDelayEndTime;
        private VehicleController botVehicle;
        private SafeZoneController safeZone;
        private GameObject botParachuteVisual;

        public bool IsDead => state == BotState.Dead;
        public bool IsStuck => stuckTimer > 0.85f;
        public string EquippedWeaponName => equippedWeaponName;

        public void ApplyDifficulty(DifficultyPreset preset)
        {
            switch (preset)
            {
                case DifficultyPreset.Easy:
                    detectionRange *= 0.82f;
                    attackRange *= 0.88f;
                    fireRate *= 0.72f;
                    aimError *= 1.45f;
                    damage *= 0.72f;
                    medkits = 0;
                    break;
                case DifficultyPreset.Hard:
                    detectionRange *= 1.12f;
                    attackRange *= 1.1f;
                    chaseSpeed *= 1.08f;
                    fireRate *= 1.18f;
                    aimError *= 0.72f;
                    damage *= 1.12f;
                    medkits = 2;
                    break;
                default:
                    medkits = 1;
                    break;
            }
        }

        public void ConfigureForRuntime(Transform eye, Renderer[] renderers, LayerMask visibilityMask)
        {
            eyePoint = eye;
            tintRenderers = renderers;
            lineOfSightMask = visibilityMask;
        }

        public void ConfigureCoverPoints(Transform[] points)
        {
            coverPoints = points;
        }

        public void EquipRuntimeWeapon(WeaponSlot slot)
        {
            equippedSlot = slot;
            switch (slot)
            {
                case WeaponSlot.SMG:
                    equippedWeaponName = "SMG";
                    damage = 7f;
                    fireRate = 5.6f;
                    attackRange = 22f;
                    aimError = 2.2f;
                    magazineSize = 35;
                    reserveAmmo = 105;
                    reloadDuration = 1.55f;
                    break;
                case WeaponSlot.Sniper:
                    equippedWeaponName = "Sniper";
                    damage = 34f;
                    fireRate = 0.65f;
                    attackRange = 48f;
                    aimError = 0.65f;
                    magazineSize = 5;
                    reserveAmmo = 20;
                    reloadDuration = 2.4f;
                    break;
                case WeaponSlot.Shotgun:
                    equippedWeaponName = "Shotgun";
                    damage = 18f;
                    fireRate = 0.9f;
                    attackRange = 15f;
                    aimError = 4.2f;
                    magazineSize = 6;
                    reserveAmmo = 24;
                    reloadDuration = 2.1f;
                    break;
                case WeaponSlot.Pistol:
                    equippedWeaponName = "Pistol";
                    damage = 9f;
                    fireRate = 1.7f;
                    attackRange = 24f;
                    aimError = 1.8f;
                    magazineSize = 12;
                    reserveAmmo = 36;
                    reloadDuration = 1.3f;
                    break;
                default:
                    equippedWeaponName = "Assault Rifle";
                    damage = 10f;
                    fireRate = 2.2f;
                    attackRange = 28f;
                    aimError = 1.4f;
                    magazineSize = 30;
                    reserveAmmo = 90;
                    reloadDuration = 1.9f;
                    break;
            }

            currentAmmo = magazineSize;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            agent = GetComponent<NavMeshAgent>();

            if (eyePoint == null)
            {
                eyePoint = transform;
            }

            health.onDied.AddListener(OnDied);
            health.onDamageTaken.AddListener(OnDamaged);
            spawnPoint = transform.position;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.onDied.RemoveListener(OnDied);
                health.onDamageTaken.RemoveListener(OnDamaged);
            }

            if (botParachuteVisual != null)
            {
                Destroy(botParachuteVisual);
            }
        }

        private void Start()
        {
            ApplyTint(aliveColor);
            PickNewPatrolPoint();
        }

        private void Update()
        {
            if (state == BotState.Dead)
            {
                return;
            }

            if (dropActive)
            {
                UpdateParachuteDrop();
                return;
            }

            if (!initialized || player == null)
            {
                return;
            }

            if (combatLocked)
            {
                StopMoving();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            UpdatePlayerVelocityEstimate();
            bool canSeePlayer = distanceToPlayer <= detectionRange && HasLineOfSight();
            if (canSeePlayer)
            {
                lastKnownPlayerPosition = player.position;
                investigateUntilTime = Time.time + combatMemoryDuration;
            }

            if (reloadEndTime > 0f)
            {
                if (Time.time < reloadEndTime)
                {
                    HoldReloadCover();
                    return;
                }

                FinishReload();
            }

            TryHeal();
            TryLootNearby();
            TryUseVehicle(distanceToPlayer);
            TryReviveNearby();

            if (ShouldMoveToSafeZone(canSeePlayer))
            {
                state = BotState.Chase;
                ApplyTint(alertColor);
                MoveTo(safeZone.GetNearestSafePoint(transform.position), chaseSpeed);
                return;
            }

            if (state == BotState.TakeCover && Time.time < coverUntilTime)
            {
                TakeCover();
                return;
            }

            BotState previousState = state;
            if (distanceToPlayer > forgetRange && Time.time > investigateUntilTime)
            {
                state = BotState.Patrol;
                ApplyTint(aliveColor);
            }
            else if (distanceToPlayer <= attackRange && canSeePlayer)
            {
                state = BotState.Attack;
                ApplyTint(alertColor);
            }
            else if (canSeePlayer || distanceToPlayer <= detectionRange * 0.65f || Time.time <= investigateUntilTime)
            {
                state = BotState.Chase;
                ApplyTint(alertColor);
            }
            else if (state != BotState.Patrol)
            {
                state = BotState.Patrol;
                ApplyTint(aliveColor);
            }

            if (state == BotState.Attack && previousState != BotState.Attack)
            {
                nextFireTime = Mathf.Max(nextFireTime, Time.time + Random.Range(reactionTimeMin, reactionTimeMax));
            }

            if (state == BotState.Attack && ShouldBreakForCover(canSeePlayer))
            {
                coverDestination = FindNearestCover();
                coverUntilTime = Time.time + Random.Range(1.25f, 2.05f);
                nextCoverDecisionTime = Time.time + Random.Range(2.6f, 4.2f);
                state = BotState.TakeCover;
            }

            switch (state)
            {
                case BotState.Patrol:
                    Patrol();
                    break;
                case BotState.Chase:
                    Chase();
                    break;
                case BotState.Attack:
                    Attack();
                    break;
            }
        }

        public void Initialize(Transform playerTarget, GameManager owner)
        {
            player = playerTarget;
            playerController = playerTarget != null ? playerTarget.GetComponent<ThirdPersonMobileController>() : null;
            gameManager = owner;
            initialized = true;
            state = BotState.Patrol;
            nextFireTime = Time.time + firstShotDelay;
            nextFlankPickTime = 0f;
            reloadEndTime = 0f;
            currentAmmo = Mathf.Clamp(currentAmmo <= 0 ? magazineSize : currentAmmo, 1, magazineSize);
            reserveAmmo = Mathf.Max(0, reserveAmmo);
            lastMovementSamplePosition = transform.position;
            lastKnownPlayerPosition = playerTarget != null ? playerTarget.position : transform.position;
            lastPlayerPosition = lastKnownPlayerPosition;
            estimatedPlayerVelocity = Vector3.zero;
            lastAgentDestination = transform.position;
            investigateUntilTime = 0f;
            stuckTimer = 0f;
            PickNewPatrolPoint();
        }

        public void SetCombatLocked(bool locked)
        {
            combatLocked = locked;
            if (combatLocked)
            {
                StopMoving();
            }
        }

        public void SetSafeZone(SafeZoneController zone)
        {
            safeZone = zone;
        }

        public void BeginParachuteDrop(Vector3 landingPoint, float delaySeconds)
        {
            dropTarget = landingPoint;
            dropDelayEndTime = Time.time + Mathf.Max(0f, delaySeconds);
            dropActive = true;
            combatLocked = true;
            transform.position = dropTarget + Vector3.up * Random.Range(72f, 94f);
            botDropVelocity = Vector3.down * Random.Range(7f, 12f);
            EnsureBotParachuteVisual();
            if (botParachuteVisual != null)
            {
                botParachuteVisual.SetActive(true);
            }

            StopMoving();
        }

        public void ReactToGunshot(Vector3 shotPosition)
        {
            if (!initialized || state == BotState.Dead)
            {
                return;
            }

            float audibleRange = detectionRange * 1.8f;
            if (Vector3.SqrMagnitude(transform.position - shotPosition) > audibleRange * audibleRange)
            {
                return;
            }

            flankDestination = SampleNavMeshPosition(shotPosition + Random.insideUnitSphere * 8f, 8f);
            flankDestination.y = transform.position.y;
            lastKnownPlayerPosition = shotPosition;
            investigateUntilTime = Time.time + combatMemoryDuration;
            currentWaypoint = flankDestination;
            nextFlankPickTime = Time.time + 1.2f;
            state = BotState.Chase;
            ApplyTint(alertColor);
        }

        private void Patrol()
        {
            MoveTo(currentWaypoint, patrolSpeed);

            if (Vector3.Distance(transform.position, currentWaypoint) <= waypointReachDistance)
            {
                PickNewPatrolPoint();
            }
        }

        private void Chase()
        {
            if (Time.time >= nextFlankPickTime || Vector3.Distance(transform.position, flankDestination) <= waypointReachDistance)
            {
                Vector3 awayFromPlayer = transform.position - player.position;
                awayFromPlayer.y = 0f;
                if (awayFromPlayer.sqrMagnitude < 0.1f)
                {
                    awayFromPlayer = -player.forward;
                }

                Vector3 flankDirection = Quaternion.Euler(0f, Random.Range(-64f, 64f), 0f) * awayFromPlayer.normalized;
                flankDestination = SampleNavMeshPosition(player.position + flankDirection * Random.Range(8f, 15f), 10f);
                nextFlankPickTime = Time.time + Random.Range(2.1f, 3.4f);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector3 chaseTarget = Time.time <= investigateUntilTime ? lastKnownPlayerPosition : player.position;
            MoveTo(distanceToPlayer > attackRange * 1.4f ? chaseTarget : flankDestination, chaseSpeed);
        }

        private void TakeCover()
        {
            MoveTo(coverDestination, coverSpeed);
            if (Vector3.Distance(transform.position, coverDestination) <= waypointReachDistance)
            {
                RotateToward(player.position);
            }
        }

        private void Attack()
        {
            StopMoving();
            RotateToward(player.position);

            if (Time.time < nextFireTime)
            {
                return;
            }

            if (currentAmmo <= 0)
            {
                StartReload();
                return;
            }

            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            currentAmmo--;
            ShootAtPlayer();

            if (currentAmmo <= 0 && reserveAmmo > 0)
            {
                StartReload();
            }
        }

        private void ShootAtPlayer()
        {
            float targetHeight = playerController != null && playerController.IsProne ? 0.58f : 1.2f;
            float distanceToTarget = Vector3.Distance(eyePoint.position, player.position);
            Vector3 target = player.position + Vector3.up * targetHeight + estimatedPlayerVelocity * Mathf.Clamp(distanceToTarget / 45f, 0f, 0.55f) * aimLeadStrength;
            Vector3 direction = (target - eyePoint.position).normalized;
            float distanceAimError = Mathf.Lerp(aimError * 0.55f, aimError * 1.08f, Mathf.Clamp01(distanceToTarget / Mathf.Max(1f, attackRange)));
            if (playerController != null && (playerController.IsProne || playerController.IsAiming))
            {
                distanceAimError *= playerController.IsProne ? 1.16f : 0.82f;
            }

            direction = Quaternion.Euler(
                Random.Range(-distanceAimError, distanceAimError),
                Random.Range(-distanceAimError, distanceAimError),
                0f) * direction;

            RuntimeAudioBank.Instance?.PlayWeaponFire(equippedSlot, eyePoint.position);

            int hitCount = Physics.RaycastNonAlloc(eyePoint.position, direction, shotHits, attackRange + 8f, lineOfSightMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(shotHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = shotHits[i];
                if (hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    damageable.TakeDamage(damage, hit.point, hit.normal, gameObject);
                }

                break;
            }
        }

        private bool HasLineOfSight()
        {
            Vector3 target = player.position + Vector3.up * 1.2f;
            Vector3 origin = eyePoint.position;
            Vector3 direction = target - origin;

            int hitCount = Physics.RaycastNonAlloc(origin, direction.normalized, sightHits, direction.magnitude, lineOfSightMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(sightHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = sightHits[i];
                if (hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                return hit.collider.GetComponentInParent<ThirdPersonMobileController>() != null ||
                       hit.collider.GetComponentInParent<PlayerInventory>() != null;
            }

            return true;
        }

        private void MoveTo(Vector3 destination, float speed)
        {
            if (HasUsableAgent())
            {
                agent.isStopped = false;
                agent.speed = speed;
                if (Time.time >= nextPathRefreshTime || Vector3.SqrMagnitude(lastAgentDestination - destination) > 4f || !agent.hasPath)
                {
                    Vector3 sampledDestination = SampleNavMeshPosition(destination, 6f);
                    agent.SetDestination(sampledDestination);
                    lastAgentDestination = sampledDestination;
                    nextPathRefreshTime = Time.time + pathRefreshInterval;
                }

                CheckForStuck(destination);
                return;
            }

            Vector3 flatDestination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 direction = flatDestination - transform.position;
            if (direction.sqrMagnitude <= 0.01f)
            {
                return;
            }

            RotateToward(destination);
            Vector3 moveDirection = direction.normalized;
            Vector3 probeOrigin = transform.position + Vector3.up * 0.72f;
            if (Physics.Raycast(probeOrigin, moveDirection, out RaycastHit obstacle, 1.25f, lineOfSightMask, QueryTriggerInteraction.Ignore) &&
                obstacle.collider.transform.root != transform.root)
            {
                Vector3 sideStep = Vector3.Cross(Vector3.up, obstacle.normal).normalized;
                if (Vector3.Dot(sideStep, direction) < 0f)
                {
                    sideStep = -sideStep;
                }

                moveDirection = (moveDirection + sideStep * 0.75f + obstacle.normal * 0.25f).normalized;
            }

            transform.position += moveDirection * speed * Time.deltaTime;
            CheckForStuck(destination);
        }

        private void StopMoving()
        {
            if (HasUsableAgent())
            {
                agent.isStopped = true;
            }
        }

        private bool HasUsableAgent()
        {
            return agent != null && agent.enabled && agent.isOnNavMesh;
        }

        private void RotateToward(Vector3 target)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manualTurnSpeed * Time.deltaTime);
        }

        private void PickNewPatrolPoint()
        {
            Vector2 random = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = spawnPoint + new Vector3(random.x, 0f, random.y);

            currentWaypoint = SampleNavMeshPosition(candidate, patrolRadius);
        }

        private void UpdateParachuteDrop()
        {
            if (Time.time < dropDelayEndTime)
            {
                UpdateBotParachuteVisual();
                return;
            }

            Vector3 horizontal = dropTarget - transform.position;
            horizontal.y = 0f;
            Vector3 desiredHorizontal = horizontal.sqrMagnitude > 0.04f ? horizontal.normalized * 12f : Vector3.zero;
            Vector3 currentHorizontal = new Vector3(botDropVelocity.x, 0f, botDropVelocity.z);
            currentHorizontal = Vector3.MoveTowards(currentHorizontal, desiredHorizontal, 18f * Time.deltaTime);
            float vertical = Mathf.MoveTowards(botDropVelocity.y, -10.5f, 24f * Time.deltaTime);
            botDropVelocity = currentHorizontal + Vector3.up * vertical;
            transform.position += botDropVelocity * Time.deltaTime;
            if (horizontal.sqrMagnitude > 0.08f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(horizontal.normalized, Vector3.up), manualTurnSpeed * Time.deltaTime);
            }

            UpdateBotParachuteVisual();
            if (transform.position.y <= dropTarget.y + 0.2f)
            {
                transform.position = dropTarget;
                dropActive = false;
                combatLocked = true;
                spawnPoint = transform.position;
                lastMovementSamplePosition = transform.position;
                lastAgentDestination = transform.position;
                if (HasUsableAgent())
                {
                    agent.Warp(transform.position);
                    agent.ResetPath();
                }

                if (botParachuteVisual != null)
                {
                    botParachuteVisual.SetActive(false);
                }

                PickNewPatrolPoint();
            }
        }

        private bool ShouldMoveToSafeZone(bool canSeePlayer)
        {
            return safeZone != null &&
                   !canSeePlayer &&
                   safeZone.IsActive &&
                   safeZone.IsOutsideZone(transform.position);
        }

        private void OnDamaged(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            if (state == BotState.Dead)
            {
                return;
            }

            RuntimeAudioBank.Instance?.PlayHit(hitPoint);
            if (source != null)
            {
                lastKnownPlayerPosition = source.transform.position;
                investigateUntilTime = Time.time + combatMemoryDuration;
            }

            if (health.CurrentHealth < health.MaxHealth * 0.55f)
            {
                coverDestination = FindNearestCover();
                coverUntilTime = Time.time + 1.8f;
                state = BotState.TakeCover;
            }
        }

        private Vector3 FindNearestCover()
        {
            if (coverPoints == null || coverPoints.Length == 0)
            {
                return transform.position - transform.forward * 8f;
            }

            Transform best = null;
            float bestScore = float.MaxValue;
            foreach (Transform point in coverPoints)
            {
                if (point == null)
                {
                    continue;
                }

                float botDistance = Vector3.Distance(transform.position, point.position);
                float playerDistance = player != null ? Vector3.Distance(player.position, point.position) : 18f;
                float score = botDistance - playerDistance * 0.36f;
                if (player != null)
                {
                    Vector3 playerEye = player.position + Vector3.up * 1.2f;
                    Vector3 coverEye = point.position + Vector3.up * 0.7f;
                    Vector3 toCover = coverEye - playerEye;
                    if (Physics.Raycast(playerEye, toCover.normalized, out RaycastHit hit, toCover.magnitude, lineOfSightMask, QueryTriggerInteraction.Ignore) &&
                        hit.collider.transform.root != transform.root)
                    {
                        score -= 8f;
                    }
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    best = point;
                }
            }

            return best != null ? best.position : transform.position - transform.forward * 8f;
        }

        private void OnDied(GameObject source)
        {
            state = BotState.Dead;
            StopMoving();
            if (botParachuteVisual != null)
            {
                Destroy(botParachuteVisual);
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider item in colliders)
            {
                item.enabled = false;
            }

            ApplyTint(deadColor);
            RuntimeAudioBank.Instance?.PlayDeath(transform.position);
            SpawnDeathEffect();
            gameManager?.NotifyBotKilled(this, source);
            Destroy(gameObject, 4f);
        }

        private void SpawnDeathEffect()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Runtime Bot Death Effect";
            marker.transform.position = transform.position + Vector3.up * 0.8f;
            marker.transform.localScale = new Vector3(0.7f, 0.18f, 0.7f);
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.2f, 0.08f, 0.8f);
            }

            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            marker.AddComponent<RuntimeEffectAutoDestroy>().Configure(1.2f);
        }

        private void EnsureBotParachuteVisual()
        {
            if (botParachuteVisual != null)
            {
                return;
            }

            botParachuteVisual = new GameObject("Milestone13 Bot Parachute Visual");
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Bot Parachute Canopy";
            canopy.transform.SetParent(botParachuteVisual.transform, false);
            canopy.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            canopy.transform.localScale = new Vector3(2.1f, 0.42f, 2.1f);
            Renderer canopyRenderer = canopy.GetComponent<Renderer>();
            if (canopyRenderer != null)
            {
                canopyRenderer.material.color = new Color(0.82f, 0.38f, 0.24f, 0.92f);
            }

            Collider canopyCollider = canopy.GetComponent<Collider>();
            if (canopyCollider != null)
            {
                Destroy(canopyCollider);
            }

            for (int i = 0; i < 2; i++)
            {
                GameObject cord = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cord.name = i == 0 ? "Bot Parachute Cord L" : "Bot Parachute Cord R";
                cord.transform.SetParent(botParachuteVisual.transform, false);
                cord.transform.localPosition = new Vector3(i == 0 ? -0.52f : 0.52f, 0.75f, 0f);
                cord.transform.localScale = new Vector3(0.04f, 1.55f, 0.04f);
                Collider cordCollider = cord.GetComponent<Collider>();
                if (cordCollider != null)
                {
                    Destroy(cordCollider);
                }
            }

            botParachuteVisual.SetActive(false);
        }

        private void UpdateBotParachuteVisual()
        {
            if (botParachuteVisual == null)
            {
                return;
            }

            botParachuteVisual.transform.SetPositionAndRotation(transform.position + Vector3.up * 1.9f, transform.rotation);
        }

        private void StartReload()
        {
            if (reserveAmmo <= 0 || reloadEndTime > 0f)
            {
                return;
            }

            reloadEndTime = Time.time + reloadDuration;
            coverDestination = FindNearestCover();
            coverUntilTime = reloadEndTime;
            state = BotState.TakeCover;
            RuntimeAudioBank.Instance?.PlayReload(transform.position);
        }

        private void FinishReload()
        {
            int needed = magazineSize - currentAmmo;
            int loaded = Mathf.Min(needed, reserveAmmo);
            currentAmmo += loaded;
            reserveAmmo -= loaded;
            reloadEndTime = 0f;
        }

        private void HoldReloadCover()
        {
            if (player != null)
            {
                RotateToward(player.position);
            }

            if (coverDestination != Vector3.zero)
            {
                MoveTo(coverDestination, coverSpeed * 0.8f);
            }
        }

        private void TryHeal()
        {
            if (medkits <= 0 || health == null || health.CurrentHealth > health.MaxHealth * 0.34f || Time.time < nextHealTime)
            {
                return;
            }

            medkits--;
            nextHealTime = Time.time + 9f;
            health.Heal(32f);
            coverDestination = FindNearestCover();
            coverUntilTime = Time.time + 2.1f;
            state = BotState.TakeCover;
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
        }

        private void UpdatePlayerVelocityEstimate()
        {
            if (player == null)
            {
                return;
            }

            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 rawVelocity = (player.position - lastPlayerPosition) / deltaTime;
            rawVelocity.y = 0f;
            estimatedPlayerVelocity = Vector3.Lerp(estimatedPlayerVelocity, rawVelocity, 1f - Mathf.Exp(-8f * deltaTime));
            lastPlayerPosition = player.position;
        }

        private bool ShouldBreakForCover(bool canSeePlayer)
        {
            if (Time.time < nextCoverDecisionTime || health == null || player == null)
            {
                return false;
            }

            nextCoverDecisionTime = Time.time + Random.Range(1.6f, 2.8f);
            float health01 = health.MaxHealth <= 0f ? 1f : health.CurrentHealth / health.MaxHealth;
            if (health01 < 0.42f)
            {
                return true;
            }

            return canSeePlayer && health01 < 0.74f && Random.value < 0.22f;
        }

        private static Vector3 SampleNavMeshPosition(Vector3 candidate, float maxDistance)
        {
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(0.1f, maxDistance), NavMesh.AllAreas))
            {
                return hit.position;
            }

            return candidate;
        }

        private void TryLootNearby()
        {
            if (Time.time < nextLootScanTime || state == BotState.Dead)
            {
                return;
            }

            nextLootScanTime = Time.time + Random.Range(1.8f, 2.9f);
            int count = Physics.OverlapSphereNonAlloc(transform.position, 3.2f, awarenessColliders, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider item = awarenessColliders[i];
                if (item == null)
                {
                    continue;
                }

                LootItem loot = item.GetComponentInParent<LootItem>();
                if (loot == null)
                {
                    continue;
                }

                ConsumeBotLoot(loot);
                Destroy(loot.gameObject);
                RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                break;
            }
        }

        private void ConsumeBotLoot(LootItem loot)
        {
            switch (loot.Kind)
            {
                case LootKind.Pistol:
                    EquipRuntimeWeapon(WeaponSlot.Pistol);
                    break;
                case LootKind.AssaultRifle:
                    EquipRuntimeWeapon(WeaponSlot.AssaultRifle);
                    break;
                case LootKind.SMG:
                    EquipRuntimeWeapon(WeaponSlot.SMG);
                    break;
                case LootKind.Sniper:
                    EquipRuntimeWeapon(WeaponSlot.Sniper);
                    break;
                case LootKind.Shotgun:
                    EquipRuntimeWeapon(WeaponSlot.Shotgun);
                    break;
                case LootKind.Medkit:
                case LootKind.Bandage:
                case LootKind.Backpack:
                    medkits = Mathf.Min(3, medkits + 1);
                    break;
                case LootKind.ArmorPlate:
                case LootKind.ArmorVest:
                    health?.RestoreArmor(loot.Kind == LootKind.ArmorVest ? 45f : 24f);
                    break;
            }
        }

        private void TryUseVehicle(float distanceToPlayer)
        {
            if (player == null || state == BotState.Dead || distanceToPlayer < detectionRange * 1.15f)
            {
                return;
            }

            if (botVehicle != null && Time.time < botVehicleUntilTime && botVehicle.IsAlive)
            {
                botVehicle.BotDriveToward(player.position, Time.deltaTime);
                transform.position = botVehicle.transform.position + botVehicle.transform.right * 1.4f + Vector3.up * 0.05f;
                transform.rotation = botVehicle.transform.rotation;
                return;
            }

            if (Time.time < nextVehicleCheckTime)
            {
                return;
            }

            nextVehicleCheckTime = Time.time + Random.Range(3.2f, 5.4f);
            VehicleController nearest = VehicleController.FindNearestAvailable(transform.position, 13f);
            if (nearest == null)
            {
                return;
            }

            botVehicle = nearest;
            botVehicleUntilTime = Time.time + Random.Range(2.2f, 4f);
        }

        private void TryReviveNearby()
        {
            if (Time.time < nextHealTime || state == BotState.Dead)
            {
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, 4.5f, awarenessColliders, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider item = awarenessColliders[i];
                if (item == null || item.transform.root == transform.root)
                {
                    continue;
                }

                KnockdownReviveState knockdown = item.GetComponentInParent<KnockdownReviveState>();
                if (knockdown != null && knockdown.IsKnocked)
                {
                    knockdown.Revive();
                    nextHealTime = Time.time + 5f;
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    return;
                }
            }
        }

        private void CheckForStuck(Vector3 destination)
        {
            if (Vector3.Distance(transform.position, destination) <= waypointReachDistance)
            {
                stuckTimer = 0f;
                lastMovementSamplePosition = transform.position;
                return;
            }

            float moved = Vector3.SqrMagnitude(transform.position - lastMovementSamplePosition);
            if (moved < 0.0009f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            lastMovementSamplePosition = transform.position;

            if (stuckTimer < 1.15f)
            {
                return;
            }

            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;
            if (toDestination.sqrMagnitude < 0.01f)
            {
                toDestination = transform.forward;
            }

            Vector3 sidestep = Quaternion.Euler(0f, Random.value > 0.5f ? 78f : -78f, 0f) * toDestination.normalized;
            currentWaypoint = transform.position + sidestep * 8f;
            coverDestination = currentWaypoint;
            flankDestination = currentWaypoint;
            if (HasUsableAgent())
            {
                agent.ResetPath();
                Vector3 sampled = SampleNavMeshPosition(currentWaypoint, 10f);
                agent.SetDestination(sampled);
                lastAgentDestination = sampled;
            }
            else
            {
                Vector3 probeOrigin = transform.position + Vector3.up * 0.8f;
                if (Physics.Raycast(probeOrigin, toDestination.normalized, out RaycastHit obstacle, 2.2f, lineOfSightMask, QueryTriggerInteraction.Ignore) &&
                    obstacle.collider.transform.root != transform.root)
                {
                    transform.position += obstacle.normal * 0.9f;
                }
            }

            stuckTimer = 0f;
        }

        private void ApplyTint(Color color)
        {
            if (tintRenderers == null)
            {
                return;
            }

            foreach (Renderer item in tintRenderers)
            {
                if (item != null && item.material != null && item.material.HasProperty("_Color"))
                {
                    item.material.color = color;
                }
            }
        }

        private sealed class RaycastHitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();

            public int Compare(RaycastHit left, RaycastHit right)
            {
                return left.distance.CompareTo(right.distance);
            }
        }
    }
}
