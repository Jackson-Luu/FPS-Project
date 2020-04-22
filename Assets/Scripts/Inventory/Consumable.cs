using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class Consumable : Item
{
    public int value = 0;

    public override bool Use(Player player)
    {
        base.Use(player);
        return false;
    }
}
