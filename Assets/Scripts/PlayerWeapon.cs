using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "AK47";

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 10f;

    public int maxBullets = 20;
    [HideInInspector]
    public int bullets;

    public float reloadTime = 2f;
    
    public GameObject graphics;

    public PlayerWeapon()
    {
        bullets = maxBullets;
    }
}
