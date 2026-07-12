using UnityEngine;

namespace BattleZoneMobile
{
    public class WeaponVisualPlaceholder : MonoBehaviour
    {
        [SerializeField] private AdvancedWeaponData weaponData;
        [SerializeField] private string placeholderLabel = "Temporary original placeholder";
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform shellEjection;
        [SerializeField] private Transform leftHandGrip;
        [SerializeField] private Transform rightHandGrip;
        [SerializeField] private Transform opticSocket;
        [SerializeField] private Transform muzzleSocket;
        [SerializeField] private Transform magazineSocket;
        [SerializeField] private Transform gripSocket;
        [SerializeField] private Collider pickupCollider;

        public AdvancedWeaponData WeaponData => weaponData;
        public string PlaceholderLabel => placeholderLabel;
        public Transform Muzzle => muzzle;
        public Transform ShellEjection => shellEjection;
        public Transform LeftHandGrip => leftHandGrip;
        public Transform RightHandGrip => rightHandGrip;
        public Transform OpticSocket => opticSocket;
        public Transform MuzzleSocket => muzzleSocket;
        public Transform MagazineSocket => magazineSocket;
        public Transform GripSocket => gripSocket;
        public Collider PickupCollider => pickupCollider;

        public void Configure(
            AdvancedWeaponData data,
            string label,
            Transform muzzleTransform,
            Transform shellTransform,
            Transform leftGrip,
            Transform rightGrip,
            Transform optic,
            Transform muzzleAttachment,
            Transform magazine,
            Transform grip,
            Collider pickup)
        {
            weaponData = data;
            placeholderLabel = string.IsNullOrWhiteSpace(label) ? "Temporary original placeholder" : label;
            muzzle = muzzleTransform;
            shellEjection = shellTransform;
            leftHandGrip = leftGrip;
            rightHandGrip = rightGrip;
            opticSocket = optic;
            muzzleSocket = muzzleAttachment;
            magazineSocket = magazine;
            gripSocket = grip;
            pickupCollider = pickup;
        }
    }
}
