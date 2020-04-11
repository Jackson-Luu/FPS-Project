using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private PlayerController controller;

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    public GameObject deathScreen;

    public void SetController (PlayerController c)
    {
        controller = c;
    }

    void Start()
    {
        PauseMenu.isOn = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
            Cursor.lockState = CursorLockMode.None;
        }    
    }

    void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
    }
}
