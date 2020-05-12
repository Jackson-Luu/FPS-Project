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

    public Weapon currentWeapon;
    public GameObject currentWeaponObject;
    private WeaponGraphics weaponGraphics;

    private WeaponSwitching weaponSwitcher;

    private Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>();

    public bool isReloading = false;

    public Inventory inventory;

    public delegate void OnWeaponSwitched();
    public OnWeaponSwitched onWeaponSwitched;

    public AudioSource audioSource;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
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
        audioSource.clip = currentWeapon.reloadOut;
        audioSource.Play();
        animator.SetTrigger("Reload_t");
        yield return new WaitForSeconds(currentWeapon.reloadTime / 2);
        audioSource.clip = currentWeapon.reloadIn;
        audioSource.Play();
        yield return new WaitForSeconds(currentWeapon.reloadTime / 2);

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
