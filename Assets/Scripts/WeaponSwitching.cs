using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField]
    private WeaponManager weaponManager;

    string selectedWeapon;

    // Update is called once per frame
    void Update()
    {
        selectedWeapon = weaponManager.GetCurrentWeapon().name;

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
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = weaponManager.secondaryWeapon.name;
        }

        if (selectedWeapon != weaponManager.GetCurrentWeapon().name)
        {
            SelectWeapon(selectedWeapon);
            weaponManager.SwitchWeapon();
        }
    }

    public void SelectWeapon(string weaponName)
    {
        foreach (Transform weapon in transform)
        {
            if (weapon.name == weaponName)
            {
                weapon.gameObject.SetActive(true);
            } else
            {
                weapon.gameObject.SetActive(false);
            }
        }
    }

    private void CheckWeapon()
    {
        if (selectedWeapon != weaponManager.GetCurrentWeapon().name)
        {
            SelectWeapon(selectedWeapon);
            weaponManager.SwitchWeapon();
        }
    }
}
