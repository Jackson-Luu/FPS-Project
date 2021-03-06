﻿using UnityEngine;
using System.Collections;
using Mirror;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;

    // Move variables
    [SerializeField]
    private float speed = 3.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 20.0f;

    public float mouseSens = 2.0f;

    [SerializeField]
    private float sprintMultiplier = 3f;

    [SerializeField]
    private float staminaBurnRate = 1.5f;

    [SerializeField]
    private float staminaRegenRate = 0.7f;
    private float stamina = 5f;
    private float maxStamina = 5f;

    private float moveX;
    private float moveZ;
    private Vector3 moveDirection = Vector3.zero;

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private Collider playerCollider;
    private PlayerCombat playerCombat;

    [SerializeField]
    private AudioController audioController;
    bool footstepsPlaying = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        playerCombat = GetComponent<PlayerCombat>();

        StartCoroutine(DeactivateGravity(GameManager.instance.matchSettings.playerLoadTime));
        animator.SetFloat("Body_Horizontal_f", -0.005f);
        animator.SetFloat("Body_Vertical_f", -0.01f);
    }

    public IEnumerator DeactivateGravity(float duration)
    {
        float temp = gravity;
        gravity = 0f;
        yield return new WaitForSeconds(duration);
        gravity = temp;
    }

    void Update()
    {
        if (!isLocalPlayer) { return; }
        MovePlayer();

        if (transform.position.y < -100)
        {
            ResetPlayerPos();
        }
    }

    void ResetPlayerPos()
    {
        characterController.enabled = false;
        transform.position = new Vector3(transform.position.x, 400, transform.position.z);
        characterController.enabled = true;
    }

    void MovePlayer()
    {
        if (!PauseMenu.isOn)
        {
            if (characterController.isGrounded)
            {
                if (!GameManager.instance.chatSelected)
                {
                    // We are grounded, so recalculate
                    // move direction directly from axes
                    moveX = Input.GetAxis("Horizontal");
                    moveZ = Input.GetAxis("Vertical");

                    moveDirection = moveX * transform.right + moveZ * transform.forward;
                    moveDirection *= speed;

                    if (Input.GetButtonDown("Jump"))
                    {
                        moveDirection.y = jumpSpeed;
                        networkAnimator.SetTrigger("Jump_t");

                        // If jumping, stop footsteps
                        if (footstepsPlaying)
                        {
                            audioController.StopClip();
                            footstepsPlaying = false;
                        }
                    }
                    else
                    {
                        // Stop footsteps if stationary
                        if (moveX == 0 && moveZ == 0)
                        {
                            if (footstepsPlaying)
                            {
                                audioController.StopClip();
                                footstepsPlaying = false;
                            }
                        }
                        else if (!footstepsPlaying)
                        {
                            audioController.PlayClip();
                            footstepsPlaying = true;
                        }
                    }
                }

                // Sprinting calls
                if (Input.GetButton("Sprint") && !GameManager.instance.chatSelected && stamina > 0f)
                {
                    stamina -= staminaBurnRate * Time.deltaTime;
                    if (stamina >= 0.01f)
                    {
                        moveDirection.x *= sprintMultiplier;
                        moveDirection.y *= 1.05f;
                        moveDirection.z *= sprintMultiplier;
                    }
                }
                else
                {
                    stamina += staminaRegenRate * Time.deltaTime;
                }

                // Restrict max stamina
                stamina = Mathf.Clamp(stamina, 0f, maxStamina);

                // Animate movement
                if (moveZ != 0)
                {
                    animator.SetFloat("Speed_f", moveZ);
                }
                else
                {
                    // Set animation to moveX or 0 if not moving
                    animator.SetFloat("Speed_f", moveX);
                }
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);        
    }
}
