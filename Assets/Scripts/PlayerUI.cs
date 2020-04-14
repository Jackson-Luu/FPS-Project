using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    RectTransform healthBar;

    [SerializeField]
    TMP_Text ammoText;

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    public GameObject deathScreen;

    private Player player;
    private PlayerStats playerStats;
    private WeaponManager weaponManager;

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
        SetHealth(playerStats.GetHealthPct());
        SetAmmo(weaponManager.GetCurrentWeapon().bullets, weaponManager.GetCurrentWeapon().maxBullets);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
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
