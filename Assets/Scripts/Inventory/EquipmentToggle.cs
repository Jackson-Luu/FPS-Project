using UnityEngine;
using TMPro;

public class EquipmentToggle : MonoBehaviour
{
    [SerializeField]
    private TMP_Text toggleText;

    public void ToggleEquipment()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
        {
            toggleText.SetText(">>");
        } else
        {
            toggleText.SetText("<< Gear");
        }
    }
}
