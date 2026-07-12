using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BattleZoneMobile
{
    [Serializable]
    public class ZoneChangedEvent : UnityEvent<float, float, bool>
    {
    }

    public class SafeZoneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Health playerHealth;
        [SerializeField] private Transform zoneVisual;
        [SerializeField] private Transform nextZoneVisual;

        [Header("Zone")]
        [SerializeField] private float initialRadius = 95f;
        [SerializeField] private float finalRadius = 12f;
        [SerializeField] private float shrinkInterval = 60f;
        [SerializeField] private float shrinkDuration = 12f;
        [SerializeField, Range(0.1f, 0.95f)] private float radiusMultiplierPerShrink = 0.72f;
        [SerializeField] private float outsideDamagePerSecond = 6f;
        [SerializeField] private float[] damageByPhase = { 4f, 6f, 8f, 11f, 15f };
        [SerializeField] private float[] holdSecondsByPhase = { 52f, 48f, 44f, 38f, 32f };
        [SerializeField] private float[] shrinkSecondsByPhase = { 16f, 15f, 14f, 13f, 12f };
        [SerializeField] private Vector2 mapHalfExtents = new Vector2(230f, 230f);
        [SerializeField] private bool randomizeCenterEachMatch = true;
        [SerializeField, Range(0.05f, 0.65f)] private float centerRandomRange = 0.34f;

        public ZoneChangedEvent onZoneChanged = new ZoneChangedEvent();

        private float currentRadius;
        private float nextRadius;
        private float nextShrinkTime;
        private bool active;
        private bool shrinking;
        private int damagePhase;
        private float lastZoneWarningTime;

        public float CurrentRadius => currentRadius;
        public float NextRadius => nextRadius;
        public bool IsActive => active;
        public Vector3 ZoneCenter => transform.position;
        public float SecondsUntilShrink => Mathf.Max(0f, nextShrinkTime - Time.time);
        public int DamagePhase => damagePhase + 1;
        public string CurrentPhase => !active ? "Inactive" : shrinking ? "Shrinking" : "Holding";

        public void ConfigureForRuntime(Transform playerTarget, Health health, Transform visual)
        {
            ConfigureForRuntime(playerTarget, health, visual, null);
        }

        public void ConfigureForRuntime(Transform playerTarget, Health health, Transform visual, Transform nextVisual)
        {
            player = playerTarget;
            playerHealth = health;
            zoneVisual = visual;
            nextZoneVisual = nextVisual;
        }

        public void ConfigureZoneScale(float startRadius, float endRadius, float intervalSeconds)
        {
            initialRadius = Mathf.Max(12f, startRadius);
            finalRadius = Mathf.Clamp(endRadius, 8f, initialRadius);
            shrinkInterval = Mathf.Max(20f, intervalSeconds);
        }

        public void ConfigureMapBounds(Vector2 halfExtents, bool randomizeCenter)
        {
            mapHalfExtents = new Vector2(Mathf.Max(80f, halfExtents.x), Mathf.Max(80f, halfExtents.y));
            randomizeCenterEachMatch = randomizeCenter;
        }

        public void PrepareForMatch()
        {
            if (randomizeCenterEachMatch)
            {
                Vector2 random = UnityEngine.Random.insideUnitCircle * centerRandomRange;
                Vector3 center = new Vector3(random.x * mapHalfExtents.x, transform.position.y, random.y * mapHalfExtents.y);
                transform.position = center;
            }

            currentRadius = initialRadius;
            nextRadius = CalculateNextRadius(currentRadius);
            nextShrinkTime = 0f;
            damagePhase = 0;
            active = false;
            shrinking = false;
            UpdateVisual();
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            if (!shrinking && Time.time >= nextShrinkTime && currentRadius > finalRadius + 0.1f)
            {
                StartCoroutine(ShrinkRoutine());
            }

            ApplyZoneDamage();
            RaiseChanged();
        }

        public void Initialize(Transform playerTarget, Health health)
        {
            player = playerTarget;
            playerHealth = health;
        }

        public void StartZone()
        {
            currentRadius = initialRadius;
            nextRadius = CalculateNextRadius(currentRadius);
            nextShrinkTime = Time.time + GetHoldDuration(0);
            active = true;
            shrinking = false;
            damagePhase = 0;
            UpdateVisual();
            RaiseChanged();
        }

        public void StopZone()
        {
            active = false;
            shrinking = false;
        }

        public bool IsOutsideZone(Vector3 worldPosition)
        {
            Vector3 offset = worldPosition - transform.position;
            offset.y = 0f;
            return offset.magnitude > currentRadius;
        }

        public Vector3 GetNearestSafePoint(Vector3 worldPosition)
        {
            Vector3 offset = worldPosition - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 1f)
            {
                return transform.position + Vector3.forward * Mathf.Max(2f, currentRadius * 0.45f);
            }

            return transform.position + offset.normalized * Mathf.Max(2f, currentRadius * 0.72f);
        }

        private IEnumerator ShrinkRoutine()
        {
            shrinking = true;
            float from = currentRadius;
            float to = Mathf.Max(finalRadius, nextRadius > 0f ? nextRadius : currentRadius * radiusMultiplierPerShrink);
            float elapsed = 0f;
            float duration = GetShrinkDuration(damagePhase);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentRadius = Mathf.Lerp(from, to, elapsed / duration);
                UpdateVisual();
                yield return null;
            }

            currentRadius = to;
            damagePhase = Mathf.Min(damagePhase + 1, damageByPhase != null && damageByPhase.Length > 0 ? damageByPhase.Length - 1 : damagePhase + 1);
            nextRadius = CalculateNextRadius(currentRadius);
            nextShrinkTime = Time.time + GetHoldDuration(damagePhase);
            shrinking = false;
            UpdateVisual();
        }

        private void ApplyZoneDamage()
        {
            if (player == null || playerHealth == null || !playerHealth.IsAlive || !IsOutsideZone(player.position))
            {
                return;
            }

            float phaseDamage = outsideDamagePerSecond;
            if (damageByPhase != null && damageByPhase.Length > 0)
            {
                phaseDamage = damageByPhase[Mathf.Clamp(damagePhase, 0, damageByPhase.Length - 1)];
            }

            playerHealth.TakeDamage(phaseDamage * Time.deltaTime, player.position, Vector3.up, gameObject);
            if (Time.time - lastZoneWarningTime > 2.4f)
            {
                lastZoneWarningTime = Time.time;
                RuntimeAudioBank.Instance?.PlayZoneWarning(player.position);
            }
        }

        private void UpdateVisual()
        {
            if (zoneVisual == null)
            {
                return;
            }

            zoneVisual.position = transform.position + Vector3.up * 0.04f;
            zoneVisual.localScale = new Vector3(currentRadius * 2f, 0.05f, currentRadius * 2f);

            if (nextZoneVisual != null)
            {
                nextZoneVisual.position = transform.position + Vector3.up * 0.08f;
                nextZoneVisual.localScale = new Vector3(nextRadius * 2f, 0.045f, nextRadius * 2f);
            }
        }

        private void RaiseChanged()
        {
            bool outside = player != null && IsOutsideZone(player.position);
            onZoneChanged.Invoke(currentRadius, SecondsUntilShrink, outside);
        }

        private float CalculateNextRadius(float radius)
        {
            if (radius <= finalRadius + 0.1f)
            {
                return finalRadius;
            }

            return Mathf.Max(finalRadius, radius * radiusMultiplierPerShrink);
        }

        private float GetHoldDuration(int phase)
        {
            if (holdSecondsByPhase != null && holdSecondsByPhase.Length > 0)
            {
                return Mathf.Max(10f, holdSecondsByPhase[Mathf.Clamp(phase, 0, holdSecondsByPhase.Length - 1)]);
            }

            return shrinkInterval;
        }

        private float GetShrinkDuration(int phase)
        {
            if (shrinkSecondsByPhase != null && shrinkSecondsByPhase.Length > 0)
            {
                return Mathf.Max(4f, shrinkSecondsByPhase[Mathf.Clamp(phase, 0, shrinkSecondsByPhase.Length - 1)]);
            }

            return shrinkDuration;
        }
    }
}
