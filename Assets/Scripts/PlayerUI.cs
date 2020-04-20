using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    RectTransform healthBar;

    [SerializeField]
    TMP_Text ammoText;

    [SerializeField]
    TMP_Text itemPickup;
    private const string ITEM_PICKUP = "Press <color=yellow>F</color> to pickup ";

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    public GameObject deathScreen;

    public Player player;
    private PlayerStats playerStats;
    private WeaponManager weaponManager;

    private GameObject item;

    public void SetPlayer (Player _player)
    {
        player = _player;
        weaponManager = player.GetComponent<WeaponManager>();
        playerStats = player.GetComponent<PlayerStats>();
    }

    void Start()
    {
        PauseMenu.isOn = false;
    }

    void Update()
    {
        // Update UI health and ammo elements
        SetHealth(playerStats.GetHealthPct());
        SetAmmo(weaponManager.GetCurrentWeapon().bullets, weaponManager.GetCurrentWeapon().maxBullets);

        // Toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
            Cursor.lockState = CursorLockMode.None;
        }

        if (!PauseMenu.isOn)
        {
            // Pick up item
            if (Input.GetKeyDown(KeyCode.F))
            {
                ItemPickupDisable();
                player.TakeItem(item);
            }
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
    }

    public void ItemPickupEnable(GameObject itemObject, string source)
    {
        itemPickup.text = ITEM_PICKUP + source;
        itemPickup.gameObject.SetActive(true);
        item = itemObject;
    }

    public void ItemPickupDisable()
    {
        itemPickup.gameObject.SetActive(false);
    }

    void SetHealth(float amount)
    {
        healthBar.localScale = new Vector3(amount, 1f, 1f);
    }

    void SetAmmo(int current, int max)
    {
        ammoText.text = current.ToString() + " / " + max.ToString();
    }
}
