// <copyright file="uPIeMenuInspector.cs" company="Yearntech">
// Copyright (c) 2015, 2016 All Rights Reserved, https://yearntech.wordpress.com/
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Yearn</author>
// <date>2016-07-25</date>
// <summary>Contains the full uPIe menu inspector (see uPIeMenu.cs for main logic code)</summary>

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace uPIe
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(uPIeMenu))]
    public class uPIeMenuInspector : Editor
    {
        private uPIeMenu targetScript;
        private GUIContent[] controlPopupContents = new GUIContent[] { new GUIContent("Gamepad", "Choose this if you want to control with a joystick/ gamepad (using the axis direction)"),
                                                                       new GUIContent("Mouse", "Choose this if you want to control with the mouse/ pointer (using screen position)") };
        private string[] inputSystemPopupOptions = new string[] { "Unity Default", "Custom" };
        private string[] circleSizePopupOptions = new string[] { "Full", "Three-Fourth", "Semi", "Quarter" };

        // Serialized Properties //
        private SerializedProperty controlWithGamepad;
        private SerializedProperty controllerDeadzone;
        private SerializedProperty useCustomInputSystem;
        private SerializedProperty confirmInputName;
        private SerializedProperty horizontalInputName;
        private SerializedProperty verticalInputName;
        private SerializedProperty startDegOffset;
        private SerializedProperty circleSize;
        private SerializedProperty setCircleSizeDirectly;
        private SerializedProperty selectedCircleSizeId;
        private SerializedProperty keepSelectedOption;
        private SerializedProperty defaultSelected;
        private SerializedProperty menuOptionPrefab;
        private SerializedProperty menuOptions;
        private SerializedProperty indicatorGraphic;
        private SerializedProperty applyIndicatorRotation;
        private SerializedProperty constrainIndicatorPosition;
        private SerializedProperty deselectIfOutsideBorders;

        private SerializedProperty alignRadius;
        private SerializedProperty alignRotation;
        private SerializedProperty alignUpDirection;
        private SerializedProperty alignForwardDirection;

        private void OnEnable()
        {
            targetScript = (uPIeMenu)target;

            controllerDeadzone = serializedObject.FindProperty("controllerDeadzone");
            controlWithGamepad = serializedObject.FindProperty("controlWithGamepad");
            useCustomInputSystem = serializedObject.FindProperty("useCustomInputSystem");
            confirmInputName = serializedObject.FindProperty("confirmInputName");
            horizontalInputName = serializedObject.FindProperty("horizontalInputName");
            verticalInputName = serializedObject.FindProperty("verticalInputName");
            startDegOffset = serializedObject.FindProperty("startDegOffset");
            circleSize = serializedObject.FindProperty("circleSize");
            setCircleSizeDirectly = serializedObject.FindProperty("SetCircleSizeDirectly");
            selectedCircleSizeId = serializedObject.FindProperty("SelectedCircleSizeId");
            keepSelectedOption = serializedObject.FindProperty("keepSelectedOption");
            defaultSelected = serializedObject.FindProperty("defaultSelected");
            menuOptionPrefab = serializedObject.FindProperty("menuOptionPrefab");
            menuOptions = serializedObject.FindProperty("menuOptions");
            indicatorGraphic = serializedObject.FindProperty("indicatorGraphic");
            applyIndicatorRotation = serializedObject.FindProperty("applyIndicatorRotation");
            constrainIndicatorPosition = serializedObject.FindProperty("constrainIndicatorPosition");
            deselectIfOutsideBorders = serializedObject.FindProperty("deselectOptionIfOutsideBorders");

            alignRadius = serializedObject.FindProperty("alignRadius");
            alignRotation = serializedObject.FindProperty("alignRotation");
            alignUpDirection = serializedObject.FindProperty("alignUpDirection");
            alignForwardDirection = serializedObject.FindProperty("alignForwardDirection");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Setting up controller type
            var rect = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(rect, new GUIContent("Control With", "Choose wich type of input you want to use"), controlWithGamepad);
            int selectedControllerTypeId = controlWithGamepad.boolValue ? 0 : 1;
            selectedControllerTypeId = EditorGUILayout.Popup(label, selectedControllerTypeId, controlPopupContents);
            controlWithGamepad.boolValue = selectedControllerTypeId <= 0;
            EditorGUI.EndProperty();

            if (controlWithGamepad.boolValue)
            {
                if (defaultSelected.objectReferenceValue != null || !keepSelectedOption.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (targetScript.UseControllerDeadzoneSlider)
                        {
                            rect = new Rect(0, 0, 200, 20);
                            DrawCustomFloatSlider(rect, controllerDeadzone, 0f, 1f, new GUIContent("Controller Deadzone"));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(controllerDeadzone);
                        }

                        if (GUILayout.Button(targetScript.UseControllerDeadzoneSlider ? "by val" : "slider", GUILayout.Height(16)))
                        {
                            targetScript.UseControllerDeadzoneSlider = !targetScript.UseControllerDeadzoneSlider;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
            }

            // Setting up input system type
            int selectedInputSystemTypeId = useCustomInputSystem.boolValue ? 1 : 0;
            selectedInputSystemTypeId = EditorGUILayout.Popup("Input System", selectedInputSystemTypeId, inputSystemPopupOptions);
            useCustomInputSystem.boolValue = selectedInputSystemTypeId > 0;
            if (!useCustomInputSystem.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(confirmInputName, new GUIContent("Confirm Input Name", "The name of the axis in the unity InputManager, this is related to"));
                if (controlWithGamepad.boolValue)
                {
                    EditorGUILayout.PropertyField(horizontalInputName, new GUIContent("Horizontal Input Name", "The name of the axis in the unity InputManager"));
                    EditorGUILayout.PropertyField(verticalInputName, new GUIContent("Vertical Input Name", "The name of the axis in the unity InputManager"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            {
                rect = new Rect(0, 0, 200, 20);
                DrawCustomFloatSlider(rect, startDegOffset, -360f, 360f, new GUIContent("Degrees Offset", "At which offset should the radial menu \"start\"/ where is the first option located?"));
                if (GUILayout.Button("zero", GUILayout.Height(14)))
                {
                    startDegOffset.floatValue = 0;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Setting circle size
            EditorGUILayout.BeginHorizontal();
            {
                if (!setCircleSizeDirectly.boolValue)
                {
                    selectedCircleSizeId.intValue = EditorGUILayout.Popup("Circle Size", selectedCircleSizeId.intValue, circleSizePopupOptions);
                    circleSize.floatValue = (1 - (0.25f * selectedCircleSizeId.intValue)) * 360f;
                    if (GUILayout.Button("Set Value", GUILayout.Height(14)))
                    {
                        setCircleSizeDirectly.boolValue = true;
                    }
                }
                else
                {
                    rect = new Rect(0, 0, 200, 20);
                    DrawCustomFloatSlider(rect, circleSize, 360f, 0f, new GUIContent("Circle size", "In degrees"));
                    if (GUILayout.Button("Choose Type", GUILayout.Height(14)))
                    {
                        setCircleSizeDirectly.boolValue = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (circleSize.floatValue < 360f)
            {
                EditorGUILayout.PropertyField(deselectIfOutsideBorders, new GUIContent("Deselect If Outside Borders"));
            }

            if (controlWithGamepad.boolValue)
            {
                EditorGUILayout.PropertyField(keepSelectedOption, new GUIContent("Keep Selected Option?"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Draw button fields
            EditorGUILayout.LabelField("Menu Options", EditorStyles.boldLabel);
            if (menuOptions.arraySize > 0)
            {
                EditorGUI.indentLevel++;
                targetScript.ButtonsFoldout = EditorGUILayout.Foldout(targetScript.ButtonsFoldout, "Currently Selected: " + (targetScript.SelectedPieceId >= 0 ? ("#" + targetScript.SelectedPieceId) : (targetScript.DefaultSelected == null ? "NONE" : "Default")));
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel = 1;
            }

            if (targetScript.ButtonsFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var firstMenuOptionRect = new Rect();
                    List<IndexedSelectable> menuOptionsToRemove = new List<IndexedSelectable>();
                    EditorGUILayout.BeginVertical();
                    {
                        // Draw default selection
                        if (defaultSelected.objectReferenceValue != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(defaultSelected, new GUIContent("Default Selected", "This selectable is selected by default"));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    // make sure navigation mode is set to none, because this would lead to problems and
                                    // strange behaviour when using gamepad
                                    SetNavigationModeToNone((Selectable)defaultSelected.objectReferenceValue);
                                }

                                if (GUILayout.Button(new GUIContent("-", "Removes the default option"), GUILayout.Width(18), GUILayout.Height(18)))
                                {
                                    if (defaultSelected.objectReferenceValue != null)
                                    {
                                        Undo.DestroyObjectImmediate(((Selectable)defaultSelected.objectReferenceValue).gameObject);
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.Space();
                                var addDefaultButtonRect = GUILayoutUtility.GetRect(100, 32);
                                if (GUI.Button(addDefaultButtonRect, new GUIContent("Add New Default\n(or DRAG & DROP)", "Adds a default option to the menu")))
                                {
                                    defaultSelected.objectReferenceValue = targetScript.AddButton("Default Option");
                                    Undo.RegisterCreatedObjectUndo(((Selectable)defaultSelected.objectReferenceValue).gameObject, "Creating new default uPi Menu option");
                                }

                                MakeDroppable(addDefaultButtonRect, go =>
                                {
                                    var selectable = go.GetComponent<Selectable>();
                                    if (selectable == null) return;

                                    targetScript.DefaultSelected = selectable;
                                }, "Dropped default option");
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(menuOptionPrefab, new GUIContent("Menu Option Prefab", "Drag and drop a prefab object to be used as menu options. Leave empty to use the default buttons"));

                        // List menu options //
                        EditorGUILayout.Space();
                        for (int i = 0; i < menuOptions.arraySize; i++)
                        {
                            if (i == 0)
                            {
                                firstMenuOptionRect = GUILayoutUtility.GetRect(0, 0);
                            }

                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginChangeCheck();
                                var elementProperty = menuOptions.GetArrayElementAtIndex(i);
                                EditorGUILayout.PropertyField(elementProperty, new GUIContent("Menu Option #" + i, "Drag and drop any kind of UnityEngine.UI Selectable"));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    SetNavigationModeToNone((Selectable)elementProperty.objectReferenceValue);
                                }

                                if (GUILayout.Button(new GUIContent("-", "Removes menu option #" + i), GUILayout.Width(22), GUILayout.Height(18)))
                                {
                                    menuOptionsToRemove.Add(new IndexedSelectable(i, (Selectable)elementProperty.objectReferenceValue));
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                    }
                    EditorGUILayout.EndVertical();

                    for (int i = 0; i < menuOptionsToRemove.Count; i++)
                    {
                        var toRemove = menuOptionsToRemove[i];
                        RemoveMenuOption(targetScript, toRemove.Selectable, toRemove.Index);
                    }

                    if (menuOptions.arraySize > 1)
                    {
                        var colorBefore = GUI.color;
                        GUI.color = new Color(0.86f, 0.01f, 0.26f, 1f);
                        firstMenuOptionRect.width = 18;
                        firstMenuOptionRect.height = 19.5f * menuOptions.arraySize;
                        if (GUI.Button(firstMenuOptionRect, new GUIContent("C\nl\ne\na\nr", "Clears all menu options")))
                        {
                            if (EditorUtility.DisplayDialog("Clear All Menu Options", "Are you sure you want to clear all menu options?", "Clear All", "Cancel"))
                            {
                                ClearMenuOptions();
                            }
                        }

                        GUI.color = colorBefore;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel = 0;

            if ((menuOptions.arraySize > 0 && targetScript.ButtonsFoldout) || menuOptions.arraySize <= 0)
            {
                var addMenuOptionRect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
                if (GUI.Button(addMenuOptionRect, new GUIContent("Add New Menu Option\nor\nDRAG & DROP",
                                                    "By default a new UnityUI button is created and linked," +
                                                    " but you can replace the link with any other kind of selectable " +
                                                    "(like Inputfield etc.)\n\n" +
                                                    "If there already are menu options set up, they can just be dragged and dropped here." +
                                                    "\nThis is useful, when upgrading the uPIe package or re-setting-up a menu" +
                                                    "\nHint: make sure the inspector view is locked (small lock icon at top right)")))
                {
                    // insert new button at the end of the menu options list
                    // we need to add a button and add to list here manually - if we would call
                    // AddMenuOption() Undo would just destroy the newly created menu option and leave an
                    // empty item in the menuOptions list
                    GameObject menuOption;
                    var prefab = menuOptionPrefab.objectReferenceValue;
                    if (prefab == null)
                    {
                        menuOption = targetScript.AddButton().gameObject;
                    }
                    else
                    {
                        menuOption = uPIeMenu.CreatePrefabInstance(prefab);
                        targetScript.InitMenuOption(menuOption);
                    }

                    Undo.RegisterCreatedObjectUndo(menuOption, "Creating new uPi(e) Menu option");
                    menuOptions.InsertArrayElementAtIndex(menuOptions.arraySize);
                    var lastMenuOption = menuOptions.GetArrayElementAtIndex(menuOptions.arraySize - 1);
                    lastMenuOption.objectReferenceValue = menuOption.GetComponent<Button>();
                }

                MakeDroppable(addMenuOptionRect, go =>
                {
                    var selectable = go.GetComponent<Selectable>();
                    if (selectable == null)
                    {
                        return;
                    }

                    targetScript.MenuOptions.Add(selectable);
                }, "Dropped menu option");
            }

            // Indicator graphic options
            EditorGUILayout.Space();
            if (indicatorGraphic.objectReferenceValue == null)
            {
                var addIndicatorButtonRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
                if (GUI.Button(addIndicatorButtonRect, "Add New Indicator\n(or DRAG & DROP)"))
                {
                    var indicatorParent = new GameObject();
                    var parentTrans = indicatorParent.AddComponent<RectTransform>();
                    parentTrans.SetParent(targetScript.transform);
                    parentTrans.SetAsFirstSibling();
                    indicatorParent.name = "RadialMenuIndicatorBorder";
                    parentTrans.localScale = Vector3.one;
                    parentTrans.localPosition = Vector3.zero;

                    var indicator = new GameObject();
                    indicator.transform.SetParent(indicatorParent.transform);
                    indicator.transform.localPosition = Vector3.zero;
                    indicator.transform.localScale = Vector3.one;
                    indicator.name = "RadialMenuIndicator";
                    var img = indicator.AddComponent<Image>();

                    // set the property
                    indicatorGraphic.objectReferenceValue = img;
                    Undo.RegisterCreatedObjectUndo(indicatorParent, "Indicator");
                }

                MakeDroppable(addIndicatorButtonRect, go =>
                {
                    var graphic = go.GetComponent<Graphic>();
                    if (graphic == null)
                    {
                        graphic = go.GetComponentInChildren<Graphic>();
                    }

                    if (graphic == null) return;

                    targetScript.IndicatorGraphic = graphic;
                }, "Dropped indicator");
            }
            else
            {
                EditorGUILayout.LabelField("Indicator Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(indicatorGraphic, new GUIContent("Indicator Graphic", "Drag and drop a UnityEngine.UI Graphic objet, that indicates what menu option in currently selected"));
                EditorGUILayout.PropertyField(applyIndicatorRotation, new GUIContent("Apply Indicator Rotation"));
                if (circleSize.floatValue < 360)
                {
                    EditorGUILayout.PropertyField(constrainIndicatorPosition, new GUIContent("Constrain Indicator Position"));
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(new GUIContent("Remove Indicator", "Removes the Indicator Graphic"), GUILayout.Height(16)))
                    {
                        RemoveIndicator();
                    }
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            // Alignment settings
            if (menuOptions.arraySize > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Alignment Helper", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(alignRadius, new GUIContent("Radius"));
                EditorGUILayout.PropertyField(alignRotation);
                EditorGUILayout.PropertyField(alignUpDirection, new GUIContent("Up Direction", "Local direction, which should face upward"));
                EditorGUILayout.PropertyField(alignForwardDirection, new GUIContent("Forward Direction", "Local direction, which should face forward"));

                if (GUILayout.Button("Align Menu Options"))
                {
                    // we need to get all the menu option transforms
                    // to store on undo stack
                    var menuOptns = new List<Object>();
                    for (int i = 0; i < menuOptions.arraySize; i++)
                    {
                        var elementProperty = menuOptions.GetArrayElementAtIndex(i);
                        if (elementProperty.objectReferenceValue == null) continue;

                        menuOptns.Add(((Selectable)elementProperty.objectReferenceValue).transform);
                    }

                    Undo.RecordObjects(menuOptns.ToArray(), "Menu option alignment");
                    targetScript.Realign();
                }

                EditorGUI.indentLevel--;
            }

            // Save and apply changes
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            targetScript.DebugFoldout = EditorGUILayout.Foldout(targetScript.DebugFoldout, "Debug Options");
            if (targetScript.DebugFoldout)
            {
                EditorGUI.indentLevel++;
                targetScript.DoDrawGizmos = EditorGUILayout.Toggle("Draw Gizmos", targetScript.DoDrawGizmos);
                if (targetScript.DoDrawGizmos)
                {
                    EditorGUI.indentLevel++;
                    targetScript.DrawOnlyOnSelected = EditorGUILayout.Toggle("Draw Only On Selected", targetScript.DrawOnlyOnSelected);
                    targetScript.BoundaryLength = EditorGUILayout.FloatField("Boundary Length", targetScript.BoundaryLength);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void RemoveIndicator()
        {
            if (indicatorGraphic.objectReferenceValue == null) return;

            var indicatorComp = (Graphic)indicatorGraphic.objectReferenceValue;
            var indicatorParent = indicatorComp.transform.parent;
            if (indicatorParent != targetScript.transform && indicatorParent != null)
            {
                Undo.DestroyObjectImmediate(indicatorParent.gameObject);
                return;
            }

            Undo.DestroyObjectImmediate(indicatorComp.gameObject);
        }

        private void RemoveMenuOption(uPIeMenu targetScript, Selectable slct, int index = -1)
        {
            if (index < 0)
            {
                index = targetScript.MenuOptions.IndexOf(slct);
            }

            if (slct == null)
            {
                // if we removed a button gameobject we have a missing link here.
                // if trying to remove this would at first turn from "missing" to "none"
                // and only then it would be deleted. To prevent the user having to press the
                // delete button twice we set the link to "none" here manually, so the option
                // is immediately removed from the list
                var tmp = menuOptions.GetArrayElementAtIndex(index);
                tmp.objectReferenceValue = null;
            }

            // Somehow DeleteArrayElementAtIndex does not actually delete
            // the element from the array (like List<T>().RemoveAt(int) would do)
            // but just sets the element at the selected index to null
            menuOptions.DeleteArrayElementAtIndex(index);

            // apply modified now, so und also connects the option
            // back to the selectable we are going to delete
            serializedObject.ApplyModifiedProperties();

            if (slct != null)
            {
                // to actually delete the array element we need to call the delete function again
                // but only call it again if the selectable was not null (else the last element
                // will be deleted instead of the desired one)
                menuOptions.DeleteArrayElementAtIndex(index);
                Undo.DestroyObjectImmediate(slct.gameObject);
            }
        }

        private void MakeDroppable(Rect dropArea, Action<GameObject> action, string undoDescription)
        {
            var evnt = Event.current;
            var colorBefore = GUI.color;
            GUI.color = Color.yellow;
            switch (evnt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!dropArea.Contains(evnt.mousePosition)) return;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (evnt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            var droppedGameObjects = new List<GameObject>();
                            var droppedObjects = DragAndDrop.objectReferences;
                            for (int i = 0; i < droppedObjects.Length; i++)
                            {
                                var droppedGo = (GameObject)droppedObjects[i];
                                if (droppedGo == null) continue;

                                droppedGameObjects.Add(droppedGo);
                            }

                            if (droppedGameObjects.Count > 0)
                            {
                                Undo.RegisterCompleteObjectUndo(targetScript, undoDescription);
                                for (int i = 0; i < droppedGameObjects.Count; i++)
                                {
                                    action(droppedGameObjects[i]);
                                }
                            }
                        }
                    }
                    break;
            }

            GUI.color = colorBefore;
        }

        private void ClearMenuOptions()
        {
            for (int i = 0; i < menuOptions.arraySize; i++)
            {
                var elementProperty = menuOptions.GetArrayElementAtIndex(i);
                if (elementProperty.objectReferenceValue == null) continue;

                Undo.DestroyObjectImmediate(((Selectable)elementProperty.objectReferenceValue).gameObject);
            }

            menuOptions.ClearArray();
            if (defaultSelected.objectReferenceValue != null)
            {
                Undo.DestroyObjectImmediate(((Selectable)defaultSelected.objectReferenceValue).gameObject);
            }
        }

        private void SetNavigationModeToNone(Selectable slc)
        {
            if (slc == null) return;

            var navi = new Navigation();
            navi.mode = Navigation.Mode.None;
            slc.navigation = navi;
        }

        private void DrawCustomFloatSlider(SerializedProperty property, float leftValue, float rightValue, GUIContent contentLabel)
        {
            var rect = EditorGUILayout.GetControlRect();
            DrawCustomFloatSlider(rect, property, leftValue, rightValue, contentLabel);
        }

        private void DrawCustomFloatSlider(Rect rect, SerializedProperty property, float leftValue, float rightValue, GUIContent contentLabel)
        {
            var label = EditorGUI.BeginProperty(rect, contentLabel, property);
            property.floatValue = EditorGUILayout.Slider(label, property.floatValue, leftValue, rightValue);
            EditorGUI.EndProperty();
        }

        private void DrawCustomBoolOptionSelection()
        {
            var rect = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(rect, new GUIContent("Control With", "Choose wich type of input you want to use"), controlWithGamepad);
            int selectedControllerTypeId = controlWithGamepad.boolValue ? 0 : 1;
            selectedControllerTypeId = EditorGUILayout.Popup(label, selectedControllerTypeId, controlPopupContents);
            controlWithGamepad.boolValue = selectedControllerTypeId <= 0;
            EditorGUI.EndProperty();
        }

        private struct IndexedSelectable
        {
            public Selectable Selectable;
            public int Index;

            public IndexedSelectable(int index, Selectable selectable)
            {
                Selectable = selectable;
                Index = index;
            }
        }
    }
}
#endif