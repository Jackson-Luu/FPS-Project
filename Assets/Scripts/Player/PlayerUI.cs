using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    RectTransform healthBar;

    [SerializeField]
    RectTransform ammoPanel;

    [SerializeField]
    TMP_Text ammoText;

    [SerializeField]
    TMP_Text itemPickup;
    private const string ITEM_PICKUP = "Press <color=yellow>F</color> to pickup ";

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    public GameObject deathScreen;

    [SerializeField]
    GameObject gameOverScreen;

    [SerializeField]
    TMP_Text gameOverText;

    [SerializeField]
    GameObject readyPanel;
    [SerializeField]
    TMP_Text readyStatus;
    [SerializeField]
    GameObject readyButton;
    [SerializeField]
    TMP_Text waitingStatus;

    [SerializeField]
    Animator crosshair;

    [HideInInspector]
    public Player player;
    private PlayerStats playerStats;
    private WeaponManager weaponManager;
    private Inventory inventory;

    private GameObject item;

    private bool zombie = false;

    public void SetPlayer (Player _player)
    {
        player = _player;
        weaponManager = player.GetComponent<WeaponManager>();
        playerStats = player.GetComponent<PlayerStats>();
        inventory = player.GetComponent<Inventory>();
        roomPlayer = player.GetComponent<RoomPlayer>();
    }

    private NetworkRoomManager room;
    private RoomPlayer roomPlayer;

    void Start()
    {
        PauseMenu.isOn = false;
        GameManager.instance.onGameOverCallback += GameOverScreen;
        player.GetComponent<PlayerShoot>().crosshair = crosshair;

        // Enable room UI
        room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (NetworkManager.IsSceneActive(room.RoomScene))
            {
                readyPanel.SetActive(true);
            }
            room.onRoomStatusChanged += UpdateReadyStatus;
            foreach (NetworkRoomPlayer player in room.roomSlots)
            {
                if (player.readyToBegin) room.playersReady++;
            }
            if (room.countdown > 0)
            {
                UpdateReadyStatus(true, room.countdown);
            } else
            {
                UpdateReadyStatus(false);
            }
        }
        player.zombifyPlayer += ZombifyUI;
    }

    private void OnDestroy()
    {
        room.onRoomStatusChanged -= UpdateReadyStatus;
        GameManager.instance.onGameOverCallback -= GameOverScreen;
    }

    void Update()
    {
        // Update UI health and ammo elements
        SetHealth(playerStats.GetHealthPct());
        if (!zombie)
        {
            int bullets = weaponManager.currentWeapon.bullets;
            SetAmmo(Mathf.Min(bullets, inventory.ammo), Mathf.Max(0, inventory.ammo - bullets));   // min updates weapon ammo to inventory ammo after switching, max clamps ammo > 0 
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);

        // Toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
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

    void ZombifyUI()
    {
        zombie = true;
        ammoText.text = "- / -";
        crosshair.gameObject.SetActive(false);
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
        if (pauseMenu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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

    void GameOverScreen(int minutes, string player)
    {
        gameOverText.text = player + " Won!\n\nKills: X\n\nSurvived : " + minutes + " mins";
        gameOverScreen.SetActive(true);
    }

    public void UpdateReadyStatus(bool allReady, int countdown = 60)
    {
        if (allReady)
        {
            readyButton.SetActive(false);
            waitingStatus.gameObject.SetActive(true);
            StartCoroutine(RoyaleCountdown(countdown));
        }
        readyStatus.text = room.playersReady + " / " + room.roomPlayers + " Players Ready";
    }

    IEnumerator RoyaleCountdown(int countdown)
    {
        for (int i = countdown; i > 0; i--)
        {
            waitingStatus.text = "Game starting in " + i;
            yield return new WaitForSeconds(1);
        }
        waitingStatus.text = "Game starting";
    }

    public void ReadyButton()
    {
        roomPlayer.CmdChangeReadyState(true);
        readyButton.SetActive(false);
        waitingStatus.gameObject.SetActive(true);
    }
}
