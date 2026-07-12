using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    public class RuntimeAmbientSoundscape : MonoBehaviour
    {
        public static RuntimeAmbientSoundscape Instance { get; private set; }

        [SerializeField] private float masterVolume = 0.32f;

        private readonly List<AudioSource> sources = new List<AudioSource>();

        private void Awake()
        {
            Instance = this;
        }

        public void Configure(Vector3 riverPosition, Vector3 forestPosition, Vector3 basePosition)
        {
            AddLoop("BZ_Ambient_Wind", transform.position + new Vector3(0f, 18f, 0f), CreateNoiseLoop("BZ_WindLoop", 3.2f, 0.055f, 0.42f), 0.55f, 1f, 180f);
            AddLoop("BZ_Ambient_Town", new Vector3(-8f, 1.4f, -28f), CreateToneNoiseLoop("BZ_TownLoop", 3.4f, 96f, 0.026f, 0.36f), 0.28f, 0.98f, 78f);
            AddLoop("BZ_Ambient_River", riverPosition + Vector3.up * 0.5f, CreateNoiseLoop("BZ_RiverLoop", 2.6f, 0.075f, 0.82f), 0.7f, 0.92f, 82f);
            AddLoop("BZ_Ambient_Forest", forestPosition + Vector3.up * 1.2f, CreateToneNoiseLoop("BZ_ForestLoop", 3.8f, 155f, 0.03f, 0.48f), 0.42f, 1.08f, 76f);
            AddLoop("BZ_Ambient_BaseHum", basePosition + Vector3.up * 1.4f, CreateToneNoiseLoop("BZ_BaseHumLoop", 2.4f, 58f, 0.035f, 0.18f), 0.28f, 0.86f, 68f);
        }

        public void SetMasterVolume(float normalizedVolume)
        {
            masterVolume = Mathf.Clamp01(normalizedVolume);
            foreach (AudioSource source in sources)
            {
                if (source != null)
                {
                    source.volume = masterVolume * 0.35f;
                }
            }
        }

        private void AddLoop(string objectName, Vector3 position, AudioClip clip, float volumeScale, float pitch, float maxDistance)
        {
            if (clip == null)
            {
                return;
            }

            GameObject sourceObject = new GameObject(objectName);
            sourceObject.transform.SetParent(transform, false);
            sourceObject.transform.position = position;

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0.68f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 6f;
            source.maxDistance = maxDistance;
            source.pitch = pitch;
            source.volume = Mathf.Clamp01(masterVolume * volumeScale);
            sources.Add(source);

            if (Application.isPlaying)
            {
                source.Play();
            }
        }

        private static AudioClip CreateNoiseLoop(string clipName, float duration, float volume, float smoothness)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
            float[] samples = new float[sampleCount];
            uint state = 2166136261u;
            float previous = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                state ^= (uint)(i + 17);
                state *= 16777619u;
                float random = ((state & 2047u) / 1023.5f) - 1f;
                previous = Mathf.Lerp(random, previous, smoothness);
                float edgeFade = Mathf.Sin((i / (float)sampleCount) * Mathf.PI);
                samples[i] = previous * volume * Mathf.Clamp01(edgeFade * 1.8f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateToneNoiseLoop(string clipName, float duration, float frequency, float volume, float noise)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
            float[] samples = new float[sampleCount];
            uint state = 2166136261u;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                state ^= (uint)(i + 31);
                state *= 16777619u;
                float random = ((state & 1023u) / 511.5f) - 1f;
                float tone = Mathf.Sin(t * frequency * Mathf.PI * 2f);
                samples[i] = (tone * (1f - noise) + random * noise) * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
