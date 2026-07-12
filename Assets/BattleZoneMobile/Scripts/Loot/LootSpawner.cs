using UnityEngine;

namespace BattleZoneMobile
{
    public class LootSpawner : MonoBehaviour
    {
        [SerializeField] private LootItem[] lootPrefabs;
        [SerializeField] private int spawnCount = 28;
        [SerializeField] private Vector2 mapSize = new Vector2(180f, 180f);
        [SerializeField] private float spawnHeight = 1f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private int maxSpawnAttempts = 18;
        [SerializeField] private Vector3[] clusterCenters;
        [SerializeField] private float clusterRadius = 42f;
        [SerializeField, Range(0f, 1f)] private float clusterChance = 0.62f;

        private readonly Collider[] clearanceHits = new Collider[12];

        public void ConfigureForRuntime(LootItem[] prefabs, int count, Vector2 size, LayerMask groundLayerMask)
        {
            lootPrefabs = prefabs;
            spawnCount = Mathf.Max(0, count);
            mapSize = size;
            groundMask = groundLayerMask;
        }

        public void ConfigureClusters(Vector3[] centers, float radius, float chance)
        {
            clusterCenters = centers;
            clusterRadius = Mathf.Max(4f, radius);
            clusterChance = Mathf.Clamp01(chance);
        }

        public void SpawnLoot()
        {
            if (lootPrefabs == null || lootPrefabs.Length == 0)
            {
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                LootItem prefab = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
                if (prefab == null)
                {
                    continue;
                }

                Vector3 point = GetRandomPoint();
                LootItem item = Instantiate(prefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                item.gameObject.SetActive(true);
            }
        }

        public void ClearLoot()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private Vector3 GetRandomPoint()
        {
            for (int attempt = 0; attempt < Mathf.Max(1, maxSpawnAttempts); attempt++)
            {
                Vector3 world = BuildCandidatePoint();
                if (Physics.Raycast(world, Vector3.down, out RaycastHit hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 point = hit.point + Vector3.up * spawnHeight;
                    if (IsPointClear(point))
                    {
                        return point;
                    }
                }
            }

            Vector3 fallback = transform.position;
            fallback.y = spawnHeight;
            return fallback;
        }

        private Vector3 BuildCandidatePoint()
        {
            bool useCluster = clusterCenters != null &&
                              clusterCenters.Length > 0 &&
                              Random.value < clusterChance;
            if (useCluster)
            {
                Vector3 center = clusterCenters[Random.Range(0, clusterCenters.Length)];
                Vector2 random = Random.insideUnitCircle * clusterRadius;
                return new Vector3(center.x + random.x, 140f, center.z + random.y);
            }

            Vector3 local = new Vector3(
                Random.Range(-mapSize.x * 0.5f, mapSize.x * 0.5f),
                140f,
                Random.Range(-mapSize.y * 0.5f, mapSize.y * 0.5f));

            return transform.position + local;
        }

        private bool IsPointClear(Vector3 point)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(point + Vector3.up * 1.15f, 0.62f, clearanceHits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                Collider item = clearanceHits[i];
                if (item == null || item.isTrigger || item.GetComponentInParent<LootItem>() != null)
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
