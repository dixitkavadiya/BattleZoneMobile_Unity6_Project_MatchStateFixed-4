using UnityEngine;

namespace BattleZoneMobile
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject source);
    }
}
