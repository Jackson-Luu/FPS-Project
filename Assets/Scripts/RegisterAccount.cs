using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterAccount : MonoBehaviour
{
    [SerializeField]
    LoginUI loginUI;

    [SerializeField]
    TMP_InputField usernameInput;
    [SerializeField]
    TMP_InputField passwordInput;

    [SerializeField]
    GameObject submitButton;
    [SerializeField]
    TMP_Text statusText;
    [SerializeField]
    GameObject inputFields;

    public void OnRegisterSubmit()
    {
        submitButton.SetActive(false);
        StartCoroutine(Register());
        inputFields.SetActive(false);
        statusText.text = "Creating account...";
        statusText.gameObject.SetActive(true);
    }

    IEnumerator Register()
    {
        string username = usernameInput.text;
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", passwordInput.text);
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Post("http://ec2-18-237-89-43.us-west-2.compute.amazonaws.com/register/", form);
        yield return request.SendWebRequest();
        if (request.responseCode == 200)
        {
            statusText.text = "<color=green>Account created!</color>";
            loginUI.mainLoginButton.interactable = false;
            LoginManager.instance.loggedIn = true;
            LoginManager.instance.user = username;
            yield return new WaitForSeconds(2.0f);
            loginUI.registerPanel.SetActive(false);
            loginUI.mainLoginText.text = "Welcome, " + LoginManager.instance.user;
        }
        else
        {
            statusText.text = "<color=red>Register failed : " + request.downloadHandler.text + "</color>";
            yield return new WaitForSeconds(2.0f);
            statusText.gameObject.SetActive(false);
            inputFields.SetActive(true);
            submitButton.SetActive(true);
        }
        usernameInput.text = "";
        passwordInput.text = "";
    }

    public void Close()
    {
        loginUI.registerPanel.SetActive(false);
    }
}
