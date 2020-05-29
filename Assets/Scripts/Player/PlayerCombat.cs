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

    private float meleeAnimationTime;

    protected override void Start()
    {
        base.Start();
        LayerMask units = 1 << LayerMask.NameToLayer("Units");
        LayerMask players = 1 << LayerMask.NameToLayer("RemotePlayer");
        layerMask = units | players;

        isPlayer = true;
        GetComponent<Player>().zombifyPlayer += ActivateMeleeMode;
        AnimationClip[] clips = playerAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "Melee_TwoHanded") { meleeAnimationTime = clip.length; }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!isLocalPlayer) { return; }
        if (Input.GetButtonDown("Melee") || (Input.GetButtonDown("Fire1") && meleeMode))
        {
            if (!GameManager.instance.chatSelected && attackCooldown <= 0f)
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
    }

    private IEnumerator MeleeAnimation()
    {
        StartCoroutine(weaponManager.Melee(meleeAnimationTime));
        CmdEquipMelee();
        playerAnimator.SetInteger("WeaponType_int", 12);
        playerAnimator.SetInteger("MeleeType_int", 2);
        yield return new WaitForSeconds(meleeAnimationTime);
        playerAnimator.SetInteger("WeaponType_int", weaponManager.currentWeapon.weaponType);
    }

    private IEnumerator UnarmedMeleeAnimation()
    {
        playerAnimator.SetInteger("WeaponType_int", 12);
        playerAnimator.SetInteger("MeleeType_int", 2);
        yield return new WaitForSeconds(meleeAnimationTime);
        playerAnimator.SetInteger("WeaponType_int", 0);
    }
    
    [Command]
    void CmdEquipMelee()
    {
        RpcEquipMelee();
        StartCoroutine(weaponManager.Melee(meleeAnimationTime));
    }

    [ClientRpc]
    void RpcEquipMelee()
    {
        if (isLocalPlayer) { return; }
        StartCoroutine(weaponManager.Melee(meleeAnimationTime));
    }
}
