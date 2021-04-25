using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerIcon : MonoBehaviour
    {
        // Content
        public Sprite buttonIcon;
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Resources
        public Image normalIcon;
        public Image highlightedIcon;

        // Settings
        public bool useCustomContent = false;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalIcon.sprite = buttonIcon;
                highlightedIcon.sprite = buttonIcon;
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
        }
    }
}