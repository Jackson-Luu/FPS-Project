using UnityEngine;
using TMPro;

public class KillFeedItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text text;

    public void Setup(string player, string source)
    {
        if (source == "Zombie")
        {
            text.text = "<color=red>" + player + "</color> was eaten alive" ;
        } else
        {
            text.text = "<b><color=red>" + source + "</color></b>" + " killed " + " <color=red>" + player + " </color>";
        }
    }
}
