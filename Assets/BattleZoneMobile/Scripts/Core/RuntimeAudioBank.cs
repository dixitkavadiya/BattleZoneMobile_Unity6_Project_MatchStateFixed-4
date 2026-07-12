using UnityEngine;

namespace BattleZoneMobile
{
    public class RuntimeAudioBank : MonoBehaviour
    {
        public static RuntimeAudioBank Instance { get; private set; }

        [SerializeField] private float masterVolume = 0.85f;

        private AudioClip pistolShot;
        private AudioClip rifleShot;
        private AudioClip smgShot;
        private AudioClip sniperShot;
        private AudioClip shotgunShot;
        private AudioClip meleeSwing;
        private AudioClip reload;
        private AudioClip footstepGrass;
        private AudioClip footstepRoad;
        private AudioClip footstepBuilding;
        private AudioClip footstepWater;
        private AudioClip hit;
        private AudioClip death;
        private AudioClip pickup;
        private AudioClip switchWeapon;
        private AudioClip uiClick;
        private AudioClip zoneWarning;

        private void Awake()
        {
            Instance = this;
            AudioListener.volume = masterVolume;
            pistolShot = CreateToneClip("BZ_PistolShot", 0.11f, 170f, 0.72f, 0.18f);
            rifleShot = CreateToneClip("BZ_RifleShot", 0.075f, 220f, 0.64f, 0.25f);
            smgShot = CreateToneClip("BZ_SMGShot", 0.055f, 260f, 0.48f, 0.32f);
            sniperShot = CreateToneClip("BZ_SniperShot", 0.22f, 95f, 0.86f, 0.24f);
            shotgunShot = CreateToneClip("BZ_ShotgunShot", 0.18f, 115f, 0.78f, 0.36f);
            meleeSwing = CreateToneClip("BZ_MeleeSwing", 0.16f, 90f, 0.34f, 0.05f);
            reload = CreateToneClip("BZ_Reload", 0.28f, 120f, 0.28f, 0.08f);
            footstepGrass = CreateToneClip("BZ_Footstep_Grass", 0.09f, 58f, 0.18f, 0.18f);
            footstepRoad = CreateToneClip("BZ_Footstep_Road", 0.075f, 82f, 0.20f, 0.08f);
            footstepBuilding = CreateToneClip("BZ_Footstep_Building", 0.085f, 112f, 0.17f, 0.05f);
            footstepWater = CreateToneClip("BZ_Footstep_Water", 0.12f, 48f, 0.16f, 0.42f);
            hit = CreateToneClip("BZ_Hit", 0.12f, 330f, 0.34f, 0.12f);
            death = CreateToneClip("BZ_Death", 0.34f, 70f, 0.36f, 0.18f);
            pickup = CreateToneClip("BZ_Pickup", 0.18f, 410f, 0.22f, 0.04f);
            switchWeapon = CreateToneClip("BZ_SwitchWeapon", 0.16f, 150f, 0.24f, 0.03f);
            uiClick = CreateToneClip("BZ_UI_Click", 0.065f, 520f, 0.18f, 0.02f);
            zoneWarning = CreateToneClip("BZ_Zone_Warning", 0.34f, 240f, 0.24f, 0.16f);
        }

        public void PlayWeaponFire(WeaponSlot slot, Vector3 position)
        {
            AudioClip clip = pistolShot;
            switch (slot)
            {
                case WeaponSlot.AssaultRifle:
                    clip = rifleShot;
                    break;
                case WeaponSlot.SMG:
                    clip = smgShot;
                    break;
                case WeaponSlot.Sniper:
                    clip = sniperShot;
                    break;
                case WeaponSlot.Shotgun:
                    clip = shotgunShot;
                    break;
                case WeaponSlot.Pistol:
                    clip = pistolShot;
                    break;
            }

            PlayOneShot(clip, position, 0.65f);
        }

        public void PlayReload(Vector3 position)
        {
            PlayOneShot(reload, position, 0.46f);
        }

        public void PlayFootstep(Vector3 position)
        {
            PlayFootstep(position, "Grass");
        }

        public void PlayFootstep(Vector3 position, string surface)
        {
            AudioClip clip = footstepGrass;
            float volume = 0.28f;
            if (!string.IsNullOrEmpty(surface))
            {
                if (surface == "Road")
                {
                    clip = footstepRoad;
                    volume = 0.30f;
                }
                else if (surface == "Building")
                {
                    clip = footstepBuilding;
                    volume = 0.26f;
                }
                else if (surface == "Water")
                {
                    clip = footstepWater;
                    volume = 0.32f;
                }
            }

            PlayOneShot(clip, position, volume);
        }

        public void PlayHit(Vector3 position)
        {
            PlayOneShot(hit, position, 0.44f);
        }

        public void PlayDeath(Vector3 position)
        {
            PlayOneShot(death, position, 0.52f);
        }

        public void PlayPickup(Vector3 position)
        {
            PlayOneShot(pickup, position, 0.38f);
        }

        public void PlaySwitch(Vector3 position)
        {
            PlayOneShot(switchWeapon, position, 0.34f);
        }

        public void PlayUiClick()
        {
            PlayOneShot(uiClick, Vector3.zero, 0.24f);
        }

        public void PlayZoneWarning(Vector3 position)
        {
            PlayOneShot(zoneWarning, position, 0.42f);
        }

        public void SetMasterVolume(float normalizedVolume)
        {
            masterVolume = Mathf.Clamp01(normalizedVolume);
            AudioListener.volume = masterVolume;
        }

        private static void PlayOneShot(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
            {
                return;
            }

            float globalVolume = Instance != null ? Instance.masterVolume : AudioListener.volume;
            AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume * globalVolume));
        }

        private static AudioClip CreateToneClip(string clipName, float duration, float frequency, float volume, float noise)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
            float[] samples = new float[sampleCount];

            uint state = 2166136261u;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - Mathf.Clamp01(t / duration);
                state ^= (uint)(i + 1);
                state *= 16777619u;
                float random = ((state & 1023u) / 511.5f) - 1f;
                float tone = Mathf.Sin(t * frequency * Mathf.PI * 2f);
                samples[i] = (tone * (1f - noise) + random * noise) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
