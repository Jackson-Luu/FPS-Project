using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private float speed = 2f;
    private float groundRadius;

    private Rigidbody zombieRb;
    private GameObject player;

    /*
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        zombieRb = GetComponent<Rigidbody>();
        groundRadius = GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        playerDirection.y = 0;
        zombieRb.MovePosition(transform.position + playerDirection * speed * Time.deltaTime);
    }
    */
}
