using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BattleZoneMobile
{
    public class BotManager : MonoBehaviour
    {
        [SerializeField] private BotAI botPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private int botCount = 5;
        [SerializeField] private Vector2 spawnArea = new Vector2(130f, 130f);
        [SerializeField] private float spawnHeight = 1f;
        [SerializeField] private Transform[] explicitSpawnPoints;
        [SerializeField] private Transform[] coverPoints;
        [SerializeField] private BotAI.DifficultyPreset difficulty = BotAI.DifficultyPreset.Normal;
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField] private float dropLateralSpread = 118f;
        [SerializeField] private float dropRouteJitter = 0.11f;

        private readonly List<BotAI> activeBots = new List<BotAI>();
        private GameManager gameManager;

        public void ConfigureForRuntime(BotAI prefab, int count, Vector2 area)
        {
            botPrefab = prefab;
            botCount = Mathf.Max(0, count);
            spawnArea = area;
        }

        public void SetCoverPoints(Transform[] points)
        {
            coverPoints = points;
        }

        public int AliveCount
        {
            get
            {
                int count = 0;
                foreach (BotAI bot in activeBots)
                {
                    if (bot != null && !bot.IsDead)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int ActiveCount => activeBots.Count;

        public int StuckCount
        {
            get
            {
                int count = 0;
                foreach (BotAI bot in activeBots)
                {
                    if (bot != null && !bot.IsDead && bot.IsStuck)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void SetDifficulty(BotAI.DifficultyPreset preset)
        {
            difficulty = preset;
        }

        public void SetSafeZone(SafeZoneController zone)
        {
            safeZone = zone;
            foreach (BotAI bot in activeBots)
            {
                if (bot != null)
                {
                    bot.SetSafeZone(safeZone);
                }
            }
        }

        public void SetCombatLocked(bool locked)
        {
            foreach (BotAI bot in activeBots)
            {
                if (bot != null && !bot.IsDead)
                {
                    bot.SetCombatLocked(locked);
                }
            }
        }

        public void BroadcastGunshot(Vector3 position)
        {
            foreach (BotAI bot in activeBots)
            {
                if (bot != null && !bot.IsDead)
                {
                    bot.ReactToGunshot(position);
                }
            }
        }

        public void Initialize(Transform playerTarget, GameManager owner)
        {
            player = playerTarget;
            gameManager = owner;
        }

        public void SpawnBots()
        {
            ClearBots();

            if (botPrefab == null || player == null)
            {
                return;
            }

            for (int i = 0; i < botCount; i++)
            {
                Vector3 point = GetSpawnPoint(i);
                BotAI bot = Instantiate(botPrefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                bot.gameObject.SetActive(true);
                bot.ConfigureCoverPoints(coverPoints);
                bot.EquipRuntimeWeapon(GetBotLoadout(i));
                bot.ApplyDifficulty(difficulty);
                bot.Initialize(player, gameManager);
                bot.SetSafeZone(safeZone);
                activeBots.Add(bot);
            }
        }

        public void BeginBattleRoyaleDrop(Vector3 routeStart, Vector3 routeEnd)
        {
            if (activeBots.Count == 0)
            {
                return;
            }

            Vector3 route = routeEnd - routeStart;
            route.y = 0f;
            if (route.sqrMagnitude < 0.1f)
            {
                route = Vector3.forward;
            }

            route.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, route).normalized;
            int aliveIndex = 0;
            int aliveTotal = AliveCount;
            for (int i = 0; i < activeBots.Count; i++)
            {
                BotAI bot = activeBots[i];
                if (bot == null || bot.IsDead)
                {
                    continue;
                }

                float t = (aliveIndex + 1f) / (Mathf.Max(1, aliveTotal) + 1f);
                t = Mathf.Clamp01(t + Random.Range(-dropRouteJitter, dropRouteJitter));
                float laneSide = aliveIndex % 2 == 0 ? 1f : -1f;
                float lateralOffset = laneSide * Random.Range(dropLateralSpread * 0.24f, dropLateralSpread);
                Vector3 landing = FindGroundPoint(Vector3.Lerp(routeStart, routeEnd, t) + side * lateralOffset + route * Random.Range(-18f, 18f));
                bot.BeginParachuteDrop(landing, Random.Range(0.15f, 4.6f));
                aliveIndex++;
            }
        }

        public void ClearBots()
        {
            foreach (BotAI bot in activeBots)
            {
                if (bot != null)
                {
                    Destroy(bot.gameObject);
                }
            }

            activeBots.Clear();
        }

        private Vector3 GetSpawnPoint(int index)
        {
            if (explicitSpawnPoints != null && index < explicitSpawnPoints.Length && explicitSpawnPoints[index] != null)
            {
                return explicitSpawnPoints[index].position;
            }

            Vector2 random = Random.insideUnitCircle * 0.5f;
            Vector3 candidate = transform.position + new Vector3(random.x * spawnArea.x, 80f, random.y * spawnArea.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, Mathf.Max(spawnArea.x, spawnArea.y), NavMesh.AllAreas))
            {
                return navHit.position;
            }

            if (Physics.Raycast(candidate, Vector3.down, out RaycastHit groundHit, 200f, ~0, QueryTriggerInteraction.Ignore))
            {
                return groundHit.point + Vector3.up * spawnHeight;
            }

            candidate.y = spawnHeight;
            return candidate;
        }

        private static Vector3 FindGroundPoint(Vector3 source)
        {
            Vector3 origin = new Vector3(source.x, 160f, source.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 320f, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * 1.05f;
            }

            return new Vector3(source.x, 1.05f, source.z);
        }

        private static WeaponSlot GetBotLoadout(int index)
        {
            WeaponSlot[] loadouts =
            {
                WeaponSlot.AssaultRifle,
                WeaponSlot.SMG,
                WeaponSlot.Shotgun,
                WeaponSlot.Sniper,
                WeaponSlot.Pistol
            };

            return loadouts[Mathf.Abs(index) % loadouts.Length];
        }
    }
}
