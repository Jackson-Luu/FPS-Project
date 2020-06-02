using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager instance;

    [HideInInspector]
    public string user = null;
    [HideInInspector]
    public bool loggedIn = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
