using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : MonoBehaviour
{
    public float lookRadius = 10f;
    private float speed = 2f;

    private NavMeshAgent agent;

    private GameObject targetPlayer;
    public GameObject SetPlayer { set { targetPlayer = value; } }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (targetPlayer != null)
        {
            Vector3 player = targetPlayer.transform.position;
            player.y = 0;
            agent.SetDestination(player);

            if (Vector3.Distance(player, transform.position) <= agent.stoppingDistance) {
                // Attack
                FaceTarget();
            }
        } else
        {
            // Todo: behaviour after player dies.
            // agent.SetDestination(transform.position);
        }
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
