using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
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

    private Vector3 moveDirection = Vector3.zero;

    private Animator animator;
    private Collider playerCollider;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();

        StartCoroutine(ActivateGravity());
    }

    private IEnumerator ActivateGravity()
    {
        float temp = gravity;
        gravity = 0f;
        yield return new WaitForSeconds(GameManager.instance.matchSettings.playerLoadTime);
        gravity = temp;
    }

    void Update()
    {
        if (PauseMenu.isOn) { return; }
        MovePlayer();
    }

    void MovePlayer()
    {
        // Lock and hide cursor
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            moveDirection =  moveX * transform.right + moveZ * transform.forward;
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }

            if (Input.GetButton("Sprint") && stamina > 0f)
            {
                stamina -= staminaBurnRate * Time.deltaTime;
                if (stamina >= 0.01f)
                {
                    moveDirection.x *= sprintMultiplier;
                    moveDirection.y *= 1.05f;
                    moveDirection.z *= sprintMultiplier;
                }
            } else
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }

            // Restrict max stamina
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);

            // Animate movement
            animator.SetFloat("Speed", moveZ);
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Units"))
        {
            Physics.IgnoreCollision(playerCollider, collision.collider);
        }
    }
}
