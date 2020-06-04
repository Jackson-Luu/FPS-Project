using UnityEngine;
using TMPro;

public class PlayerChat : MonoBehaviour
{
    [SerializeField]
    TMP_InputField chatInput;

    [SerializeField]
    GameObject chatDisplay;

    [SerializeField]
    TMP_Text chatPrefab;

    [HideInInspector]
    public Player player;

    private int messageCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.chatMessageCallback += DisplayMessage;
    }

    private void OnDestroy()
    {
        GameManager.instance.chatMessageCallback -= DisplayMessage;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !GameManager.instance.chatSelected)
        {
            chatInput.interactable = true;
            chatInput.Select();
            GameManager.instance.chatSelected = true;
        }    
    }

    public void SendChatMessage()
    {
        GameManager.instance.chatSelected = false;
        string message = chatInput.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            message = player.username + ": " + message;
            DisplayMessage(message);
            player.CmdChatMessage(message);
        }
        chatInput.text = "";
        chatInput.interactable = false;
    }

    void DisplayMessage(string message)
    {
        if (messageCount > 10)
        {
            GameObject oldestMessage = chatDisplay.transform.GetChild(0).gameObject;
            oldestMessage.GetComponent<TMP_Text>().text = message;
            oldestMessage.transform.SetAsLastSibling();
        } else
        {
            TMP_Text go = Instantiate(chatPrefab, chatDisplay.transform);
            go.text = message;
            messageCount++;
        }
    }
}
