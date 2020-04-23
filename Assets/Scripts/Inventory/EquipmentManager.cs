using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    Equipment[] currentEquipment;

    Inventory inventory;

    public WeaponManager weaponManager;

    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem, EquipmentSlot slot);
    public OnEquipmentChanged onEquipmentChangedCallback;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        weaponManager = GetComponent<WeaponManager>();
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new Equipment[numSlots];
    }

    public void Equip(Equipment newItem)
    {
        int slotIndex = (int)newItem.equipmentSlot;

        Equipment oldItem = null;

        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
            inventory.Add(oldItem);
        }

        if (onEquipmentChangedCallback != null)
        {
            onEquipmentChangedCallback.Invoke(newItem, oldItem, newItem.equipmentSlot);
        }
        currentEquipment[slotIndex] = newItem;

        if (newItem.equipmentSlot == EquipmentSlot.Primary)
        {
            weaponManager.EquipWeapon(newItem.name);
        }
    }

    public void UnEquip(EquipmentSlot slot)
    {
        Debug.Log("EqManager: " + slot);
        int slotIndex = (int)slot;

        Equipment oldItem = null;
        Equipment newItem = null;

        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
            inventory.Add(oldItem);
        }

        if (onEquipmentChangedCallback != null)
        {
            onEquipmentChangedCallback.Invoke(newItem, oldItem, slot);
        }
        currentEquipment[slotIndex] = null;
    }

    public Equipment GetEquipment(EquipmentSlot slot)
    {
        return currentEquipment[(int)slot];
    }

    public void EquipDefault(Transform weaponHolder)
    {
        Equip((Equipment)weaponHolder.GetChild(0).GetComponent<ItemPickup>().item);
    }
}
