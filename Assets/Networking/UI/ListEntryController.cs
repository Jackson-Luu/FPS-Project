// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<TMP_Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NodeListServer
{
    public class ListEntryController : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text playersText;
        public TMP_Text latencyText;
        public TMP_Text statusText;
        public Button joinButton;
    }
}