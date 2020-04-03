using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    // Move variables
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private float mapBound = 10;

    public float mouseSens = 2.0f;

    private Vector3 moveDirection = Vector3.zero;
    private float groundRadius;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Get terrain radius (square)
        groundRadius = GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2;
    }

    void Update()
    {
        MovePlayer();
        ConstrainPlayerPosition();
    }

    void MovePlayer()
    {
        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void ConstrainPlayerPosition()
    {
        if (transform.position.x > groundRadius)
        {
            transform.position = new Vector3(groundRadius, transform.position.y, transform.position.z);
        }

        if (transform.position.z > groundRadius)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, groundRadius);
        }

        if (transform.position.x < -groundRadius)
        {
            transform.position = new Vector3(-groundRadius, transform.position.y, transform.position.z);
        }

        if (transform.position.z < -groundRadius)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -groundRadius);
        }
    }
}
