﻿using UnityEngine;
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
        if (!isLocalPlayer) { return; }

        // Call OnShoot method on server
        CmdOnShoot();

        RaycastHit hit;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.range, mask))
        {
            Debug.Log("Checkpoint 1");
            if (hit.collider.CompareTag("Player")) // Second case removes self damage bug at time of writing && hit.collider.transform.name != transform.name
            {
                Debug.Log("Checkpoint 2");
                CmdPlayerShot(hit.collider.name, currentWeapon.damage);
            }

            // Call hit effects on impact point
            CmdOnHit(hit.point, hit.normal);
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
    void CmdPlayerShot(string playerID, float damage)
    {
        Debug.Log(playerID + " has been shot.");

        Player player = GameManager.GetPlayer(playerID).GetComponent<Player>();
        player.TakeDamage(damage);
        player.RpcTakeDamage(damage);
    }
}
