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
    private bool sprinting = false;

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
    private Collider playerCollider;
    private PlayerCombat playerCombat;
    private WeaponManager weaponManager;

    [SerializeField]
    private Animator weaponAnimator;

    [SerializeField]
    private AudioSource audioSource;
    private AudioClip footsteps0;
    private AudioClip footsteps1;
    bool footstepsPlaying = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        weaponManager = GetComponent<WeaponManager>();

        StartCoroutine(DeactivateGravity(GameManager.instance.matchSettings.playerLoadTime));
        footsteps0 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 0);
        footsteps1 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 1);
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
        MovePlayer();
    }

    void MovePlayer()
    {
        if (!PauseMenu.isOn)
        {
            if (characterController.isGrounded)
            {
                // We are grounded, so recalculate
                // move direction directly from axes
                moveX = Input.GetAxis("Horizontal");
                moveZ = Input.GetAxis("Vertical");

                if (moveX == 0 && moveZ == 0)
                {
                    audioSource.Stop();
                }

                moveDirection = moveX * transform.right + moveZ * transform.forward;
                moveDirection *= speed;

                if (Input.GetButtonDown("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                    animator.SetTrigger("Jump_t");
                    audioSource.Stop();
                    footstepsPlaying = false;
                } else
                {
                    if (!footstepsPlaying && (moveX != 0 || moveZ != 0))
                    {
                        StartCoroutine(Footsteps());
                        footstepsPlaying = true;
                    }
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
                } else if (moveX != 0)
                {
                    animator.SetFloat("Speed_f", moveX);
                }
            }

            if (Input.GetButtonDown("Melee"))
            {
                if (playerCombat.attackCooldown <= 0f)
                {
                    playerCombat.Melee();
                    StartCoroutine(MeleeAnimation());
                }
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);        
    }

    private IEnumerator MeleeAnimation()
    {
        weaponManager.Melee();
        animator.SetInteger("WeaponType_int", 12);
        animator.SetInteger("MeleeType_int", 2);
        weaponAnimator.SetTrigger("Melee_t");
        yield return new WaitForSeconds(1.3f);
        weaponManager.EquipCurrent();
        animator.SetInteger("WeaponType_int", 1);
    }

    private IEnumerator Footsteps()
    {
        while ((moveX != 0 || moveZ != 0) && characterController.isGrounded)
        {
            audioSource.clip = footsteps0;
            audioSource.Play();
            Debug.Log("PLAYING CLIP 1");
            yield return new WaitForSeconds(audioSource.clip.length);
            audioSource.clip = footsteps1;
            audioSource.Play();
            Debug.Log("PLAYING CLIP 2");
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        footstepsPlaying = false;
    }
}
