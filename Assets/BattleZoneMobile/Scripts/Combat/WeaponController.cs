using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BattleZoneMobile
{
    [Serializable]
    public class WeaponRuntimeState
    {
        public WeaponDefinition definition;
        public WeaponAttachmentProfile attachments;
        public bool unlocked = true;
        public int magazineAmmo;
        public int reserveAmmo;
    }

    [Serializable]
    public class AmmoChangedEvent : UnityEvent<string, int, int>
    {
    }

    [Serializable]
    public class WeaponChangedEvent : UnityEvent<string>
    {
    }

    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private ThirdPersonMobileController recoilReceiver;
        [SerializeField] private WeaponModelRig weaponModelRig;
        [SerializeField] private WorldDamageNumber damageNumberPrefab;
        [SerializeField] private ParticleSystem hitEffectPrefab;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Weapons")]
        [SerializeField] private List<WeaponRuntimeState> weapons = new List<WeaponRuntimeState>();
        [SerializeField] private WeaponSlot startingWeapon = WeaponSlot.Pistol;
        [SerializeField] private float switchDuration = 0.18f;
        [SerializeField] private float switchFireLockout = 0.06f;
        [SerializeField] private float headshotMultiplier = 2f;

        [Header("Feel")]
        [SerializeField] private float aimAssistRadius = 0.55f;
        [SerializeField] private float aimAssistStrength = 0.22f;
        [SerializeField] private float recoilHeatPerShot = 0.16f;
        [SerializeField] private float recoilHeatRecovery = 3.25f;
        [SerializeField] private float adsRecoilScale = 0.62f;
        [SerializeField] private float hipRecoilScale = 1f;
        [SerializeField] private float tracerLife = 0.085f;
        [SerializeField] private float tracerStartWidth = 0.044f;
        [SerializeField] private float tracerEndWidth = 0.006f;
        [SerializeField] private float shellEjectionSpread = 0.18f;
        [SerializeField] private float fireInputBufferSeconds = 0.085f;

        [Header("Events")]
        public AmmoChangedEvent onAmmoChanged = new AmmoChangedEvent();
        public WeaponChangedEvent onWeaponChanged = new WeaponChangedEvent();
        public UnityEvent onFired = new UnityEvent();
        public UnityEvent onReloadStarted = new UnityEvent();
        public UnityEvent onReloadFinished = new UnityEvent();
        public UnityEvent onWeaponSwitchStarted = new UnityEvent();
        public UnityEvent onWeaponSwitchFinished = new UnityEvent();
        public UnityEvent onHeadshotConfirmed = new UnityEvent();
        public HitConfirmedEvent onHitConfirmed = new HitConfirmedEvent();

        private WeaponRuntimeState currentWeapon;
        private readonly RaycastHit[] raycastHits = new RaycastHit[24];
        private readonly RaycastHit[] aimAssistHits = new RaycastHit[12];
        private Coroutine reloadRoutine;
        private Coroutine switchRoutine;
        private float nextFireTime;
        private float fireLockUntilTime;
        private float fireBufferedUntilTime;
        private float recoilHeat;
        private bool fireHeld;
        private bool controlsEnabled = true;

        public WeaponRuntimeState CurrentWeapon => currentWeapon;
        public bool IsReloading => reloadRoutine != null;
        public bool IsSwitching => switchRoutine != null;
        public bool ControlsEnabled => controlsEnabled;

        public void ConfigureForRuntime(
            Camera camera,
            Transform muzzle,
            ThirdPersonMobileController controller,
            WeaponModelRig modelRig,
            WorldDamageNumber damagePopup,
            ParticleSystem hitEffect,
            LayerMask mask,
            params WeaponDefinition[] runtimeWeapons)
        {
            aimCamera = camera;
            muzzlePoint = muzzle;
            recoilReceiver = controller;
            weaponModelRig = modelRig;
            damageNumberPrefab = damagePopup;
            hitEffectPrefab = hitEffect;
            hitMask = mask;
            startingWeapon = WeaponSlot.Pistol;
            weapons.Clear();

            foreach (WeaponDefinition definition in runtimeWeapons)
            {
                AddRuntimeWeapon(definition, definition != null && definition.slot == startingWeapon);
            }
        }

        private void Awake()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            foreach (WeaponRuntimeState weapon in weapons)
            {
                InitializeWeaponState(weapon);
            }
        }

        private void Start()
        {
            SelectWeapon(startingWeapon, true);
        }

        private void Update()
        {
            recoilHeat = Mathf.MoveTowards(recoilHeat, 0f, recoilHeatRecovery * Time.deltaTime);
            if (fireBufferedUntilTime > Time.time || (fireHeld && currentWeapon != null && currentWeapon.definition != null && currentWeapon.definition.automatic))
            {
                TryFire();
            }
        }

        public void FirePressed()
        {
            if (!controlsEnabled)
            {
                return;
            }

            fireHeld = true;
            fireBufferedUntilTime = Time.time + fireInputBufferSeconds;
            TryFire();
        }

        public void FireReleased()
        {
            fireHeld = false;
        }

        public void ReloadPressed()
        {
            if (!controlsEnabled)
            {
                return;
            }

            RequestReload();
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!controlsEnabled)
            {
                fireHeld = false;
                fireBufferedUntilTime = 0f;
            }
        }

        public void SelectWeapon(WeaponSlot slot)
        {
            SelectWeapon(slot, false);
        }

        public void SelectNextWeapon()
        {
            if (weapons.Count == 0)
            {
                return;
            }

            int startIndex = Mathf.Max(0, weapons.IndexOf(currentWeapon));
            for (int offset = 1; offset <= weapons.Count; offset++)
            {
                int index = (startIndex + offset) % weapons.Count;
                WeaponRuntimeState candidate = weapons[index];
                if (candidate.definition != null && candidate.unlocked)
                {
                    SelectWeapon(candidate.definition.slot);
                    return;
                }
            }
        }

        public void UnlockWeapon(WeaponSlot slot, int reserveAmmoBonus)
        {
            WeaponRuntimeState state = weapons.Find(item => item.definition != null && item.definition.slot == slot);
            if (state == null)
            {
                return;
            }

            state.unlocked = true;
            state.reserveAmmo += Mathf.Max(0, reserveAmmoBonus);

            if (state.magazineAmmo <= 0)
            {
                state.magazineAmmo = state.definition.magazineSize;
            }

            SelectWeapon(slot);
        }

        public void AddReserveAmmo(AmmoKind ammoKind, int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            foreach (WeaponRuntimeState state in weapons)
            {
                if (state.definition != null && state.definition.ammoKind == ammoKind)
                {
                    state.reserveAmmo += safeAmount;
                }
            }

            RaiseAmmoChanged();
        }

        public bool HasWeapon(WeaponSlot slot)
        {
            WeaponRuntimeState state = weapons.Find(item => item.definition != null && item.definition.slot == slot);
            return state != null && state.unlocked;
        }

        public string BuildWeaponSummary()
        {
            if (weapons.Count == 0)
            {
                return "Weapons: None";
            }

            List<string> names = new List<string>();
            foreach (WeaponRuntimeState state in weapons)
            {
                if (state != null && state.definition != null && state.unlocked)
                {
                    string attachmentText = state.attachments != null ? $" ({state.attachments.BuildShortLabel()})" : string.Empty;
                    names.Add($"{state.definition.displayName} [{state.definition.rarity}]{attachmentText}");
                }
            }

            return names.Count == 0 ? "Weapons: None" : $"Weapons: {string.Join(", ", names)}";
        }

        public void ResetWeapons()
        {
            foreach (WeaponRuntimeState weapon in weapons)
            {
                if (weapon.definition == null)
                {
                    continue;
                }

                if (weapon.attachments == null)
                {
                    weapon.attachments = WeaponAttachmentProfile.CreateDefault(weapon.definition);
                }

                weapon.magazineAmmo = GetMagazineSize(weapon);
                weapon.reserveAmmo = weapon.definition.startingReserveAmmo;
                weapon.unlocked = weapon.definition.slot == startingWeapon;
            }

            SelectWeapon(startingWeapon, true);
        }

        private void AddRuntimeWeapon(WeaponDefinition definition, bool unlocked)
        {
            if (definition == null)
            {
                return;
            }

            WeaponRuntimeState state = new WeaponRuntimeState
            {
                definition = definition,
                attachments = WeaponAttachmentProfile.CreateDefault(definition),
                unlocked = unlocked,
                reserveAmmo = definition.startingReserveAmmo
            };

            state.magazineAmmo = GetMagazineSize(state);
            InitializeWeaponState(state);
            weapons.Add(state);
        }

        private static void InitializeWeaponState(WeaponRuntimeState weapon)
        {
            if (weapon == null || weapon.definition == null)
            {
                return;
            }

            if (weapon.attachments == null)
            {
                weapon.attachments = WeaponAttachmentProfile.CreateDefault(weapon.definition);
            }

            if (weapon.magazineAmmo <= 0)
            {
                weapon.magazineAmmo = GetMagazineSize(weapon);
            }

            if (weapon.reserveAmmo < 0)
            {
                weapon.reserveAmmo = 0;
            }
        }

        private void SelectWeapon(WeaponSlot slot, bool instant)
        {
            WeaponRuntimeState match = weapons.Find(item => item.definition != null && item.definition.slot == slot);
            if (match == null || !match.unlocked)
            {
                return;
            }

            if (reloadRoutine != null)
            {
                StopCoroutine(reloadRoutine);
                reloadRoutine = null;
            }

            if (instant)
            {
                currentWeapon = match;
                RaiseWeaponChanged();
                return;
            }

            if (switchRoutine != null)
            {
                StopCoroutine(switchRoutine);
            }

            switchRoutine = StartCoroutine(SwitchRoutine(match));
        }

        private IEnumerator SwitchRoutine(WeaponRuntimeState nextWeapon)
        {
            fireHeld = false;
            onWeaponSwitchStarted.Invoke();
            RuntimeAudioBank.Instance?.PlaySwitch(transform.position);
            float duration = nextWeapon != null && nextWeapon.definition != null ? nextWeapon.definition.switchTime : switchDuration;
            fireLockUntilTime = Time.time + duration + switchFireLockout;
            yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
            currentWeapon = nextWeapon;
            RaiseWeaponChanged();
            onWeaponSwitchFinished.Invoke();
            switchRoutine = null;
        }

        private void TryFire()
        {
            if (currentWeapon == null || currentWeapon.definition == null || reloadRoutine != null || switchRoutine != null)
            {
                return;
            }

            if (!controlsEnabled)
            {
                fireHeld = false;
                return;
            }

            WeaponDefinition definition = currentWeapon.definition;
            if (Time.time < nextFireTime || Time.time < fireLockUntilTime)
            {
                return;
            }

            if (definition.usesAmmo && currentWeapon.magazineAmmo <= 0)
            {
                RequestReload();
                return;
            }

            float fireRateModifier = currentWeapon.attachments != null ? currentWeapon.attachments.FireRateMultiplier : 1f;
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, definition.fireRate * fireRateModifier);
            if (definition.usesAmmo)
            {
                currentWeapon.magazineAmmo--;
            }

            recoilHeat = Mathf.Clamp01(recoilHeat + recoilHeatPerShot);
            fireBufferedUntilTime = 0f;
            FireRay(definition);
            SpawnMuzzleFlash(definition);
            SpawnShellEjection();
            ApplyRecoil(definition);
            RuntimeAudioBank.Instance?.PlayWeaponFire(definition.slot, muzzlePoint != null ? muzzlePoint.position : transform.position);
            onFired.Invoke();
            RaiseAmmoChanged();

            if (definition.usesAmmo && currentWeapon.magazineAmmo <= 0 && currentWeapon.reserveAmmo > 0)
            {
                RequestReload();
            }
        }

        private void FireRay(WeaponDefinition definition)
        {
            if (aimCamera == null)
            {
                return;
            }

            int pellets = Mathf.Max(1, definition.pelletCount);
            for (int pellet = 0; pellet < pellets; pellet++)
            {
                Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                float stanceSpread = recoilReceiver != null && recoilReceiver.IsAiming ? definition.adsSpreadMultiplier : definition.hipSpreadMultiplier;
                float heatMultiplier = 1f + recoilHeat * (recoilReceiver != null && recoilReceiver.IsAiming ? 0.34f : 0.58f);
                float attachmentSpread = currentWeapon != null && currentWeapon.attachments != null ? currentWeapon.attachments.SpreadMultiplier : 1f;
                Vector3 assistedDirection = ApplyAimAssist(ray.origin, ray.direction, definition.range);
                Vector2 spread = UnityEngine.Random.insideUnitCircle * Mathf.Tan(definition.spreadAngle * stanceSpread * heatMultiplier * attachmentSpread * Mathf.Deg2Rad);
                Vector3 direction = (assistedDirection + aimCamera.transform.right * spread.x + aimCamera.transform.up * spread.y).normalized;
                Vector3 hitPoint = ray.origin + direction * definition.range;

                int hitCount = Physics.RaycastNonAlloc(ray.origin, direction, raycastHits, definition.range, hitMask, QueryTriggerInteraction.Ignore);
                Array.Sort(raycastHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = raycastHits[i];
                    if (hit.collider.transform.root == transform.root)
                    {
                        continue;
                    }

                    hitPoint = hit.point;
                    IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null && damageable.IsAlive)
                    {
                        bool headshot = IsHeadshot(hit, damageable);
                        float damage = definition.damage / pellets * (headshot ? headshotMultiplier : 1f);
                        if (headshot)
                        {
                            PlayerEquipment equipment = hit.collider.GetComponentInParent<PlayerEquipment>();
                            if (equipment != null)
                            {
                                damage = equipment.ApplyHeadshotMitigation(damage);
                            }
                        }

                        damageable.TakeDamage(damage, hit.point, hit.normal, gameObject);
                        SpawnDamageNumber(hit.point, damage, headshot);
                        SpawnHitEffect(hit.point, hit.normal);
                        RuntimeAudioBank.Instance?.PlayHit(hit.point);
                        onHitConfirmed.Invoke(hit.point, damage);
                        if (headshot)
                        {
                            onHeadshotConfirmed.Invoke();
                        }
                    }
                    else
                    {
                        SpawnHitEffect(hit.point, hit.normal);
                    }

                    break;
                }

                SpawnTracer(hitPoint, definition.tracerColor);
            }
        }

        private void SpawnMuzzleFlash(WeaponDefinition definition)
        {
            if (definition.muzzleFlashPrefab == null || muzzlePoint == null)
            {
                return;
            }

            RuntimeParticlePool.Play(definition.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, 0.24f);
        }

        private void SpawnShellEjection()
        {
            if (muzzlePoint == null)
            {
                return;
            }

            Vector3 ejectDirection = -muzzlePoint.right + muzzlePoint.up * 0.25f - muzzlePoint.forward * 0.18f;
            ejectDirection = (ejectDirection + UnityEngine.Random.insideUnitSphere * shellEjectionSpread).normalized;
            PooledShellCasing.Spawn(muzzlePoint.position - muzzlePoint.right * 0.18f, muzzlePoint.rotation, ejectDirection);
        }

        private void SpawnTracer(Vector3 endPoint, Color color)
        {
            if (muzzlePoint == null)
            {
                return;
            }

            PooledTracer tracer = PooledTracer.Get();
            LineRenderer line = tracer.Line;
            line.positionCount = 2;
            line.SetPosition(0, muzzlePoint.position);
            line.SetPosition(1, endPoint);
            line.startWidth = tracerStartWidth;
            line.endWidth = tracerEndWidth;
            if (line.sharedMaterial == null)
            {
                line.sharedMaterial = CreateLineMaterial(color);
            }

            ApplyLineMaterialColor(line.sharedMaterial, color);
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            tracer.Play(tracerLife);
        }

        private void SpawnHitEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (hitEffectPrefab == null)
            {
                return;
            }

            RuntimeParticlePool.Play(hitEffectPrefab, hitPoint + hitNormal * 0.02f, Quaternion.LookRotation(hitNormal), 0.5f);
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

            Material material = new Material(shader);
            ApplyLineMaterialColor(material, color);
            return material;
        }

        private static void ApplyLineMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void ApplyRecoil(WeaponDefinition definition)
        {
            if (recoilReceiver == null)
            {
                return;
            }

            bool aiming = recoilReceiver.IsAiming;
            float verticalKick = Mathf.Max(0.1f, definition.cameraKick);
            float horizontalKick = UnityEngine.Random.Range(-definition.cameraKick, definition.cameraKick) * (0.12f + recoilHeat * 0.11f);
            float aimScale = aiming ? adsRecoilScale : hipRecoilScale;
            float attachmentRecoil = currentWeapon != null && currentWeapon.attachments != null ? currentWeapon.attachments.RecoilMultiplier : 1f;
            float recovery = aiming ? definition.recoilRecovery * 1.15f : definition.recoilRecovery;
            recoilReceiver.AddRecoil(verticalKick * aimScale * (1f + recoilHeat * 0.16f) * attachmentRecoil, horizontalKick * aimScale * attachmentRecoil, recovery);
            recoilReceiver.SetAimFieldOfView(definition.adsFieldOfView);
        }

        private Vector3 ApplyAimAssist(Vector3 origin, Vector3 direction, float range)
        {
            if (aimAssistRadius <= 0f || aimAssistStrength <= 0f)
            {
                return direction;
            }

            float radius = recoilReceiver != null && recoilReceiver.IsAiming ? aimAssistRadius : aimAssistRadius * 0.62f;
            int hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, aimAssistHits, range, hitMask, QueryTriggerInteraction.Ignore);
            Array.Sort(aimAssistHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = aimAssistHits[i];
                if (hit.collider == null || hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                Vector3 targetDirection = (hit.collider.bounds.center - origin).normalized;
                float strength = recoilReceiver != null && recoilReceiver.IsAiming ? aimAssistStrength : aimAssistStrength * 0.55f;
                return Vector3.Slerp(direction, targetDirection, strength).normalized;
            }

            return direction;
        }

        private static bool IsHeadshot(RaycastHit hit, IDamageable damageable)
        {
            if (hit.collider != null && hit.collider.name.ToLowerInvariant().Contains("head"))
            {
                return true;
            }

            Component damageComponent = damageable as Component;
            if (damageComponent == null)
            {
                return false;
            }

            return hit.point.y > damageComponent.transform.position.y + 1.32f;
        }

        private void SpawnDamageNumber(Vector3 hitPoint, float damage, bool headshot)
        {
            if (damageNumberPrefab == null)
            {
                return;
            }

            WorldDamageNumber.Spawn(damageNumberPrefab, hitPoint + Vector3.up * 0.65f, aimCamera, damage, headshot);
        }

        private void RequestReload()
        {
            if (currentWeapon == null || currentWeapon.definition == null || reloadRoutine != null || switchRoutine != null)
            {
                return;
            }

            if (!currentWeapon.definition.usesAmmo || currentWeapon.magazineAmmo >= GetMagazineSize(currentWeapon) || currentWeapon.reserveAmmo <= 0)
            {
                return;
            }

            reloadRoutine = StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            WeaponRuntimeState reloadingWeapon = currentWeapon;
            onReloadStarted.Invoke();
            RuntimeAudioBank.Instance?.PlayReload(transform.position);
            fireHeld = false;
            float reloadModifier = reloadingWeapon.attachments != null ? reloadingWeapon.attachments.ReloadMultiplier : 1f;
            float reloadDuration = Mathf.Max(0.12f, reloadingWeapon.definition.reloadTime * reloadModifier);
            float commitTime = reloadDuration * 0.82f;
            yield return new WaitForSeconds(commitTime);

            if (currentWeapon != reloadingWeapon)
            {
                reloadRoutine = null;
                yield break;
            }

            int needed = GetMagazineSize(reloadingWeapon) - reloadingWeapon.magazineAmmo;
            int loaded = Mathf.Min(needed, reloadingWeapon.reserveAmmo);
            reloadingWeapon.magazineAmmo += loaded;
            reloadingWeapon.reserveAmmo -= loaded;
            RaiseAmmoChanged();

            yield return new WaitForSeconds(Mathf.Max(0f, reloadDuration - commitTime));

            reloadRoutine = null;
            onReloadFinished.Invoke();
            RaiseAmmoChanged();
        }

        private void RaiseWeaponChanged()
        {
            if (currentWeapon == null || currentWeapon.definition == null)
            {
                onWeaponChanged.Invoke("None");
                RaiseAmmoChanged();
                return;
            }

            onWeaponChanged.Invoke(BuildCurrentWeaponDisplayName());
            weaponModelRig?.ShowWeapon(currentWeapon.definition.slot);
            recoilReceiver?.SetAimFieldOfView(currentWeapon.definition.adsFieldOfView);
            RaiseAmmoChanged();
        }

        private void RaiseAmmoChanged()
        {
            if (currentWeapon == null || currentWeapon.definition == null)
            {
                onAmmoChanged.Invoke("None", 0, 0);
                return;
            }

            onAmmoChanged.Invoke(BuildCurrentWeaponDisplayName(), currentWeapon.magazineAmmo, currentWeapon.reserveAmmo);
        }

        private string BuildCurrentWeaponDisplayName()
        {
            if (currentWeapon == null || currentWeapon.definition == null)
            {
                return "None";
            }

            string attachmentText = currentWeapon.attachments != null ? $" | {currentWeapon.attachments.BuildShortLabel()}" : string.Empty;
            return $"{currentWeapon.definition.displayName} [{currentWeapon.definition.rarity}]{attachmentText}";
        }

        private static int GetMagazineSize(WeaponRuntimeState state)
        {
            if (state == null || state.definition == null)
            {
                return 0;
            }

            float modifier = state.attachments != null ? state.attachments.MagazineMultiplier : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(state.definition.magazineSize * modifier));
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
