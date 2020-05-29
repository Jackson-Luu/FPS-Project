using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : NetworkBehaviour
{
    public float lookRadius = 20f;

    // For animation on clients
    [SyncVar]
    private float currVelocity = 0f;

    private NavMeshAgent agent;
    private CharacterCombat combat;

    private GameObject targetPlayer;
    private Player player;
    public GameObject SetPlayer { set { targetPlayer = value; player = targetPlayer.GetComponent<Player>(); player.playerDied += ResetPlayer; } }

    private Animator animator;

    [SerializeField]
    private AudioSource audioSource;

    private int layerMask;
    private float patrolRadius = 20f;
    private bool isPatrolling = false;

    private Coroutine audioCoroutine = null;
    private WaitForSeconds zombieAudioWait = new WaitForSeconds(20f);
    private bool client = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        animator = GetComponent<Animator>();

        layerMask = 1 << LayerMask.NameToLayer("RemotePlayer");
        if (isClient)
        {
            audioCoroutine = StartCoroutine(zombieAudio());
            client = true;
        }
    }

    private void OnEnable()
    {
        if (client && targetPlayer != null) { audioCoroutine = StartCoroutine(zombieAudio()); };
    }

    private void OnDisable()
    {
        if (targetPlayer != null) { player.playerDied -= ResetPlayer; targetPlayer = null; audioSource.Stop(); }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (targetPlayer != null)
            {
                Vector3 player = targetPlayer.transform.position;
                agent.SetDestination(player);

                if (Vector3.Distance(player, transform.position) <= (agent.stoppingDistance + 0.1f))
                {
                    // Attack
                    Attack();
                    FaceTarget();
                }
            }
            else
            {
                if (!isPatrolling)
                {
                    isPatrolling = true;
                    StartCoroutine(Patrol());
                }

                // Check if any players are within lookRadius
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, lookRadius, layerMask);
                foreach (Collider collider in hitColliders)
                {
                    // If royale, only target human players
                    if (GameManager.instance.isRoyale)
                    {
                        if (RoyaleManager.GetStatus(collider.name) != Player.PlayerStatus.Alive)
                        {
                            continue;
                        }
                    }

                    // Set player as new pathing target
                    targetPlayer = collider.gameObject;
                    isPatrolling = false;
                    targetPlayer.GetComponent<Player>().playerDied += ResetPlayer;
                    break;
                }
            }
            currVelocity = agent.velocity.magnitude;
        } else
        {
            // Animate movement on client
            animator.SetFloat("Speed_f", currVelocity);
        }
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
            Vector3 pos = transform.position + (Random.insideUnitSphere * patrolRadius);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            yield return new WaitForSeconds(5f);
        }
    }

    private void ResetPlayer()
    {
        player.playerDied -= ResetPlayer;
        targetPlayer = null;
    }

    IEnumerator zombieAudio()
    {
        while (targetPlayer != null)
        {
            yield return zombieAudioWait;
            audioSource.Play();
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
