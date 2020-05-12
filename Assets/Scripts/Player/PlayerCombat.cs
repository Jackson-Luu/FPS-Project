using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterStats))]
public class PlayerCombat : CharacterCombat
{
    [SerializeField]
    private Transform cam;
    private LayerMask layerMask;

    public float meleeRange = 2f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        LayerMask units = 1 << LayerMask.NameToLayer("Units");
        LayerMask players = 1 << LayerMask.NameToLayer("RemotePlayer");
        layerMask = units | players;
    }

    public void Melee()
    {
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
}
