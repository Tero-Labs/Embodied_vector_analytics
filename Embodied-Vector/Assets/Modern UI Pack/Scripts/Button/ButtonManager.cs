using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManager : MonoBehaviour
    {
        // Content
        public string buttonText = "Button";
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Resources
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;

        // Settings
        public bool useCustomContent = false;

        void Start()
        {
            if (useCustomContent == false && normalText == null && highlightedText == null)
                UpdateUI();

            if (buttonVar == null)
                buttonVar = gameObject.GetComponent<Button>();

            buttonVar.onClick.AddListener(delegate
            {
                buttonEvent.Invoke();
            });
        }

        public void UpdateUI()
        {
            normalText.text = buttonText;
            highlightedText.text = buttonText;
        }
    }
}