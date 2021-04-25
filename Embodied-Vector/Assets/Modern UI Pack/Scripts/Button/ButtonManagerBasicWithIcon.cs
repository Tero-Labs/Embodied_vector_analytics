using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasicWithIcon : MonoBehaviour
    {
        // Content
        public Sprite buttonIcon;
        public string buttonText = "Button";
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Resources
        public Image normalImage;
        public TextMeshProUGUI normalText;

        // Settings
        public bool useCustomContent = false;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalImage.sprite = buttonIcon;
                normalText.text = buttonText;
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
            normalImage.sprite = buttonIcon;
            normalText.text = buttonText;
        }
    }
}