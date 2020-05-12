using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField]
    private WeaponManager weaponManager;

    string selectedWeapon;

    // Update is called once per frame
    void Update()
    {
        selectedWeapon = weaponManager.currentWeapon.name;

        /*if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedWeapon++;

            // Wrap around if > number of weapons
            selectedWeapon %= 2;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedWeapon--;
            selectedWeapon %= 2;
        }

        if (previousWeapon != selectedWeapon)
        {
            SelectWeapon(selectedWeapon);
        }
        */

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = weaponManager.primaryWeapon.name;
            CheckWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = weaponManager.secondaryWeapon.name;
            CheckWeapon();
        }
    }

    public void SelectWeapon(string weaponName)
    {
        foreach (Transform weapon in transform)
        {
            if (weapon.name == weaponName)
            {
                weapon.gameObject.SetActive(true);
                weaponManager.currentWeaponObject = weapon.gameObject;
                weaponManager.audioSource = weapon.gameObject.GetComponent<AudioSource>();
            } else
            {
                weapon.gameObject.SetActive(false);
            }
        }

        if (weaponManager.onWeaponSwitched != null)
        {
            weaponManager.onWeaponSwitched.Invoke();
        }        
    }

    private void CheckWeapon()
    {
        if (selectedWeapon != weaponManager.currentWeapon.name)
        {
            SelectWeapon(selectedWeapon);
            weaponManager.SwitchWeapon();
        }
    }
}
