using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static bool isOn = false;

    [SerializeField]
    private Transform gearParent;
    [SerializeField]
    private Transform equipmentParent;
    [SerializeField]
    private Transform inventoryParent;

    public GameObject inventoryUI;

    [HideInInspector]
    public Inventory inventory;
    private EquipmentManager equipmentManager;

    InventorySlot[] equipmentSlots;
    InventorySlot[] inventorySlots;

    void Start()
    {
        // Subscribe to inventory/equipment manager on change delegates
        inventory = GetComponent<PlayerUI>().player.GetComponent<Inventory>();
        inventory.onItemChangedCallback += UpdateInventory;

        equipmentManager = inventory.equipmentManager;
        equipmentManager.onEquipmentChangedCallback += UpdateEquipment;

        // Initialise inventory slots and link to UI slots
        InventorySlot[] gear = gearParent.GetComponentsInChildren<InventorySlot>();
        InventorySlot[] equipment = equipmentParent.GetComponentsInChildren<InventorySlot>();
        inventorySlots = inventoryParent.GetComponentsInChildren<InventorySlot>();

        equipmentSlots = new InventorySlot[gear.Length + equipment.Length];

        gear.CopyTo(equipmentSlots, 0);
        equipment.CopyTo(equipmentSlots, gear.Length);

        // Initialise inventory indexes to identify slots
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].index = i;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].index = i;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        isOn = inventoryUI.activeSelf;
        if (inventoryUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateInventory()
    {
        for (int i = 0; i < inventory.space; i++)
        {
            if (i < inventory.items.Count)
            {
                inventorySlots[i].AddItem(inventory.items[i]);
            } else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    void UpdateEquipment(Equipment newItem, Equipment oldItem, EquipmentSlot slot)
    {
        if (newItem != null)
        {
            equipmentSlots[(int)slot].AddItem(newItem);
        } else
        {
            equipmentSlots[(int)slot].ClearSlot();
        }
    }
}
