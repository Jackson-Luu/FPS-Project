using System.Collections;
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

    public bool isReloading = false;

    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    public void EquipWeapon(PlayerWeapon weapon)
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
        CmdOnReload();
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;
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
