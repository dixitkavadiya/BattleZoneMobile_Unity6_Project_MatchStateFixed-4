namespace BattleZoneMobile
{
    public enum CombatWeaponType
    {
        AssaultRifle,
        SMG,
        Sniper,
        DMR,
        Shotgun,
        Pistol,
        Melee,
        Throwable
    }

    public enum CombatWeaponDelivery
    {
        Raycast,
        Projectile,
        Melee,
        Throwable
    }

    public enum CombatAmmoClass
    {
        None,
        Light,
        Medium,
        Heavy,
        Shell,
        Throwable
    }

    public enum CombatHitZone
    {
        Head,
        Neck,
        Chest,
        Arm,
        Leg
    }

    public enum CombatSurfaceType
    {
        Default,
        Metal,
        Stone,
        Wood,
        Glass,
        Sand,
        Water,
        Grass
    }

    public enum CombatDamageSourceType
    {
        Bullet,
        Projectile,
        Explosion,
        Melee,
        Throwable,
        Fall
    }
}
