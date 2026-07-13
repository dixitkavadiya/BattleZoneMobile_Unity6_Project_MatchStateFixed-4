using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class LootPickupInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private float pickupDistance = 4f;
        [SerializeField] private float scanRadius = 1.6f;
        [SerializeField] private LayerMask lootMask = ~0;
        [SerializeField] private Text promptText;
        [SerializeField] private Button pickupButton;

        private Component focusedPickup;
        private Button promptClickButton;

        public void ConfigureForRuntime(Camera camera, PlayerInventory playerInventory, Text pickupPrompt, LayerMask mask)
        {
            ConfigureForRuntime(camera, playerInventory, pickupPrompt, null, mask);
        }

        public void ConfigureForRuntime(Camera camera, PlayerInventory playerInventory, Text pickupPrompt, Button mobilePickupButton, LayerMask mask)
        {
            playerCamera = camera;
            inventory = playerInventory;
            promptText = pickupPrompt;
            pickupButton = mobilePickupButton;
            lootMask = mask;
            ConfigurePromptClickTarget();
            ConfigurePickupButton();
        }

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (inventory == null)
            {
                inventory = GetComponent<PlayerInventory>();
            }

            ConfigurePromptClickTarget();
            ConfigurePickupButton();
        }

        private void Update()
        {
            focusedPickup = FindClosestPickup();
            UpdatePrompt();

            if (WasKeyboardPickupPressed())
            {
                PickUpFocused();
                return;
            }

            if (WasPickupTapped(out Vector2 screenPosition))
            {
                TryPickUp(screenPosition);
            }
        }

        public void PickUpFocused()
        {
            PickUpComponent(focusedPickup);
        }

        private bool WasKeyboardPickupPressed()
        {
            return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F);
        }

        private bool WasPickupTapped(out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began)
                {
                    return false;
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return false;
                }

                screenPosition = touch.position;
                return true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }

                screenPosition = Input.mousePosition;
                return true;
            }

            return false;
        }

        private void TryPickUp(Vector2 screenPosition)
        {
            if (playerCamera != null)
            {
                Ray ray = playerCamera.ScreenPointToRay(screenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, lootMask, QueryTriggerInteraction.Collide))
                {
                    Component pickup = ResolvePickup(hit.collider);
                    if (pickup != null && Vector3.Distance(transform.position, pickup.transform.position) <= pickupDistance)
                    {
                        PickUpComponent(pickup);
                        return;
                    }
                }
            }

            PickUpFocused();
        }

        private Component FindClosestPickup()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupDistance, lootMask, QueryTriggerInteraction.Collide);
            Component closest = null;
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                Component pickup = ResolvePickup(hit);
                if (pickup == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, pickup.transform.position);
                if (distance < bestDistance && distance <= pickupDistance + scanRadius)
                {
                    bestDistance = distance;
                    closest = pickup;
                }
            }

            return closest;
        }

        private void UpdatePrompt()
        {
            if (promptText == null)
            {
                return;
            }

            promptText.enabled = focusedPickup != null;
            if (focusedPickup is AdvancedWeaponPickup weaponPickup)
            {
                promptText.text = BuildPromptText(weaponPickup.RarityLabel, weaponPickup.DisplayName);
            }
            else if (focusedPickup is LootItem loot)
            {
                promptText.text = BuildPromptText(loot.RarityLabel, loot.DisplayName);
            }

            if (pickupButton != null)
            {
                bool hasPickup = focusedPickup != null;
                pickupButton.gameObject.SetActive(hasPickup);
                pickupButton.interactable = hasPickup;
            }
        }

        private static Component ResolvePickup(Collider hit)
        {
            if (hit == null)
            {
                return null;
            }

            AdvancedWeaponPickup weaponPickup = hit.GetComponentInParent<AdvancedWeaponPickup>();
            if (weaponPickup != null)
            {
                return weaponPickup;
            }

            return hit.GetComponentInParent<LootItem>();
        }

        private void PickUpComponent(Component pickup)
        {
            if (pickup == null || inventory == null)
            {
                return;
            }

            GameObject pickupObject = pickup.gameObject;
            if (pickupObject == null || !pickupObject.activeInHierarchy)
            {
                return;
            }

            bool consumed = false;

            if (pickup is AdvancedWeaponPickup weaponPickup)
            {
                consumed = weaponPickup.TryPickUp(inventory);
            }
            else if (pickup is LootItem loot)
            {
                consumed = loot.TryPickUp(inventory);
            }

            if (!consumed)
            {
                UpdatePrompt();
                return;
            }

            DisablePickupColliders(pickupObject);
            pickupObject.SetActive(false);
            focusedPickup = null;
            UpdatePrompt();
        }

        private void ConfigurePromptClickTarget()
        {
            if (promptText == null)
            {
                return;
            }

            promptText.raycastTarget = true;
            promptClickButton = promptText.GetComponent<Button>();
            if (promptClickButton == null)
            {
                promptClickButton = promptText.gameObject.AddComponent<Button>();
            }

            promptClickButton.targetGraphic = promptText;
            promptClickButton.transition = Selectable.Transition.None;
            promptClickButton.onClick.RemoveListener(PickUpFocused);
            promptClickButton.onClick.AddListener(PickUpFocused);
        }

        private void ConfigurePickupButton()
        {
            if (pickupButton == null)
            {
                return;
            }

            pickupButton.onClick.RemoveListener(PickUpFocused);
            pickupButton.onClick.AddListener(PickUpFocused);
            pickupButton.gameObject.SetActive(false);
        }

        private static string BuildPromptText(string rarity, string displayName)
        {
            string action = Application.isMobilePlatform ? "Tap PICKUP" : "Press E to pick up";
            return $"{action}\n[{rarity}] {displayName}";
        }

        private static void DisablePickupColliders(GameObject pickupObject)
        {
            Collider[] colliders = pickupObject.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }
    }
}
