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
    private Weapon currentWeapon;
    private Transform weaponHolder;
    private Vector3 weaponInitialAngle;
    private Inventory inventory;

    // Recoil variables
    public Vector2 kickMinMax = new Vector2(0.01f, 0.05f);
    public Vector2 recoilAngleMinMax = new Vector2(5f, 8f);
    public float recoilSettleTime = 0.1f;
    public float recoilRotateSettleTime = 0.5f;
    private Vector3 recoilSmoothDampVelocity;
    private float recoilRotSmoothDampVelocity;
    private float recoilAngle;

    [SerializeField]
    private Transform camParent;

    private AudioSource audioSource;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            Debug.Log("No camera referenced.");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
        inventory = GetComponent<Inventory>();
        weaponManager.onWeaponSwitched += SwitchWeapon;
        weaponHolder = weaponManager.weaponHolder;
        weaponInitialAngle = weaponHolder.localEulerAngles;

        // Parent muzzle flash to main camera of local player
        if (isLocalPlayer)
        {
            muzzleFlash.transform.SetParent(cam.transform);
            muzzleFlash.transform.localEulerAngles = Vector3.zero;
            muzzleFlash.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }

    private void SwitchWeapon()
    {
        currentWeapon = weaponManager.currentWeapon;
        audioSource = weaponManager.currentWeaponObject.GetComponent<AudioSource>();

        // Calibrate muzzle flash position to new weapon
        if (currentWeapon != null && isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                muzzleFlash.transform.localPosition = currentWeapon.muzzleFlashPosition;
            } else
            {
                muzzleFlash.transform.localPosition = currentWeapon.worldMuzzleFlashPosition;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.isOn || !isLocalPlayer) { return; }

        if (currentWeapon.bullets < currentWeapon.maxBullets)
        {
            if (Input.GetButton("Reload"))
            {
                if ((inventory.ammo - currentWeapon.bullets) > 0)
                {
                    weaponManager.Reload();
                }
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

        // Recoil
        weaponHolder.localPosition = Vector3.SmoothDamp(weaponHolder.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotateSettleTime);
        weaponHolder.localEulerAngles = weaponInitialAngle + Vector3.down * recoilAngle;
        camParent.localEulerAngles = Vector3.left * recoilAngle / 10f;
    }

    // Method only called on client
    [Client]
    void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading || InventoryUI.isOn || inventory.ammo <= 0) { return; }

        if (currentWeapon.bullets <= 0) {
            weaponManager.Reload();
            return;
        }

        // Play sound effect
        CmdPlayClip();

        // Use gun bullets and inventory ammo
        currentWeapon.bullets--;
        inventory.ammo--;

        // Recoil
        weaponHolder.localPosition -= Vector3.right * Random.Range(kickMinMax.x, kickMinMax.y);
        recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
        recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

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
            if (inventory.ammo > 0)
            {
                weaponManager.Reload();
            } else
            {
                inventory.CmdRemoveAmmo();
            }
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
        muzzleFlash.Play();
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
        // TODO : Object pool effects
        //GameObject hitEffect = (GameObject)Instantiate(weaponManager.GetWeaponGraphics().impactEffectPrefab, pos, Quaternion.LookRotation(normal));
        //Destroy(hitEffect, 2f);
    }

    // Method only called on server
    [Command]
    void CmdTargetShot(GameObject target, float damage, string sourceID)
    {
        CharacterStats character = target.GetComponent<CharacterStats>();
        character.TakeDamage(damage, sourceID);
    }

    // Play firing audio clips
    [Command]
    void CmdPlayClip()
    {
        RpcPlayClip();
    }

    [ClientRpc]
    void RpcPlayClip()
    {
        audioSource.PlayOneShot(currentWeapon.fire);
    }
}