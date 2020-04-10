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
        Vector3 playerDirection = (targetPlayer.transform.position - transform.position).normalized;
        playerDirection.y = 0;
        enemyRb.MovePosition(transform.position + playerDirection * speed * Time.deltaTime);
    }
}
