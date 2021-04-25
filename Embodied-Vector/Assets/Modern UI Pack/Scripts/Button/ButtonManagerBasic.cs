using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasic : MonoBehaviour
    {
        // Content
        public string buttonText = "Button";
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Resources
        public TextMeshProUGUI normalText;

        // Settings
        public bool useCustomContent = false;

        void Start()
        {
            if (useCustomContent == false)
                normalText.text = buttonText;

            if (buttonVar == null)
                buttonVar = gameObject.GetComponent<Button>();

            buttonVar.onClick.AddListener(delegate
            {
                buttonEvent.Invoke();
            });
        }

        void UpdateUI()
        {
            normalText.text = buttonText;
        }
    }
}