using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class RuntimeDeveloperPanel : MonoBehaviour
    {
        [SerializeField] private Text output;
        [SerializeField] private Transform player;
        [SerializeField] private ThirdPersonMobileController movementController;
        [SerializeField] private BotManager botManager;
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Health playerHealth;

        private float frameTimer;
        private int frameCount;
        private float fps;

        public void Configure(Text outputText, Transform playerTransform, BotManager bots, SafeZoneController zone)
        {
            output = outputText;
            SetTargets(playerTransform, bots, zone);
        }

        public void Configure(Text outputText, Transform playerTransform, BotManager bots, SafeZoneController zone, ThirdPersonMobileController controller)
        {
            output = outputText;
            SetTargets(playerTransform, bots, zone, controller);
        }

        public void Configure(Text outputText, Transform playerTransform, BotManager bots, SafeZoneController zone, ThirdPersonMobileController controller, GameManager manager, Health health)
        {
            output = outputText;
            SetTargets(playerTransform, bots, zone, controller, manager, health);
        }

        public void SetTargets(Transform playerTransform, BotManager bots, SafeZoneController zone)
        {
            SetTargets(playerTransform, bots, zone, movementController);
        }

        public void SetTargets(Transform playerTransform, BotManager bots, SafeZoneController zone, ThirdPersonMobileController controller)
        {
            player = playerTransform;
            botManager = bots;
            safeZone = zone;
            movementController = controller;
        }

        public void SetTargets(Transform playerTransform, BotManager bots, SafeZoneController zone, ThirdPersonMobileController controller, GameManager manager, Health health)
        {
            SetTargets(playerTransform, bots, zone, controller);
            gameManager = manager;
            playerHealth = health;
        }

        private void Awake()
        {
#if !UNITY_EDITOR
            gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            frameTimer += Time.unscaledDeltaTime;
            frameCount++;
            if (frameTimer >= 0.35f)
            {
                fps = frameCount / Mathf.Max(0.001f, frameTimer);
                frameTimer = 0f;
                frameCount = 0;
            }

            if (output == null)
            {
                return;
            }

            Vector3 position = player != null ? player.position : Vector3.zero;
            int activeBots = botManager != null ? botManager.ActiveCount : 0;
            int aliveBots = botManager != null ? botManager.AliveCount : 0;
            int stuckBots = botManager != null ? botManager.StuckCount : 0;
            string zonePhase = safeZone != null ? safeZone.CurrentPhase : "Inactive";
            if (movementController == null)
            {
                movementController = Object.FindAnyObjectByType<ThirdPersonMobileController>(FindObjectsInactive.Include);
            }

            if (gameManager == null)
            {
                gameManager = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            }

            if (playerHealth == null && player != null)
            {
                playerHealth = player.GetComponentInChildren<Health>();
            }

            string matchState = gameManager != null ? gameManager.DebugMatchState : "NoGM";
            bool localAlive = gameManager != null ? gameManager.DebugLocalPlayerAlive : playerHealth != null && playerHealth.IsAlive;
            int aliveCount = gameManager != null ? gameManager.DebugAliveCount : aliveBots + (localAlive ? 1 : 0);
            bool controlsEnabled = gameManager != null ? gameManager.DebugControlsEnabled : movementController != null && movementController.DebugControlsEnabled;
            string matchLine = $"MATCH | STATE {matchState} | LOCAL {localAlive} | ALIVE {aliveCount} | CTRL {controlsEnabled} | TS {Time.timeScale:0.00}";

            if (movementController != null)
            {
                movementController.RefreshMovementDebugSnapshot();
                output.text =
                    $"DEV | FPS {fps:0} | POS {position.x:0},{position.y:0},{position.z:0} | BOTS {aliveBots}/{activeBots} | STUCK {stuckBots} | ZONE {zonePhase}\n" +
                    $"MOVE | KEY {FormatVector(movementController.DebugKeyboardInput)} | JOY {FormatVector(movementController.DebugJoystickInput)} | FINAL {FormatVector(movementController.DebugFinalMoveInput)} | SPEED {movementController.DebugMoveSpeed:0.0} | GND {movementController.DebugGrounded} | CTRL {movementController.DebugControlsEnabled} | TS {Time.timeScale:0.00}\n" +
                    matchLine;
            }
            else
            {
                output.text = $"DEV | FPS {fps:0} | POS {position.x:0},{position.y:0},{position.z:0} | BOTS {aliveBots}/{activeBots} | STUCK {stuckBots} | ZONE {zonePhase}\nMOVE | controller missing\n{matchLine}";
            }
#endif
        }

        private static string FormatVector(Vector2 value)
        {
            return $"{value.x:0.00},{value.y:0.00}";
        }
    }
}
