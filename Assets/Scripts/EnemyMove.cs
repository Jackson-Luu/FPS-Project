using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : MonoBehaviour
{
    private float speed = 2f;

    private Rigidbody enemyRb;
    private GameObject targetPlayer;
    public GameObject SetPlayer { set { targetPlayer = value; } }

    
    // Start is called before the first frame update
    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerDirection;
        if (targetPlayer != null)
        {
            playerDirection = (targetPlayer.transform.position - transform.position).normalized;
            playerDirection.y = 0;
        } else
        {
            playerDirection = Vector3.zero;
        }
        enemyRb.MovePosition(transform.position + playerDirection * speed * Time.deltaTime);
    }
}
