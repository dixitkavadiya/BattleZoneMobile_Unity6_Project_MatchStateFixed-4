using UnityEngine;

namespace BattleZoneMobile
{
    public class InventoryInputBridge : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private PlayerInventory inventory;

        public void Configure(UIManager manager, PlayerInventory playerInventory)
        {
            uiManager = manager;
            inventory = playerInventory;
        }

        private void Awake()
        {
            if (inventory == null)
            {
                inventory = GetComponent<PlayerInventory>();
            }
        }

        private void Update()
        {
            if (uiManager == null || inventory == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                uiManager.ToggleInventory();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && uiManager.InventoryVisible)
            {
                uiManager.HideInventory();
            }

            if (uiManager.InventoryVisible && Input.GetMouseButtonDown(1))
            {
                inventory.UseOrEquipBestInventoryAction();
            }
        }
    }
}
