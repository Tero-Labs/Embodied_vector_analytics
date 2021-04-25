using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasicIcon : MonoBehaviour
    {
        // Content
        public Sprite buttonIcon;
        public UnityEvent buttonEvent;
        Button buttonVar;

        // Resources
        public Image normalIcon;

        // Settings
        public bool useCustomContent = false;


        void Start()
        {
            if (useCustomContent == false)
                normalIcon.sprite = buttonIcon;
        
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
        }
    }
}