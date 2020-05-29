using System.Collections;
using UnityEngine;
using TMPro;

public class RegisterAccount : MonoBehaviour
{
    [SerializeField]
    TMP_InputField usernameInput;
    [SerializeField]
    TMP_InputField passwordInput;

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameInput.text);
        form.AddField("password", passwordInput.text);
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Post("http://localhost/sqlconnect/register.php", form);
        yield return request.SendWebRequest();
        if (request.responseCode == 0)
        {
            Debug.Log("REGISTER SUCCESS");
        }
        else
        {
            Debug.Log("REGISTER FAILED ");
        }
    }

    void VerifyInputs()
    {

    }
}
