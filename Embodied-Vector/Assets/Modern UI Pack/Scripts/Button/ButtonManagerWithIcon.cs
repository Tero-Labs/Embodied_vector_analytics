using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerWithIcon : MonoBehaviour
    {
        // Content
        public Sprite buttonIcon;
        public string buttonText = "Button";
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Settings
        public bool useCustomContent = false;

        // Variables
        public Image normalIcon;
        public Image highlightedIcon;
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalIcon.sprite = buttonIcon;
                highlightedIcon.sprite = buttonIcon;
                normalText.text = buttonText;
                highlightedText.text = buttonText;
            }

            if (buttonVar == null)
                buttonVar = gameObject.GetComponent<Button>();

            buttonVar.onClick.AddListener(delegate
            {
                buttonEvent.Invoke();
            });
        }

        public void UpdateUI()
        {
            normalIcon.sprite = buttonIcon;
            highlightedIcon.sprite = buttonIcon;
            normalText.text = buttonText;
            highlightedText.text = buttonText;
        }
    }
}