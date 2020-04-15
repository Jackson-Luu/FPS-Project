using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : NetworkBehaviour
{
    public float lookRadius = 10f;
    private float speed = 2f;

    // For animation on clients
    [SyncVar]
    private float currVelocity = 0f;

    private NavMeshAgent agent;
    private CharacterCombat combat;

    private GameObject targetPlayer;
    public GameObject SetPlayer { set { targetPlayer = value; } }

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer != null)
        {
            Vector3 player = targetPlayer.transform.position;
            player.y = 0;
            agent.SetDestination(player);

            if (Vector3.Distance(player, transform.position) <= agent.stoppingDistance) {
                // Attack
                Attack();
                FaceTarget();
            }
        } else
        {
            // Todo: behaviour after player dies.
            // agent.SetDestination(transform.position);
        }

        // Animate movement
        if (isServer)
        {
            currVelocity = agent.velocity.magnitude;
        }
        Debug.Log(currVelocity);
        animator.SetFloat("Speed", currVelocity);
    }

    void Attack()
    {
        combat.Attack(targetPlayer.GetComponent<CharacterStats>());
        if (isServer)
        {
            RpcAttack();
        }
    }
    
    [ClientRpc]
    void RpcAttack()
    {
        //combat.Attack(targetPlayer.GetComponent<CharacterStats>());
    }

    void FaceTarget()
    {
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
