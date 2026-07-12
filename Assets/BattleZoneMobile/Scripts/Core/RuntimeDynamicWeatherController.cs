using UnityEngine;

namespace BattleZoneMobile
{
    public class RuntimeDynamicWeatherController : MonoBehaviour
    {
        [SerializeField] private Light sunLight;
        [SerializeField] private ParticleSystem rainSystem;
        [SerializeField] private Transform cloudRoot;
        [SerializeField] private float dayDurationSeconds = 220f;
        [SerializeField] private float rainCycleSeconds = 92f;

        private Color dayFog = new Color(0.58f, 0.68f, 0.70f);
        private Color nightFog = new Color(0.08f, 0.11f, 0.16f);
        private Color rainFog = new Color(0.33f, 0.42f, 0.46f);
        private float weatherUpdateTimer;

        public void Configure(Light targetSun, ParticleSystem targetRain, Transform targetCloudRoot, float targetDayDurationSeconds)
        {
            sunLight = targetSun;
            rainSystem = targetRain;
            cloudRoot = targetCloudRoot;
            dayDurationSeconds = Mathf.Max(45f, targetDayDurationSeconds);
            ApplyWeatherState(0f, true);
        }

        private void Update()
        {
            float time = Time.time;
            float dayT = Mathf.Repeat(time / dayDurationSeconds, 1f);
            float rainT = Mathf.Repeat(time / rainCycleSeconds, 1f);
            float rainStrength = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, 0.55f, rainT)) *
                                 (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.80f, 0.96f, rainT)));

            if (sunLight != null)
            {
                float sunPitch = Mathf.Lerp(-16f, 192f, dayT);
                sunLight.transform.rotation = Quaternion.Euler(sunPitch, -36f, 0f);
                float daylight = Mathf.Clamp01(Mathf.Sin(dayT * Mathf.PI));
                sunLight.intensity = Mathf.Lerp(0.18f, 1.22f, daylight) * Mathf.Lerp(1f, 0.72f, rainStrength);
                sunLight.color = Color.Lerp(new Color(0.62f, 0.72f, 1f), new Color(1f, 0.91f, 0.75f), daylight);
            }

            if (cloudRoot != null)
            {
                cloudRoot.Rotate(0f, 0.18f * Time.deltaTime, 0f, Space.World);
                cloudRoot.position = new Vector3(Mathf.Sin(time * 0.018f) * 5f, cloudRoot.position.y, Mathf.Cos(time * 0.014f) * 4f);
            }

            weatherUpdateTimer -= Time.deltaTime;
            if (weatherUpdateTimer <= 0f)
            {
                weatherUpdateTimer = 0.2f;
                ApplyWeatherState(rainStrength, false);
            }
        }

        private void ApplyWeatherState(float rainStrength, bool force)
        {
            float dayT = Mathf.Repeat(Time.time / dayDurationSeconds, 1f);
            float daylight = Mathf.Clamp01(Mathf.Sin(dayT * Mathf.PI));
            Color clearFog = Color.Lerp(nightFog, dayFog, daylight);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.Lerp(clearFog, rainFog, rainStrength);
            RenderSettings.fogDensity = Mathf.Lerp(Mathf.Lerp(0.009f, 0.0034f, daylight), 0.0115f, rainStrength);
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.09f, 0.12f, 0.16f), new Color(0.58f, 0.67f, 0.73f), daylight);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(0.06f, 0.08f, 0.10f), new Color(0.40f, 0.47f, 0.45f), daylight);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(0.035f, 0.045f, 0.04f), new Color(0.20f, 0.25f, 0.18f), daylight);

            if (rainSystem == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = rainSystem.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 145f, rainStrength);
            if ((force || rainStrength > 0.03f) && !rainSystem.isPlaying)
            {
                rainSystem.Play();
            }
        }
    }
}
