using UnityEngine;

namespace BattleZoneMobile
{
    public class KnockdownReviveState : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private bool knockdownEnabled = true;
        [SerializeField] private float reviveHealth = 35f;
        [SerializeField] private float reviveArmor = 10f;

        public bool IsKnocked { get; private set; }
        public bool KnockdownEnabled => knockdownEnabled;

        public void ConfigureForRuntime(Health runtimeHealth, bool enabledForRuntime)
        {
            health = runtimeHealth;
            knockdownEnabled = enabledForRuntime;
            WireHealth();
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            WireHealth();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.onDied.RemoveListener(OnDied);
            }
        }

        public void Revive()
        {
            if (!IsKnocked || health == null)
            {
                return;
            }

            IsKnocked = false;
            health.ReviveWithValues(reviveHealth, reviveArmor);
        }

        private void WireHealth()
        {
            if (health == null)
            {
                return;
            }

            health.onDied.RemoveListener(OnDied);
            health.onDied.AddListener(OnDied);
        }

        private void OnDied(GameObject source)
        {
            if (!knockdownEnabled)
            {
                return;
            }

            IsKnocked = true;
        }
    }
}
