using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatProjectile : MonoBehaviour
    {
        private static readonly Dictionary<CombatProjectile, Stack<CombatProjectile>> Pools = new Dictionary<CombatProjectile, Stack<CombatProjectile>>();

        [SerializeField] private bool useGravity = false;
        [SerializeField] private float gravityScale = 1f;
        [SerializeField] private GameObject impactFallbackPrefab = null;

        private CombatProjectile sourcePrefab;
        private AdvancedWeaponData weaponData;
        private ModularWeaponRuntimeContext context;
        private Vector3 velocity;
        private float remainingRange;
        private bool launched;

        public static CombatProjectile Spawn(CombatProjectile prefab, AdvancedWeaponData data, ModularWeaponRuntimeContext runtimeContext, Vector3 origin, Vector3 direction)
        {
            if (prefab == null || data == null)
            {
                return null;
            }

            CombatProjectile projectile = Get(prefab);
            projectile.transform.SetPositionAndRotation(origin, direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction.normalized) : Quaternion.identity);
            projectile.gameObject.SetActive(true);
            projectile.Launch(data, runtimeContext, direction.normalized);
            return projectile;
        }

        private void Launch(AdvancedWeaponData data, ModularWeaponRuntimeContext runtimeContext, Vector3 direction)
        {
            weaponData = data;
            context = runtimeContext;
            float speed = Mathf.Max(0.01f, data.BulletSpeed);
            velocity = direction * speed;
            remainingRange = data.Range;
            launched = true;
        }

        private void Update()
        {
            if (!launched || weaponData == null)
            {
                return;
            }

            if (useGravity)
            {
                velocity += Physics.gravity * Mathf.Max(0f, gravityScale) * Time.deltaTime;
            }

            Vector3 current = transform.position;
            Vector3 next = current + velocity * Time.deltaTime;
            Vector3 segment = next - current;
            float distance = segment.magnitude;
            if (distance > 0.0001f)
            {
                if (Physics.Raycast(current, segment / distance, out RaycastHit hit, distance, context != null ? context.hitMask : ~0, QueryTriggerInteraction.Ignore) && !IsOwnCollider(hit.collider))
                {
                    HandleHit(hit);
                    Release();
                    return;
                }

                remainingRange -= distance;
                transform.SetPositionAndRotation(next, velocity.sqrMagnitude > 0.001f ? Quaternion.LookRotation(velocity.normalized) : transform.rotation);
            }

            if (remainingRange <= 0f)
            {
                Release();
            }
        }

        private void HandleHit(RaycastHit hit)
        {
            GameObject fallback = impactFallbackPrefab != null ? impactFallbackPrefab : context != null ? context.impactFallbackPrefab : null;
            CombatImpactUtility.PlaySurfaceImpact(weaponData, hit, fallback);
            CombatImpactUtility.ApplyDamage(weaponData, hit.collider, hit.point, hit.normal, context != null ? context.owner : gameObject);
        }

        private bool IsOwnCollider(Collider collider)
        {
            return collider != null && context != null && context.owner != null && collider.transform.root == context.owner.transform.root;
        }

        private static CombatProjectile Get(CombatProjectile prefab)
        {
            if (Pools.TryGetValue(prefab, out Stack<CombatProjectile> pool))
            {
                while (pool.Count > 0)
                {
                    CombatProjectile candidate = pool.Pop();
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }

            CombatProjectile instance = Instantiate(prefab);
            instance.sourcePrefab = prefab;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void Release()
        {
            launched = false;
            weaponData = null;
            context = null;
            velocity = Vector3.zero;
            gameObject.SetActive(false);

            CombatProjectile prefab = sourcePrefab != null ? sourcePrefab : this;
            if (!Pools.TryGetValue(prefab, out Stack<CombatProjectile> pool))
            {
                pool = new Stack<CombatProjectile>(16);
                Pools[prefab] = pool;
            }

            pool.Push(this);
        }
    }
}
