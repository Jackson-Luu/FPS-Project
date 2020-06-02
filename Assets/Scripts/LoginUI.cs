using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    GameObject loginPanel;

    public GameObject registerPanel;

    [SerializeField]
    TMP_InputField usernameInput;
    [SerializeField]
    TMP_InputField passwordInput;

    [SerializeField]
    GameObject buttons;
    [SerializeField]
    TMP_Text statusText;
    [SerializeField]
    GameObject inputFields;

    public Button mainLoginButton;
    public TMP_Text mainLoginText;

    public void OnLoginPressed()
    {
        loginPanel.SetActive(true);
    }

    public void OnLoginSubmitted()
    {
        buttons.SetActive(false);
        StartCoroutine(Login());
        inputFields.SetActive(false);
        statusText.text = "Logging in...";
        statusText.gameObject.SetActive(true);
    }

    public void OnRegisterPressed()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    IEnumerator Login()
    {
        string username = usernameInput.text;
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", passwordInput.text);
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Post("http://ec2-18-237-89-43.us-west-2.compute.amazonaws.com/login/", form);
        yield return request.SendWebRequest();
        if (request.responseCode == 200)
        {
            statusText.text = "<color=green>Login Success!</color>";
            mainLoginButton.interactable = false;
            LoginManager.instance.loggedIn = true;
            LoginManager.instance.user = username;
            yield return new WaitForSeconds(2.0f);
            loginPanel.SetActive(false);
            mainLoginText.text = "Welcome, " + LoginManager.instance.user;
        }
        else
        {
            statusText.text = "<color=red>Login Failed : " + request.downloadHandler.text + "</color>";
            yield return new WaitForSeconds(2.0f);
            statusText.gameObject.SetActive(false);
            inputFields.SetActive(true);
            buttons.SetActive(true);
        }
        usernameInput.text = "";
        passwordInput.text = "";
    }

    public void Close()
    {
        loginPanel.SetActive(false);
    }
}
