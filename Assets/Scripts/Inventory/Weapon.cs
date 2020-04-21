using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 10f;

    public int maxBullets = 20;
    public float reloadTime = 2f;

    public GameObject graphics;
}
