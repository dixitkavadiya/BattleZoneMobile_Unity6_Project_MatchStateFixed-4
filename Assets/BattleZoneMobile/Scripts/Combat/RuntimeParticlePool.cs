using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    public class RuntimeParticlePool : MonoBehaviour
    {
        private const int MaxPerPrefab = 36;
        private static readonly Dictionary<ParticleSystem, Stack<ParticleSystem>> Pools = new Dictionary<ParticleSystem, Stack<ParticleSystem>>();
        private static readonly Dictionary<ParticleSystem, ParticleSystem> Sources = new Dictionary<ParticleSystem, ParticleSystem>();

        private ParticleSystem particles;
        private float remainingLife;

        public static ParticleSystem Play(ParticleSystem prefab, Vector3 position, Quaternion rotation, float lifeTime)
        {
            if (prefab == null)
            {
                return null;
            }

            ParticleSystem instance = Get(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            RuntimeParticlePool pooled = instance.GetComponent<RuntimeParticlePool>();
            if (pooled == null)
            {
                pooled = instance.gameObject.AddComponent<RuntimeParticlePool>();
            }

            pooled.particles = instance;
            pooled.remainingLife = Mathf.Max(0.05f, lifeTime);
            pooled.enabled = true;
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            instance.Play(true);
            return instance;
        }

        private void Update()
        {
            remainingLife -= Time.deltaTime;
            if (remainingLife > 0f)
            {
                return;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (particles == null)
            {
                enabled = false;
                gameObject.SetActive(false);
                return;
            }

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enabled = false;
            gameObject.SetActive(false);

            if (!Sources.TryGetValue(particles, out ParticleSystem source) || source == null)
            {
                Destroy(gameObject);
                return;
            }

            if (!Pools.TryGetValue(source, out Stack<ParticleSystem> pool))
            {
                pool = new Stack<ParticleSystem>(MaxPerPrefab);
                Pools[source] = pool;
            }

            if (pool.Count < MaxPerPrefab)
            {
                pool.Push(particles);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private static ParticleSystem Get(ParticleSystem prefab)
        {
            if (Pools.TryGetValue(prefab, out Stack<ParticleSystem> pool))
            {
                while (pool.Count > 0)
                {
                    ParticleSystem item = pool.Pop();
                    if (item != null)
                    {
                        return item;
                    }
                }
            }

            ParticleSystem instance = Instantiate(prefab);
            Sources[instance] = prefab;
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
