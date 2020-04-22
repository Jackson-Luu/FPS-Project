using UnityEngine;

[CreateAssetMenu(fileName = "New Heal", menuName = "Inventory/HealItem")]
public class HealItem : Consumable
{
    public override bool Use(Player player)
    {
        player.GetComponent<CharacterStats>().Heal(value);
        return true;
    }
}
