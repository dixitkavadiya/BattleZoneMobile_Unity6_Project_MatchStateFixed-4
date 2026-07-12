using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject matchSummaryPanel;

        [Header("HUD Text")]
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text armorText;
        [SerializeField] private Image armorFill;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text medkitText;
        [SerializeField] private Text pickupMessageText;
        [SerializeField] private Text botsAliveText;
        [SerializeField] private Text zoneText;
        [SerializeField] private Text matchTimerText;
        [SerializeField] private Text killFeedText;
        [SerializeField] private Text matchAnnouncementText;
        [SerializeField] private Text matchPhaseText;
        [SerializeField] private Text flightPathText;
        [SerializeField] private Text matchSummaryText;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Text inventoryDetailsText;
        [SerializeField] private RectTransform crosshairRect;
        [SerializeField] private RectTransform minimapFlightPathRect;
        [SerializeField] private RuntimeHudTelemetry hudTelemetry;

        [Header("Feedback")]
        [SerializeField] private Image damageFlash;
        [SerializeField] private float flashFadeSpeed = 4f;
        [SerializeField] private float crosshairBaseSize = 80f;
        [SerializeField] private float crosshairBloomSize = 132f;
        [SerializeField] private float crosshairReturnSpeed = 7f;
        [SerializeField] private Color normalAmmoColor = new Color(0.9f, 0.97f, 1f, 1f);
        [SerializeField] private Color lowAmmoColor = new Color(1f, 0.72f, 0.26f, 1f);
        [SerializeField] private Color emptyAmmoColor = new Color(1f, 0.28f, 0.22f, 1f);
        [SerializeField] private Color safeZoneColor = new Color(0.75f, 1f, 0.82f, 1f);
        [SerializeField] private Color dangerZoneColor = new Color(1f, 0.42f, 0.35f, 1f);

        private Coroutine flashRoutine;
        private Coroutine pickupMessageRoutine;
        private Coroutine killFeedRoutine;
        private Coroutine matchAnnouncementRoutine;
        private float crosshairCurrentSize;
        private readonly Vector2 minimapWorldHalfExtents = new Vector2(260f, 260f);
        private int latestAliveCount = 1;
        private int latestKillCount;

        public void ConfigureForRuntime(
            GameObject mainMenu,
            GameObject hud,
            GameObject gameOver,
            GameObject victory,
            Text health,
            Image healthBar,
            Text armor,
            Image armorBar,
            Text ammo,
            Text weapon,
            Text medkit,
            Text pickupMessage,
            Text botsAlive,
            Text zone,
            Image damage,
            Text matchTimer = null,
            Text killFeed = null,
            Text matchAnnouncement = null,
            GameObject inventory = null,
            Text inventoryDetails = null,
            RectTransform crosshair = null,
            RuntimeHudTelemetry telemetry = null,
            GameObject summaryPanel = null,
            Text summaryText = null,
            Text phaseText = null,
            Text routeText = null,
            RectTransform flightPathRect = null)
        {
            mainMenuPanel = mainMenu;
            hudPanel = hud;
            gameOverPanel = gameOver;
            victoryPanel = victory;
            healthText = health;
            healthFill = healthBar;
            armorText = armor;
            armorFill = armorBar;
            ammoText = ammo;
            weaponText = weapon;
            medkitText = medkit;
            pickupMessageText = pickupMessage;
            botsAliveText = botsAlive;
            zoneText = zone;
            damageFlash = damage;
            matchTimerText = matchTimer;
            killFeedText = killFeed;
            matchAnnouncementText = matchAnnouncement;
            matchPhaseText = phaseText;
            flightPathText = routeText;
            matchSummaryPanel = summaryPanel;
            matchSummaryText = summaryText;
            inventoryPanel = inventory;
            inventoryDetailsText = inventoryDetails;
            crosshairRect = crosshair;
            minimapFlightPathRect = flightPathRect;
            hudTelemetry = telemetry;
            crosshairCurrentSize = crosshairBaseSize;
            SetFlightPathPreview(Vector3.zero, Vector3.forward, false);
        }

        private void Update()
        {
            if (crosshairRect == null)
            {
                return;
            }

            crosshairCurrentSize = Mathf.Lerp(crosshairCurrentSize, crosshairBaseSize, crosshairReturnSpeed * Time.unscaledDeltaTime);
            crosshairRect.sizeDelta = new Vector2(crosshairCurrentSize, crosshairCurrentSize);
        }

        public void ShowMainMenu()
        {
            SetPanel(mainMenuPanel, true);
            SetPanel(hudPanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(victoryPanel, false);
            SetPanel(inventoryPanel, false);
            SetPanel(matchSummaryPanel, false);
            SetMatchPhase("MENU", "Ready");
            SetFlightPathPreview(Vector3.zero, Vector3.forward, false);
        }

        public void ShowHUD()
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(hudPanel, true);
            SetPanel(gameOverPanel, false);
            SetPanel(victoryPanel, false);
            SetPanel(inventoryPanel, false);
            SetPanel(matchSummaryPanel, false);
        }

        public void ShowGameOver()
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(hudPanel, false);
            SetPanel(gameOverPanel, true);
            SetPanel(victoryPanel, false);
            SetPanel(inventoryPanel, false);
            SetPanel(matchSummaryPanel, false);
        }

        public void ShowVictory()
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(hudPanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(victoryPanel, true);
            SetPanel(inventoryPanel, false);
            SetPanel(matchSummaryPanel, false);
        }

        public void SetHealth(float current, float max)
        {
            if (healthText != null)
            {
                healthText.text = $"HP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }

            if (healthFill != null)
            {
                healthFill.fillAmount = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            }
        }

        public void SetArmor(float current, float max)
        {
            if (armorText != null)
            {
                armorText.text = $"AR {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }

            if (armorFill != null)
            {
                armorFill.fillAmount = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            }
        }

        public void SetAmmo(string weapon, int magazine, int reserve)
        {
            if (weaponText != null)
            {
                weaponText.text = weapon;
            }

            if (ammoText != null)
            {
                ammoText.text = $"{magazine}/{reserve}";
                ammoText.color = magazine <= 0 ? emptyAmmoColor : magazine <= 5 ? lowAmmoColor : normalAmmoColor;
            }
        }

        public void SetWeapon(string weapon)
        {
            if (weaponText != null)
            {
                weaponText.text = weapon;
            }
        }

        public void SetInventory(int medkits, string latestPickup)
        {
            if (medkitText != null)
            {
                medkitText.text = $"Medkits {medkits}";
            }

            if (!string.IsNullOrEmpty(latestPickup))
            {
                ShowPickupMessage(latestPickup);
            }
        }

        public void SetBotsAlive(int alive)
        {
            latestAliveCount = Mathf.Max(1, Mathf.Max(0, alive) + 1);
            UpdateAliveCounter();
        }

        public void SetKillCount(int kills)
        {
            latestKillCount = Mathf.Max(0, kills);
            UpdateAliveCounter();
        }

        public void SetZone(float radius, float secondsUntilShrink, bool outside, float nextRadius = -1f, int phase = 1)
        {
            if (zoneText == null)
            {
                return;
            }

            string status = outside ? "Outside zone" : "Safe";
            string next = nextRadius > 0f ? $" | Next {Mathf.CeilToInt(nextRadius)}m" : string.Empty;
            zoneText.text = $"{status} | P{Mathf.Max(1, phase)} | Zone {Mathf.CeilToInt(radius)}m{next} | Shrink {Mathf.CeilToInt(secondsUntilShrink)}s";
            zoneText.color = outside ? dangerZoneColor : safeZoneColor;
            hudTelemetry?.SetZone(radius, outside, nextRadius);
        }

        public void SetMatchTimer(float seconds)
        {
            if (matchTimerText == null)
            {
                return;
            }

            int wholeSeconds = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            int minutes = wholeSeconds / 60;
            int remainingSeconds = wholeSeconds % 60;
            matchTimerText.text = $"{minutes:00}:{remainingSeconds:00}";
        }

        public void SetMatchPhase(string phase, string detail, float progress01 = -1f)
        {
            string safePhase = string.IsNullOrEmpty(phase) ? "MATCH" : phase.ToUpperInvariant();
            string safeDetail = string.IsNullOrEmpty(detail) ? string.Empty : detail;
            string progress = progress01 >= 0f ? $" | {Mathf.RoundToInt(Mathf.Clamp01(progress01) * 100f)}%" : string.Empty;

            if (matchPhaseText != null)
            {
                matchPhaseText.text = $"{safePhase}{progress}";
                matchPhaseText.enabled = true;
            }

            if (flightPathText != null)
            {
                flightPathText.text = safeDetail;
                flightPathText.enabled = !string.IsNullOrEmpty(safeDetail);
            }
        }

        public void SetFlightPathPreview(Vector3 start, Vector3 end, bool visible)
        {
            if (minimapFlightPathRect == null)
            {
                return;
            }

            minimapFlightPathRect.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            Vector2 startPoint = WorldToMinimap(start);
            Vector2 endPoint = WorldToMinimap(end);
            Vector2 delta = endPoint - startPoint;
            minimapFlightPathRect.anchoredPosition = (startPoint + endPoint) * 0.5f;
            minimapFlightPathRect.sizeDelta = new Vector2(Mathf.Max(8f, delta.magnitude), 4f);
            minimapFlightPathRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        public void SetInventoryDetails(string details, int used, int capacity)
        {
            if (inventoryDetailsText != null)
            {
                string capacityLine = $"Backpack {used}/{capacity}";
                string cleanDetails = string.IsNullOrEmpty(details) ? string.Empty : details.Replace(capacityLine + "\n", string.Empty);
                inventoryDetailsText.text = $"BACKPACK {used}/{capacity}\n{cleanDetails}";
            }

            if (medkitText != null)
            {
                medkitText.text = $"Pack {used}/{capacity}";
            }
        }

        public void ToggleInventory()
        {
            if (inventoryPanel == null)
            {
                return;
            }

            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }

        public void HideInventory()
        {
            SetPanel(inventoryPanel, false);
        }

        public void HideMatchSummary()
        {
            SetPanel(matchSummaryPanel, false);
        }

        public void ShowMatchSummary(
            string result,
            float matchSeconds,
            int botsRemaining,
            int kills = 0,
            float damage = 0f,
            int shots = 0,
            int hits = 0,
            int placement = 0,
            int totalPlayers = 0,
            string localHistory = "")
        {
            if (matchSummaryPanel == null)
            {
                return;
            }

            int wholeSeconds = Mathf.FloorToInt(Mathf.Max(0f, matchSeconds));
            int minutes = wholeSeconds / 60;
            int seconds = wholeSeconds % 60;

            if (matchSummaryText != null)
            {
                float accuracy = shots <= 0 ? 0f : Mathf.Clamp01((float)hits / shots) * 100f;
                string stats = totalPlayers > 0
                    ? $"Placement: #{Mathf.Max(1, placement)}/{Mathf.Max(1, totalPlayers)}\n" +
                      $"Kills: {Mathf.Max(0, kills)}\n" +
                      $"Damage: {Mathf.RoundToInt(Mathf.Max(0f, damage))}\n" +
                      $"Accuracy: {accuracy:0}% ({hits}/{shots})\n"
                    : string.Empty;
                string history = string.IsNullOrEmpty(localHistory) ? string.Empty : $"{localHistory}\n";
                matchSummaryText.text =
                    $"{result}\n" +
                    $"Time: {minutes:00}:{seconds:00}\n" +
                    stats +
                    $"Bots Remaining: {Mathf.Max(0, botsRemaining)}\n" +
                    history +
                    "Map: Milestone 19 Drop Terrain";
            }

            SetPanel(matchSummaryPanel, true);
        }

        public void AddKillFeed(string message)
        {
            if (killFeedText == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            if (killFeedRoutine != null)
            {
                StopCoroutine(killFeedRoutine);
            }

            killFeedRoutine = StartCoroutine(KillFeedRoutine(message));
        }

        public void SetMatchAnnouncement(string message)
        {
            if (matchAnnouncementText == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            if (matchAnnouncementRoutine != null)
            {
                StopCoroutine(matchAnnouncementRoutine);
            }

            matchAnnouncementRoutine = StartCoroutine(MatchAnnouncementRoutine(message));
        }

        public void PulseCrosshair()
        {
            crosshairCurrentSize = crosshairBloomSize;
        }

        public void FlashDamage()
        {
            if (damageFlash == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void ShowPickupMessage(string message)
        {
            if (pickupMessageText == null)
            {
                return;
            }

            if (pickupMessageRoutine != null)
            {
                StopCoroutine(pickupMessageRoutine);
            }

            pickupMessageRoutine = StartCoroutine(PickupMessageRoutine(message));
        }

        private IEnumerator PickupMessageRoutine(string message)
        {
            pickupMessageText.text = message;
            pickupMessageText.enabled = true;
            yield return new WaitForSeconds(1.6f);
            pickupMessageText.enabled = false;
        }

        private IEnumerator KillFeedRoutine(string message)
        {
            killFeedText.text = message;
            killFeedText.enabled = true;
            yield return new WaitForSeconds(4f);
            killFeedText.enabled = false;
            killFeedRoutine = null;
        }

        private IEnumerator MatchAnnouncementRoutine(string message)
        {
            matchAnnouncementText.text = message;
            matchAnnouncementText.enabled = true;
            yield return new WaitForSeconds(1.8f);
            matchAnnouncementText.enabled = false;
            matchAnnouncementRoutine = null;
        }

        private IEnumerator FlashRoutine()
        {
            Color color = damageFlash.color;
            color.a = 0.42f;
            damageFlash.color = color;

            while (damageFlash.color.a > 0.01f)
            {
                color = damageFlash.color;
                color.a = Mathf.MoveTowards(color.a, 0f, flashFadeSpeed * Time.deltaTime);
                damageFlash.color = color;
                yield return null;
            }

            color.a = 0f;
            damageFlash.color = color;
            flashRoutine = null;
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private void UpdateAliveCounter()
        {
            if (botsAliveText != null)
            {
                botsAliveText.text = $"Alive {latestAliveCount} | K {latestKillCount}";
            }
        }

        private Vector2 WorldToMinimap(Vector3 world)
        {
            const float minimapRadius = 88f;
            float x = Mathf.Clamp(world.x / Mathf.Max(1f, minimapWorldHalfExtents.x), -1f, 1f) * minimapRadius;
            float y = Mathf.Clamp(world.z / Mathf.Max(1f, minimapWorldHalfExtents.y), -1f, 1f) * minimapRadius;
            return new Vector2(x, y);
        }
    }
}
