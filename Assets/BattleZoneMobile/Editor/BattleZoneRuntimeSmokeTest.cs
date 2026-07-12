#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using BattleZoneMobile;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BattleZoneMobile.Editor
{
    public static class BattleZoneRuntimeSmokeTest
    {
        private const string ScenePath = "Assets/BattleZoneMobile/Scenes/BZ_Main.unity";

        private static readonly List<string> Failures = new List<string>();

        public static void Run()
        {
            Failures.Clear();
            Application.logMessageReceived += OnLogMessageReceived;

            try
            {
                BattleZoneProjectConfigurator.ConfigureProject(false);
                EditorSceneManager.OpenScene(ScenePath);

                BattleZoneRuntimeBuilder builder = Object.FindAnyObjectByType<BattleZoneRuntimeBuilder>();
                if (builder == null)
                {
                    Failures.Add("BattleZoneRuntimeBuilder was not found in BZ_Main.");
                }
                else
                {
                    MethodInfo awake = typeof(BattleZoneRuntimeBuilder).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (awake == null)
                    {
                        Failures.Add("BattleZoneRuntimeBuilder.Awake could not be reflected.");
                    }
                    else
                    {
                        awake.Invoke(builder, null);
                    }
                }

                ValidateRuntimeObjects();
            }
            catch (Exception exception)
            {
                Failures.Add(exception.ToString());
            }
            finally
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }

            if (Failures.Count > 0)
            {
                foreach (string failure in Failures)
                {
                    Debug.LogError(failure);
                }

                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("BattleZone runtime smoke test passed.");
            EditorApplication.Exit(0);
        }

        private static void ValidateRuntimeObjects()
        {
            if (GameObject.Find("Mobile UI Canvas") == null)
            {
                Failures.Add("Mobile UI Canvas was not created.");
            }

            if (GameObject.Find("MainMenuPanel") == null)
            {
                Failures.Add("MainMenuPanel was not created.");
            }

            if (GameObject.Find("LowPolyOriginalHumanoid") == null)
            {
                Failures.Add("Milestone 3 humanoid placeholder visual was not created.");
            }

            if (FindGameObjectIncludingInactive("PausePanel") == null || FindGameObjectIncludingInactive("SettingsPanel") == null)
            {
                Failures.Add("Pause/settings UI panels were not created.");
            }

            if (Object.FindAnyObjectByType<HitMarkerUI>() == null)
            {
                Failures.Add("Hit marker UI was not created.");
            }

            if (Object.FindAnyObjectByType<RuntimeHudTelemetry>() == null)
            {
                Failures.Add("Milestone 3 minimap/compass telemetry was not created.");
            }

            if (Object.FindAnyObjectByType<RuntimeAudioBank>() == null)
            {
                Failures.Add("Milestone 3 runtime audio bank was not created.");
            }

            if (Object.FindAnyObjectByType<RuntimeAmbientSoundscape>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 9 ambient soundscape was not created.");
            }

            if (Object.FindAnyObjectByType<ReflectionProbe>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 9 reflection probes were not created.");
            }

            if (FindGameObjectIncludingInactive("InventoryPanel") == null ||
                FindGameObjectIncludingInactive("MiniMapPanel") == null ||
                FindGameObjectIncludingInactive("KillFeedText") == null ||
                FindGameObjectIncludingInactive("MatchTimerText") == null ||
                FindGameObjectIncludingInactive("MiniMapLocation") == null ||
                FindGameObjectIncludingInactive("AimSensitivitySlider") == null ||
                FindGameObjectIncludingInactive("ScopeSensitivitySlider") == null ||
                FindGameObjectIncludingInactive("GraphicsSlider") == null ||
                FindGameObjectIncludingInactive("AudioSlider") == null ||
                FindGameObjectIncludingInactive("ButtonScaleSlider") == null ||
                FindGameObjectIncludingInactive("ArmorFill") == null ||
                FindGameObjectIncludingInactive("ShoulderButton") == null ||
                FindGameObjectIncludingInactive("DeveloperTestPanel") == null ||
                FindGameObjectIncludingInactive("MiniMapRiverLine") == null ||
                FindGameObjectIncludingInactive("MatchSummaryPanel") == null ||
                FindGameObjectIncludingInactive("MatchSummaryText") == null)
            {
                Failures.Add("Milestone 3 HUD/settings widgets were not created.");
            }

            if (Object.FindAnyObjectByType<InventoryDragDropSlot>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 9 inventory drag-and-drop slots were not created.");
            }

            if (FindGameObjectIncludingInactive("ProneButton") == null ||
                FindGameObjectIncludingInactive("DriveButton") == null ||
                FindGameObjectIncludingInactive("ThrottleButton") == null ||
                FindGameObjectIncludingInactive("BrakeButton") == null ||
                FindGameObjectIncludingInactive("VehiclePrompt") == null ||
                FindGameObjectIncludingInactive("VehicleStatusText") == null ||
                FindGameObjectIncludingInactive("MatchAnnouncementText") == null)
            {
                Failures.Add("Milestone 4 mobile controls and HUD widgets were not created.");
            }

            string[] requiredButtons =
            {
                "StartMatchButton",
                "JumpButton",
                "CrouchButton",
                "ReloadButton",
                "MedkitButton",
                "SwitchWeaponButton",
                "ShoulderButton",
                "ProneButton",
                "DriveButton",
                "InventoryButton",
                "PauseButton",
                "FireButton",
                "SprintButton",
                "AimButton",
                "ThrottleButton",
                "BrakeButton"
            };

            foreach (string buttonName in requiredButtons)
            {
                GameObject buttonObject = FindGameObjectIncludingInactive(buttonName);
                if (buttonObject == null ||
                    (buttonObject.GetComponent<Selectable>() == null && buttonObject.GetComponent<MobileHoldButton>() == null))
                {
                    Failures.Add($"Required control '{buttonName}' was not created as a selectable or holdable UI control.");
                }
            }

            if (GameObject.Find("North Warehouse") == null ||
                GameObject.Find("Military Base Command") == null ||
                GameObject.Find("Village House 1") == null ||
                GameObject.Find("Cedar Town Location Marker") == null ||
                GameObject.Find("Sentinel Checkpoint Location Marker") == null ||
                GameObject.Find("Pine Forest Location Marker") == null)
            {
                Failures.Add("Milestone 3 world landmarks were not created.");
            }

            if (FindGameObjectIncludingInactive("Harbor Coast Location Marker") == null ||
                FindGameObjectIncludingInactive("Orion Industrial Location Marker") == null ||
                FindGameObjectIncludingInactive("Fort Array Location Marker") == null ||
                FindGameObjectIncludingInactive("Old Town Location Marker") == null ||
                FindGameObjectIncludingInactive("Westpine Forest Location Marker") == null ||
                FindGameObjectIncludingInactive("Milestone4 Jeep") == null ||
                FindGameObjectIncludingInactive("Milestone4 Buggy") == null ||
                FindGameObjectIncludingInactive("Milestone4 Motorcycle") == null ||
                FindGameObjectIncludingInactive("Milestone4 Original Drop Plane") == null ||
                FindGameObjectIncludingInactive("Milestone4 Parachute Visual") == null ||
                FindGameObjectIncludingInactive("Milestone4 Battle Royale Flow") == null)
            {
                Failures.Add("Milestone 4 world, vehicle, or battle royale flow objects were not created.");
            }

            if (Object.FindAnyObjectByType<VehicleController>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 4 vehicle controllers were not created.");
            }

            if (Object.FindAnyObjectByType<VehicleInteractor>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 4 vehicle interactor was not created on the player.");
            }

            if (Object.FindAnyObjectByType<PlayerEquipment>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 4 player equipment system was not created.");
            }

            if (Object.FindAnyObjectByType<BattleRoyaleMatchFlow>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 4 match flow was not created.");
            }

            if (Object.FindAnyObjectByType<WeaponModelRig>(FindObjectsInactive.Include) == null ||
                FindGameObjectIncludingInactive("Original Low Poly Pistol") == null ||
                FindGameObjectIncludingInactive("Original Low Poly Assault Rifle") == null ||
                FindGameObjectIncludingInactive("Original Low Poly SMG") == null ||
                FindGameObjectIncludingInactive("Original Low Poly Sniper") == null ||
                FindGameObjectIncludingInactive("Original Low Poly Shotgun") == null)
            {
                Failures.Add("Milestone 5 original weapon model rig or weapon models were not created.");
            }

            if (FindGameObjectIncludingInactive("North Fuel Stop Awning") == null ||
                FindGameObjectIncludingInactive("South Fuel Stop Awning") == null ||
                FindGameObjectIncludingInactive("Ridge Radio Mast Base") == null ||
                FindGameObjectIncludingInactive("Old Town Bus Stop Back Wall") == null ||
                FindGameObjectIncludingInactive("Harbor Road Sign") == null ||
                FindGameObjectIncludingInactive("Milestone5 Power Pole 1 Pole") == null ||
                FindGameObjectIncludingInactive("Milestone5 Hunter Blind 1 Cabin") == null)
            {
                Failures.Add("Milestone 5 world polish props were not created.");
            }

            if (FindGameObjectIncludingInactive("Silver River Segment 1") == null ||
                FindGameObjectIncludingInactive("East Bridge Deck") == null ||
                FindGameObjectIncludingInactive("North Mountain 1") == null ||
                FindGameObjectIncludingInactive("Kestrel Factory Main Hall") == null ||
                FindGameObjectIncludingInactive("Watchtower Ridge Tower 1") == null ||
                FindGameObjectIncludingInactive("Silver River Location Marker") == null ||
                FindGameObjectIncludingInactive("Kestrel Factory Location Marker") == null)
            {
                Failures.Add("Milestone 9 alpha-polish world landmarks were not created.");
            }

            if (FindGameObjectIncludingInactive("Aurora City Location Marker") == null ||
                FindGameObjectIncludingInactive("M10 City Block A1 Building Root") == null ||
                FindGameObjectIncludingInactive("M10 Main Intersection") == null ||
                FindGameObjectIncludingInactive("M10 Aurora Main Avenue Sidewalk A") == null ||
                FindGameObjectIncludingInactive("M10 Terrain Hill 1") == null ||
                FindGameObjectIncludingInactive("M10 Forest Cluster 1") == null ||
                FindGameObjectIncludingInactive("Milestone10 URP Global Post Processing") == null)
            {
                Failures.Add("Milestone 10 modular city, road, terrain, vegetation, or post-processing objects were not created.");
            }

            string[] milestone11Locations =
            {
                "Central City Location Marker",
                "Small Village Location Marker",
                "Industrial Factory Location Marker",
                "Military Base Location Marker",
                "Forest Location Marker",
                "Mountain Area Location Marker",
                "River Location Marker",
                "Bridge Location Marker",
                "Gas Station Location Marker",
                "Warehouse Area Location Marker",
                "Farm Location Marker",
                "Watch Towers Location Marker"
            };

            foreach (string location in milestone11Locations)
            {
                if (FindGameObjectIncludingInactive(location) == null)
                {
                    Failures.Add($"Milestone 11 named location '{location}' was not created.");
                }
            }

            if (FindGameObjectIncludingInactive("M11 Central City Apartment A Building Root") == null ||
                FindGameObjectIncludingInactive("M11 Central City Main Intersection") == null ||
                FindGameObjectIncludingInactive("M11 Village Connector Road") == null ||
                FindGameObjectIncludingInactive("M11 Mountain Lake") == null ||
                FindGameObjectIncludingInactive("M11 Cliff Face North") == null ||
                FindGameObjectIncludingInactive("M11 River Bridge Deck") == null ||
                FindGameObjectIncludingInactive("M11 Industrial Factory Main Hall Building Root") == null ||
                FindGameObjectIncludingInactive("M11 Military Barracks A Building Root") == null ||
                FindGameObjectIncludingInactive("M11 Gas Station Awning") == null ||
                FindGameObjectIncludingInactive("M11 Farm Crop Row 1") == null ||
                FindGameObjectIncludingInactive("M11 Forest Cluster 1") == null ||
                FindGameObjectIncludingInactive("M11 Watch Tower Alpha") == null ||
                FindGameObjectIncludingInactive("M11 Ammo Box Prop") == null ||
                FindGameObjectIncludingInactive("M11 Medical Kit Prop") == null ||
                FindGameObjectIncludingInactive("M11 Backpack Prop") == null ||
                FindGameObjectIncludingInactive("M11 Helmet Prop") == null ||
                FindGameObjectIncludingInactive("M11 Armor Prop") == null ||
                FindGameObjectIncludingInactive("M11 Weapon Crate Prop") == null ||
                FindGameObjectIncludingInactive("M11 Fuel Can Prop") == null ||
                FindGameObjectIncludingInactive("Milestone11 Pickup Truck") == null ||
                FindGameObjectIncludingInactive("M11 Cloud Bank Central") == null)
            {
                Failures.Add("Milestone 11 original world, terrain, loot, vehicle, or visual atmosphere objects were not created.");
            }

            if (FindGameObjectIncludingInactive("M12 Professional Visual Upgrade Marker") == null ||
                FindGameObjectIncludingInactive("M12 Grass Terrain Material Blend") == null ||
                FindGameObjectIncludingInactive("M12 Dirt Terrain Material Blend") == null ||
                FindGameObjectIncludingInactive("M12 Rock Terrain Material Blend") == null ||
                FindGameObjectIncludingInactive("M12 Sand Terrain Material Blend") == null ||
                FindGameObjectIncludingInactive("M12 River Foam Strip 1") == null ||
                FindGameObjectIncludingInactive("M12 Bridge Cable Rail North") == null ||
                FindGameObjectIncludingInactive("M12 Road Safety Cone 1") == null ||
                FindGameObjectIncludingInactive("M12 Tactical Status Panel") == null ||
                FindGameObjectIncludingInactive("M12 Tactical Weapon Panel") == null ||
                FindGameObjectIncludingInactive("M12CrosshairTop") == null ||
                FindGameObjectIncludingInactive("M12 Tactical Vest Left Pouch") == null ||
                FindGameObjectIncludingInactive("M12 Helmet Side Rail L") == null ||
                FindGameObjectIncludingInactive("M12 Backpack Bedroll") == null ||
                FindGameObjectIncludingInactive("Original Low Poly Assault Rifle Ejection Port") == null ||
                FindGameObjectIncludingInactive("Original Low Poly Sniper Long Scope Shade") == null)
            {
                Failures.Add("Milestone 12 professional visual, HUD, character, or weapon polish objects were not created.");
            }

            if (FindGameObjectIncludingInactive("Milestone13 Complete Battle Royale Flow") == null ||
                FindGameObjectIncludingInactive("Milestone13 Waiting Area Platform") == null ||
                FindGameObjectIncludingInactive("Milestone13 Waiting Area Spawn") == null ||
                FindGameObjectIncludingInactive("Milestone13 Flight Path Preview") == null ||
                FindGameObjectIncludingInactive("MiniMapNextZoneRing") == null ||
                FindGameObjectIncludingInactive("ZoneNextVisual") == null ||
                FindGameObjectIncludingInactive("PF_RuntimeLoot_Backpack") == null)
            {
                Failures.Add("Milestone 13 match flow, waiting area, next zone, flight path, or backpack loot objects were not created.");
            }

            if (FindGameObjectIncludingInactive("Milestone18 Complete Battle Royale Match Flow") == null ||
                FindGameObjectIncludingInactive("Milestone18 Randomized Flight Path") == null ||
                FindGameObjectIncludingInactive("Milestone18 Flight Start Marker") == null ||
                FindGameObjectIncludingInactive("Milestone18 Flight End Marker") == null ||
                FindGameObjectIncludingInactive("MiniMapFlightPath") == null ||
                FindGameObjectIncludingInactive("MatchPhaseText") == null ||
                FindGameObjectIncludingInactive("FlightPathText") == null)
            {
                Failures.Add("Milestone 18 complete BR match-flow director, phase HUD, or minimap flight-path widgets were not created.");
            }

            if (FindGameObjectIncludingInactive("Milestone19 Drop Experience Terrain Root") == null ||
                FindGameObjectIncludingInactive("M19 Large Drop Grassland") == null ||
                FindGameObjectIncludingInactive("M19 Village Road") == null ||
                FindGameObjectIncludingInactive("M19 Drop River Segment 1") == null ||
                FindGameObjectIncludingInactive("M19 Drop Bridge Deck") == null ||
                FindGameObjectIncludingInactive("M19 Drop Forest Cluster 1") == null ||
                FindGameObjectIncludingInactive("M19 Container Yard Stack 1") == null ||
                FindGameObjectIncludingInactive("M19 Military Camp Command Tent") == null)
            {
                Failures.Add("Milestone 19 drop terrain, river, bridge, village, forest, container yard, or military camp objects were not created.");
            }

            Terrain m20Terrain = Object.FindAnyObjectByType<Terrain>(FindObjectsInactive.Include);
            if (FindGameObjectIncludingInactive("Milestone20 Realistic Original World Root") == null ||
                FindGameObjectIncludingInactive("Milestone20 Unity Terrain - Original Battle Royale World") == null ||
                m20Terrain == null ||
                m20Terrain.terrainData == null ||
                m20Terrain.terrainData.terrainLayers == null ||
                m20Terrain.terrainData.terrainLayers.Length < 5 ||
                FindGameObjectIncludingInactive("M20 Main Asphalt Route") == null ||
                FindGameObjectIncludingInactive("M20 River Water Segment 1") == null ||
                FindGameObjectIncludingInactive("M20 River Bridge North Deck") == null ||
                FindGameObjectIncludingInactive("M20 Riverbend Village House 1 Root") == null ||
                FindGameObjectIncludingInactive("M20 Aster Town Shop Row Building Root") == null ||
                FindGameObjectIncludingInactive("M20 Military Base Command Building Root") == null ||
                FindGameObjectIncludingInactive("M20 Warehouse Main Building Root") == null ||
                FindGameObjectIncludingInactive("M20 Container Yard Stack 1") == null ||
                FindGameObjectIncludingInactive("M20 Gas Station Canopy") == null ||
                FindGameObjectIncludingInactive("M20 Light Probe Grid") == null ||
                FindGameObjectIncludingInactive("M20 Rain System") == null ||
                FindGameObjectIncludingInactive("M20 Dynamic Cloud Root") == null ||
                FindGameObjectIncludingInactive("Milestone20 Dynamic Weather Controller") == null ||
                Object.FindAnyObjectByType<RuntimeDynamicWeatherController>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 20 terrain system, world locations, probes, weather, or optimization foundations were not created.");
            }

            if (FindGameObjectIncludingInactive("BZ_Ambient_Town") == null)
            {
                Failures.Add("Milestone 12 town ambient audio source was not created.");
            }

            ValidateMilestone21Runtime();

            if (Object.FindAnyObjectByType<Volume>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 10 URP post-processing Volume was not created.");
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null || mainCamera.GetComponent<UniversalAdditionalCameraData>() == null)
            {
                Failures.Add("Main Camera is missing UniversalAdditionalCameraData for URP post-processing.");
            }
            else if (!mainCamera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing)
            {
                Failures.Add("Main Camera URP post-processing is disabled.");
            }

            ValidateVisualModules();
            ValidateVisualSliceRuntime();
            ValidateMilestone17Runtime();
            ValidateVisualAssetCatalog();
            ValidateQualityPresetNames();
            ValidateMovementInputChain();

            if (!HasAssignedRenderPipeline())
            {
                Failures.Add("Unity 6 URP render pipeline asset was not assigned.");
            }

            MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                string objectName = meshFilter.name;
                bool humanoidPart = objectName.Contains("Torso") || objectName.Contains("Arm") || objectName.Contains("Leg") || objectName.Contains("Bot");
                if (humanoidPart && meshFilter.sharedMesh.name.Contains("Capsule"))
                {
                    Failures.Add($"Humanoid visual part '{objectName}' still uses a capsule mesh.");
                }
            }

            if (Object.FindAnyObjectByType<FloatingJoystick>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Milestone 5 mobile joystick control was not created.");
            }

            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("AC_PlayerHumanoid");
            if (controller == null)
            {
                Failures.Add("AC_PlayerHumanoid runtime animator controller was not found in Resources.");
            }

            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Failures.Add("EventSystem was not created.");
            }
            else
            {
                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    Failures.Add("EventSystem is missing InputSystemUIInputModule.");
                }
                else if (eventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset == null)
                {
                    Failures.Add("EventSystem InputSystemUIInputModule has no default UI actions assigned.");
                }

                BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
                if (modules.Length != 1)
                {
                    Failures.Add($"EventSystem should have exactly one input module, but has {modules.Length}.");
                }
                else if (modules[0].GetType() != typeof(InputSystemUIInputModule))
                {
                    Failures.Add($"EventSystem contains unexpected input module '{modules[0].GetType().Name}'.");
                }
            }

            if (Object.FindAnyObjectByType<MobileButtonLayoutProfile>() == null)
            {
                Failures.Add("Mobile button layout profile was not created.");
            }

            if (Object.FindAnyObjectByType<RuntimeDeveloperPanel>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Editor developer test panel was not created.");
            }

            Text[] textComponents = Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
            if (textComponents.Length == 0)
            {
                Failures.Add("No Unity UI Text components were created.");
            }

            foreach (Text text in textComponents)
            {
                if (text.font == null)
                {
                    Failures.Add($"Text component '{text.name}' has no font assigned.");
                }
            }

            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include);
            foreach (Renderer renderer in renderers)
            {
                Material material = renderer.sharedMaterial;
                if (material == null)
                {
                    Failures.Add($"Renderer '{renderer.name}' has no material.");
                    continue;
                }

                Shader shader = material.shader;
                if (shader == null || shader.name.Contains("InternalErrorShader"))
                {
                    Failures.Add($"Renderer '{renderer.name}' uses a missing/error shader.");
                }
            }
            ValidateTerrainSurfaceMaterials(renderers);

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject item in allObjects)
            {
                Component[] components = item.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        Failures.Add($"GameObject '{item.name}' has a missing script/component reference.");
                    }
                }
            }
        }

        private static void ValidateMilestone21Runtime()
        {
            bool hasM21Terrain = false;
            Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include);
            foreach (Terrain terrain in terrains)
            {
                if (terrain == null ||
                    terrain.name != "Milestone21 Unity Terrain - First Playable Island" ||
                    terrain.terrainData == null ||
                    terrain.terrainData.terrainLayers == null ||
                    terrain.terrainData.terrainLayers.Length < 5)
                {
                    continue;
                }

                hasM21Terrain = true;
                break;
            }

            NavMeshTriangulation navTriangulation = NavMesh.CalculateTriangulation();
            if (FindGameObjectIncludingInactive("Milestone21 Playable Original Island Root") == null ||
                !hasM21Terrain ||
                FindGameObjectIncludingInactive("M21 Island Water Perimeter") == null ||
                FindGameObjectIncludingInactive("M21 Village House 1 Root") == null ||
                FindGameObjectIncludingInactive("M21 Warehouse District Warehouse A Root") == null ||
                FindGameObjectIncludingInactive("M21 Container Yard Stack 1") == null ||
                FindGameObjectIncludingInactive("M21 Military Checkpoint Gatehouse Root") == null ||
                FindGameObjectIncludingInactive("M21 Gas Station Canopy") == null ||
                FindGameObjectIncludingInactive("M21 School Main Building Root") == null ||
                FindGameObjectIncludingInactive("M21 Hospital Main Building Root") == null ||
                FindGameObjectIncludingInactive("M21 River Segment 1") == null ||
                FindGameObjectIncludingInactive("M21 River North Bridge Deck") == null ||
                FindGameObjectIncludingInactive("M21 River South Bridge Deck") == null ||
                FindGameObjectIncludingInactive("M21 Loot Anchor School 1") == null ||
                FindGameObjectIncludingInactive("Milestone21 Runtime NavMesh Bake") == null ||
                navTriangulation.vertices == null ||
                navTriangulation.vertices.Length == 0)
            {
                Failures.Add("Milestone 21 playable island terrain, POIs, interiors, loot anchors, bridges, or runtime NavMesh were not created.");
            }
        }

        private static void ValidateMovementInputChain()
        {
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            int playerTagCount = 0;
            foreach (GameObject item in allObjects)
            {
                if (item != null && item.CompareTag("Player"))
                {
                    playerTagCount++;
                }
            }

            if (playerTagCount != 1)
            {
                Failures.Add($"Expected exactly one runtime Player-tagged object, found {playerTagCount}.");
            }

            GameObject playerObject = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (playerObject == null)
            {
                Failures.Add("Runtime player object was not created.");
                return;
            }

            if (playerObject.isStatic)
            {
                Failures.Add("Runtime player object is marked static.");
            }

            if (playerObject.transform.parent != null)
            {
                Failures.Add($"Runtime player should not start parented, but parent is '{playerObject.transform.parent.name}'.");
            }

            ThirdPersonMobileController controller = playerObject.GetComponent<ThirdPersonMobileController>();
            if (controller == null)
            {
                Failures.Add("Runtime player is missing ThirdPersonMobileController.");
                return;
            }

            if (!controller.enabled)
            {
                Failures.Add("ThirdPersonMobileController is disabled.");
            }

            CharacterController characterController = playerObject.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Failures.Add("Runtime player is missing CharacterController.");
            }
            else
            {
                if (!characterController.enabled)
                {
                    Failures.Add("Runtime player CharacterController is disabled.");
                }

                if (characterController.height <= 0.2f || characterController.radius <= 0.05f)
                {
                    Failures.Add("Runtime player CharacterController dimensions are invalid.");
                }
            }

            Rigidbody body = playerObject.GetComponent<Rigidbody>();
            if (body != null)
            {
                if (body.isKinematic)
                {
                    Failures.Add("Runtime player Rigidbody is kinematic while movement expects active locomotion.");
                }

                RigidbodyConstraints frozenPositionAxes =
                    RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezePositionZ;
                if ((body.constraints & frozenPositionAxes) == frozenPositionAxes)
                {
                    Failures.Add("Runtime player Rigidbody freezes every position axis.");
                }
            }

            FloatingJoystick joystick = Object.FindAnyObjectByType<FloatingJoystick>(FindObjectsInactive.Include);
            if (joystick == null)
            {
                Failures.Add("Movement joystick was not created.");
            }
            else
            {
                if (!joystick.HasVisualReferences)
                {
                    Failures.Add("Movement joystick is missing background or handle references.");
                }

                joystick.SetValueForRuntimeTest(Vector2.up);
                if (joystick.Value.y <= 0.5f)
                {
                    Failures.Add("Movement joystick test input did not produce a forward Vector2.");
                }
            }

            controller.ControlsEnabled = true;
            controller.SetExternalMotionLock(false);
            controller.RefreshMovementDebugSnapshot();
            Vector2 finalInput = controller.ReadExternalMovementInput();
            if (joystick != null && finalInput.y <= 0.5f)
            {
                Failures.Add($"ThirdPersonMobileController did not read joystick movement input. Final input was {finalInput}.");
            }

            if (controller.DebugMoveSpeed <= 0.1f)
            {
                Failures.Add("ThirdPersonMobileController reports zero move speed.");
            }

            if (!controller.DebugControlsEnabled)
            {
                Failures.Add("ThirdPersonMobileController reports controls disabled during movement validation.");
            }

            if (!controller.DebugHasCharacterController)
            {
                Failures.Add("ThirdPersonMobileController reports no enabled CharacterController.");
            }

            if (Object.FindAnyObjectByType<MobileLookArea>(FindObjectsInactive.Include) == null)
            {
                Failures.Add("Right-side mobile look area was not created.");
            }

            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null || eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Failures.Add("Valid Input System EventSystem was not available for joystick pointer input.");
            }

            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                Failures.Add("Time.timeScale is zero during movement validation.");
            }
        }

        private static void ValidateTerrainSurfaceMaterials(Renderer[] renderers)
        {
            bool foundGround = false;
            bool foundRoad = false;
            bool foundWater = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Material material = renderer.sharedMaterial;
                if (material == null || !IsTerrainSurfaceRenderer(renderer, material))
                {
                    continue;
                }

                string materialName = material.name;
                string objectName = renderer.name;
                foundGround |= ContainsAny(objectName, materialName, "Ground", "Grass", "Terrain", "Beach", "Sand", "Cliff", "Hill");
                foundRoad |= ContainsAny(objectName, materialName, "Road", "Track", "Highway", "Intersection");
                foundWater |= ContainsAny(objectName, materialName, "River", "Lake", "Water");

                Shader shader = material.shader;
                string shaderName = shader != null ? shader.name : string.Empty;
                if (!shaderName.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal))
                {
                    Failures.Add($"Terrain surface '{objectName}' uses non-URP shader '{shaderName}'.");
                }

                if (TryGetMaterialColor(material, out Color color) && IsBrightYellowDiagnosticColor(color))
                {
                    Failures.Add($"Terrain surface '{objectName}' is using a bright yellow diagnostic color instead of an authored terrain material.");
                }
            }

            if (!foundGround)
            {
                Failures.Add("No authored grass/dirt/sand/rock terrain surface was found.");
            }

            if (!foundRoad)
            {
                Failures.Add("No authored road material surface was found.");
            }

            if (!foundWater)
            {
                Failures.Add("No authored water material surface was found.");
            }
        }

        private static bool IsTerrainSurfaceRenderer(Renderer renderer, Material material)
        {
            if (renderer == null || renderer.GetComponent<TextMesh>() != null)
            {
                return false;
            }

            string objectName = renderer != null ? renderer.name : string.Empty;
            string materialName = material != null ? material.name : string.Empty;
            return ContainsAny(
                objectName,
                materialName,
                "Ground",
                "Terrain",
                "Grass",
                "Dirt",
                "Road",
                "Track",
                "Highway",
                "Intersection",
                "Beach",
                "Sand",
                "Cliff",
                "Rock",
                "River",
                "Lake",
                "Water",
                "Hill",
                "Mountain",
                "Field");
        }

        private static bool ContainsAny(string objectName, string materialName, params string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (objectName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    materialName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetMaterialColor(Material material, out Color color)
        {
            color = Color.clear;
            if (material == null)
            {
                return false;
            }

            if (material.HasProperty("_BaseColor"))
            {
                color = material.GetColor("_BaseColor");
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
                return true;
            }

            return false;
        }

        private static bool IsBrightYellowDiagnosticColor(Color color)
        {
            return color.a > 0.2f && color.r >= 0.85f && color.g >= 0.75f && color.b <= 0.25f;
        }

        private static void ValidateVisualSliceRuntime()
        {
            string[] requiredObjects =
            {
                "VS_StreetBlock_Root",
                "VS_Enterable_House_Root",
                "VS_Warehouse_Root",
                "VS_Tactical_Humanoid_Showcase",
                "VS_Rook17_AssaultRifle_Showcase",
                "VS_Sable9_Pistol_Showcase"
            };

            foreach (string objectName in requiredObjects)
            {
                GameObject target = FindGameObjectIncludingInactive(objectName);
                if (target == null)
                {
                    Failures.Add($"Visual slice object '{objectName}' was not created.");
                    continue;
                }

                if (target.GetComponent<BattleZoneArtReplacementSlot>() == null)
                {
                    Failures.Add($"Visual slice object '{objectName}' is missing a BattleZoneArtReplacementSlot.");
                }

                if (target.GetComponent<LODGroup>() == null)
                {
                    Failures.Add($"Visual slice object '{objectName}' is missing an LODGroup.");
                }
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject item in allObjects)
            {
                if (!item.name.StartsWith("VS_", StringComparison.Ordinal))
                {
                    continue;
                }

                MeshFilter meshFilter = item.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                string meshName = meshFilter.sharedMesh.name;
                if (meshName.Contains("Cube") || meshName.Contains("Capsule"))
                {
                    Failures.Add($"Visual slice mesh '{item.name}' still uses Unity primitive mesh '{meshName}'.");
                }
            }
        }

        private static void ValidateMilestone17Runtime()
        {
            string[] requiredObjects =
            {
                "M17_VerticalSlice_200m_Root",
                "M17_Apartment_Root",
                "M17_Warehouse_Root",
                "M17_Shop_Root",
                "M17_GuardTower_Root",
                "M17_TacticalCharacter_PrefabSlot",
                "M17_Terrain_Grass_Base_200m",
                "M17_Terrain_Dirt_Service_Yard",
                "M17_Terrain_Sand_River_Bank",
                "M17_Main_Road_Asphalt",
                "M17_Main_Road_NorthSidewalk",
                "M17_River_Edge_Water",
                "M17_VerticalSlice_ReflectionProbe"
            };

            foreach (string objectName in requiredObjects)
            {
                if (FindGameObjectIncludingInactive(objectName) == null)
                {
                    Failures.Add($"Milestone 17 vertical-slice object '{objectName}' was not created.");
                }
            }

            string[] slotObjects =
            {
                "M17_VerticalSlice_200m_Root",
                "M17_Apartment_Root",
                "M17_Warehouse_Root",
                "M17_Shop_Root",
                "M17_GuardTower_Root",
                "M17_TacticalCharacter_PrefabSlot"
            };

            foreach (string objectName in slotObjects)
            {
                GameObject target = FindGameObjectIncludingInactive(objectName);
                if (target == null)
                {
                    continue;
                }

                if (target.GetComponent<BattleZoneArtReplacementSlot>() == null)
                {
                    Failures.Add($"Milestone 17 object '{objectName}' is missing a BattleZoneArtReplacementSlot.");
                }

                if (target.GetComponent<LODGroup>() == null)
                {
                    Failures.Add($"Milestone 17 object '{objectName}' is missing an LODGroup.");
                }
            }

            GameObject probeObject = FindGameObjectIncludingInactive("M17_VerticalSlice_ReflectionProbe");
            if (probeObject != null && probeObject.GetComponent<ReflectionProbe>() == null)
            {
                Failures.Add("Milestone 17 reflection probe object exists but has no ReflectionProbe component.");
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject item in allObjects)
            {
                if (!item.name.StartsWith("M17_", StringComparison.Ordinal))
                {
                    continue;
                }

                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial == null)
                {
                    Failures.Add($"Milestone 17 renderer '{item.name}' has no material assigned.");
                }

                MeshFilter meshFilter = item.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                string meshName = meshFilter.sharedMesh.name;
                if (meshName.Contains("Cube") || meshName.Contains("Capsule") || meshName.Contains("Sphere") || meshName.Contains("Cylinder"))
                {
                    Failures.Add($"Milestone 17 mesh '{item.name}' still uses Unity primitive mesh '{meshName}'.");
                }
            }
        }

        private static void ValidateVisualAssetCatalog()
        {
            BattleZoneVisualCatalog catalog = AssetDatabase.LoadAssetAtPath<BattleZoneVisualCatalog>("Assets/BattleZoneMobile/ArtPipeline/VisualDefinitions/BattleZoneVisualCatalog.asset");
            if (catalog == null)
            {
                Failures.Add("BattleZone visual asset catalog was not generated.");
                return;
            }

            if (catalog.definitions == null || catalog.definitions.Length < 8)
            {
                Failures.Add("BattleZone visual asset catalog should contain at least 8 replacement definitions.");
                return;
            }

            foreach (BattleZoneVisualAssetDefinition definition in catalog.definitions)
            {
                if (definition == null)
                {
                    Failures.Add("BattleZone visual asset catalog contains a null definition.");
                    continue;
                }

                if (definition.visualPrefab == null)
                {
                    Failures.Add($"Visual asset definition '{definition.name}' has no prefab slot assigned.");
                }
                else if (definition.visualPrefab.GetComponent<BattleZoneArtReplacementSlot>() == null)
                {
                    Failures.Add($"Visual prefab slot '{definition.visualPrefab.name}' is missing BattleZoneArtReplacementSlot.");
                }

                if (definition.requiredSockets == null || definition.requiredSockets.Length == 0)
                {
                    Failures.Add($"Visual asset definition '{definition.name}' has no required socket list.");
                }

                if (!definition.usesURPCompatibleShaders)
                {
                    Failures.Add($"Visual asset definition '{definition.name}' is not marked URP-compatible.");
                }
            }
        }

        private static void ValidateQualityPresetNames()
        {
            string[] qualityNames = QualitySettings.names;
            string[] requiredNames = { "Low", "Medium", "High", "Ultra" };
            foreach (string requiredName in requiredNames)
            {
                bool found = false;
                for (int i = 0; i < qualityNames.Length; i++)
                {
                    if (qualityNames[i] == requiredName)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Failures.Add($"Quality preset '{requiredName}' was not found.");
                }
            }
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                Failures.Add($"{type}: {condition}\n{stackTrace}");
            }
        }

        private static GameObject FindGameObjectIncludingInactive(string objectName)
        {
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject item in allObjects)
            {
                if (item.name == objectName)
                {
                    return item;
                }
            }

            return null;
        }

        private static bool HasAssignedRenderPipeline()
        {
            Type graphicsSettingsType = typeof(UnityEngine.Rendering.GraphicsSettings);
            object defaultPipeline = GetStaticProperty(graphicsSettingsType, "defaultRenderPipeline");
            object activePipeline = GetStaticProperty(graphicsSettingsType, "currentRenderPipeline");
            object legacyPipeline = GetStaticProperty(graphicsSettingsType, "renderPipelineAsset");
            return defaultPipeline != null || activePipeline != null || legacyPipeline != null;
        }

        private static void ValidateVisualModules()
        {
            RuntimeVisualModule[] modules = Object.FindObjectsByType<RuntimeVisualModule>(FindObjectsInactive.Include);
            if (modules.Length < 24)
            {
                Failures.Add($"Expected at least 24 runtime visual modules, found {modules.Length}.");
                return;
            }

            bool hasCharacter = false;
            bool hasWeapon = false;
            bool hasBuilding = false;
            bool hasRoad = false;
            bool hasTerrain = false;
            bool hasVegetation = false;
            bool hasLighting = false;
            bool hasVehicle = false;
            bool hasLoot = false;

            foreach (RuntimeVisualModule module in modules)
            {
                if (module == null)
                {
                    continue;
                }

                switch (module.Kind)
                {
                    case RuntimeVisualModuleKind.Character:
                        hasCharacter = true;
                        break;
                    case RuntimeVisualModuleKind.Weapon:
                        hasWeapon = true;
                        break;
                    case RuntimeVisualModuleKind.Building:
                        hasBuilding = true;
                        break;
                    case RuntimeVisualModuleKind.Road:
                        hasRoad = true;
                        break;
                    case RuntimeVisualModuleKind.Terrain:
                        hasTerrain = true;
                        break;
                    case RuntimeVisualModuleKind.Vegetation:
                        hasVegetation = true;
                        break;
                    case RuntimeVisualModuleKind.Lighting:
                        hasLighting = true;
                        break;
                    case RuntimeVisualModuleKind.Vehicle:
                        hasVehicle = true;
                        break;
                    case RuntimeVisualModuleKind.Loot:
                        hasLoot = true;
                        break;
                }
            }

            if (!hasCharacter || !hasWeapon || !hasBuilding || !hasRoad || !hasTerrain || !hasVegetation || !hasLighting || !hasVehicle || !hasLoot)
            {
                Failures.Add("Runtime visual module architecture is missing one or more required module kinds.");
            }
        }

        private static object GetStaticProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            return property != null ? property.GetValue(null) : null;
        }
    }
}
#endif
