namespace BattleZoneMobile
{
    public class AssaultRifleWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.AssaultRifle;
    }

    public class SMGWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.SMG;
    }

    public class SniperWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.Sniper;
    }

    public class DMRWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.DMR;
    }

    public class ShotgunWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.Shotgun;
    }

    public class PistolWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.Pistol;
    }

    public class MeleeWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.Melee;
    }

    public class ThrowableWeapon : ModularWeaponBase
    {
        public override CombatWeaponType WeaponType => CombatWeaponType.Throwable;
    }
}
