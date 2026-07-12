using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class RuntimeHudTelemetry : MonoBehaviour
    {
        private static readonly string[] CardinalLabels = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        [SerializeField] private Transform player;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Text compassText;
        [SerializeField] private RectTransform minimapPlayerArrow;
        [SerializeField] private RectTransform minimapZoneRing;
        [SerializeField] private RectTransform minimapNextZoneRing;
        [SerializeField] private Text minimapLabel;
        [SerializeField] private Text locationText;

        private float latestZoneRadius = 95f;
        private float latestNextZoneRadius = 72f;
        private bool outsideZone;
        private string[] locationNames;
        private Vector3[] locationPositions;
        private float displayedYaw;
        private bool yawInitialized;

        public void Configure(Transform playerTransform, Camera camera, Text compass, RectTransform arrow, RectTransform zoneRing, Text mapLabel, RectTransform nextZoneRing = null)
        {
            player = playerTransform;
            playerCamera = camera;
            compassText = compass;
            minimapPlayerArrow = arrow;
            minimapZoneRing = zoneRing;
            minimapLabel = mapLabel;
            minimapNextZoneRing = nextZoneRing;
        }

        public void ConfigureNamedLocations(string[] names, Vector3[] positions, Text currentLocationText)
        {
            locationNames = names;
            locationPositions = positions;
            locationText = currentLocationText;
        }

        public void SetZone(float radius, bool outside)
        {
            SetZone(radius, outside, latestNextZoneRadius);
        }

        public void SetZone(float radius, bool outside, float nextRadius)
        {
            latestZoneRadius = Mathf.Max(1f, radius);
            latestNextZoneRadius = nextRadius > 0f ? Mathf.Max(1f, nextRadius) : latestZoneRadius;
            outsideZone = outside;
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            float targetYaw = playerCamera != null ? playerCamera.transform.eulerAngles.y : player.eulerAngles.y;
            if (!yawInitialized)
            {
                displayedYaw = targetYaw;
                yawInitialized = true;
            }
            else
            {
                displayedYaw = Mathf.LerpAngle(displayedYaw, targetYaw, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            }

            if (compassText != null)
            {
                compassText.text = $"{CardinalFromYaw(displayedYaw)} {Mathf.RoundToInt(Mathf.Repeat(displayedYaw, 360f)):000}";
            }

            if (minimapPlayerArrow != null)
            {
                minimapPlayerArrow.localRotation = Quaternion.Euler(0f, 0f, -displayedYaw);
            }

            if (minimapZoneRing != null)
            {
                float size = Mathf.Clamp(latestZoneRadius * 1.8f, 34f, 156f);
                minimapZoneRing.sizeDelta = new Vector2(size, size);
            }

            if (minimapNextZoneRing != null)
            {
                float nextSize = Mathf.Clamp(latestNextZoneRadius * 1.8f, 24f, 150f);
                minimapNextZoneRing.sizeDelta = new Vector2(nextSize, nextSize);
            }

            if (minimapLabel != null)
            {
                minimapLabel.text = outsideZone ? "ZONE" : $"NEXT {Mathf.CeilToInt(latestNextZoneRadius)}";
            }

            if (locationText != null)
            {
                locationText.text = FindNearestLocation();
            }
        }

        private static string CardinalFromYaw(float yaw)
        {
            int index = Mathf.RoundToInt(Mathf.Repeat(yaw, 360f) / 45f) % CardinalLabels.Length;
            return CardinalLabels[index];
        }

        private string FindNearestLocation()
        {
            if (locationNames == null || locationPositions == null || locationNames.Length == 0)
            {
                return "Open Range";
            }

            int count = Mathf.Min(locationNames.Length, locationPositions.Length);
            int nearest = -1;
            float bestDistance = float.MaxValue;
            Vector3 playerPosition = player.position;

            for (int i = 0; i < count; i++)
            {
                float distance = Vector3.SqrMagnitude(playerPosition - locationPositions[i]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = i;
                }
            }

            if (nearest < 0 || bestDistance > 42f * 42f)
            {
                return "Open Range";
            }

            return locationNames[nearest];
        }
    }
}
