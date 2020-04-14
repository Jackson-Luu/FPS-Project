public class EnemyStats : CharacterStats
{
    public override void Die(string sourceID)
    {
        base.Die(sourceID);
        GetComponent<Enemy>().Die(sourceID);
    }
}
