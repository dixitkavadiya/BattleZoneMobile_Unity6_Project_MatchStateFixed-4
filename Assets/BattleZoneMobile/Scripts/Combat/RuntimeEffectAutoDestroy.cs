using UnityEngine;

namespace BattleZoneMobile
{
    public class RuntimeEffectAutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.2f;

        public void Configure(float seconds)
        {
            lifetime = Mathf.Max(0.05f, seconds);
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
