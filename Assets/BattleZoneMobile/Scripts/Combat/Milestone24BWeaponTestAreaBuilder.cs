using UnityEngine;

namespace BattleZoneMobile
{
    public static class Milestone24BWeaponTestAreaBuilder
    {
        public const string TestAreaObjectName = "M24B Live Weapon Test Area";
        public static readonly Vector3 DefaultOrigin = new Vector3(0f, 1.08f, -40f);

        public static GameObject BuildOrRepair(
            AdvancedWeaponData[] weaponRoster,
            Vector3 origin,
            int lootLayer,
            Material weaponMaterial,
            Material ammoMaterial,
            Material targetMaterial,
            Material surfaceMaterial,
            bool rebuildExisting)
        {
            GameObject root = GameObject.Find(TestAreaObjectName);
            if (root != null && !rebuildExisting)
            {
                return root;
            }

            if (root == null)
            {
                root = new GameObject(TestAreaObjectName);
            }
            else
            {
                ClearChildren(root.transform);
            }

            root.transform.position = origin;
            Material safeWeaponMaterial = weaponMaterial != null ? weaponMaterial : CreateMaterial("M24B Weapon Placeholder Material", new Color(0.22f, 0.25f, 0.28f, 1f));
            Material safeAmmoMaterial = ammoMaterial != null ? ammoMaterial : CreateMaterial("M24B Ammo Material", new Color(0.9f, 0.72f, 0.28f, 1f));
            Material safeTargetMaterial = targetMaterial != null ? targetMaterial : CreateMaterial("M24B Target Material", new Color(0.48f, 0.56f, 0.62f, 1f));
            Material safeSurfaceMaterial = surfaceMaterial != null ? surfaceMaterial : CreateMaterial("M24B Surface Material", new Color(0.36f, 0.38f, 0.36f, 1f));

            CreatePad(root.transform, safeSurfaceMaterial);
            BuildWeaponRows(root.transform, weaponRoster, lootLayer, safeWeaponMaterial, safeAmmoMaterial);
            BuildTargets(root.transform, safeTargetMaterial);
            BuildSurfacePanels(root.transform, safeSurfaceMaterial);
            return root;
        }

        private static void BuildWeaponRows(Transform root, AdvancedWeaponData[] weaponRoster, int lootLayer, Material weaponMaterial, Material ammoMaterial)
        {
            AdvancedWeaponData[] orderedWeapons = BuildOrderedWeaponList(weaponRoster);
            for (int i = 0; i < orderedWeapons.Length; i++)
            {
                AdvancedWeaponData weapon = orderedWeapons[i];
                if (weapon == null)
                {
                    continue;
                }

                int row = i / 5;
                int column = i % 5;
                Vector3 localPosition = new Vector3(-9.4f + column * 4.7f, 0.38f, -3.4f + row * 3.2f);
                WeaponVisualPlaceholder visual = WeaponVisualPlaceholderFactory.CreateWorldPickup(weapon, root, root.position + localPosition, weaponMaterial);
                visual.name = $"M24B Pickup {weapon.DisplayName}";
                visual.transform.rotation = Quaternion.Euler(0f, -18f, 0f);
                visual.transform.localScale = Vector3.one * 1.18f;

                TextMesh label = CreateLabel(visual.transform, weapon.DisplayName);
                AdvancedWeaponPickup pickup = visual.GetComponent<AdvancedWeaponPickup>();
                if (pickup == null)
                {
                    pickup = visual.gameObject.AddComponent<AdvancedWeaponPickup>();
                }

                pickup.Configure(weapon, ResolveReserveAmmoBonus(weapon), label);
                SetLayerRecursive(visual.gameObject, lootLayer);

                CreateAmmoPickup(root, weapon, lootLayer, ammoMaterial, localPosition + new Vector3(1.42f, -0.03f, 0.72f));
            }
        }

        private static int ResolveReserveAmmoBonus(AdvancedWeaponData weapon)
        {
            if (weapon == null || !weapon.UsesAmmo)
            {
                return 0;
            }

            return Mathf.Max(weapon.MagazineSize, weapon.StartingReserveAmmo);
        }

