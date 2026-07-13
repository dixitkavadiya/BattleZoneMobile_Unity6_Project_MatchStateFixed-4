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
        public string stockName = "No Stock";
        public string laserName = "No Laser";
        public WeaponAttachmentData optic;
        public WeaponAttachmentData muzzle;
        public WeaponAttachmentData magazine;
        public WeaponAttachmentData grip;
        public WeaponAttachmentData stock;
        public WeaponAttachmentData laser;
        public float spreadMultiplier = 1f;
        public float recoilMultiplier = 1f;
        public float reloadMultiplier = 1f;
        public float magazineMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public bool suppressesFireAudio;

        public float SpreadMultiplier => spreadMultiplier;
        public float RecoilMultiplier => recoilMultiplier;
        public float ReloadMultiplier => reloadMultiplier;
        public float MagazineMultiplier => magazineMultiplier;
        public float FireRateMultiplier => fireRateMultiplier;
        public bool SuppressesFireAudio => suppressesFireAudio;

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
            profile.stockName = definition.supportsStock ? "Open Stock" : "Fixed Stock";
            profile.laserName = definition.supportsLaser ? "No Laser" : "No Rail";

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

        public bool TryAttach(WeaponDefinition definition, WeaponAttachmentData attachment, out WeaponAttachmentData replaced, out string message)
        {
            replaced = null;
            if (definition == null)
            {
                message = "No weapon selected";
                return false;
            }

            if (attachment == null)
            {
                message = "No attachment selected";
                return false;
            }

            if (!attachment.IsCompatible(definition))
            {
                message = $"{attachment.DisplayName} does not fit {definition.displayName}";
                return false;
            }

            switch (attachment.Slot)
            {
                case WeaponAttachmentSlot.Optic:
                    replaced = optic;
                    optic = attachment;
                    opticName = attachment.DisplayName;
                    break;
                case WeaponAttachmentSlot.Muzzle:
                    replaced = muzzle;
                    muzzle = attachment;
                    muzzleName = attachment.DisplayName;
                    break;
                case WeaponAttachmentSlot.Magazine:
                    replaced = magazine;
                    magazine = attachment;
                    magazineName = attachment.DisplayName;
                    break;
                case WeaponAttachmentSlot.Grip:
                    replaced = grip;
                    grip = attachment;
                    gripName = attachment.DisplayName;
                    break;
                case WeaponAttachmentSlot.Stock:
                    replaced = stock;
                    stock = attachment;
                    stockName = attachment.DisplayName;
                    break;
                case WeaponAttachmentSlot.Laser:
                    replaced = laser;
                    laser = attachment;
                    laserName = attachment.DisplayName;
                    break;
                default:
                    message = "Unsupported attachment slot";
                    return false;
            }

            RecalculateModifiers(definition);
            message = replaced != null
                ? $"Replaced {replaced.DisplayName} with {attachment.DisplayName}"
                : $"Attached {attachment.DisplayName}";
            return true;
        }

        public WeaponAttachmentData GetAttachment(WeaponAttachmentSlot slot)
        {
            switch (slot)
            {
                case WeaponAttachmentSlot.Optic:
                    return optic;
                case WeaponAttachmentSlot.Muzzle:
                    return muzzle;
                case WeaponAttachmentSlot.Magazine:
                    return magazine;
                case WeaponAttachmentSlot.Grip:
                    return grip;
                case WeaponAttachmentSlot.Stock:
                    return stock;
                case WeaponAttachmentSlot.Laser:
                    return laser;
                default:
                    return null;
            }
        }

        public bool TryDetach(WeaponDefinition definition, WeaponAttachmentSlot slot, out WeaponAttachmentData detached, out string message)
        {
            detached = GetAttachment(slot);
            if (detached == null)
            {
                message = $"No {slot} attachment equipped";
                return false;
            }

            switch (slot)
            {
                case WeaponAttachmentSlot.Optic:
                    optic = null;
                    opticName = definition != null && definition.supportsOptic ? OpticFor(definition.slot) : "Fixed Sights";
                    break;
                case WeaponAttachmentSlot.Muzzle:
                    muzzle = null;
                    muzzleName = definition != null && definition.supportsMuzzle ? MuzzleFor(definition.slot) : "Integral Barrel";
                    break;
                case WeaponAttachmentSlot.Magazine:
                    magazine = null;
                    magazineName = definition != null && definition.supportsMagazine ? MagazineFor(definition.slot) : "Fixed Mag";
                    break;
                case WeaponAttachmentSlot.Grip:
                    grip = null;
                    gripName = definition != null && definition.supportsGrip ? GripFor(definition.slot) : "No Grip";
                    break;
                case WeaponAttachmentSlot.Stock:
                    stock = null;
                    stockName = definition != null && definition.supportsStock ? "Open Stock" : "Fixed Stock";
                    break;
                case WeaponAttachmentSlot.Laser:
                    laser = null;
                    laserName = definition != null && definition.supportsLaser ? "No Laser" : "No Rail";
                    break;
            }

            RecalculateModifiers(definition);
            message = $"Detached {detached.DisplayName}";
            return true;
        }

        public string BuildDetailedLabel()
        {
            return $"Optic: {SafeName(opticName)}\n" +
                   $"Muzzle: {SafeName(muzzleName)}\n" +
                   $"Magazine: {SafeName(magazineName)}\n" +
                   $"Grip: {SafeName(gripName)}\n" +
                   $"Stock: {SafeName(stockName)}\n" +
                   $"Laser: {SafeName(laserName)}";
        }

        public string BuildShortLabel()
        {
            string optic = string.IsNullOrEmpty(opticName) ? "Sights" : opticName;
            string muzzle = string.IsNullOrEmpty(muzzleName) ? "Muzzle" : muzzleName;
            string magazine = string.IsNullOrEmpty(magazineName) ? "Mag" : magazineName;
            string grip = string.IsNullOrEmpty(gripName) ? "Grip" : gripName;
            return $"{optic} / {muzzle} / {magazine} / {grip}";
        }

        private void RecalculateModifiers(WeaponDefinition definition)
        {
            spreadMultiplier = 1f;
            recoilMultiplier = 1f;
            reloadMultiplier = 1f;
            magazineMultiplier = 1f;
            fireRateMultiplier = 1f;
            suppressesFireAudio = false;
            ApplyDefinitionBaseline(definition);

            ApplyAttachment(optic);
            ApplyAttachment(muzzle);
            ApplyAttachment(magazine);
            ApplyAttachment(grip);
            ApplyAttachment(stock);
            ApplyAttachment(laser);
        }

        private void ApplyDefinitionBaseline(WeaponDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            switch (definition.rarity)
            {
                case WeaponRarity.Epic:
                    spreadMultiplier *= 0.78f;
                    recoilMultiplier *= 0.78f;
                    reloadMultiplier *= 0.88f;
                    magazineMultiplier *= definition.supportsMagazine ? 1.2f : 1f;
                    break;
                case WeaponRarity.Rare:
                    spreadMultiplier *= 0.86f;
                    recoilMultiplier *= 0.86f;
                    reloadMultiplier *= 0.93f;
                    magazineMultiplier *= definition.supportsMagazine ? 1.12f : 1f;
                    break;
                case WeaponRarity.Uncommon:
                    spreadMultiplier *= 0.94f;
                    recoilMultiplier *= 0.94f;
                    reloadMultiplier *= 0.97f;
                    magazineMultiplier *= definition.supportsMagazine ? 1.06f : 1f;
                    break;
            }

            if (definition.slot == WeaponSlot.SMG)
            {
                fireRateMultiplier *= 1.03f;
            }
            else if (definition.slot == WeaponSlot.Sniper)
            {
                spreadMultiplier *= 0.82f;
                recoilMultiplier *= 1.08f;
            }
            else if (definition.slot == WeaponSlot.Shotgun)
            {
                reloadMultiplier *= 0.94f;
            }
        }

        private void ApplyAttachment(WeaponAttachmentData attachment)
        {
            if (attachment == null)
            {
                return;
            }

            spreadMultiplier *= attachment.SpreadMultiplier;
            recoilMultiplier *= attachment.RecoilMultiplier;
            reloadMultiplier *= attachment.ReloadMultiplier;
            magazineMultiplier *= attachment.MagazineMultiplier;
            fireRateMultiplier *= attachment.FireRateMultiplier;
            suppressesFireAudio |= attachment.SuppressesFireAudio;
        }

        private static string SafeName(string value)
        {
            return string.IsNullOrEmpty(value) ? "None" : value;
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
