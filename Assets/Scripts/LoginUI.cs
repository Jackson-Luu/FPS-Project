using UnityEngine;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    GameObject loginPanel;

    [SerializeField]
    GameObject registerPanel;

    [SerializeField]
    TMP_InputField usernameInput;
    [SerializeField]
    TMP_InputField passwordInput;

    public void OnLoginPressed()
    {
        loginPanel.SetActive(true);
    }

    public void OnRegisterPressed()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }
}
