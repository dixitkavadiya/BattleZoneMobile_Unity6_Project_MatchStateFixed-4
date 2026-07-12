#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BattleZoneMobile.Editor
{
    public static class BattleZoneVisualSliceAssetGenerator
    {
        private const string RootFolder = "Assets/BattleZoneMobile/ArtPipeline";
        private const string MaterialFolder = RootFolder + "/Materials";
        private const string PrefabFolder = RootFolder + "/PrefabSlots";
        private const string DefinitionFolder = RootFolder + "/VisualDefinitions";
        private const string CatalogPath = DefinitionFolder + "/BattleZoneVisualCatalog.asset";

        [MenuItem("BattleZone Mobile/Generate Visual Slice Art Pipeline Assets")]
        public static void GenerateMenu()
        {
            Generate(false);
        }

        public static void GenerateAndExit()
        {
            Generate(true);
        }

        private static void Generate(bool exitWhenDone)
        {
            EnsureFolder(RootFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(DefinitionFolder);

            Material tacticalCloth = CreateMaterial("MAT_VS_TacticalCloth", new Color(0.11f, 0.16f, 0.15f));
            Material armor = CreateMaterial("MAT_VS_ArmorGraphite", new Color(0.08f, 0.10f, 0.11f));
            Material metal = CreateMaterial("MAT_VS_Gunmetal", new Color(0.13f, 0.15f, 0.16f));
            Material asphalt = CreateMaterial("MAT_VS_Asphalt", new Color(0.09f, 0.10f, 0.10f));
            Material concrete = CreateMaterial("MAT_VS_Concrete", new Color(0.42f, 0.44f, 0.42f));
            Material foliage = CreateMaterial("MAT_VS_Foliage", new Color(0.13f, 0.32f, 0.17f));
            Material lootBlue = CreateMaterial("MAT_VS_LootBlue", new Color(0.09f, 0.22f, 0.34f));

            List<BattleZoneVisualAssetDefinition> definitions = new List<BattleZoneVisualAssetDefinition>
            {
                CreateDefinition(
                    "CHR_TacticalOperator_VS",
                    "Tactical Operator Visual Slot",
                    BattleZoneArtAssetCategory.Character,
                    CreateSlotPrefab("PF_ArtSlot_CHR_TacticalOperator", BattleZoneArtAssetCategory.Character, "CHR_TacticalOperator_VS", new[] { "Head", "Spine", "RightHand", "LeftHand", "Backpack", "MuzzleAnchor" }),
                    new[] { tacticalCloth, armor },
                    new[] { "Head", "Spine", "RightHand", "LeftHand", "Backpack", "MuzzleAnchor" },
                    7200,
                    3600,
                    1400,
                    new Vector2Int(1024, 1024),
                    "Replace the current runtime humanoid proxy with an original rigged tactical human. Keep socket names stable for weapons/camera/audio."),

                CreateDefinition(
                    "WPN_Rook17_AR_VS",
                    "Rook-17 Assault Rifle Visual Slot",
                    BattleZoneArtAssetCategory.Weapon,
                    CreateSlotPrefab("PF_ArtSlot_WPN_Rook17_AssaultRifle", BattleZoneArtAssetCategory.Weapon, "WPN_Rook17_AR_VS", new[] { "Muzzle", "Optic", "Magazine", "Grip", "ShellEject" }),
                    new[] { metal, armor },
                    new[] { "Muzzle", "Optic", "Magazine", "Grip", "ShellEject" },
                    2800,
                    1400,
                    550,
                    new Vector2Int(1024, 512),
                    "Original assault rifle silhouette. Do not trace or approximate existing commercial weapon/game silhouettes."),

                CreateDefinition(
                    "WPN_Sable9_Pistol_VS",
                    "Sable-9 Pistol Visual Slot",
                    BattleZoneArtAssetCategory.Weapon,
                    CreateSlotPrefab("PF_ArtSlot_WPN_Sable9_Pistol", BattleZoneArtAssetCategory.Weapon, "WPN_Sable9_Pistol_VS", new[] { "Muzzle", "Magazine", "ShellEject" }),
                    new[] { metal, armor },
                    new[] { "Muzzle", "Magazine", "ShellEject" },
                    1500,
                    800,
                    300,
                    new Vector2Int(512, 512),
                    "Original compact sidearm with separate muzzle, magazine, and shell ejection sockets."),

                CreateDefinition(
                    "BLD_EnterableHouse_VS",
                    "Enterable House Visual Slot",
                    BattleZoneArtAssetCategory.Building,
                    CreateSlotPrefab("PF_ArtSlot_BLD_EnterableHouse", BattleZoneArtAssetCategory.Building, "BLD_EnterableHouse_VS", new[] { "Doorway", "LootAnchor", "InteriorAudio" }),
                    new[] { concrete, armor },
                    new[] { "Doorway", "LootAnchor", "InteriorAudio" },
                    9000,
                    4200,
                    1500,
                    new Vector2Int(1024, 1024),
                    "Modular house kit with interior collision authored as separate wall pieces."),

                CreateDefinition(
                    "BLD_Warehouse_VS",
                    "Warehouse Visual Slot",
                    BattleZoneArtAssetCategory.Building,
                    CreateSlotPrefab("PF_ArtSlot_BLD_Warehouse", BattleZoneArtAssetCategory.Building, "BLD_Warehouse_VS", new[] { "DoorwayA", "DoorwayB", "LootAnchor", "CoverAnchor" }),
                    new[] { concrete, metal },
                    new[] { "DoorwayA", "DoorwayB", "LootAnchor", "CoverAnchor" },
                    12000,
                    6000,
                    2200,
                    new Vector2Int(1024, 1024),
                    "Warehouse shell and interior props should be modular and occlusion-friendly."),

                CreateDefinition(
                    "ENV_StreetBlock_VS",
                    "Street Block Visual Slot",
                    BattleZoneArtAssetCategory.Terrain,
                    CreateSlotPrefab("PF_ArtSlot_ENV_StreetBlock", BattleZoneArtAssetCategory.Terrain, "ENV_StreetBlock_VS", new[] { "RoadCenter", "SidewalkA", "SidewalkB", "MinimapLabel" }),
                    new[] { asphalt, concrete },
                    new[] { "RoadCenter", "SidewalkA", "SidewalkB", "MinimapLabel" },
                    8000,
                    3200,
                    1000,
                    new Vector2Int(1024, 1024),
                    "Road, curb, sidewalk, and decal atlas slot for the visual-slice block."),

                CreateDefinition(
                    "VEG_StreetTree_VS",
                    "Street Tree Visual Slot",
                    BattleZoneArtAssetCategory.Vegetation,
                    CreateSlotPrefab("PF_ArtSlot_VEG_StreetTree", BattleZoneArtAssetCategory.Vegetation, "VEG_StreetTree_VS", new[] { "TrunkBase", "CanopyCenter" }),
                    new[] { foliage },
                    new[] { "TrunkBase", "CanopyCenter" },
                    1800,
                    800,
                    220,
                    new Vector2Int(512, 512),
                    "Use low-poly cards/clustered mesh with wind disabled by default for mobile."),

                CreateDefinition(
                    "LOOT_FieldCrate_VS",
                    "Loot Crate Visual Slot",
                    BattleZoneArtAssetCategory.Loot,
                    CreateSlotPrefab("PF_ArtSlot_LOOT_FieldCrate", BattleZoneArtAssetCategory.Loot, "LOOT_FieldCrate_VS", new[] { "PickupAnchor", "GlowAnchor" }),
                    new[] { lootBlue, metal },
                    new[] { "PickupAnchor", "GlowAnchor" },
                    1200,
                    600,
                    220,
                    new Vector2Int(512, 512),
                    "Loot visuals must preserve pickup collider and prompt anchor positions.")
            };

            BattleZoneVisualCatalog catalog = AssetDatabase.LoadAssetAtPath<BattleZoneVisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<BattleZoneVisualCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.definitions = definitions.ToArray();
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BattleZone visual slice art pipeline assets generated.");

            if (exitWhenDone)
            {
                EditorApplication.Exit(0);
            }
        }

        private static BattleZoneVisualAssetDefinition CreateDefinition(
            string assetId,
            string displayName,
            BattleZoneArtAssetCategory category,
            GameObject prefab,
            Material[] materials,
            string[] sockets,
            int lod0Triangles,
            int lod1Triangles,
            int lod2Triangles,
            Vector2Int textureResolution,
            string notes)
        {
            string path = $"{DefinitionFolder}/VA_{assetId}.asset";
            BattleZoneVisualAssetDefinition definition = AssetDatabase.LoadAssetAtPath<BattleZoneVisualAssetDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<BattleZoneVisualAssetDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.assetId = assetId;
            definition.displayName = displayName;
            definition.category = category;
            definition.productionState = BattleZoneArtProductionState.ArtistReadySlot;
            definition.originalityNotes = "Original BattleZone Mobile art slot. Do not copy BGMI/PUBG assets, map layouts, UI, sounds, silhouettes, or branding.";
            definition.visualPrefab = prefab;
            definition.lod0Prefab = prefab;
            definition.lod1Prefab = prefab;
            definition.lod2Prefab = prefab;
            definition.materialPalette = materials;
            definition.maxTextureResolution = textureResolution;
            definition.androidCompression = "ASTC 6x6 target, ASTC 8x8 for low-memory props, ETC2 fallback when ASTC is unavailable.";
            definition.usesURPCompatibleShaders = true;
            definition.lodRequirement = new BattleZoneLODRequirement
            {
                lod0TriangleBudget = lod0Triangles,
                lod1TriangleBudget = lod1Triangles,
                lod2TriangleBudget = lod2Triangles,
                lod0ScreenRelativeHeight = 0.25f,
                lod1ScreenRelativeHeight = 0.09f,
                lod2ScreenRelativeHeight = 0.025f
            };
            definition.requiredSockets = sockets;
            definition.gameplayReady = false;
            definition.replacementNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject CreateSlotPrefab(string prefabName, BattleZoneArtAssetCategory category, string assetId, string[] socketNames)
        {
            string path = $"{PrefabFolder}/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject(prefabName);
            BattleZoneArtReplacementSlot slot = root.AddComponent<BattleZoneArtReplacementSlot>();
            Transform lod0 = CreateChild(root.transform, "LOD0_ART_MESH_SLOT");
            Transform lod1 = CreateChild(root.transform, "LOD1_ART_MESH_SLOT");
            Transform lod2 = CreateChild(root.transform, "LOD2_ART_MESH_SLOT");
            Transform socketsRoot = CreateChild(root.transform, "SOCKETS_DO_NOT_RENAME");
            Transform[] sockets = new Transform[socketNames.Length];
            for (int i = 0; i < socketNames.Length; i++)
            {
                Transform socket = CreateChild(socketsRoot, socketNames[i]);
                socket.gameObject.AddComponent<BattleZoneArtSocket>().ConfigureRuntime(socketNames[i], "Preserve this socket name when replacing the visual mesh.");
                sockets[i] = socket;
            }

            root.AddComponent<LODGroup>();
            slot.ConfigureRuntime(assetId, category, BattleZoneArtProductionState.ArtistReadySlot, null, lod0, lod1, lod2, sockets, true);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Transform CreateChild(Transform parent, string objectName)
        {
            GameObject child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            string path = $"{MaterialFolder}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                }

                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.32f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
