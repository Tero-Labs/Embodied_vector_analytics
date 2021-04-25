using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Michsky.UI.ModernUIPack
{
    [CustomEditor(typeof(ButtonManagerIcon))]
    [System.Serializable]
    public class ButtonManagerIconEditor : Editor
    {
        // Variables
        private ButtonManagerIcon buttonTarget;
        private int currentTab;

        private void OnEnable()
        {
            // Set target
            buttonTarget = (ButtonManagerIcon)target;
        }

        public override void OnInspectorGUI()
        {
            // GUI skin variable
            GUISkin customSkin;

            // Select GUI skin depending on the editor theme
            if (EditorGUIUtility.isProSkin == true)
                customSkin = (GUISkin)Resources.Load("Editor\\Custom Skin Dark");
            else
                customSkin = (GUISkin)Resources.Load("Editor\\Custom Skin Light");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Top Header
            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Button Top Header"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Toolbar content
            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Draw toolbar tabs as a button
            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Toolbar Items")))
                currentTab = 0;

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Toolbar Resources")))
                currentTab = 1;

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Toolbar Settings")))
                currentTab = 2;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Draw toolbar indicators
            currentTab = GUILayout.Toolbar(currentTab, toolbarTabs, customSkin.FindStyle("Toolbar Indicators"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Property variables
            var buttonIcon = serializedObject.FindProperty("buttonIcon");
            var buttonEvent = serializedObject.FindProperty("buttonEvent");
            var normalIcon = serializedObject.FindProperty("normalIcon");
            var highlightedIcon = serializedObject.FindProperty("highlightedIcon");
            var useCustomContent = serializedObject.FindProperty("useCustomContent");

            // Draw content depending on tab index
            switch (currentTab)
            {
                case 0:
                    GUILayout.Label("CONTENT", customSkin.FindStyle("Header"));

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Button Icon"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(buttonIcon, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    if (useCustomContent.boolValue == false && buttonTarget.normalIcon != null)
                    {
                        buttonTarget.normalIcon.sprite = buttonTarget.buttonIcon;
                        buttonTarget.highlightedIcon.sprite = buttonTarget.buttonIcon;
                    }               

                    else if(useCustomContent.boolValue == false && buttonTarget.normalIcon == null)
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.HelpBox("'Icon Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                    }

                    GUILayout.Space(4);
                    EditorGUILayout.PropertyField(buttonEvent, new GUIContent("On Click Event"), true);
                    GUILayout.Space(2);
                    break;

                case 1:
                    GUILayout.Label("RESOURCES", customSkin.FindStyle("Header"));

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Normal Icon"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(normalIcon, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Highlighted Icon"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(highlightedIcon, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.Space(6);
                    break;

                case 2:
                    GUILayout.Label("SETTINGS", customSkin.FindStyle("Header"));

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent("Use Custom Content"), customSkin.FindStyle("Toggle"));
                    useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                    break;            
            }

            // Apply the changes
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif