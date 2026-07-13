using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class BattleZoneRuntimeBuilder : MonoBehaviour
    {
        private const int BotCount = 8;

        private readonly List<GameObject> runtimeObjects = new List<GameObject>();
        private readonly List<Transform> runtimeCoverPoints = new List<Transform>();
        private readonly List<string> runtimeLocationNames = new List<string>();
        private readonly List<Vector3> runtimeLocationPositions = new List<Vector3>();

        private Material groundMaterial;
        private Material coverMaterial;
        private Material playerMaterial;
        private Material playerAccentMaterial;
        private Material botMaterial;
        private Material lootWeaponMaterial;
        private Material lootAmmoMaterial;
        private Material medkitMaterial;
        private Material zoneMaterial;
        private Material buildingMaterial;
        private Material roadMaterial;
        private Material treeMaterial;
        private Material treeTrunkMaterial;
        private Material rockMaterial;
        private Material militaryMaterial;
        private Material warehouseMaterial;
        private Material terrainAccentMaterial;
        private Material windowMaterial;
        private Material fenceMaterial;
        private Material vehicleMaterial;
        private Material vehicleAccentMaterial;
        private Material airdropMaterial;
        private Material coastMaterial;
        private Material waterMaterial;
        private Material facadeMaterial;
        private Material facadeAccentMaterial;
        private Material roofMaterial;
        private Material sidewalkMaterial;
        private Material laneMarkMaterial;
        private Material hillMaterial;
        private Material grassMaterial;
        private Material foliageLightMaterial;
        private Material flowerMaterial;
        private Material dirtRoadMaterial;
        private Material beachMaterial;
        private Material lakeMaterial;
        private Material cliffMaterial;
        private Material cropMaterial;
        private Material cloudMaterial;
        private Material lootCrateMaterial;
        private Material armorDisplayMaterial;
        private Material fuelCanMaterial;
        private Material m17GrassMaterial;
        private Material m17DirtMaterial;
        private Material m17SandMaterial;
        private Material m17RoadMaterial;
        private Material m17SidewalkMaterial;
        private Material m17RiverMaterial;
        private Material m17ApartmentMaterial;
        private Material m17ShopMaterial;
        private Material m17WarehouseMaterial;
        private Material m17WarmLightMaterial;
        private Material m17BarrelMaterial;
        private Material m20TerrainGrassMaterial;
        private Material m20TerrainDirtMaterial;
        private Material m20TerrainRockMaterial;
        private Material m20TerrainSandMaterial;
        private Material m20TerrainRoadMaterial;
        private Material m20WaterMaterial;
        private Material m20HouseWallMaterial;
        private Material m20HouseRoofMaterial;
        private Material m20BrickMaterial;
        private Material m20MetalMaterial;
        private Material m20ContainerBlueMaterial;
        private Material m20ContainerOrangeMaterial;
        private Material m20FuelStationMaterial;

        private int playerLayer;
        private int botLayer;
        private int groundLayer;
        private int lootLayer;
        private int zoneLayer;
        private Light runtimeSunLight;
        private NavMeshData runtimeNavMeshData;
        private NavMeshDataInstance runtimeNavMeshInstance;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = 82f;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
            QualitySettings.shadowCascades = 2;
            QualitySettings.lodBias = 1.05f;
            QualitySettings.particleRaycastBudget = 128;
            Input.multiTouchEnabled = true;

            ResolveLayers();
            CreateMaterials();

            BuildLighting();
            BuildPostProcessing();
            BuildAudioSystem();
            BuildArena();
            BuildReflectionProbes();
            BuildAmbientSoundscape();

            Camera mainCamera = BuildCamera();
            RuntimeUIRefs uiRefs = BuildUI();
            AdvancedWeaponData[] weaponRoster = CombatWeaponRoster.LoadAll();
            PlayerRefs playerRefs = BuildPlayer(mainCamera, uiRefs, weaponRoster);
            BotAI botPrefab = BuildBotTemplate();
            LootItem[] lootPrefabs = BuildLootTemplates();

            SafeZoneController zone = BuildSafeZone(playerRefs.Transform, playerRefs.Health);
            LootSpawner lootSpawner = BuildLootSpawner(lootPrefabs);
            BotManager botManager = BuildBotManager(botPrefab);
            BuildVehicles(uiRefs);
            BuildWeaponTestArea(weaponRoster, lootPrefabs);
            BattleRoyaleMatchFlow matchFlow = BuildMatchFlow(playerRefs, uiRefs, lootPrefabs, botManager);

            GameObject managerObject = new GameObject("GameManager");
            runtimeObjects.Add(managerObject);
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            managerObject.AddComponent<AndroidPerformanceSetup>();
            gameManager.ConfigureForRuntime(
                playerRefs.Controller,
                playerRefs.Health,
                playerRefs.Weapons,
                playerRefs.Inventory,
                playerRefs.VehicleInteractor,
                playerRefs.SpawnPoint,
                uiRefs.UIManager,
                botManager,
                lootSpawner,
                zone,
                matchFlow);

            uiRefs.DeveloperPanel?.SetTargets(playerRefs.Transform, botManager, zone, playerRefs.Controller, gameManager, playerRefs.Health);
            WireButtons(uiRefs, playerRefs, gameManager);
        }

        private void ResolveLayers()
        {
            playerLayer = ResolveLayer("Player");
            botLayer = ResolveLayer("Bot");
            groundLayer = ResolveLayer("Ground");
            lootLayer = ResolveLayer("Loot");
            zoneLayer = ResolveLayer("Zone");
        }

        private int ResolveLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? layer : 0;
        }

        private void CreateMaterials()
        {
            groundMaterial = CreateMaterial("Runtime Ground", new Color(0.19f, 0.34f, 0.20f));
            coverMaterial = CreateMaterial("Runtime Cover", new Color(0.38f, 0.42f, 0.42f));
            playerMaterial = CreateMaterial("Runtime Player", new Color(0.07f, 0.48f, 0.66f));
            playerAccentMaterial = CreateMaterial("Runtime Player Accent", new Color(0.12f, 0.18f, 0.22f));
            botMaterial = CreateMaterial("Runtime Bot", new Color(0.88f, 0.24f, 0.18f));
            lootWeaponMaterial = CreateMaterial("Runtime Weapon Loot", new Color(0.16f, 0.17f, 0.18f));
            lootAmmoMaterial = CreateMaterial("Runtime Ammo Loot", new Color(0.94f, 0.76f, 0.18f));
            medkitMaterial = CreateMaterial("Runtime Medkit", new Color(0.96f, 0.96f, 0.92f));
            buildingMaterial = CreateMaterial("Runtime Building", new Color(0.48f, 0.50f, 0.48f));
            roadMaterial = CreateMaterial("Runtime Road", new Color(0.18f, 0.20f, 0.21f));
            treeMaterial = CreateMaterial("Runtime Tree Canopy", new Color(0.10f, 0.38f, 0.18f));
            treeTrunkMaterial = CreateMaterial("Runtime Tree Trunk", new Color(0.34f, 0.22f, 0.12f));
            rockMaterial = CreateMaterial("Runtime Rock", new Color(0.30f, 0.34f, 0.34f));
            militaryMaterial = CreateMaterial("Runtime Military Base", new Color(0.32f, 0.39f, 0.31f));
            warehouseMaterial = CreateMaterial("Runtime Warehouse", new Color(0.42f, 0.45f, 0.48f));
            terrainAccentMaterial = CreateMaterial("Runtime Terrain Accent", new Color(0.31f, 0.49f, 0.22f));
            windowMaterial = CreateMaterial("Runtime Window Glass", new Color(0.10f, 0.19f, 0.24f));
            fenceMaterial = CreateMaterial("Runtime Fence", new Color(0.28f, 0.24f, 0.18f));
            vehicleMaterial = CreateMaterial("Runtime Vehicle Body", new Color(0.18f, 0.34f, 0.36f));
            vehicleAccentMaterial = CreateMaterial("Runtime Vehicle Trim", new Color(0.08f, 0.10f, 0.11f));
            airdropMaterial = CreateMaterial("Runtime Airdrop Crate", new Color(0.12f, 0.34f, 0.62f));
            coastMaterial = CreateMaterial("Runtime Coastal Sand", new Color(0.48f, 0.45f, 0.34f));
            waterMaterial = CreateMaterial("Runtime River Water", new Color(0.08f, 0.28f, 0.43f, 0.68f));
            ConfigureTransparentMaterial(waterMaterial);
            facadeMaterial = CreateMaterial("M10 Modular Facade", new Color(0.50f, 0.55f, 0.52f));
            facadeAccentMaterial = CreateMaterial("M10 Modular Facade Accent", new Color(0.23f, 0.30f, 0.32f));
            roofMaterial = CreateMaterial("M10 Roof Panels", new Color(0.16f, 0.18f, 0.18f));
            sidewalkMaterial = CreateMaterial("M10 Sidewalk Concrete", new Color(0.46f, 0.48f, 0.45f));
            laneMarkMaterial = CreateMaterial("M10 Road Markings", new Color(0.86f, 0.82f, 0.66f));
            hillMaterial = CreateMaterial("M10 Hillside Terrain", new Color(0.23f, 0.36f, 0.21f));
            grassMaterial = CreateMaterial("M10 Grass Detail", new Color(0.15f, 0.38f, 0.18f));
            foliageLightMaterial = CreateMaterial("M10 Young Foliage", new Color(0.18f, 0.50f, 0.24f));
            flowerMaterial = CreateMaterial("M10 Wildflower Accent", new Color(0.86f, 0.68f, 0.32f));
            dirtRoadMaterial = CreateMaterial("M11 Packed Dirt Road", new Color(0.31f, 0.25f, 0.17f));
            beachMaterial = CreateMaterial("M11 River Beach Sand", new Color(0.50f, 0.47f, 0.35f));
            lakeMaterial = CreateMaterial("M11 Lake Water", new Color(0.06f, 0.25f, 0.38f, 0.68f));
            ConfigureTransparentMaterial(lakeMaterial);
            cliffMaterial = CreateMaterial("M11 Cliff Stone", new Color(0.25f, 0.28f, 0.27f));
            cropMaterial = CreateMaterial("M11 Farm Crops", new Color(0.46f, 0.49f, 0.18f));
            cloudMaterial = CreateMaterial("M11 Soft Cloud", new Color(0.82f, 0.88f, 0.89f, 0.52f));
            ConfigureTransparentMaterial(cloudMaterial);
            lootCrateMaterial = CreateMaterial("M11 Loot Crate Blue", new Color(0.12f, 0.25f, 0.38f));
            armorDisplayMaterial = CreateMaterial("M11 Armor Display", new Color(0.18f, 0.23f, 0.27f));
            fuelCanMaterial = CreateMaterial("M11 Fuel Can Red", new Color(0.72f, 0.12f, 0.08f));
            m17GrassMaterial = CreateMaterial("M17 Vertical Slice Grass", new Color(0.16f, 0.34f, 0.18f));
            m17DirtMaterial = CreateMaterial("M17 Vertical Slice Dirt", new Color(0.29f, 0.23f, 0.16f));
            m17SandMaterial = CreateMaterial("M17 Vertical Slice River Sand", new Color(0.48f, 0.44f, 0.34f));
            m17RoadMaterial = CreateMaterial("M17 Vertical Slice Asphalt", new Color(0.075f, 0.085f, 0.09f));
            m17SidewalkMaterial = CreateMaterial("M17 Vertical Slice Sidewalk", new Color(0.38f, 0.40f, 0.38f));
            m17RiverMaterial = CreateMaterial("M17 Vertical Slice River Water", new Color(0.055f, 0.24f, 0.36f, 0.70f));
            ConfigureTransparentMaterial(m17RiverMaterial);
            m17ApartmentMaterial = CreateMaterial("M17 Apartment Concrete", new Color(0.46f, 0.49f, 0.47f));
            m17ShopMaterial = CreateMaterial("M17 Shop Painted Wall", new Color(0.34f, 0.42f, 0.39f));
            m17WarehouseMaterial = CreateMaterial("M17 Warehouse Steel", new Color(0.30f, 0.34f, 0.35f));
            m17WarmLightMaterial = CreateMaterial("M17 Warm Lamp Glass", new Color(0.95f, 0.72f, 0.36f));
            m17BarrelMaterial = CreateMaterial("M17 Utility Barrel", new Color(0.42f, 0.23f, 0.18f));
            m20TerrainGrassMaterial = CreateMaterial("M20 PBR Grass Ground", new Color(0.18f, 0.34f, 0.17f));
            ConfigureMaterialSurface(m20TerrainGrassMaterial, 0.42f, 0f);
            m20TerrainDirtMaterial = CreateMaterial("M20 PBR Packed Dirt", new Color(0.32f, 0.25f, 0.17f));
            ConfigureMaterialSurface(m20TerrainDirtMaterial, 0.50f, 0f);
            m20TerrainRockMaterial = CreateMaterial("M20 PBR Wet Rock", new Color(0.25f, 0.28f, 0.27f));
            ConfigureMaterialSurface(m20TerrainRockMaterial, 0.58f, 0f);
            m20TerrainSandMaterial = CreateMaterial("M20 PBR River Sand", new Color(0.52f, 0.48f, 0.36f));
            ConfigureMaterialSurface(m20TerrainSandMaterial, 0.46f, 0f);
            m20TerrainRoadMaterial = CreateMaterial("M20 PBR Asphalt Road", new Color(0.08f, 0.09f, 0.095f));
            ConfigureMaterialSurface(m20TerrainRoadMaterial, 0.62f, 0f);
            m20WaterMaterial = CreateMaterial("M20 Reflective River Water", new Color(0.045f, 0.22f, 0.34f, 0.72f));
            ConfigureMaterialSurface(m20WaterMaterial, 0.82f, 0f);
            ConfigureTransparentMaterial(m20WaterMaterial);
            m20HouseWallMaterial = CreateMaterial("M20 PBR Plaster House Wall", new Color(0.54f, 0.53f, 0.47f));
            ConfigureMaterialSurface(m20HouseWallMaterial, 0.44f, 0f);
            m20HouseRoofMaterial = CreateMaterial("M20 PBR Clay Roof", new Color(0.43f, 0.18f, 0.12f));
            ConfigureMaterialSurface(m20HouseRoofMaterial, 0.38f, 0f);
            m20BrickMaterial = CreateMaterial("M20 PBR Muted Brick", new Color(0.44f, 0.25f, 0.20f));
            ConfigureMaterialSurface(m20BrickMaterial, 0.47f, 0f);
            m20MetalMaterial = CreateMaterial("M20 PBR Industrial Metal", new Color(0.23f, 0.27f, 0.29f));
            ConfigureMaterialSurface(m20MetalMaterial, 0.62f, 0.15f);
            m20ContainerBlueMaterial = CreateMaterial("M20 PBR Container Blue", new Color(0.10f, 0.24f, 0.38f));
            ConfigureMaterialSurface(m20ContainerBlueMaterial, 0.56f, 0.08f);
            m20ContainerOrangeMaterial = CreateMaterial("M20 PBR Container Ochre", new Color(0.55f, 0.31f, 0.14f));
            ConfigureMaterialSurface(m20ContainerOrangeMaterial, 0.52f, 0.05f);
            m20FuelStationMaterial = CreateMaterial("M20 PBR Fuel Station Accent", new Color(0.78f, 0.24f, 0.12f));
            ConfigureMaterialSurface(m20FuelStationMaterial, 0.50f, 0.02f);
            zoneMaterial = CreateMaterial("Runtime Zone", new Color(0.16f, 0.55f, 1f, 0.18f));
            ConfigureTransparentMaterial(zoneMaterial);
        }

        private Material CreateMaterial(string materialName, Color color)
        {
            Shader shader = SelectRuntimeShader();
            Material material = new Material(shader);
            material.name = materialName;
            ApplyMaterialColor(material, color);
            ConfigureMaterialSurface(material, 0.38f, 0f);
            ConfigureOpaqueMaterial(material);
            material.enableInstancing = true;
            return material;
        }

        private static Shader SelectRuntimeShader()
        {
            string[] shaderNames =
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Standard",
                "Unlit/Color",
                "Sprites/Default"
            };

            foreach (string shaderName in shaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    return shader;
                }
            }

            return Shader.Find("Sprites/Default");
        }

        private static void ApplyMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void ConfigureMaterialSurface(Material material, float smoothness, float metallic)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            }
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        private Material CreateParticleMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            }

            if (shader == null)
            {
                shader = SelectRuntimeShader();
            }

            Material material = new Material(shader);
            material.name = materialName;
            ApplyMaterialColor(material, color);
            ConfigureTransparentMaterial(material);
            return material;
        }

        private static void ConfigureOpaqueMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 0f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            if (material.HasProperty("_AlphaClip"))
            {
                material.SetFloat("_AlphaClip", 0f);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 1f);
            }

            material.SetOverrideTag("RenderType", "Opaque");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
        }

        private Font LoadRuntimeFont()
        {
            try
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font != null)
                {
                    return font;
                }
            }
            catch
            {
                // Unity 6 provides LegacyRuntime.ttf. If a later editor changes it,
                // fall back without surfacing the old built-in font exception.
            }

            return Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "Helvetica Neue", "Helvetica" }, 16);
        }

        private void DestroyRuntimeObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void BuildLighting()
        {
            GameObject lightObject = new GameObject("Sun Light");
            runtimeObjects.Add(lightObject);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.93f, 0.78f);
            light.intensity = 1.38f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.78f;
            light.shadowBias = 0.045f;
            light.shadowNormalBias = 0.38f;
            light.lightmapBakeType = LightmapBakeType.Mixed;
            lightObject.transform.rotation = Quaternion.Euler(42f, -31f, 0f);
            runtimeSunLight = light;
            RenderSettings.sun = runtimeSunLight;
            RuntimeVisualModule lightModule = lightObject.AddComponent<RuntimeVisualModule>();
            lightModule.Configure(RuntimeVisualModuleKind.Lighting, "M20_URP_HDR_KeySun");

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.43f, 0.49f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.27f, 0.20f);
            Shader skyboxShader = Shader.Find("Skybox/Procedural");
            if (skyboxShader != null)
            {
                Material skybox = new Material(skyboxShader)
                {
                    name = "Milestone20 HDR Procedural Sky"
                };
                if (skybox.HasProperty("_SkyTint")) skybox.SetColor("_SkyTint", new Color(0.64f, 0.78f, 0.94f) * 1.12f);
                if (skybox.HasProperty("_GroundColor")) skybox.SetColor("_GroundColor", new Color(0.27f, 0.34f, 0.29f));
                if (skybox.HasProperty("_SunSize")) skybox.SetFloat("_SunSize", 0.032f);
                if (skybox.HasProperty("_AtmosphereThickness")) skybox.SetFloat("_AtmosphereThickness", 0.92f);
                if (skybox.HasProperty("_Exposure")) skybox.SetFloat("_Exposure", 1.12f);
                RenderSettings.skybox = skybox;
            }
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.64f, 0.75f, 0.78f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0038f;
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 0.7f;
            RenderSettings.reflectionBounces = 1;
        }

        private void BuildPostProcessing()
        {
            GameObject volumeObject = new GameObject("Milestone10 URP Global Post Processing");
            runtimeObjects.Add(volumeObject);
            RuntimeVisualModule module = volumeObject.AddComponent<RuntimeVisualModule>();
            module.Configure(RuntimeVisualModuleKind.Lighting, "M10_URP_PostProcessing");

            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Runtime Milestone10 Visual Foundation Profile";
            volume.profile = profile;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.08f);
            bloom.threshold.Override(1.15f);
            bloom.scatter.Override(0.38f);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0f);
            color.contrast.Override(9f);
            color.saturation.Override(8f);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.16f);
            vignette.smoothness.Override(0.42f);

            Tonemapping tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral);
        }

        private void BuildAudioSystem()
        {
            GameObject audioObject = new GameObject("RuntimeAudioBank");
            runtimeObjects.Add(audioObject);
            audioObject.AddComponent<RuntimeAudioBank>();
        }

        private void BuildAmbientSoundscape()
        {
            GameObject soundscapeObject = new GameObject("RuntimeAmbientSoundscape");
            runtimeObjects.Add(soundscapeObject);
            RuntimeAmbientSoundscape soundscape = soundscapeObject.AddComponent<RuntimeAmbientSoundscape>();
            soundscape.Configure(new Vector3(62f, 0f, 0f), new Vector3(-168f, 0f, -150f), new Vector3(166f, 0f, -146f));
        }

        private void BuildReflectionProbes()
        {
            CreateReflectionProbe("BZ_Main Reflection Probe", new Vector3(0f, 10f, 0f), new Vector3(260f, 60f, 260f), 0.42f);
            CreateReflectionProbe("Kestrel Factory Reflection Probe", new Vector3(192f, 8f, -18f), new Vector3(92f, 34f, 92f), 0.35f);
            CreateReflectionProbe("M10 Aurora City Reflection Probe", new Vector3(-56f, 10f, -206f), new Vector3(124f, 42f, 96f), 0.34f);
            CreateReflectionProbe("M10 River Valley Reflection Probe", new Vector3(68f, 5f, 14f), new Vector3(86f, 28f, 196f), 0.38f);
            CreateReflectionProbe("M11 Central City Reflection Probe", new Vector3(-8f, 11f, -26f), new Vector3(160f, 48f, 132f), 0.36f);
            CreateReflectionProbe("M11 Mountain Lake Reflection Probe", new Vector3(-98f, 7f, 214f), new Vector3(136f, 38f, 96f), 0.42f);
            CreateReflectionProbe("M11 Industrial Reflection Probe", new Vector3(214f, 9f, -72f), new Vector3(116f, 40f, 112f), 0.34f);
        }

        private void CreateReflectionProbe(string objectName, Vector3 position, Vector3 size, float intensity)
        {
            GameObject probeObject = new GameObject(objectName);
            runtimeObjects.Add(probeObject);
            probeObject.transform.position = position;
            ReflectionProbe probe = probeObject.AddComponent<ReflectionProbe>();
            probe.size = size;
            probe.intensity = intensity;
            probe.importance = 1;
        }

        private void BuildArena()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            runtimeObjects.Add(ground);
            ground.name = "Legacy Prototype Arena Ground Hidden Under M20 Terrain";
            ground.layer = groundLayer;
            ground.transform.position = new Vector3(0f, -1.25f, 0f);
            ground.transform.localScale = new Vector3(46f, 1f, 46f);
            ground.GetComponent<Renderer>().material = groundMaterial;

            CreateWorldBlock("Central Road A", new Vector3(0f, 0.025f, 0f), new Vector3(15f, 0.05f, 386f), roadMaterial);
            CreateWorldBlock("Central Road B", new Vector3(0f, 0.03f, 0f), new Vector3(386f, 0.05f, 13f), roadMaterial);

            Vector3[] coverPositions =
            {
                new Vector3(-28f, 1.5f, 18f),
                new Vector3(-12f, 1.5f, 30f),
                new Vector3(18f, 1.5f, 24f),
                new Vector3(35f, 1.5f, -8f),
                new Vector3(10f, 1.5f, -28f),
                new Vector3(-26f, 1.5f, -26f),
                new Vector3(-48f, 1.5f, 2f),
                new Vector3(48f, 1.5f, 34f),
                new Vector3(0f, 1.5f, 48f),
                new Vector3(44f, 1.5f, -42f)
            };

            for (int i = 0; i < coverPositions.Length; i++)
            {
                GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
                runtimeObjects.Add(cover);
                cover.name = $"Angular Cover {i + 1}";
                cover.layer = groundLayer;
                cover.transform.position = coverPositions[i];
                cover.transform.rotation = Quaternion.Euler(0f, i * 23f, 0f);
                cover.transform.localScale = new Vector3(4f + i % 3, 3f, 3f + i % 2);
                cover.GetComponent<Renderer>().material = coverMaterial;
            }

            CreateBuilding("North Depot", new Vector3(-38f, 2.8f, 54f), new Vector3(14f, 5.6f, 11f));
            CreateBuilding("East Relay House", new Vector3(54f, 2.5f, 8f), new Vector3(12f, 5f, 16f));
            CreateBuilding("South Garage", new Vector3(-8f, 2.2f, -58f), new Vector3(20f, 4.4f, 10f));
            CreateBuilding("West Workshop", new Vector3(-58f, 2.4f, -8f), new Vector3(10f, 4.8f, 18f));

            Vector3[] barricades =
            {
                new Vector3(-18f, 0.6f, 5f),
                new Vector3(18f, 0.6f, -5f),
                new Vector3(5f, 0.6f, 18f),
                new Vector3(-5f, 0.6f, -18f),
                new Vector3(36f, 0.6f, 42f),
                new Vector3(-42f, 0.6f, 36f)
            };

            for (int i = 0; i < barricades.Length; i++)
            {
                GameObject barricade = CreateWorldBlock($"Low Cover Barricade {i + 1}", barricades[i], new Vector3(7f, 1.2f, 1.2f), coverMaterial);
                barricade.transform.rotation = Quaternion.Euler(0f, i * 28f, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                runtimeObjects.Add(tower);
                tower.name = $"Signal Pillar {i + 1}";
                tower.layer = groundLayer;
                tower.transform.position = Quaternion.Euler(0f, i * 90f + 35f, 0f) * new Vector3(62f, 3f, 0f);
                tower.transform.localScale = new Vector3(2f, 3f, 2f);
                tower.GetComponent<Renderer>().material = coverMaterial;
            }

            for (int i = 0; i < 6; i++)
            {
                GameObject spawnPoint = new GameObject($"Match Spawn Point {i + 1}");
                runtimeObjects.Add(spawnPoint);
                spawnPoint.transform.position = Quaternion.Euler(0f, i * 60f, 0f) * new Vector3(66f, 1.1f, 0f);
            }

            BuildMilestone2WorldDressing();
            BuildMilestone3WorldExpansion();
            BuildMilestone4WorldExpansion();
            BuildMilestone5WorldPolish();
            BuildMilestone9AlphaPolish();
            BuildMilestone10VisualFoundation();
            BuildMilestone11OriginalWorldFoundation();
            BuildMilestone12ProfessionalUpgrade();
            BuildVisualSliceArtPipelineArea();
            BuildMilestone17VerticalSliceArea();
            BuildMilestone19DropExperienceTerrain();
            BuildMilestone20RealisticWorldFoundation();
            BuildMilestone21PlayableIslandMap();
            BuildMilestone21RuntimeNavMesh();
        }

        private void BuildMilestone19DropExperienceTerrain()
        {
            GameObject root = new GameObject("Milestone19 Drop Experience Terrain Root");
            runtimeObjects.Add(root);

            RegisterNamedLocation("M19 Drop Village", new Vector3(-172f, 0f, -28f));
            RegisterNamedLocation("M19 Drop Forest", new Vector3(-118f, 0f, -176f));
            RegisterNamedLocation("M19 Drop River", new Vector3(126f, 0f, -18f));
            RegisterNamedLocation("M19 Hill Road", new Vector3(-12f, 0f, 176f));
            RegisterNamedLocation("M19 Container Yard", new Vector3(178f, 0f, 54f));
            RegisterNamedLocation("M19 Military Camp", new Vector3(178f, 0f, -172f));

            CreateWorldBlock("M19 Large Drop Grassland", new Vector3(0f, 0.018f, 0f), new Vector3(470f, 0.035f, 470f), grassMaterial);
            CreateWorldBlock("M19 North Rolling Hill", new Vector3(-18f, 1.15f, 184f), new Vector3(110f, 2.3f, 42f), hillMaterial).transform.rotation = Quaternion.Euler(0f, 14f, -2f);
            CreateWorldBlock("M19 West Rolling Hill", new Vector3(-186f, 0.95f, -86f), new Vector3(82f, 1.9f, 36f), hillMaterial).transform.rotation = Quaternion.Euler(0f, -24f, 2f);
            CreateWorldBlock("M19 Dirt Landing Ridge", new Vector3(38f, 0.18f, 128f), new Vector3(76f, 0.14f, 18f), dirtRoadMaterial).transform.rotation = Quaternion.Euler(0f, -9f, 0f);

            for (int i = 0; i < 7; i++)
            {
                Vector3 riverPosition = new Vector3(126f + Mathf.Sin(i * 0.75f) * 10f, 0.055f, -146f + i * 46f);
                GameObject river = CreateWorldBlock($"M19 Drop River Segment {i + 1}", riverPosition, new Vector3(26f + (i % 2) * 4f, 0.05f, 44f), lakeMaterial);
                river.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i * 0.6f) * 10f, 0f);
                TagVisualModule(river, RuntimeVisualModuleKind.Terrain, $"M19_Drop_River_{i + 1}");
            }

            CreateBridge("M19 Drop Bridge", new Vector3(126f, 0f, -18f));
            CreateWorldBlock("M19 Village Road", new Vector3(-106f, 0.07f, -24f), new Vector3(146f, 0.05f, 10f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 3f, 0f);
            CreateWorldBlock("M19 Bridge Road", new Vector3(64f, 0.075f, -18f), new Vector3(118f, 0.05f, 9f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            CreateWorldBlock("M19 Forest Dirt Road", new Vector3(-120f, 0.075f, -128f), new Vector3(108f, 0.05f, 7f), dirtRoadMaterial).transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            CreateWorldBlock("M19 Military Supply Road", new Vector3(174f, 0.075f, -92f), new Vector3(10f, 0.05f, 154f), roadMaterial).transform.rotation = Quaternion.Euler(0f, -5f, 0f);

            for (int i = 0; i < 6; i++)
            {
                Vector3 housePosition = new Vector3(-196f + (i % 3) * 24f, 2.3f, -46f + (i / 3) * 32f);
                CreateBuilding($"M19 Village House {i + 1}", housePosition, new Vector3(12f + (i % 2) * 3f, 4.6f, 10f + (i % 3) * 2f));
            }

            CreateWorldBlock("M19 Village Well Cover", new Vector3(-168f, 0.8f, -18f), new Vector3(4.2f, 1.6f, 4.2f), coverMaterial);
            CreateFenceLine("M19 Village Low Fence", new Vector3(-172f, 0.75f, 12f), 84f, true);

            for (int i = 0; i < 7; i++)
            {
                CreateVegetationCluster($"M19 Drop Forest Cluster {i + 1}", new Vector3(-156f + (i % 3) * 34f, 0f, -198f + (i / 3) * 30f), 0.9f + (i % 3) * 0.12f);
            }

            for (int i = 0; i < 14; i++)
            {
                Vector3 stackPosition = new Vector3(150f + (i % 4) * 13f, 1.05f + (i % 3) * 0.32f, 28f + (i / 4) * 12f);
                Material material = i % 3 == 0 ? lootCrateMaterial : i % 3 == 1 ? vehicleMaterial : warehouseMaterial;
                GameObject container = CreateWorldBlock($"M19 Container Yard Stack {i + 1}", stackPosition, new Vector3(10.5f, 2.1f + (i % 2) * 0.8f, 4.4f), material);
                container.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? 0f : 90f, 0f);
            }

            CreateFenceLine("M19 Container Yard Fence North", new Vector3(174f, 0.75f, 96f), 92f, true);
            CreateFenceLine("M19 Container Yard Fence West", new Vector3(128f, 0.75f, 52f), 82f, false);

            CreateWorldBlock("M19 Military Camp Command Tent", new Vector3(170f, 2.2f, -176f), new Vector3(20f, 4.4f, 12f), militaryMaterial);
            CreateWorldBlock("M19 Military Camp Barracks", new Vector3(202f, 2.4f, -156f), new Vector3(24f, 4.8f, 12f), militaryMaterial);
            CreateWorldBlock("M19 Military Sandbag North", new Vector3(178f, 0.7f, -134f), new Vector3(58f, 1.4f, 2.2f), coverMaterial);
            CreateWorldBlock("M19 Military Sandbag West", new Vector3(144f, 0.7f, -170f), new Vector3(2.2f, 1.4f, 58f), coverMaterial);
            BuildWatchTower("M19 Military Camp Watch Tower", new Vector3(220f, 0f, -204f), -42f);
        }

        private void BuildMilestone20RealisticWorldFoundation()
        {
            GameObject root = new GameObject("Milestone20 Realistic Original World Root");
            runtimeObjects.Add(root);
            TagVisualModule(root, RuntimeVisualModuleKind.Terrain, "M20_Realistic_World_Foundation");

            RegisterNamedLocation("M20 Riverbend Village", new Vector3(-176f, 0f, 24f));
            RegisterNamedLocation("M20 Aster Town", new Vector3(-34f, 0f, -156f));
            RegisterNamedLocation("M20 North Military Base", new Vector3(190f, 0f, -190f));
            RegisterNamedLocation("M20 Rail Warehouse", new Vector3(116f, 0f, 124f));
            RegisterNamedLocation("M20 Shipping Yard", new Vector3(196f, 0f, 56f));
            RegisterNamedLocation("M20 Hillside Gas Station", new Vector3(-42f, 0f, 176f));
            RegisterNamedLocation("M20 Pine River Forest", new Vector3(-168f, 0f, -174f));
            RegisterNamedLocation("M20 Ridge Hills", new Vector3(-92f, 0f, 226f));

            BuildM20UnityTerrainSystem(root.transform);
            BuildM20RoadRiverAndBridges(root.transform);
            BuildM20RiverbendVillage(root.transform, new Vector3(-176f, 0f, 24f));
            BuildM20AsterTown(root.transform, new Vector3(-34f, 0f, -156f));
            BuildM20MilitaryBase(root.transform, new Vector3(190f, 0f, -190f));
            BuildM20WarehouseArea(root.transform, new Vector3(116f, 0f, 124f));
            BuildM20ShippingYard(root.transform, new Vector3(196f, 0f, 56f));
            BuildM20GasStation(root.transform, new Vector3(-42f, 0f, 176f));
            BuildM20NaturalScatter(root.transform);
            BuildM20ProbeLightingRig(root.transform);
            BuildM20DynamicWeather(root.transform);
            BuildM20OptimizationMarkers(root.transform);
        }

        private void BuildM20UnityTerrainSystem(Transform parent)
        {
            const int heightResolution = 129;
            const int alphaResolution = 128;
            TerrainData terrainData = new TerrainData
            {
                heightmapResolution = heightResolution,
                alphamapResolution = alphaResolution,
                baseMapResolution = 256,
                size = new Vector3(560f, 32f, 560f)
            };

            float[,] heights = new float[heightResolution, heightResolution];
            for (int y = 0; y < heightResolution; y++)
            {
                float z = Mathf.Lerp(-280f, 280f, y / (float)(heightResolution - 1));
                for (int x = 0; x < heightResolution; x++)
                {
                    float worldX = Mathf.Lerp(-280f, 280f, x / (float)(heightResolution - 1));
                    heights[y, x] = GetM20TerrainHeight01(worldX, z);
                }
            }

            terrainData.SetHeights(0, 0, heights);
            TerrainLayer[] layers = CreateM20TerrainLayers();
            terrainData.terrainLayers = layers;
            terrainData.SetAlphamaps(0, 0, BuildM20TerrainAlphamaps(terrainData.alphamapWidth, terrainData.alphamapHeight, layers.Length));
            AddM20GrassDetails(terrainData);

            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            runtimeObjects.Add(terrainObject);
            terrainObject.name = "Milestone20 Unity Terrain - Original Battle Royale World";
            terrainObject.layer = groundLayer;
            terrainObject.isStatic = true;
            terrainObject.transform.position = new Vector3(-280f, -1.42f, -280f);
            terrainObject.transform.SetParent(parent, true);

            Terrain terrain = terrainObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                terrain.drawInstanced = true;
                terrain.heightmapPixelError = 14f;
                terrain.basemapDistance = 420f;
                terrain.detailObjectDistance = 58f;
                terrain.detailObjectDensity = 0.58f;
                terrain.treeDistance = 240f;
                terrain.shadowCastingMode = ShadowCastingMode.On;
                Shader terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                if (terrainShader != null)
                {
                    Material terrainMaterial = new Material(terrainShader)
                    {
                        name = "M20 URP Terrain Lit Material",
                        enableInstancing = true
                    };
                    terrain.materialTemplate = terrainMaterial;
                }
            }

            TerrainCollider collider = terrainObject.GetComponent<TerrainCollider>();
            if (collider != null)
            {
                collider.terrainData = terrainData;
            }

            TagVisualModule(terrainObject, RuntimeVisualModuleKind.Terrain, "M20_Unity_Terrain_System");
        }

        private TerrainLayer[] CreateM20TerrainLayers()
        {
            return new[]
            {
                CreateM20TerrainLayer("M20 TerrainLayer Grass", new Color(0.16f, 0.32f, 0.15f), new Color(0.21f, 0.42f, 0.18f), new Vector2(12f, 12f)),
                CreateM20TerrainLayer("M20 TerrainLayer Dirt", new Color(0.27f, 0.21f, 0.14f), new Color(0.38f, 0.30f, 0.20f), new Vector2(10f, 10f)),
                CreateM20TerrainLayer("M20 TerrainLayer Road", new Color(0.07f, 0.075f, 0.08f), new Color(0.15f, 0.16f, 0.16f), new Vector2(9f, 9f)),
                CreateM20TerrainLayer("M20 TerrainLayer Rock", new Color(0.23f, 0.25f, 0.24f), new Color(0.36f, 0.38f, 0.36f), new Vector2(11f, 11f)),
                CreateM20TerrainLayer("M20 TerrainLayer Sand", new Color(0.45f, 0.40f, 0.30f), new Color(0.61f, 0.56f, 0.42f), new Vector2(10f, 10f))
            };
        }

        private TerrainLayer CreateM20TerrainLayer(string layerName, Color lowColor, Color highColor, Vector2 tileSize)
        {
            TerrainLayer layer = new TerrainLayer
            {
                name = layerName,
                diffuseTexture = CreateM20TerrainTexture($"{layerName} Procedural Texture", lowColor, highColor),
                tileSize = tileSize,
                metallic = 0f,
                smoothness = 0.34f
            };
            return layer;
        }

        private Texture2D CreateM20TerrainTexture(string textureName, Color lowColor, Color highColor)
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = textureName,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.17f + lowColor.r * 11f, y * 0.17f + highColor.g * 13f);
                    float fine = Mathf.PerlinNoise(x * 0.55f + 19f, y * 0.55f + 23f) * 0.28f;
                    texture.SetPixel(x, y, Color.Lerp(lowColor, highColor, Mathf.Clamp01(grain * 0.72f + fine)));
                }
            }

            texture.Apply(true, true);
            return texture;
        }

        private float[,,] BuildM20TerrainAlphamaps(int width, int height, int layerCount)
        {
            float[,,] maps = new float[height, width, layerCount];
            for (int y = 0; y < height; y++)
            {
                float z = Mathf.Lerp(-280f, 280f, y / (float)(height - 1));
                for (int x = 0; x < width; x++)
                {
                    float worldX = Mathf.Lerp(-280f, 280f, x / (float)(width - 1));
                    float height01 = GetM20TerrainHeight01(worldX, z);
                    float road = GetM20RoadWeight(worldX, z);
                    float river = GetM20RiverWeight(worldX, z);
                    float dirt = Mathf.Max(GetM20LocationMask(worldX, z), 1f - Mathf.Abs(Mathf.PerlinNoise(worldX * 0.014f, z * 0.014f) - 0.42f) * 2.2f);
                    float rock = Mathf.Clamp01((height01 - 0.105f) * 7.8f);
                    float sand = Mathf.Clamp01(river * 1.25f);
                    float grass = Mathf.Max(0.12f, 1f - Mathf.Max(Mathf.Max(road, rock), Mathf.Max(sand, dirt * 0.72f)));

                    float total = grass + dirt + road + rock + sand + 0.0001f;
                    maps[y, x, 0] = grass / total;
                    maps[y, x, 1] = dirt / total;
                    maps[y, x, 2] = road / total;
                    maps[y, x, 3] = rock / total;
                    maps[y, x, 4] = sand / total;
                }
            }

            return maps;
        }

        private void AddM20GrassDetails(TerrainData terrainData)
        {
            DetailPrototype grass = new DetailPrototype
            {
                prototypeTexture = CreateM20TerrainTexture("M20 Terrain Detail Grass Texture", new Color(0.13f, 0.28f, 0.12f), new Color(0.26f, 0.45f, 0.17f)),
                renderMode = DetailRenderMode.GrassBillboard,
                minWidth = 0.28f,
                maxWidth = 0.52f,
                minHeight = 0.42f,
                maxHeight = 0.78f,
                healthyColor = new Color(0.20f, 0.42f, 0.17f),
                dryColor = new Color(0.42f, 0.38f, 0.18f)
            };

            terrainData.detailPrototypes = new[] { grass };
            terrainData.SetDetailResolution(128, 16);
            int[,] details = new int[terrainData.detailHeight, terrainData.detailWidth];
            for (int y = 0; y < terrainData.detailHeight; y++)
            {
                float z = Mathf.Lerp(-280f, 280f, y / (float)(terrainData.detailHeight - 1));
                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    float worldX = Mathf.Lerp(-280f, 280f, x / (float)(terrainData.detailWidth - 1));
                    float blocked = Mathf.Max(GetM20RoadWeight(worldX, z), GetM20RiverWeight(worldX, z));
                    float noise = Mathf.PerlinNoise(worldX * 0.04f + 3f, z * 0.04f + 5f);
                    details[y, x] = blocked > 0.18f ? 0 : Mathf.RoundToInt(Mathf.Lerp(1f, 7f, noise));
                }
            }

            terrainData.SetDetailLayer(0, 0, 0, details);
        }

        private float GetM20TerrainHeight01(float worldX, float worldZ)
        {
            float rolling = Mathf.PerlinNoise(worldX * 0.0085f + 11.2f, worldZ * 0.0085f + 4.8f) * 0.022f;
            float fine = Mathf.PerlinNoise(worldX * 0.022f + 17.4f, worldZ * 0.022f + 9.1f) * 0.010f;
            float height = 0.042f + rolling + fine;
            height += M20Bell(worldX, worldZ, -96f, 230f, 120f, 58f) * 0.175f;
            height += M20Bell(worldX, worldZ, -226f, -130f, 82f, 96f) * 0.095f;
            height += M20Bell(worldX, worldZ, 224f, -228f, 96f, 84f) * 0.120f;
            height += M20Bell(worldX, worldZ, 36f, 246f, 90f, 56f) * 0.080f;
            height = Mathf.Lerp(height, 0.043f, GetM20RoadWeight(worldX, worldZ) * 0.75f);
            height = Mathf.Lerp(height, 0.038f, GetM20LocationMask(worldX, worldZ) * 0.58f);
            height -= GetM20RiverWeight(worldX, worldZ) * 0.034f;
            return Mathf.Clamp(height, 0.018f, 0.34f);
        }

        private static float M20Bell(float x, float z, float centerX, float centerZ, float widthX, float widthZ)
        {
            float dx = (x - centerX) / Mathf.Max(1f, widthX);
            float dz = (z - centerZ) / Mathf.Max(1f, widthZ);
            return Mathf.Exp(-(dx * dx + dz * dz) * 2.15f);
        }

        private float GetM20RiverWeight(float worldX, float worldZ)
        {
            float riverX = 88f + Mathf.Sin(worldZ * 0.025f) * 18f;
            float distance = Mathf.Abs(worldX - riverX);
            return Mathf.Clamp01(1f - distance / 24f);
        }

        private float GetM20RoadWeight(float worldX, float worldZ)
        {
            float weight = 0f;
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -230f, 30f, 230f, 30f) / 10f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -78f, -230f, -4f, 214f) / 9f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, 42f, -168f, 214f, -190f) / 10f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, 114f, 120f, 210f, 52f) / 9f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -44f, 176f, 102f, 34f) / 8f);
            return Mathf.Clamp01(weight);
        }

        private float GetM20LocationMask(float worldX, float worldZ)
        {
            float mask = 0f;
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -176f, 24f, 58f, 46f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -34f, -156f, 72f, 54f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 190f, -190f, 70f, 62f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 116f, 124f, 58f, 48f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 196f, 56f, 64f, 48f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -42f, 176f, 46f, 38f));
            return Mathf.Clamp01(mask);
        }

        private static float DistanceToM20Segment(float x, float z, float ax, float az, float bx, float bz)
        {
            Vector2 point = new Vector2(x, z);
            Vector2 a = new Vector2(ax, az);
            Vector2 b = new Vector2(bx, bz);
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude <= 0.001f ? 0f : Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(point, a + ab * t);
        }

        private void BuildM20RoadRiverAndBridges(Transform parent)
        {
            CreateM20Road("M20 Main Asphalt Route", parent, new Vector3(0f, 0.105f, 30f), new Vector3(460f, 0.08f, 12f), 0f);
            CreateM20Road("M20 Town North Road", parent, new Vector3(-42f, 0.11f, -12f), new Vector3(12f, 0.08f, 350f), -9f);
            CreateM20Road("M20 Military Connector Road", parent, new Vector3(132f, 0.12f, -178f), new Vector3(180f, 0.08f, 11f), -8f);
            CreateM20Road("M20 Warehouse Service Road", parent, new Vector3(164f, 0.12f, 86f), new Vector3(118f, 0.08f, 10f), -34f);
            CreateM20Road("M20 Gas Station Hill Road", parent, new Vector3(28f, 0.12f, 108f), new Vector3(188f, 0.08f, 9f), -44f);

            for (int i = 0; i < 9; i++)
            {
                float z = -206f + i * 52f;
                float x = 88f + Mathf.Sin(z * 0.025f) * 18f;
                GameObject water = CreateM20Block($"M20 River Water Segment {i + 1}", parent, new Vector3(x, 0.045f, z), new Vector3(28f + (i % 2) * 4f, 0.08f, 56f), m20WaterMaterial, RuntimeVisualModuleKind.Terrain);
                water.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i * 0.7f) * 13f, 0f);
                CreateM20Block($"M20 River Sand Bank {i + 1}", parent, new Vector3(x - 18f, 0.06f, z), new Vector3(9f, 0.06f, 48f), m20TerrainSandMaterial, RuntimeVisualModuleKind.Terrain).transform.rotation = water.transform.rotation;
            }

            CreateBridge("M20 River Bridge North", new Vector3(96f, 0f, 30f));
            CreateBridge("M20 River Bridge South", new Vector3(82f, 0f, -124f));
        }

        private void CreateM20Road(string objectName, Transform parent, Vector3 center, Vector3 scale, float yaw)
        {
            GameObject road = CreateM20Block(objectName, parent, center, scale, m20TerrainRoadMaterial, RuntimeVisualModuleKind.Road);
            road.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            int dashes = Mathf.Max(5, Mathf.RoundToInt(scale.x / 34f));
            Quaternion rotation = road.transform.rotation;
            for (int i = 0; i < dashes; i++)
            {
                float offset = Mathf.Lerp(-scale.x * 0.42f, scale.x * 0.42f, i / (float)(dashes - 1));
                GameObject mark = CreateM20Block($"{objectName} Lane Reflector {i + 1}", parent, center + rotation * new Vector3(offset, 0.055f, 0f), new Vector3(7.2f, 0.04f, 0.24f), laneMarkMaterial, RuntimeVisualModuleKind.Road);
                mark.transform.rotation = rotation;
            }
        }

        private void BuildM20RiverbendVillage(Transform parent, Vector3 origin)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 house = origin + new Vector3(-34f + (i % 4) * 22f, 0f, -22f + (i / 4) * 28f);
                CreateM20House($"M20 Riverbend Village House {i + 1}", parent, house, 0.92f + (i % 3) * 0.08f);
            }

            CreateM20Block("M20 Village Stone Well", parent, origin + new Vector3(10f, 0.78f, 18f), new Vector3(4.2f, 1.55f, 4.2f), m20TerrainRockMaterial, RuntimeVisualModuleKind.Prop);
            CreateFenceLine("M20 Village Timber Fence", origin + new Vector3(0f, 0.72f, 42f), 116f, true);
            CreateM20Block("M20 Village Market Stall", parent, origin + new Vector3(-18f, 1.1f, 22f), new Vector3(9f, 2.2f, 3.6f), m20HouseWallMaterial, RuntimeVisualModuleKind.Prop);
            CreateM20Block("M20 Village Market Tarp", parent, origin + new Vector3(-18f, 2.45f, 22f), new Vector3(10.5f, 0.24f, 4.4f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
        }

        private void CreateM20House(string objectName, Transform parent, Vector3 origin, float scale)
        {
            GameObject root = new GameObject($"{objectName} Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            root.transform.SetParent(parent, true);
            root.isStatic = true;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, objectName);
            List<Renderer> renderers = new List<Renderer>();

            Vector3 body = new Vector3(12f * scale, 4.8f * scale, 10f * scale);
            AddM20Part(root.transform, $"{objectName} Foundation", new Vector3(0f, 0.18f, 0f), new Vector3(body.x + 1.4f, 0.36f, body.z + 1.4f), m20TerrainRockMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Plaster Walls", new Vector3(0f, body.y * 0.5f, 0f), body, m20HouseWallMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Clay Roof A", new Vector3(-body.x * 0.24f, body.y + 0.55f, 0f), new Vector3(body.x * 0.62f, 0.58f, body.z + 1.8f), m20HouseRoofMaterial, renderers).transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            AddM20Part(root.transform, $"{objectName} Clay Roof B", new Vector3(body.x * 0.24f, body.y + 0.55f, 0f), new Vector3(body.x * 0.62f, 0.58f, body.z + 1.8f), m20HouseRoofMaterial, renderers).transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            AddM20Part(root.transform, $"{objectName} Front Door", new Vector3(0f, 1.12f, -body.z * 0.525f), new Vector3(2.0f, 2.25f, 0.12f), roadMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Window L", new Vector3(-body.x * 0.28f, 2.7f, -body.z * 0.535f), new Vector3(1.25f, 0.9f, 0.10f), windowMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Window R", new Vector3(body.x * 0.28f, 2.7f, -body.z * 0.535f), new Vector3(1.25f, 0.9f, 0.10f), windowMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Interior Table", new Vector3(-body.x * 0.18f, 0.72f, body.z * 0.14f), new Vector3(3.6f, 0.55f, 1.4f), coverMaterial, renderers);
            AddM20Part(root.transform, $"{objectName} Loot Shelf", new Vector3(body.x * 0.34f, 1.1f, body.z * 0.16f), new Vector3(1.0f, 2.2f, 3.6f), armorDisplayMaterial, renderers);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, body.y * 0.5f, 0f);
            collider.size = body;
            RegisterCoverPoint($"{objectName} Porch Cover", origin + new Vector3(0f, 0f, -body.z * 0.7f));
            AddM20LODGroup(root, renderers);
        }

        private void BuildM20AsterTown(Transform parent, Vector3 origin)
        {
            CreateModularBuildingAsset("M20 Aster Town Shop Row", origin + new Vector3(-24f, 0f, -8f), new Vector3(32f, 9.8f, 16f), 3, 0.16f);
            CreateModularBuildingAsset("M20 Aster Town Apartments", origin + new Vector3(28f, 0f, -12f), new Vector3(24f, 16.5f, 18f), 5, 0.07f);
            CreateModularBuildingAsset("M20 Aster Town Clinic", origin + new Vector3(-18f, 0f, 32f), new Vector3(20f, 10.5f, 16f), 3, 0.2f);
            CreateM20Block("M20 Aster Town Plaza", parent, origin + new Vector3(0f, 0.11f, 12f), new Vector3(64f, 0.08f, 34f), sidewalkMaterial, RuntimeVisualModuleKind.Road);
            CreateCityPropSet("M20 Aster Town", origin);
            for (int i = 0; i < 6; i++)
            {
                CreateM20Block($"M20 Aster Town Street Planter {i + 1}", parent, origin + new Vector3(-44f + i * 17f, 0.5f, 12f + (i % 2) * 15f), new Vector3(5.5f, 0.65f, 1.8f), fenceMaterial, RuntimeVisualModuleKind.Vegetation);
            }
        }

        private void BuildM20MilitaryBase(Transform parent, Vector3 origin)
        {
            CreateModularBuildingAsset("M20 Military Base Command", origin + new Vector3(0f, 0f, 18f), new Vector3(26f, 10.2f, 18f), 3, 0.04f);
            CreateModularBuildingAsset("M20 Military Base Barracks A", origin + new Vector3(-34f, 0f, -10f), new Vector3(26f, 7.2f, 13f), 2, 0.09f);
            CreateModularBuildingAsset("M20 Military Base Barracks B", origin + new Vector3(34f, 0f, -10f), new Vector3(26f, 7.2f, 13f), 2, 0.12f);
            CreateM20Block("M20 Military Motor Pool", parent, origin + new Vector3(0f, 1.15f, -38f), new Vector3(60f, 2.3f, 22f), m20MetalMaterial, RuntimeVisualModuleKind.Prop);
            CreateFenceLine("M20 Military Base North Fence", origin + new Vector3(0f, 0.75f, 58f), 128f, true);
            CreateFenceLine("M20 Military Base South Fence", origin + new Vector3(0f, 0.75f, -58f), 128f, true);
            CreateFenceLine("M20 Military Base East Fence", origin + new Vector3(66f, 0.75f, 0f), 116f, false);
            CreateFenceLine("M20 Military Base West Fence", origin + new Vector3(-66f, 0.75f, 0f), 116f, false);
            BuildWatchTower("M20 Military Watch Tower NE", origin + new Vector3(58f, 0f, 50f), -38f);
            BuildWatchTower("M20 Military Watch Tower SW", origin + new Vector3(-58f, 0f, -50f), 140f);
        }

        private void BuildM20WarehouseArea(Transform parent, Vector3 origin)
        {
            CreateModularBuildingAsset("M20 Warehouse Main", origin, new Vector3(42f, 13.2f, 28f), 3, 0.12f);
            CreateModularBuildingAsset("M20 Warehouse Office", origin + new Vector3(-40f, 0f, 24f), new Vector3(18f, 8.6f, 14f), 3, 0.18f);
            CreateM20Block("M20 Warehouse Loading Apron", parent, origin + new Vector3(8f, 0.18f, -28f), new Vector3(64f, 0.16f, 16f), m20TerrainRoadMaterial, RuntimeVisualModuleKind.Road);
            for (int i = 0; i < 10; i++)
            {
                CreateM20Block($"M20 Warehouse Pallet Stack {i + 1}", parent, origin + new Vector3(-36f + (i % 5) * 17f, 1.05f, -42f + (i / 5) * 12f), new Vector3(8f, 2.1f + (i % 2) * 0.7f, 5.8f), coverMaterial, RuntimeVisualModuleKind.Prop);
            }
        }

        private void BuildM20ShippingYard(Transform parent, Vector3 origin)
        {
            for (int i = 0; i < 18; i++)
            {
                Vector3 position = origin + new Vector3(-34f + (i % 6) * 14f, 1.1f + (i % 3) * 0.38f, -22f + (i / 6) * 14f);
                Material material = i % 2 == 0 ? m20ContainerBlueMaterial : m20ContainerOrangeMaterial;
                GameObject container = CreateM20Block($"M20 Container Yard Stack {i + 1}", parent, position, new Vector3(11.5f, 2.2f + (i % 3) * 0.65f, 4.7f), material, RuntimeVisualModuleKind.Prop);
                container.transform.rotation = Quaternion.Euler(0f, i % 3 == 0 ? 90f : 0f, 0f);
            }

            CreateM20Block("M20 Shipping Yard Crane Base", parent, origin + new Vector3(48f, 4.2f, 18f), new Vector3(4f, 8.4f, 4f), m20MetalMaterial, RuntimeVisualModuleKind.Prop);
            CreateM20Block("M20 Shipping Yard Crane Arm", parent, origin + new Vector3(24f, 8.4f, 18f), new Vector3(52f, 0.9f, 1.2f), m20MetalMaterial, RuntimeVisualModuleKind.Prop);
            CreateFenceLine("M20 Container Yard Fence North", origin + new Vector3(0f, 0.75f, 42f), 118f, true);
        }

        private void BuildM20GasStation(Transform parent, Vector3 origin)
        {
            CreateM20Block("M20 Gas Station Shop", parent, origin + new Vector3(-10f, 2.4f, 4f), new Vector3(16f, 4.8f, 12f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building);
            CreateM20Block("M20 Gas Station Canopy", parent, origin + new Vector3(12f, 3.4f, -8f), new Vector3(26f, 0.72f, 15f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
            for (int i = 0; i < 3; i++)
            {
                CreateM20Block($"M20 Gas Pump {i + 1}", parent, origin + new Vector3(3f + i * 8f, 0.9f, -8f), new Vector3(1.2f, 1.8f, 0.8f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
            }

            CreateRoadSign("M20 Gas Station Road Sign", origin + new Vector3(-22f, 1.6f, -16f), "FUEL");
        }

        private void BuildM20NaturalScatter(Transform parent)
        {
            for (int i = 0; i < 12; i++)
            {
                CreateVegetationCluster($"M20 Pine River Forest Cluster {i + 1}", new Vector3(-210f + (i % 4) * 28f, 0f, -218f + (i / 4) * 30f), 0.98f + (i % 3) * 0.08f);
            }

            for (int i = 0; i < 14; i++)
            {
                CreateM11Rock($"M20 Ridge Boulder {i + 1}", new Vector3(-136f + (i % 7) * 16f, 0.85f, 204f + (i / 7) * 26f), 1.05f + (i % 4) * 0.18f);
            }

            for (int i = 0; i < 20; i++)
            {
                Vector3 patch = new Vector3(-238f + (i % 10) * 48f, 0.18f, -236f + (i / 10) * 420f);
                CreateM11GrassPatch($"M20 Wild Grass Patch {i + 1}", patch, 7f + (i % 4) * 1.4f);
            }
        }

        private void BuildM20ProbeLightingRig(Transform parent)
        {
            CreateReflectionProbe("M20 Reflection Probe Town", new Vector3(-34f, 9f, -156f), new Vector3(132f, 38f, 116f), 0.48f);
            CreateReflectionProbe("M20 Reflection Probe River", new Vector3(92f, 6f, 24f), new Vector3(84f, 26f, 210f), 0.54f);
            CreateReflectionProbe("M20 Reflection Probe Military", new Vector3(190f, 9f, -190f), new Vector3(142f, 40f, 130f), 0.42f);

            GameObject probeObject = new GameObject("M20 Light Probe Grid");
            runtimeObjects.Add(probeObject);
            probeObject.transform.SetParent(parent, true);
            LightProbeGroup group = probeObject.AddComponent<LightProbeGroup>();
            List<Vector3> probes = new List<Vector3>();
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    probes.Add(new Vector3(x * 92f, 3.2f, z * 92f));
                    probes.Add(new Vector3(x * 92f, 11.5f, z * 92f));
                }
            }

            group.probePositions = probes.ToArray();
            TagVisualModule(probeObject, RuntimeVisualModuleKind.Lighting, "M20_Light_Probe_Grid");

            GameObject marker = new GameObject("M20 Baked Lighting Setup Marker");
            runtimeObjects.Add(marker);
            marker.transform.SetParent(parent, true);
            marker.transform.position = new Vector3(0f, 2f, 0f);
            TagVisualModule(marker, RuntimeVisualModuleKind.Lighting, "M20_Bake_Ready_Mixed_Lighting");
        }

        private void BuildM20DynamicWeather(Transform parent)
        {
            GameObject cloudRoot = new GameObject("M20 Dynamic Cloud Root");
            runtimeObjects.Add(cloudRoot);
            cloudRoot.transform.SetParent(parent, true);
            cloudRoot.transform.position = new Vector3(0f, 72f, 0f);
            TagVisualModule(cloudRoot, RuntimeVisualModuleKind.Lighting, "M20_Dynamic_Clouds");

            for (int i = 0; i < 7; i++)
            {
                GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(cloud);
                cloud.name = $"M20 Dynamic Cloud Puff {i + 1}";
                cloud.transform.SetParent(cloudRoot.transform, false);
                cloud.transform.localPosition = new Vector3(-160f + i * 54f, Mathf.Sin(i) * 5f, -36f + (i % 3) * 36f);
                cloud.transform.localScale = new Vector3(36f + (i % 3) * 12f, 4.8f + (i % 2) * 1.4f, 13f + (i % 4) * 3f);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
                Collider collider = cloud.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyRuntimeObject(collider);
                }
            }

            GameObject rainObject = new GameObject("M20 Rain System");
            runtimeObjects.Add(rainObject);
            rainObject.transform.SetParent(parent, true);
            rainObject.transform.position = new Vector3(0f, 44f, 0f);
            ParticleSystem rain = rainObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.startLifetime = 2.7f;
            main.startSpeed = 28f;
            main.startSize = 0.035f;
            main.maxParticles = 900;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(430f, 1f, 430f);
            ParticleSystemRenderer rainRenderer = rainObject.GetComponent<ParticleSystemRenderer>();
            if (rainRenderer != null)
            {
                rainRenderer.material = CreateParticleMaterial("M20 Rain Drop Particle", new Color(0.62f, 0.78f, 0.90f, 0.55f));
                rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                rainRenderer.lengthScale = 2.6f;
                rainRenderer.velocityScale = 0.18f;
            }

            GameObject controllerObject = new GameObject("Milestone20 Dynamic Weather Controller");
            runtimeObjects.Add(controllerObject);
            controllerObject.transform.SetParent(parent, true);
            RuntimeDynamicWeatherController weather = controllerObject.AddComponent<RuntimeDynamicWeatherController>();
            weather.Configure(runtimeSunLight, rain, cloudRoot.transform, 240f);
        }

        private void BuildM20OptimizationMarkers(Transform parent)
        {
            GameObject marker = new GameObject("M20 Mobile Optimization Marker");
            runtimeObjects.Add(marker);
            marker.transform.SetParent(parent, true);
            marker.transform.position = new Vector3(0f, 1.6f, 0f);
            QualitySettings.shadowDistance = Mathf.Min(82f, QualitySettings.shadowDistance <= 0f ? 82f : QualitySettings.shadowDistance);
            QualitySettings.lodBias = Mathf.Clamp(QualitySettings.lodBias, 0.85f, 1.25f);
            QualitySettings.realtimeReflectionProbes = false;
            TagVisualModule(marker, RuntimeVisualModuleKind.Lighting, "M20_Mobile_LOD_Occlusion_Instancing");

            GameObject occlusionRoot = new GameObject("M20 Occlusion Friendly Static Cell Root");
            runtimeObjects.Add(occlusionRoot);
            occlusionRoot.transform.SetParent(parent, true);
            occlusionRoot.isStatic = true;
            TagVisualModule(occlusionRoot, RuntimeVisualModuleKind.Prop, "M20_Occlusion_Friendly_Cells");
        }

        private GameObject CreateM20Block(string objectName, Transform parent, Vector3 position, Vector3 scale, Material material, RuntimeVisualModuleKind kind)
        {
            GameObject block = CreateWorldBlock(objectName, position, scale, material);
            block.transform.SetParent(parent, true);
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                if (renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.enableInstancing = true;
                }
            }

            TagVisualModule(block, kind, objectName);
            return block;
        }

        private Renderer AddM20Part(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material, List<Renderer> renderers)
        {
            Renderer renderer = AddVisualPart(parent, objectName, localPosition, localScale, material, renderers);
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            if (renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.enableInstancing = true;
            }

            return renderer;
        }

        private void AddM20LODGroup(GameObject root, List<Renderer> renderers)
        {
            if (root == null || renderers == null || renderers.Count == 0)
            {
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }

            lodGroup.SetLODs(new[]
            {
                new LOD(0.20f, renderers.ToArray()),
                new LOD(0.075f, new[] { renderers[0] })
            });
            lodGroup.RecalculateBounds();
        }

        private void BuildMilestone21PlayableIslandMap()
        {
            GameObject root = new GameObject("Milestone21 Playable Original Island Root");
            runtimeObjects.Add(root);
            TagVisualModule(root, RuntimeVisualModuleKind.Terrain, "M21_Playable_Production_Island");

            RegisterNamedLocation("M21 River Village", new Vector3(-196f, 0f, 48f));
            RegisterNamedLocation("M21 Warehouse District", new Vector3(98f, 0f, 132f));
            RegisterNamedLocation("M21 Shipping Yard", new Vector3(208f, 0f, 60f));
            RegisterNamedLocation("M21 Military Checkpoint", new Vector3(194f, 0f, -164f));
            RegisterNamedLocation("M21 Coast Gas Station", new Vector3(-42f, 0f, 186f));
            RegisterNamedLocation("M21 Island School", new Vector3(-34f, 0f, -128f));
            RegisterNamedLocation("M21 Small Hospital", new Vector3(34f, 0f, -92f));
            RegisterNamedLocation("M21 Pine Forest", new Vector3(-204f, 0f, -170f));
            RegisterNamedLocation("M21 North Hills", new Vector3(-86f, 0f, 236f));

            BuildM21IslandTerrain(root.transform);
            BuildM21RoadsRiversAndBridges(root.transform);
            BuildM21Village(root.transform, new Vector3(-196f, 0f, 48f));
            BuildM21WarehouseDistrict(root.transform, new Vector3(98f, 0f, 132f));
            BuildM21ShippingContainerYard(root.transform, new Vector3(208f, 0f, 60f));
            BuildM21MilitaryCheckpoint(root.transform, new Vector3(194f, 0f, -164f));
            BuildM21GasStation(root.transform, new Vector3(-42f, 0f, 186f));
            BuildM21School(root.transform, new Vector3(-34f, 0f, -128f));
            BuildM21Hospital(root.transform, new Vector3(34f, 0f, -92f));
            BuildM21ForestHillsAndScatter(root.transform);
            BuildM21LootAnchors(root.transform);
            BuildM21LightingProbes(root.transform);
        }

        private void BuildM21IslandTerrain(Transform parent)
        {
            const int heightResolution = 129;
            const int alphaResolution = 128;
            TerrainData terrainData = new TerrainData
            {
                heightmapResolution = heightResolution,
                alphamapResolution = alphaResolution,
                baseMapResolution = 256,
                size = new Vector3(640f, 36f, 640f)
            };

            float[,] heights = new float[heightResolution, heightResolution];
            for (int y = 0; y < heightResolution; y++)
            {
                float z = Mathf.Lerp(-320f, 320f, y / (float)(heightResolution - 1));
                for (int x = 0; x < heightResolution; x++)
                {
                    float worldX = Mathf.Lerp(-320f, 320f, x / (float)(heightResolution - 1));
                    heights[y, x] = GetM21TerrainHeight01(worldX, z);
                }
            }

            terrainData.SetHeights(0, 0, heights);
            TerrainLayer[] layers =
            {
                CreateM20TerrainLayer("M21 TerrainLayer Coastal Grass", new Color(0.15f, 0.31f, 0.14f), new Color(0.24f, 0.44f, 0.18f), new Vector2(11f, 11f)),
                CreateM20TerrainLayer("M21 TerrainLayer Packed Dirt", new Color(0.28f, 0.21f, 0.13f), new Color(0.42f, 0.31f, 0.19f), new Vector2(9f, 9f)),
                CreateM20TerrainLayer("M21 TerrainLayer Asphalt", new Color(0.06f, 0.065f, 0.07f), new Color(0.14f, 0.15f, 0.15f), new Vector2(8f, 8f)),
                CreateM20TerrainLayer("M21 TerrainLayer Cliff Rock", new Color(0.22f, 0.24f, 0.23f), new Color(0.38f, 0.39f, 0.36f), new Vector2(10f, 10f)),
                CreateM20TerrainLayer("M21 TerrainLayer Beach Sand", new Color(0.48f, 0.43f, 0.31f), new Color(0.66f, 0.58f, 0.42f), new Vector2(10f, 10f))
            };
            terrainData.terrainLayers = layers;
            terrainData.SetAlphamaps(0, 0, BuildM21TerrainAlphamaps(alphaResolution, alphaResolution, layers.Length));
            AddM21GrassDetails(terrainData);

            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            runtimeObjects.Add(terrainObject);
            terrainObject.name = "Milestone21 Unity Terrain - First Playable Island";
            terrainObject.layer = groundLayer;
            terrainObject.isStatic = true;
            terrainObject.transform.position = new Vector3(-320f, -1.38f, -320f);
            terrainObject.transform.SetParent(parent, true);

            Terrain terrain = terrainObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                terrain.drawInstanced = true;
                terrain.heightmapPixelError = 12f;
                terrain.basemapDistance = 460f;
                terrain.detailObjectDistance = 62f;
                terrain.detailObjectDensity = 0.62f;
                terrain.treeDistance = 260f;
                terrain.shadowCastingMode = ShadowCastingMode.On;
                Shader terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                if (terrainShader != null)
                {
                    terrain.materialTemplate = new Material(terrainShader)
                    {
                        name = "M21 URP Terrain Lit Production Island Material",
                        enableInstancing = true
                    };
                }
            }

            TerrainCollider collider = terrainObject.GetComponent<TerrainCollider>();
            if (collider != null)
            {
                collider.terrainData = terrainData;
            }

            GameObject water = CreateM21Block("M21 Island Water Perimeter", parent, new Vector3(0f, -0.48f, 0f), new Vector3(690f, 0.05f, 690f), m20WaterMaterial, RuntimeVisualModuleKind.Terrain);
            Collider waterCollider = water.GetComponent<Collider>();
            if (waterCollider != null)
            {
                DestroyRuntimeObject(waterCollider);
            }

            TagVisualModule(terrainObject, RuntimeVisualModuleKind.Terrain, "M21_Unity_Terrain_Production_Island");
        }

        private float[,,] BuildM21TerrainAlphamaps(int width, int height, int layerCount)
        {
            float[,,] maps = new float[height, width, layerCount];
            for (int y = 0; y < height; y++)
            {
                float z = Mathf.Lerp(-320f, 320f, y / (float)(height - 1));
                for (int x = 0; x < width; x++)
                {
                    float worldX = Mathf.Lerp(-320f, 320f, x / (float)(width - 1));
                    float height01 = GetM21TerrainHeight01(worldX, z);
                    float road = GetM21RoadWeight(worldX, z);
                    float river = GetM21RiverWeight(worldX, z);
                    float beach = Mathf.Clamp01((Mathf.Max(Mathf.Abs(worldX), Mathf.Abs(z)) - 238f) / 42f + river * 0.55f);
                    float poi = GetM21LocationMask(worldX, z);
                    float dirt = Mathf.Clamp01(poi * 0.9f + (1f - Mathf.Abs(Mathf.PerlinNoise(worldX * 0.018f, z * 0.018f) - 0.5f) * 2f) * 0.25f);
                    float rock = Mathf.Clamp01((height01 - 0.12f) * 8f);
                    float grass = Mathf.Max(0.1f, 1f - Mathf.Max(Mathf.Max(road, beach), Mathf.Max(rock, dirt * 0.6f)));
                    float total = grass + dirt + road + rock + beach + 0.0001f;
                    maps[y, x, 0] = grass / total;
                    maps[y, x, 1] = dirt / total;
                    maps[y, x, 2] = road / total;
                    maps[y, x, 3] = rock / total;
                    maps[y, x, 4] = beach / total;
                }
            }

            return maps;
        }

        private void AddM21GrassDetails(TerrainData terrainData)
        {
            DetailPrototype grass = new DetailPrototype
            {
                prototypeTexture = CreateM20TerrainTexture("M21 Terrain Detail Grass Texture", new Color(0.12f, 0.28f, 0.11f), new Color(0.30f, 0.48f, 0.18f)),
                renderMode = DetailRenderMode.GrassBillboard,
                minWidth = 0.24f,
                maxWidth = 0.56f,
                minHeight = 0.40f,
                maxHeight = 0.82f,
                healthyColor = new Color(0.21f, 0.44f, 0.16f),
                dryColor = new Color(0.43f, 0.39f, 0.18f)
            };

            terrainData.detailPrototypes = new[] { grass };
            terrainData.SetDetailResolution(128, 16);
            int[,] details = new int[terrainData.detailHeight, terrainData.detailWidth];
            for (int y = 0; y < terrainData.detailHeight; y++)
            {
                float z = Mathf.Lerp(-320f, 320f, y / (float)(terrainData.detailHeight - 1));
                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    float worldX = Mathf.Lerp(-320f, 320f, x / (float)(terrainData.detailWidth - 1));
                    float blocked = Mathf.Max(GetM21RoadWeight(worldX, z), Mathf.Max(GetM21RiverWeight(worldX, z), GetM21LocationMask(worldX, z)));
                    float edge = Mathf.Clamp01((Mathf.Max(Mathf.Abs(worldX), Mathf.Abs(z)) - 246f) / 50f);
                    float noise = Mathf.PerlinNoise(worldX * 0.035f + 7f, z * 0.035f + 13f);
                    details[y, x] = blocked > 0.24f || edge > 0.35f ? 0 : Mathf.RoundToInt(Mathf.Lerp(1f, 8f, noise));
                }
            }

            terrainData.SetDetailLayer(0, 0, 0, details);
        }

        private float GetM21TerrainHeight01(float worldX, float worldZ)
        {
            float islandFalloff = Mathf.Clamp01(1f - (new Vector2(worldX / 315f, worldZ / 295f).sqrMagnitude - 0.40f) * 1.28f);
            float rolling = Mathf.PerlinNoise(worldX * 0.0078f + 2.8f, worldZ * 0.0078f + 8.4f) * 0.022f;
            float fine = Mathf.PerlinNoise(worldX * 0.025f + 18f, worldZ * 0.025f + 21f) * 0.008f;
            float height = 0.033f + rolling + fine;
            height += M20Bell(worldX, worldZ, -96f, 236f, 118f, 62f) * 0.17f;
            height += M20Bell(worldX, worldZ, -230f, -170f, 82f, 94f) * 0.10f;
            height += M20Bell(worldX, worldZ, 226f, -206f, 94f, 82f) * 0.11f;
            height = Mathf.Lerp(height, 0.040f, GetM21RoadWeight(worldX, worldZ) * 0.8f);
            height = Mathf.Lerp(height, 0.038f, GetM21LocationMask(worldX, worldZ) * 0.66f);
            height -= GetM21RiverWeight(worldX, worldZ) * 0.026f;
            height = Mathf.Lerp(0.014f, height, islandFalloff);
            return Mathf.Clamp(height, 0.010f, 0.34f);
        }

        private float GetM21RiverWeight(float worldX, float worldZ)
        {
            float riverX = 58f + Mathf.Sin(worldZ * 0.023f) * 24f;
            return Mathf.Clamp01(1f - Mathf.Abs(worldX - riverX) / 25f);
        }

        private float GetM21RoadWeight(float worldX, float worldZ)
        {
            float weight = 0f;
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -260f, 44f, 250f, 44f) / 10f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -72f, -230f, -30f, 220f) / 9f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, 42f, -122f, 206f, -164f) / 9f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, 90f, 132f, 218f, 58f) / 8.5f);
            weight = Mathf.Max(weight, 1f - DistanceToM20Segment(worldX, worldZ, -42f, 184f, 68f, 48f) / 8f);
            return Mathf.Clamp01(weight);
        }

        private float GetM21LocationMask(float worldX, float worldZ)
        {
            float mask = 0f;
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -196f, 48f, 62f, 48f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 98f, 132f, 62f, 48f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 208f, 60f, 64f, 48f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 194f, -164f, 62f, 52f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -42f, 186f, 46f, 36f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, -34f, -128f, 62f, 46f));
            mask = Mathf.Max(mask, M20Bell(worldX, worldZ, 34f, -92f, 46f, 38f));
            return Mathf.Clamp01(mask);
        }

        private void BuildM21RoadsRiversAndBridges(Transform parent)
        {
            CreateM21Road("M21 Main Island Asphalt Road", parent, new Vector3(0f, 0.16f, 44f), new Vector3(510f, 0.08f, 12f), 0f, m20TerrainRoadMaterial);
            CreateM21Road("M21 School Hospital Asphalt Road", parent, new Vector3(-52f, 0.17f, -4f), new Vector3(12f, 0.08f, 390f), -6f, m20TerrainRoadMaterial);
            CreateM21Road("M21 Military Asphalt Connector", parent, new Vector3(126f, 0.17f, -150f), new Vector3(178f, 0.08f, 11f), -13f, m20TerrainRoadMaterial);
            CreateM21Road("M21 Warehouse Asphalt Connector", parent, new Vector3(152f, 0.17f, 94f), new Vector3(144f, 0.08f, 10f), -30f, m20TerrainRoadMaterial);
            CreateM21Road("M21 Village Dirt Road", parent, new Vector3(-154f, 0.16f, 82f), new Vector3(118f, 0.08f, 8f), 18f, dirtRoadMaterial);
            CreateM21Road("M21 Forest Dirt Road", parent, new Vector3(-188f, 0.16f, -112f), new Vector3(138f, 0.08f, 7f), -38f, dirtRoadMaterial);

            for (int i = 0; i < 10; i++)
            {
                float z = -232f + i * 52f;
                float x = 58f + Mathf.Sin(z * 0.023f) * 24f;
                GameObject river = CreateM21Block($"M21 River Segment {i + 1}", parent, new Vector3(x, 0.04f, z), new Vector3(29f + (i % 3) * 3f, 0.08f, 57f), m20WaterMaterial, RuntimeVisualModuleKind.Terrain);
                river.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i * 0.62f) * 13f, 0f);
                CreateM21Block($"M21 River Sand Bank West {i + 1}", parent, new Vector3(x - 19f, 0.07f, z), new Vector3(9f, 0.06f, 48f), m20TerrainSandMaterial, RuntimeVisualModuleKind.Terrain).transform.rotation = river.transform.rotation;
            }

            CreateBridge("M21 River North Bridge", new Vector3(64f, 0f, 44f));
            CreateBridge("M21 River South Bridge", new Vector3(38f, 0f, -128f));
        }

        private void CreateM21Road(string objectName, Transform parent, Vector3 center, Vector3 scale, float yaw, Material material)
        {
            GameObject road = CreateM21Block(objectName, parent, center, scale, material, RuntimeVisualModuleKind.Road);
            road.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Quaternion rotation = road.transform.rotation;
            int marks = Mathf.Max(4, Mathf.RoundToInt(scale.x / 38f));
            for (int i = 0; i < marks; i++)
            {
                float offset = Mathf.Lerp(-scale.x * 0.42f, scale.x * 0.42f, marks == 1 ? 0.5f : i / (float)(marks - 1));
                GameObject mark = CreateM21Block($"{objectName} Center Line {i + 1}", parent, center + rotation * new Vector3(offset, 0.06f, 0f), new Vector3(7f, 0.04f, 0.26f), laneMarkMaterial, RuntimeVisualModuleKind.Road);
                mark.transform.rotation = rotation;
            }
        }

        private void BuildM21Village(Transform parent, Vector3 origin)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 housePosition = origin + new Vector3(-38f + (i % 4) * 24f, 0f, -28f + (i / 4) * 30f);
                CreateM21DetailedHouse($"M21 Village House {i + 1}", parent, housePosition, -8f + i * 7f, 0.95f + (i % 3) * 0.08f);
            }

            CreateM21Block("M21 Village Central Well", parent, origin + new Vector3(10f, 0.8f, 10f), new Vector3(4.4f, 1.6f, 4.4f), m20TerrainRockMaterial, RuntimeVisualModuleKind.Prop);
            CreateM21Block("M21 Village Market Canopy", parent, origin + new Vector3(-16f, 2.3f, 18f), new Vector3(12f, 0.32f, 5f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
            CreateFenceLine("M21 Village Perimeter Fence", origin + new Vector3(0f, 0.72f, 48f), 128f, true);
        }

        private void CreateM21DetailedHouse(string objectName, Transform parent, Vector3 origin, float yaw, float scale)
        {
            GameObject root = new GameObject($"{objectName} Root");
            runtimeObjects.Add(root);
            root.transform.SetParent(parent, true);
            root.transform.SetPositionAndRotation(origin, Quaternion.Euler(0f, yaw, 0f));
            root.isStatic = true;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, objectName);
            List<Renderer> renderers = new List<Renderer>();

            Vector3 size = new Vector3(12f * scale, 4.8f * scale, 10f * scale);
            CreateM21Part(root.transform, $"{objectName} Floor", new Vector3(0f, 0.12f, 0f), new Vector3(size.x, 0.24f, size.z), m20TerrainRockMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Back Wall", new Vector3(0f, 2.4f, size.z * 0.5f), new Vector3(size.x, 4.8f, 0.42f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Left Wall", new Vector3(-size.x * 0.5f, 2.4f, 0f), new Vector3(0.42f, 4.8f, size.z), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Right Wall", new Vector3(size.x * 0.5f, 2.4f, 0f), new Vector3(0.42f, 4.8f, size.z), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Front Wall Left Door Split", new Vector3(-size.x * 0.33f, 2.4f, -size.z * 0.5f), new Vector3(size.x * 0.34f, 4.8f, 0.42f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Front Wall Right Door Split", new Vector3(size.x * 0.33f, 2.4f, -size.z * 0.5f), new Vector3(size.x * 0.34f, 4.8f, 0.42f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Door Header", new Vector3(0f, 4.1f, -size.z * 0.5f), new Vector3(size.x * 0.34f, 1.4f, 0.42f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Interior Room Divider", new Vector3(0f, 1.55f, 1.2f), new Vector3(size.x * 0.62f, 3.1f, 0.28f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Clay Roof Left Plane", new Vector3(-size.x * 0.22f, 5.2f, 0f), new Vector3(size.x * 0.62f, 0.55f, size.z + 1.5f), m20HouseRoofMaterial, RuntimeVisualModuleKind.Building, renderers).transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            CreateM21Part(root.transform, $"{objectName} Clay Roof Right Plane", new Vector3(size.x * 0.22f, 5.2f, 0f), new Vector3(size.x * 0.62f, 0.55f, size.z + 1.5f), m20HouseRoofMaterial, RuntimeVisualModuleKind.Building, renderers).transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            CreateM21Part(root.transform, $"{objectName} Window Front L", new Vector3(-size.x * 0.25f, 2.7f, -size.z * 0.53f), new Vector3(1.2f, 0.95f, 0.08f), windowMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Window Back R", new Vector3(size.x * 0.25f, 2.7f, size.z * 0.53f), new Vector3(1.2f, 0.95f, 0.08f), windowMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Roof Access Ladder", new Vector3(size.x * 0.55f, 2.65f, size.z * 0.15f), new Vector3(0.24f, 5.3f, 0.24f), fenceMaterial, RuntimeVisualModuleKind.Prop, renderers);
            CreateM21Part(root.transform, $"{objectName} Interior Loot Shelf", new Vector3(size.x * 0.30f, 1.1f, 2.5f), new Vector3(1.1f, 2.2f, 3.2f), armorDisplayMaterial, RuntimeVisualModuleKind.Prop, renderers);
            RegisterCoverPoint($"{objectName} Interior Cover", root.transform.TransformPoint(new Vector3(-size.x * 0.22f, 0f, 1.8f)));
            AddM20LODGroup(root, renderers);
        }

        private void BuildM21WarehouseDistrict(Transform parent, Vector3 origin)
        {
            CreateM21LargeBuilding("M21 Warehouse District Warehouse A", parent, origin + new Vector3(-26f, 0f, 0f), new Vector3(42f, 11f, 28f), m20MetalMaterial, 0f, true);
            CreateM21LargeBuilding("M21 Warehouse District Warehouse B", parent, origin + new Vector3(30f, 0f, 26f), new Vector3(36f, 9.8f, 24f), warehouseMaterial, -8f, true);
            CreateM21Block("M21 Warehouse Loading Apron", parent, origin + new Vector3(0f, 0.18f, -32f), new Vector3(88f, 0.16f, 16f), m20TerrainRoadMaterial, RuntimeVisualModuleKind.Road);
            for (int i = 0; i < 12; i++)
            {
                CreateM21Block($"M21 Warehouse Loot Pallet {i + 1}", parent, origin + new Vector3(-48f + (i % 6) * 17f, 1.1f, -48f + (i / 6) * 13f), new Vector3(8f, 2.2f + (i % 2) * 0.8f, 5.8f), coverMaterial, RuntimeVisualModuleKind.Prop);
            }
        }

        private void BuildM21ShippingContainerYard(Transform parent, Vector3 origin)
        {
            for (int i = 0; i < 22; i++)
            {
                Vector3 pos = origin + new Vector3(-44f + (i % 6) * 16f, 1.08f + (i % 3) * 0.42f, -24f + (i / 6) * 13f);
                Material material = i % 2 == 0 ? m20ContainerBlueMaterial : m20ContainerOrangeMaterial;
                GameObject stack = CreateM21Block($"M21 Container Yard Stack {i + 1}", parent, pos, new Vector3(12f, 2.15f + (i % 3) * 0.7f, 4.8f), material, RuntimeVisualModuleKind.Prop);
                stack.transform.rotation = Quaternion.Euler(0f, i % 3 == 0 ? 90f : 0f, 0f);
            }

            CreateM21Block("M21 Container Yard Office", parent, origin + new Vector3(48f, 2.4f, 34f), new Vector3(14f, 4.8f, 10f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building);
            CreateM21Block("M21 Container Yard Crane Tower", parent, origin + new Vector3(52f, 5.0f, -18f), new Vector3(4f, 10f, 4f), m20MetalMaterial, RuntimeVisualModuleKind.Prop);
            CreateM21Block("M21 Container Yard Crane Arm", parent, origin + new Vector3(23f, 9.7f, -18f), new Vector3(62f, 0.9f, 1.2f), m20MetalMaterial, RuntimeVisualModuleKind.Prop);
            CreateFenceLine("M21 Container Yard Perimeter Fence", origin + new Vector3(0f, 0.75f, 48f), 138f, true);
        }

        private void BuildM21MilitaryCheckpoint(Transform parent, Vector3 origin)
        {
            CreateM21LargeBuilding("M21 Military Checkpoint Gatehouse", parent, origin + new Vector3(0f, 0f, 10f), new Vector3(22f, 7f, 14f), militaryMaterial, 0f, true);
            CreateM21Block("M21 Military Checkpoint Vehicle Gate", parent, origin + new Vector3(0f, 2.6f, -10f), new Vector3(42f, 5.2f, 2.2f), militaryMaterial, RuntimeVisualModuleKind.Building);
            CreateM21Block("M21 Military Checkpoint Barrier A", parent, origin + new Vector3(-22f, 0.75f, -28f), new Vector3(18f, 1.5f, 2.4f), coverMaterial, RuntimeVisualModuleKind.Prop).transform.rotation = Quaternion.Euler(0f, -12f, 0f);
            CreateM21Block("M21 Military Checkpoint Barrier B", parent, origin + new Vector3(22f, 0.75f, -26f), new Vector3(18f, 1.5f, 2.4f), coverMaterial, RuntimeVisualModuleKind.Prop).transform.rotation = Quaternion.Euler(0f, 15f, 0f);
            BuildWatchTower("M21 Checkpoint Watch Tower North", origin + new Vector3(38f, 0f, 38f), -34f);
            BuildWatchTower("M21 Checkpoint Watch Tower South", origin + new Vector3(-38f, 0f, -38f), 146f);
            CreateFenceLine("M21 Military Checkpoint Fence North", origin + new Vector3(0f, 0.75f, 52f), 106f, true);
        }

        private void BuildM21GasStation(Transform parent, Vector3 origin)
        {
            CreateM21LargeBuilding("M21 Gas Station Shop", parent, origin + new Vector3(-12f, 0f, 6f), new Vector3(18f, 5.4f, 13f), m20HouseWallMaterial, 0f, false);
            CreateM21Block("M21 Gas Station Canopy", parent, origin + new Vector3(14f, 3.4f, -9f), new Vector3(30f, 0.72f, 16f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
            for (int i = 0; i < 4; i++)
            {
                CreateM21Block($"M21 Gas Pump {i + 1}", parent, origin + new Vector3(2f + i * 8f, 0.92f, -9f), new Vector3(1.2f, 1.85f, 0.82f), m20FuelStationMaterial, RuntimeVisualModuleKind.Prop);
            }

            CreateRoadSign("M21 Gas Station Sign", origin + new Vector3(-25f, 1.6f, -18f), "FUEL");
        }

        private void BuildM21School(Transform parent, Vector3 origin)
        {
            CreateM21LargeBuilding("M21 School Main Building", parent, origin, new Vector3(42f, 10.5f, 26f), m20BrickMaterial, 0f, true);
            CreateM21Block("M21 School Courtyard", parent, origin + new Vector3(0f, 0.14f, 34f), new Vector3(54f, 0.12f, 26f), sidewalkMaterial, RuntimeVisualModuleKind.Road);
            CreateM21Block("M21 School Classroom Divider A", parent, origin + new Vector3(-12f, 1.6f, 0f), new Vector3(0.34f, 3.2f, 22f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building);
            CreateM21Block("M21 School Classroom Divider B", parent, origin + new Vector3(12f, 1.6f, 0f), new Vector3(0.34f, 3.2f, 22f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building);
        }

        private void BuildM21Hospital(Transform parent, Vector3 origin)
        {
            CreateM21LargeBuilding("M21 Hospital Main Building", parent, origin, new Vector3(32f, 8.6f, 22f), medkitMaterial, 4f, true);
            CreateM21Block("M21 Hospital Reception Desk", parent, origin + new Vector3(0f, 0.8f, -5f), new Vector3(9f, 0.9f, 1.5f), coverMaterial, RuntimeVisualModuleKind.Prop);
            CreateM21Block("M21 Hospital Treatment Room Divider", parent, origin + new Vector3(0f, 1.6f, 3f), new Vector3(26f, 3.2f, 0.32f), m20HouseWallMaterial, RuntimeVisualModuleKind.Building);
            CreateM21Block("M21 Hospital Roof Med Symbol", parent, origin + new Vector3(0f, 9.1f, 0f), new Vector3(8f, 0.18f, 1.3f), fuelCanMaterial, RuntimeVisualModuleKind.Prop);
            CreateM21Block("M21 Hospital Roof Med Symbol Cross", parent, origin + new Vector3(0f, 9.12f, 0f), new Vector3(1.3f, 0.18f, 8f), fuelCanMaterial, RuntimeVisualModuleKind.Prop);
        }

        private void BuildM21ForestHillsAndScatter(Transform parent)
        {
            for (int i = 0; i < 15; i++)
            {
                CreateVegetationCluster($"M21 Pine Forest Cluster {i + 1}", new Vector3(-242f + (i % 5) * 28f, 0f, -218f + (i / 5) * 28f), 1.0f + (i % 4) * 0.08f);
            }

            for (int i = 0; i < 16; i++)
            {
                CreateM11Rock($"M21 Hill Rock {i + 1}", new Vector3(-148f + (i % 8) * 19f, 0.8f, 204f + (i / 8) * 30f), 1.0f + (i % 4) * 0.16f);
            }

            for (int i = 0; i < 24; i++)
            {
                Vector3 patch = new Vector3(-278f + (i % 8) * 78f, 0.18f, -248f + (i / 8) * 172f);
                CreateM11GrassPatch($"M21 Grass Cover Patch {i + 1}", patch, 7.5f + (i % 4) * 1.5f);
            }
        }

        private void CreateM21LargeBuilding(string objectName, Transform parent, Vector3 origin, Vector3 size, Material wallMaterial, float yaw, bool rooftopAccess)
        {
            GameObject root = new GameObject($"{objectName} Root");
            runtimeObjects.Add(root);
            root.transform.SetParent(parent, true);
            root.transform.SetPositionAndRotation(origin, Quaternion.Euler(0f, yaw, 0f));
            root.isStatic = true;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, objectName);
            List<Renderer> renderers = new List<Renderer>();

            CreateM21Part(root.transform, $"{objectName} Floor", new Vector3(0f, 0.14f, 0f), new Vector3(size.x, 0.28f, size.z), m20TerrainRockMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Back Wall", new Vector3(0f, size.y * 0.5f, size.z * 0.5f), new Vector3(size.x, size.y, 0.52f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Left Wall", new Vector3(-size.x * 0.5f, size.y * 0.5f, 0f), new Vector3(0.52f, size.y, size.z), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Right Wall", new Vector3(size.x * 0.5f, size.y * 0.5f, 0f), new Vector3(0.52f, size.y, size.z), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Front Wall Left Door Split", new Vector3(-size.x * 0.30f, size.y * 0.5f, -size.z * 0.5f), new Vector3(size.x * 0.40f, size.y, 0.52f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Front Wall Right Door Split", new Vector3(size.x * 0.30f, size.y * 0.5f, -size.z * 0.5f), new Vector3(size.x * 0.40f, size.y, 0.52f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Door Header", new Vector3(0f, size.y - 0.7f, -size.z * 0.5f), new Vector3(size.x * 0.22f, 1.4f, 0.52f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Roof Slab", new Vector3(0f, size.y + 0.32f, 0f), new Vector3(size.x + 1.5f, 0.62f, size.z + 1.5f), roofMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Interior Room A", new Vector3(-size.x * 0.16f, 1.7f, 0f), new Vector3(0.34f, 3.4f, size.z * 0.74f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            CreateM21Part(root.transform, $"{objectName} Interior Room B", new Vector3(size.x * 0.16f, 1.7f, 0f), new Vector3(0.34f, 3.4f, size.z * 0.74f), wallMaterial, RuntimeVisualModuleKind.Building, renderers);
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-size.x * 0.34f, size.x * 0.34f, i / 3f);
                CreateM21Part(root.transform, $"{objectName} Front Window {i + 1}", new Vector3(x, size.y * 0.52f, -size.z * 0.53f), new Vector3(1.45f, 1.1f, 0.10f), windowMaterial, RuntimeVisualModuleKind.Building, renderers);
            }

            if (rooftopAccess)
            {
                Renderer ramp = CreateM21Part(root.transform, $"{objectName} Rooftop Access Ramp", new Vector3(size.x * 0.58f, size.y * 0.32f, size.z * 0.18f), new Vector3(3.2f, 0.42f, size.y * 1.1f), roadMaterial, RuntimeVisualModuleKind.Building, renderers);
                ramp.transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);
                CreateM21Part(root.transform, $"{objectName} Rooftop Cover Rail", new Vector3(0f, size.y + 1.05f, size.z * 0.38f), new Vector3(size.x * 0.72f, 0.7f, 0.45f), coverMaterial, RuntimeVisualModuleKind.Prop, renderers);
            }

            RegisterCoverPoint($"{objectName} Interior Cover", root.transform.TransformPoint(new Vector3(-size.x * 0.22f, 0f, 1.8f)));
            RegisterCoverPoint($"{objectName} Rooftop Cover", root.transform.TransformPoint(new Vector3(0f, size.y + 0.4f, size.z * 0.35f)));
            AddM20LODGroup(root, renderers);
        }

        private Renderer CreateM21Part(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material, RuntimeVisualModuleKind kind, List<Renderer> renderers)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = groundLayer;
            part.isStatic = true;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderers?.Add(renderer);
            TagVisualModule(part, kind, objectName);
            return renderer;
        }

        private void BuildM21LootAnchors(Transform parent)
        {
            CreateM21LootAnchor(parent, "M21 Loot Anchor Village 1", new Vector3(-198f, 0.42f, 52f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor Warehouse 1", new Vector3(88f, 0.42f, 106f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor Container Yard 1", new Vector3(206f, 0.42f, 54f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor Military 1", new Vector3(194f, 0.42f, -154f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor Gas Station 1", new Vector3(-42f, 0.42f, 174f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor School 1", new Vector3(-34f, 0.42f, -116f));
            CreateM21LootAnchor(parent, "M21 Loot Anchor Hospital 1", new Vector3(34f, 0.42f, -82f));
        }

        private void CreateM21LootAnchor(Transform parent, string objectName, Vector3 position)
        {
            GameObject anchor = new GameObject(objectName);
            runtimeObjects.Add(anchor);
            anchor.transform.SetParent(parent, true);
            anchor.transform.position = position;
            TagVisualModule(anchor, RuntimeVisualModuleKind.Loot, objectName);
            CreateM21Block($"{objectName} Floor Marker", parent, position + Vector3.up * 0.02f, new Vector3(1.4f, 0.08f, 1.4f), lootCrateMaterial, RuntimeVisualModuleKind.Loot);
        }

        private void BuildM21LightingProbes(Transform parent)
        {
            CreateReflectionProbe("M21 Reflection Probe Village", new Vector3(-196f, 9f, 48f), new Vector3(126f, 38f, 106f), 0.48f);
            CreateReflectionProbe("M21 Reflection Probe School Hospital", new Vector3(0f, 9f, -108f), new Vector3(124f, 38f, 98f), 0.46f);
            CreateReflectionProbe("M21 Reflection Probe Warehouse Yard", new Vector3(150f, 9f, 96f), new Vector3(150f, 42f, 118f), 0.44f);
            CreateReflectionProbe("M21 Reflection Probe Military", new Vector3(194f, 9f, -164f), new Vector3(126f, 40f, 112f), 0.42f);
        }

        private GameObject CreateM21Block(string objectName, Transform parent, Vector3 position, Vector3 scale, Material material, RuntimeVisualModuleKind kind)
        {
            GameObject block = CreateWorldBlock(objectName, position, scale, material);
            block.transform.SetParent(parent, true);
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                if (renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.enableInstancing = true;
                }
            }

            TagVisualModule(block, kind, objectName);
            return block;
        }

        private void BuildMilestone21RuntimeNavMesh()
        {
            GameObject navObject = new GameObject("Milestone21 Runtime NavMesh Bake");
            runtimeObjects.Add(navObject);
            TagVisualModule(navObject, RuntimeVisualModuleKind.Prop, "M21_Runtime_NavMesh");

            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>(512);
            List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(680f, 90f, 680f));
            int includedLayers = 1 << Mathf.Clamp(groundLayer, 0, 31);
            NavMeshBuilder.CollectSources(bounds, includedLayers, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            settings.agentRadius = 0.42f;
            settings.agentHeight = 1.85f;
            settings.agentClimb = 0.58f;
            settings.agentSlope = 48f;
            settings.overrideVoxelSize = true;
            settings.voxelSize = 0.24f;
            settings.overrideTileSize = true;
            settings.tileSize = 128;

            runtimeNavMeshData = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (runtimeNavMeshData != null)
            {
                runtimeNavMeshData.name = "M21 Runtime Island NavMeshData";
                runtimeNavMeshInstance = NavMesh.AddNavMeshData(runtimeNavMeshData);
            }

            GameObject navSummary = new GameObject("M21 Runtime NavMesh Source Count " + sources.Count);
            runtimeObjects.Add(navSummary);
            navSummary.transform.SetParent(navObject.transform, false);
        }

        private void BuildMilestone2WorldDressing()
        {
            CreateWorldBlock("Low Poly Terrain Ridge North", new Vector3(-12f, 0.22f, 74f), new Vector3(64f, 0.42f, 18f), terrainAccentMaterial).transform.rotation = Quaternion.Euler(0f, -9f, 0f);
            CreateWorldBlock("Low Poly Terrain Ridge South", new Vector3(38f, 0.18f, -74f), new Vector3(70f, 0.36f, 16f), terrainAccentMaterial).transform.rotation = Quaternion.Euler(0f, 12f, 0f);
            CreateWorldBlock("Village Road Spur", new Vector3(-72f, 0.04f, 42f), new Vector3(12f, 0.05f, 58f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            CreateWorldBlock("Base Access Road", new Vector3(68f, 0.04f, -42f), new Vector3(15f, 0.05f, 70f), roadMaterial).transform.rotation = Quaternion.Euler(0f, -22f, 0f);

            BuildSmallVillage(new Vector3(-72f, 0f, 42f));
            BuildMilitaryBase(new Vector3(68f, 0f, -42f));
            BuildWarehouseCompound(new Vector3(32f, 0f, 68f));
            BuildNatureSet();
        }

        private void BuildMilestone3WorldExpansion()
        {
            RegisterNamedLocation("Cedar Town", new Vector3(-112f, 0f, 58f));
            RegisterNamedLocation("Ridge Village", new Vector3(-72f, 0f, 42f));
            RegisterNamedLocation("Iron Warehouse", new Vector3(32f, 0f, 68f));
            RegisterNamedLocation("Sentinel Checkpoint", new Vector3(104f, 0f, -86f));
            RegisterNamedLocation("Pine Forest", new Vector3(-104f, 0f, -92f));

            CreateWorldBlock("North Loop Road", new Vector3(0f, 0.035f, 122f), new Vector3(238f, 0.05f, 10f), roadMaterial);
            CreateWorldBlock("South Loop Road", new Vector3(0f, 0.035f, -122f), new Vector3(238f, 0.05f, 10f), roadMaterial);
            CreateWorldBlock("West Loop Road", new Vector3(-122f, 0.035f, 0f), new Vector3(10f, 0.05f, 238f), roadMaterial);
            CreateWorldBlock("East Loop Road", new Vector3(122f, 0.035f, 0f), new Vector3(10f, 0.05f, 238f), roadMaterial);

            BuildCedarTown(new Vector3(-112f, 0f, 58f));
            BuildCheckpoint(new Vector3(104f, 0f, -86f));
            BuildPineForest(new Vector3(-104f, 0f, -92f));
            BuildWarehouseYard(new Vector3(32f, 0f, 68f));
        }

        private void BuildMilestone4WorldExpansion()
        {
            RegisterNamedLocation("Harbor Coast", new Vector3(0f, 0f, 184f));
            RegisterNamedLocation("Orion Industrial", new Vector3(150f, 0f, 92f));
            RegisterNamedLocation("Fort Array", new Vector3(166f, 0f, -146f));
            RegisterNamedLocation("Old Town", new Vector3(-166f, 0f, 126f));
            RegisterNamedLocation("Westpine Forest", new Vector3(-168f, 0f, -150f));

            CreateWorldBlock("Milestone4 North Highway", new Vector3(0f, 0.04f, 176f), new Vector3(360f, 0.05f, 12f), roadMaterial);
            CreateWorldBlock("Milestone4 South Highway", new Vector3(0f, 0.04f, -176f), new Vector3(360f, 0.05f, 12f), roadMaterial);
            CreateWorldBlock("Milestone4 Industrial Connector", new Vector3(150f, 0.04f, -16f), new Vector3(12f, 0.05f, 210f), roadMaterial);
            CreateWorldBlock("Milestone4 Coast Road", new Vector3(-48f, 0.04f, 186f), new Vector3(220f, 0.05f, 10f), roadMaterial).transform.rotation = Quaternion.Euler(0f, -8f, 0f);
            CreateWorldBlock("Milestone4 Forest Road", new Vector3(-158f, 0.04f, -52f), new Vector3(10f, 0.05f, 210f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 10f, 0f);

            BuildHarborCoast(new Vector3(0f, 0f, 184f));
            BuildOrionIndustrial(new Vector3(150f, 0f, 92f));
            BuildFortArray(new Vector3(166f, 0f, -146f));
            BuildOldTown(new Vector3(-166f, 0f, 126f));
            BuildWestpineForest(new Vector3(-168f, 0f, -150f));
        }

        private void BuildMilestone5WorldPolish()
        {
            CreateFuelStop("North Fuel Stop", new Vector3(74f, 0f, 176f));
            CreateFuelStop("South Fuel Stop", new Vector3(-58f, 0f, -176f));
            CreateRadioMast("Ridge Radio Mast", new Vector3(-132f, 0f, -24f));
            CreateBusStop("Old Town Bus Stop", new Vector3(-142f, 0f, 156f), 14f);
            CreateBusStop("Industrial Shuttle Stop", new Vector3(132f, 0f, 54f), -22f);

            CreateRoadSign("Harbor Road Sign", new Vector3(-64f, 1.5f, 176f), "HARBOR");
            CreateRoadSign("Fort Road Sign", new Vector3(142f, 1.5f, -116f), "FORT");
            CreateRoadSign("Forest Road Sign", new Vector3(-176f, 1.5f, -84f), "FOREST");

            for (int i = 0; i < 6; i++)
            {
                Vector3 polePosition = new Vector3(-138f + i * 48f, 0f, -188f + (i % 2) * 18f);
                CreatePowerPole($"Milestone5 Power Pole {i + 1}", polePosition);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 blindPosition = Quaternion.Euler(0f, i * 72f, 0f) * new Vector3(118f + (i % 2) * 18f, 0f, 0f);
                CreateWatchBlind($"Milestone5 Hunter Blind {i + 1}", blindPosition);
            }
        }

        private void BuildMilestone9AlphaPolish()
        {
            RegisterNamedLocation("Silver River", new Vector3(62f, 0f, 0f));
            RegisterNamedLocation("East Bridge", new Vector3(62f, 0f, 22f));
            RegisterNamedLocation("North Mountains", new Vector3(-30f, 0f, 220f));
            RegisterNamedLocation("Kestrel Factory", new Vector3(192f, 0f, -18f));
            RegisterNamedLocation("Watchtower Ridge", new Vector3(118f, 0f, 128f));

            CreateWorldBlock("Milestone9 Factory Road", new Vector3(154f, 0.045f, -18f), new Vector3(112f, 0.05f, 10f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 4f, 0f);
            CreateWorldBlock("Milestone9 Mountain Road", new Vector3(-32f, 0.045f, 206f), new Vector3(92f, 0.05f, 9f), roadMaterial).transform.rotation = Quaternion.Euler(0f, -18f, 0f);
            CreateWorldBlock("Milestone9 Riverside Track", new Vector3(68f, 0.045f, 30f), new Vector3(9f, 0.05f, 188f), roadMaterial).transform.rotation = Quaternion.Euler(0f, 5f, 0f);

            BuildSilverRiver(new Vector3(62f, 0f, 0f));
            BuildNorthMountains(new Vector3(-30f, 0f, 220f));
            BuildKestrelFactory(new Vector3(192f, 0f, -18f));
            BuildWatchTower("Watchtower Ridge Tower 1", new Vector3(118f, 0f, 128f), 18f);
            BuildWatchTower("Factory Watch Tower", new Vector3(220f, 0f, 16f), -28f);
            BuildWatchTower("River Watch Tower", new Vector3(74f, 0f, 72f), 94f);

            for (int i = 0; i < 4; i++)
            {
                Vector3 homePosition = new Vector3(86f + i * 11f, 2.25f, 54f + (i % 2) * 16f);
                CreateBuilding($"Milestone9 Riverside Home {i + 1}", homePosition, new Vector3(8.5f, 4.5f, 8.2f));
            }

            CreateFenceLine("Milestone9 River Village Fence", new Vector3(106f, 0.75f, 84f), 58f, true);
            CreateStreetProps("Milestone9 River Village", new Vector3(104f, 0f, 62f));
        }

        private void BuildMilestone10VisualFoundation()
        {
            RegisterNamedLocation("Aurora City", new Vector3(-56f, 0f, -206f));
            RegisterNamedLocation("Stonebend Hills", new Vector3(-134f, 0f, -210f));
            RegisterNamedLocation("Riverwood Forest", new Vector3(112f, 0f, -124f));

            BuildM10RoadNetwork(new Vector3(-56f, 0f, -206f));
            BuildM10ModularCity(new Vector3(-56f, 0f, -206f));
            BuildM10TerrainPass();
            BuildM10VegetationPass();
        }

        private void BuildM10RoadNetwork(Vector3 origin)
        {
            CreateRoadModule("M10 Aurora Main Avenue", origin + new Vector3(0f, 0f, 0f), 132f, false, 15f);
            CreateRoadModule("M10 Aurora Cross Street", origin + new Vector3(0f, 0f, 0f), 112f, true, 13f);
            CreateRoadModule("M10 Aurora North Connector", origin + new Vector3(0f, 0f, 55f), 106f, false, 12f);
            CreateRoadModule("M10 Aurora Riverside Link", new Vector3(18f, 0f, -146f), 124f, true, 12f);
            CreateIntersection("M10 Main Intersection", origin, new Vector2(26f, 24f));
            CreateIntersection("M10 North Intersection", origin + new Vector3(0f, 0f, 55f), new Vector2(24f, 22f));
        }

        private void BuildM10ModularCity(Vector3 origin)
        {
            int index = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    float x = -48f + column * 32f;
                    float z = row == 0 ? -28f : 32f;
                    int floors = 2 + (row + column) % 4;
                    Vector3 size = new Vector3(15f + (column % 2) * 4f, floors * 3.2f, 13f + (row % 2) * 4f);
                    CreateModularBuildingAsset($"M10 City Block A{index + 1}", origin + new Vector3(x, 0f, z), size, floors, (index % 3) * 0.08f);
                    index++;
                }
            }

            CreateModularBuildingAsset("M10 City Civic Hall", origin + new Vector3(-66f, 0f, 0f), new Vector3(22f, 13f, 18f), 4, 0.18f);
            CreateModularBuildingAsset("M10 City Corner Market", origin + new Vector3(62f, 0f, -4f), new Vector3(18f, 9.2f, 16f), 3, 0.05f);
            CreateCityPropSet("M10 Aurora City", origin);
        }

        private void BuildM10TerrainPass()
        {
            CreateTerrainHillAsset("M10 Terrain Hill 1", new Vector3(-138f, 0f, -212f), new Vector3(42f, 8f, 28f), -11f);
            CreateTerrainHillAsset("M10 Terrain Hill 2", new Vector3(-178f, 0f, -184f), new Vector3(34f, 6.2f, 22f), 18f);
            CreateTerrainHillAsset("M10 River Bluff East", new Vector3(94f, 0f, -48f), new Vector3(28f, 5.4f, 36f), 8f);
            CreateTerrainHillAsset("M10 River Bluff West", new Vector3(30f, 0f, -34f), new Vector3(24f, 4.7f, 34f), -16f);

            for (int i = 0; i < 5; i++)
            {
                Vector3 terracePosition = new Vector3(-132f + i * 14f, 0.11f, -224f + (i % 2) * 8f);
                GameObject terrace = CreateWorldBlock($"M10 Hill Terrace {i + 1}", terracePosition, new Vector3(24f - i * 1.8f, 0.22f, 8f), hillMaterial);
                terrace.transform.rotation = Quaternion.Euler(0f, -10f + i * 4f, 0f);
                TagVisualModule(terrace, RuntimeVisualModuleKind.Terrain, $"M10_HillTerrace_{i + 1}");
            }
        }

        private void BuildM10VegetationPass()
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 origin = new Vector3(92f + (i % 3) * 18f, 0f, -154f + (i / 3) * 24f);
                CreateVegetationCluster($"M10 Forest Cluster {i + 1}", origin, 0.86f + (i % 3) * 0.1f);
            }

            for (int i = 0; i < 8; i++)
            {
                Vector3 planter = new Vector3(-112f + i * 15f, 0.22f, -194f + (i % 2) * 24f);
                GameObject box = CreateWorldBlock($"M10 City Planter {i + 1}", planter, new Vector3(7.2f, 0.44f, 1.8f), fenceMaterial);
                TagVisualModule(box, RuntimeVisualModuleKind.Vegetation, $"M10_CityPlanter_{i + 1}");
                CreateBush($"{box.name} Shrub", planter + Vector3.up * 0.36f, 0.82f);
            }
        }

        private void BuildMilestone11OriginalWorldFoundation()
        {
            RegisterNamedLocation("Central City", new Vector3(-8f, 0f, -28f));
            RegisterNamedLocation("Small Village", new Vector3(-214f, 0f, 72f));
            RegisterNamedLocation("Industrial Factory", new Vector3(214f, 0f, -72f));
            RegisterNamedLocation("Military Base", new Vector3(210f, 0f, -214f));
            RegisterNamedLocation("Forest", new Vector3(-214f, 0f, -162f));
            RegisterNamedLocation("Mountain Area", new Vector3(-96f, 0f, 226f));
            RegisterNamedLocation("River", new Vector3(82f, 0f, 4f));
            RegisterNamedLocation("Bridge", new Vector3(82f, 0f, 34f));
            RegisterNamedLocation("Gas Station", new Vector3(-18f, 0f, 178f));
            RegisterNamedLocation("Warehouse Area", new Vector3(116f, 0f, 122f));
            RegisterNamedLocation("Farm", new Vector3(-222f, 0f, 164f));
            RegisterNamedLocation("Watch Towers", new Vector3(178f, 0f, 142f));

            BuildM11TerrainFoundation();
            BuildM11RoadNetwork();
            BuildM11CentralCity(new Vector3(-8f, 0f, -28f));
            BuildM11SmallVillageAndFarm(new Vector3(-214f, 0f, 72f), new Vector3(-222f, 0f, 164f));
            BuildM11IndustrialFactory(new Vector3(214f, 0f, -72f));
            BuildM11MilitaryBase(new Vector3(210f, 0f, -214f));
            BuildM11WarehouseArea(new Vector3(116f, 0f, 122f));
            BuildM11GasStation(new Vector3(-18f, 0f, 178f));
            BuildM11ForestMountainRiver(new Vector3(-214f, 0f, -162f), new Vector3(-96f, 0f, 226f), new Vector3(82f, 0f, 4f));
            BuildM11WatchTowers();
            BuildM11LootPropDisplays();
            BuildM11Atmosphere();
        }

        private void BuildM11TerrainFoundation()
        {
            GameObject grassland = CreateWorldBlock("M11 Grassland Terrain Patch", new Vector3(0f, -0.025f, 0f), new Vector3(430f, 0.05f, 430f), grassMaterial);
            TagVisualModule(grassland, RuntimeVisualModuleKind.Terrain, "M11_Grassland_Base");

            CreateTerrainHillAsset("M11 Rolling Hill North", new Vector3(-118f, 0f, 158f), new Vector3(54f, 7.8f, 34f), -18f);
            CreateTerrainHillAsset("M11 Rolling Hill East", new Vector3(178f, 0f, 74f), new Vector3(42f, 6.4f, 32f), 24f);
            CreateTerrainHillAsset("M11 Mountain Ridge A", new Vector3(-112f, 0f, 236f), new Vector3(62f, 13f, 38f), 8f);
            CreateTerrainHillAsset("M11 Mountain Ridge B", new Vector3(-58f, 0f, 226f), new Vector3(50f, 10f, 32f), -16f);
            CreateM11Slope("M11 South Grass Slope", new Vector3(38f, 1.2f, -214f), new Vector3(92f, 2.4f, 28f), 9f);
            CreateM11Slope("M11 Factory Service Slope", new Vector3(164f, 0.95f, -116f), new Vector3(64f, 1.9f, 18f), -14f);

            CreateM11Lake("M11 Mountain Lake", new Vector3(-116f, 0f, 204f), new Vector3(46f, 0.05f, 24f), 12f);
            CreateWorldBlock("M11 River Beach North", new Vector3(72f, 0.05f, 72f), new Vector3(32f, 0.08f, 16f), beachMaterial).transform.rotation = Quaternion.Euler(0f, 8f, 0f);
            CreateWorldBlock("M11 River Beach South", new Vector3(90f, 0.05f, -86f), new Vector3(36f, 0.08f, 18f), beachMaterial).transform.rotation = Quaternion.Euler(0f, -12f, 0f);
            CreateWorldBlock("M11 Cliff Face North", new Vector3(-76f, 3.1f, 252f), new Vector3(86f, 6.2f, 6f), cliffMaterial).transform.rotation = Quaternion.Euler(0f, -7f, 0f);
            CreateWorldBlock("M11 Cliff Face West", new Vector3(-154f, 2.8f, 210f), new Vector3(8f, 5.6f, 64f), cliffMaterial).transform.rotation = Quaternion.Euler(0f, 12f, 0f);

            for (int i = 0; i < 10; i++)
            {
                Vector3 patch = new Vector3(-184f + (i % 5) * 34f, 0.11f, 122f + (i / 5) * 42f);
                CreateM11GrassPatch($"M11 Tall Grass Patch {i + 1}", patch, 8f + (i % 3) * 2f);
            }
        }

        private void BuildM11RoadNetwork()
        {
            CreateRoadModule("M11 Central City Boulevard", new Vector3(-8f, 0f, -28f), 168f, false, 16f);
            CreateRoadModule("M11 Central City North Road", new Vector3(-8f, 0f, 62f), 184f, true, 13f);
            CreateIntersection("M11 Central City Main Intersection", new Vector3(-8f, 0f, -28f), new Vector2(30f, 28f));
            CreateIntersection("M11 Gas Station Intersection", new Vector3(-8f, 0f, 154f), new Vector2(24f, 22f));

            CreateAngledRoadModule("M11 Village Connector Road", new Vector3(-116f, 0f, 36f), new Vector3(188f, 0.05f, 10f), -13f, roadMaterial);
            CreateAngledRoadModule("M11 Farm Dirt Road", new Vector3(-178f, 0f, 142f), new Vector3(116f, 0.05f, 7.5f), -8f, dirtRoadMaterial);
            CreateAngledRoadModule("M11 Factory Connector Road", new Vector3(116f, 0f, -52f), new Vector3(220f, 0.05f, 12f), -9f, roadMaterial);
            CreateAngledRoadModule("M11 Military Base Road", new Vector3(202f, 0f, -146f), new Vector3(154f, 0.05f, 11f), 86f, roadMaterial);
            CreateAngledRoadModule("M11 Forest Dirt Road", new Vector3(-156f, 0f, -116f), new Vector3(132f, 0.05f, 7f), -30f, dirtRoadMaterial);
            CreateAngledRoadModule("M11 Mountain Access Road", new Vector3(-82f, 0f, 164f), new Vector3(134f, 0.05f, 8.5f), 20f, dirtRoadMaterial);
            CreateAngledRoadModule("M11 Warehouse Spur Road", new Vector3(74f, 0f, 98f), new Vector3(96f, 0.05f, 10f), 18f, roadMaterial);
            CreateAngledRoadModule("M11 Bridge Approach Road", new Vector3(82f, 0f, 26f), new Vector3(88f, 0.05f, 9f), 90f, roadMaterial);
        }

        private void BuildM11CentralCity(Vector3 origin)
        {
            CreateModularBuildingAsset("M11 Central City Apartment A", origin + new Vector3(-44f, 0f, -28f), new Vector3(20f, 16f, 17f), 5, 0.08f);
            CreateModularBuildingAsset("M11 Central City Apartment B", origin + new Vector3(38f, 0f, -30f), new Vector3(18f, 14f, 18f), 4, 0.18f);
            CreateModularBuildingAsset("M11 Central City Shop Row", origin + new Vector3(-44f, 0f, 26f), new Vector3(24f, 9.5f, 15f), 3, 0.12f);
            CreateModularBuildingAsset("M11 Central City Office", origin + new Vector3(34f, 0f, 24f), new Vector3(22f, 18f, 16f), 5, 0.22f);
            CreateModularBuildingAsset("M11 Central City Transit Shop", origin + new Vector3(0f, 0f, 52f), new Vector3(18f, 8.4f, 13f), 2, 0.04f);
            CreateCityPropSet("M11 Central City", origin);

            for (int i = 0; i < 6; i++)
            {
                Vector3 planter = origin + new Vector3(-62f + i * 24f, 0.25f, -3f + (i % 2) * 24f);
                GameObject prop = CreateWorldBlock($"M11 Central City Sidewalk Planter {i + 1}", planter, new Vector3(6.2f, 0.5f, 1.8f), fenceMaterial);
                TagVisualModule(prop, RuntimeVisualModuleKind.Prop, $"M11_Central_Planter_{i + 1}");
                CreateBush($"{prop.name} Bush", planter + Vector3.up * 0.42f, 0.72f);
            }
        }

        private void BuildM11SmallVillageAndFarm(Vector3 villageOrigin, Vector3 farmOrigin)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 home = villageOrigin + new Vector3((i % 3) * 16f - 16f, 0f, (i / 3) * 18f - 8f);
                CreateModularBuildingAsset($"M11 Small Village House {i + 1}", home, new Vector3(10f, 6.2f, 9f), 2, (i % 3) * 0.06f);
            }

            CreateModularBuildingAsset("M11 Small Village Shop", villageOrigin + new Vector3(18f, 0f, 26f), new Vector3(13f, 6.6f, 10f), 2, 0.16f);
            CreateFenceLine("M11 Small Village Fence", villageOrigin + new Vector3(2f, 0.72f, 38f), 74f, true);
            CreateM11LootProp("M11 Village Medical Kit Prop", villageOrigin + new Vector3(6f, 0.45f, 20f), LootKind.Medkit);

            CreateBuilding("M11 Farm Barn", farmOrigin + new Vector3(-12f, 3.2f, 0f), new Vector3(20f, 6.4f, 14f));
            CreateModularBuildingAsset("M11 Farm House", farmOrigin + new Vector3(22f, 0f, -8f), new Vector3(13f, 6.8f, 11f), 2, 0.1f);
            CreateFenceLine("M11 Farm Fence North", farmOrigin + new Vector3(0f, 0.75f, 32f), 92f, true);
            CreateFenceLine("M11 Farm Fence West", farmOrigin + new Vector3(-48f, 0.75f, 0f), 62f, false);

            for (int i = 0; i < 8; i++)
            {
                Vector3 row = farmOrigin + new Vector3(-42f + i * 11f, 0.18f, 24f);
                GameObject crop = CreateWorldBlock($"M11 Farm Crop Row {i + 1}", row, new Vector3(7.5f, 0.36f, 32f), cropMaterial);
                TagVisualModule(crop, RuntimeVisualModuleKind.Vegetation, $"M11_Farm_Crop_{i + 1}");
            }
        }

        private void BuildM11IndustrialFactory(Vector3 origin)
        {
            CreateModularBuildingAsset("M11 Industrial Factory Main Hall", origin, new Vector3(38f, 12f, 26f), 3, 0.2f);
            CreateModularBuildingAsset("M11 Industrial Factory Office", origin + new Vector3(-36f, 0f, 18f), new Vector3(17f, 8.6f, 13f), 3, 0.08f);
            CreateWorldBlock("M11 Industrial Factory Loading Dock", origin + new Vector3(28f, 0.7f, 24f), new Vector3(28f, 1.4f, 7f), coverMaterial);
            CreateWorldBlock("M11 Industrial Factory Smokestack A", origin + new Vector3(24f, 8f, -14f), new Vector3(3.2f, 16f, 3.2f), vehicleAccentMaterial);
            CreateWorldBlock("M11 Industrial Factory Smokestack B", origin + new Vector3(32f, 6.4f, 4f), new Vector3(2.4f, 12.8f, 2.4f), vehicleAccentMaterial);
            CreateFenceLine("M11 Industrial Factory Fence North", origin + new Vector3(0f, 0.72f, 42f), 96f, true);
            CreateFenceLine("M11 Industrial Factory Fence East", origin + new Vector3(50f, 0.72f, 0f), 82f, false);

            for (int i = 0; i < 10; i++)
            {
                Vector3 stack = origin + new Vector3(-32f + (i % 5) * 14f, 1.1f + (i % 2) * 0.45f, 44f + (i / 5) * 11f);
                CreateWorldBlock($"M11 Factory Cargo Stack {i + 1}", stack, new Vector3(7.5f, 2.2f + (i % 2), 5.6f), coverMaterial);
            }

            CreateM11LootProp("M11 Factory Weapon Crate Prop", origin + new Vector3(18f, 0.55f, 35f), LootKind.AssaultRifle);
        }

        private void BuildM11MilitaryBase(Vector3 origin)
        {
            CreateModularBuildingAsset("M11 Military Barracks A", origin + new Vector3(-22f, 0f, 0f), new Vector3(24f, 7.8f, 12f), 2, 0.08f);
            CreateModularBuildingAsset("M11 Military Barracks B", origin + new Vector3(22f, 0f, -4f), new Vector3(24f, 7.8f, 12f), 2, 0.12f);
            CreateModularBuildingAsset("M11 Military Base Office", origin + new Vector3(0f, 0f, 28f), new Vector3(18f, 9.2f, 14f), 3, 0.18f);
            CreateWorldBlock("M11 Military Motor Pool Cover", origin + new Vector3(44f, 1.1f, 18f), new Vector3(24f, 2.2f, 11f), warehouseMaterial);
            CreateFenceLine("M11 Military Base North Fence", origin + new Vector3(0f, 0.75f, 54f), 112f, true);
            CreateFenceLine("M11 Military Base South Fence", origin + new Vector3(0f, 0.75f, -42f), 112f, true);
            CreateFenceLine("M11 Military Base East Fence", origin + new Vector3(58f, 0.75f, 5f), 96f, false);
            CreateFenceLine("M11 Military Base West Fence", origin + new Vector3(-58f, 0.75f, 5f), 96f, false);
            BuildWatchTower("M11 Military Watch Tower East", origin + new Vector3(54f, 0f, 48f), -35f);
            BuildWatchTower("M11 Military Watch Tower West", origin + new Vector3(-54f, 0f, 48f), 35f);
            CreateM11LootProp("M11 Military Armor Prop", origin + new Vector3(2f, 0.55f, 42f), LootKind.ArmorVest);
        }

        private void BuildM11WarehouseArea(Vector3 origin)
        {
            CreateModularBuildingAsset("M11 Warehouse Area Warehouse A", origin + new Vector3(-24f, 0f, 0f), new Vector3(28f, 9.8f, 20f), 2, 0.12f);
            CreateModularBuildingAsset("M11 Warehouse Area Warehouse B", origin + new Vector3(24f, 0f, 18f), new Vector3(24f, 8.8f, 18f), 2, 0.18f);
            CreateWorldBlock("M11 Warehouse Yard Loading Ramp", origin + new Vector3(0f, 0.7f, -22f), new Vector3(44f, 1.4f, 8f), roadMaterial);
            CreateFenceLine("M11 Warehouse Area Fence", origin + new Vector3(0f, 0.7f, 44f), 104f, true);

            for (int i = 0; i < 12; i++)
            {
                Vector3 crate = origin + new Vector3(-38f + (i % 6) * 15f, 1.1f, -30f + (i / 6) * 12f);
                CreateWorldBlock($"M11 Warehouse Crate Stack {i + 1}", crate, new Vector3(8f, 2.2f + (i % 3) * 0.5f, 6f), coverMaterial);
            }
        }

        private void BuildM11GasStation(Vector3 origin)
        {
            CreateFuelStop("M11 Gas Station", origin);
            CreateRoadSign("M11 Gas Station Sign", origin + new Vector3(-12f, 1.6f, -14f), "GAS");
            CreateM11LootProp("M11 Gas Station Fuel Can Prop", origin + new Vector3(6f, 0.45f, -8f), LootKind.ArmorPlate);
            CreateWorldBlock("M11 Gas Station Car Wash Wall", origin + new Vector3(15f, 1.7f, 9f), new Vector3(8f, 3.4f, 0.6f), buildingMaterial);
        }

        private void BuildM11ForestMountainRiver(Vector3 forestOrigin, Vector3 mountainOrigin, Vector3 riverOrigin)
        {
            for (int i = 0; i < 9; i++)
            {
                Vector3 cluster = forestOrigin + new Vector3((i % 3) * 24f - 24f, 0f, (i / 3) * 24f - 24f);
                CreateVegetationCluster($"M11 Forest Cluster {i + 1}", cluster, 0.94f + (i % 3) * 0.08f);
            }

            CreateFenceLine("M11 Forest Fence", forestOrigin + new Vector3(4f, 0.72f, 46f), 86f, true);
            for (int i = 0; i < 10; i++)
            {
                CreateM11Rock($"M11 Forest Rock {i + 1}", forestOrigin + Quaternion.Euler(0f, i * 36f, 0f) * new Vector3(22f + (i % 4) * 5f, 0.45f, 0f), 1f + (i % 3) * 0.14f);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector3 segmentPosition = riverOrigin + new Vector3(Mathf.Sin(i * 0.75f) * 10f, 0.04f, -118f + i * 38f);
                GameObject segment = CreateWorldBlock($"M11 River Segment {i + 1}", segmentPosition, new Vector3(22f + (i % 2) * 4f, 0.05f, 42f), lakeMaterial);
                segment.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i * 0.5f) * 12f, 0f);
                TagVisualModule(segment, RuntimeVisualModuleKind.Terrain, $"M11_River_{i + 1}");
            }

            CreateBridge("M11 River Bridge", riverOrigin + new Vector3(0f, 0f, 34f));
            CreateM11FlowerPatch("M11 Wildflower Patch 1", mountainOrigin + new Vector3(28f, 0.18f, -12f));
            CreateM11FlowerPatch("M11 Wildflower Patch 2", forestOrigin + new Vector3(-18f, 0.18f, 36f));
        }

        private void BuildM11WatchTowers()
        {
            BuildWatchTower("M11 Watch Tower Alpha", new Vector3(178f, 0f, 142f), 18f);
            BuildWatchTower("M11 Watch Tower Bravo", new Vector3(142f, 0f, 168f), -22f);
            BuildWatchTower("M11 Watch Tower Charlie", new Vector3(224f, 0f, -168f), 44f);
        }

        private void BuildM11LootPropDisplays()
        {
            Vector3 origin = new Vector3(-22f, 0f, 8f);
            CreateM11LootProp("M11 Ammo Box Prop", origin + new Vector3(-18f, 0.5f, 0f), LootKind.MediumAmmo);
            CreateM11LootProp("M11 Medical Kit Prop", origin + new Vector3(-12f, 0.5f, 0f), LootKind.Medkit);
            CreateM11LootProp("M11 Backpack Prop", origin + new Vector3(-6f, 0.5f, 0f), LootKind.Bandage);
            CreateM11LootProp("M11 Helmet Prop", origin + new Vector3(0f, 0.5f, 0f), LootKind.Helmet);
            CreateM11LootProp("M11 Armor Prop", origin + new Vector3(6f, 0.5f, 0f), LootKind.ArmorVest);
            CreateM11LootProp("M11 Weapon Crate Prop", origin + new Vector3(12f, 0.5f, 0f), LootKind.AssaultRifle);
            CreateM11FuelCanProp("M11 Fuel Can Prop", origin + new Vector3(18f, 0.5f, 0f));
        }

        private void BuildM11Atmosphere()
        {
            CreateM11Cloud("M11 Cloud Bank North", new Vector3(-80f, 58f, 188f), new Vector3(48f, 7f, 16f));
            CreateM11Cloud("M11 Cloud Bank Central", new Vector3(64f, 54f, 46f), new Vector3(62f, 8f, 18f));
            CreateM11Cloud("M11 Cloud Bank Coast", new Vector3(28f, 52f, 218f), new Vector3(42f, 6f, 15f));
        }

        private void BuildMilestone12ProfessionalUpgrade()
        {
            RegisterNamedLocation("M12 Tactical City Core", new Vector3(-8f, 0f, -28f));
            BuildM12TerrainMaterialPass();
            BuildM12RiverBridgePolish();
            BuildM12RoadAndPropPolish();
            BuildM12BuildingDetailPass();
            BuildM12LightingAtmospherePass();
        }

        private void BuildM12TerrainMaterialPass()
        {
            GameObject grassBlend = CreateWorldBlock("M12 Grass Terrain Material Blend", new Vector3(-18f, 0.015f, -6f), new Vector3(96f, 0.035f, 86f), grassMaterial);
            TagVisualModule(grassBlend, RuntimeVisualModuleKind.Terrain, "M12_Grass_Material");
            GameObject dirtBlend = CreateWorldBlock("M12 Dirt Terrain Material Blend", new Vector3(-178f, 0.03f, 142f), new Vector3(128f, 0.035f, 42f), dirtRoadMaterial);
            dirtBlend.transform.rotation = Quaternion.Euler(0f, -8f, 0f);
            TagVisualModule(dirtBlend, RuntimeVisualModuleKind.Terrain, "M12_Dirt_Material");
            GameObject rockBlend = CreateWorldBlock("M12 Rock Terrain Material Blend", new Vector3(-112f, 0.04f, 236f), new Vector3(108f, 0.04f, 58f), cliffMaterial);
            rockBlend.transform.rotation = Quaternion.Euler(0f, 8f, 0f);
            TagVisualModule(rockBlend, RuntimeVisualModuleKind.Terrain, "M12_Rock_Material");
            GameObject sandBlend = CreateWorldBlock("M12 Sand Terrain Material Blend", new Vector3(86f, 0.035f, -86f), new Vector3(46f, 0.035f, 24f), beachMaterial);
            sandBlend.transform.rotation = Quaternion.Euler(0f, -12f, 0f);
            TagVisualModule(sandBlend, RuntimeVisualModuleKind.Terrain, "M12_Sand_Material");

            for (int i = 0; i < 12; i++)
            {
                Vector3 tuft = new Vector3(-56f + (i % 6) * 18f, 0.17f, -62f + (i / 6) * 28f);
                CreateM11GrassPatch($"M12 Grass Blade Cluster {i + 1}", tuft, 5.8f + (i % 3));
            }
        }

        private void BuildM12RiverBridgePolish()
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 foamPosition = new Vector3(82f + Mathf.Sin(i * 0.8f) * 7f, 0.11f, -88f + i * 35f);
                GameObject foam = CreateWorldBlock($"M12 River Foam Strip {i + 1}", foamPosition, new Vector3(11f, 0.035f, 1.1f), laneMarkMaterial);
                foam.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i) * 18f, 0f);
                TagVisualModule(foam, RuntimeVisualModuleKind.Terrain, $"M12_RiverFoam_{i + 1}");
            }

            CreateWorldBlock("M12 Bridge Cable Rail North", new Vector3(82f, 2.45f, 38.8f), new Vector3(40f, 0.16f, 0.18f), vehicleAccentMaterial);
            CreateWorldBlock("M12 Bridge Cable Rail South", new Vector3(82f, 2.45f, 29.2f), new Vector3(40f, 0.16f, 0.18f), vehicleAccentMaterial);
            for (int i = 0; i < 6; i++)
            {
                float x = 64f + i * 7.2f;
                CreateWorldBlock($"M12 Bridge Vertical Cable {i + 1}", new Vector3(x, 1.95f, 38.8f), new Vector3(0.14f, 1.1f, 0.14f), vehicleAccentMaterial);
                CreateWorldBlock($"M12 Bridge South Cable {i + 1}", new Vector3(x, 1.95f, 29.2f), new Vector3(0.14f, 1.1f, 0.14f), vehicleAccentMaterial);
            }
        }

        private void BuildM12RoadAndPropPolish()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 conePosition = new Vector3(-72f + i * 18f, 0.45f, -20f + (i % 2) * 8f);
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                runtimeObjects.Add(cone);
                cone.name = $"M12 Road Safety Cone {i + 1}";
                cone.layer = groundLayer;
                cone.transform.position = conePosition;
                cone.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                cone.GetComponent<Renderer>().material = fuelCanMaterial;
                TagVisualModule(cone, RuntimeVisualModuleKind.Prop, $"M12_RoadCone_{i + 1}");
            }

            for (int i = 0; i < 6; i++)
            {
                Vector3 cratePosition = new Vector3(86f + (i % 3) * 9f, 0.85f, 106f + (i / 3) * 10f);
                CreateWorldBlock($"M12 Roadside Supply Crate {i + 1}", cratePosition, new Vector3(4.6f, 1.7f, 3.8f), coverMaterial);
            }

            CreatePowerPole("M12 City Utility Pole A", new Vector3(-42f, 0f, 74f));
            CreatePowerPole("M12 City Utility Pole B", new Vector3(28f, 0f, 88f));
            CreateRoadSign("M12 Tactical City Sign", new Vector3(-72f, 1.5f, -10f), "CITY");
            CreateRoadSign("M12 Bridge Route Sign", new Vector3(56f, 1.5f, 18f), "BRIDGE");
        }

        private void BuildM12BuildingDetailPass()
        {
            CreateWorldBlock("M12 Central City Interior Counter", new Vector3(-48f, 0.72f, 2f), new Vector3(7.2f, 1.44f, 1.5f), coverMaterial);
            CreateWorldBlock("M12 Central City Roof Antenna", new Vector3(26f, 14.9f, -7f), new Vector3(0.28f, 4.2f, 0.28f), vehicleAccentMaterial);
            CreateWorldBlock("M12 Apartment Rooftop HVAC", new Vector3(-52f, 16.8f, -55f), new Vector3(4.8f, 1.0f, 3.4f), vehicleAccentMaterial);
            CreateWorldBlock("M12 Shop Glass Door", new Vector3(-52f, 1.36f, -0.5f), new Vector3(2.1f, 2.7f, 0.12f), windowMaterial);
            CreateWorldBlock("M12 Barracks Interior Locker Row", new Vector3(198f, 1.08f, -208f), new Vector3(8.4f, 2.16f, 1.1f), armorDisplayMaterial);
            CreateWorldBlock("M12 Factory Control Desk", new Vector3(186f, 0.7f, -49f), new Vector3(6f, 1.4f, 2f), vehicleAccentMaterial);
        }

        private void BuildM12LightingAtmospherePass()
        {
            CreateM11Cloud("M12 Layered Sky Cloud East", new Vector3(136f, 62f, -28f), new Vector3(58f, 6f, 18f));
            CreateM11Cloud("M12 Layered Sky Cloud West", new Vector3(-162f, 56f, -88f), new Vector3(46f, 5f, 16f));
            GameObject marker = new GameObject("M12 Professional Visual Upgrade Marker");
            runtimeObjects.Add(marker);
            marker.transform.position = new Vector3(-8f, 1f, -28f);
            TagVisualModule(marker, RuntimeVisualModuleKind.Lighting, "M12_Professional_Visual_Upgrade");
        }

        private void BuildVisualSliceArtPipelineArea()
        {
            Vector3 origin = new Vector3(-246f, 0f, 214f);
            RegisterNamedLocation("Visual Slice Block", origin);

            GameObject root = new GameObject("VS_StreetBlock_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Prop, "VisualSlice_StreetBlock");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "ENV_StreetBlock_VS",
                BattleZoneArtAssetCategory.Terrain,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("VS_Street_Asphalt", root.transform, origin + new Vector3(0f, 0.045f, 0f), new Vector3(68f, 0.09f, 16f), roadMaterial, RuntimeVisualModuleKind.Road, "VS_Road_Asphalt", true, renderers);
            CreateVisualSliceBlock("VS_Street_NorthSidewalk", root.transform, origin + new Vector3(0f, 0.16f, 11.5f), new Vector3(68f, 0.28f, 5.4f), sidewalkMaterial, RuntimeVisualModuleKind.Road, "VS_Sidewalk_North", true, renderers);
            CreateVisualSliceBlock("VS_Street_SouthSidewalk", root.transform, origin + new Vector3(0f, 0.16f, -11.5f), new Vector3(68f, 0.28f, 5.4f), sidewalkMaterial, RuntimeVisualModuleKind.Road, "VS_Sidewalk_South", true, renderers);
            CreateVisualSliceBlock("VS_Street_CenterMark_A", root.transform, origin + new Vector3(-15f, 0.12f, 0f), new Vector3(9f, 0.04f, 0.32f), laneMarkMaterial, RuntimeVisualModuleKind.Road, "VS_Road_Mark_A", false, renderers);
            CreateVisualSliceBlock("VS_Street_CenterMark_B", root.transform, origin + new Vector3(15f, 0.12f, 0f), new Vector3(9f, 0.04f, 0.32f), laneMarkMaterial, RuntimeVisualModuleKind.Road, "VS_Road_Mark_B", false, renderers);
            CreateVisualSliceBlock("VS_Street_Curb_North", root.transform, origin + new Vector3(0f, 0.36f, 8.4f), new Vector3(68f, 0.28f, 0.44f), coverMaterial, RuntimeVisualModuleKind.Road, "VS_Curb_North", true, renderers);
            CreateVisualSliceBlock("VS_Street_Curb_South", root.transform, origin + new Vector3(0f, 0.36f, -8.4f), new Vector3(68f, 0.28f, 0.44f), coverMaterial, RuntimeVisualModuleKind.Road, "VS_Curb_South", true, renderers);
            CreateVisualSliceBlock("VS_Street_GrassInset_A", root.transform, origin + new Vector3(-26f, 0.13f, 16.2f), new Vector3(12f, 0.12f, 4.8f), grassMaterial, RuntimeVisualModuleKind.Terrain, "VS_Grass_Inset_A", false, renderers);
            CreateVisualSliceBlock("VS_Street_GrassInset_B", root.transform, origin + new Vector3(24f, 0.13f, -16.2f), new Vector3(12f, 0.12f, 4.8f), grassMaterial, RuntimeVisualModuleKind.Terrain, "VS_Grass_Inset_B", false, renderers);

            CreateVisualSliceEnterableHouse(origin + new Vector3(-25f, 0f, 26f));
            CreateVisualSliceWarehouse(origin + new Vector3(25f, 0f, -27f));

            for (int i = 0; i < 4; i++)
            {
                float x = -30f + i * 20f;
                CreateVisualSliceTree($"VS_StreetTree_{i + 1}", origin + new Vector3(x, 0f, i % 2 == 0 ? 16.5f : -16.5f));
            }

            for (int i = 0; i < 5; i++)
            {
                CreateVisualSliceRock($"VS_RoadsideRock_{i + 1}", origin + new Vector3(-26f + i * 13f, 0.45f, i % 2 == 0 ? -18.5f : 18.5f), 0.75f + i * 0.08f);
            }

            CreateVisualSliceBlock("VS_ModularBench_A", root.transform, origin + new Vector3(-6f, 0.58f, 14.8f), new Vector3(5.2f, 0.42f, 1.1f), fenceMaterial, RuntimeVisualModuleKind.Prop, "VS_Bench_A", true, renderers);
            CreateVisualSliceBlock("VS_ModularPlanter_A", root.transform, origin + new Vector3(8f, 0.46f, -14.9f), new Vector3(5.8f, 0.92f, 1.8f), fenceMaterial, RuntimeVisualModuleKind.Prop, "VS_Planter_A", true, renderers);
            CreateVisualSliceBlock("VS_LootDisplay_Crate", root.transform, origin + new Vector3(15f, 0.55f, 14.8f), new Vector3(2.2f, 1.1f, 1.6f), lootCrateMaterial, RuntimeVisualModuleKind.Loot, "VS_Loot_Crate", true, renderers);
            CreateVisualSliceCharacterShowcase(origin + new Vector3(0f, 0f, 24.5f));
            CreateVisualSliceWeaponShowcase("VS_Rook17_AssaultRifle_Showcase", "WPN_Rook17_AR_VS", origin + new Vector3(-9f, 1.25f, -18f), true);
            CreateVisualSliceWeaponShowcase("VS_Sable9_Pistol_Showcase", "WPN_Sable9_Pistol_VS", origin + new Vector3(9f, 1.25f, -18f), false);
            CreateReflectionProbe("VS_StreetBlock_ReflectionProbe", origin + new Vector3(0f, 6f, 0f), new Vector3(86f, 18f, 62f), 0.55f);
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateVisualSliceEnterableHouse(Vector3 origin)
        {
            GameObject root = new GameObject("VS_Enterable_House_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "VS_Enterable_House");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_EnterableHouse_VS",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("VS_House_Floor", root.transform, origin + new Vector3(0f, 0.12f, 0f), new Vector3(14f, 0.24f, 12f), roadMaterial, RuntimeVisualModuleKind.Building, "VS_House_Floor", true, renderers);
            CreateVisualSliceBlock("VS_House_BackWall", root.transform, origin + new Vector3(0f, 2.2f, 6f), new Vector3(14f, 4.4f, 0.42f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_House_BackWall", true, renderers);
            CreateVisualSliceBlock("VS_House_LeftWall", root.transform, origin + new Vector3(-7f, 2.2f, 0f), new Vector3(0.42f, 4.4f, 12f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_House_LeftWall", true, renderers);
            CreateVisualSliceBlock("VS_House_RightWall", root.transform, origin + new Vector3(7f, 2.2f, 0f), new Vector3(0.42f, 4.4f, 12f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_House_RightWall", true, renderers);
            CreateVisualSliceBlock("VS_House_FrontLeftWall", root.transform, origin + new Vector3(-4.9f, 2.2f, -6f), new Vector3(4.2f, 4.4f, 0.42f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_House_FrontLeft", true, renderers);
            CreateVisualSliceBlock("VS_House_FrontRightWall", root.transform, origin + new Vector3(4.9f, 2.2f, -6f), new Vector3(4.2f, 4.4f, 0.42f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_House_FrontRight", true, renderers);
            CreateVisualSliceBlock("VS_House_DoorHeader", root.transform, origin + new Vector3(0f, 3.8f, -6f), new Vector3(5.6f, 1.2f, 0.42f), facadeAccentMaterial, RuntimeVisualModuleKind.Building, "VS_House_DoorHeader", true, renderers);
            CreateVisualSliceBlock("VS_House_RoofSlab", root.transform, origin + new Vector3(0f, 4.68f, 0f), new Vector3(15.5f, 0.55f, 13.5f), roofMaterial, RuntimeVisualModuleKind.Building, "VS_House_Roof", true, renderers);
            CreateVisualSliceBlock("VS_House_Window_L", root.transform, origin + new Vector3(-3.6f, 2.35f, -6.25f), new Vector3(1.4f, 1.1f, 0.12f), windowMaterial, RuntimeVisualModuleKind.Building, "VS_House_Window_L", false, renderers);
            CreateVisualSliceBlock("VS_House_Window_R", root.transform, origin + new Vector3(3.6f, 2.35f, -6.25f), new Vector3(1.4f, 1.1f, 0.12f), windowMaterial, RuntimeVisualModuleKind.Building, "VS_House_Window_R", false, renderers);
            CreateVisualSliceBlock("VS_House_InteriorTable", root.transform, origin + new Vector3(-2.1f, 0.76f, 1.8f), new Vector3(3.8f, 0.58f, 1.4f), coverMaterial, RuntimeVisualModuleKind.Prop, "VS_House_Table", true, renderers);
            CreateVisualSliceBlock("VS_House_LootShelf", root.transform, origin + new Vector3(4.9f, 1.1f, 2f), new Vector3(1.1f, 2.2f, 4.3f), armorDisplayMaterial, RuntimeVisualModuleKind.Prop, "VS_House_Shelf", true, renderers);
            CreateVisualSliceSocket(root.transform, "Doorway", origin + new Vector3(0f, 1.1f, -6.35f));
            CreateVisualSliceSocket(root.transform, "LootAnchor", origin + new Vector3(4.1f, 0.8f, 1.8f));
            RegisterCoverPoint("VS House Interior Cover", origin + new Vector3(-2.1f, 0f, 1.8f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateVisualSliceWarehouse(Vector3 origin)
        {
            GameObject root = new GameObject("VS_Warehouse_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "VS_Warehouse");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_Warehouse_VS",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("VS_Warehouse_Floor", root.transform, origin + new Vector3(0f, 0.14f, 0f), new Vector3(24f, 0.28f, 18f), roadMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_Floor", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_BackWall", root.transform, origin + new Vector3(0f, 3.2f, 9f), new Vector3(24f, 6.4f, 0.5f), warehouseMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_BackWall", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_LeftWall", root.transform, origin + new Vector3(-12f, 3.2f, 0f), new Vector3(0.5f, 6.4f, 18f), warehouseMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_LeftWall", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_RightWall", root.transform, origin + new Vector3(12f, 3.2f, 0f), new Vector3(0.5f, 6.4f, 18f), warehouseMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_RightWall", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_FrontHeader", root.transform, origin + new Vector3(0f, 5.3f, -9f), new Vector3(24f, 2.2f, 0.5f), warehouseMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_FrontHeader", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_Roof", root.transform, origin + new Vector3(0f, 6.7f, 0f), new Vector3(25.5f, 0.56f, 19.5f), roofMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_Roof", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_OpenDoor_L", root.transform, origin + new Vector3(-5.2f, 2.05f, -9.35f), new Vector3(3.6f, 4.1f, 0.22f), vehicleAccentMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_Door_L", false, renderers);
            CreateVisualSliceBlock("VS_Warehouse_OpenDoor_R", root.transform, origin + new Vector3(5.2f, 2.05f, -9.35f), new Vector3(3.6f, 4.1f, 0.22f), vehicleAccentMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_Door_R", false, renderers);
            CreateVisualSliceBlock("VS_Warehouse_Cargo_A", root.transform, origin + new Vector3(-6f, 1.1f, 1.4f), new Vector3(4.8f, 2.2f, 3.5f), coverMaterial, RuntimeVisualModuleKind.Prop, "VS_Warehouse_Cargo_A", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_Cargo_B", root.transform, origin + new Vector3(2.2f, 0.9f, 3.8f), new Vector3(5.2f, 1.8f, 3.2f), lootCrateMaterial, RuntimeVisualModuleKind.Prop, "VS_Warehouse_Cargo_B", true, renderers);
            CreateVisualSliceBlock("VS_Warehouse_Office", root.transform, origin + new Vector3(7.9f, 1.6f, -1.7f), new Vector3(5.2f, 3.2f, 4.5f), buildingMaterial, RuntimeVisualModuleKind.Building, "VS_Warehouse_Office", true, renderers);
            CreateVisualSliceSocket(root.transform, "DoorwayA", origin + new Vector3(0f, 1.4f, -9.6f));
            CreateVisualSliceSocket(root.transform, "LootAnchor", origin + new Vector3(2.2f, 1.8f, 3.8f));
            RegisterCoverPoint("VS Warehouse Cargo Cover", origin + new Vector3(-6f, 0f, 1.4f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateVisualSliceCharacterShowcase(Vector3 origin)
        {
            GameObject root = new GameObject("VS_Tactical_Humanoid_Showcase");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Character, "VS_Tactical_Humanoid");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "CHR_TacticalOperator_VS",
                BattleZoneArtAssetCategory.Character,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("VS_Character_Boots", root.transform, origin + new Vector3(0f, 0.18f, 0f), new Vector3(0.75f, 0.36f, 0.42f), playerAccentMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Boots", false, renderers);
            CreateVisualSliceBlock("VS_Character_Legs", root.transform, origin + new Vector3(0f, 0.75f, 0f), new Vector3(0.58f, 0.98f, 0.36f), playerMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Legs", false, renderers);
            CreateVisualSliceBlock("VS_Character_Torso", root.transform, origin + new Vector3(0f, 1.55f, 0f), new Vector3(0.72f, 0.92f, 0.44f), playerMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Torso", false, renderers);
            CreateVisualSliceBlock("VS_Character_ArmorVest", root.transform, origin + new Vector3(0f, 1.56f, -0.25f), new Vector3(0.66f, 0.72f, 0.08f), playerAccentMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Armor", false, renderers);
            CreateVisualSliceBlock("VS_Character_Head", root.transform, origin + new Vector3(0f, 2.26f, -0.03f), new Vector3(0.42f, 0.42f, 0.38f), playerMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Head", false, renderers);
            CreateVisualSliceBlock("VS_Character_Helmet", root.transform, origin + new Vector3(0f, 2.48f, -0.02f), new Vector3(0.52f, 0.18f, 0.46f), playerAccentMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Helmet", false, renderers);
            CreateVisualSliceBlock("VS_Character_Arms", root.transform, origin + new Vector3(0f, 1.54f, -0.08f), new Vector3(1.28f, 0.22f, 0.26f), playerMaterial, RuntimeVisualModuleKind.Character, "VS_Character_Arms", false, renderers);
            CreateVisualSliceSocket(root.transform, "RightHand", origin + new Vector3(0.64f, 1.45f, -0.18f));
            CreateVisualSliceSocket(root.transform, "MuzzleAnchor", origin + new Vector3(0.38f, 1.45f, -0.95f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateVisualSliceWeaponShowcase(string objectName, string assetId, Vector3 origin, bool rifle)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.position = origin;
            root.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            TagVisualModule(root, RuntimeVisualModuleKind.Weapon, assetId);
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                assetId,
                BattleZoneArtAssetCategory.Weapon,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            float length = rifle ? 1.5f : 0.72f;
            float height = rifle ? 0.22f : 0.18f;
            CreateVisualSliceBlock($"{objectName}_Receiver", root.transform, origin, new Vector3(0.16f, height, length), lootWeaponMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Receiver", false, renderers);
            CreateVisualSliceBlock($"{objectName}_Barrel", root.transform, origin + new Vector3(0f, 0.02f, length * 0.55f), new Vector3(0.10f, height * 0.5f, length * 0.42f), playerAccentMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Barrel", false, renderers);
            CreateVisualSliceBlock($"{objectName}_Grip", root.transform, origin + new Vector3(0f, -0.22f, -length * 0.12f), new Vector3(0.16f, 0.42f, length * 0.13f), playerAccentMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Grip", false, renderers);
            CreateVisualSliceBlock($"{objectName}_Sight", root.transform, origin + new Vector3(0f, height * 0.72f, length * 0.08f), new Vector3(0.14f, 0.08f, length * 0.22f), windowMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Sight", false, renderers);
            if (rifle)
            {
                CreateVisualSliceBlock($"{objectName}_Stock", root.transform, origin + new Vector3(0f, 0f, -length * 0.52f), new Vector3(0.22f, 0.20f, length * 0.28f), playerAccentMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Stock", false, renderers);
                CreateVisualSliceBlock($"{objectName}_Magazine", root.transform, origin + new Vector3(0f, -0.24f, length * 0.05f), new Vector3(0.13f, 0.42f, length * 0.13f), lootAmmoMaterial, RuntimeVisualModuleKind.Weapon, $"{assetId}_Magazine", false, renderers);
            }

            CreateVisualSliceSocket(root.transform, "Muzzle", origin + new Vector3(0f, 0.02f, length * 0.78f));
            CreateVisualSliceSocket(root.transform, rifle ? "Optic" : "ShellEject", origin + new Vector3(0f, height * 0.85f, length * 0.02f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private GameObject CreateVisualSliceBlock(string objectName, Transform parent, Vector3 position, Vector3 size, Material material, RuntimeVisualModuleKind kind, string moduleId, bool collider, List<Renderer> renderers)
        {
            GameObject block = new GameObject(objectName);
            runtimeObjects.Add(block);
            block.layer = groundLayer;
            block.isStatic = true;
            block.transform.position = position;
            if (parent != null)
            {
                block.transform.SetParent(parent, true);
            }

            MeshFilter meshFilter = block.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateVisualSliceBoxMesh($"{objectName}_Mesh", size);
            MeshRenderer meshRenderer = block.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            renderers?.Add(meshRenderer);
            if (collider)
            {
                BoxCollider box = block.AddComponent<BoxCollider>();
                box.size = size;
            }

            TagVisualModule(block, kind, moduleId);
            return block;
        }

        private void CreateVisualSliceTree(string objectName, Vector3 origin)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Vegetation, objectName);
            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock($"{objectName}_Trunk", root.transform, origin + new Vector3(0f, 1.05f, 0f), new Vector3(0.48f, 2.1f, 0.48f), treeTrunkMaterial, RuntimeVisualModuleKind.Vegetation, $"{objectName}_Trunk", true, renderers);
            CreateVisualSliceOctahedron($"{objectName}_Canopy_A", root.transform, origin + new Vector3(0f, 2.85f, 0f), new Vector3(2.4f, 1.55f, 2.2f), treeMaterial, RuntimeVisualModuleKind.Vegetation, $"{objectName}_Canopy_A", renderers);
            CreateVisualSliceOctahedron($"{objectName}_Canopy_B", root.transform, origin + new Vector3(0.35f, 3.65f, -0.2f), new Vector3(1.65f, 1.15f, 1.5f), foliageLightMaterial, RuntimeVisualModuleKind.Vegetation, $"{objectName}_Canopy_B", renderers);
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateVisualSliceRock(string objectName, Vector3 position, float scale)
        {
            GameObject rock = CreateVisualSliceOctahedron(objectName, null, position, new Vector3(2.5f * scale, 1.05f * scale, 1.85f * scale), rockMaterial, RuntimeVisualModuleKind.Terrain, objectName, null);
            rock.AddComponent<BoxCollider>().size = new Vector3(2.2f * scale, 0.9f * scale, 1.65f * scale);
            RegisterCoverPoint($"{objectName} Cover", position + Vector3.back * 2f);
        }

        private GameObject CreateVisualSliceOctahedron(string objectName, Transform parent, Vector3 position, Vector3 scale, Material material, RuntimeVisualModuleKind kind, string moduleId, List<Renderer> renderers)
        {
            GameObject target = new GameObject(objectName);
            runtimeObjects.Add(target);
            target.layer = groundLayer;
            target.isStatic = true;
            target.transform.position = position;
            target.transform.localScale = scale;
            if (parent != null)
            {
                target.transform.SetParent(parent, true);
            }

            Mesh mesh = new Mesh { name = $"{objectName}_Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(0f, 0.5f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(0f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, 0f),
                new Vector3(0f, 0f, -0.5f),
                new Vector3(0f, -0.5f, 0f)
            };
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 1, 4,
                5, 1, 2, 5, 2, 3, 5, 3, 4, 5, 4, 1
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            target.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = target.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderers?.Add(renderer);
            TagVisualModule(target, kind, moduleId);
            return target;
        }

        private Mesh CreateVisualSliceBoxMesh(string meshName, Vector3 size)
        {
            Vector3 h = size * 0.5f;
            Mesh mesh = new Mesh { name = meshName };
            mesh.vertices = new[]
            {
                new Vector3(-h.x, -h.y, h.z), new Vector3(h.x, -h.y, h.z), new Vector3(h.x, h.y, h.z), new Vector3(-h.x, h.y, h.z),
                new Vector3(h.x, -h.y, -h.z), new Vector3(-h.x, -h.y, -h.z), new Vector3(-h.x, h.y, -h.z), new Vector3(h.x, h.y, -h.z),
                new Vector3(-h.x, h.y, h.z), new Vector3(h.x, h.y, h.z), new Vector3(h.x, h.y, -h.z), new Vector3(-h.x, h.y, -h.z),
                new Vector3(-h.x, -h.y, -h.z), new Vector3(h.x, -h.y, -h.z), new Vector3(h.x, -h.y, h.z), new Vector3(-h.x, -h.y, h.z),
                new Vector3(h.x, -h.y, h.z), new Vector3(h.x, -h.y, -h.z), new Vector3(h.x, h.y, -h.z), new Vector3(h.x, h.y, h.z),
                new Vector3(-h.x, -h.y, -h.z), new Vector3(-h.x, -h.y, h.z), new Vector3(-h.x, h.y, h.z), new Vector3(-h.x, h.y, -h.z)
            };
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 7, 5, 5, 7, 6,
                8, 10, 9, 8, 11, 10,
                12, 14, 13, 12, 15, 14,
                16, 18, 17, 16, 19, 18,
                20, 22, 21, 20, 23, 22
            };
            mesh.uv = new[]
            {
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateVisualSliceSocket(Transform parent, string socketId, Vector3 position)
        {
            GameObject socket = new GameObject($"SOCKET_{socketId}");
            runtimeObjects.Add(socket);
            socket.transform.position = position;
            socket.transform.SetParent(parent, true);
            socket.AddComponent<BattleZoneArtSocket>().ConfigureRuntime(socketId, "Visual-slice socket. Preserve this name when replacing art.");
        }

        private void AddVisualSliceLODGroup(GameObject root, List<Renderer> renderers)
        {
            if (root == null || renderers == null || renderers.Count == 0)
            {
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }

            lodGroup.SetLODs(new[]
            {
                new LOD(0.22f, renderers.ToArray()),
                new LOD(0.07f, new[] { renderers[0] })
            });
            lodGroup.RecalculateBounds();
        }

        private void BuildMilestone17VerticalSliceArea()
        {
            Vector3 origin = new Vector3(18f, 0f, -72f);
            RegisterNamedLocation("M17 Vertical Slice District", origin);

            GameObject root = new GameObject("M17_VerticalSlice_200m_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Prop, "M17_VerticalSlice_200m");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "ENV_M17_200m_VerticalSlice",
                BattleZoneArtAssetCategory.Terrain,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("M17_Terrain_Grass_Base_200m", root.transform, origin + new Vector3(0f, 0.018f, 0f), new Vector3(200f, 0.036f, 200f), m17GrassMaterial, RuntimeVisualModuleKind.Terrain, "M17_Grass_Base", false, renderers);
            CreateVisualSliceBlock("M17_Terrain_Dirt_Service_Yard", root.transform, origin + new Vector3(46f, 0.04f, -42f), new Vector3(72f, 0.045f, 44f), m17DirtMaterial, RuntimeVisualModuleKind.Terrain, "M17_Dirt_Service_Yard", false, renderers);
            CreateVisualSliceBlock("M17_Terrain_Sand_River_Bank", root.transform, origin + new Vector3(83f, 0.045f, 0f), new Vector3(18f, 0.055f, 184f), m17SandMaterial, RuntimeVisualModuleKind.Terrain, "M17_Sand_River_Bank", false, renderers);
            CreateVisualSliceBlock("M17_River_Edge_Water", root.transform, origin + new Vector3(98f, 0.055f, 0f), new Vector3(20f, 0.04f, 190f), m17RiverMaterial, RuntimeVisualModuleKind.Terrain, "M17_River_Edge", false, renderers);

            CreateVisualSliceBlock("M17_Main_Road_Asphalt", root.transform, origin + new Vector3(0f, 0.085f, 0f), new Vector3(176f, 0.07f, 15f), m17RoadMaterial, RuntimeVisualModuleKind.Road, "M17_Main_Road", true, renderers);
            CreateVisualSliceBlock("M17_Cross_Road_Asphalt", root.transform, origin + new Vector3(0f, 0.09f, -36f), new Vector3(14f, 0.07f, 126f), m17RoadMaterial, RuntimeVisualModuleKind.Road, "M17_Cross_Road", true, renderers);
            CreateVisualSliceBlock("M17_Main_Road_NorthSidewalk", root.transform, origin + new Vector3(0f, 0.18f, 10.8f), new Vector3(178f, 0.24f, 4.6f), m17SidewalkMaterial, RuntimeVisualModuleKind.Road, "M17_Sidewalk_North", true, renderers);
            CreateVisualSliceBlock("M17_Main_Road_SouthSidewalk", root.transform, origin + new Vector3(0f, 0.18f, -10.8f), new Vector3(178f, 0.24f, 4.6f), m17SidewalkMaterial, RuntimeVisualModuleKind.Road, "M17_Sidewalk_South", true, renderers);
            CreateVisualSliceBlock("M17_Cross_Road_EastSidewalk", root.transform, origin + new Vector3(10.5f, 0.19f, -36f), new Vector3(4.2f, 0.24f, 126f), m17SidewalkMaterial, RuntimeVisualModuleKind.Road, "M17_Sidewalk_East", true, renderers);
            CreateVisualSliceBlock("M17_Cross_Road_WestSidewalk", root.transform, origin + new Vector3(-10.5f, 0.19f, -36f), new Vector3(4.2f, 0.24f, 126f), m17SidewalkMaterial, RuntimeVisualModuleKind.Road, "M17_Sidewalk_West", true, renderers);

            for (int i = 0; i < 7; i++)
            {
                float x = -68f + i * 22f;
                CreateVisualSliceBlock($"M17_Main_Road_LaneMark_{i + 1}", root.transform, origin + new Vector3(x, 0.135f, 0f), new Vector3(8f, 0.035f, 0.28f), laneMarkMaterial, RuntimeVisualModuleKind.Road, $"M17_LaneMark_{i + 1}", false, renderers);
            }

            CreateM17Apartment(origin + new Vector3(-55f, 0f, 34f));
            CreateM17Warehouse(origin + new Vector3(46f, 0f, -50f));
            CreateM17Shop(origin + new Vector3(-48f, 0f, -28f));
            CreateM17GuardTower(origin + new Vector3(56f, 0f, 42f));
            CreateM17CharacterPrefabSlot(origin + new Vector3(-12f, 0f, 34f));

            for (int i = 0; i < 11; i++)
            {
                float z = -82f + i * 16f;
                CreateVisualSliceTree($"M17_River_Tree_{i + 1}", origin + new Vector3(72f + (i % 2) * 5f, 0f, z));
            }

            for (int i = 0; i < 8; i++)
            {
                CreateM17Bush($"M17_Sidewalk_Bush_{i + 1}", origin + new Vector3(-76f + i * 21f, 0.35f, 15.5f + (i % 2) * 4f), 1f + (i % 3) * 0.12f);
            }

            for (int i = 0; i < 9; i++)
            {
                CreateVisualSliceRock($"M17_River_Rock_{i + 1}", origin + new Vector3(78f + (i % 3) * 4f, 0.45f, -72f + i * 18f), 0.82f + (i % 3) * 0.12f);
            }

            for (int i = 0; i < 6; i++)
            {
                float x = -66f + i * 26f;
                CreateM17StreetLight($"M17_StreetLight_{i + 1}", root.transform, origin + new Vector3(x, 0f, i % 2 == 0 ? 14.5f : -14.5f), renderers);
            }

            for (int i = 0; i < 5; i++)
            {
                CreateM17ElectricPole($"M17_ElectricPole_{i + 1}", root.transform, origin + new Vector3(-88f, 0f, -78f + i * 38f), renderers);
            }

            CreateM17FenceLine("M17_River_Safety_Fence", root.transform, origin + new Vector3(70f, 0.8f, 0f), 172f, false);
            CreateM17FenceLine("M17_Warehouse_Yard_Fence", root.transform, origin + new Vector3(45f, 0.8f, -79f), 72f, true);
            CreateM17PropCluster(root.transform, origin + new Vector3(40f, 0f, -72f));
            CreateM17PropCluster(root.transform, origin + new Vector3(-44f, 0f, -52f));
            CreateReflectionProbe("M17_VerticalSlice_ReflectionProbe", origin + new Vector3(0f, 7f, -5f), new Vector3(210f, 24f, 210f), 0.62f);
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17Apartment(Vector3 origin)
        {
            GameObject root = new GameObject("M17_Apartment_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "M17_Apartment");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_M17_Apartment",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("M17_Apartment_Lobby_Floor", root.transform, origin + new Vector3(0f, 0.14f, 0f), new Vector3(22f, 0.28f, 18f), roadMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_Floor", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_BackWall", root.transform, origin + new Vector3(0f, 7.2f, 9f), new Vector3(22f, 14.4f, 0.5f), m17ApartmentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_BackWall", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_LeftWall", root.transform, origin + new Vector3(-11f, 7.2f, 0f), new Vector3(0.5f, 14.4f, 18f), m17ApartmentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_LeftWall", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_RightWall", root.transform, origin + new Vector3(11f, 7.2f, 0f), new Vector3(0.5f, 14.4f, 18f), m17ApartmentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_RightWall", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_FrontLeft", root.transform, origin + new Vector3(-7.4f, 7.2f, -9f), new Vector3(7.2f, 14.4f, 0.5f), m17ApartmentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_FrontLeft", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_FrontRight", root.transform, origin + new Vector3(7.4f, 7.2f, -9f), new Vector3(7.2f, 14.4f, 0.5f), m17ApartmentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_FrontRight", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_DoorHeader", root.transform, origin + new Vector3(0f, 4.2f, -9f), new Vector3(7f, 2f, 0.5f), facadeAccentMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_DoorHeader", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_Roof", root.transform, origin + new Vector3(0f, 14.75f, 0f), new Vector3(23.8f, 0.7f, 19.8f), roofMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_Roof", true, renderers);

            for (int floor = 0; floor < 4; floor++)
            {
                float y = 2.45f + floor * 3.1f;
                CreateVisualSliceBlock($"M17_Apartment_FloorTrim_{floor + 1}", root.transform, origin + new Vector3(0f, y - 1.18f, -9.28f), new Vector3(21.4f, 0.14f, 0.12f), facadeAccentMaterial, RuntimeVisualModuleKind.Building, $"M17_Apartment_Trim_{floor + 1}", false, renderers);
                for (int column = 0; column < 4; column++)
                {
                    float x = -6.9f + column * 4.6f;
                    CreateVisualSliceBlock($"M17_Apartment_Window_{floor + 1}_{column + 1}", root.transform, origin + new Vector3(x, y, -9.34f), new Vector3(1.25f, 1.05f, 0.12f), windowMaterial, RuntimeVisualModuleKind.Building, $"M17_Apartment_Window_{floor + 1}_{column + 1}", false, renderers);
                }
            }

            CreateVisualSliceBlock("M17_Apartment_Interior_Desk", root.transform, origin + new Vector3(-4f, 0.82f, 1.6f), new Vector3(4.6f, 0.72f, 1.5f), coverMaterial, RuntimeVisualModuleKind.Prop, "M17_Apartment_Desk", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_Stair_Run", root.transform, origin + new Vector3(6.5f, 1.05f, 3.4f), new Vector3(4.4f, 2.1f, 1.5f), m17SidewalkMaterial, RuntimeVisualModuleKind.Building, "M17_Apartment_Stairs", true, renderers);
            CreateVisualSliceBlock("M17_Apartment_Loot_Shelf", root.transform, origin + new Vector3(-8.5f, 1.25f, 4.8f), new Vector3(1.35f, 2.5f, 4.4f), armorDisplayMaterial, RuntimeVisualModuleKind.Prop, "M17_Apartment_LootShelf", true, renderers);
            CreateVisualSliceSocket(root.transform, "Doorway", origin + new Vector3(0f, 1.25f, -9.45f));
            CreateVisualSliceSocket(root.transform, "LootAnchor", origin + new Vector3(-8.3f, 1.2f, 4.8f));
            RegisterCoverPoint("M17 Apartment Lobby Cover", origin + new Vector3(-4f, 0f, 1.6f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17Warehouse(Vector3 origin)
        {
            GameObject root = new GameObject("M17_Warehouse_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "M17_Warehouse");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_M17_Warehouse",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("M17_Warehouse_Floor", root.transform, origin + new Vector3(0f, 0.14f, 0f), new Vector3(34f, 0.28f, 26f), roadMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_Floor", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_BackWall", root.transform, origin + new Vector3(0f, 5.1f, 13f), new Vector3(34f, 10.2f, 0.55f), m17WarehouseMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_BackWall", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_LeftWall", root.transform, origin + new Vector3(-17f, 5.1f, 0f), new Vector3(0.55f, 10.2f, 26f), m17WarehouseMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_LeftWall", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_RightWall", root.transform, origin + new Vector3(17f, 5.1f, 0f), new Vector3(0.55f, 10.2f, 26f), m17WarehouseMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_RightWall", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_FrontHeader", root.transform, origin + new Vector3(0f, 7.6f, -13f), new Vector3(34f, 4.8f, 0.55f), m17WarehouseMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_FrontHeader", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_Roof", root.transform, origin + new Vector3(0f, 10.6f, 0f), new Vector3(36f, 0.72f, 28f), roofMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_Roof", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_SideOffice", root.transform, origin + new Vector3(10.5f, 2.1f, -2f), new Vector3(9.5f, 4.2f, 6.5f), buildingMaterial, RuntimeVisualModuleKind.Building, "M17_Warehouse_Office", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_CrateStack_A", root.transform, origin + new Vector3(-7f, 1.45f, 4.5f), new Vector3(7f, 2.9f, 5.2f), coverMaterial, RuntimeVisualModuleKind.Prop, "M17_Warehouse_Crates_A", true, renderers);
            CreateVisualSliceBlock("M17_Warehouse_CrateStack_B", root.transform, origin + new Vector3(2.4f, 1.2f, 6.5f), new Vector3(6.4f, 2.4f, 4.8f), lootCrateMaterial, RuntimeVisualModuleKind.Prop, "M17_Warehouse_Crates_B", true, renderers);
            CreateVisualSliceSocket(root.transform, "DoorwayA", origin + new Vector3(0f, 1.4f, -13.35f));
            CreateVisualSliceSocket(root.transform, "LootAnchor", origin + new Vector3(2.4f, 1.7f, 6.5f));
            RegisterCoverPoint("M17 Warehouse Crate Cover", origin + new Vector3(-7f, 0f, 4.5f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17Shop(Vector3 origin)
        {
            GameObject root = new GameObject("M17_Shop_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "M17_Shop");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_M17_CornerShop",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("M17_Shop_Floor", root.transform, origin + new Vector3(0f, 0.12f, 0f), new Vector3(18f, 0.24f, 14f), roadMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_Floor", true, renderers);
            CreateVisualSliceBlock("M17_Shop_BackWall", root.transform, origin + new Vector3(0f, 3.1f, 7f), new Vector3(18f, 6.2f, 0.45f), m17ShopMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_BackWall", true, renderers);
            CreateVisualSliceBlock("M17_Shop_LeftWall", root.transform, origin + new Vector3(-9f, 3.1f, 0f), new Vector3(0.45f, 6.2f, 14f), m17ShopMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_LeftWall", true, renderers);
            CreateVisualSliceBlock("M17_Shop_RightWall", root.transform, origin + new Vector3(9f, 3.1f, 0f), new Vector3(0.45f, 6.2f, 14f), m17ShopMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_RightWall", true, renderers);
            CreateVisualSliceBlock("M17_Shop_FrontHeader", root.transform, origin + new Vector3(0f, 4.75f, -7f), new Vector3(18f, 2.1f, 0.45f), m17ShopMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_FrontHeader", true, renderers);
            CreateVisualSliceBlock("M17_Shop_Roof", root.transform, origin + new Vector3(0f, 6.48f, 0f), new Vector3(19.4f, 0.54f, 15.4f), roofMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_Roof", true, renderers);
            CreateVisualSliceBlock("M17_Shop_GlassLeft", root.transform, origin + new Vector3(-4.8f, 2.1f, -7.25f), new Vector3(2.6f, 2.2f, 0.12f), windowMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_Window_L", false, renderers);
            CreateVisualSliceBlock("M17_Shop_GlassRight", root.transform, origin + new Vector3(4.8f, 2.1f, -7.25f), new Vector3(2.6f, 2.2f, 0.12f), windowMaterial, RuntimeVisualModuleKind.Building, "M17_Shop_Window_R", false, renderers);
            CreateVisualSliceBlock("M17_Shop_Counter", root.transform, origin + new Vector3(0f, 0.82f, 2f), new Vector3(7f, 0.8f, 1.4f), coverMaterial, RuntimeVisualModuleKind.Prop, "M17_Shop_Counter", true, renderers);
            CreateVisualSliceBlock("M17_Shop_Shelf", root.transform, origin + new Vector3(7.1f, 1.15f, 2.6f), new Vector3(1.3f, 2.3f, 5f), armorDisplayMaterial, RuntimeVisualModuleKind.Prop, "M17_Shop_Shelf", true, renderers);
            CreateVisualSliceSocket(root.transform, "Doorway", origin + new Vector3(0f, 1.15f, -7.35f));
            CreateVisualSliceSocket(root.transform, "LootAnchor", origin + new Vector3(7f, 1.4f, 2.6f));
            RegisterCoverPoint("M17 Shop Counter Cover", origin + new Vector3(0f, 0f, 2f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17GuardTower(Vector3 origin)
        {
            GameObject root = new GameObject("M17_GuardTower_Root");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, "M17_GuardTower");
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "BLD_M17_GuardTower",
                BattleZoneArtAssetCategory.Building,
                BattleZoneArtProductionState.VisualSliceProxy);

            List<Renderer> renderers = new List<Renderer>();
            for (int i = 0; i < 4; i++)
            {
                float x = i < 2 ? -2.6f : 2.6f;
                float z = i % 2 == 0 ? -2.6f : 2.6f;
                CreateM17Cylinder($"M17_GuardTower_Leg_{i + 1}", root.transform, origin + new Vector3(x, 3.1f, z), new Vector3(0.34f, 6.2f, 0.34f), militaryMaterial, RuntimeVisualModuleKind.Building, $"M17_Tower_Leg_{i + 1}", true, renderers);
            }

            CreateVisualSliceBlock("M17_GuardTower_Platform", root.transform, origin + new Vector3(0f, 6.35f, 0f), new Vector3(7.2f, 0.45f, 7.2f), coverMaterial, RuntimeVisualModuleKind.Building, "M17_Tower_Platform", true, renderers);
            CreateVisualSliceBlock("M17_GuardTower_Cabin", root.transform, origin + new Vector3(0f, 7.8f, 0f), new Vector3(5.5f, 2.2f, 5.5f), militaryMaterial, RuntimeVisualModuleKind.Building, "M17_Tower_Cabin", true, renderers);
            CreateVisualSliceBlock("M17_GuardTower_WindowSlit", root.transform, origin + new Vector3(0f, 8.05f, -2.86f), new Vector3(3.4f, 0.38f, 0.1f), windowMaterial, RuntimeVisualModuleKind.Building, "M17_Tower_Window", false, renderers);
            CreateVisualSliceBlock("M17_GuardTower_Roof", root.transform, origin + new Vector3(0f, 9.2f, 0f), new Vector3(7f, 0.42f, 7f), roofMaterial, RuntimeVisualModuleKind.Building, "M17_Tower_Roof", true, renderers);
            CreateVisualSliceBlock("M17_GuardTower_Ladder", root.transform, origin + new Vector3(-3.15f, 3.2f, -0.3f), new Vector3(0.28f, 6.4f, 0.28f), fenceMaterial, RuntimeVisualModuleKind.Prop, "M17_Tower_Ladder", true, renderers);
            CreateVisualSliceSocket(root.transform, "LookoutAnchor", origin + new Vector3(0f, 7.8f, -2.4f));
            RegisterCoverPoint("M17 Guard Tower Cover", origin + new Vector3(3.8f, 0f, -3.8f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17CharacterPrefabSlot(Vector3 origin)
        {
            GameObject root = new GameObject("M17_TacticalCharacter_PrefabSlot");
            runtimeObjects.Add(root);
            root.transform.position = origin;
            root.AddComponent<BattleZoneArtReplacementSlot>().ConfigureRuntime(
                "CHR_TacticalOperator_VS",
                BattleZoneArtAssetCategory.Character,
                BattleZoneArtProductionState.ArtistReadySlot);
            TagVisualModule(root, RuntimeVisualModuleKind.Character, "M17_Character_PrefabSlot");

            List<Renderer> renderers = new List<Renderer>();
            CreateVisualSliceBlock("M17_CharacterSlot_Base", root.transform, origin + new Vector3(0f, 0.08f, 0f), new Vector3(2.2f, 0.16f, 2.2f), m17SidewalkMaterial, RuntimeVisualModuleKind.Prop, "M17_CharacterSlot_Base", false, renderers);
            CreateVisualSliceBlock("M17_CharacterSlot_RigMarker", root.transform, origin + new Vector3(0f, 1.05f, 0f), new Vector3(0.42f, 2.1f, 0.42f), playerAccentMaterial, RuntimeVisualModuleKind.Character, "M17_CharacterSlot_RigMarker", false, renderers);
            CreateVisualSliceSocket(root.transform, "Head", origin + new Vector3(0f, 1.78f, 0f));
            CreateVisualSliceSocket(root.transform, "RightHand", origin + new Vector3(0.45f, 1.18f, 0f));
            CreateVisualSliceSocket(root.transform, "LeftHand", origin + new Vector3(-0.45f, 1.18f, 0f));
            CreateVisualSliceSocket(root.transform, "MuzzleAnchor", origin + new Vector3(0.38f, 1.2f, -0.72f));
            AddVisualSliceLODGroup(root, renderers);
        }

        private void CreateM17Bush(string objectName, Vector3 position, float scale)
        {
            GameObject bush = CreateVisualSliceOctahedron(objectName, null, position, new Vector3(1.8f * scale, 0.82f * scale, 1.45f * scale), foliageLightMaterial, RuntimeVisualModuleKind.Vegetation, objectName, null);
            Collider collider = bush.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }
        }

        private void CreateM17PropCluster(Transform parent, Vector3 origin)
        {
            CreateM17Cylinder("M17_Utility_Barrel_A", parent, origin + new Vector3(-3.2f, 0.55f, 0f), new Vector3(0.85f, 1.1f, 0.85f), m17BarrelMaterial, RuntimeVisualModuleKind.Prop, "M17_Barrel_A", true, null);
            CreateM17Cylinder("M17_Utility_Barrel_B", parent, origin + new Vector3(-1.8f, 0.55f, 1.2f), new Vector3(0.85f, 1.1f, 0.85f), m17BarrelMaterial, RuntimeVisualModuleKind.Prop, "M17_Barrel_B", true, null);
            CreateVisualSliceBlock("M17_Stacked_Crate_A", parent, origin + new Vector3(1.5f, 0.72f, 0f), new Vector3(2.8f, 1.44f, 2.2f), coverMaterial, RuntimeVisualModuleKind.Prop, "M17_Crate_A", true, null);
            CreateVisualSliceBlock("M17_Stacked_Crate_B", parent, origin + new Vector3(4.1f, 0.5f, 1.4f), new Vector3(2.2f, 1.0f, 1.8f), lootCrateMaterial, RuntimeVisualModuleKind.Prop, "M17_Crate_B", true, null);
            CreateVisualSliceBlock("M17_Street_Bench", parent, origin + new Vector3(0f, 0.52f, -3.7f), new Vector3(5.4f, 0.46f, 1.1f), fenceMaterial, RuntimeVisualModuleKind.Prop, "M17_Bench", true, null);
        }

        private void CreateM17FenceLine(string objectName, Transform parent, Vector3 center, float length, bool horizontal)
        {
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 8f));
            for (int i = 0; i < posts; i++)
            {
                float offset = Mathf.Lerp(-length * 0.5f, length * 0.5f, posts == 1 ? 0.5f : i / (float)(posts - 1));
                Vector3 position = center + (horizontal ? new Vector3(offset, 0f, 0f) : new Vector3(0f, 0f, offset));
                CreateM17Cylinder($"{objectName}_Post_{i + 1}", parent, position, new Vector3(0.34f, 1.6f, 0.34f), fenceMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_Post_{i + 1}", true, null);
            }

            Vector3 railScale = horizontal ? new Vector3(length, 0.18f, 0.24f) : new Vector3(0.24f, 0.18f, length);
            CreateVisualSliceBlock($"{objectName}_Rail", parent, center + Vector3.up * 0.42f, railScale, fenceMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_Rail", true, null);
        }

        private void CreateM17StreetLight(string objectName, Transform parent, Vector3 origin, List<Renderer> renderers)
        {
            CreateM17Cylinder($"{objectName}_Pole", parent, origin + new Vector3(0f, 2.3f, 0f), new Vector3(0.24f, 4.6f, 0.24f), vehicleAccentMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_Pole", true, renderers);
            CreateVisualSliceBlock($"{objectName}_Arm", parent, origin + new Vector3(1.15f, 4.38f, 0f), new Vector3(2.3f, 0.16f, 0.16f), vehicleAccentMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_Arm", false, renderers);
            CreateVisualSliceBlock($"{objectName}_Lamp", parent, origin + new Vector3(2.25f, 4.2f, 0f), new Vector3(0.72f, 0.28f, 0.52f), m17WarmLightMaterial, RuntimeVisualModuleKind.Lighting, $"{objectName}_Lamp", false, renderers);
        }

        private void CreateM17ElectricPole(string objectName, Transform parent, Vector3 origin, List<Renderer> renderers)
        {
            CreateM17Cylinder($"{objectName}_Pole", parent, origin + new Vector3(0f, 2.9f, 0f), new Vector3(0.36f, 5.8f, 0.36f), treeTrunkMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_Pole", true, renderers);
            CreateVisualSliceBlock($"{objectName}_CrossArm", parent, origin + new Vector3(0f, 5.45f, 0f), new Vector3(5.8f, 0.18f, 0.22f), fenceMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_CrossArm", false, renderers);
            CreateVisualSliceBlock($"{objectName}_WireHint", parent, origin + new Vector3(0f, 5.36f, 0f), new Vector3(7.2f, 0.045f, 0.06f), vehicleAccentMaterial, RuntimeVisualModuleKind.Prop, $"{objectName}_WireHint", false, renderers);
        }

        private GameObject CreateM17Cylinder(string objectName, Transform parent, Vector3 position, Vector3 scale, Material material, RuntimeVisualModuleKind kind, string moduleId, bool collider, List<Renderer> renderers)
        {
            GameObject target = new GameObject(objectName);
            runtimeObjects.Add(target);
            target.layer = groundLayer;
            target.isStatic = true;
            target.transform.position = position;
            target.transform.localScale = scale;
            if (parent != null)
            {
                target.transform.SetParent(parent, true);
            }

            MeshFilter meshFilter = target.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateM17CylinderMesh($"{objectName}_Mesh", 12);
            MeshRenderer meshRenderer = target.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            renderers?.Add(meshRenderer);

            if (collider)
            {
                BoxCollider box = target.AddComponent<BoxCollider>();
                box.size = Vector3.one;
            }

            TagVisualModule(target, kind, moduleId);
            return target;
        }

        private Mesh CreateM17CylinderMesh(string meshName, int segments)
        {
            segments = Mathf.Max(8, segments);
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            float halfHeight = 0.5f;
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * 0.5f;
                float z = Mathf.Sin(angle) * 0.5f;
                vertices.Add(new Vector3(x, -halfHeight, z));
                vertices.Add(new Vector3(x, halfHeight, z));
            }

            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -halfHeight, 0f));
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, halfHeight, 0f));

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int bottomA = i * 2;
                int topA = bottomA + 1;
                int bottomB = next * 2;
                int topB = bottomB + 1;

                triangles.Add(bottomA);
                triangles.Add(topA);
                triangles.Add(topB);
                triangles.Add(bottomA);
                triangles.Add(topB);
                triangles.Add(bottomB);

                triangles.Add(bottomCenter);
                triangles.Add(bottomB);
                triangles.Add(bottomA);

                triangles.Add(topCenter);
                triangles.Add(topA);
                triangles.Add(topB);
            }

            Mesh mesh = new Mesh { name = meshName };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateAngledRoadModule(string objectName, Vector3 center, Vector3 scale, float yaw, Material material)
        {
            Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject road = CreateWorldBlock(objectName, center + Vector3.up * 0.055f, scale, material);
            road.transform.rotation = rotation;
            TagVisualModule(road, RuntimeVisualModuleKind.Road, objectName);

            int dashCount = Mathf.Max(4, Mathf.RoundToInt(scale.x / 24f));
            for (int i = 0; i < dashCount; i++)
            {
                float offset = Mathf.Lerp(-scale.x * 0.42f, scale.x * 0.42f, dashCount == 1 ? 0.5f : i / (float)(dashCount - 1));
                GameObject dash = CreateWorldBlock($"{objectName} Center Mark {i + 1}", center + rotation * new Vector3(offset, 0.12f, 0f), new Vector3(5.1f, 0.05f, 0.3f), laneMarkMaterial);
                dash.transform.rotation = rotation;
                TagVisualModule(dash, RuntimeVisualModuleKind.Road, $"{objectName}_Mark_{i + 1}");
            }
        }

        private void CreateM11Slope(string objectName, Vector3 position, Vector3 scale, float yaw)
        {
            GameObject slope = CreateWorldBlock(objectName, position, scale, hillMaterial);
            slope.transform.rotation = Quaternion.Euler(4f, yaw, -3f);
            TagVisualModule(slope, RuntimeVisualModuleKind.Terrain, objectName);
        }

        private void CreateM11Lake(string objectName, Vector3 position, Vector3 scale, float yaw)
        {
            GameObject lake = CreateWorldBlock(objectName, position + Vector3.up * 0.04f, scale, lakeMaterial);
            lake.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            TagVisualModule(lake, RuntimeVisualModuleKind.Terrain, objectName);
            GameObject beach = CreateWorldBlock($"{objectName} Beach Rim", position + Vector3.up * 0.025f, new Vector3(scale.x + 8f, 0.06f, scale.z + 7f), beachMaterial);
            beach.transform.rotation = lake.transform.rotation;
            TagVisualModule(beach, RuntimeVisualModuleKind.Terrain, $"{objectName}_Beach");
        }

        private void CreateM11GrassPatch(string objectName, Vector3 position, float size)
        {
            GameObject patch = CreateWorldBlock(objectName, position, new Vector3(size, 0.08f, size * 0.65f), grassMaterial);
            patch.transform.rotation = Quaternion.Euler(0f, size * 5f, 0f);
            TagVisualModule(patch, RuntimeVisualModuleKind.Vegetation, objectName);
            Collider collider = patch.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }
        }

        private void CreateM11Rock(string objectName, Vector3 position, float scale)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            runtimeObjects.Add(rock);
            rock.name = objectName;
            rock.layer = groundLayer;
            rock.isStatic = true;
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(3.4f * scale, 1.05f * scale, 2.7f * scale);
            rock.GetComponent<Renderer>().material = rockMaterial;
            TagVisualModule(rock, RuntimeVisualModuleKind.Terrain, objectName);
            RegisterCoverPoint($"{objectName} Cover", position + Vector3.back * 3f);
        }

        private void CreateM11FlowerPatch(string objectName, Vector3 position)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.position = position;
            TagVisualModule(root, RuntimeVisualModuleKind.Vegetation, objectName);

            for (int i = 0; i < 9; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 40f, 0f) * new Vector3(1.2f + (i % 3) * 0.45f, 0.12f, 0f);
                GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(flower);
                flower.name = $"{objectName} Flower {i + 1}";
                flower.layer = groundLayer;
                flower.transform.SetParent(root.transform, false);
                flower.transform.localPosition = offset;
                flower.transform.localScale = new Vector3(0.22f, 0.18f, 0.22f);
                flower.GetComponent<Renderer>().material = i % 2 == 0 ? flowerMaterial : foliageLightMaterial;
                Collider collider = flower.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyRuntimeObject(collider);
                }
            }
        }

        private void CreateM11LootProp(string objectName, Vector3 position, LootKind kind)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runtimeObjects.Add(root);
            root.name = objectName;
            root.layer = lootLayer;
            root.transform.position = position;
            root.transform.localScale = new Vector3(1.2f, 0.42f, 0.82f);
            root.GetComponent<Renderer>().material = kind == LootKind.Medkit ? medkitMaterial : kind == LootKind.ArmorVest ? armorDisplayMaterial : lootCrateMaterial;
            TagVisualModule(root, RuntimeVisualModuleKind.Loot, objectName);
            DecorateLootVisual(root.transform, kind, objectName);
        }

        private void CreateM11FuelCanProp(string objectName, Vector3 position)
        {
            GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runtimeObjects.Add(can);
            can.name = objectName;
            can.layer = groundLayer;
            can.transform.position = position;
            can.transform.localScale = new Vector3(0.72f, 0.95f, 0.42f);
            can.GetComponent<Renderer>().material = fuelCanMaterial;
            TagVisualModule(can, RuntimeVisualModuleKind.Loot, objectName);
            CreateWorldBlock($"{objectName} Handle", position + new Vector3(0f, 0.65f, 0f), new Vector3(0.46f, 0.16f, 0.18f), vehicleAccentMaterial);
            CreateWorldBlock($"{objectName} Cap", position + new Vector3(0.28f, 0.56f, -0.08f), new Vector3(0.18f, 0.18f, 0.18f), vehicleAccentMaterial);
        }

        private void CreateM11Cloud(string objectName, Vector3 position, Vector3 scale)
        {
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            runtimeObjects.Add(cloud);
            cloud.name = objectName;
            cloud.layer = groundLayer;
            cloud.transform.position = position;
            cloud.transform.localScale = scale;
            cloud.GetComponent<Renderer>().material = cloudMaterial;
            TagVisualModule(cloud, RuntimeVisualModuleKind.Lighting, objectName);
            Collider collider = cloud.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }
        }

        private void CreateRoadModule(string objectName, Vector3 center, float length, bool northSouth, float width)
        {
            Vector3 roadScale = northSouth ? new Vector3(width, 0.06f, length) : new Vector3(length, 0.06f, width);
            GameObject road = CreateWorldBlock(objectName, center + Vector3.up * 0.055f, roadScale, roadMaterial);
            TagVisualModule(road, RuntimeVisualModuleKind.Road, objectName);

            Vector3 sidewalkScale = northSouth ? new Vector3(3.4f, 0.18f, length) : new Vector3(length, 0.18f, 3.4f);
            Vector3 sidewalkOffset = northSouth ? new Vector3(width * 0.5f + 2.1f, 0.13f, 0f) : new Vector3(0f, 0.13f, width * 0.5f + 2.1f);
            GameObject sidewalkA = CreateWorldBlock($"{objectName} Sidewalk A", center + sidewalkOffset, sidewalkScale, sidewalkMaterial);
            GameObject sidewalkB = CreateWorldBlock($"{objectName} Sidewalk B", center - sidewalkOffset, sidewalkScale, sidewalkMaterial);
            TagVisualModule(sidewalkA, RuntimeVisualModuleKind.Road, $"{objectName}_Sidewalk_A");
            TagVisualModule(sidewalkB, RuntimeVisualModuleKind.Road, $"{objectName}_Sidewalk_B");

            int dashCount = Mathf.Max(4, Mathf.RoundToInt(length / 18f));
            for (int i = 0; i < dashCount; i++)
            {
                float t = dashCount == 1 ? 0.5f : i / (float)(dashCount - 1);
                float offset = Mathf.Lerp(-length * 0.42f, length * 0.42f, t);
                Vector3 dashPosition = center + (northSouth ? new Vector3(0f, 0.12f, offset) : new Vector3(offset, 0.12f, 0f));
                Vector3 dashScale = northSouth ? new Vector3(0.34f, 0.05f, 5.2f) : new Vector3(5.2f, 0.05f, 0.34f);
                GameObject dash = CreateWorldBlock($"{objectName} Lane Mark {i + 1}", dashPosition, dashScale, laneMarkMaterial);
                TagVisualModule(dash, RuntimeVisualModuleKind.Road, $"{objectName}_Lane_{i + 1}");
            }
        }

        private void CreateIntersection(string objectName, Vector3 center, Vector2 size)
        {
            GameObject intersection = CreateWorldBlock(objectName, center + Vector3.up * 0.08f, new Vector3(size.x, 0.08f, size.y), roadMaterial);
            TagVisualModule(intersection, RuntimeVisualModuleKind.Road, objectName);
            CreateWorldBlock($"{objectName} Crosswalk North", center + new Vector3(0f, 0.16f, size.y * 0.36f), new Vector3(size.x * 0.72f, 0.05f, 0.55f), laneMarkMaterial);
            CreateWorldBlock($"{objectName} Crosswalk South", center + new Vector3(0f, 0.16f, -size.y * 0.36f), new Vector3(size.x * 0.72f, 0.05f, 0.55f), laneMarkMaterial);
            CreateWorldBlock($"{objectName} Crosswalk East", center + new Vector3(size.x * 0.36f, 0.16f, 0f), new Vector3(0.55f, 0.05f, size.y * 0.72f), laneMarkMaterial);
            CreateWorldBlock($"{objectName} Crosswalk West", center + new Vector3(-size.x * 0.36f, 0.16f, 0f), new Vector3(0.55f, 0.05f, size.y * 0.72f), laneMarkMaterial);
        }

        private void CreateModularBuildingAsset(string objectName, Vector3 origin, Vector3 size, int floors, float tintOffset)
        {
            GameObject root = new GameObject($"{objectName} Building Root");
            runtimeObjects.Add(root);
            root.layer = groundLayer;
            root.isStatic = true;
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Building, objectName);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, size.y * 0.5f, 0f);
            collider.size = size;
            RegisterCoverPoint($"{objectName} Cover", origin + new Vector3(0f, 0f, -size.z * 0.62f));

            List<Renderer> renderers = new List<Renderer>();
            Material facade = CreateMaterial($"{objectName} Facade Material", Color.Lerp(new Color(0.42f, 0.46f, 0.44f), new Color(0.58f, 0.56f, 0.50f), tintOffset));
            Material trim = tintOffset > 0.12f ? facadeAccentMaterial : coverMaterial;

            AddVisualPart(root.transform, $"{objectName} Podium", new Vector3(0f, 1.25f, 0f), new Vector3(size.x + 1.2f, 2.5f, size.z + 1.2f), facade, renderers);
            AddVisualPart(root.transform, $"{objectName} Core", new Vector3(0f, size.y * 0.5f, 0f), size, facade, renderers);
            AddVisualPart(root.transform, $"{objectName} Roof Cap", new Vector3(0f, size.y + 0.32f, 0f), new Vector3(size.x + 1.7f, 0.62f, size.z + 1.7f), roofMaterial, renderers);

            for (int floor = 1; floor <= floors; floor++)
            {
                float y = Mathf.Min(size.y - 1.25f, floor * (size.y / Mathf.Max(1, floors)) + 0.75f);
                AddVisualPart(root.transform, $"{objectName} Floor Trim {floor}", new Vector3(0f, y - 0.95f, -size.z * 0.52f), new Vector3(size.x + 0.65f, 0.18f, 0.18f), trim, renderers);
                int windowColumns = Mathf.Max(2, Mathf.RoundToInt(size.x / 4.2f));
                for (int column = 0; column < windowColumns; column++)
                {
                    float x = Mathf.Lerp(-size.x * 0.34f, size.x * 0.34f, windowColumns == 1 ? 0.5f : column / (float)(windowColumns - 1));
                    AddVisualPart(root.transform, $"{objectName} Front Window {floor}-{column + 1}", new Vector3(x, y, -size.z * 0.535f), new Vector3(1.15f, 0.9f, 0.08f), windowMaterial, renderers);
                }

                AddVisualPart(root.transform, $"{objectName} Side Window {floor}", new Vector3(-size.x * 0.535f, y, 0f), new Vector3(0.08f, 0.88f, Mathf.Max(1.2f, size.z * 0.26f)), windowMaterial, renderers);
            }

            AddVisualPart(root.transform, $"{objectName} Entry Door", new Vector3(0f, 1.15f, -size.z * 0.57f), new Vector3(2.4f, 2.3f, 0.14f), roadMaterial, renderers);
            AddVisualPart(root.transform, $"{objectName} Roof Unit", new Vector3(size.x * 0.2f, size.y + 0.95f, size.z * 0.18f), new Vector3(3.2f, 0.78f, 2.4f), vehicleAccentMaterial, renderers);
            AddVisualPart(root.transform, $"{objectName} Sign Panel", new Vector3(-size.x * 0.28f, 2.65f, -size.z * 0.58f), new Vector3(3.6f, 0.7f, 0.12f), laneMarkMaterial, renderers);

            LODGroup lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.16f, renderers.ToArray()),
                new LOD(0.055f, new[] { renderers[1], renderers[2] })
            });
            lodGroup.RecalculateBounds();
        }

        private void CreateCityPropSet(string prefix, Vector3 origin)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 lightPosition = origin + new Vector3(-56f + i * 22f, 1.7f, -12f + (i % 2) * 28f);
                GameObject pole = CreateWorldBlock($"{prefix} Modular Street Light {i + 1}", lightPosition, new Vector3(0.24f, 3.4f, 0.24f), vehicleAccentMaterial);
                TagVisualModule(pole, RuntimeVisualModuleKind.Prop, $"{prefix}_StreetLight_{i + 1}");
                CreateWorldBlock($"{pole.name} Arm", lightPosition + new Vector3(1.05f, 1.55f, 0f), new Vector3(2.1f, 0.18f, 0.18f), vehicleAccentMaterial);
                CreateWorldBlock($"{pole.name} Lamp", lightPosition + new Vector3(2.05f, 1.42f, 0f), new Vector3(0.56f, 0.22f, 0.44f), windowMaterial);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector3 kioskPosition = origin + new Vector3(-44f + i * 31f, 0.72f, 16f);
                GameObject kiosk = CreateWorldBlock($"{prefix} Modular Kiosk {i + 1}", kioskPosition, new Vector3(3.4f, 1.44f, 2.2f), facadeAccentMaterial);
                TagVisualModule(kiosk, RuntimeVisualModuleKind.Prop, $"{prefix}_Kiosk_{i + 1}");
                CreateWorldBlock($"{kiosk.name} Awning", kioskPosition + Vector3.up * 1.02f, new Vector3(4.2f, 0.22f, 2.8f), laneMarkMaterial);
            }
        }

        private void CreateTerrainHillAsset(string objectName, Vector3 origin, Vector3 size, float yaw)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.layer = groundLayer;
            root.isStatic = true;
            root.transform.SetPositionAndRotation(origin, Quaternion.Euler(0f, yaw, 0f));
            TagVisualModule(root, RuntimeVisualModuleKind.Terrain, objectName);

            List<Renderer> renderers = new List<Renderer>();
            AddVisualPart(root.transform, $"{objectName} Mass", new Vector3(0f, size.y * 0.42f, 0f), size, hillMaterial, renderers, PrimitiveType.Sphere);
            AddVisualPart(root.transform, $"{objectName} Rock Face", new Vector3(-size.x * 0.18f, size.y * 0.36f, -size.z * 0.2f), new Vector3(size.x * 0.46f, size.y * 0.42f, size.z * 0.2f), rockMaterial, renderers);
            AddVisualPart(root.transform, $"{objectName} Grass Shelf", new Vector3(size.x * 0.08f, size.y * 0.78f, size.z * 0.12f), new Vector3(size.x * 0.56f, 0.28f, size.z * 0.35f), grassMaterial, renderers);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, size.y * 0.35f, 0f);
            collider.size = new Vector3(size.x * 0.8f, size.y * 0.7f, size.z * 0.75f);
            RegisterCoverPoint($"{objectName} Cover", origin + root.transform.forward * -size.z * 0.55f);

            LODGroup lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.18f, renderers.ToArray()),
                new LOD(0.06f, new[] { renderers[0] })
            });
            lodGroup.RecalculateBounds();
        }

        private void CreateVegetationCluster(string objectName, Vector3 origin, float scale)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.position = origin;
            TagVisualModule(root, RuntimeVisualModuleKind.Vegetation, objectName);

            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 72f, 0f) * new Vector3(5f + (i % 2) * 2f, 0f, 0f);
                CreateTree($"{objectName} Tree {i + 1}", origin + offset, scale * (0.84f + i * 0.04f));
            }

            for (int i = 0; i < 7; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 51f, 0f) * new Vector3(3f + (i % 3) * 1.4f, 0f, 0f);
                CreateBush($"{objectName} Bush {i + 1}", origin + offset + Vector3.up * 0.25f, scale * 0.72f);
            }
        }

        private void CreateBush(string objectName, Vector3 position, float scale)
        {
            GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            runtimeObjects.Add(bush);
            bush.name = objectName;
            bush.layer = groundLayer;
            bush.isStatic = true;
            bush.transform.position = position;
            bush.transform.localScale = new Vector3(1.6f * scale, 0.72f * scale, 1.35f * scale);
            bush.GetComponent<Renderer>().material = foliageLightMaterial;
            TagVisualModule(bush, RuntimeVisualModuleKind.Vegetation, objectName);
            Collider collider = bush.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }

            if (scale > 0.55f)
            {
                GameObject flower = CreateWorldBlock($"{objectName} Flower Accent", position + new Vector3(0.15f, 0.42f, -0.2f), new Vector3(0.22f, 0.22f, 0.22f), flowerMaterial);
                TagVisualModule(flower, RuntimeVisualModuleKind.Vegetation, $"{objectName}_Flower");
            }
        }

        private Renderer AddVisualPart(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material, List<Renderer> renderers, PrimitiveType primitive = PrimitiveType.Cube)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = groundLayer;
            part.isStatic = true;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.material = material;
            renderers.Add(renderer);

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }

            return renderer;
        }

        private void BuildSilverRiver(Vector3 origin)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 segmentPosition = origin + new Vector3(Mathf.Sin(i * 0.8f) * 8f, 0.035f, i * 38f - 118f);
                GameObject segment = CreateWorldBlock($"Silver River Segment {i + 1}", segmentPosition, new Vector3(18f + (i % 3) * 2.5f, 0.05f, 44f), waterMaterial);
                segment.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i * 0.7f) * 10f, 0f);
            }

            CreateBridge("East Bridge", origin + new Vector3(0f, 0f, 22f));
            CreateBridge("South River Bridge", origin + new Vector3(-4f, 0f, -72f));

            for (int i = 0; i < 8; i++)
            {
                Vector3 rockPosition = origin + new Vector3((i % 2 == 0 ? -15f : 15f) + Mathf.Sin(i) * 3f, 0.45f, -105f + i * 30f);
                GameObject riverRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(riverRock);
                riverRock.name = $"Silver River Bank Rock {i + 1}";
                riverRock.layer = groundLayer;
                riverRock.transform.position = rockPosition;
                riverRock.transform.localScale = new Vector3(3.2f + i % 3, 0.9f, 2.4f + i % 2);
                riverRock.GetComponent<Renderer>().material = rockMaterial;
                RegisterCoverPoint($"{riverRock.name} Cover", rockPosition + Vector3.back * 2.8f);
            }
        }

        private void CreateBridge(string objectName, Vector3 origin)
        {
            CreateWorldBlock($"{objectName} Deck", origin + new Vector3(0f, 0.42f, 0f), new Vector3(36f, 0.7f, 9f), fenceMaterial);
            CreateWorldBlock($"{objectName} Road Surface", origin + new Vector3(0f, 0.82f, 0f), new Vector3(33f, 0.12f, 7f), roadMaterial);
            CreateWorldBlock($"{objectName} North Rail", origin + new Vector3(0f, 1.35f, 4.6f), new Vector3(37f, 0.55f, 0.45f), coverMaterial);
            CreateWorldBlock($"{objectName} South Rail", origin + new Vector3(0f, 1.35f, -4.6f), new Vector3(37f, 0.55f, 0.45f), coverMaterial);
            for (int i = 0; i < 5; i++)
            {
                float x = -16f + i * 8f;
                CreateWorldBlock($"{objectName} Support {i + 1}", origin + new Vector3(x, 0.85f, 0f), new Vector3(0.75f, 1.7f, 7.8f), coverMaterial);
            }
        }

        private void BuildNorthMountains(Vector3 origin)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(i * 16f - 56f, 3.2f + (i % 3), Mathf.Sin(i * 0.9f) * 16f);
                GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(mountain);
                mountain.name = $"North Mountain {i + 1}";
                mountain.layer = groundLayer;
                mountain.transform.position = origin + offset;
                mountain.transform.localScale = new Vector3(18f + i % 3 * 3f, 6.4f + i % 2 * 2.5f, 15f + i % 4 * 2f);
                mountain.GetComponent<Renderer>().material = rockMaterial;
                RegisterCoverPoint($"{mountain.name} Cover", mountain.transform.position + new Vector3(0f, -2.5f, -10f));
            }

            for (int i = 0; i < 14; i++)
            {
                Vector3 treePosition = origin + new Vector3(-62f + i * 9.5f, 0f, -24f + (i % 4) * 9f);
                CreateTree($"North Mountain Pine {i + 1}", treePosition, 0.92f + (i % 3) * 0.14f);
            }
        }

        private void BuildKestrelFactory(Vector3 origin)
        {
            CreateWorldBlock("Kestrel Factory Main Hall", origin + new Vector3(0f, 4.6f, 0f), new Vector3(34f, 9.2f, 24f), warehouseMaterial);
            CreateWorldBlock("Kestrel Factory Roof Monitor", origin + new Vector3(0f, 9.45f, 0f), new Vector3(20f, 1.2f, 10f), coverMaterial);
            BuildOpenInterior("Kestrel Factory Main Interior", origin + new Vector3(0f, 0f, -24f), new Vector3(30f, 0f, 16f));
            CreateWorldBlock("Kestrel Factory Office", origin + new Vector3(-28f, 2.9f, 8f), new Vector3(14f, 5.8f, 12f), buildingMaterial);
            BuildOpenInterior("Kestrel Factory Office Interior", origin + new Vector3(-28f, 0f, 20f), new Vector3(13f, 0f, 9f));
            CreateWorldBlock("Kestrel Factory Smokestack A", origin + new Vector3(22f, 7.5f, -8f), new Vector3(3f, 15f, 3f), vehicleAccentMaterial);
            CreateWorldBlock("Kestrel Factory Smokestack B", origin + new Vector3(28f, 6.2f, 6f), new Vector3(2.4f, 12.4f, 2.4f), vehicleAccentMaterial);
            CreateFenceLine("Kestrel Factory Perimeter North", origin + new Vector3(0f, 0.75f, 36f), 78f, true);
            CreateFenceLine("Kestrel Factory Perimeter East", origin + new Vector3(44f, 0.75f, 2f), 62f, false);

            for (int i = 0; i < 12; i++)
            {
                Vector3 offset = new Vector3((i % 4) * 8.5f - 14f, 1.1f + (i % 3) * 0.35f, (i / 4) * 9f + 20f);
                GameObject crate = CreateWorldBlock($"Kestrel Factory Cargo Stack {i + 1}", origin + offset, new Vector3(5.6f, 2.2f + (i % 2) * 0.9f, 4.6f), coverMaterial);
                crate.transform.rotation = Quaternion.Euler(0f, i * 13f, 0f);
            }
        }

        private void BuildWatchTower(string objectName, Vector3 origin, float yaw)
        {
            GameObject tower = CreateWorldBlock(objectName, origin + new Vector3(0f, 4.2f, 0f), new Vector3(4.4f, 8.4f, 4.4f), militaryMaterial);
            tower.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            CreateWorldBlock($"{objectName} Platform", origin + new Vector3(0f, 8.65f, 0f), new Vector3(7f, 0.45f, 7f), coverMaterial);
            CreateWorldBlock($"{objectName} Roof", origin + new Vector3(0f, 10.6f, 0f), new Vector3(7.8f, 0.42f, 7.8f), coverMaterial);
            CreateWorldBlock($"{objectName} Cabin Back", origin + new Vector3(0f, 9.55f, 3.3f), new Vector3(6.4f, 1.7f, 0.35f), buildingMaterial);
            CreateWorldBlock($"{objectName} Ladder", origin + new Vector3(-3.5f, 3.8f, -0.2f), new Vector3(0.34f, 7.2f, 0.34f), fenceMaterial);
            RegisterCoverPoint($"{objectName} Ground Cover", origin + new Vector3(3.8f, 0f, -3.8f));
        }

        private void CreateFuelStop(string objectName, Vector3 origin)
        {
            CreateWorldBlock($"{objectName} Awning", origin + new Vector3(0f, 3.1f, 0f), new Vector3(16f, 0.5f, 9f), coverMaterial);
            CreateWorldBlock($"{objectName} Pump A", origin + new Vector3(-4f, 0.9f, -1.5f), new Vector3(1.3f, 1.8f, 1f), vehicleAccentMaterial);
            CreateWorldBlock($"{objectName} Pump B", origin + new Vector3(4f, 0.9f, -1.5f), new Vector3(1.3f, 1.8f, 1f), vehicleAccentMaterial);
            CreateWorldBlock($"{objectName} Kiosk", origin + new Vector3(0f, 1.6f, 8f), new Vector3(8f, 3.2f, 5f), buildingMaterial);
            BuildOpenInterior($"{objectName} Kiosk Interior", origin + new Vector3(0f, 0f, 12.5f), new Vector3(7f, 0f, 4f));
        }

        private void CreateRoadSign(string objectName, Vector3 position, string label)
        {
            CreateWorldBlock($"{objectName} Post", position + Vector3.down * 0.35f, new Vector3(0.28f, 2.1f, 0.28f), fenceMaterial);
            GameObject sign = CreateWorldBlock(objectName, position + Vector3.up * 0.72f, new Vector3(4.2f, 1.15f, 0.18f), roadMaterial);
            GameObject labelObject = new GameObject($"{objectName} Label");
            runtimeObjects.Add(labelObject);
            labelObject.transform.SetParent(sign.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -0.11f);
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.font = LoadRuntimeFont();
            text.text = label;
            text.characterSize = 0.22f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.82f, 0.96f, 1f);
        }

        private void CreatePowerPole(string objectName, Vector3 position)
        {
            CreateWorldBlock($"{objectName} Pole", position + new Vector3(0f, 2.4f, 0f), new Vector3(0.42f, 4.8f, 0.42f), fenceMaterial);
            CreateWorldBlock($"{objectName} Cross Arm", position + new Vector3(0f, 4.55f, 0f), new Vector3(4.8f, 0.18f, 0.22f), fenceMaterial);
            CreateWorldBlock($"{objectName} Wire Hint", position + new Vector3(0f, 4.48f, 0f), new Vector3(6.8f, 0.04f, 0.08f), vehicleAccentMaterial);
        }

        private void CreateRadioMast(string objectName, Vector3 origin)
        {
            CreateWorldBlock($"{objectName} Base", origin + new Vector3(0f, 0.35f, 0f), new Vector3(5.5f, 0.7f, 5.5f), coverMaterial);
            for (int i = 0; i < 4; i++)
            {
                GameObject mast = CreateWorldBlock($"{objectName} Strut {i + 1}", origin + new Vector3(0f, 5.5f, 0f), new Vector3(0.25f, 11f, 0.25f), vehicleAccentMaterial);
                mast.transform.rotation = Quaternion.Euler(i % 2 == 0 ? 5f : -5f, i * 90f, i % 2 == 0 ? -5f : 5f);
            }

            CreateWorldBlock($"{objectName} Beacon", origin + new Vector3(0f, 11.4f, 0f), new Vector3(1.1f, 0.5f, 1.1f), windowMaterial);
        }

        private void CreateBusStop(string objectName, Vector3 origin, float yaw)
        {
            GameObject shelter = CreateWorldBlock($"{objectName} Back Wall", origin + new Vector3(0f, 1.25f, 0f), new Vector3(6.8f, 2.5f, 0.28f), buildingMaterial);
            shelter.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject roof = CreateWorldBlock($"{objectName} Roof", origin + new Vector3(0f, 2.65f, -0.4f), new Vector3(7.4f, 0.28f, 2.4f), coverMaterial);
            roof.transform.rotation = shelter.transform.rotation;
            GameObject bench = CreateWorldBlock($"{objectName} Bench", origin + new Vector3(0f, 0.55f, -0.75f), new Vector3(5.2f, 0.5f, 0.7f), fenceMaterial);
            bench.transform.rotation = shelter.transform.rotation;
        }

        private void CreateWatchBlind(string objectName, Vector3 origin)
        {
            CreateWorldBlock($"{objectName} Platform", origin + new Vector3(0f, 2.2f, 0f), new Vector3(4.4f, 0.28f, 4.4f), fenceMaterial);
            CreateWorldBlock($"{objectName} Cabin", origin + new Vector3(0f, 3.45f, 0f), new Vector3(3.6f, 2.2f, 3.6f), buildingMaterial);
            CreateWorldBlock($"{objectName} Window Slit", origin + new Vector3(0f, 3.7f, -1.85f), new Vector3(2.4f, 0.35f, 0.08f), windowMaterial);
            CreateWorldBlock($"{objectName} Ladder", origin + new Vector3(-2.35f, 1.2f, 0f), new Vector3(0.26f, 2.4f, 0.26f), fenceMaterial);
        }

        private void BuildHarborCoast(Vector3 origin)
        {
            CreateWorldBlock("Harbor Coast Sand Shelf", origin + new Vector3(0f, 0.03f, 28f), new Vector3(138f, 0.08f, 34f), coastMaterial);
            CreateWorldBlock("Harbor Coast Pier", origin + new Vector3(-22f, 0.32f, 42f), new Vector3(12f, 0.64f, 42f), fenceMaterial);
            CreateWorldBlock("Harbor Coast Dock Office", origin + new Vector3(24f, 2.35f, 11f), new Vector3(14f, 4.7f, 10f), warehouseMaterial);
            BuildOpenInterior("Harbor Dock Office Interior", origin + new Vector3(24f, 0f, -1f), new Vector3(13f, 0f, 9f));
            CreateFenceLine("Harbor Coast Rail", origin + new Vector3(-12f, 0.75f, 17f), 74f, true);
            CreateStreetProps("Harbor Coast", origin + new Vector3(-6f, 0f, 4f));
        }

        private void BuildOrionIndustrial(Vector3 origin)
        {
            CreateWorldBlock("Orion Industrial Warehouse A", origin + new Vector3(-18f, 4.2f, 0f), new Vector3(26f, 8.4f, 18f), warehouseMaterial);
            CreateWorldBlock("Orion Industrial Warehouse B", origin + new Vector3(20f, 3.6f, -18f), new Vector3(22f, 7.2f, 16f), warehouseMaterial);
            BuildOpenInterior("Orion Warehouse A Interior", origin + new Vector3(-18f, 0f, -18f), new Vector3(22f, 0f, 14f));
            for (int i = 0; i < 10; i++)
            {
                Vector3 offset = new Vector3((i % 5) * 7.6f - 15f, 1.1f + (i % 2) * 0.5f, (i / 5) * 11f + 14f);
                CreateWorldBlock($"Orion Industrial Cargo {i + 1}", origin + offset, new Vector3(5.2f, 2.2f + (i % 2) * 1f, 4.6f), coverMaterial);
            }

            CreateFenceLine("Orion Industrial Yard Fence", origin + new Vector3(0f, 0.72f, 36f), 78f, true);
        }

        private void BuildFortArray(Vector3 origin)
        {
            CreateWorldBlock("Fort Array Command Hall", origin + new Vector3(0f, 3.4f, 0f), new Vector3(24f, 6.8f, 16f), militaryMaterial);
            BuildOpenInterior("Fort Array Command Interior", origin + new Vector3(0f, 0f, -17f), new Vector3(22f, 0f, 12f));
            CreateWorldBlock("Fort Array Vehicle Bay", origin + new Vector3(32f, 2.6f, 12f), new Vector3(20f, 5.2f, 14f), warehouseMaterial);
            CreateWorldBlock("Fort Array Sandbag North", origin + new Vector3(0f, 0.75f, 31f), new Vector3(52f, 1.5f, 2.6f), coverMaterial);
            CreateWorldBlock("Fort Array Sandbag West", origin + new Vector3(-31f, 0.75f, 0f), new Vector3(2.6f, 1.5f, 52f), coverMaterial);
            CreateFenceLine("Fort Array Perimeter East", origin + new Vector3(44f, 0.75f, 0f), 82f, false);
        }

        private void BuildOldTown(Vector3 origin)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 offset = new Vector3((i % 3) * 13f - 13f, 2.3f, (i / 3) * 14f - 10f);
                CreateBuilding($"Old Town Row House {i + 1}", origin + offset, new Vector3(8.5f, 4.6f, 8.5f));
            }

            CreateWorldBlock("Old Town Clock Cover", origin + new Vector3(22f, 3f, 12f), new Vector3(4.2f, 6f, 4.2f), buildingMaterial);
            BuildOpenInterior("Old Town Corner Shop Interior", origin + new Vector3(-18f, 0f, 20f), new Vector3(11f, 0f, 10f));
            CreateStreetProps("Old Town", origin + new Vector3(0f, 0f, 8f));
        }

        private void BuildWestpineForest(Vector3 origin)
        {
            for (int i = 0; i < 26; i++)
            {
                float angle = i * 97.5f;
                float radius = 12f + (i % 8) * 6.5f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);
                CreateTree($"Westpine Tree {i + 1}", origin + offset, 0.9f + (i % 5) * 0.13f);
            }

            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 45f, 0f) * new Vector3(26f + (i % 3) * 6f, 0.5f, 0f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(rock);
                rock.name = $"Westpine Boulder {i + 1}";
                rock.layer = groundLayer;
                rock.transform.position = origin + offset;
                rock.transform.localScale = new Vector3(3.8f + i % 3, 1.2f, 3.1f + i % 2);
                rock.GetComponent<Renderer>().material = rockMaterial;
                RegisterCoverPoint($"{rock.name} Cover", rock.transform.position + Vector3.back * 3.4f);
            }
        }

        private void BuildCedarTown(Vector3 origin)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 offset = new Vector3(x * 14f - 14f, 2.35f, z * 15f - 8f);
                    CreateBuilding($"Cedar Town Home {x + z * 3 + 1}", origin + offset, new Vector3(9f + x % 2, 4.7f, 8.5f));
                }
            }

            CreateBuilding("Cedar Town Clinic", origin + new Vector3(21f, 2.8f, 16f), new Vector3(15f, 5.6f, 10f));
            BuildOpenInterior("Cedar Town Clinic Interior", origin + new Vector3(21f, 0f, 28f), new Vector3(14f, 0f, 10f));
            CreateFenceLine("Cedar Town Fence North", origin + new Vector3(0f, 0.7f, 29f), 52f, true);
            CreateFenceLine("Cedar Town Fence West", origin + new Vector3(-31f, 0.7f, 4f), 48f, false);
            CreateWorldBlock("Cedar Town Square Cover", origin + new Vector3(3f, 0.85f, 9f), new Vector3(10f, 1.7f, 3.2f), coverMaterial);
            CreateStreetProps("Cedar Town", origin + new Vector3(0f, 0f, 8f));
        }

        private void BuildCheckpoint(Vector3 origin)
        {
            CreateWorldBlock("Sentinel Checkpoint Gate", origin + new Vector3(0f, 2.3f, 0f), new Vector3(24f, 4.6f, 3f), militaryMaterial);
            CreateWorldBlock("Sentinel Checkpoint Bunker", origin + new Vector3(-18f, 1.8f, -10f), new Vector3(14f, 3.6f, 10f), militaryMaterial);
            CreateWorldBlock("Sentinel Checkpoint Garage", origin + new Vector3(18f, 2.2f, 12f), new Vector3(16f, 4.4f, 12f), warehouseMaterial);
            CreateFenceLine("Sentinel Checkpoint Fence East", origin + new Vector3(31f, 0.7f, 0f), 54f, false);
            CreateFenceLine("Sentinel Checkpoint Fence South", origin + new Vector3(0f, 0.7f, -31f), 60f, true);
            CreateWorldBlock("Sentinel Roadblock A", origin + new Vector3(-4f, 0.8f, 19f), new Vector3(11f, 1.6f, 2.4f), coverMaterial).transform.rotation = Quaternion.Euler(0f, 12f, 0f);
            CreateWorldBlock("Sentinel Roadblock B", origin + new Vector3(10f, 0.8f, -18f), new Vector3(11f, 1.6f, 2.4f), coverMaterial).transform.rotation = Quaternion.Euler(0f, -16f, 0f);
            CreateStreetProps("Sentinel", origin + new Vector3(0f, 0f, 6f));
        }

        private void BuildPineForest(Vector3 origin)
        {
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 137.5f;
                float radius = 10f + (i % 6) * 7f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);
                CreateTree($"Pine Forest Tree {i + 1}", origin + offset, 0.95f + (i % 4) * 0.16f);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 61f, 0f) * new Vector3(22f + (i % 2) * 8f, 0.45f, 0f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(rock);
                rock.name = $"Pine Forest Rock Cover {i + 1}";
                rock.layer = groundLayer;
                rock.transform.position = origin + offset;
                rock.transform.localScale = new Vector3(3.4f + i % 2, 1.1f, 2.8f + i % 3);
                rock.GetComponent<Renderer>().material = rockMaterial;
                RegisterCoverPoint($"{rock.name} Point", rock.transform.position + Vector3.back * 3f);
            }
        }

        private void BuildWarehouseYard(Vector3 origin)
        {
            CreateFenceLine("Iron Warehouse Yard Fence North", origin + new Vector3(0f, 0.75f, 31f), 70f, true);
            CreateFenceLine("Iron Warehouse Yard Fence East", origin + new Vector3(38f, 0.75f, 0f), 58f, false);
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3((i % 4) * 9f - 13.5f, 1.2f + (i % 2) * 0.45f, (i / 4) * 12f - 22f);
                GameObject crate = CreateWorldBlock($"Iron Warehouse Cargo Crate {i + 1}", origin + offset, new Vector3(6.5f, 2.4f + (i % 2) * 0.9f, 5.2f), coverMaterial);
                crate.transform.rotation = Quaternion.Euler(0f, i * 11f, 0f);
            }

            BuildOpenInterior("Iron Warehouse Interior Bay", origin + new Vector3(-2f, 0f, -34f), new Vector3(24f, 0f, 14f));
        }

        private void BuildOpenInterior(string objectName, Vector3 origin, Vector3 size)
        {
            CreateWorldBlock($"{objectName} Floor", origin + new Vector3(0f, 0.08f, 0f), new Vector3(size.x, 0.16f, size.z), roadMaterial);
            CreateWorldBlock($"{objectName} Back Wall", origin + new Vector3(0f, 1.45f, size.z * 0.5f), new Vector3(size.x, 2.9f, 0.45f), buildingMaterial);
            CreateWorldBlock($"{objectName} Left Wall", origin + new Vector3(-size.x * 0.5f, 1.2f, 0f), new Vector3(0.45f, 2.4f, size.z), buildingMaterial);
            CreateWorldBlock($"{objectName} Right Half Wall", origin + new Vector3(size.x * 0.5f, 0.82f, -size.z * 0.18f), new Vector3(0.42f, 1.64f, size.z * 0.52f), buildingMaterial);
            CreateWorldBlock($"{objectName} Desk Cover", origin + new Vector3(-size.x * 0.2f, 0.55f, -size.z * 0.08f), new Vector3(4.2f, 1.1f, 1.5f), coverMaterial);
            CreateWorldBlock($"{objectName} Shelf Cover", origin + new Vector3(size.x * 0.24f, 0.85f, size.z * 0.12f), new Vector3(1.6f, 1.7f, 4f), coverMaterial);
            CreateWorldBlock($"{objectName} Cot", origin + new Vector3(size.x * 0.22f, 0.32f, -size.z * 0.24f), new Vector3(3.4f, 0.62f, 1.5f), fenceMaterial);
            CreateWorldBlock($"{objectName} Locker", origin + new Vector3(-size.x * 0.32f, 1.05f, size.z * 0.22f), new Vector3(1.1f, 2.1f, 1f), vehicleAccentMaterial);
            CreateWorldBlock($"{objectName} Interior Window", origin + new Vector3(-size.x * 0.51f, 1.75f, size.z * 0.22f), new Vector3(0.08f, 0.62f, 1.4f), windowMaterial);
        }

        private void CreateStreetProps(string prefix, Vector3 origin)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 offset = new Vector3(i * 8f - 12f, 1.15f, (i % 2 == 0 ? 1f : -1f) * 5f);
                GameObject pole = CreateWorldBlock($"{prefix} Street Pole {i + 1}", origin + offset, new Vector3(0.35f, 2.3f, 0.35f), coverMaterial);
                CreateWorldBlock($"{pole.name} Lamp", pole.transform.position + new Vector3(0f, 1.35f, 0f), new Vector3(1.2f, 0.28f, 0.6f), windowMaterial);
            }

            CreateWorldBlock($"{prefix} Bench A", origin + new Vector3(-8f, 0.45f, -8f), new Vector3(4.8f, 0.9f, 1.1f), fenceMaterial);
            CreateWorldBlock($"{prefix} Bench B", origin + new Vector3(10f, 0.45f, 8f), new Vector3(4.8f, 0.9f, 1.1f), fenceMaterial);
        }

        private void CreateFenceLine(string objectName, Vector3 center, float length, bool horizontal)
        {
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 8f));
            for (int i = 0; i < posts; i++)
            {
                float offset = Mathf.Lerp(-length * 0.5f, length * 0.5f, posts == 1 ? 0.5f : i / (float)(posts - 1));
                Vector3 position = center + (horizontal ? new Vector3(offset, 0f, 0f) : new Vector3(0f, 0f, offset));
                CreateWorldBlock($"{objectName} Post {i + 1}", position, new Vector3(0.5f, 1.4f, 0.5f), fenceMaterial);
            }

            Vector3 railScale = horizontal ? new Vector3(length, 0.22f, 0.32f) : new Vector3(0.32f, 0.22f, length);
            CreateWorldBlock($"{objectName} Rail", center + Vector3.up * 0.35f, railScale, fenceMaterial);
        }

        private void RegisterNamedLocation(string locationName, Vector3 position)
        {
            runtimeLocationNames.Add(locationName);
            runtimeLocationPositions.Add(position);

            GameObject marker = new GameObject($"{locationName} Location Marker");
            runtimeObjects.Add(marker);
            marker.transform.position = position + Vector3.up * 0.1f;
        }

        private void BuildSmallVillage(Vector3 origin)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3((i % 2) * 14f - 7f, 2.1f, (i / 2) * 14f - 14f);
                CreateBuilding($"Village House {i + 1}", origin + offset, new Vector3(9f, 4.2f, 8f));
            }

            CreateWorldBlock("Village Market Cover", origin + new Vector3(10f, 1f, 18f), new Vector3(14f, 2f, 3f), coverMaterial);
            CreateWorldBlock("Village Well Cover", origin + new Vector3(-11f, 0.9f, 18f), new Vector3(4f, 1.8f, 4f), rockMaterial);
        }

        private void BuildMilitaryBase(Vector3 origin)
        {
            CreateWorldBlock("Military Base Barracks", origin + new Vector3(-10f, 2.5f, -8f), new Vector3(18f, 5f, 10f), militaryMaterial);
            CreateWorldBlock("Military Base Command", origin + new Vector3(12f, 3f, 8f), new Vector3(16f, 6f, 14f), militaryMaterial);
            CreateWorldBlock("Military Sandbag Line A", origin + new Vector3(0f, 0.7f, 23f), new Vector3(32f, 1.4f, 2.4f), coverMaterial);
            CreateWorldBlock("Military Sandbag Line B", origin + new Vector3(-23f, 0.7f, 0f), new Vector3(2.4f, 1.4f, 32f), coverMaterial);

            for (int i = 0; i < 4; i++)
            {
                Vector3 towerOffset = Quaternion.Euler(0f, i * 90f, 0f) * new Vector3(24f, 3f, 24f);
                GameObject tower = CreateWorldBlock($"Military Watch Tower {i + 1}", origin + towerOffset, new Vector3(4f, 6f, 4f), militaryMaterial);
                CreateWorldBlock($"{tower.name} Rail", tower.transform.position + Vector3.up * 3.3f, new Vector3(5.3f, 0.4f, 5.3f), coverMaterial);
            }
        }

        private void BuildWarehouseCompound(Vector3 origin)
        {
            GameObject warehouse = CreateWorldBlock("North Warehouse", origin + new Vector3(0f, 4f, 0f), new Vector3(28f, 8f, 18f), warehouseMaterial);
            CreateWorldBlock("North Warehouse Roof", warehouse.transform.position + Vector3.up * 4.4f, new Vector3(30f, 0.6f, 20f), coverMaterial);
            CreateWorldBlock("Warehouse Cargo Stack A", origin + new Vector3(-18f, 1.2f, 12f), new Vector3(8f, 2.4f, 6f), coverMaterial);
            CreateWorldBlock("Warehouse Cargo Stack B", origin + new Vector3(18f, 1.5f, -12f), new Vector3(7f, 3f, 7f), coverMaterial);
            CreateWorldBlock("Warehouse Loading Ramp", origin + new Vector3(0f, 0.5f, -14f), new Vector3(18f, 1f, 4f), roadMaterial);
        }

        private void BuildNatureSet()
        {
            Vector3[] treePositions =
            {
                new Vector3(-84f, 0f, -38f), new Vector3(-72f, 0f, -58f), new Vector3(-52f, 0f, -78f),
                new Vector3(-18f, 0f, -84f), new Vector3(18f, 0f, -82f), new Vector3(76f, 0f, 58f),
                new Vector3(88f, 0f, 18f), new Vector3(78f, 0f, -74f), new Vector3(-92f, 0f, 74f),
                new Vector3(12f, 0f, 86f), new Vector3(52f, 0f, 84f), new Vector3(-38f, 0f, 82f)
            };

            for (int i = 0; i < treePositions.Length; i++)
            {
                CreateTree($"Low Poly Tree {i + 1}", treePositions[i], 1f + (i % 3) * 0.18f);
            }

            Vector3[] rockPositions =
            {
                new Vector3(-62f, 0.45f, 8f), new Vector3(-34f, 0.5f, -52f), new Vector3(28f, 0.5f, -62f),
                new Vector3(62f, 0.45f, 20f), new Vector3(6f, 0.45f, 62f), new Vector3(-76f, 0.45f, 30f)
            };

            for (int i = 0; i < rockPositions.Length; i++)
            {
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                runtimeObjects.Add(rock);
                rock.name = $"Low Poly Rock {i + 1}";
                rock.layer = groundLayer;
                rock.transform.position = rockPositions[i];
                rock.transform.localScale = new Vector3(3f + i % 3, 0.9f + (i % 2) * 0.4f, 2.4f + i % 2);
                rock.GetComponent<Renderer>().material = rockMaterial;
                RegisterCoverPoint($"{rock.name} Cover", rock.transform.position + new Vector3(0f, 0f, -3f));
            }
        }

        private void CreateTree(string objectName, Vector3 position, float scale)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.position = position;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            runtimeObjects.Add(trunk);
            trunk.name = $"{objectName} Trunk";
            trunk.layer = groundLayer;
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = Vector3.up * (1.4f * scale);
            trunk.transform.localScale = new Vector3(0.45f * scale, 1.4f * scale, 0.45f * scale);
            Renderer trunkRenderer = trunk.GetComponent<Renderer>();
            trunkRenderer.material = treeTrunkMaterial;

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            runtimeObjects.Add(canopy);
            canopy.name = $"{objectName} Canopy";
            canopy.layer = groundLayer;
            canopy.transform.SetParent(root.transform, false);
            canopy.transform.localPosition = Vector3.up * (3.6f * scale);
            canopy.transform.localScale = new Vector3(2.7f * scale, 2.4f * scale, 2.7f * scale);
            Renderer canopyRenderer = canopy.GetComponent<Renderer>();
            canopyRenderer.material = treeMaterial;
            TagVisualModule(root, RuntimeVisualModuleKind.Vegetation, objectName);

            LODGroup lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.18f, new[] { trunkRenderer, canopyRenderer }),
                new LOD(0.06f, new[] { canopyRenderer })
            });
            lodGroup.RecalculateBounds();

            RegisterCoverPoint($"{objectName} Cover", position + new Vector3(1.8f * scale, 0f, -1.8f * scale));
        }

        private void TagVisualModule(GameObject target, RuntimeVisualModuleKind kind, string moduleId)
        {
            if (target == null)
            {
                return;
            }

            RuntimeVisualModule module = target.GetComponent<RuntimeVisualModule>();
            if (module == null)
            {
                module = target.AddComponent<RuntimeVisualModule>();
            }

            module.Configure(kind, moduleId);
        }

        private GameObject CreateWorldBlock(string objectName, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runtimeObjects.Add(block);
            block.name = objectName;
            block.layer = groundLayer;
            block.isStatic = true;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().material = material;
            if (scale.y >= 1f && !objectName.Contains("Road") && !objectName.Contains("Door") && !objectName.Contains("Terrain"))
            {
                RegisterCoverPoint($"{objectName} Cover", position + new Vector3(0f, 0f, -Mathf.Max(2f, scale.z * 0.55f)));
            }

            return block;
        }

        private void RegisterCoverPoint(string objectName, Vector3 position)
        {
            GameObject point = new GameObject(objectName);
            runtimeObjects.Add(point);
            point.transform.position = new Vector3(position.x, 0.1f, position.z);
            runtimeCoverPoints.Add(point.transform);
        }

        private void CreateBuilding(string objectName, Vector3 position, Vector3 scale)
        {
            GameObject building = CreateWorldBlock(objectName, position, scale, buildingMaterial);

            GameObject roof = CreateWorldBlock($"{objectName} Roof", position + Vector3.up * (scale.y * 0.55f), new Vector3(scale.x + 1.2f, 0.45f, scale.z + 1.2f), coverMaterial);
            roof.transform.rotation = building.transform.rotation;

            GameObject door = CreateWorldBlock($"{objectName} Door Marker", position + new Vector3(0f, -scale.y * 0.28f, -scale.z * 0.51f), new Vector3(2.2f, scale.y * 0.42f, 0.12f), roadMaterial);
            door.transform.rotation = building.transform.rotation;

            GameObject sideCover = CreateWorldBlock($"{objectName} Side Cover", position + new Vector3(scale.x * 0.58f, -scale.y * 0.25f, 0f), new Vector3(1.2f, scale.y * 0.45f, scale.z * 0.55f), coverMaterial);
            sideCover.transform.rotation = building.transform.rotation;

            CreateWorldBlock($"{objectName} Front Window L", position + new Vector3(-scale.x * 0.24f, scale.y * 0.08f, -scale.z * 0.515f), new Vector3(1.35f, 0.72f, 0.1f), windowMaterial);
            CreateWorldBlock($"{objectName} Front Window R", position + new Vector3(scale.x * 0.24f, scale.y * 0.08f, -scale.z * 0.515f), new Vector3(1.35f, 0.72f, 0.1f), windowMaterial);
            CreateWorldBlock($"{objectName} Side Window", position + new Vector3(-scale.x * 0.515f, scale.y * 0.08f, 0f), new Vector3(0.1f, 0.72f, 1.25f), windowMaterial);
            BuildOpenInterior($"{objectName} Interior", position + new Vector3(0f, -scale.y * 0.5f, scale.z * 0.74f), new Vector3(Mathf.Max(6f, scale.x - 1.5f), 0f, Mathf.Max(5f, scale.z * 0.72f)));
        }

        private Camera BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            runtimeObjects.Add(cameraObject);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.56f, 0.70f, 0.80f);
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 460f;
            UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            float[] cullDistances = new float[32];
            cullDistances[lootLayer] = 115f;
            cullDistances[zoneLayer] = 430f;
            camera.layerCullDistances = cullDistances;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void ConfigureInputSystemUIModule(InputSystemUIInputModule inputModule)
        {
            if (inputModule == null)
            {
                return;
            }

            try
            {
                System.Reflection.MethodInfo assignDefaultActions = typeof(InputSystemUIInputModule).GetMethod(
                    "AssignDefaultActions",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                assignDefaultActions?.Invoke(inputModule, null);
            }
            catch
            {
                // Some Input System package versions create defaults internally.
            }
        }

        private RuntimeUIRefs BuildUI()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            runtimeObjects.Add(eventSystemObject);
            EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
            EventSystem.current = eventSystem;
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            ConfigureInputSystemUIModule(inputModule);

            GameObject canvasObject = new GameObject("Mobile UI Canvas");
            runtimeObjects.Add(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = LoadRuntimeFont();

            GameObject mainMenuPanel = CreatePanel("MainMenuPanel", canvasObject.transform, new Color(0.05f, 0.07f, 0.08f, 0.92f));
            GameObject hudPanel = CreatePanel("HUDPanel", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
            Image hudPanelImage = hudPanel.GetComponent<Image>();
            if (hudPanelImage != null)
            {
                hudPanelImage.raycastTarget = false;
            }
            GameObject gameOverPanel = CreatePanel("GameOverPanel", canvasObject.transform, new Color(0.08f, 0.04f, 0.04f, 0.92f));
            GameObject victoryPanel = CreatePanel("VictoryPanel", canvasObject.transform, new Color(0.04f, 0.08f, 0.05f, 0.92f));
            GameObject pausePanel = CreatePanel("PausePanel", canvasObject.transform, new Color(0.03f, 0.04f, 0.045f, 0.88f));
            GameObject settingsPanel = CreatePanel("SettingsPanel", canvasObject.transform, new Color(0.035f, 0.045f, 0.052f, 0.92f));
            GameObject inventoryPanel = CreatePanel("InventoryPanel", canvasObject.transform, new Color(0.025f, 0.032f, 0.036f, 0.92f));
            GameObject matchSummaryPanel = CreatePanel("MatchSummaryPanel", canvasObject.transform, new Color(0.025f, 0.035f, 0.04f, 0.94f));
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            matchSummaryPanel.SetActive(false);

            Text title = CreateText("Title", mainMenuPanel.transform, font, "BattleZone Mobile", 64, TextAnchor.MiddleCenter, new Vector2(0f, 120f), new Vector2(900f, 110f));
            title.color = new Color(0.93f, 0.98f, 1f);
            Button startButton = CreateButton("StartMatchButton", mainMenuPanel.transform, font, "START MATCH", new Vector2(0f, -30f), new Vector2(360f, 92f), new Color(0.12f, 0.55f, 0.42f));
            Text subtitle = CreateText("Subtitle", mainMenuPanel.transform, font, "Milestone 19 Full Drop Experience", 26, TextAnchor.MiddleCenter, new Vector2(0f, -140f), new Vector2(980f, 60f));
            subtitle.color = new Color(0.78f, 0.84f, 0.86f);

            Text gameOverTitle = CreateText("GameOverTitle", gameOverPanel.transform, font, "GAME OVER", 68, TextAnchor.MiddleCenter, new Vector2(0f, 80f), new Vector2(760f, 110f));
            gameOverTitle.color = new Color(1f, 0.5f, 0.45f);
            Button gameOverRestartButton = CreateButton("GameOverRestartButton", gameOverPanel.transform, font, "RETRY", new Vector2(0f, -70f), new Vector2(300f, 86f), new Color(0.55f, 0.2f, 0.18f));

            Text victoryTitle = CreateText("VictoryTitle", victoryPanel.transform, font, "VICTORY", 72, TextAnchor.MiddleCenter, new Vector2(0f, 80f), new Vector2(760f, 110f));
            victoryTitle.color = new Color(0.64f, 1f, 0.7f);
            Button victoryRestartButton = CreateButton("VictoryRestartButton", victoryPanel.transform, font, "PLAY AGAIN", new Vector2(0f, -70f), new Vector2(340f, 86f), new Color(0.16f, 0.52f, 0.27f));

            Text summaryTitle = CreateText("MatchSummaryTitle", matchSummaryPanel.transform, font, "MATCH SUMMARY", 58, TextAnchor.MiddleCenter, new Vector2(0f, 170f), new Vector2(760f, 88f));
            summaryTitle.color = new Color(0.9f, 0.98f, 1f);
            Text matchSummaryText = CreateText("MatchSummaryText", matchSummaryPanel.transform, font, "Result pending", 34, TextAnchor.MiddleCenter, new Vector2(0f, 22f), new Vector2(720f, 250f));
            matchSummaryText.color = new Color(0.82f, 0.92f, 0.95f);
            Button closeSummaryButton = CreateButton("CloseSummaryButton", matchSummaryPanel.transform, font, "CLOSE", new Vector2(0f, -178f), new Vector2(260f, 72f), new Color(0.16f, 0.28f, 0.42f));

            Text pauseTitle = CreateText("PauseTitle", pausePanel.transform, font, "PAUSED", 64, TextAnchor.MiddleCenter, new Vector2(0f, 120f), new Vector2(560f, 100f));
            pauseTitle.color = new Color(0.9f, 0.96f, 1f);
            Button resumeButton = CreateButton("ResumeButton", pausePanel.transform, font, "RESUME", new Vector2(0f, 5f), new Vector2(320f, 78f), new Color(0.12f, 0.48f, 0.42f));
            Button settingsButton = CreateButton("SettingsButton", pausePanel.transform, font, "SETTINGS", new Vector2(0f, -95f), new Vector2(320f, 78f), new Color(0.16f, 0.28f, 0.42f));

            Text settingsTitle = CreateText("SettingsTitle", settingsPanel.transform, font, "SETTINGS", 58, TextAnchor.MiddleCenter, new Vector2(0f, 208f), new Vector2(620f, 90f));
            settingsTitle.color = new Color(0.9f, 0.96f, 1f);
            Text sensitivityLabel = CreateText("SensitivityLabel", settingsPanel.transform, font, "CAMERA SENSITIVITY", 26, TextAnchor.MiddleCenter, new Vector2(0f, 134f), new Vector2(540f, 48f));
            sensitivityLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider sensitivitySlider = CreateSlider("SensitivitySlider", settingsPanel.transform, new Vector2(0f, 92f), new Vector2(520f, 38f));
            sensitivitySlider.value = 0.5f;
            Text aimSensitivityLabel = CreateText("AimSensitivityLabel", settingsPanel.transform, font, "ADS SENSITIVITY", 26, TextAnchor.MiddleCenter, new Vector2(0f, 42f), new Vector2(540f, 48f));
            aimSensitivityLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider aimSensitivitySlider = CreateSlider("AimSensitivitySlider", settingsPanel.transform, new Vector2(0f, 0f), new Vector2(520f, 38f));
            aimSensitivitySlider.value = 0.5f;
            Text scopeSensitivityLabel = CreateText("ScopeSensitivityLabel", settingsPanel.transform, font, "SCOPE SENSITIVITY", 26, TextAnchor.MiddleCenter, new Vector2(0f, -50f), new Vector2(540f, 48f));
            scopeSensitivityLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider scopeSensitivitySlider = CreateSlider("ScopeSensitivitySlider", settingsPanel.transform, new Vector2(0f, -92f), new Vector2(520f, 38f));
            scopeSensitivitySlider.value = 0.5f;
            Text graphicsLabel = CreateText("GraphicsLabel", settingsPanel.transform, font, "GRAPHICS QUALITY", 26, TextAnchor.MiddleCenter, new Vector2(0f, -142f), new Vector2(540f, 48f));
            graphicsLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider graphicsSlider = CreateSlider("GraphicsSlider", settingsPanel.transform, new Vector2(0f, -184f), new Vector2(520f, 38f));
            graphicsSlider.value = 0.62f;
            Text audioLabel = CreateText("AudioLabel", settingsPanel.transform, font, "AUDIO VOLUME", 26, TextAnchor.MiddleCenter, new Vector2(0f, -234f), new Vector2(540f, 48f));
            audioLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider audioSlider = CreateSlider("AudioSlider", settingsPanel.transform, new Vector2(0f, -276f), new Vector2(520f, 38f));
            audioSlider.value = 0.85f;
            Text buttonScaleLabel = CreateText("ButtonScaleLabel", settingsPanel.transform, font, "BUTTON SCALE", 26, TextAnchor.MiddleCenter, new Vector2(0f, -326f), new Vector2(540f, 48f));
            buttonScaleLabel.color = new Color(0.78f, 0.86f, 0.9f);
            Slider buttonScaleSlider = CreateSlider("ButtonScaleSlider", settingsPanel.transform, new Vector2(0f, -368f), new Vector2(520f, 38f));
            buttonScaleSlider.value = 0.5f;
            Button closeSettingsButton = CreateButton("CloseSettingsButton", settingsPanel.transform, font, "CLOSE", new Vector2(0f, -452f), new Vector2(260f, 68f), new Color(0.16f, 0.28f, 0.42f));

            Text inventoryTitle = CreateText("InventoryTitle", inventoryPanel.transform, font, "BACKPACK", 54, TextAnchor.MiddleCenter, new Vector2(0f, 222f), new Vector2(640f, 72f));
            inventoryTitle.color = new Color(0.9f, 0.96f, 1f);
            Text inventoryDetails = CreateText("InventoryDetails", inventoryPanel.transform, font, "Backpack 0/120", 30, TextAnchor.UpperLeft, new Vector2(0f, 10f), new Vector2(620f, 360f));
            inventoryDetails.color = new Color(0.82f, 0.9f, 0.92f);
            CreateInventorySlot("InventoryDragSlotPrimary", inventoryPanel.transform, font, "Primary", new Vector2(-460f, 108f), 0);
            CreateInventorySlot("InventoryDragSlotSecondary", inventoryPanel.transform, font, "Secondary", new Vector2(-460f, 22f), 1);
            CreateInventorySlot("InventoryDragSlotMelee", inventoryPanel.transform, font, "Melee", new Vector2(-460f, -64f), 2);
            CreateInventorySlot("InventoryDragSlotOptic", inventoryPanel.transform, font, "Optic", new Vector2(460f, 108f), 3);
            CreateInventorySlot("InventoryDragSlotMuzzle", inventoryPanel.transform, font, "Muzzle", new Vector2(460f, 22f), 4);
            CreateInventorySlot("InventoryDragSlotMagazine", inventoryPanel.transform, font, "Magazine", new Vector2(460f, -64f), 5);
            Button closeInventoryButton = CreateButton("CloseInventoryButton", inventoryPanel.transform, font, "CLOSE", new Vector2(0f, -235f), new Vector2(260f, 72f), new Color(0.16f, 0.28f, 0.42f));

            GameObject lookAreaObject = CreateUIObject("RightLookArea", hudPanel.transform, new Vector2(0f, 0f), new Vector2(1920f, 1080f));
            Image lookAreaImage = lookAreaObject.AddComponent<Image>();
            lookAreaImage.color = new Color(0f, 0f, 0f, 0.01f);
            MobileLookArea lookArea = lookAreaObject.AddComponent<MobileLookArea>();
            lookAreaObject.transform.SetAsFirstSibling();

            FloatingJoystick joystick = CreateJoystick(hudPanel.transform);
            Button jumpButton = CreateButton("JumpButton", hudPanel.transform, font, "JUMP", new Vector2(725f, -285f), new Vector2(150f, 78f), new Color(0.16f, 0.28f, 0.38f));
            Button crouchButton = CreateButton("CrouchButton", hudPanel.transform, font, "CROUCH", new Vector2(560f, -385f), new Vector2(178f, 72f), new Color(0.16f, 0.28f, 0.38f));
            Button reloadButton = CreateButton("ReloadButton", hudPanel.transform, font, "RELOAD", new Vector2(760f, -390f), new Vector2(178f, 72f), new Color(0.16f, 0.28f, 0.38f));
            Button medkitButton = CreateButton("MedkitButton", hudPanel.transform, font, "MEDKIT", new Vector2(355f, -390f), new Vector2(178f, 72f), new Color(0.2f, 0.42f, 0.32f));
            Button switchButton = CreateButton("SwitchWeaponButton", hudPanel.transform, font, "SWAP", new Vector2(355f, -292f), new Vector2(150f, 72f), new Color(0.18f, 0.32f, 0.44f));
            Button shoulderButton = CreateButton("ShoulderButton", hudPanel.transform, font, "SHLD", new Vector2(540f, -292f), new Vector2(150f, 72f), new Color(0.18f, 0.28f, 0.40f));
            Button proneButton = CreateButton("ProneButton", hudPanel.transform, font, "PRONE", new Vector2(555f, -470f), new Vector2(160f, 66f), new Color(0.13f, 0.23f, 0.32f));
            Button driveButton = CreateButton("DriveButton", hudPanel.transform, font, "DRIVE", new Vector2(170f, -390f), new Vector2(150f, 72f), new Color(0.18f, 0.38f, 0.34f));
            Button inventoryButton = CreateButton("InventoryButton", hudPanel.transform, font, "PACK", new Vector2(-690f, 214f), new Vector2(150f, 64f), new Color(0.12f, 0.18f, 0.22f));
            Button pauseButton = CreateButton("PauseButton", hudPanel.transform, font, "II", new Vector2(870f, 480f), new Vector2(82f, 64f), new Color(0.08f, 0.12f, 0.15f));

            MobileHoldButton fireButton = CreateHoldButton("FireButton", hudPanel.transform, font, "FIRE", new Vector2(815f, -185f), new Vector2(170f, 170f), new Color(0.62f, 0.17f, 0.12f));
            MobileHoldButton sprintButton = CreateHoldButton("SprintButton", hudPanel.transform, font, "SPRINT", new Vector2(-610f, -370f), new Vector2(180f, 76f), new Color(0.17f, 0.38f, 0.52f));
            MobileHoldButton aimButton = CreateHoldButton("AimButton", hudPanel.transform, font, "AIM", new Vector2(615f, -185f), new Vector2(150f, 120f), new Color(0.18f, 0.30f, 0.42f));
            MobileHoldButton throttleButton = CreateHoldButton("ThrottleButton", hudPanel.transform, font, "GAS", new Vector2(650f, -470f), new Vector2(124f, 64f), new Color(0.16f, 0.44f, 0.28f));
            MobileHoldButton brakeButton = CreateHoldButton("BrakeButton", hudPanel.transform, font, "BRK", new Vector2(795f, -470f), new Vector2(124f, 64f), new Color(0.44f, 0.18f, 0.16f));

            Text crosshair = CreateText("Crosshair", hudPanel.transform, font, "+", 42, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(80f, 80f));
            crosshair.color = Color.white;
            crosshair.raycastTarget = false;
            Image crosshairTop = CreateImage("M12CrosshairTop", crosshair.transform, new Vector2(0f, 24f), new Vector2(3f, 18f), new Color(0.85f, 1f, 0.95f, 0.86f));
            Image crosshairBottom = CreateImage("M12CrosshairBottom", crosshair.transform, new Vector2(0f, -24f), new Vector2(3f, 18f), new Color(0.85f, 1f, 0.95f, 0.86f));
            Image crosshairLeft = CreateImage("M12CrosshairLeft", crosshair.transform, new Vector2(-24f, 0f), new Vector2(18f, 3f), new Color(0.85f, 1f, 0.95f, 0.86f));
            Image crosshairRight = CreateImage("M12CrosshairRight", crosshair.transform, new Vector2(24f, 0f), new Vector2(18f, 3f), new Color(0.85f, 1f, 0.95f, 0.86f));
            crosshairTop.raycastTarget = false;
            crosshairBottom.raycastTarget = false;
            crosshairLeft.raycastTarget = false;
            crosshairRight.raycastTarget = false;
            Text hitMarkerText = CreateText("HitMarker", hudPanel.transform, font, "x", 48, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(80f, 80f));
            hitMarkerText.color = new Color(1f, 0.86f, 0.25f, 0f);
            HitMarkerUI hitMarker = hitMarkerText.gameObject.AddComponent<HitMarkerUI>();
            hitMarker.Configure(hitMarkerText);
            Text headshotMarkerText = CreateText("HeadshotMarker", hudPanel.transform, font, "HEADSHOT", 28, TextAnchor.MiddleCenter, new Vector2(0f, 58f), new Vector2(220f, 42f));
            headshotMarkerText.color = new Color(1f, 0.22f, 0.08f, 0f);
            HitMarkerUI headshotMarker = headshotMarkerText.gameObject.AddComponent<HitMarkerUI>();
            headshotMarker.Configure(headshotMarkerText);

            Text compassText = CreateText("CompassText", hudPanel.transform, font, "N 000", 30, TextAnchor.MiddleCenter, new Vector2(0f, 370f), new Vector2(420f, 44f));
            compassText.color = new Color(0.93f, 0.98f, 1f);
            Text matchTimerText = CreateText("MatchTimerText", hudPanel.transform, font, "00:00", 30, TextAnchor.MiddleCenter, new Vector2(0f, 524f), new Vector2(220f, 44f));
            matchTimerText.color = new Color(0.98f, 0.95f, 0.75f);
            Text killFeedText = CreateText("KillFeedText", hudPanel.transform, font, "", 25, TextAnchor.MiddleRight, new Vector2(640f, 336f), new Vector2(480f, 58f));
            killFeedText.color = new Color(1f, 0.86f, 0.54f);
            killFeedText.enabled = false;
            Text matchAnnouncementText = CreateText("MatchAnnouncementText", hudPanel.transform, font, "", 44, TextAnchor.MiddleCenter, new Vector2(0f, 216f), new Vector2(760f, 76f));
            matchAnnouncementText.color = new Color(0.86f, 0.96f, 1f, 0.95f);
            matchAnnouncementText.enabled = false;
            Text matchPhaseText = CreateText("MatchPhaseText", hudPanel.transform, font, "LOBBY", 26, TextAnchor.MiddleCenter, new Vector2(0f, 338f), new Vector2(360f, 42f));
            matchPhaseText.color = new Color(0.82f, 0.98f, 1f, 0.9f);
            Text flightPathText = CreateText("FlightPathText", hudPanel.transform, font, "", 22, TextAnchor.MiddleCenter, new Vector2(0f, 302f), new Vector2(560f, 38f));
            flightPathText.color = new Color(1f, 0.92f, 0.56f, 0.9f);
            flightPathText.enabled = false;

            Image minimapBack = CreateImage("MiniMapPanel", hudPanel.transform, new Vector2(-790f, 338f), new Vector2(218f, 218f), new Color(0.03f, 0.05f, 0.055f, 0.72f));
            Image minimapZone = CreateImage("MiniMapZoneRing", minimapBack.transform, Vector2.zero, new Vector2(150f, 150f), new Color(0.16f, 0.55f, 1f, 0.08f));
            Image minimapNextZone = CreateImage("MiniMapNextZoneRing", minimapBack.transform, Vector2.zero, new Vector2(112f, 112f), new Color(0.9f, 0.95f, 0.34f, 0.055f));
            Image minimapRiver = CreateImage("MiniMapRiverLine", minimapBack.transform, new Vector2(28f, 0f), new Vector2(22f, 184f), new Color(0.12f, 0.48f, 0.7f, 0.72f));
            minimapRiver.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -11f);
            Image minimapFlightPath = CreateImage("MiniMapFlightPath", minimapBack.transform, Vector2.zero, new Vector2(120f, 4f), new Color(0.96f, 0.98f, 1f, 0.72f));
            minimapFlightPath.raycastTarget = false;
            minimapFlightPath.gameObject.SetActive(false);
            CreateText("MiniMapTownBlip", minimapBack.transform, font, "T", 14, TextAnchor.MiddleCenter, new Vector2(-54f, 24f), new Vector2(24f, 24f)).color = new Color(1f, 0.9f, 0.5f);
            CreateText("MiniMapFactoryBlip", minimapBack.transform, font, "F", 14, TextAnchor.MiddleCenter, new Vector2(62f, -34f), new Vector2(24f, 24f)).color = new Color(1f, 0.72f, 0.4f);
            CreateText("MiniMapBaseBlip", minimapBack.transform, font, "B", 14, TextAnchor.MiddleCenter, new Vector2(48f, -74f), new Vector2(24f, 24f)).color = new Color(0.65f, 0.9f, 1f);
            Outline minimapZoneOutline = minimapZone.gameObject.AddComponent<Outline>();
            minimapZoneOutline.effectColor = new Color(0.16f, 0.72f, 1f, 0.8f);
            minimapZoneOutline.effectDistance = new Vector2(2f, -2f);
            Outline minimapNextZoneOutline = minimapNextZone.gameObject.AddComponent<Outline>();
            minimapNextZoneOutline.effectColor = new Color(1f, 0.94f, 0.32f, 0.72f);
            minimapNextZoneOutline.effectDistance = new Vector2(2f, -2f);
            Text minimapArrow = CreateText("MiniMapPlayerArrow", minimapBack.transform, font, "^", 38, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(60f, 60f));
            minimapArrow.color = new Color(1f, 0.95f, 0.55f);
            Text minimapLabel = CreateText("MiniMapLabel", minimapBack.transform, font, "SAFE", 20, TextAnchor.MiddleCenter, new Vector2(0f, -88f), new Vector2(170f, 32f));
            minimapLabel.color = new Color(0.78f, 0.95f, 1f);
            Text minimapLocation = CreateText("MiniMapLocation", minimapBack.transform, font, "Open Range", 18, TextAnchor.MiddleCenter, new Vector2(0f, 84f), new Vector2(188f, 30f));
            minimapLocation.color = new Color(1f, 0.95f, 0.62f);
            RuntimeHudTelemetry hudTelemetry = hudPanel.AddComponent<RuntimeHudTelemetry>();

            Text healthText = CreateText("HealthText", hudPanel.transform, font, "HP 100/100", 30, TextAnchor.MiddleLeft, new Vector2(-760f, 474f), new Vector2(280f, 50f));
            Image statusPanel = CreateImage("M12 Tactical Status Panel", hudPanel.transform, new Vector2(-623f, 448f), new Vector2(388f, 116f), new Color(0.02f, 0.035f, 0.04f, 0.62f));
            statusPanel.raycastTarget = false;
            statusPanel.transform.SetSiblingIndex(Mathf.Max(0, healthText.transform.GetSiblingIndex() - 1));
            Image healthBack = CreateImage("HealthBack", hudPanel.transform, new Vector2(-600f, 474f), new Vector2(240f, 24f), new Color(0f, 0f, 0f, 0.5f));
            Image healthFill = CreateImage("HealthFill", healthBack.transform, Vector2.zero, new Vector2(240f, 24f), new Color(0.18f, 0.82f, 0.36f));
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = 0;
            Text armorText = CreateText("ArmorText", hudPanel.transform, font, "AR 50/50", 26, TextAnchor.MiddleLeft, new Vector2(-760f, 442f), new Vector2(280f, 44f));
            Image armorBack = CreateImage("ArmorBack", hudPanel.transform, new Vector2(-600f, 442f), new Vector2(240f, 18f), new Color(0f, 0f, 0f, 0.46f));
            Image armorFill = CreateImage("ArmorFill", armorBack.transform, Vector2.zero, new Vector2(240f, 18f), new Color(0.22f, 0.58f, 0.95f));
            armorFill.type = Image.Type.Filled;
            armorFill.fillMethod = Image.FillMethod.Horizontal;
            armorFill.fillOrigin = 0;

            Text weaponText = CreateText("WeaponText", hudPanel.transform, font, "Pistol", 30, TextAnchor.MiddleRight, new Vector2(700f, 476f), new Vector2(320f, 50f));
            Image weaponPanel = CreateImage("M12 Tactical Weapon Panel", hudPanel.transform, new Vector2(720f, 448f), new Vector2(410f, 128f), new Color(0.02f, 0.035f, 0.04f, 0.58f));
            weaponPanel.raycastTarget = false;
            weaponPanel.transform.SetSiblingIndex(Mathf.Max(0, weaponText.transform.GetSiblingIndex() - 1));
            Text ammoText = CreateText("AmmoText", hudPanel.transform, font, "12/36", 38, TextAnchor.MiddleRight, new Vector2(750f, 420f), new Vector2(260f, 60f));
            Text medkitText = CreateText("MedkitText", hudPanel.transform, font, "Medkits 0", 28, TextAnchor.MiddleLeft, new Vector2(-758f, 420f), new Vector2(300f, 50f));
            Text botsText = CreateText("BotsAliveText", hudPanel.transform, font, "Bots 5", 28, TextAnchor.MiddleCenter, new Vector2(0f, 476f), new Vector2(240f, 50f));
            Text zoneText = CreateText("ZoneText", hudPanel.transform, font, "Safe | Zone 95m | Shrink 60s", 26, TextAnchor.MiddleCenter, new Vector2(0f, 426f), new Vector2(720f, 50f));
            Text pickupPrompt = CreateText("PickupPrompt", hudPanel.transform, font, "", 28, TextAnchor.MiddleCenter, new Vector2(0f, -300f), new Vector2(720f, 50f));
            pickupPrompt.color = new Color(1f, 0.94f, 0.56f);
            Text vehiclePrompt = CreateText("VehiclePrompt", hudPanel.transform, font, "", 26, TextAnchor.MiddleCenter, new Vector2(0f, -352f), new Vector2(760f, 46f));
            vehiclePrompt.color = new Color(0.62f, 1f, 0.88f);
            Text vehicleStatusText = CreateText("VehicleStatusText", hudPanel.transform, font, "", 24, TextAnchor.MiddleCenter, new Vector2(0f, 384f), new Vector2(760f, 42f));
            vehicleStatusText.color = new Color(0.75f, 0.96f, 1f);
            vehicleStatusText.enabled = false;
            Text pickupMessage = CreateText("PickupMessage", hudPanel.transform, font, "", 28, TextAnchor.MiddleCenter, new Vector2(0f, -230f), new Vector2(720f, 50f));
            pickupMessage.color = new Color(0.88f, 1f, 0.76f);
            pickupMessage.enabled = false;
            Text developerText = CreateText("DeveloperTestPanel", hudPanel.transform, font, "", 17, TextAnchor.MiddleLeft, new Vector2(-400f, -492f), new Vector2(1040f, 84f));
            developerText.color = new Color(0.74f, 1f, 0.82f, 0.88f);
            RuntimeDeveloperPanel developerPanel = developerText.gameObject.AddComponent<RuntimeDeveloperPanel>();
            developerPanel.Configure(developerText, null, null, null);

            MobileButtonLayoutProfile layoutProfile = hudPanel.AddComponent<MobileButtonLayoutProfile>();
            layoutProfile.Configure(
                jumpButton.GetComponent<RectTransform>(),
                crouchButton.GetComponent<RectTransform>(),
                reloadButton.GetComponent<RectTransform>(),
                medkitButton.GetComponent<RectTransform>(),
                switchButton.GetComponent<RectTransform>(),
                shoulderButton.GetComponent<RectTransform>(),
                proneButton.GetComponent<RectTransform>(),
                driveButton.GetComponent<RectTransform>(),
                inventoryButton.GetComponent<RectTransform>(),
                fireButton.GetComponent<RectTransform>(),
                sprintButton.GetComponent<RectTransform>(),
                aimButton.GetComponent<RectTransform>(),
                throttleButton.GetComponent<RectTransform>(),
                brakeButton.GetComponent<RectTransform>());

            Image damageFlash = CreateImage("DamageFlash", hudPanel.transform, Vector2.zero, new Vector2(1920f, 1080f), new Color(1f, 0f, 0f, 0f));
            damageFlash.raycastTarget = false;
            damageFlash.transform.SetAsLastSibling();
            damageFlash.gameObject.SetActive(false);

            GameObject uiManagerObject = new GameObject("UIManager");
            uiManagerObject.transform.SetParent(canvasObject.transform, false);
            UIManager uiManager = uiManagerObject.AddComponent<UIManager>();
            uiManager.ConfigureForRuntime(
                mainMenuPanel,
                hudPanel,
                gameOverPanel,
                victoryPanel,
                healthText,
                healthFill,
                armorText,
                armorFill,
                ammoText,
                weaponText,
                medkitText,
                pickupMessage,
                botsText,
                zoneText,
                damageFlash,
                matchTimerText,
                killFeedText,
                matchAnnouncementText,
                inventoryPanel,
                inventoryDetails,
                crosshair.rectTransform,
                hudTelemetry,
                matchSummaryPanel,
                matchSummaryText,
                matchPhaseText,
                flightPathText,
                minimapFlightPath.rectTransform);

            return new RuntimeUIRefs
            {
                UIManager = uiManager,
                Joystick = joystick,
                LookArea = lookArea,
                PickupPrompt = pickupPrompt,
                StartButton = startButton,
                GameOverRestartButton = gameOverRestartButton,
                VictoryRestartButton = victoryRestartButton,
                CloseSummaryButton = closeSummaryButton,
                JumpButton = jumpButton,
                CrouchButton = crouchButton,
                ReloadButton = reloadButton,
                MedkitButton = medkitButton,
                SwitchButton = switchButton,
                ShoulderButton = shoulderButton,
                ProneButton = proneButton,
                DriveButton = driveButton,
                InventoryButton = inventoryButton,
                FireButton = fireButton,
                SprintButton = sprintButton,
                AimButton = aimButton,
                ThrottleButton = throttleButton,
                BrakeButton = brakeButton,
                InventoryPanel = inventoryPanel,
                PausePanel = pausePanel,
                SettingsPanel = settingsPanel,
                PauseButton = pauseButton,
                ResumeButton = resumeButton,
                SettingsButton = settingsButton,
                CloseInventoryButton = closeInventoryButton,
                CloseSettingsButton = closeSettingsButton,
                SensitivitySlider = sensitivitySlider,
                AimSensitivitySlider = aimSensitivitySlider,
                ScopeSensitivitySlider = scopeSensitivitySlider,
                GraphicsSlider = graphicsSlider,
                AudioSlider = audioSlider,
                ButtonScaleSlider = buttonScaleSlider,
                ButtonLayout = layoutProfile,
                HitMarker = hitMarker,
                HeadshotMarker = headshotMarker,
                HudTelemetry = hudTelemetry,
                DeveloperPanel = developerPanel,
                CompassText = compassText,
                VehiclePrompt = vehiclePrompt,
                VehicleStatusText = vehicleStatusText,
                MinimapArrow = minimapArrow.rectTransform,
                MinimapZoneRing = minimapZone.rectTransform,
                MinimapNextZoneRing = minimapNextZone.rectTransform,
                MinimapLabel = minimapLabel,
                MinimapLocation = minimapLocation
            };
        }

        private PlayerRefs BuildPlayer(Camera mainCamera, RuntimeUIRefs uiRefs, AdvancedWeaponData[] weaponRoster)
        {
            GameObject spawn = new GameObject("PlayerSpawn");
            runtimeObjects.Add(spawn);
            spawn.transform.position = new Vector3(0f, 1.1f, -52f);
            spawn.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject player = new GameObject("Player");
            runtimeObjects.Add(player);
            player.layer = playerLayer;
            player.tag = "Player";
            player.transform.position = spawn.transform.position;

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.stepOffset = 0.35f;
            characterController.slopeLimit = 45f;

            GameObject pivotObject = new GameObject("CameraPivot");
            pivotObject.transform.SetParent(player.transform);
            pivotObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            GameObject muzzleObject = new GameObject("MuzzlePoint");
            muzzleObject.transform.SetParent(player.transform);
            muzzleObject.transform.localPosition = new Vector3(0.35f, 1.25f, 0.45f);

            Health health = player.AddComponent<Health>();
            health.SetMaxArmor(50f, true);
            PlayerEquipment equipment = player.AddComponent<PlayerEquipment>();
            equipment.ConfigureForRuntime(health);
            KnockdownReviveState knockdown = player.AddComponent<KnockdownReviveState>();
            knockdown.ConfigureForRuntime(health, true);

            ThirdPersonMobileController controller = player.AddComponent<ThirdPersonMobileController>();
            ReliablePlayerMovement reliableMovement = player.AddComponent<ReliablePlayerMovement>();
            WeaponController weapons = player.AddComponent<WeaponController>();
            CombatRecoilApplicator combatRecoil = player.AddComponent<CombatRecoilApplicator>();
            CombatDebugWindow combatDebug = player.AddComponent<CombatDebugWindow>();
            ModularWeaponLoadout modularLoadout = player.AddComponent<ModularWeaponLoadout>();
            PlayerInventory inventory = player.AddComponent<PlayerInventory>();
            LootPickupInteractor pickup = player.AddComponent<LootPickupInteractor>();
            VehicleInteractor vehicleInteractor = player.AddComponent<VehicleInteractor>();
            HumanoidPlaceholderAnimator placeholderAnimator = BuildHumanoidVisual(player.transform, muzzleObject.transform, health);
            Animator visualAnimator = placeholderAnimator.GetComponent<Animator>();
            CombatAnimationEventBridge animationEventBridge = visualAnimator != null ? visualAnimator.gameObject.AddComponent<CombatAnimationEventBridge>() : null;

            WorldDamageNumber damageNumberPrefab = BuildDamageNumberTemplate();
            ParticleSystem muzzleFlashPrefab = BuildMuzzleFlashTemplate();
            ParticleSystem hitEffectPrefab = BuildHitEffectTemplate();
            WeaponDefinition pistol = CreatePistolDefinition(muzzleFlashPrefab);
            WeaponDefinition rifle = CreateRifleDefinition(muzzleFlashPrefab);
            WeaponDefinition smg = CreateSMGDefinition(muzzleFlashPrefab);
            WeaponDefinition sniper = CreateSniperDefinition(muzzleFlashPrefab);
            WeaponDefinition shotgun = CreateShotgunDefinition(muzzleFlashPrefab);
            LayerMask combatMask = ~0;
            LayerMask lootMask = 1 << lootLayer;

            controller.ConfigureForRuntime(uiRefs.Joystick, uiRefs.LookArea, mainCamera, pivotObject.transform);
            controller.SetHumanoidAnimator(placeholderAnimator);
            controller.SetUnityAnimator(visualAnimator);
            controller.SetCameraCollisionMask(~(1 << playerLayer));
            reliableMovement.ConfigureForRuntime(characterController, mainCamera, uiRefs.Joystick);
            controller.enabled = true;
            reliableMovement.enabled = true;
            controller.SetExternalGroundMovementDriver(reliableMovement.OwnsGroundMovement);
            WeaponModelRig weaponModelRig = placeholderAnimator.GetComponentInChildren<WeaponModelRig>();
            weapons.ConfigureForRuntime(mainCamera, muzzleObject.transform, controller, weaponModelRig, damageNumberPrefab, hitEffectPrefab, combatMask, pistol, rifle, smg, sniper, shotgun);
            combatRecoil.Configure(controller, null, uiRefs.UIManager);
            modularLoadout.ConfigureForRuntime(mainCamera, muzzleObject.transform, visualAnimator, controller, combatRecoil, null, combatMask, BuildStartingModularWeapons(weaponRoster));
            combatDebug.Configure(weapons, modularLoadout, combatRecoil, health);
            animationEventBridge?.Configure(weapons, null);
            inventory.ConfigureForRuntime(weapons, health, modularLoadout);
            pickup.ConfigureForRuntime(mainCamera, inventory, uiRefs.PickupPrompt, lootMask);
            vehicleInteractor.ConfigureForRuntime(controller, weapons, uiRefs.VehiclePrompt);
            uiRefs.HudTelemetry.Configure(player.transform, mainCamera, uiRefs.CompassText, uiRefs.MinimapArrow, uiRefs.MinimapZoneRing, uiRefs.MinimapLabel, uiRefs.MinimapNextZoneRing);
            uiRefs.HudTelemetry.ConfigureNamedLocations(runtimeLocationNames.ToArray(), runtimeLocationPositions.ToArray(), uiRefs.MinimapLocation);
            weapons.onFired.AddListener(controller.TriggerFireAnimation);
            weapons.onReloadStarted.AddListener(controller.TriggerReloadAnimation);
            weapons.onWeaponSwitchStarted.AddListener(controller.TriggerWeaponSwitchAnimation);
            health.onDamageTaken.AddListener((amount, hitPoint, hitNormal, source) => placeholderAnimator.TriggerHit(amount));
            health.onDied.AddListener(_ => placeholderAnimator.TriggerDeath());

            return new PlayerRefs
            {
                Transform = player.transform,
                SpawnPoint = spawn.transform,
                Controller = controller,
                ReliableMovement = reliableMovement,
                Health = health,
                Weapons = weapons,
                Inventory = inventory,
                Equipment = equipment,
                VehicleInteractor = vehicleInteractor,
                Knockdown = knockdown
            };
        }

        private static AdvancedWeaponData[] BuildStartingModularWeapons(AdvancedWeaponData[] weaponRoster)
        {
            AdvancedWeaponData sidearm = FindAdvancedWeapon(weaponRoster, "sidearm_p9");
            AdvancedWeaponData rifle = FindAdvancedWeapon(weaponRoster, "vxr_56");

            if (sidearm != null && rifle != null)
            {
                return new[] { sidearm, rifle };
            }

            if (sidearm != null)
            {
                return new[] { sidearm };
            }

            if (rifle != null)
            {
                return new[] { rifle };
            }

            return new AdvancedWeaponData[0];
        }

        private static AdvancedWeaponData FindAdvancedWeapon(AdvancedWeaponData[] weaponRoster, string weaponId)
        {
            if (weaponRoster == null || string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            for (int i = 0; i < weaponRoster.Length; i++)
            {
                AdvancedWeaponData weapon = weaponRoster[i];
                if (weapon != null && string.Equals(weapon.WeaponId, weaponId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return weapon;
                }
            }

            return null;
        }

        private HumanoidPlaceholderAnimator BuildHumanoidVisual(Transform playerRoot, Transform muzzlePoint, Health health)
        {
            GameObject visualRoot = new GameObject("LowPolyOriginalHumanoid");
            visualRoot.transform.SetParent(playerRoot, false);
            visualRoot.transform.localPosition = Vector3.zero;
            TagVisualModule(visualRoot, RuntimeVisualModuleKind.Character, "M12_Player_TacticalHumanoid");

            GameObject hips = new GameObject("Hips");
            hips.transform.SetParent(visualRoot.transform, false);
            hips.transform.localPosition = new Vector3(0f, 0.96f, 0f);

            Transform torso = CreateBodyPart("Torso", hips.transform, PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(0.62f, 0.84f, 0.40f), playerMaterial);
            CreateBodyPart("PelvisPlate", hips.transform, PrimitiveType.Cube, new Vector3(0f, -0.08f, -0.03f), new Vector3(0.52f, 0.26f, 0.36f), playerAccentMaterial);
            Transform chest = CreateBodyPart("ChestPlate", torso, PrimitiveType.Cube, new Vector3(0f, 0.11f, -0.20f), new Vector3(0.52f, 0.48f, 0.08f), playerAccentMaterial);
            CreateBodyPart("M12 Tactical Vest Left Pouch", torso, PrimitiveType.Cube, new Vector3(-0.17f, -0.08f, -0.245f), new Vector3(0.13f, 0.18f, 0.055f), lootAmmoMaterial);
            CreateBodyPart("M12 Tactical Vest Right Pouch", torso, PrimitiveType.Cube, new Vector3(0.17f, -0.08f, -0.245f), new Vector3(0.13f, 0.18f, 0.055f), lootAmmoMaterial);
            CreateBodyPart("M12 Tactical Radio Pack", torso, PrimitiveType.Cube, new Vector3(0.28f, 0.12f, 0.24f), new Vector3(0.12f, 0.28f, 0.10f), vehicleAccentMaterial);
            Transform head = CreateBodyPart("Head", hips.transform, PrimitiveType.Sphere, new Vector3(0f, 1.06f, 0f), new Vector3(0.34f, 0.36f, 0.34f), playerMaterial);
            Transform visor = CreateBodyPart("Visor", head, PrimitiveType.Cube, new Vector3(0f, 0.03f, -0.18f), new Vector3(0.26f, 0.08f, 0.035f), playerAccentMaterial);
            CreateBodyPart("HelmetCrest", head, PrimitiveType.Cube, new Vector3(0f, 0.19f, 0f), new Vector3(0.38f, 0.07f, 0.38f), playerAccentMaterial);
            CreateBodyPart("M12 Helmet Side Rail L", head, PrimitiveType.Cube, new Vector3(-0.21f, 0.04f, -0.03f), new Vector3(0.04f, 0.08f, 0.22f), playerAccentMaterial);
            CreateBodyPart("M12 Helmet Side Rail R", head, PrimitiveType.Cube, new Vector3(0.21f, 0.04f, -0.03f), new Vector3(0.04f, 0.08f, 0.22f), playerAccentMaterial);
            Transform leftArm = CreateBodyPart("LeftArm", hips.transform, PrimitiveType.Cylinder, new Vector3(-0.49f, 0.43f, 0f), new Vector3(0.105f, 0.38f, 0.105f), playerMaterial);
            Transform rightArm = CreateBodyPart("RightArm", hips.transform, PrimitiveType.Cylinder, new Vector3(0.49f, 0.43f, 0f), new Vector3(0.105f, 0.38f, 0.105f), playerMaterial);
            CreateBodyPart("LeftForearmGuard", hips.transform, PrimitiveType.Cube, new Vector3(-0.49f, 0.12f, -0.02f), new Vector3(0.17f, 0.22f, 0.13f), playerAccentMaterial);
            CreateBodyPart("RightForearmGuard", hips.transform, PrimitiveType.Cube, new Vector3(0.49f, 0.12f, -0.02f), new Vector3(0.17f, 0.22f, 0.13f), playerAccentMaterial);
            CreateBodyPart("LeftHand", hips.transform, PrimitiveType.Cube, new Vector3(-0.49f, -0.03f, -0.01f), new Vector3(0.16f, 0.12f, 0.14f), playerAccentMaterial);
            CreateBodyPart("RightHand", hips.transform, PrimitiveType.Cube, new Vector3(0.49f, -0.03f, -0.01f), new Vector3(0.16f, 0.12f, 0.14f), playerAccentMaterial);
            CreateBodyPart("LeftShoulderPad", hips.transform, PrimitiveType.Cube, new Vector3(-0.49f, 0.84f, 0f), new Vector3(0.30f, 0.12f, 0.30f), playerAccentMaterial);
            CreateBodyPart("RightShoulderPad", hips.transform, PrimitiveType.Cube, new Vector3(0.49f, 0.84f, 0f), new Vector3(0.30f, 0.12f, 0.30f), playerAccentMaterial);
            Transform leftLeg = CreateBodyPart("LeftLeg", hips.transform, PrimitiveType.Cylinder, new Vector3(-0.19f, -0.50f, 0f), new Vector3(0.115f, 0.46f, 0.115f), playerMaterial);
            Transform rightLeg = CreateBodyPart("RightLeg", hips.transform, PrimitiveType.Cylinder, new Vector3(0.19f, -0.50f, 0f), new Vector3(0.115f, 0.46f, 0.115f), playerMaterial);
            CreateBodyPart("LeftKneePad", hips.transform, PrimitiveType.Cube, new Vector3(-0.18f, -0.58f, -0.13f), new Vector3(0.2f, 0.14f, 0.08f), playerAccentMaterial);
            CreateBodyPart("RightKneePad", hips.transform, PrimitiveType.Cube, new Vector3(0.18f, -0.58f, -0.13f), new Vector3(0.2f, 0.14f, 0.08f), playerAccentMaterial);
            CreateBodyPart("LeftBoot", hips.transform, PrimitiveType.Cube, new Vector3(-0.19f, -1.0f, -0.05f), new Vector3(0.25f, 0.15f, 0.34f), playerAccentMaterial);
            CreateBodyPart("RightBoot", hips.transform, PrimitiveType.Cube, new Vector3(0.19f, -1.0f, -0.05f), new Vector3(0.25f, 0.15f, 0.34f), playerAccentMaterial);
            CreateBodyPart("Backpack", torso, PrimitiveType.Cube, new Vector3(0f, 0f, 0.27f), new Vector3(0.48f, 0.64f, 0.18f), playerAccentMaterial);
            CreateBodyPart("M12 Backpack Bedroll", torso, PrimitiveType.Cylinder, new Vector3(0f, 0.34f, 0.40f), new Vector3(0.17f, 0.32f, 0.17f), fenceMaterial);

            GameObject weaponRig = new GameObject("WeaponRig");
            weaponRig.transform.SetParent(hips.transform, false);
            weaponRig.transform.localPosition = new Vector3(0.28f, 0.34f, 0.36f);
            WeaponModelRig modelRig = weaponRig.AddComponent<WeaponModelRig>();
            WeaponModelEntry pistolModel = CreateWeaponModel(weaponRig.transform, WeaponSlot.Pistol, "Original Low Poly Pistol", 0.42f, 0.15f, 0.10f, false);
            WeaponModelEntry rifleModel = CreateWeaponModel(weaponRig.transform, WeaponSlot.AssaultRifle, "Original Low Poly Assault Rifle", 0.88f, 0.16f, 0.12f, true);
            WeaponModelEntry smgModel = CreateWeaponModel(weaponRig.transform, WeaponSlot.SMG, "Original Low Poly SMG", 0.68f, 0.15f, 0.11f, true);
            WeaponModelEntry sniperModel = CreateWeaponModel(weaponRig.transform, WeaponSlot.Sniper, "Original Low Poly Sniper", 1.18f, 0.13f, 0.10f, true);
            WeaponModelEntry shotgunModel = CreateWeaponModel(weaponRig.transform, WeaponSlot.Shotgun, "Original Low Poly Shotgun", 0.96f, 0.18f, 0.13f, false);
            modelRig.Configure(pistolModel, rifleModel, smgModel, sniperModel, shotgunModel);
            muzzlePoint.SetParent(weaponRig.transform, false);
            muzzlePoint.localPosition = new Vector3(0f, 0f, 0.78f);

            ConfigureCombatHitbox(torso, CombatHitZone.Chest, health);
            ConfigureCombatHitbox(chest, CombatHitZone.Chest, health);
            ConfigureCombatHitbox(head, CombatHitZone.Head, health);
            ConfigureCombatHitbox(leftArm, CombatHitZone.Arm, health);
            ConfigureCombatHitbox(rightArm, CombatHitZone.Arm, health);
            ConfigureCombatHitbox(leftLeg, CombatHitZone.Leg, health);
            ConfigureCombatHitbox(rightLeg, CombatHitZone.Leg, health);

            HumanoidPlaceholderAnimator placeholder = visualRoot.AddComponent<HumanoidPlaceholderAnimator>();
            placeholder.Configure(hips.transform, torso, head, leftArm, rightArm, leftLeg, rightLeg, weaponRig.transform);
            Animator visualAnimator = visualRoot.AddComponent<Animator>();
            visualAnimator.applyRootMotion = false;
            visualAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            RuntimeAnimatorController controllerAsset = Resources.Load<RuntimeAnimatorController>("AC_PlayerHumanoid");
            if (controllerAsset != null)
            {
                visualAnimator.runtimeAnimatorController = controllerAsset;
            }

            chest.name = "ChestPlate";
            visor.name = "Visor";
            return placeholder;
        }

        private static void ConfigureCombatHitbox(Transform target, CombatHitZone zone, Health owner)
        {
            if (target == null)
            {
                return;
            }

            CombatHitbox hitbox = target.GetComponent<CombatHitbox>();
            if (hitbox == null)
            {
                hitbox = target.gameObject.AddComponent<CombatHitbox>();
            }

            hitbox.Configure(zone, owner);
        }

        private WeaponModelEntry CreateWeaponModel(Transform parent, WeaponSlot slot, string objectName, float length, float height, float width, bool optic)
        {
            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            TagVisualModule(root, RuntimeVisualModuleKind.Weapon, $"{objectName}_{slot}");

            CreateBodyPart($"{objectName} Receiver", root.transform, PrimitiveType.Cube, Vector3.zero, new Vector3(width, height, length), lootWeaponMaterial);
            CreateBodyPart($"{objectName} Barrel", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.01f, length * 0.54f), new Vector3(width * 0.55f, height * 0.55f, length * 0.42f), playerAccentMaterial);
            CreateBodyPart($"{objectName} Grip", root.transform, PrimitiveType.Cube, new Vector3(0f, -height * 0.8f, -length * 0.18f), new Vector3(width * 0.72f, height * 1.25f, length * 0.16f), playerAccentMaterial);
            CreateBodyPart($"{objectName} Muzzle Device", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.01f, length * 0.82f), new Vector3(width * 0.82f, height * 0.72f, length * 0.08f), vehicleAccentMaterial);
            CreateBodyPart($"{objectName} Top Rail", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.64f, -length * 0.04f), new Vector3(width * 0.72f, height * 0.16f, length * 0.52f), playerAccentMaterial);
            CreateBodyPart($"{objectName} Ejection Port", root.transform, PrimitiveType.Cube, new Vector3(width * 0.56f, height * 0.12f, length * 0.05f), new Vector3(width * 0.10f, height * 0.38f, length * 0.18f), windowMaterial);
            CreateBodyPart($"{objectName} Rear Sight", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.86f, -length * 0.30f), new Vector3(width * 0.72f, height * 0.18f, length * 0.045f), vehicleAccentMaterial);
            CreateBodyPart($"{objectName} Front Sight", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.84f, length * 0.54f), new Vector3(width * 0.62f, height * 0.22f, length * 0.04f), vehicleAccentMaterial);

            if (slot != WeaponSlot.Pistol)
            {
                CreateBodyPart($"{objectName} Stock", root.transform, PrimitiveType.Cube, new Vector3(0f, 0f, -length * 0.56f), new Vector3(width * 1.1f, height * 0.88f, length * 0.24f), playerAccentMaterial);
                CreateBodyPart($"{objectName} Magazine", root.transform, PrimitiveType.Cube, new Vector3(0f, -height * 0.72f, length * 0.06f), new Vector3(width * 0.78f, height * 1.25f, length * 0.13f), lootAmmoMaterial);
                CreateBodyPart($"{objectName} Underbarrel Attachment", root.transform, PrimitiveType.Cube, new Vector3(0f, -height * 0.54f, length * 0.26f), new Vector3(width * 0.58f, height * 0.34f, length * 0.24f), vehicleAccentMaterial);
                CreateBodyPart($"{objectName} Handguard Vent A", root.transform, PrimitiveType.Cube, new Vector3(-width * 0.44f, height * 0.08f, length * 0.34f), new Vector3(width * 0.08f, height * 0.2f, length * 0.20f), windowMaterial);
                CreateBodyPart($"{objectName} Handguard Vent B", root.transform, PrimitiveType.Cube, new Vector3(width * 0.44f, height * 0.08f, length * 0.34f), new Vector3(width * 0.08f, height * 0.2f, length * 0.20f), windowMaterial);
                CreateBodyPart($"{objectName} Sling Loop", root.transform, PrimitiveType.Cube, new Vector3(-width * 0.6f, -height * 0.18f, -length * 0.42f), new Vector3(width * 0.10f, height * 0.18f, length * 0.12f), vehicleAccentMaterial);
            }
            else
            {
                CreateBodyPart($"{objectName} Pistol Slide Cut", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.34f, length * 0.08f), new Vector3(width * 0.64f, height * 0.12f, length * 0.34f), windowMaterial);
                CreateBodyPart($"{objectName} Trigger Guard", root.transform, PrimitiveType.Cube, new Vector3(0f, -height * 0.38f, length * 0.02f), new Vector3(width * 0.86f, height * 0.18f, length * 0.12f), vehicleAccentMaterial);
            }

            if (optic)
            {
                CreateBodyPart($"{objectName} Optic", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.9f, length * 0.02f), new Vector3(width * 0.78f, height * 0.44f, length * 0.18f), windowMaterial);
                CreateBodyPart($"{objectName} Scope Lens Front", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.9f, length * 0.13f), new Vector3(width * 0.82f, height * 0.36f, length * 0.025f), laneMarkMaterial);
                CreateBodyPart($"{objectName} Scope Lens Rear", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.9f, -length * 0.09f), new Vector3(width * 0.82f, height * 0.36f, length * 0.025f), windowMaterial);
            }

            if (slot == WeaponSlot.Shotgun)
            {
                CreateBodyPart($"{objectName} Tube Magazine", root.transform, PrimitiveType.Cube, new Vector3(0f, -height * 0.28f, length * 0.34f), new Vector3(width * 0.52f, height * 0.28f, length * 0.46f), lootAmmoMaterial);
            }

            if (slot == WeaponSlot.Sniper)
            {
                CreateBodyPart($"{objectName} Long Scope Shade", root.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.91f, length * 0.23f), new Vector3(width * 0.86f, height * 0.40f, length * 0.14f), vehicleAccentMaterial);
            }

            root.SetActive(false);
            return new WeaponModelEntry
            {
                slot = slot,
                modelRoot = root
            };
        }

        private Transform CreateBodyPart(string objectName, Transform parent, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = playerLayer;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = material;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }

            return part.transform;
        }

        private WorldDamageNumber BuildDamageNumberTemplate()
        {
            GameObject template = new GameObject("PF_RuntimeDamageNumber");
            runtimeObjects.Add(template);
            TextMesh textMesh = template.AddComponent<TextMesh>();
            textMesh.font = LoadRuntimeFont();
            WorldDamageNumber number = template.AddComponent<WorldDamageNumber>();
            number.Configure(1f, Camera.main);
            template.SetActive(false);
            return number;
        }

        private ParticleSystem BuildMuzzleFlashTemplate()
        {
            GameObject template = new GameObject("PF_RuntimeMuzzleFlash");
            runtimeObjects.Add(template);
            template.SetActive(false);
            ParticleSystem particles = template.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.duration = 0.08f;
            main.startLifetime = 0.08f;
            main.startSpeed = 0.8f;
            main.startSize = 0.16f;
            main.startColor = new Color(1f, 0.74f, 0.22f, 1f);
            main.loop = false;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.035f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = CreateParticleMaterial("Runtime Muzzle Flash Particle", new Color(1f, 0.74f, 0.22f, 1f));
            }

            return particles;
        }

        private ParticleSystem BuildHitEffectTemplate()
        {
            GameObject template = new GameObject("PF_RuntimeHitEffect");
            runtimeObjects.Add(template);
            template.SetActive(false);
            ParticleSystem particles = template.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.duration = 0.18f;
            main.startLifetime = 0.16f;
            main.startSpeed = 1.7f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.62f, 0.22f, 1f);
            main.loop = false;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.08f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = CreateParticleMaterial("Runtime Hit Spark Particle", new Color(1f, 0.62f, 0.22f, 1f));
            }

            return particles;
        }

        private WeaponDefinition CreatePistolDefinition(ParticleSystem muzzleFlashPrefab)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "Runtime Pistol";
            definition.slot = WeaponSlot.Pistol;
            definition.displayName = "Pistol";
            definition.rarity = WeaponRarity.Common;
            definition.attachmentProfile = "Sidearm rail ready";
            definition.ammoKind = AmmoKind.Light;
            definition.magazineSize = 12;
            definition.startingReserveAmmo = 36;
            definition.damage = 24f;
            definition.range = 90f;
            definition.fireRate = 3f;
            definition.reloadTime = 1.3f;
            definition.automatic = false;
            definition.spreadAngle = 1.1f;
            definition.adsSpreadMultiplier = 0.48f;
            definition.hipSpreadMultiplier = 1.05f;
            definition.cameraKick = 0.7f;
            definition.recoilRecovery = 18f;
            definition.adsFieldOfView = 46f;
            definition.switchTime = 0.18f;
            definition.tracerColor = new Color(1f, 0.86f, 0.24f, 1f);
            definition.muzzleFlashPrefab = muzzleFlashPrefab;
            return definition;
        }

        private WeaponDefinition CreateRifleDefinition(ParticleSystem muzzleFlashPrefab)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "Runtime Assault Rifle";
            definition.slot = WeaponSlot.AssaultRifle;
            definition.displayName = "Assault Rifle";
            definition.rarity = WeaponRarity.Rare;
            definition.attachmentProfile = "Optic muzzle grip mag";
            definition.ammoKind = AmmoKind.Medium;
            definition.magazineSize = 30;
            definition.startingReserveAmmo = 90;
            definition.damage = 16f;
            definition.range = 130f;
            definition.fireRate = 9f;
            definition.reloadTime = 1.9f;
            definition.automatic = true;
            definition.spreadAngle = 1.8f;
            definition.adsSpreadMultiplier = 0.52f;
            definition.hipSpreadMultiplier = 1.08f;
            definition.cameraKick = 0.34f;
            definition.recoilRecovery = 17f;
            definition.adsFieldOfView = 44f;
            definition.switchTime = 0.28f;
            definition.tracerColor = new Color(1f, 0.72f, 0.22f, 1f);
            definition.muzzleFlashPrefab = muzzleFlashPrefab;
            return definition;
        }

        private WeaponDefinition CreateSMGDefinition(ParticleSystem muzzleFlashPrefab)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "Runtime SMG";
            definition.slot = WeaponSlot.SMG;
            definition.displayName = "SMG";
            definition.rarity = WeaponRarity.Uncommon;
            definition.attachmentProfile = "Close optic mag";
            definition.ammoKind = AmmoKind.Light;
            definition.magazineSize = 35;
            definition.startingReserveAmmo = 105;
            definition.damage = 11f;
            definition.range = 82f;
            definition.fireRate = 12f;
            definition.reloadTime = 1.55f;
            definition.automatic = true;
            definition.spreadAngle = 2.3f;
            definition.adsSpreadMultiplier = 0.58f;
            definition.hipSpreadMultiplier = 0.92f;
            definition.cameraKick = 0.24f;
            definition.recoilRecovery = 21f;
            definition.adsFieldOfView = 48f;
            definition.switchTime = 0.22f;
            definition.tracerColor = new Color(0.7f, 0.95f, 1f, 1f);
            definition.muzzleFlashPrefab = muzzleFlashPrefab;
            return definition;
        }

        private WeaponDefinition CreateSniperDefinition(ParticleSystem muzzleFlashPrefab)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "Runtime Sniper";
            definition.slot = WeaponSlot.Sniper;
            definition.displayName = "Sniper";
            definition.rarity = WeaponRarity.Epic;
            definition.attachmentProfile = "Long optic muzzle";
            definition.ammoKind = AmmoKind.Heavy;
            definition.magazineSize = 5;
            definition.startingReserveAmmo = 20;
            definition.damage = 86f;
            definition.range = 185f;
            definition.fireRate = 0.75f;
            definition.reloadTime = 2.4f;
            definition.automatic = false;
            definition.spreadAngle = 0.18f;
            definition.adsSpreadMultiplier = 0.22f;
            definition.hipSpreadMultiplier = 2.6f;
            definition.cameraKick = 1.45f;
            definition.recoilRecovery = 11f;
            definition.adsFieldOfView = 36f;
            definition.switchTime = 0.38f;
            definition.tracerColor = new Color(1f, 0.42f, 0.26f, 1f);
            definition.supportsGrip = false;
            definition.muzzleFlashPrefab = muzzleFlashPrefab;
            return definition;
        }

        private WeaponDefinition CreateShotgunDefinition(ParticleSystem muzzleFlashPrefab)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "Runtime Shotgun";
            definition.slot = WeaponSlot.Shotgun;
            definition.displayName = "Shotgun";
            definition.rarity = WeaponRarity.Uncommon;
            definition.attachmentProfile = "Muzzle shell carrier";
            definition.ammoKind = AmmoKind.Shell;
            definition.magazineSize = 6;
            definition.startingReserveAmmo = 24;
            definition.damage = 74f;
            definition.range = 46f;
            definition.fireRate = 1.05f;
            definition.reloadTime = 2.1f;
            definition.automatic = false;
            definition.pelletCount = 8;
            definition.spreadAngle = 4.8f;
            definition.adsSpreadMultiplier = 0.72f;
            definition.hipSpreadMultiplier = 1.08f;
            definition.cameraKick = 1.05f;
            definition.recoilRecovery = 13f;
            definition.adsFieldOfView = 43f;
            definition.switchTime = 0.34f;
            definition.tracerColor = new Color(1f, 0.62f, 0.18f, 1f);
            definition.supportsGrip = false;
            definition.muzzleFlashPrefab = muzzleFlashPrefab;
            return definition;
        }

        private BotAI BuildBotTemplate()
        {
            GameObject bot = new GameObject("PF_RuntimeBot_Template");
            runtimeObjects.Add(bot);
            bot.layer = botLayer;
            CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
            collider.height = 1.35f;
            collider.radius = 0.35f;
            collider.center = new Vector3(0f, 0.68f, 0f);

            GameObject eye = new GameObject("EyePoint");
            eye.transform.SetParent(bot.transform);
            eye.transform.localPosition = new Vector3(0f, 1.45f, 0.25f);
            Renderer[] renderers = BuildBotHumanoidVisual(bot.transform);

            Health health = bot.AddComponent<Health>();
            health.SetMaxHealth(100f, true);
            health.SetMaxArmor(35f, true);
            KnockdownReviveState knockdown = bot.AddComponent<KnockdownReviveState>();
            knockdown.ConfigureForRuntime(health, true);
            BotAI ai = bot.AddComponent<BotAI>();
            ai.ConfigureForRuntime(eye.transform, renderers, ~0);
            bot.SetActive(false);
            return ai;
        }

        private Renderer[] BuildBotHumanoidVisual(Transform botRoot)
        {
            List<Renderer> renderers = new List<Renderer>();
            GameObject visualRoot = new GameObject("LowPolyBotHumanoid");
            visualRoot.transform.SetParent(botRoot, false);
            TagVisualModule(visualRoot, RuntimeVisualModuleKind.Character, "M10_Bot_LowPolyHumanoid");

            Transform torso = CreateBotBodyPart("BotTorso", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, 1.1f, 0f), new Vector3(0.58f, 0.72f, 0.38f), botMaterial, renderers);
            CreateBotBodyPart("BotArmorPanel", torso, PrimitiveType.Cube, new Vector3(0f, 0.08f, -0.19f), new Vector3(0.48f, 0.4f, 0.08f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotPelvisPlate", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, 0.78f, 0f), new Vector3(0.48f, 0.22f, 0.32f), playerAccentMaterial, renderers);
            Transform head = CreateBotBodyPart("BotHead", visualRoot.transform, PrimitiveType.Sphere, new Vector3(0f, 1.67f, 0f), new Vector3(0.34f, 0.34f, 0.34f), botMaterial, renderers);
            CreateBotBodyPart("BotVisor", head, PrimitiveType.Cube, new Vector3(0f, 0.03f, -0.18f), new Vector3(0.25f, 0.08f, 0.04f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotLeftArm", visualRoot.transform, PrimitiveType.Cylinder, new Vector3(-0.46f, 1.08f, 0f), new Vector3(0.11f, 0.32f, 0.11f), botMaterial, renderers);
            CreateBotBodyPart("BotRightArm", visualRoot.transform, PrimitiveType.Cylinder, new Vector3(0.46f, 1.08f, 0f), new Vector3(0.11f, 0.32f, 0.11f), botMaterial, renderers);
            CreateBotBodyPart("BotLeftHand", visualRoot.transform, PrimitiveType.Cube, new Vector3(-0.46f, 0.73f, -0.01f), new Vector3(0.16f, 0.12f, 0.14f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotRightHand", visualRoot.transform, PrimitiveType.Cube, new Vector3(0.46f, 0.73f, -0.01f), new Vector3(0.16f, 0.12f, 0.14f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotLeftLeg", visualRoot.transform, PrimitiveType.Cylinder, new Vector3(-0.17f, 0.42f, 0f), new Vector3(0.12f, 0.38f, 0.12f), botMaterial, renderers);
            CreateBotBodyPart("BotRightLeg", visualRoot.transform, PrimitiveType.Cylinder, new Vector3(0.17f, 0.42f, 0f), new Vector3(0.12f, 0.38f, 0.12f), botMaterial, renderers);
            CreateBotBodyPart("BotLeftKneePad", visualRoot.transform, PrimitiveType.Cube, new Vector3(-0.17f, 0.34f, -0.13f), new Vector3(0.2f, 0.13f, 0.08f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotRightKneePad", visualRoot.transform, PrimitiveType.Cube, new Vector3(0.17f, 0.34f, -0.13f), new Vector3(0.2f, 0.13f, 0.08f), playerAccentMaterial, renderers);
            CreateBotBodyPart("BotLootedWeapon", visualRoot.transform, PrimitiveType.Cube, new Vector3(0.32f, 1.15f, 0.35f), new Vector3(0.14f, 0.12f, 0.68f), lootWeaponMaterial, renderers);
            return renderers.ToArray();
        }

        private Transform CreateBotBodyPart(string objectName, Transform parent, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Material material, List<Renderer> renderers)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = botLayer;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.material = material;
            renderers.Add(renderer);

            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                DestroyRuntimeObject(partCollider);
            }

            return part.transform;
        }

        private LootItem[] BuildLootTemplates()
        {
            return new[]
            {
                CreateLootTemplate("PF_RuntimeLoot_Pistol", LootKind.Pistol, 12, "Pistol", new Vector3(0.7f, 0.18f, 0.25f), lootWeaponMaterial, 0, LootRarity.Common),
                CreateLootTemplate("PF_RuntimeLoot_AssaultRifle", LootKind.AssaultRifle, 30, "Assault Rifle", new Vector3(1.1f, 0.18f, 0.22f), lootWeaponMaterial, 0, LootRarity.Rare),
                CreateLootTemplate("PF_RuntimeLoot_SMG", LootKind.SMG, 35, "SMG", new Vector3(0.9f, 0.16f, 0.22f), lootWeaponMaterial, 0, LootRarity.Uncommon),
                CreateLootTemplate("PF_RuntimeLoot_Sniper", LootKind.Sniper, 5, "Sniper", new Vector3(1.45f, 0.16f, 0.20f), lootWeaponMaterial, 0, LootRarity.Epic),
                CreateLootTemplate("PF_RuntimeLoot_Shotgun", LootKind.Shotgun, 6, "Shotgun", new Vector3(1.25f, 0.2f, 0.26f), lootWeaponMaterial, 0, LootRarity.Rare),
                CreateLootTemplate("PF_RuntimeLoot_LightAmmo", LootKind.LightAmmo, 36, "Light Ammo", new Vector3(0.42f, 0.26f, 0.42f), lootAmmoMaterial, 1, LootRarity.Common),
                CreateLootTemplate("PF_RuntimeLoot_MediumAmmo", LootKind.MediumAmmo, 60, "Medium Ammo", new Vector3(0.55f, 0.32f, 0.45f), lootAmmoMaterial, 1, LootRarity.Common),
                CreateLootTemplate("PF_RuntimeLoot_HeavyAmmo", LootKind.HeavyAmmo, 15, "Heavy Ammo", new Vector3(0.48f, 0.42f, 0.48f), lootAmmoMaterial, 2, LootRarity.Uncommon),
                CreateLootTemplate("PF_RuntimeLoot_ShellAmmo", LootKind.ShellAmmo, 16, "Shells", new Vector3(0.62f, 0.28f, 0.46f), lootAmmoMaterial, 2, LootRarity.Uncommon),
                CreateLootTemplate("PF_RuntimeLoot_Medkit", LootKind.Medkit, 1, "Medkit", new Vector3(0.55f, 0.28f, 0.55f), medkitMaterial, 6, LootRarity.Uncommon),
                CreateLootTemplate("PF_RuntimeLoot_Bandage", LootKind.Bandage, 2, "Bandage", new Vector3(0.48f, 0.18f, 0.48f), medkitMaterial, 2, LootRarity.Common),
                CreateLootTemplate("PF_RuntimeLoot_ArmorPlate", LootKind.ArmorPlate, 1, "Armor Plate", new Vector3(0.54f, 0.12f, 0.54f), vehicleAccentMaterial, 4, LootRarity.Uncommon),
                CreateLootTemplate("PF_RuntimeLoot_ArmorVest", LootKind.ArmorVest, 1, "Tier 2 Armor", new Vector3(0.58f, 0.42f, 0.22f), playerAccentMaterial, 0, LootRarity.Rare),
                CreateLootTemplate("PF_RuntimeLoot_Helmet", LootKind.Helmet, 1, "Tier 2 Helmet", new Vector3(0.46f, 0.28f, 0.46f), playerMaterial, 0, LootRarity.Rare),
                CreateLootTemplate("PF_RuntimeLoot_Backpack", LootKind.Backpack, 1, "Backpack", new Vector3(0.52f, 0.44f, 0.28f), fenceMaterial, 0, LootRarity.Rare)
            };
        }

        private void BuildWeaponTestArea(AdvancedWeaponData[] weaponRoster, LootItem[] lootPrefabs)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (weaponRoster == null || weaponRoster.Length == 0)
            {
                return;
            }

            GameObject root = Milestone24BWeaponTestAreaBuilder.BuildOrRepair(
                weaponRoster,
                Milestone24BWeaponTestAreaBuilder.DefaultOrigin,
                lootLayer,
                lootWeaponMaterial,
                lootAmmoMaterial,
                playerMaterial,
                sidewalkMaterial != null ? sidewalkMaterial : roadMaterial,
                false);

            if (root != null && !runtimeObjects.Contains(root))
            {
                runtimeObjects.Add(root);
            }

            runtimeLocationNames.Add("Weapon Test");
            runtimeLocationPositions.Add(Milestone24BWeaponTestAreaBuilder.DefaultOrigin);
#endif
        }

        private void PlaceTestAmmoPickup(Transform parent, LootItem[] lootPrefabs, LootKind kind, Vector3 position)
        {
            LootItem prefab = FindLootPrefab(lootPrefabs, kind);
            if (prefab == null)
            {
                return;
            }

            LootItem instance = Instantiate(prefab, position, Quaternion.identity, parent);
            instance.name = $"M24B Test {kind}";
            instance.gameObject.SetActive(true);
            SetLayerRecursive(instance.gameObject, lootLayer);
        }

        private static LootItem FindLootPrefab(LootItem[] lootPrefabs, LootKind kind)
        {
            if (lootPrefabs == null)
            {
                return null;
            }

            for (int i = 0; i < lootPrefabs.Length; i++)
            {
                LootItem prefab = lootPrefabs[i];
                if (prefab != null && prefab.Kind == kind)
                {
                    return prefab;
                }
            }

            return null;
        }

        private void CreateWeaponTestTarget(Transform parent, Vector3 position, string objectName)
        {
            GameObject target = new GameObject(objectName);
            target.transform.SetParent(parent, true);
            target.transform.position = position;
            Health health = target.AddComponent<Health>();
            health.SetMaxHealth(250f, true);

            CreateTargetHitPart(target.transform, health, "Head", CombatHitZone.Head, PrimitiveType.Sphere, new Vector3(0f, 1.72f, 0f), new Vector3(0.36f, 0.36f, 0.36f), playerMaterial);
            CreateTargetHitPart(target.transform, health, "Chest", CombatHitZone.Chest, PrimitiveType.Cube, new Vector3(0f, 1.14f, 0f), new Vector3(0.62f, 0.72f, 0.28f), playerAccentMaterial);
            CreateTargetHitPart(target.transform, health, "Left Arm", CombatHitZone.Arm, PrimitiveType.Capsule, new Vector3(-0.48f, 1.15f, 0f), new Vector3(0.18f, 0.56f, 0.18f), playerMaterial);
            CreateTargetHitPart(target.transform, health, "Right Arm", CombatHitZone.Arm, PrimitiveType.Capsule, new Vector3(0.48f, 1.15f, 0f), new Vector3(0.18f, 0.56f, 0.18f), playerMaterial);
            CreateTargetHitPart(target.transform, health, "Left Leg", CombatHitZone.Leg, PrimitiveType.Capsule, new Vector3(-0.18f, 0.46f, 0f), new Vector3(0.2f, 0.72f, 0.2f), playerMaterial);
            CreateTargetHitPart(target.transform, health, "Right Leg", CombatHitZone.Leg, PrimitiveType.Capsule, new Vector3(0.18f, 0.46f, 0f), new Vector3(0.2f, 0.72f, 0.2f), playerMaterial);
        }

        private void CreateTargetHitPart(Transform parent, Health health, string partName, CombatHitZone zone, PrimitiveType primitive, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = $"{parent.name} {partName}";
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material != null ? material : playerMaterial;
            }

            CombatHitbox hitbox = part.AddComponent<CombatHitbox>();
            hitbox.Configure(zone, health);
        }

        private void CreateSurfaceTestPanel(Transform parent, Vector3 position, CombatSurfaceType surfaceType, Material material, string label)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = $"M24B {label} Surface Panel";
            panel.transform.SetParent(parent, true);
            panel.transform.position = position;
            panel.transform.localScale = new Vector3(1.75f, 1.4f, 0.18f);
            Renderer renderer = panel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material != null ? material : coverMaterial;
            }

            panel.AddComponent<CombatSurface>().Configure(surfaceType);
        }

        private static void SetLayerRecursive(GameObject target, int layer)
        {
            if (target == null || layer < 0)
            {
                return;
            }

            target.layer = layer;
            for (int i = 0; i < target.transform.childCount; i++)
            {
                SetLayerRecursive(target.transform.GetChild(i).gameObject, layer);
            }
        }

        private LootItem CreateLootTemplate(string objectName, LootKind kind, int amount, string displayName, Vector3 scale, Material material, int backpackCost, LootRarity rarity = LootRarity.Common)
        {
            GameObject loot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runtimeObjects.Add(loot);
            loot.name = objectName;
            loot.layer = lootLayer;
            loot.transform.localScale = scale;
            loot.GetComponent<Renderer>().material = material;
            TagVisualModule(loot, RuntimeVisualModuleKind.Loot, objectName);
            DecorateLootVisual(loot.transform, kind, objectName);

            BoxCollider boxCollider = loot.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            LootItem item = loot.AddComponent<LootItem>();
            item.Configure(kind, amount, displayName, backpackCost, rarity);
            loot.SetActive(false);
            return item;
        }

        private void DecorateLootVisual(Transform parent, LootKind kind, string prefix)
        {
            if (parent == null)
            {
                return;
            }

            switch (kind)
            {
                case LootKind.Pistol:
                case LootKind.AssaultRifle:
                case LootKind.SMG:
                case LootKind.Sniper:
                case LootKind.Shotgun:
                    AddLootVisualPart(parent, $"{prefix} Weapon Crate Lid", new Vector3(0f, 0.34f, 0f), new Vector3(1.14f, 0.12f, 0.86f), lootCrateMaterial);
                    AddLootVisualPart(parent, $"{prefix} Barrel Hint", new Vector3(0.22f, 0.48f, -0.38f), new Vector3(0.18f, 0.12f, 0.74f), lootWeaponMaterial);
                    AddLootVisualPart(parent, $"{prefix} Grip Hint", new Vector3(-0.26f, 0.44f, 0.18f), new Vector3(0.16f, 0.32f, 0.18f), vehicleAccentMaterial);
                    break;
                case LootKind.LightAmmo:
                case LootKind.MediumAmmo:
                case LootKind.HeavyAmmo:
                case LootKind.ShellAmmo:
                    AddLootVisualPart(parent, $"{prefix} Ammo Strap", new Vector3(0f, 0.29f, 0f), new Vector3(1.08f, 0.09f, 0.18f), vehicleAccentMaterial);
                    AddLootVisualPart(parent, $"{prefix} Ammo Tag", new Vector3(0.38f, 0.34f, -0.28f), new Vector3(0.24f, 0.06f, 0.18f), laneMarkMaterial);
                    break;
                case LootKind.Medkit:
                case LootKind.Bandage:
                    AddLootVisualPart(parent, $"{prefix} Medical Cross Vertical", new Vector3(0f, 0.33f, -0.02f), new Vector3(0.16f, 0.1f, 0.58f), fuelCanMaterial);
                    AddLootVisualPart(parent, $"{prefix} Medical Cross Horizontal", new Vector3(0f, 0.34f, -0.02f), new Vector3(0.56f, 0.1f, 0.16f), fuelCanMaterial);
                    break;
                case LootKind.ArmorPlate:
                case LootKind.ArmorVest:
                    AddLootVisualPart(parent, $"{prefix} Armor Chest Plate", new Vector3(0f, 0.36f, 0f), new Vector3(0.66f, 0.12f, 0.72f), armorDisplayMaterial);
                    AddLootVisualPart(parent, $"{prefix} Armor Shoulder Plate", new Vector3(0.48f, 0.34f, 0f), new Vector3(0.24f, 0.1f, 0.45f), vehicleAccentMaterial);
                    break;
                case LootKind.Helmet:
                    AddLootVisualPart(parent, $"{prefix} Helmet Dome", new Vector3(0f, 0.38f, 0f), new Vector3(0.62f, 0.26f, 0.62f), playerMaterial, PrimitiveType.Sphere);
                    AddLootVisualPart(parent, $"{prefix} Helmet Visor", new Vector3(0f, 0.35f, -0.32f), new Vector3(0.52f, 0.12f, 0.08f), windowMaterial);
                    break;
                case LootKind.Backpack:
                    AddLootVisualPart(parent, $"{prefix} Pack Pocket", new Vector3(0f, 0.36f, -0.18f), new Vector3(0.46f, 0.24f, 0.12f), playerAccentMaterial);
                    AddLootVisualPart(parent, $"{prefix} Pack Roll", new Vector3(0f, 0.62f, 0f), new Vector3(0.18f, 0.42f, 0.18f), coverMaterial, PrimitiveType.Cylinder);
                    break;
            }
        }

        private Renderer AddLootVisualPart(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material, PrimitiveType primitive = PrimitiveType.Cube)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = lootLayer;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.material = material;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }

            return renderer;
        }

        private SafeZoneController BuildSafeZone(Transform player, Health health)
        {
            GameObject zoneObject = new GameObject("SafeZone");
            runtimeObjects.Add(zoneObject);
            zoneObject.transform.position = Vector3.zero;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            runtimeObjects.Add(visual);
            visual.name = "ZoneVisual";
            visual.layer = zoneLayer;
            visual.transform.SetParent(zoneObject.transform, false);
            visual.GetComponent<Renderer>().material = zoneMaterial;
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                DestroyRuntimeObject(visualCollider);
            }

            SafeZoneController zone = zoneObject.AddComponent<SafeZoneController>();
            GameObject nextVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            runtimeObjects.Add(nextVisual);
            nextVisual.name = "ZoneNextVisual";
            nextVisual.layer = zoneLayer;
            nextVisual.transform.SetParent(zoneObject.transform, false);
            nextVisual.GetComponent<Renderer>().material = CreateMaterial("Runtime Next Zone", new Color(1f, 0.92f, 0.22f, 0.16f));
            Collider nextVisualCollider = nextVisual.GetComponent<Collider>();
            if (nextVisualCollider != null)
            {
                DestroyRuntimeObject(nextVisualCollider);
            }

            zone.ConfigureForRuntime(player, health, visual.transform, nextVisual.transform);
            zone.ConfigureZoneScale(178f, 14f, 56f);
            zone.ConfigureMapBounds(new Vector2(232f, 232f), true);
            return zone;
        }

        private LootSpawner BuildLootSpawner(LootItem[] lootPrefabs)
        {
            GameObject spawnerObject = new GameObject("LootSpawner");
            runtimeObjects.Add(spawnerObject);
            LootSpawner spawner = spawnerObject.AddComponent<LootSpawner>();
            spawner.ConfigureForRuntime(lootPrefabs, 212, new Vector2(510f, 510f), ~0);
            spawner.ConfigureClusters(new[]
            {
                new Vector3(-8f, 0f, -28f),
                new Vector3(-214f, 0f, 72f),
                new Vector3(214f, 0f, -72f),
                new Vector3(210f, 0f, -214f),
                new Vector3(116f, 0f, 122f),
                new Vector3(18f, 0f, -72f),
                new Vector3(-56f, 0f, -206f),
                new Vector3(82f, 0f, 34f),
                new Vector3(-172f, 0f, -28f),
                new Vector3(-118f, 0f, -176f),
                new Vector3(178f, 0f, 54f),
                new Vector3(178f, 0f, -172f),
                new Vector3(-196f, 0f, 48f),
                new Vector3(98f, 0f, 132f),
                new Vector3(208f, 0f, 60f),
                new Vector3(194f, 0f, -164f),
                new Vector3(-42f, 0f, 186f),
                new Vector3(-34f, 0f, -128f),
                new Vector3(34f, 0f, -92f)
            }, 32f, 0.76f);
            return spawner;
        }

        private VehicleController[] BuildVehicles(RuntimeUIRefs uiRefs)
        {
            return new[]
            {
                CreateVehicle("Milestone4 Jeep", VehicleKind.Jeep, "Jeep", new Vector3(-42f, 0.35f, -6f), Quaternion.Euler(0f, 88f, 0f), new Vector3(2.7f, 0.9f, 4.3f), 19f, uiRefs),
                CreateVehicle("Milestone4 Buggy", VehicleKind.Buggy, "Buggy", new Vector3(112f, 0.35f, -64f), Quaternion.Euler(0f, -20f, 0f), new Vector3(2.4f, 0.75f, 3.5f), 22f, uiRefs),
                CreateVehicle("Milestone4 Motorcycle", VehicleKind.Motorcycle, "Motorcycle", new Vector3(-132f, 0.28f, 96f), Quaternion.Euler(0f, 38f, 0f), new Vector3(1.1f, 0.7f, 2.8f), 26f, uiRefs),
                CreateVehicle("Milestone4 Coastal Jeep", VehicleKind.Jeep, "Coast Jeep", new Vector3(16f, 0.35f, 168f), Quaternion.Euler(0f, -86f, 0f), new Vector3(2.7f, 0.9f, 4.3f), 19f, uiRefs),
                CreateVehicle("Milestone11 Pickup Truck", VehicleKind.PickupTruck, "Pickup Truck", new Vector3(-18f, 0.35f, 166f), Quaternion.Euler(0f, 12f, 0f), new Vector3(2.85f, 0.95f, 4.85f), 18f, uiRefs)
            };
        }

        private VehicleController CreateVehicle(string objectName, VehicleKind kind, string displayName, Vector3 position, Quaternion rotation, Vector3 bodyScale, float speed, RuntimeUIRefs uiRefs)
        {
            GameObject spawn = new GameObject($"{objectName} Spawn Point");
            runtimeObjects.Add(spawn);
            spawn.transform.SetPositionAndRotation(position, rotation);

            GameObject root = new GameObject(objectName);
            runtimeObjects.Add(root);
            root.layer = groundLayer;
            root.transform.SetPositionAndRotation(position, rotation);
            TagVisualModule(root, RuntimeVisualModuleKind.Vehicle, objectName);

            BoxCollider rootCollider = root.AddComponent<BoxCollider>();
            rootCollider.center = new Vector3(0f, bodyScale.y * 0.55f, 0f);
            rootCollider.size = new Vector3(bodyScale.x, bodyScale.y * 1.25f, bodyScale.z);

            Renderer body = CreateVehiclePart($"{objectName} Body", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.74f, 0f), bodyScale, vehicleMaterial);
            Renderer cabin = null;
            if (kind != VehicleKind.Motorcycle)
            {
                cabin = CreateVehiclePart($"{objectName} Cabin", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.36f, -0.28f), new Vector3(bodyScale.x * 0.62f, 0.72f, bodyScale.z * 0.42f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Windshield", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.44f, -bodyScale.z * 0.5f), new Vector3(bodyScale.x * 0.45f, 0.34f, 0.08f), windowMaterial);
                CreateVehiclePart($"{objectName} Front Bumper", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.62f, -bodyScale.z * 0.56f), new Vector3(bodyScale.x * 0.82f, 0.18f, 0.18f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Rear Bumper", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.62f, bodyScale.z * 0.56f), new Vector3(bodyScale.x * 0.78f, 0.18f, 0.18f), vehicleAccentMaterial);

                if (kind == VehicleKind.PickupTruck)
                {
                    CreateVehiclePart($"{objectName} Cargo Bed Floor", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.19f, bodyScale.z * 0.22f), new Vector3(bodyScale.x * 0.78f, 0.16f, bodyScale.z * 0.42f), vehicleAccentMaterial);
                    CreateVehiclePart($"{objectName} Cargo Bed Left Rail", root.transform, PrimitiveType.Cube, new Vector3(-bodyScale.x * 0.43f, 1.44f, bodyScale.z * 0.22f), new Vector3(0.16f, 0.42f, bodyScale.z * 0.46f), vehicleAccentMaterial);
                    CreateVehiclePart($"{objectName} Cargo Bed Right Rail", root.transform, PrimitiveType.Cube, new Vector3(bodyScale.x * 0.43f, 1.44f, bodyScale.z * 0.22f), new Vector3(0.16f, 0.42f, bodyScale.z * 0.46f), vehicleAccentMaterial);
                    CreateVehiclePart($"{objectName} Tailgate", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.34f, bodyScale.z * 0.48f), new Vector3(bodyScale.x * 0.78f, 0.42f, 0.14f), vehicleAccentMaterial);
                }
                else if (kind == VehicleKind.Buggy)
                {
                    CreateVehiclePart($"{objectName} Roll Cage Front", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.86f, -bodyScale.z * 0.15f), new Vector3(bodyScale.x * 0.72f, 0.14f, 0.14f), vehicleAccentMaterial);
                    CreateVehiclePart($"{objectName} Roll Cage Rear", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.84f, bodyScale.z * 0.24f), new Vector3(bodyScale.x * 0.72f, 0.14f, 0.14f), vehicleAccentMaterial);
                }
                else
                {
                    CreateVehiclePart($"{objectName} Spare Tire", root.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.12f, bodyScale.z * 0.58f), new Vector3(0.48f, 0.18f, 0.48f), vehicleAccentMaterial).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }
            else
            {
                CreateVehiclePart($"{objectName} Handlebar", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.2f, -1.08f), new Vector3(1.05f, 0.12f, 0.14f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Rider Pad", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.02f, 0.22f), new Vector3(0.72f, 0.16f, 0.96f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Engine Block", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.72f, 0.18f), new Vector3(0.54f, 0.42f, 0.62f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Front Fork", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.83f, -1.02f), new Vector3(0.18f, 0.78f, 0.14f), vehicleAccentMaterial);
            }

            float wheelZ = bodyScale.z * 0.42f;
            float wheelX = bodyScale.x * 0.56f;
            List<Transform> wheels = new List<Transform>();
            if (kind == VehicleKind.Motorcycle)
            {
                wheels.Add(CreateVehicleWheel($"{objectName} Front Wheel", root.transform, new Vector3(0f, 0.42f, -wheelZ)));
                wheels.Add(CreateVehicleWheel($"{objectName} Rear Wheel", root.transform, new Vector3(0f, 0.42f, wheelZ)));
            }
            else
            {
                wheels.Add(CreateVehicleWheel($"{objectName} Front Left Wheel", root.transform, new Vector3(-wheelX, 0.42f, -wheelZ)));
                wheels.Add(CreateVehicleWheel($"{objectName} Front Right Wheel", root.transform, new Vector3(wheelX, 0.42f, -wheelZ)));
                wheels.Add(CreateVehicleWheel($"{objectName} Rear Left Wheel", root.transform, new Vector3(-wheelX, 0.42f, wheelZ)));
                wheels.Add(CreateVehicleWheel($"{objectName} Rear Right Wheel", root.transform, new Vector3(wheelX, 0.42f, wheelZ)));
                CreateVehiclePart($"{objectName} Front Suspension Bar", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.48f, -wheelZ), new Vector3(bodyScale.x * 1.15f, 0.12f, 0.12f), vehicleAccentMaterial);
                CreateVehiclePart($"{objectName} Rear Suspension Bar", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.48f, wheelZ), new Vector3(bodyScale.x * 1.15f, 0.12f, 0.12f), vehicleAccentMaterial);
            }

            GameObject seat = new GameObject($"{objectName} Seat");
            runtimeObjects.Add(seat);
            seat.transform.SetParent(root.transform, false);
            seat.transform.localPosition = new Vector3(0f, kind == VehicleKind.Motorcycle ? 1.18f : 1.28f, 0.15f);

            GameObject exit = new GameObject($"{objectName} Exit");
            runtimeObjects.Add(exit);
            exit.transform.SetParent(root.transform, false);
            exit.transform.localPosition = new Vector3(bodyScale.x * 0.85f + 0.9f, 0.2f, 0f);

            VehicleController controller = root.AddComponent<VehicleController>();
            controller.ConfigureSeatPoints(seat.transform, exit.transform);
            controller.ConfigureForRuntime(kind, displayName, speed, uiRefs.Joystick, uiRefs.ThrottleButton, uiRefs.BrakeButton, uiRefs.VehicleStatusText);
            controller.ConfigureVisuals(body.transform, wheels.ToArray());

            LODGroup lodGroup = root.AddComponent<LODGroup>();
            List<Renderer> lodRenderers = new List<Renderer> { body };
            if (cabin != null)
            {
                lodRenderers.Add(cabin);
            }

            lodGroup.SetLODs(new[]
            {
                new LOD(0.16f, root.GetComponentsInChildren<Renderer>()),
                new LOD(0.05f, lodRenderers.ToArray())
            });
            lodGroup.RecalculateBounds();

            return controller;
        }

        private Renderer CreateVehiclePart(string objectName, Transform parent, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            runtimeObjects.Add(part);
            part.name = objectName;
            part.layer = groundLayer;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.material = material;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyRuntimeObject(collider);
            }

            return renderer;
        }

        private Transform CreateVehicleWheel(string objectName, Transform parent, Vector3 localPosition)
        {
            Renderer wheel = CreateVehiclePart(objectName, parent, PrimitiveType.Cylinder, localPosition, new Vector3(0.36f, 0.16f, 0.36f), vehicleAccentMaterial);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            return wheel.transform;
        }

        private BattleRoyaleMatchFlow BuildMatchFlow(PlayerRefs playerRefs, RuntimeUIRefs uiRefs, LootItem[] lootPrefabs, BotManager botManager)
        {
            GameObject flowObject = new GameObject("Milestone4 Battle Royale Flow");
            runtimeObjects.Add(flowObject);
            GameObject milestone13Marker = new GameObject("Milestone13 Complete Battle Royale Flow");
            runtimeObjects.Add(milestone13Marker);
            milestone13Marker.transform.SetParent(flowObject.transform, false);
            GameObject milestone18Marker = new GameObject("Milestone18 Complete Battle Royale Match Flow");
            runtimeObjects.Add(milestone18Marker);
            milestone18Marker.transform.SetParent(flowObject.transform, false);
            BattleRoyaleMatchFlow flow = flowObject.AddComponent<BattleRoyaleMatchFlow>();
            GameObject plane = CreatePlaneVisual();
            GameObject parachute = CreateParachuteVisual();
            Transform waitingArea = CreateWaitingArea();

            Vector3[] starts =
            {
                new Vector3(-205f, 26f, -190f),
                new Vector3(205f, 26f, -178f),
                new Vector3(-198f, 26f, 188f)
            };

            Vector3[] ends =
            {
                new Vector3(205f, 26f, 190f),
                new Vector3(-205f, 26f, 178f),
                new Vector3(198f, 26f, -188f)
            };

            flow.ConfigureForRuntime(playerRefs.Transform, playerRefs.Controller, uiRefs.UIManager, plane, parachute, lootPrefabs, airdropMaterial, starts, ends, waitingArea, botManager);
            flow.ConfigureBattleRoyaleDirector(new Vector2(250f, 250f), 138f, 15f, 6f);
            return flow;
        }

        private Transform CreateWaitingArea()
        {
            GameObject platform = CreateWorldBlock("Milestone13 Waiting Area Platform", new Vector3(-238f, 0.12f, -238f), new Vector3(32f, 0.24f, 32f), roadMaterial);
            TagVisualModule(platform, RuntimeVisualModuleKind.Prop, "M13_Waiting_Area");
            CreateWorldBlock("Milestone13 Waiting Area North Rail", new Vector3(-238f, 1.05f, -254f), new Vector3(32f, 1.7f, 0.25f), fenceMaterial);
            CreateWorldBlock("Milestone13 Waiting Area South Rail", new Vector3(-238f, 1.05f, -222f), new Vector3(32f, 1.7f, 0.25f), fenceMaterial);
            CreateWorldBlock("Milestone13 Waiting Area East Rail", new Vector3(-222f, 1.05f, -238f), new Vector3(0.25f, 1.7f, 32f), fenceMaterial);
            CreateWorldBlock("Milestone13 Waiting Area Supply Bench", new Vector3(-246f, 0.7f, -238f), new Vector3(5.2f, 1.0f, 1.4f), coverMaterial);

            GameObject spawn = new GameObject("Milestone13 Waiting Area Spawn");
            runtimeObjects.Add(spawn);
            spawn.transform.SetPositionAndRotation(new Vector3(-238f, 1.25f, -238f), Quaternion.Euler(0f, 42f, 0f));
            return spawn.transform;
        }

        private GameObject CreatePlaneVisual()
        {
            GameObject root = new GameObject("Milestone4 Original Drop Plane");
            runtimeObjects.Add(root);
            CreateVehiclePart("Drop Plane Fuselage", root.transform, PrimitiveType.Cube, Vector3.zero, new Vector3(3.2f, 0.62f, 9.5f), vehicleMaterial);
            CreateVehiclePart("Drop Plane Wing", root.transform, PrimitiveType.Cube, new Vector3(0f, 0f, -0.4f), new Vector3(12.5f, 0.18f, 2.1f), vehicleAccentMaterial);
            CreateVehiclePart("Drop Plane Tail", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, 4.3f), new Vector3(3.6f, 1.2f, 0.38f), vehicleAccentMaterial);
            root.SetActive(false);
            return root;
        }

        private GameObject CreateParachuteVisual()
        {
            GameObject root = new GameObject("Milestone4 Parachute Visual");
            runtimeObjects.Add(root);
            CreateVehiclePart("Parachute Canopy", root.transform, PrimitiveType.Sphere, new Vector3(0f, 1.6f, 0f), new Vector3(2.8f, 0.55f, 2.8f), airdropMaterial);
            CreateVehiclePart("M19 Parachute Front Canopy Lip", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.42f, -1.1f), new Vector3(2.8f, 0.12f, 0.22f), vehicleAccentMaterial);
            CreateVehiclePart("M19 Parachute Rear Canopy Lip", root.transform, PrimitiveType.Cube, new Vector3(0f, 1.42f, 1.1f), new Vector3(2.8f, 0.12f, 0.22f), vehicleAccentMaterial);
            CreateVehiclePart("M19 Parachute Left Panel", root.transform, PrimitiveType.Cube, new Vector3(-0.9f, 1.52f, 0f), new Vector3(0.08f, 0.12f, 2.35f), airdropMaterial);
            CreateVehiclePart("M19 Parachute Right Panel", root.transform, PrimitiveType.Cube, new Vector3(0.9f, 1.52f, 0f), new Vector3(0.08f, 0.12f, 2.35f), airdropMaterial);
            CreateVehiclePart("Parachute Cord A", root.transform, PrimitiveType.Cube, new Vector3(-0.68f, 0.74f, 0f), new Vector3(0.05f, 1.6f, 0.05f), vehicleAccentMaterial);
            CreateVehiclePart("Parachute Cord B", root.transform, PrimitiveType.Cube, new Vector3(0.68f, 0.74f, 0f), new Vector3(0.05f, 1.6f, 0.05f), vehicleAccentMaterial);
            CreateVehiclePart("M19 Parachute Stabilizer Cord L", root.transform, PrimitiveType.Cube, new Vector3(-1.05f, 0.72f, -0.34f), new Vector3(0.04f, 1.52f, 0.04f), vehicleAccentMaterial).transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
            CreateVehiclePart("M19 Parachute Stabilizer Cord R", root.transform, PrimitiveType.Cube, new Vector3(1.05f, 0.72f, -0.34f), new Vector3(0.04f, 1.52f, 0.04f), vehicleAccentMaterial).transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
            root.SetActive(false);
            return root;
        }

        private BotManager BuildBotManager(BotAI botPrefab)
        {
            GameObject managerObject = new GameObject("BotManager");
            runtimeObjects.Add(managerObject);
            BotManager botManager = managerObject.AddComponent<BotManager>();
            botManager.ConfigureForRuntime(botPrefab, BotCount, new Vector2(382f, 382f));
            botManager.SetCoverPoints(runtimeCoverPoints.ToArray());
            return botManager;
        }

        private void WireButtons(RuntimeUIRefs uiRefs, PlayerRefs playerRefs, GameManager gameManager)
        {
            uiRefs.StartButton.onClick.AddListener(gameManager.StartMatch);
            uiRefs.GameOverRestartButton.onClick.AddListener(gameManager.RestartMatch);
            uiRefs.VictoryRestartButton.onClick.AddListener(gameManager.RestartMatch);
            uiRefs.CloseSummaryButton.onClick.AddListener(gameManager.CloseMatchSummary);
            uiRefs.JumpButton.onClick.AddListener(gameManager.HandleJumpButton);
            if (playerRefs.ReliableMovement != null)
            {
                uiRefs.JumpButton.onClick.AddListener(playerRefs.ReliableMovement.Jump);
                uiRefs.CrouchButton.onClick.AddListener(playerRefs.ReliableMovement.ToggleCrouch);
                uiRefs.SprintButton.onStateChanged.AddListener(playerRefs.ReliableMovement.SetSprint);
            }
            else
            {
                uiRefs.CrouchButton.onClick.AddListener(playerRefs.Controller.ToggleCrouch);
                uiRefs.SprintButton.onStateChanged.AddListener(playerRefs.Controller.SetSprint);
            }
            uiRefs.ReloadButton.onClick.AddListener(playerRefs.Weapons.ReloadPressed);
            uiRefs.MedkitButton.onClick.AddListener(playerRefs.Inventory.UseMedkit);
            uiRefs.SwitchButton.onClick.AddListener(playerRefs.Weapons.SelectNextWeapon);
            uiRefs.ShoulderButton.onClick.AddListener(playerRefs.Controller.ToggleShoulder);
            uiRefs.ProneButton.onClick.AddListener(playerRefs.Controller.ToggleProne);
            uiRefs.DriveButton.onClick.AddListener(playerRefs.VehicleInteractor.ToggleVehicle);
            uiRefs.InventoryButton.onClick.AddListener(uiRefs.UIManager.ToggleInventory);
            uiRefs.CloseInventoryButton.onClick.AddListener(uiRefs.UIManager.HideInventory);
            uiRefs.FireButton.onPressed.AddListener(playerRefs.Weapons.FirePressed);
            uiRefs.FireButton.onReleased.AddListener(playerRefs.Weapons.FireReleased);
            uiRefs.AimButton.onStateChanged.AddListener(playerRefs.Controller.SetAim);
            uiRefs.SensitivitySlider.onValueChanged.AddListener(playerRefs.Controller.SetSensitivity);
            uiRefs.AimSensitivitySlider.onValueChanged.AddListener(playerRefs.Controller.SetAimSensitivity);
            uiRefs.ScopeSensitivitySlider.onValueChanged.AddListener(playerRefs.Controller.SetScopeSensitivity);
            uiRefs.GraphicsSlider.onValueChanged.AddListener(SetRuntimeGraphicsQuality);
            uiRefs.AudioSlider.onValueChanged.AddListener(value =>
            {
                RuntimeAudioBank.Instance?.SetMasterVolume(value);
                RuntimeAmbientSoundscape.Instance?.SetMasterVolume(value);
            });
            uiRefs.ButtonScaleSlider.onValueChanged.AddListener(uiRefs.ButtonLayout.SetButtonScale);
            playerRefs.Weapons.onHitConfirmed.AddListener((hitPoint, damage) => uiRefs.HitMarker.ShowHit());
            playerRefs.Weapons.onHeadshotConfirmed.AddListener(uiRefs.HeadshotMarker.ShowHeadshot);
            playerRefs.Weapons.onFired.AddListener(uiRefs.UIManager.PulseCrosshair);

            uiRefs.PauseButton.onClick.AddListener(() =>
            {
                Time.timeScale = 0f;
                playerRefs.Controller.ControlsEnabled = false;
                uiRefs.UIManager.HideInventory();
                uiRefs.SettingsPanel.SetActive(false);
                uiRefs.PausePanel.SetActive(true);
            });

            uiRefs.ResumeButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                playerRefs.Controller.ControlsEnabled = true;
                uiRefs.SettingsPanel.SetActive(false);
                uiRefs.PausePanel.SetActive(false);
            });

            uiRefs.SettingsButton.onClick.AddListener(() => uiRefs.SettingsPanel.SetActive(true));
            uiRefs.CloseSettingsButton.onClick.AddListener(() => uiRefs.SettingsPanel.SetActive(false));
        }

        private static void SetRuntimeGraphicsQuality(float normalizedValue)
        {
            int qualityCount = QualitySettings.names != null ? QualitySettings.names.Length : 0;
            if (qualityCount > 0)
            {
                int level = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * (qualityCount - 1)), 0, qualityCount - 1);
                QualitySettings.SetQualityLevel(level, true);
            }

            QualitySettings.shadowDistance = Mathf.Lerp(28f, 70f, Mathf.Clamp01(normalizedValue));
            QualitySettings.lodBias = Mathf.Lerp(0.7f, 1.25f, Mathf.Clamp01(normalizedValue));
        }

        private GameObject CreatePanel(string objectName, Transform parent, Color color)
        {
            GameObject panel = CreateUIObject(objectName, parent, Vector2.zero, new Vector2(1920f, 1080f));
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private GameObject CreateUIObject(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject uiObject = new GameObject(objectName);
            uiObject.transform.SetParent(parent, false);
            RectTransform rect = uiObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return uiObject;
        }

        private Image CreateImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject imageObject = CreateUIObject(objectName, parent, anchoredPosition, size);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string objectName, Transform parent, Font font, string value, int size, TextAnchor anchor, Vector2 anchoredPosition, Vector2 dimensions)
        {
            GameObject textObject = CreateUIObject(objectName, parent, anchoredPosition, dimensions);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(string objectName, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject buttonObject = CreateUIObject(objectName, parent, anchoredPosition, size);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => RuntimeAudioBank.Instance?.PlayUiClick());

            Text buttonText = CreateText("Label", buttonObject.transform, font, label, 26, TextAnchor.MiddleCenter, Vector2.zero, size);
            buttonText.color = Color.white;
            Outline outline = buttonText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            return button;
        }

        private InventoryDragDropSlot CreateInventorySlot(string objectName, Transform parent, Font font, string label, Vector2 anchoredPosition, int slotIndex)
        {
            GameObject slotObject = CreateUIObject(objectName, parent, anchoredPosition, new Vector2(260f, 68f));
            Image image = slotObject.AddComponent<Image>();
            image.color = new Color(0.07f, 0.11f, 0.13f, 0.9f);
            Outline outline = slotObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.52f, 0.58f, 0.75f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            Text slotText = CreateText("Label", slotObject.transform, font, label, 24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(250f, 58f));
            slotText.color = new Color(0.86f, 0.96f, 1f);
            slotText.raycastTarget = false;

            InventoryDragDropSlot slot = slotObject.AddComponent<InventoryDragDropSlot>();
            slot.Configure(slotIndex, slotText, image);
            return slot;
        }

        private MobileHoldButton CreateHoldButton(string objectName, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject buttonObject = CreateUIObject(objectName, parent, anchoredPosition, size);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            MobileHoldButton holdButton = buttonObject.AddComponent<MobileHoldButton>();
            holdButton.onPressed.AddListener(() => RuntimeAudioBank.Instance?.PlayUiClick());

            Text buttonText = CreateText("Label", buttonObject.transform, font, label, 26, TextAnchor.MiddleCenter, Vector2.zero, size);
            buttonText.color = Color.white;
            Outline outline = buttonText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            return holdButton;
        }

        private Slider CreateSlider(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject sliderObject = CreateUIObject(objectName, parent, anchoredPosition, size);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            Image background = CreateImage("Background", sliderObject.transform, Vector2.zero, size, new Color(0.06f, 0.09f, 0.11f, 0.95f));
            RectTransform fillArea = CreateUIObject("Fill Area", sliderObject.transform, Vector2.zero, size - new Vector2(18f, 16f)).GetComponent<RectTransform>();
            Image fill = CreateImage("Fill", fillArea, Vector2.zero, fillArea.sizeDelta, new Color(0.18f, 0.62f, 0.74f, 1f));
            Image handle = CreateImage("Handle", sliderObject.transform, Vector2.zero, new Vector2(42f, 58f), new Color(0.88f, 0.96f, 1f, 1f));

            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            background.raycastTarget = true;
            return slider;
        }

        private FloatingJoystick CreateJoystick(Transform parent)
        {
            GameObject root = CreateUIObject("MovementJoystick", parent, new Vector2(-690f, -300f), new Vector2(240f, 240f));
            Image backgroundImage = root.AddComponent<Image>();
            backgroundImage.color = new Color(0.05f, 0.08f, 0.1f, 0.42f);

            GameObject handleObject = CreateUIObject("Handle", root.transform, Vector2.zero, new Vector2(92f, 92f));
            Image handleImage = handleObject.AddComponent<Image>();
            handleImage.color = new Color(0.85f, 0.94f, 1f, 0.68f);

            FloatingJoystick joystick = root.AddComponent<FloatingJoystick>();
            joystick.Configure(root.GetComponent<RectTransform>(), handleObject.GetComponent<RectTransform>(), 92f, false);
            joystick.ConfigureAdvanced(0.12f, 1.14f);
            return joystick;
        }

        private struct RuntimeUIRefs
        {
            public UIManager UIManager;
            public FloatingJoystick Joystick;
            public MobileLookArea LookArea;
            public Text PickupPrompt;
            public Button StartButton;
            public Button GameOverRestartButton;
            public Button VictoryRestartButton;
            public Button CloseSummaryButton;
            public Button JumpButton;
            public Button CrouchButton;
            public Button ReloadButton;
            public Button MedkitButton;
            public Button SwitchButton;
            public Button ShoulderButton;
            public Button ProneButton;
            public Button DriveButton;
            public Button InventoryButton;
            public MobileHoldButton FireButton;
            public MobileHoldButton SprintButton;
            public MobileHoldButton AimButton;
            public MobileHoldButton ThrottleButton;
            public MobileHoldButton BrakeButton;
            public GameObject InventoryPanel;
            public GameObject PausePanel;
            public GameObject SettingsPanel;
            public Button PauseButton;
            public Button ResumeButton;
            public Button SettingsButton;
            public Button CloseInventoryButton;
            public Button CloseSettingsButton;
            public Slider SensitivitySlider;
            public Slider AimSensitivitySlider;
            public Slider ScopeSensitivitySlider;
            public Slider GraphicsSlider;
            public Slider AudioSlider;
            public Slider ButtonScaleSlider;
            public MobileButtonLayoutProfile ButtonLayout;
            public HitMarkerUI HitMarker;
            public HitMarkerUI HeadshotMarker;
            public RuntimeHudTelemetry HudTelemetry;
            public RuntimeDeveloperPanel DeveloperPanel;
            public Text CompassText;
            public Text VehiclePrompt;
            public Text VehicleStatusText;
            public RectTransform MinimapArrow;
            public RectTransform MinimapZoneRing;
            public RectTransform MinimapNextZoneRing;
            public Text MinimapLabel;
            public Text MinimapLocation;
        }

        private struct PlayerRefs
        {
            public Transform Transform;
            public Transform SpawnPoint;
            public ThirdPersonMobileController Controller;
            public ReliablePlayerMovement ReliableMovement;
            public Health Health;
            public WeaponController Weapons;
            public PlayerInventory Inventory;
            public PlayerEquipment Equipment;
            public VehicleInteractor VehicleInteractor;
            public KnockdownReviveState Knockdown;
        }
    }
}
