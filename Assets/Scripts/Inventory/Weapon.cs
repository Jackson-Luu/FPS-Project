using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    new public string name = "New Weapon";
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 10f;

    public int maxBullets = 20;
    public float reloadTime = 2f;

    public Vector2 kickMinMax = new Vector2(0.01f, 0.05f);
    public Vector2 recoilAngleMinMax = new Vector2(5f, 8f);

    public bool auto = false;

    public GameObject graphics;
    public AudioClip fire;
    public AudioClip reloadOut;
    public AudioClip reloadIn;

    public Vector3 weaponCameraPosition;
    public Vector3 weaponCameraRotation;
    public Vector3 muzzleFlashPosition;
    public int weaponType;

    //[HideInInspector]
    public int bullets;

    public void SetBullets()
    {
        bullets = maxBullets;
    }
}
