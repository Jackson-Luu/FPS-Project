using UnityEngine;
using TMPro;

public class PlayerNameplate : MonoBehaviour
{
    // TODO: assign usernames to this variable when login is done
    [SerializeField]
    private TMP_Text usernameText;

    [SerializeField]
    private Player player;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        // Rotate nameplate to face(LookAt) camera of whoever is looking
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }
    }
}
