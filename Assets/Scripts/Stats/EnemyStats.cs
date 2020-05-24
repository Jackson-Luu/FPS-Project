public class EnemyStats : CharacterStats
{
    private Enemy enemy;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    public override void Die(string sourceID)
    {
        base.Die(sourceID);
        enemy.Die(sourceID);
    }
}
