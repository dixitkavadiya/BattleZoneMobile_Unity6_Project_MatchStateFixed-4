#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BattleZoneMobile.Editor
{
    public static class BattleZoneProjectConfigurator
    {
        private const string SettingsFolder = "Assets/BattleZoneMobile/Settings";
        private const string PipelineAssetPath = SettingsFolder + "/BattleZoneMobile_URP.asset";
        private const string RendererAssetPath = SettingsFolder + "/BattleZoneMobile_UniversalRenderer.asset";

        [MenuItem("BattleZone Mobile/Configure Unity 6 URP Project")]
        public static void ConfigureProject()
        {
            ConfigureProject(true);
        }

        public static void ConfigureProject(bool saveAssets)
        {
            EnsureFolder(SettingsFolder);

            ScriptableRendererData rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererAssetPath);
            if (rendererData == null)
            {
                rendererData = CreateUniversalRendererData(RendererAssetPath);
            }

            UniversalRenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
            }

            EnsureRendererData(pipelineAsset, rendererData);
            ConfigurePipelineAsset(pipelineAsset);
            AssignRenderPipeline(pipelineAsset);
            ConfigureQualityDefaults();

            EditorUtility.SetDirty(pipelineAsset);
            if (rendererData != null)
            {
                EditorUtility.SetDirty(rendererData);
            }

            if (saveAssets)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static ScriptableRendererData CreateUniversalRendererData(string path)
        {
            MethodInfo createRendererAsset = typeof(UniversalRenderPipelineAsset).GetMethod(
                "CreateRendererAsset",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (createRendererAsset != null)
            {
                Type rendererType = typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.RendererType");
                object universalRenderer = Enum.Parse(rendererType, "UniversalRenderer");
                object created = createRendererAsset.Invoke(null, new[] { path, universalRenderer, false, "Renderer" });
                ScriptableRendererData data = created as ScriptableRendererData;
                if (data != null)
                {
                    return data;
                }
            }

            UniversalRendererData fallback = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(fallback, path);
            return fallback;
        }

        private static void EnsureRendererData(UniversalRenderPipelineAsset pipelineAsset, ScriptableRendererData rendererData)
        {
            if (pipelineAsset == null || rendererData == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(pipelineAsset);
            SerializedProperty list = serialized.FindProperty("m_RendererDataList");
            if (list != null)
            {
                list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            }

            SerializedProperty defaultIndex = serialized.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null)
            {
                defaultIndex.intValue = 0;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePipelineAsset(UniversalRenderPipelineAsset pipelineAsset)
        {
            SetProperty(pipelineAsset, "supportsCameraDepthTexture", true);
            SetProperty(pipelineAsset, "supportsCameraOpaqueTexture", false);
            SetProperty(pipelineAsset, "supportsHDR", false);
            SetProperty(pipelineAsset, "msaaSampleCount", 1);
            SetProperty(pipelineAsset, "renderScale", 1f);
            SetProperty(pipelineAsset, "shadowDistance", 70f);
            SetProperty(pipelineAsset, "supportsMainLightShadows", true);
            SetProperty(pipelineAsset, "supportsAdditionalLightShadows", false);
            SetProperty(pipelineAsset, "maxAdditionalLightsCount", 1);
        }

        private static void AssignRenderPipeline(RenderPipelineAsset pipelineAsset)
        {
            SetStaticProperty(typeof(GraphicsSettings), "defaultRenderPipeline", pipelineAsset);
            SetStaticProperty(typeof(GraphicsSettings), "renderPipelineAsset", pipelineAsset);

            PropertyInfo qualityPipeline = typeof(QualitySettings).GetProperty("renderPipeline", BindingFlags.Static | BindingFlags.Public);
            if (qualityPipeline == null || !qualityPipeline.CanWrite)
            {
                return;
            }

            int currentQuality = QualitySettings.GetQualityLevel();
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                qualityPipeline.SetValue(null, pipelineAsset);
            }

            QualitySettings.SetQualityLevel(currentQuality, false);
        }

        private static void ConfigureQualityDefaults()
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = 70f;
            QualitySettings.lodBias = 1f;
            SetStaticProperty(typeof(QualitySettings), "realtimeReflectionProbes", false);
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
            }
        }

        private static void SetStaticProperty(Type type, string propertyName, object value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                property.SetValue(null, value);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
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
