using System;
using UnityEngine;
using UnityEngine.Events;

namespace BattleZoneMobile
{
    [Serializable]
    public class HitConfirmedEvent : UnityEvent<Vector3, float>
    {
    }
}
