using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform equippedParent;
    public Transform backpackParent;

    public GameObject inventoryUI;

    Inventory inventory;
    InventorySlot[] equipped;
    InventorySlot[] backpack;

    void Start()
    {
        inventory = GetComponent<PlayerUI>().player.GetComponent<Inventory>();
        inventory.onItemChangedCallback += UpdateUI;

        equipped = equippedParent.GetComponentsInChildren<InventorySlot>();
        backpack = backpackParent.GetComponentsInChildren<InventorySlot>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }

    void UpdateUI()
    {
        int equipLength = equipped.Length;
        int bpLength = backpack.Length;

        for (int i = 0; i < equipLength + bpLength; i++)
        {
            if (i < inventory.items.Count)
            {
                if (i < equipped.Length)
                {
                    equipped[i].AddItem(inventory.items[i]);
                } else
                {
                    backpack[i - equipLength].AddItem(inventory.items[i]);
                }
            } else
            {
                if (i < equipped.Length)
                {
                    equipped[i].ClearSlot();
                }
                else
                {
                    backpack[i - equipLength].ClearSlot();
                }
            }
        }
    }
}
