using UnityEngine;
using Mirror;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    // Layer mask to restrict what objects player can hit
    [SerializeField]
    private LayerMask mask;

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            Debug.Log("No camera referenced.");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.isOn) { return; }

        currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon.bullets < currentWeapon.maxBullets)
        {
            if (Input.GetButton("Reload"))
            {
                weaponManager.Reload();
            }
        }

        // Non-auto weapon
        if (currentWeapon.fireRate <= 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();

                // May have to use waitForSeconds to prevent spamming a non-auto weapon
            }

        // Automatic weapon
        } else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            } else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }

    }

    // Method only called on client
    [Client]
    void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading) { return; }

        if (currentWeapon.bullets <= 0) {
            weaponManager.Reload();
            return;
        }

        currentWeapon.bullets--;
        // Call OnShoot method on server
        CmdOnShoot();

        RaycastHit hit;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.range, mask))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy")) // Second case removes self damage bug at time of writing && hit.collider.transform.name != transform.name
            {
                CmdTargetShot(hit.collider.gameObject, currentWeapon.damage, gameObject.name);
            }

            // Call hit effects on impact point
            CmdOnHit(hit.point, hit.normal);
        }

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
        }
    }

    // Called on server when a player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    // Called on all clients to show shooting effects of all players
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetWeaponGraphics().muzzleFlash.Play();
    }

    // Called on server when we hit something
    [Command]
    void CmdOnHit(Vector3 pos, Vector3 normal)
    {
        RpcDoHitEffect(pos, normal);
    }

    // Called across clients to spawn impact effects
    [ClientRpc]
    void RpcDoHitEffect(Vector3 pos, Vector3 normal)
    {
        GameObject hitEffect = (GameObject)Instantiate(weaponManager.GetWeaponGraphics().impactEffectPrefab, pos, Quaternion.LookRotation(normal));
        Destroy(hitEffect, 2f);
    }

    // Method only called on server
    [Command]
    void CmdTargetShot(GameObject target, float damage, string sourceID)
    {
        Debug.Log(target.name + " has been shot.");

        CharacterStats character = target.GetComponent<CharacterStats>();
        character.TakeDamage(damage, sourceID);
        //character.RpcTakeDamage(damage, sourceID);
    }
}
