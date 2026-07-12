using System;
using UnityEngine;

namespace BattleZoneMobile
{
    public static class CombatWeaponRoster
    {
        private const string ResourcePath = "WeaponData";

        public static AdvancedWeaponData[] LoadAll()
        {
            AdvancedWeaponData[] weapons = Resources.LoadAll<AdvancedWeaponData>(ResourcePath);
            Array.Sort(weapons, CompareWeapons);
            return weapons;
        }

        public static AdvancedWeaponData FindById(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            AdvancedWeaponData[] weapons = LoadAll();
            for (int i = 0; i < weapons.Length; i++)
            {
                AdvancedWeaponData weapon = weapons[i];
                if (weapon != null && string.Equals(weapon.WeaponId, weaponId, StringComparison.OrdinalIgnoreCase))
                {
                    return weapon;
                }
            }

            return null;
        }

        public static CombatLoadoutSlot ResolveLoadoutSlot(AdvancedWeaponData data, bool preferSecondaryWhenPrimaryOccupied)
        {
            if (data == null)
            {
                return CombatLoadoutSlot.Primary;
            }

            if (data.EquipSlot == CombatWeaponEquipSlot.Pistol || data.WeaponType == CombatWeaponType.Pistol)
            {
                return CombatLoadoutSlot.Pistol;
            }

            return preferSecondaryWhenPrimaryOccupied ? CombatLoadoutSlot.Secondary : CombatLoadoutSlot.Primary;
        }

        private static int CompareWeapons(AdvancedWeaponData left, AdvancedWeaponData right)
        {
            string leftName = left != null ? left.DisplayName : string.Empty;
            string rightName = right != null ? right.DisplayName : string.Empty;
            return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
