using System;

namespace BattleZoneMobile
{
    [Serializable]
    public class WeaponAttachmentProfile
    {
        public string opticName = "Iron Sights";
        public string muzzleName = "Standard Muzzle";
        public string magazineName = "Standard Mag";
        public string gripName = "No Grip";
        public float spreadMultiplier = 1f;
        public float recoilMultiplier = 1f;
        public float reloadMultiplier = 1f;
        public float magazineMultiplier = 1f;
        public float fireRateMultiplier = 1f;

        public float SpreadMultiplier => spreadMultiplier;
        public float RecoilMultiplier => recoilMultiplier;
        public float ReloadMultiplier => reloadMultiplier;
        public float MagazineMultiplier => magazineMultiplier;
        public float FireRateMultiplier => fireRateMultiplier;

        public static WeaponAttachmentProfile CreateDefault(WeaponDefinition definition)
        {
            WeaponAttachmentProfile profile = new WeaponAttachmentProfile();
            if (definition == null)
            {
                return profile;
            }

            profile.opticName = definition.supportsOptic ? OpticFor(definition.slot) : "Fixed Sights";
            profile.muzzleName = definition.supportsMuzzle ? MuzzleFor(definition.slot) : "Integral Barrel";
            profile.magazineName = definition.supportsMagazine ? MagazineFor(definition.slot) : "Fixed Mag";
            profile.gripName = definition.supportsGrip ? GripFor(definition.slot) : "No Grip";

            switch (definition.rarity)
            {
                case WeaponRarity.Epic:
                    profile.spreadMultiplier = 0.78f;
                    profile.recoilMultiplier = 0.78f;
                    profile.reloadMultiplier = 0.88f;
                    profile.magazineMultiplier = definition.supportsMagazine ? 1.2f : 1f;
                    break;
                case WeaponRarity.Rare:
                    profile.spreadMultiplier = 0.86f;
                    profile.recoilMultiplier = 0.86f;
                    profile.reloadMultiplier = 0.93f;
                    profile.magazineMultiplier = definition.supportsMagazine ? 1.12f : 1f;
                    break;
                case WeaponRarity.Uncommon:
                    profile.spreadMultiplier = 0.94f;
                    profile.recoilMultiplier = 0.94f;
                    profile.reloadMultiplier = 0.97f;
                    profile.magazineMultiplier = definition.supportsMagazine ? 1.06f : 1f;
                    break;
                default:
                    profile.spreadMultiplier = 1f;
                    profile.recoilMultiplier = 1f;
                    profile.reloadMultiplier = 1f;
                    profile.magazineMultiplier = 1f;
                    break;
            }

            if (definition.slot == WeaponSlot.SMG)
            {
                profile.fireRateMultiplier = 1.03f;
            }
            else if (definition.slot == WeaponSlot.Sniper)
            {
                profile.spreadMultiplier *= 0.82f;
                profile.recoilMultiplier *= 1.08f;
            }
            else if (definition.slot == WeaponSlot.Shotgun)
            {
                profile.reloadMultiplier *= 0.94f;
            }

            return profile;
        }

        public string BuildShortLabel()
        {
            string optic = string.IsNullOrEmpty(opticName) ? "Sights" : opticName;
            string muzzle = string.IsNullOrEmpty(muzzleName) ? "Muzzle" : muzzleName;
            string magazine = string.IsNullOrEmpty(magazineName) ? "Mag" : magazineName;
            string grip = string.IsNullOrEmpty(gripName) ? "Grip" : gripName;
            return $"{optic} / {muzzle} / {magazine} / {grip}";
        }

        private static string OpticFor(WeaponSlot slot)
        {
            switch (slot)
            {
                case WeaponSlot.Sniper:
                    return "Scout Scope";
                case WeaponSlot.Shotgun:
                    return "Reflex Bead";
                case WeaponSlot.Pistol:
                    return "Micro Reflex";
                default:
                    return "Clean Dot";
            }
        }

        private static string MuzzleFor(WeaponSlot slot)
        {
            switch (slot)
            {
                case WeaponSlot.Sniper:
                    return "Long Brake";
                case WeaponSlot.Shotgun:
                    return "Ported Choke";
                case WeaponSlot.SMG:
                    return "Flash Cone";
                default:
                    return "Compensator";
            }
        }

        private static string MagazineFor(WeaponSlot slot)
        {
            switch (slot)
            {
                case WeaponSlot.Shotgun:
                    return "Side Saddle";
                case WeaponSlot.Sniper:
                    return "Precision Box";
                case WeaponSlot.Pistol:
                    return "Compact Mag";
                default:
                    return "Extended Mag";
            }
        }

        private static string GripFor(WeaponSlot slot)
        {
            switch (slot)
            {
                case WeaponSlot.SMG:
                    return "Stub Grip";
                case WeaponSlot.AssaultRifle:
                    return "Angled Grip";
                default:
                    return "No Grip";
            }
        }
    }
}
