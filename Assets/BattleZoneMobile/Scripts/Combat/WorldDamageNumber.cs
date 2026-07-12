using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    public class WorldDamageNumber : MonoBehaviour
    {
        private const int MaxPoolSize = 64;
        private static readonly Dictionary<WorldDamageNumber, Stack<WorldDamageNumber>> Pools = new Dictionary<WorldDamageNumber, Stack<WorldDamageNumber>>();

        [SerializeField] private TextMesh textMesh;
        [SerializeField] private float lifeTime = 0.85f;
        [SerializeField] private float riseSpeed = 1.2f;
        [SerializeField] private Color startColor = new Color(1f, 0.86f, 0.28f, 1f);

        private WorldDamageNumber sourcePrefab;
        private float age;
        private Camera targetCamera;

        public static WorldDamageNumber Spawn(WorldDamageNumber prefab, Vector3 position, Camera camera, float damage, bool headshot)
        {
            if (prefab == null)
            {
                return null;
            }

            WorldDamageNumber number = Get(prefab);
            number.transform.position = position;
            number.transform.rotation = Quaternion.identity;
            number.gameObject.SetActive(true);
            number.Configure(damage, camera, headshot);
            return number;
        }

        public void Configure(float damage, Camera camera)
        {
            Configure(damage, camera, false);
        }

        public void Configure(float damage, Camera camera, bool headshot)
        {
            targetCamera = camera != null ? camera : Camera.main;
            age = 0f;
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMesh>();
            }

            if (textMesh != null)
            {
                startColor = headshot ? new Color(1f, 0.32f, 0.16f, 1f) : new Color(1f, 0.86f, 0.28f, 1f);
                textMesh.text = headshot ? $"HEAD {Mathf.CeilToInt(damage)}" : Mathf.CeilToInt(damage).ToString();
                textMesh.color = startColor;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.characterSize = headshot ? 0.15f : 0.18f;
                textMesh.fontSize = 64;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            if (targetCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position, Vector3.up);
            }

            if (textMesh != null)
            {
                Color color = startColor;
                color.a = Mathf.Clamp01(1f - age / lifeTime);
                textMesh.color = color;
            }

            if (age >= lifeTime)
            {
                ReturnToPool();
            }
        }

        private static WorldDamageNumber Get(WorldDamageNumber prefab)
        {
            if (Pools.TryGetValue(prefab, out Stack<WorldDamageNumber> pool))
            {
                while (pool.Count > 0)
                {
                    WorldDamageNumber item = pool.Pop();
                    if (item != null)
                    {
                        return item;
                    }
                }
            }

            WorldDamageNumber instance = Instantiate(prefab);
            instance.sourcePrefab = prefab;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void ReturnToPool()
        {
            if (sourcePrefab == null)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
            if (!Pools.TryGetValue(sourcePrefab, out Stack<WorldDamageNumber> pool))
            {
                pool = new Stack<WorldDamageNumber>(MaxPoolSize);
                Pools[sourcePrefab] = pool;
            }

            if (pool.Count < MaxPoolSize)
            {
                pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
