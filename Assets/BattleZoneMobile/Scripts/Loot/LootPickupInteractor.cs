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

        private LootItem focusedLoot;

        public void ConfigureForRuntime(Camera camera, PlayerInventory playerInventory, Text pickupPrompt, LayerMask mask)
        {
            playerCamera = camera;
            inventory = playerInventory;
            promptText = pickupPrompt;
            lootMask = mask;
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
        }

        private void Update()
        {
            focusedLoot = FindClosestLoot();
            UpdatePrompt();

            if (WasPickupTapped(out Vector2 screenPosition))
            {
                TryPickUp(screenPosition);
            }
        }

        public void PickUpFocused()
        {
            if (focusedLoot != null)
            {
                focusedLoot.PickUp(inventory);
            }
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
                    LootItem tappedLoot = hit.collider.GetComponentInParent<LootItem>();
                    if (tappedLoot != null && Vector3.Distance(transform.position, tappedLoot.transform.position) <= pickupDistance)
                    {
                        tappedLoot.PickUp(inventory);
                        return;
                    }
                }
            }

            PickUpFocused();
        }

        private LootItem FindClosestLoot()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupDistance, lootMask, QueryTriggerInteraction.Collide);
            LootItem closest = null;
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                LootItem loot = hit.GetComponentInParent<LootItem>();
                if (loot == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, loot.transform.position);
                if (distance < bestDistance && distance <= pickupDistance + scanRadius)
                {
                    bestDistance = distance;
                    closest = loot;
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

            promptText.enabled = focusedLoot != null;
            if (focusedLoot != null)
            {
                promptText.text = $"Tap to pick up [{focusedLoot.RarityLabel}] {focusedLoot.DisplayName}";
            }
        }
    }
}
