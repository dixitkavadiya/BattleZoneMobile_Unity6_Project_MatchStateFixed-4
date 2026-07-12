using UnityEngine;

namespace BattleZoneMobile
{
    public class AndroidPerformanceSetup : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableSleepTimeout = true;
        [SerializeField] private float defaultShadowDistance = 48f;
        [SerializeField] private float defaultLodBias = 0.92f;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            Input.multiTouchEnabled = true;
            QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance <= 0f ? defaultShadowDistance : QualitySettings.shadowDistance, defaultShadowDistance);
            QualitySettings.lodBias = Mathf.Clamp(defaultLodBias, 0.55f, 1.35f);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = true;

            if (disableSleepTimeout)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        private void OnEnable()
        {
            Application.lowMemory += OnLowMemory;
        }

        private void OnDisable()
        {
            Application.lowMemory -= OnLowMemory;
        }

        private static void OnLowMemory()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
