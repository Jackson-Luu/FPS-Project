﻿using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon primaryWeapon;

    [SerializeField]
    private Transform weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics weaponGraphics;

    private bool isReloading = false;

    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    void EquipWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;

        GameObject weaponInstance = Instantiate(weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        weaponInstance.transform.SetParent(weaponHolder);

        weaponGraphics = weaponInstance.GetComponent<WeaponGraphics>();
        if (weaponGraphics == null)
        {
            Debug.LogError("No WeaponGraphics on weapon: " + weaponInstance.name);
        }

        if (isLocalPlayer)
        {
            // Add weapon to weapon camera layer to prevent seeing gun clipping into objects
            Util.SetLayerRecursively(weaponInstance, LayerMask.NameToLayer("Weapon"));
        }
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetWeaponGraphics()
    {
        return weaponGraphics;
    }

    public void Reload()
    {
        if (isReloading) { return; }
        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;
        isReloading = false;
    }
}
