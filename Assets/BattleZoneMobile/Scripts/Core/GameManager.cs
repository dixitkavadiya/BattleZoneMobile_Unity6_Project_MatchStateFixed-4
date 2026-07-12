using System.Collections;
using UnityEngine;

namespace BattleZoneMobile
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player")]
        [SerializeField] private ThirdPersonMobileController playerController;
        [SerializeField] private Health playerHealth;
        [SerializeField] private WeaponController playerWeapons;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private VehicleInteractor vehicleInteractor;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Systems")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private BotManager botManager;
        [SerializeField] private LootSpawner lootSpawner;
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField] private BattleRoyaleMatchFlow matchFlow;

        private bool matchActive;
        private bool combatLocked;
        private bool matchConcluded;
        private float matchElapsedSeconds;
        private Coroutine startMatchRoutine;
        private int playerKills;
        private int shotsFired;
        private int confirmedHits;
        private int totalParticipants = 1;
        private float playerDamageDealt;
        private bool localPlayerRegistered;
        private bool localPlayerAlive;
        private bool openingSequenceComplete;
        private bool validOpponentRoster;
        private int expectedBotCount;
        private int lastKnownAliveBots;
        private int confirmedBotEliminations;
        private string debugMatchState = "Boot";

        public string DebugMatchState => debugMatchState;
        public bool DebugLocalPlayerAlive => IsLocalPlayerAlive();
        public int DebugAliveCount => GetAliveParticipantCount();
        public bool DebugControlsEnabled => playerController != null && playerController.DebugControlsEnabled;
        public bool IsMatchActive => matchActive;

        public void ConfigureForRuntime(
            ThirdPersonMobileController controller,
            Health health,
            WeaponController weapons,
            PlayerInventory inventory,
            VehicleInteractor vehicles,
            Transform spawnPoint,
            UIManager ui,
            BotManager bots,
            LootSpawner loot,
            SafeZoneController zone,
            BattleRoyaleMatchFlow flow = null)
        {
            playerController = controller;
            playerHealth = health;
            playerWeapons = weapons;
            playerInventory = inventory;
            vehicleInteractor = vehicles;
            playerSpawnPoint = spawnPoint;
            uiManager = ui;
            botManager = bots;
            lootSpawner = loot;
            safeZone = zone;
            matchFlow = flow;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = 60;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            WireEvents();
            ShowMainMenu();
        }

        private void Update()
        {
            if (!matchActive)
            {
                return;
            }

            matchElapsedSeconds += Time.deltaTime;
            uiManager?.SetMatchTimer(matchElapsedSeconds);
        }

        public void StartMatch()
        {
            if (startMatchRoutine != null)
            {
                StopCoroutine(startMatchRoutine);
            }

            startMatchRoutine = StartCoroutine(StartMatchRoutine());
        }

        private IEnumerator StartMatchRoutine()
        {
            debugMatchState = "Resetting";
            matchActive = true;
            combatLocked = true;
            matchConcluded = false;
            openingSequenceComplete = false;
            validOpponentRoster = false;
            expectedBotCount = 0;
            lastKnownAliveBots = 0;
            Time.timeScale = 1f;
            matchElapsedSeconds = 0f;
            ResetMatchStats();
            uiManager?.HideMatchSummary();
            uiManager?.SetMatchTimer(0f);
            uiManager?.ShowHUD();
            uiManager?.SetMatchAnnouncement("Preparing drop route");
            uiManager?.SetMatchPhase("Lobby", "Preparing drop route");
            uiManager?.SetKillCount(0);

            ResetPlayer();
            RegisterLocalPlayer();
            ForceEnablePlayerControls(false);
            playerWeapons?.SetControlsEnabled(false);

            safeZone?.PrepareForMatch();
            lootSpawner?.ClearLoot();
            lootSpawner?.SpawnLoot();

            Transform playerTransform = playerController != null ? playerController.transform : null;

            botManager?.Initialize(playerTransform, this);
            botManager?.SpawnBots();
            botManager?.SetSafeZone(safeZone);
            botManager?.SetCombatLocked(true);
            RefreshOpponentRoster();

            safeZone?.Initialize(playerTransform, playerHealth);
            safeZone?.StopZone();

            uiManager?.SetBotsAlive(lastKnownAliveBots);
            uiManager?.AddKillFeed("Waiting lobby armed");
            if (!validOpponentRoster)
            {
                uiManager?.AddKillFeed("Bot roster unavailable | match kept open");
            }
            debugMatchState = "Lobby";

            if (matchFlow != null)
            {
                yield return matchFlow.PlayOpeningSequence();
            }

            if (matchConcluded)
            {
                startMatchRoutine = null;
                yield break;
            }

            if (!IsLocalPlayerAlive())
            {
                ConcludeDefeat("Player eliminated before combat");
                startMatchRoutine = null;
                yield break;
            }

            openingSequenceComplete = true;
            combatLocked = false;
            ForceEnablePlayerControls(true);
            botManager?.SetCombatLocked(false);
            safeZone?.StartZone();
            matchActive = true;
            debugMatchState = "Combat";
            uiManager?.SetMatchPhase("Combat", "Winner is last standing");
            uiManager?.AddKillFeed("Match started");
            startMatchRoutine = null;
        }

        public void ShowMainMenu()
        {
            debugMatchState = "Menu";
            matchActive = false;
            combatLocked = true;
            matchConcluded = false;
            openingSequenceComplete = false;
            if (startMatchRoutine != null)
            {
                StopCoroutine(startMatchRoutine);
                startMatchRoutine = null;
            }

            if (playerController != null)
            {
                playerController.ControlsEnabled = false;
            }

            playerWeapons?.SetControlsEnabled(false);
            botManager?.SetCombatLocked(true);
            safeZone?.StopZone();
            uiManager?.ShowMainMenu();
        }

        public void RestartMatch()
        {
            uiManager?.HideMatchSummary();
            StartMatch();
        }

        public void CloseMatchSummary()
        {
            uiManager?.HideMatchSummary();
            if (matchConcluded)
            {
                ShowMainMenu();
            }
        }

        public void HandleJumpButton()
        {
            if (matchFlow != null && matchFlow.ConsumesJumpButton)
            {
                matchFlow.RequestJump();
                return;
            }

            playerController?.Jump();
        }

        public void NotifyBotKilled(BotAI bot)
        {
            NotifyBotKilled(bot, null);
        }

        public void NotifyBotKilled(BotAI bot, GameObject source)
        {
            if (!matchActive || matchConcluded)
            {
                return;
            }

            if (source == null || playerController == null || source.transform.root == playerController.transform)
            {
                playerKills++;
                uiManager?.SetKillCount(playerKills);
            }

            confirmedBotEliminations = Mathf.Min(Mathf.Max(0, expectedBotCount), confirmedBotEliminations + 1);
            int alive = GetAliveBotCount();
            lastKnownAliveBots = alive;
            uiManager?.SetBotsAlive(alive);
            uiManager?.AddKillFeed($"Enemy bot eliminated | {alive + 1} alive");

            if (alive <= 0 && CanConcludeVictory())
            {
                WinMatch();
            }
        }

        private void WireEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged.AddListener(OnPlayerHealthChanged);
                playerHealth.onArmorChanged.AddListener(OnPlayerArmorChanged);
                playerHealth.onDamageTaken.AddListener(OnPlayerDamaged);
                playerHealth.onDied.AddListener(OnPlayerDied);
            }

            if (playerWeapons != null)
            {
                playerWeapons.onAmmoChanged.AddListener(OnAmmoChanged);
                playerWeapons.onWeaponChanged.AddListener(OnWeaponChanged);
                playerWeapons.onFired.AddListener(OnPlayerFired);
                playerWeapons.onHitConfirmed.AddListener(OnPlayerHitConfirmed);
            }

            if (playerInventory != null)
            {
                playerInventory.onInventoryChanged.AddListener(OnInventoryChanged);
                playerInventory.onBackpackChanged.AddListener(OnBackpackChanged);
            }

            if (safeZone != null)
            {
                safeZone.onZoneChanged.AddListener(OnZoneChanged);
            }
        }

        private void ResetPlayer()
        {
            Time.timeScale = 1f;
            vehicleInteractor?.ForceExitVehicle();

            if (playerController != null)
            {
                playerController.enabled = true;
                ReliablePlayerMovement reliableMovement = playerController.GetComponent<ReliablePlayerMovement>();
                if (reliableMovement != null && !reliableMovement.enabled)
                {
                    reliableMovement.enabled = true;
                }

                playerController.SetExternalGroundMovementDriver(reliableMovement != null && reliableMovement.OwnsGroundMovement);
                CharacterController characterController = playerController.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = true;
                }

                if (playerSpawnPoint != null)
                {
                    playerController.ResetController(playerSpawnPoint.position, playerSpawnPoint.rotation);
                }
                else
                {
                    playerController.SetExternalMotionLock(false);
                    playerController.ControlsEnabled = true;
                }
            }

            playerHealth?.ResetHealth();
            playerWeapons?.ResetWeapons();
            playerInventory?.ResetInventory();
            RegisterLocalPlayer();
        }

        private void OnPlayerHealthChanged(float current, float max)
        {
            uiManager?.SetHealth(current, max);
        }

        private void OnPlayerArmorChanged(float current, float max)
        {
            uiManager?.SetArmor(current, max);
        }

        private void OnPlayerDamaged(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            if (IsSafeZoneDamageSource(source))
            {
                uiManager?.SetZoneWarningOverlay(true);
            }
            else
            {
                uiManager?.FlashDamage();
            }

            RuntimeAudioBank.Instance?.PlayHit(hitPoint);
        }

        private static bool IsSafeZoneDamageSource(GameObject source)
        {
            return source != null && source.GetComponentInParent<SafeZoneController>() != null;
        }

        private void OnPlayerDied(GameObject source)
        {
            if (!matchActive || matchConcluded)
            {
                return;
            }

            if (playerHealth == null)
            {
                debugMatchState = openingSequenceComplete ? "Combat" : "Lobby";
                ForceEnablePlayerControls(!combatLocked);
                uiManager?.AddKillFeed("Ignored invalid defeat | player health missing");
                return;
            }

            if (playerHealth.IsAlive)
            {
                localPlayerAlive = true;
                debugMatchState = openingSequenceComplete ? "Combat" : "Lobby";
                ForceEnablePlayerControls(!combatLocked);
                uiManager?.AddKillFeed("Ignored invalid defeat | local player alive");
                return;
            }

            localPlayerAlive = false;
            ConcludeDefeat("Player eliminated");
        }

        private void ConcludeDefeat(string reason)
        {
            matchActive = false;
            combatLocked = true;
            matchConcluded = true;
            debugMatchState = "Defeat";
            if (playerController != null)
            {
                playerController.ControlsEnabled = false;
            }

            playerWeapons?.SetControlsEnabled(false);
            botManager?.SetCombatLocked(true);
            safeZone?.StopZone();
            uiManager?.SetMatchPhase("Defeat", reason);
            uiManager?.ShowGameOver();
            int botsRemaining = GetAliveBotCount();
            lastKnownAliveBots = botsRemaining;
            int placement = Mathf.Clamp(botsRemaining + 1, 1, Mathf.Max(1, totalParticipants));
            string history = BattleZoneLocalMatchHistory.SaveMatch("DEFEAT", playerKills, placement, totalParticipants, matchElapsedSeconds, playerDamageDealt, shotsFired, confirmedHits);
            uiManager?.ShowMatchSummary("DEFEAT", matchElapsedSeconds, botsRemaining, playerKills, playerDamageDealt, shotsFired, confirmedHits, placement, totalParticipants, history);
            uiManager?.AddKillFeed(reason);
        }

        private void OnAmmoChanged(string weapon, int magazine, int reserve)
        {
            uiManager?.SetAmmo(weapon, magazine, reserve);
        }

        private void OnWeaponChanged(string weapon)
        {
            uiManager?.SetWeapon(weapon);
        }

        private void OnInventoryChanged(int medkits, string latestPickup)
        {
            uiManager?.SetInventory(medkits, latestPickup);
        }

        private void OnBackpackChanged(string details, int used, int capacity)
        {
            uiManager?.SetInventoryDetails(details, used, capacity);
        }

        private void OnZoneChanged(float radius, float secondsUntilShrink, bool outside)
        {
            uiManager?.SetZone(radius, secondsUntilShrink, outside, safeZone != null ? safeZone.NextRadius : -1f, safeZone != null ? safeZone.DamagePhase : 1);
        }

        private void OnPlayerFired()
        {
            if (combatLocked)
            {
                return;
            }

            shotsFired++;
            if (playerWeapons != null)
            {
                botManager?.BroadcastGunshot(playerWeapons.transform.position);
            }
        }

        private void OnPlayerHitConfirmed(Vector3 hitPoint, float damage)
        {
            if (combatLocked)
            {
                return;
            }

            confirmedHits++;
            playerDamageDealt += Mathf.Max(0f, damage);
        }

        private void WinMatch()
        {
            if (!CanConcludeVictory())
            {
                return;
            }

            matchActive = false;
            combatLocked = true;
            matchConcluded = true;
            debugMatchState = "Victory";
            if (playerController != null)
            {
                playerController.ControlsEnabled = false;
            }

            playerWeapons?.SetControlsEnabled(false);
            botManager?.SetCombatLocked(true);
            safeZone?.StopZone();
            uiManager?.SetMatchPhase("Victory", "Last survivor");
            uiManager?.ShowVictory();
            string history = BattleZoneLocalMatchHistory.SaveMatch("VICTORY", playerKills, 1, totalParticipants, matchElapsedSeconds, playerDamageDealt, shotsFired, confirmedHits);
            uiManager?.ShowMatchSummary("VICTORY", matchElapsedSeconds, 0, playerKills, playerDamageDealt, shotsFired, confirmedHits, 1, totalParticipants, history);
            uiManager?.AddKillFeed("All enemy bots eliminated");
        }

        private void ResetMatchStats()
        {
            playerKills = 0;
            shotsFired = 0;
            confirmedHits = 0;
            playerDamageDealt = 0f;
            totalParticipants = 1;
            confirmedBotEliminations = 0;
        }

        private void RegisterLocalPlayer()
        {
            localPlayerRegistered = playerController != null && playerHealth != null;
            localPlayerAlive = localPlayerRegistered && playerHealth.IsAlive;
        }

        private bool IsLocalPlayerAlive()
        {
            if (!localPlayerRegistered || playerHealth == null)
            {
                return false;
            }

            localPlayerAlive = playerHealth.IsAlive;
            return localPlayerAlive;
        }

        private int GetAliveBotCount()
        {
            return botManager != null ? Mathf.Max(0, botManager.AliveCount) : 0;
        }

        private int GetAliveParticipantCount()
        {
            return (IsLocalPlayerAlive() ? 1 : 0) + GetAliveBotCount();
        }

        private void RefreshOpponentRoster()
        {
            expectedBotCount = botManager != null ? Mathf.Max(0, botManager.ActiveCount) : 0;
            lastKnownAliveBots = GetAliveBotCount();
            validOpponentRoster = expectedBotCount > 0 && lastKnownAliveBots > 0;
            int rosterBots = Mathf.Max(expectedBotCount, lastKnownAliveBots);
            totalParticipants = Mathf.Max(1, rosterBots + (IsLocalPlayerAlive() ? 1 : 0));
        }

        private bool CanConcludeVictory()
        {
            return matchActive &&
                !matchConcluded &&
                openingSequenceComplete &&
                IsLocalPlayerAlive() &&
                validOpponentRoster &&
                confirmedBotEliminations >= expectedBotCount &&
                GetAliveBotCount() <= 0;
        }

        private void ForceEnablePlayerControls(bool weaponsEnabled)
        {
            Time.timeScale = 1f;
            if (playerController != null)
            {
                playerController.enabled = true;
                ReliablePlayerMovement reliableMovement = playerController.GetComponent<ReliablePlayerMovement>();
                if (reliableMovement != null && !reliableMovement.enabled)
                {
                    reliableMovement.enabled = true;
                }

                playerController.SetExternalGroundMovementDriver(reliableMovement != null && reliableMovement.OwnsGroundMovement);
                CharacterController characterController = playerController.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = true;
                }

                playerController.SetExternalMotionLock(false);
                playerController.ControlsEnabled = true;
            }

            playerWeapons?.SetControlsEnabled(weaponsEnabled);
        }

        private bool HasReliableMovement()
        {
            return playerController != null && playerController.GetComponent<ReliablePlayerMovement>() != null;
        }
    }
}
