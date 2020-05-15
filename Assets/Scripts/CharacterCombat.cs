using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : NetworkBehaviour
{
    public float attackSpeed = 0.5f;
    public float attackCooldown = 0f;

    CharacterStats myStats;

    protected bool isPlayer = false;

    [SerializeField]
    protected NetworkAnimator networkAnimator;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        myStats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Return if this is a remote player on clients
        if (!isLocalPlayer && !isServer) { return; }
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
            if (!isPlayer)
            {
                networkAnimator.SetTrigger("Melee_t");
            }
        }
    }
}
