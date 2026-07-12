using UnityEngine;

namespace BattleZoneMobile
{
    public static class WeaponVisualPlaceholderFactory
    {
        public static WeaponVisualPlaceholder CreateWorldPickup(AdvancedWeaponData data, Transform parent, Vector3 position, Material material)
        {
            WeaponVisualPlaceholder placeholder = Create(data, parent, material, true);
            placeholder.transform.position = position;
            placeholder.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            placeholder.transform.localScale = data != null && data.Visual != null ? data.Visual.worldScale : Vector3.one;
            return placeholder;
        }

        public static WeaponVisualPlaceholder CreateEquippedView(AdvancedWeaponData data, Transform parent, Material material)
        {
            WeaponVisualPlaceholder placeholder = Create(data, parent, material, false);
            placeholder.transform.localPosition = Vector3.zero;
            placeholder.transform.localRotation = Quaternion.identity;
            placeholder.transform.localScale = data != null && data.Visual != null ? data.Visual.equippedScale : Vector3.one;
            return placeholder;
        }

        private static WeaponVisualPlaceholder Create(AdvancedWeaponData data, Transform parent, Material material, bool worldPickup)
        {
            string displayName = data != null ? data.DisplayName : "Unknown Weapon";
            GameObject root = new GameObject($"{(worldPickup ? "WP" : "EQ")}_{displayName}_TemporaryOriginalPlaceholder");
            root.transform.SetParent(parent, false);

            float length = ResolveLength(data);
            float width = ResolveWidth(data);
            float height = ResolveHeight(data);
            Material safeMaterial = material != null ? material : new Material(Shader.Find("Universal Render Pipeline/Lit"));

            CreateBeveledPart("Receiver", root.transform, new Vector3(0f, 0f, 0f), new Vector3(width, height, length * 0.48f), safeMaterial);
            CreateBeveledPart("Forward Shroud", root.transform, new Vector3(0f, 0.015f, length * 0.34f), new Vector3(width * 0.78f, height * 0.72f, length * 0.34f), safeMaterial);
            CreateCylinderPart("Barrel", root.transform, new Vector3(0f, 0.02f, length * 0.58f), width * 0.17f, length * 0.32f, safeMaterial);
            CreateBeveledPart("Grip", root.transform, new Vector3(0f, -height * 0.72f, -length * 0.10f), new Vector3(width * 0.42f, height * 0.85f, length * 0.13f), safeMaterial);
            CreateBeveledPart("Magazine", root.transform, new Vector3(0f, -height * 0.62f, length * 0.12f), new Vector3(width * 0.54f, height * 0.78f, length * 0.12f), safeMaterial);
            CreateBeveledPart("Top Rail", root.transform, new Vector3(0f, height * 0.58f, length * 0.08f), new Vector3(width * 0.64f, height * 0.16f, length * 0.46f), safeMaterial);

            if (data != null && data.Attachments != null && data.Attachments.supportsStock && data.WeaponType != CombatWeaponType.Pistol)
            {
                CreateBeveledPart("Stock", root.transform, new Vector3(0f, 0f, -length * 0.42f), new Vector3(width * 0.82f, height * 0.72f, length * 0.22f), safeMaterial);
            }

            if (data != null && data.Attachments != null && data.Attachments.supportsOptic)
            {
                CreateBeveledPart("Temporary Optic Housing", root.transform, new Vector3(0f, height * 0.88f, length * 0.05f), new Vector3(width * 0.46f, height * 0.36f, length * 0.12f), safeMaterial);
            }

            Transform muzzle = CreateSocket("Muzzle", root.transform, new Vector3(0f, 0.02f, length * 0.76f));
            Transform shell = CreateSocket("ShellEjection", root.transform, new Vector3(width * 0.62f, height * 0.22f, length * 0.08f));
            Transform leftGrip = CreateSocket("LeftHandGrip", root.transform, new Vector3(-width * 0.62f, -height * 0.10f, length * 0.25f));
            Transform rightGrip = CreateSocket("RightHandGrip", root.transform, new Vector3(width * 0.38f, -height * 0.45f, -length * 0.06f));
            Transform optic = CreateSocket("OpticSocket", root.transform, new Vector3(0f, height * 1.08f, length * 0.04f));
            Transform muzzleSocket = CreateSocket("MuzzleSocket", root.transform, new Vector3(0f, 0.02f, length * 0.70f));
            Transform magazine = CreateSocket("MagazineSocket", root.transform, new Vector3(0f, -height * 0.98f, length * 0.12f));
            Transform grip = CreateSocket("GripSocket", root.transform, new Vector3(0f, -height * 0.34f, length * 0.18f));

            BoxCollider pickupCollider = root.AddComponent<BoxCollider>();
            pickupCollider.size = new Vector3(width * 1.6f, height * 2.1f, length * 1.15f);
            pickupCollider.center = Vector3.zero;
            pickupCollider.isTrigger = worldPickup;

            WeaponVisualPlaceholder placeholder = root.AddComponent<WeaponVisualPlaceholder>();
            string label = data != null && data.Visual != null ? data.Visual.placeholderLabel : "Temporary original placeholder";
            placeholder.Configure(data, label, muzzle, shell, leftGrip, rightGrip, optic, muzzleSocket, magazine, grip, pickupCollider);
            return placeholder;
        }

        private static float ResolveLength(AdvancedWeaponData data)
        {
            if (data == null)
            {
                return 0.8f;
            }

            switch (data.WeaponType)
            {
                case CombatWeaponType.Sniper:
                case CombatWeaponType.DMR:
                    return 1.25f;
                case CombatWeaponType.Shotgun:
                    return 1.05f;
                case CombatWeaponType.SMG:
                    return 0.72f;
                case CombatWeaponType.Pistol:
                    return 0.42f;
                default:
                    return 0.95f;
            }
        }

        private static float ResolveWidth(AdvancedWeaponData data)
        {
            return data != null && data.WeaponType == CombatWeaponType.Pistol ? 0.11f : 0.15f;
        }

        private static float ResolveHeight(AdvancedWeaponData data)
        {
            return data != null && data.WeaponType == CombatWeaponType.Shotgun ? 0.18f : 0.15f;
        }

        private static Transform CreateSocket(string name, Transform parent, Vector3 localPosition)
        {
            GameObject socket = new GameObject(name);
            socket.transform.SetParent(parent, false);
            socket.transform.localPosition = localPosition;
            return socket.transform;
        }

        private static void CreateBeveledPart(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            MeshFilter filter = part.AddComponent<MeshFilter>();
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildBoxMesh(size);
            renderer.sharedMaterial = material;
        }

        private static void CreateCylinderPart(string name, Transform parent, Vector3 localPosition, float radius, float length, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name;
            Object.Destroy(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            part.transform.localScale = new Vector3(radius, length * 0.5f, radius);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Mesh BuildBoxMesh(Vector3 size)
        {
            Vector3 h = size * 0.5f;
            Vector3[] vertices =
            {
                new Vector3(-h.x, -h.y, -h.z), new Vector3(h.x, -h.y, -h.z), new Vector3(h.x, h.y, -h.z), new Vector3(-h.x, h.y, -h.z),
                new Vector3(-h.x, -h.y, h.z), new Vector3(h.x, -h.y, h.z), new Vector3(h.x, h.y, h.z), new Vector3(-h.x, h.y, h.z)
            };

            int[] triangles =
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                2, 3, 7, 2, 7, 6,
                1, 2, 6, 1, 6, 5,
                3, 0, 4, 3, 4, 7
            };

            Mesh mesh = new Mesh
            {
                name = "TemporaryOriginalWeaponPartMesh",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