        private static AdvancedWeaponData[] BuildOrderedWeaponList(AdvancedWeaponData[] weaponRoster)
        {
            string[] order =
            {
                "vxr_56",
                "ark_74",
                "sentinel_ar",
                "pulse_9",
                "raptor_45",
                "longshot_dmr",
                "falcon_sr",
                "breacher_12",
                "sidearm_p9",
                "hammer_50"
            };

            AdvancedWeaponData[] ordered = new AdvancedWeaponData[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                ordered[i] = FindWeapon(weaponRoster, order[i]);
            }

            return ordered;
        }

        private static AdvancedWeaponData FindWeapon(AdvancedWeaponData[] weaponRoster, string id)
        {
            if (weaponRoster == null)
            {
                return null;
            }

            for (int i = 0; i < weaponRoster.Length; i++)
            {
                AdvancedWeaponData weapon = weaponRoster[i];
                if (weapon != null && string.Equals(weapon.WeaponId, id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return weapon;
                }
            }

            return null;
        }

        private static void CreateAmmoPickup(Transform root, AdvancedWeaponData weapon, int lootLayer, Material material, Vector3 localPosition)
        {
            if (!TryMapAmmo(weapon, out LootKind lootKind, out int amount, out string label, out int backpackCost))
            {
                return;
            }

            GameObject ammo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ammo.name = $"M24B Ammo {weapon.DisplayName}";
            ammo.transform.SetParent(root, false);
            ammo.transform.localPosition = localPosition;
            ammo.transform.localScale = new Vector3(0.62f, 0.26f, 0.48f);
            Renderer renderer = ammo.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            BoxCollider collider = ammo.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            LootItem loot = ammo.AddComponent<LootItem>();
            loot.Configure(lootKind, amount, label, backpackCost, LootRarity.Common);
            SetLayerRecursive(ammo, lootLayer);
        }

        private static bool TryMapAmmo(AdvancedWeaponData weapon, out LootKind lootKind, out int amount, out string label, out int backpackCost)
        {
            lootKind = LootKind.LightAmmo;
            amount = 30;
            label = "Ammo";
            backpackCost = 1;

            if (weapon == null)
            {
                return false;
            }

            switch (weapon.AmmoClass)
            {
                case CombatAmmoClass.Medium:
                    lootKind = LootKind.MediumAmmo;
                    amount = Mathf.Max(30, weapon.MagazineSize * 2);
                    label = $"{weapon.DisplayName} Medium Ammo";
                    backpackCost = 1;
                    return true;
                case CombatAmmoClass.Heavy:
                    lootKind = LootKind.HeavyAmmo;
                    amount = Mathf.Max(12, weapon.MagazineSize * 2);
                    label = $"{weapon.DisplayName} Heavy Ammo";
                    backpackCost = 2;
                    return true;
                case CombatAmmoClass.Shell:
                    lootKind = LootKind.ShellAmmo;
                    amount = Mathf.Max(12, weapon.MagazineSize * 2);
                    label = $"{weapon.DisplayName} Shells";
                    backpackCost = 2;
                    return true;
                case CombatAmmoClass.Light:
                    lootKind = LootKind.LightAmmo;
                    amount = Mathf.Max(30, weapon.MagazineSize * 2);
                    label = $"{weapon.DisplayName} Light Ammo";
                    backpackCost = 1;
                    return true;
                default:
                    return false;
            }
        }

        private static TextMesh CreateLabel(Transform parent, string text)
        {
            GameObject labelObject = new GameObject("Weapon Name Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.18f;
            label.color = new Color(1f, 0.94f, 0.56f, 1f);
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyFont != null)
            {
                label.font = legacyFont;
                MeshRenderer renderer = label.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = legacyFont.material;
                }
            }

            return label;
        }

        private static void BuildTargets(Transform root, Material targetMaterial)
        {
            CreateTarget(root, new Vector3(-7.1f, 0f, 5.9f), "M24B Test Target A", targetMaterial);
            CreateTarget(root, new Vector3(-2.8f, 0f, 5.9f), "M24B Test Target B", targetMaterial);
            CreateTarget(root, new Vector3(1.5f, 0f, 5.9f), "M24B Test Target C", targetMaterial);
        }

