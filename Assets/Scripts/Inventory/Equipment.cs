using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    public EquipmentSlot equipmentSlot;
    public int armorModifier;
}

public enum EquipmentSlot { Head, Chest, Legs, Feet, Primary, Secondary, Melee, Grenade, Utility }
