using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    public string defaultWeapon = "Pistol";
    public string meleeWeapon = "Knife";

    [HideInInspector]
    public Weapon primaryWeapon;
    [HideInInspector]
    public Weapon secondaryWeapon;

    [SerializeField]
    public Transform weaponHolder;

    private Weapon currentWeapon;
    private WeaponGraphics weaponGraphics;

    private WeaponSwitching weaponSwitcher;

    private Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>();

    public bool isReloading = false;

    public Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        weaponSwitcher = weaponHolder.GetComponent<WeaponSwitching>();
        EquipWeapon(defaultWeapon);
        GetComponent<EquipmentManager>().EquipDefault(weaponHolder);
    }

    public void EquipWeapon(string weaponName)
    {
        if (weapons.ContainsKey(weaponName))
        {
            weaponSwitcher.SelectWeapon(weaponName);
        } else
        {
            GameObject weaponPrefab = GameManager.GetWeapon(weaponName);
            GameObject weaponInstance = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation);
            weaponInstance.transform.SetParent(weaponHolder);
            weaponInstance.transform.localRotation = weaponPrefab.transform.rotation;
            weaponInstance.transform.localPosition = weaponPrefab.transform.position;
            weaponInstance.name = weaponName;
            weaponInstance.GetComponent<Collider>().enabled = false;

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

            weapons[weaponName] = weaponInstance.GetComponent<WeaponStats>().weapon;
            weaponSwitcher.SelectWeapon(weaponName);
            weapons[weaponName].SetBullets();
        }

        currentWeapon = weapons[weaponName];
    }

    public void SwitchWeapon()
    {
        if (currentWeapon == primaryWeapon)
        {
            currentWeapon = secondaryWeapon;
        } else
        {
            currentWeapon = primaryWeapon;
        }
    }

    public void Melee()
    {
        weaponSwitcher.SelectWeapon(meleeWeapon);
    }

    public void EquipCurrent()
    {
        weaponSwitcher.SelectWeapon(currentWeapon.name);
    }

    public void ClearBullets()
    {
        foreach (Weapon weapon in weapons.Values)
        {
            weapon.bullets = 0;
        }
    }

    public Weapon GetCurrentWeapon()
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
        CmdOnReload();
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = Mathf.Min(currentWeapon.maxBullets, inventory.ammo);
        isReloading = false;
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnReload()
    {
        Animator anim = weaponGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Reload");
        }
    }
}
