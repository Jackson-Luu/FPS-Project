using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : NetworkBehaviour
{
    public float attackSpeed = 0.5f;
    public float attackCooldown = 0f;

    CharacterStats myStats;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        myStats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    public void Attack(CharacterStats targetStats)
    {
        if (attackCooldown <= 0f)
        {
            attackCooldown = 1f / attackSpeed;
            targetStats.TakeDamage(myStats.damage.GetValue(), gameObject.name);
        }
    }
}
