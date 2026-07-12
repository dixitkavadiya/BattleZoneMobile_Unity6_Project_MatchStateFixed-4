using System;
using System.Collections;
using UnityEngine;

namespace BattleZoneMobile
{
    [Serializable]
    public class ModularWeaponRuntimeContext
    {
        public GameObject owner;
        public Camera aimCamera;
        public Transform muzzlePoint;
        public Animator visualAnimator;
        public ThirdPersonMobileController controller;
        public CombatRecoilApplicator recoilApplicator;
        public GameObject impactFallbackPrefab;
        public LayerMask hitMask = ~0;
    }

    public abstract class ModularWeaponBase : MonoBehaviour
    {
        [Header("Weapon Data")]
        [SerializeField] private AdvancedWeaponData weaponData;
        [SerializeField] private CombatProjectile projectilePrefab = null;

        [Header("Runtime References")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Animator visualAnimator;
        [SerializeField] private ThirdPersonMobileController controller;
        [SerializeField] private CombatRecoilApplicator recoilApplicator;
        [SerializeField] private GameObject impactFallbackPrefab;
        [SerializeField] private LayerMask hitMask = ~0;

        private readonly RaycastHit[] raycastHits = new RaycastHit[32];
        private Coroutine reloadRoutine;
        private Coroutine burstRoutine;
        private float nextFireTime;
        private float actionLockedUntilTime;
        private float spreadBloom;
        private bool equipped = true;
        private CombatFireMode currentFireMode;

        public abstract CombatWeaponType WeaponType { get; }
        public AdvancedWeaponData Data => weaponData;
        public int MagazineAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        public bool IsReloading => reloadRoutine != null;
        public bool IsBursting => burstRoutine != null;
        public bool IsEquipped => equipped;
        public CombatFireMode CurrentFireMode => currentFireMode;

        public virtual void Initialize(ModularWeaponRuntimeContext context, AdvancedWeaponData data)
        {
            weaponData = data;
            if (context != null)
            {
                aimCamera = context.aimCamera;
                muzzlePoint = context.muzzlePoint;
                visualAnimator = context.visualAnimator;
                controller = context.controller;
                recoilApplicator = context.recoilApplicator;
                impactFallbackPrefab = context.impactFallbackPrefab;
                hitMask = context.hitMask;
            }

            MagazineAmmo = weaponData != null ? weaponData.MagazineSize : 0;
            ReserveAmmo = weaponData != null ? weaponData.StartingReserveAmmo : 0;
            currentFireMode = weaponData != null ? weaponData.PrimaryFireMode : CombatFireMode.SemiAuto;
            spreadBloom = 0f;
        }

        protected virtual void Update()
        {
            if (weaponData != null && weaponData.Spread != null)
            {
                spreadBloom = Mathf.MoveTowards(spreadBloom, 0f, Time.deltaTime * weaponData.Spread.bloomRecoverySpeed);
            }
        }

        public virtual void Equip()
        {
            equipped = true;
            actionLockedUntilTime = Time.time + (weaponData != null ? weaponData.EquipTime : 0f);
            PlayClip(weaponData != null && weaponData.Audio != null ? weaponData.Audio.equip : null);
            SetAnimatorTrigger(weaponData != null && weaponData.AnimationHooks != null ? weaponData.AnimationHooks.equipTrigger : null);
        }

        public virtual void Unequip()
        {
            equipped = false;
            StopBurst();
            PlayClip(weaponData != null && weaponData.Audio != null ? weaponData.Audio.unequip : null);
            SetAnimatorTrigger(weaponData != null && weaponData.AnimationHooks != null ? weaponData.AnimationHooks.unequipTrigger : null);
        }

        public virtual bool TryFire(bool triggerHeld, bool aiming, bool moving, bool crouching, bool prone)
        {
            return TryFire(currentFireMode, triggerHeld, aiming, moving, crouching, prone);
        }

        public virtual bool TryFire(CombatFireMode requestedFireMode, bool triggerHeld, bool aiming, bool moving, bool crouching, bool prone)
        {
            if (!equipped || weaponData == null || IsReloading || Time.time < nextFireTime)
            {
                return false;
            }

            if (Time.time < actionLockedUntilTime || IsBursting)
            {
                return false;
            }

            CombatFireMode fireMode = weaponData.SupportsFireMode(requestedFireMode) ? requestedFireMode : weaponData.PrimaryFireMode;
            currentFireMode = fireMode;
            if (fireMode == CombatFireMode.FullAuto && !triggerHeld)
            {
                return false;
            }

            if (weaponData.UsesAmmo && MagazineAmmo <= 0)
            {
                DryFire();
                return false;
            }

            if (fireMode == CombatFireMode.Burst)
            {
                burstRoutine = StartCoroutine(BurstRoutine(aiming, moving, crouching, prone));
                return true;
            }

            FireOneShot(aiming, moving, crouching, prone);
            nextFireTime = Time.time + ResolvePostShotDelay(fireMode);
            actionLockedUntilTime = Mathf.Max(actionLockedUntilTime, Time.time + ResolveActionCooldown(fireMode));
            return true;
        }

        public virtual CombatFireMode SwitchFireMode()
        {
            if (weaponData == null)
            {
                currentFireMode = CombatFireMode.SemiAuto;
                return currentFireMode;
            }

            currentFireMode = weaponData.GetNextFireMode(currentFireMode);
            return currentFireMode;
        }

        public virtual void AddReserveAmmo(int amount)
        {
            ReserveAmmo = Mathf.Max(0, ReserveAmmo + Mathf.Max(0, amount));
        }

        public virtual void SetAmmo(int magazine, int reserve)
        {
            int magazineSize = weaponData != null ? weaponData.MagazineSize : Mathf.Max(1, magazine);
            MagazineAmmo = Mathf.Clamp(magazine, 0, magazineSize);
            ReserveAmmo = Mathf.Max(0, reserve);
        }

        private void FireOneShot(bool aiming, bool moving, bool crouching, bool prone)
        {
            if (weaponData.UsesAmmo)
            {
                MagazineAmmo = Mathf.Max(0, MagazineAmmo - 1);
            }

            FireInternal(aiming, moving, crouching, prone);
            spreadBloom = Mathf.Min(weaponData.Spread != null ? weaponData.Spread.maxBloomDegrees : spreadBloom, spreadBloom + (weaponData.Spread != null ? weaponData.Spread.bloomPerShotDegrees : 0f));
            recoilApplicator?.Apply(weaponData, aiming, spreadBloom);
            PlayClip(weaponData.Audio != null ? weaponData.Audio.shoot : null);
            SetAnimatorTrigger(weaponData.AnimationHooks != null ? weaponData.AnimationHooks.fireTrigger : null);
        }

        public virtual void RequestReload()
        {
            if (weaponData == null || IsReloading || !weaponData.UsesAmmo || MagazineAmmo >= weaponData.MagazineSize || ReserveAmmo <= 0)
            {
                return;
            }

            reloadRoutine = StartCoroutine(ReloadRoutine());
        }

        public virtual void OnAnimationFireEvent()
        {
        }

        public virtual void OnAnimationReloadCommit()
        {
            CommitReload();
        }

        public virtual void OnAnimationReloadFinished()
        {
        }

        protected virtual void FireInternal(bool aiming, bool moving, bool crouching, bool prone)
        {
            switch (weaponData.Delivery)
            {
                case CombatWeaponDelivery.Projectile:
                case CombatWeaponDelivery.Throwable:
                    FireProjectile(aiming, moving, crouching, prone);
                    break;
                case CombatWeaponDelivery.Melee:
                    FireMelee(aiming, moving, crouching, prone);
                    break;
                default:
                    FireRaycast(aiming, moving, crouching, prone);
                    break;
            }
        }

        protected virtual void FireRaycast(bool aiming, bool moving, bool crouching, bool prone)
        {
            Ray ray = BuildAimRay();
            int pellets = Mathf.Max(1, weaponData.PelletCount);
            for (int pellet = 0; pellet < pellets; pellet++)
            {
                Vector3 direction = ApplySpread(ray.direction, aiming, moving, crouching, prone);
                int hitCount = Physics.RaycastNonAlloc(ray.origin, direction, raycastHits, weaponData.Range, hitMask, QueryTriggerInteraction.Ignore);
                Array.Sort(raycastHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = raycastHits[i];
                    if (IsOwnCollider(hit.collider))
                    {
                        continue;
                    }

                    ApplyHit(hit, pellets);
                    break;
                }
            }
        }

        protected virtual void FireProjectile(bool aiming, bool moving, bool crouching, bool prone)
        {
            if (projectilePrefab == null)
            {
                FireRaycast(aiming, moving, crouching, prone);
                return;
            }

            Ray ray = BuildAimRay();
            Vector3 direction = ApplySpread(ray.direction, aiming, moving, crouching, prone);
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : ray.origin;
            CombatProjectile.Spawn(projectilePrefab, weaponData, BuildContext(), origin, direction);
        }

        protected virtual void FireMelee(bool aiming, bool moving, bool crouching, bool prone)
        {
            Ray ray = BuildAimRay();
            float radius = 0.55f;
            if (Physics.SphereCast(ray.origin, radius, ray.direction, out RaycastHit hit, Mathf.Min(weaponData.Range, 3f), hitMask, QueryTriggerInteraction.Ignore) && !IsOwnCollider(hit.collider))
            {
                ApplyHit(hit, 1);
            }
        }

        protected virtual Ray BuildAimRay()
        {
            if (aimCamera != null)
            {
                return aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }

            Transform origin = muzzlePoint != null ? muzzlePoint : transform;
            return new Ray(origin.position, origin.forward);
        }

        protected virtual Vector3 ApplySpread(Vector3 direction, bool aiming, bool moving, bool crouching, bool prone)
        {
            if (weaponData == null || weaponData.Spread == null)
            {
                return direction.normalized;
            }

            float spreadDegrees = weaponData.Spread.ResolveSpread(aiming, moving, crouching, prone, spreadBloom);
            if (spreadDegrees <= 0.001f || aimCamera == null)
            {
                return direction.normalized;
            }

            Vector2 spread = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
            return (direction + aimCamera.transform.right * spread.x + aimCamera.transform.up * spread.y).normalized;
        }

        protected virtual void ApplyHit(RaycastHit hit, int damageDivisor)
        {
            CombatImpactUtility.PlaySurfaceImpact(weaponData, hit, impactFallbackPrefab);
            IDamageable damageable = hit.collider != null ? hit.collider.GetComponentInParent<IDamageable>() : null;
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            float damage = CombatImpactUtility.ResolveDamage(weaponData, hit.collider) / Mathf.Max(1, damageDivisor);
            if (damage > 0f)
            {
                damageable.TakeDamage(damage, hit.point, hit.normal, gameObject);
            }
        }

        protected bool IsOwnCollider(Collider collider)
        {
            return collider != null && collider.transform.root == transform.root;
        }

        protected ModularWeaponRuntimeContext BuildContext()
        {
            return new ModularWeaponRuntimeContext
            {
                owner = gameObject,
                aimCamera = aimCamera,
                muzzlePoint = muzzlePoint,
                visualAnimator = visualAnimator,
                controller = controller,
                recoilApplicator = recoilApplicator,
                impactFallbackPrefab = impactFallbackPrefab,
                hitMask = hitMask
            };
        }

        private IEnumerator ReloadRoutine()
        {
            PlayClip(weaponData.Audio != null ? weaponData.Audio.reload : null);
            bool emptyReload = MagazineAmmo <= 0;
            string trigger = weaponData.AnimationHooks != null
                ? emptyReload ? weaponData.AnimationHooks.emptyReloadTrigger : weaponData.AnimationHooks.tacticalReloadTrigger
                : null;
            if (string.IsNullOrWhiteSpace(trigger) && weaponData.AnimationHooks != null)
            {
                trigger = weaponData.AnimationHooks.reloadTrigger;
            }

            SetAnimatorTrigger(trigger);

            float duration = emptyReload ? weaponData.EmptyReloadTime : weaponData.TacticalReloadTime;
            if (duration <= 0f)
            {
                duration = weaponData.ReloadTime;
            }

            float commitTime = duration * (weaponData.AnimationHooks != null ? weaponData.AnimationHooks.reloadCommitNormalizedTime : 0.82f);
            yield return new WaitForSeconds(Mathf.Max(0f, commitTime));
            CommitReload();
            yield return new WaitForSeconds(Mathf.Max(0f, duration - commitTime));
            reloadRoutine = null;
        }

        private void CommitReload()
        {
            if (weaponData == null || !weaponData.UsesAmmo)
            {
                return;
            }

            int needed = Mathf.Max(0, weaponData.MagazineSize - MagazineAmmo);
            int loaded = Mathf.Min(needed, ReserveAmmo);
            MagazineAmmo += loaded;
            ReserveAmmo -= loaded;
        }

        private void DryFire()
        {
            nextFireTime = Time.time + 0.16f;
            PlayClip(weaponData != null && weaponData.Audio != null ? weaponData.Audio.dryFire : null);
            SetAnimatorTrigger(weaponData != null && weaponData.AnimationHooks != null ? weaponData.AnimationHooks.dryFireTrigger : null);
        }

        private IEnumerator BurstRoutine(bool aiming, bool moving, bool crouching, bool prone)
        {
            int shots = Mathf.Max(1, weaponData.BurstShotCount);
            for (int i = 0; i < shots; i++)
            {
                if (!equipped || IsReloading || (weaponData.UsesAmmo && MagazineAmmo <= 0))
                {
                    break;
                }

                FireOneShot(aiming, moving, crouching, prone);
                if (i < shots - 1)
                {
                    yield return new WaitForSeconds(weaponData.BurstShotInterval);
                }
            }

            nextFireTime = Time.time + ResolvePostShotDelay(CombatFireMode.Burst);
            actionLockedUntilTime = Mathf.Max(actionLockedUntilTime, Time.time + ResolveActionCooldown(CombatFireMode.Burst));
            burstRoutine = null;
        }

        private void StopBurst()
        {
            if (burstRoutine != null)
            {
                StopCoroutine(burstRoutine);
                burstRoutine = null;
            }
        }

        private float ResolvePostShotDelay(CombatFireMode mode)
        {
            float baseDelay = 1f / Mathf.Max(0.01f, weaponData.FireRate);
            if (mode == CombatFireMode.Burst)
            {
                return Mathf.Max(baseDelay, weaponData.BurstShotInterval * Mathf.Max(1, weaponData.BurstShotCount));
            }

            return baseDelay;
        }

        private float ResolveActionCooldown(CombatFireMode mode)
        {
            switch (mode)
            {
                case CombatFireMode.BoltAction:
                    SetAnimatorTrigger(weaponData.AnimationHooks != null ? weaponData.AnimationHooks.boltCycleTrigger : null);
                    return weaponData.BoltActionCooldown;
                case CombatFireMode.PumpAction:
                    SetAnimatorTrigger(weaponData.AnimationHooks != null ? weaponData.AnimationHooks.pumpCycleTrigger : null);
                    return weaponData.PumpActionCooldown;
                default:
                    return 0f;
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            Vector3 position = muzzlePoint != null ? muzzlePoint.position : transform.position;
            AudioSource.PlayClipAtPoint(clip, position);
        }

        private void SetAnimatorTrigger(string triggerName)
        {
            if (visualAnimator == null || string.IsNullOrWhiteSpace(triggerName) || !HasAnimatorTrigger(visualAnimator, triggerName))
            {
                return;
            }

            visualAnimator.SetTrigger(triggerName);
        }

        private static bool HasAnimatorTrigger(Animator animator, string triggerName)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    return true;
                }
            }

            return false;
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
