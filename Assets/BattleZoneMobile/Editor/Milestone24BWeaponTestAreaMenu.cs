#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleZoneMobile.Editor
{
    public static class Milestone24BWeaponTestAreaMenu
    {
        private const string ScenePath = "Assets/BattleZoneMobile/Scenes/BZ_Main.unity";
        private const string WeaponDataFolder = "Assets/BattleZoneMobile/Resources/WeaponData";

        [MenuItem("BattleZone Tools/Build Milestone 24B Weapon Test Area")]
        public static void BuildWeaponTestArea()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AdvancedWeaponData[] roster = LoadWeaponRoster();
            int lootLayer = LayerMask.NameToLayer("Loot");
            if (lootLayer < 0)
            {
                lootLayer = 0;
            }

            GameObject root = Milestone24BWeaponTestAreaBuilder.BuildOrRepair(
                roster,
                Milestone24BWeaponTestAreaBuilder.DefaultOrigin,
                lootLayer,
                CreatePreviewMaterial("M24B Editor Weapon Preview", new Color(0.22f, 0.24f, 0.27f, 1f)),
                CreatePreviewMaterial("M24B Editor Ammo Preview", new Color(0.94f, 0.76f, 0.22f, 1f)),
                CreatePreviewMaterial("M24B Editor Target Preview", new Color(0.52f, 0.60f, 0.66f, 1f)),
                CreatePreviewMaterial("M24B Editor Surface Preview", new Color(0.40f, 0.42f, 0.40f, 1f)),
                true);

            if (root != null)
            {
                EditorUtility.SetDirty(root);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Built {Milestone24BWeaponTestAreaBuilder.TestAreaObjectName} in BZ_Main with {roster.Length} weapon pickups.");
        }

        private static AdvancedWeaponData[] LoadWeaponRoster()
        {
            string[] guids = AssetDatabase.FindAssets("t:AdvancedWeaponData", new[] { WeaponDataFolder });
            List<AdvancedWeaponData> weapons = new List<AdvancedWeaponData>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AdvancedWeaponData data = AssetDatabase.LoadAssetAtPath<AdvancedWeaponData>(path);
                if (data != null)
                {
                    weapons.Add(data);
                }
            }

            weapons.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, System.StringComparison.OrdinalIgnoreCase));
            return weapons.ToArray();
        }

        private static Material CreatePreviewMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }
    }
}
#endif
