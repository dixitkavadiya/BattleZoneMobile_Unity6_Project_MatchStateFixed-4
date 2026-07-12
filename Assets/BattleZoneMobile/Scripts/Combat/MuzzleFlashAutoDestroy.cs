using UnityEngine;

namespace BattleZoneMobile
{
    public class MuzzleFlashAutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.12f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
