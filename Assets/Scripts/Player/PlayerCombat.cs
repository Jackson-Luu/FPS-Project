using UnityEngine;
using System.Collections;
using Mirror;

[RequireComponent(typeof(CharacterStats))]
public class PlayerCombat : CharacterCombat
{
    [SerializeField]
    private Transform cam;
    private LayerMask layerMask;

    public float meleeRange = 2f;
    private bool meleeMode = false;

    [SerializeField]
    private Animator playerAnimator;

    [SerializeField]
    private WeaponManager weaponManager;

    protected override void Start()
    {
        base.Start();
        LayerMask units = 1 << LayerMask.NameToLayer("Units");
        LayerMask players = 1 << LayerMask.NameToLayer("RemotePlayer");
        layerMask = units | players;

        GetComponent<Player>().zombifyPlayer += ActivateMeleeMode;
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetButtonDown("Melee") || (Input.GetButtonDown("Fire1") && meleeMode))
        {
            if (attackCooldown <= 0f)
            {
                Melee();
            }
        }
    }

    public void Melee()
    {
        if (meleeMode)
        {
            StartCoroutine(UnarmedMeleeAnimation());
        } else
        {
            StartCoroutine(MeleeAnimation());
        }
        attackCooldown = 1f / attackSpeed;
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.forward, out hit, meleeRange, layerMask))
        {
            CmdMelee(hit.collider.gameObject);
        }
    }

    [Command]
    void CmdMelee(GameObject target)
    {
        // Cooldown double checked on server in attack function
        Attack(target.GetComponent<CharacterStats>());
    }

    void ActivateMeleeMode()
    {
        meleeMode = true;
        playerAnimator.SetInteger("WeaponType_int", 0);
        playerAnimator.SetInteger("MeleeType_int", 2);
    }

    private IEnumerator MeleeAnimation()
    {
        weaponManager.Melee();
        playerAnimator.SetInteger("WeaponType_int", 12);
        playerAnimator.SetInteger("MeleeType_int", 2);
        yield return new WaitForSeconds(1.3f);
        weaponManager.EquipCurrent();
        playerAnimator.SetInteger("WeaponType_int", 1);
    }

    private IEnumerator UnarmedMeleeAnimation()
    {
        playerAnimator.SetInteger("WeaponType_int", 12);
        yield return new WaitForSeconds(1.3f);
        playerAnimator.SetInteger("WeaponType_int", 0);
    }
}
