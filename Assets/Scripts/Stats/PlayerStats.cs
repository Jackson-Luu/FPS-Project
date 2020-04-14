public class PlayerStats : CharacterStats
{
    public float GetHealthPct()
    {
        return (float)currHealth / maxHealth;
    }

    public override void Die(string sourceID)
    {
        base.Die(sourceID);
        GetComponent<Player>().Die(sourceID);
    }
}
