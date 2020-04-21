public class PlayerStats : CharacterStats
{
    private void Start()
    {
        GetComponent<EquipmentManager>().onEquipmentChangedCallback += OnEquipmentChanged;
    }
    public float GetHealthPct()
    {
        return (float)currHealth / maxHealth;
    }

    public override void Die(string sourceID)
    {
        base.Die(sourceID);
        GetComponent<Player>().Die(sourceID);
    }

    private void OnEquipmentChanged(Equipment newItem, Equipment oldItem, EquipmentSlot slot)
    {
        if (newItem != null)
        {
            armor.AddModifier(newItem.armorModifier);
        }

        if (oldItem != null)
        {
            armor.RemoveModifier(oldItem.armorModifier);
        }
    }
}
