using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : NetworkBehaviour
{
    public float lookRadius = 20f;
    private float speed = 2f;

    // For animation on clients
    [SyncVar]
    private float currVelocity = 0f;

    private NavMeshAgent agent;
    private CharacterCombat combat;

    private GameObject targetPlayer;
    public GameObject SetPlayer { set { targetPlayer = value; } }

    private Animator animator;

    private int layerMask;
    private float patrolRadius = 20f;
    private bool isPatrolling = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        animator = GetComponent<Animator>();

        layerMask = 1 << LayerMask.NameToLayer("RemotePlayer");

        //lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer != null)
        {
            Vector3 player = targetPlayer.transform.position;
            if (isServer)
            {
                agent.SetDestination(player);
            }

            if (Vector3.Distance(player, transform.position) <= (agent.stoppingDistance + 0.1f)) {
                // Attack
                Attack();
                FaceTarget();
            }
        } else
        {
            if (!isPatrolling)
            {
                isPatrolling = true;
                if (isServer)
                {
                    StartCoroutine(Patrol());
                }
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, lookRadius, layerMask);
            if (hitColliders.Length > 0)
            {
                isPatrolling = false;
                targetPlayer = hitColliders[0].gameObject;
            }
        }

        // Animate movement
        if (isServer)
        {
            currVelocity = agent.velocity.magnitude;
        }
        animator.SetFloat("Speed", currVelocity);
    }

    void Attack()
    {
        combat.Attack(targetPlayer.GetComponent<CharacterStats>());
    }
    
    void FaceTarget()
    {
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
    }

    IEnumerator Patrol()
    {
        while (isPatrolling)
        {
            agent.SetDestination(transform.position + (Random.insideUnitSphere * patrolRadius));
            yield return new WaitForSeconds(5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

    /*
    // Helper function for showing agent path
    private void DrawPath()
    {
        lineRenderer.SetPosition(0, transform.position);
        if (agent.path.corners.Length < 2)
        { //if the path has 1 or no corners, there is no need
            return;
        }
        lineRenderer.positionCount = agent.path.corners.Length;
        for (int i = 1; i < agent.path.corners.Length; i++)
        {
            lineRenderer.SetPosition(i, agent.path.corners[i]); //go through each corner and set that to the line renderer's position
        }
    }
    */
}