        private static void CreateTarget(Transform root, Vector3 localPosition, string name, Material material)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(root, false);
            target.transform.localPosition = localPosition;
            Health health = target.AddComponent<Health>();
            health.SetMaxHealth(300f, true);
            CreateTargetPart(target.transform, health, "Head", CombatHitZone.Head, PrimitiveType.Sphere, new Vector3(0f, 1.7f, 0f), new Vector3(0.34f, 0.34f, 0.34f), material);
            CreateTargetPart(target.transform, health, "Chest", CombatHitZone.Chest, PrimitiveType.Cube, new Vector3(0f, 1.1f, 0f), new Vector3(0.62f, 0.68f, 0.28f), material);
            CreateTargetPart(target.transform, health, "Left Arm", CombatHitZone.Arm, PrimitiveType.Capsule, new Vector3(-0.46f, 1.1f, 0f), new Vector3(0.16f, 0.54f, 0.16f), material);
            CreateTargetPart(target.transform, health, "Right Arm", CombatHitZone.Arm, PrimitiveType.Capsule, new Vector3(0.46f, 1.1f, 0f), new Vector3(0.16f, 0.54f, 0.16f), material);
            CreateTargetPart(target.transform, health, "Left Leg", CombatHitZone.Leg, PrimitiveType.Capsule, new Vector3(-0.17f, 0.42f, 0f), new Vector3(0.18f, 0.68f, 0.18f), material);
            CreateTargetPart(target.transform, health, "Right Leg", CombatHitZone.Leg, PrimitiveType.Capsule, new Vector3(0.17f, 0.42f, 0f), new Vector3(0.18f, 0.68f, 0.18f), material);
        }

        private static void CreateTargetPart(Transform parent, Health health, string name, CombatHitZone zone, PrimitiveType primitive, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = $"{parent.name} {name}";
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            CombatHitbox hitbox = part.AddComponent<CombatHitbox>();
            hitbox.Configure(zone, health);
        }

        private static void BuildSurfacePanels(Transform root, Material material)
        {
            CreateSurfacePanel(root, new Vector3(6f, 1f, 5.7f), CombatSurfaceType.Metal, "Metal", material);
            CreateSurfacePanel(root, new Vector3(8.4f, 1f, 5.7f), CombatSurfaceType.Wood, "Wood", material);
            CreateSurfacePanel(root, new Vector3(10.8f, 1f, 5.7f), CombatSurfaceType.Stone, "Stone", material);
            CreateSurfacePanel(root, new Vector3(13.2f, 1f, 5.7f), CombatSurfaceType.Glass, "Glass", material);
        }

        private static void CreateSurfacePanel(Transform root, Vector3 localPosition, CombatSurfaceType surfaceType, string label, Material material)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = $"M24B {label} Surface Panel";
            panel.transform.SetParent(root, false);
            panel.transform.localPosition = localPosition;
            panel.transform.localScale = new Vector3(1.45f, 1.2f, 0.16f);
            Renderer renderer = panel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            panel.AddComponent<CombatSurface>().Configure(surfaceType);
        }

        private static void CreatePad(Transform root, Material material)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "M24B Weapon Test Area Ground Pad";
            pad.transform.SetParent(root, false);
            pad.transform.localPosition = new Vector3(0f, -0.12f, 1.1f);
            pad.transform.localScale = new Vector3(25.5f, 0.18f, 14.5f);
            Renderer renderer = pad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            pad.AddComponent<CombatSurface>().Configure(CombatSurfaceType.Stone);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = name,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Object child = root.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Object.Destroy(child);
                }
                else
                {
                    Object.DestroyImmediate(child);
                }
            }
        }

        private static void SetLayerRecursive(GameObject target, int layer)
        {
            if (target == null || layer < 0)
            {
                return;
            }

            target.layer = layer;
            for (int i = 0; i < target.transform.childCount; i++)
            {
                SetLayerRecursive(target.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
