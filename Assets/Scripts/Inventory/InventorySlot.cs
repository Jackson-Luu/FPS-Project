using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public int index;
    public Image icon;
    public Item item;

    public InventoryUI inventoryUI;
    private Inventory inventory;

    private void Start()
    {
        inventory = inventoryUI.inventory;
    }

    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
    }

    public void UseItem()
    {
        if (item.name == "Ammo" ) { 
            // do nothing
        } else if (item is Equipment)
        {
            inventory.equipItem(index);
        }
        else
        {
            inventory.UseItem(index);
        }
    }

    public void UnEquip()
    {
        inventory.UnEquipItem(index);
    }
}
