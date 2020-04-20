using UnityEngine;

[CreateAssetMenu(fileName = "New Heal", menuName = "Inventory/HealItem")]
public class HealItem : Item
{
    float heal = 50;

    public override void Use(Player player)
    {
        player.GetComponent<CharacterStats>().Heal(heal);
    }
}
