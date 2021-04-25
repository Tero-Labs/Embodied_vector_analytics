// <copyright file="uPIeMenu.cs" company="Yearntech">
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
// <summary>Contains the full uPIe menu logic (see uPIeMenuInspect.cs for inspector code)</summary>

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace uPIe
{
    /// <summary>
    ///     Contains the main uPIe menu (runtime) logic
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [Serializable]
    public class uPIeMenu : MonoBehaviour
    {
        [SerializeField]
        private bool controlWithGamepad;

        [Tooltip("If the controller is not moved far enough the default " +
                 "selectable or simply nothing (see keepSelectedOption) will be selected")]
        [SerializeField]
        private float controllerDeadzone = 0.3f;

        [SerializeField]
        private bool useCustomInputSystem;

        // custom input position, or axis. Depending on
        // if we want to use "gamepad behaviour" or "mouse behaviour"
        // which basically means, taking the direction or the absolute position
        // of the "pointer"
        private Vector2 customInput;
        private bool confirmButtonDown;

        [SerializeField]
        private string confirmInputName = "Fire1";

        [SerializeField]
        private string horizontalInputName = "Horizontal";

        [SerializeField]
        private string verticalInputName = "Vertical";

        [Tooltip("the visual indicator graphic should be attached to a child of this gameobject")]
        [SerializeField]
        private Graphic indicatorGraphic;

        [SerializeField]
        private bool applyIndicatorRotation = true;

        [Tooltip("Where should the first option start? Normally left is 0 degree, so to start at the top we need to add 90 degrees offset")]
        [SerializeField]
        private float startDegOffset = 0;
        private const float standardOffset = 90;

        [SerializeField]
        private GameObject menuOptionPrefab;

        [Tooltip("In clockwise order")]
        [SerializeField]
        private List<Selectable> menuOptions = new List<Selectable>();

        [SerializeField]
        private int selectedPieceId = -1;
        private int prevSelectedPieceId = -1;

        [SerializeField]
        private float circleSize = 360;

        [SerializeField]
        private bool constrainIndicatorPosition = true;

        [Tooltip("If set to true the most recently selected option stays selected - even if the controller is back at \"origin\"")]
        [SerializeField]
        private bool keepSelectedOption = true;

        [SerializeField]
        private Selectable defaultSelected;

        [Tooltip("If the menu is not a full circle, should the \"border\" options get deselected, when not aiming directly at them?")]
        [SerializeField]
        private bool deselectOptionIfOutsideBorders;

        [SerializeField]
        private float alignRadius = 60f;

        [SerializeField]
        private bool alignRotation = true;

        [SerializeField]
        private Vector3 alignUpDirection = Vector3.up;

        [SerializeField]
        private Vector3 alignForwardDirection = Vector3.forward;

        private bool enableSelecting = true;
        private Vector2 currentDirection;
        private Selectable currentlyActiveOption;
        private bool currentlyHoveringWithMouse;

        // \cond
        // to stay version independent. Cannot set this method inside uPIeMenuInspector (
        // even if it just is an editor-only function) because, when compiled the uPIeMenuInspector
        // namespace is not found in Unity 4.6 (for whatever reason...)
        public static Func<UnityEngine.Object, GameObject> CreatePrefabInstance;
        // \endcond

        // DEBUG OPTIONS //
        public bool DoDrawGizmos = true;
        public bool DrawOnlyOnSelected;
        public float BoundaryLength = 1;
        public bool DebugFoldout;

        // ** Fields to store inspector values in ** //
        public bool SetCircleSizeDirectly;
        public int SelectedCircleSizeId;
        public bool ButtonsFoldout = true;
        public bool UseControllerDeadzoneSlider = true;

        /// <summary>
        ///     Gets or sets the controller deadzone. This means, when is the analogue stick
        ///     of the controller considered to be centered.
        /// </summary>
        /// <value>
        ///     The controller deadzone.
        /// </value>
        public float ControllerDeadzone
        {
            get { return controllerDeadzone; }
            set { controllerDeadzone = value; }
        }

        /// <summary>
        ///     Gets or sets a value controlling whether selecting menu options is enabled or not.
        ///     This is mostly used to enable or disable a parent menu when a submenu is
        ///     opened or closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be able to select the options from this menu; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSelecting
        {
            get { return enableSelecting; }
            set { enableSelecting = value; }
        }

        /// <summary>
        ///     Gets or sets the option (button or other selectable) that is selected by default.
        ///     This means this is what is selected, when the controllers analogue stick is centered
        /// </summary>
        public Selectable DefaultSelected
        {
            get { return defaultSelected; }
            set
            {
                defaultSelected = value;
                if (defaultSelected != null)
                {
                    // removing the default unity navigation
                    // because this would result in strange behaviour if controlling with a gamepad
                    var navi = new Navigation();
                    navi.mode = Navigation.Mode.None;
                    defaultSelected.navigation = navi;
                }
            }
        }

        /// <summary>
        ///     If unity default input is selected (no custom input) this is what the horizontal input
        ///     is called in the input manager
        /// </summary>
        public string HorizontalInputName
        {
            get { return horizontalInputName; }
            set { horizontalInputName = value; }
        }

        /// <summary>
        ///     If unity default input is selected (no custom input) this is what the vertical input
        ///     is called in the input manager
        /// </summary>
        public string VerticalInputName
        {
            get { return verticalInputName; }
            set { verticalInputName = value; }
        }

        /// <summary>
        ///     If unity default input is selected (no custom input) this is what the confirm input
        ///     is called in the input manager
        /// </summary>
        public string ConfirmInputName
        {
            get { return confirmInputName; }
            set { confirmInputName = value; }
        }

        /// <summary>
        ///     If unity default input is used this returns, if the confirm button was pressed last frame
        ///     Note: you can set this value too, but this is only recommended, if you want to create your own
        ///           customized version of uPIe
        /// </summary>
        public bool ConfirmButtonDown
        {
            get { return confirmButtonDown; }
            set { confirmButtonDown = value; }
        }

        /// <summary>
        ///     Gets the menu option, that is currently active.
        ///     Note: you can set the value too, but this should only be done, if you
        ///           want to create your own, customized version of uPIe.
        /// </summary>
        public Selectable CurrentlyActiveOption
        {
            get { return currentlyActiveOption; }
            set { currentlyActiveOption = value; }
        }

        /// <summary>
        ///     Gets or sets the value that determines whether to constrain the indicator position to the
        ///     nearest menu option. This only makes a difference, when using menus that are not full circle.
        /// </summary>
        public bool ConstrainIndicatorPosition
        {
            get { return constrainIndicatorPosition; }
            set { constrainIndicatorPosition = value; }
        }

        /// <summary>
        ///     Gets or sets the size of the circle menu.
        /// </summary>
        /// <value>
        ///     The size of the circle.
        /// </value>
        public float CircleSize
        {
            get { return circleSize; }
            set { circleSize = value; }
        }

        /// <summary>
        ///     Gets or sets the the value, that determines whether to keep the most recently selected option
        ///     with gamepad, when the stick is in "origin" position
        /// </summary>
        public bool KeepSelectedOption
        {
            get { return keepSelectedOption; }
            set { keepSelectedOption = value; }
        }

        /// <summary>
        ///     Gets or sets the current direction from center of the menu
        ///     to pointer (mouse or analogue stick direction)
        /// </summary>
        /// <value>
        ///     The current direction.
        /// </value>
        public Vector2 CurrentDirection
        {
            get { return currentDirection; }
            set { currentDirection = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to use a custom input system or the unity default one.
        /// </summary>
        /// <value>
        ///     <c>true</c> if custom input system should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomInputSystem
        {
            get { return useCustomInputSystem; }
            set { useCustomInputSystem = value; }
        }

        /// <summary>
        ///     If we choose to use a custom input system, we need to set the direction (for analogue stick)
        ///     or position (for mouse) here
        /// </summary>
        /// <value>
        /// The custom input.
        /// </value>
        public Vector2 CustomInput
        {
            get { return customInput; }
            set { customInput = value; }
        }

        /// <summary>
        ///     Gets or sets the selected piece identifier.
        /// </summary>
        /// <value>
        ///     The selected piece identifier.
        /// </value>
        public int SelectedPieceId
        {
            get { return selectedPieceId; }
            set { selectedPieceId = value; }
        }

        /// <summary>
        ///     Gets or sets the offset in degrees where to start / where the first menu option should be
        /// </summary>
        /// <value>
        ///     The start offset in degrees
        /// </value>
        public float StartDegOffset
        {
            get { return startDegOffset; }
            set { startDegOffset = value; }
        }

        /// <summary>
        ///     Gets or sets the menu option prefab to use when creating new
        ///     menu options
        /// </summary>
        /// <value>
        ///     The prefab asset to use
        /// </value>
        public GameObject MenuOptionPrefab
        {
            get { return menuOptionPrefab; }
            set { menuOptionPrefab = value; }
        }

        /// <summary>
        ///     Gets or sets the menu options.
        /// </summary>
        /// <value>
        ///     The menu options.
        /// </value>
        public List<Selectable> MenuOptions
        {
            get { return menuOptions; }
            set { menuOptions = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to apply indicator rotation or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if indicator rotation should be applied; otherwise, <c>false</c>.
        /// </value>
        public bool ApplyIndicatorRotation
        {
            get { return applyIndicatorRotation; }
            set { applyIndicatorRotation = value; }
        }

        /// <summary>
        ///     Gets or sets the indicator graphic.
        /// </summary>
        /// <value>
        ///     The indicator graphic.
        /// </value>
        public Graphic IndicatorGraphic
        {
            get { return indicatorGraphic; }
            set { indicatorGraphic = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to control with gamepad or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if controlling with gamepad; otherwise, <c>false</c>.
        /// </value>
        public bool ControlWithGamepad
        {
            get { return controlWithGamepad; }
            set { controlWithGamepad = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to deselect option if outside the menu borders.
        ///     Note: only makes a difference when using menus that are not full circle
        /// </summary>
        /// <value>
        ///     <c>true</c> if the menu option should be deselected when outside the menus borders; otherwise, <c>false</c>.
        /// </value>
        public bool DeselectOptionIfOutsideBorders
        {
            get { return deselectOptionIfOutsideBorders; }
            set { deselectOptionIfOutsideBorders = value; }
        }

        /// <summary>
        ///     Gets or sets the align radius (align helper in the inspector)
        /// </summary>
        /// <value>
        ///     The align radius.
        /// </value>
        public float AlignRadius
        {
            get { return alignRadius; }
            set { alignRadius = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the align helper (inspector) should also align rotation or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if menu option rotation should be aligned; otherwise, <c>false</c>.
        /// </value>
        public bool AlignRotation
        {
            get { return alignRotation; }
            set { alignRotation = value; }
        }

        /// <summary>
        ///     Gets or sets the up direction used for alignment.
        /// </summary>
        /// <value>
        ///     The align up direction.
        /// </value>
        public Vector3 AlignUpDirection
        {
            get { return alignUpDirection; }
            set { alignUpDirection = value; }
        }

        /// <summary>
        ///     Gets or sets the forward direction used for alignment.
        /// </summary>
        /// <value>
        ///     The align forward direction.
        /// </value>
        public Vector3 AlignForwardDirection
        {
            get { return alignForwardDirection; }
            set { alignForwardDirection = value; }
        }

        /// <summary>
        ///     Gets the current direction from the center of the menu to the pointer (analogue stick direction or mouse position).
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDirection()
        {
            var dir = Vector2.zero;

            // if we are not using a gamepad, we assume
            // we are using a mouse
            if (!controlWithGamepad)
            {
                // use custom input is already handled inside GetMousePosition
                Vector2 cursorPosition = GetMousePosition();
                dir = cursorPosition - new Vector2(transform.position.x, transform.position.y);
            }
            else
            {
                dir = new Vector2(CustomInput.x, CustomInput.y);
            }

            return dir.normalized;
        }

        /// <summary>
        ///     Polls the input.
        /// </summary>
        public void PollInput()
        {
            if (!useCustomInputSystem)
            {
                ConfirmButtonDown = Input.GetButtonDown(ConfirmInputName);

                if (ControlWithGamepad)
                {
                    CustomInput = new Vector2(Input.GetAxisRaw(HorizontalInputName), Input.GetAxisRaw(VerticalInputName));
                }
            }
        }

        /// <summary>
        ///     Gets the indicator position.
        /// </summary>
        /// <param name="resultPosition">The resulting position.</param>
        /// <returns></returns>
        public Vector2 GetIndicatorPosition(out Vector2 resultPosition)
        {
            var dir = GetDirection();
            return GetIndicatorPosition(dir, out resultPosition);
        }

        /// <summary>
        ///     Gets the indicator position.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <param name="resultPosition">The resulting position.</param>
        /// <returns></returns>
        public Vector2 GetIndicatorPosition(Vector2 dir, out Vector2 resultPosition)
        {
            if (IndicatorGraphic == null)
            {
                resultPosition = new Vector2();
                return Vector2.zero;
            }

            var myRectTrans = (RectTransform)IndicatorGraphic.transform.parent;
            if (myRectTrans == null)
            {
                myRectTrans = (RectTransform)transform;
            }

            Vector2 constrainedDir = Vector2.zero;
            if (ConstrainIndicatorPosition && CircleSize < 360)
            {
                var startDir = GetStartDirection();
                var endDir = GetEndDirection();
                var angleFromStart = GetSignedAngle(startDir, dir);
                var angleFromEnd = GetSignedAngle(endDir, dir);
                if (angleFromStart < 0)
                {
                    angleFromStart += 360;
                }

                var deadAngle = 360 - CircleSize;
                var midDir = Quaternion.Euler(0, 0, deadAngle / 2) * startDir;
                var angleToMid = GetSignedAngle(midDir, dir);
                if (angleFromStart > CircleSize && angleToMid > 0)
                {
                    constrainedDir = dir = startDir;
                }
                else if (angleFromEnd > 0 && angleToMid < 0)
                {
                    constrainedDir = dir = endDir;
                }
            }

            Vector2 elipticRadius = new Vector2(myRectTrans.sizeDelta.x * myRectTrans.pivot.x, myRectTrans.sizeDelta.y * myRectTrans.pivot.y);
            resultPosition = Vector2.Scale(dir, elipticRadius);
            return constrainedDir;
        }

        /// <summary>
        ///     Confirms the current selection (simulates a click respectively button down on the currently selected
        ///     menu option).
        /// </summary>
        public void ConfirmCurrentSelection()
        {
            if (CurrentlyActiveOption == null) return;

            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(CurrentlyActiveOption.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        /// <summary>
        ///     Gets the signed angle between two directions.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        private float GetSignedAngle(Vector3 from, Vector3 to)
        {
            return Mathf.DeltaAngle(Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg,
                                    Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg);
        }

        /// <summary>
        ///     Gets the mouse position.
        /// </summary>
        /// <returns></returns>
        private Vector2 GetMousePosition()
        {
            var root = transform.root;
            var canvas = root.GetComponent<Canvas>();
            if (canvas == null) return Vector2.zero;

            // maybe we don't wat the use the unity mouse,
            // but our own custom mouse cursor system or the like. In that
            // cas we use the value that has being set in the appropriate field
            Vector2 pointerPosition = UseCustomInputSystem ? CustomInput : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return pointerPosition;
            }

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root as RectTransform, pointerPosition, canvas.worldCamera, out pos);
            return root.TransformPoint(pos);
        }

        /// <summary>
        ///     Gets the angle from a direction by offset and clamps between 0 and 360.
        ///     (starts at 0 again, if bigger than 360)
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <param name="degOffset">The offset in degrees.</param>
        /// <param name="zeroTo360">if set to <c>true</c> the resulting angle will not get bigger than 360°.</param>
        /// <returns></returns>
        private float GetAngle(Vector2 dir, float degOffset = 0, bool zeroTo360 = false)
        {
            var offsettedDir = GetOffsettedDir(dir, degOffset);
            var atan = Mathf.Atan2(offsettedDir.y, offsettedDir.x);
            var degAngle = atan * Mathf.Rad2Deg;
            if (zeroTo360)
            {
                degAngle -= 180;
                degAngle = Mathf.Abs(degAngle);
            }

            return degAngle;
        }

        /// <summary>
        ///     Gets the offsetted direction.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        private Vector2 GetOffsettedDir(Vector2 dir, float offset)
        {
            return Quaternion.Euler(0, 0, offset + standardOffset) * dir;
        }

        /// <summary>
        ///     Gets the piece (menu option) by angle.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        private int GetPieceByAngle(float angle)
        {
            if (menuOptions.Count <= 0) return -1;

            angle = Mathf.Clamp(angle, 0, CircleSize);
            var anglePerPiece = CircleSize / menuOptions.Count;
            var selectedPieceId = (int)(angle / anglePerPiece);
            selectedPieceId = selectedPieceId >= menuOptions.Count ? -1 : selectedPieceId;
            return selectedPieceId;
        }

        /// <summary>
        ///     Adds the menu option callback.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        private void AddMenuOptionCallback(Selectable target)
        {
            var trigger = GetOrAdduPieEventTrigger(target);
            trigger.SubmitEvent += (e) =>
            {
                var btn = trigger.gameObject.GetComponent<Button>();
                if (btn == null) return;

                var index = MenuOptions.IndexOf(btn);
                if (index == SelectedPieceId)
                {
                    btn.Select();
                }
            };

            trigger.PointerEnterEvent += (e) =>
            {
                currentlyHoveringWithMouse = true;
            };

            trigger.PointerExitEvent += (e) =>
            {
                currentlyHoveringWithMouse = false;
            };
        }

        /// <summary>
        ///     Adds the default selectable callback.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        private void AddDefaultSelectableCallback(Selectable target)
        {
            var trigger = GetOrAdduPieEventTrigger(target);

            // On Select default
            trigger.PointerEnterEvent += (e) =>
                {
                    if (DefaultSelected != null)
                    {
                        CurrentlyActiveOption = DefaultSelected;
                        DefaultSelected.Select();
                        SelectedPieceId = -1;
                        EnableSelecting = false;
                    }
                };

            // On Deselect default
            trigger.PointerExitEvent += (e) => EnableSelecting = true;
        }

        /// <summary>
        ///     Creates the callbacks.
        /// </summary>
        private void CreateCallbacks()
        {
            for (int i = 0; i < MenuOptions.Count; i++)
            {
                if (MenuOptions[i] == null) continue;

                AddMenuOptionCallback(MenuOptions[i]);
            }

            if (DefaultSelected != null)
            {
                AddDefaultSelectableCallback(DefaultSelected);
            }
        }

        /// <summary>
        ///     Selects the related option. The id stored in the field <see cref="SelectedPieceId"/>
        ///     is used.
        /// </summary>
        /// <returns></returns>
        public Selectable SelectRelatedOption()
        {
            return SelectRelatedOption(SelectedPieceId);
        }

        /// <summary>
        ///     Selects the related option by a given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Selectable SelectRelatedOption(int id)
        {
            try
            {
                if (id >= menuOptions.Count || id < 0) return null;

                var selectedPieceSelectable = menuOptions[id];
                if (selectedPieceSelectable == null) return null;

                selectedPieceSelectable.Select();
                return selectedPieceSelectable;
            }
            catch (Exception e)
            {
                Debug.LogWarning("uPI(e) Menu could not select an option. Maybe the EventSystem is deactivated?\nError message: " + e.Message);
                return null;
            }
        }

        /// <summary>
        ///     Removes the indicator.
        /// </summary>
        public void RemoveIndicator()
        {
            if (IndicatorGraphic == null) return;

            var indicatorParent = indicatorGraphic.transform.parent;
            if (indicatorParent != transform && indicatorParent != null)
            {
                DestroyImmediate(indicatorParent.gameObject);
                return;
            }

            DestroyImmediate(IndicatorGraphic.gameObject);
        }

        /// <summary>
        ///     Gets the start direction.
        /// </summary>
        /// <param name="additionalOffset">The additional offset.</param>
        /// <returns></returns>
        public Vector3 GetStartDirection(float additionalOffset = 0)
        {
            return (Quaternion.Euler(0, 0, standardOffset - startDegOffset - additionalOffset) * new Vector3(1, 0, 0)).normalized;
        }

        /// <summary>
        ///     Gets the end direction.
        /// </summary>
        /// <param name="startDir">The start dir.</param>
        /// <returns></returns>
        public Vector3 GetEndDirection(Vector3 startDir)
        {
            return Quaternion.Euler(0, 0, -CircleSize) * startDir;
        }

        /// <summary>
        ///     Gets the end direction.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEndDirection()
        {
            return Quaternion.Euler(0, 0, -CircleSize) * GetStartDirection();
        }

        /// <summary>
        ///     Adds a new button (without adding it to the menu option list).
        ///     In most cases you should use <see cref="AddMenuOption"/> as
        ///     this sets up the button correctly and adds it to the menu options list
        /// </summary>
        /// <param name="name">A name for the button.</param>
        /// <param name="tryCopyFromLastMenuOption">
        ///     If set to true (default) this method will try to copy
        ///     the most recently added menu option button
        /// </param>
        /// <returns></returns>
        public Button AddButton(string name = "", bool tryCopyFromLastMenuOption = true)
        {
            GameObject button = null;
            if (MenuOptionPrefab != null)
            {
                button = (GameObject)Instantiate(MenuOptionPrefab, Vector3.zero, Quaternion.identity);
            }
            else if (tryCopyFromLastMenuOption)
            {
                if (MenuOptions.Count > 0)
                {
                    var mostRecent = MenuOptions[MenuOptions.Count - 1];
                    if (mostRecent != null)
                    {
                        // we need this overload, because the overload that just takes a GameObject as parameter
                        // does not work with unity 4.6
                        button = (GameObject)Instantiate(mostRecent.gameObject, Vector3.zero, Quaternion.identity);
                    }
                }
            }

            if (button == null)
            {
                button = new GameObject();
                var img = button.AddComponent<Image>();
                var btn = button.AddComponent<Button>();
                var navi = new Navigation();
                navi.mode = Navigation.Mode.None;
                btn.navigation = navi;
                btn.image = img;
            }

            InitMenuOption(button, name);
            return button.GetComponent<Button>();
        }

        /// <summary>
        ///     Initialize a newly created menu option gameobject instance
        /// </summary>
        /// <param name="instance">The gameobject instance you want to be treated as a menu option</param>
        /// <param name="name">Name the menu option gameobject (optional)</param>
        public void InitMenuOption(GameObject instance, string name = "")
        {
            var scaleBefore = instance.transform.localScale;
            instance.transform.SetParent(transform);
            instance.transform.SetAsLastSibling();
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = scaleBefore;
            instance.name = string.IsNullOrEmpty(name) ? "uPIeMenuOptionButton#" + MenuOptions.Count : name;
            var btn = instance.GetComponent<Selectable>();

            // if the prefab (and therefore the newly created instance) has
            // no selectable attached add a button as default
            if (btn == null)
            {
                btn = instance.AddComponent<Button>();
            }

            // only necessary if a new button is added during runtime
            AddMenuOptionCallback(btn);
        }

        /// <summary>
        ///     Adds a new menu option (button) to the menu.
        /// </summary>
        /// <returns>The newly added menu option button</returns>
        public Button AddMenuOption()
        {
            var btn = AddButton();
            MenuOptions.Add(btn);
            return btn;
        }

        /// <summary>
        ///     <see cref="AddMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void AddMenuOptionAndRescaleX()
        {
            AddMenuOptionAndRealign();
        }

        /// <summary>
        ///     <see cref="AddMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void AddMenuOptionAndRescaleY()
        {
            AddMenuOptionAndRealign(false, true);
        }

        /// <summary>
        ///     <see cref="AddMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void AddMenuOptionAndRescaleZ()
        {
            AddMenuOptionAndRealign(false, false, true);
        }

        /// <summary>
        ///     Adds a new menu option (button) to the menu and automatically realigns the menu options
        ///     according to what is set up in the alignment options.
        ///     See <see cref="AlignRadius"/>, <see cref="AlignRotation"/>, <see cref="AlignUpDirection"/>, <see cref="AlignForwardDirection"/>
        /// </summary>
        /// <param name="autoRescaleX">if set to <c>true</c> automatically rescales the x value.</param>
        /// <param name="autoRescaleY">if set to <c>true</c> automatically rescales the y value.</param>
        /// <param name="autoRescaleZ">if set to <c>true</c> automatically rescales the z value.</param>
        /// <returns>The newly added menu option button</returns>
        public Button AddMenuOptionAndRealign(bool autoRescaleX = true, bool autoRescaleY = false, bool autoRescaleZ = false)
        {
            var addedButton = AddMenuOption();

            // apply previous scale
            if (MenuOptions.Count > 1)
            {
                var previousMenuOption = MenuOptions[MenuOptions.Count - 2];
                if (previousMenuOption != null)
                {
                    addedButton.transform.localScale = previousMenuOption.transform.localScale;
                }
            }

            var ratio = GetMenuOptionScaleRatio(true);
            Realign();
            RescaleMenuOptions(autoRescaleX ? ratio : 1, autoRescaleY ? ratio : 1, autoRescaleZ ? ratio : 1);
            return addedButton;
        }

        /// <summary>
        ///     <see cref="RemoveMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void RemoveMenuOptionAndRescaleX()
        {
            RemoveMenuOptionAndRealign();
        }

        /// <summary>
        ///     <see cref="RemoveMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void RemoveMenuOptionAndRescaleY()
        {
            RemoveMenuOptionAndRealign(false, true);
        }

        /// <summary>
        ///     <see cref="RemoveMenuOptionAndRealign"/> takes more options than this, so from a script
        ///     you should probably use that method. But unity only allows one parameter when calling
        ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
        ///     do that, use this method.
        /// </summary>
        public void RemoveMenuOptionAndRescaleZ()
        {
            RemoveMenuOptionAndRealign(false, false, true);
        }

        /// <summary>
        ///     Removes the last menu option (button) from the menu and automatically realigns the menu options
        ///     according to what is set up in the alignment options.
        ///     See <see cref="AlignRadius"/>, <see cref="AlignRotation"/>, <see cref="AlignUpDirection"/>, <see cref="AlignForwardDirection"/>
        /// </summary>
        /// <param name="autoRescaleX">if set to <c>true</c> automatically rescales the x value.</param>
        /// <param name="autoRescaleY">if set to <c>true</c> automatically rescales the y value.</param>
        /// <param name="autoRescaleZ">if set to <c>true</c> automatically rescales the z value.</param>
        /// <returns>The newly added menu option button</returns>
        public void RemoveMenuOptionAndRealign(bool autoRescaleX = true, bool autoRescaleY = false, bool autoRescaleZ = false)
        {
            RemoveMenuOption();
            var ratio = GetMenuOptionScaleRatio(false);
            Realign();
            RescaleMenuOptions(autoRescaleX ? ratio : 1, autoRescaleY ? ratio : 1, autoRescaleZ ? ratio : 1);
        }

        /// <summary>
        ///     Removes the most recently added menu option.
        /// </summary>
        public void RemoveMenuOption()
        {
            RemoveMenuOption(MenuOptions.Count - 1);
        }

        /// <summary>
        ///     Removes the menu option by a given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveMenuOption(int id)
        {
            if (id < 0 || id >= MenuOptions.Count) return;

            RemoveMenuOption(MenuOptions[id]);
        }

        /// <summary>
        ///     Removes the menu option by a given selectable.
        /// </summary>
        /// <param name="slct">The selectable to remove.</param>
        public void RemoveMenuOption(Selectable slct)
        {
            if (slct == null) return;

            DestroyImmediate(slct.gameObject);
            MenuOptions.Remove(slct);
        }

        /// <summary>
        ///     Clears all menu options.
        /// </summary>
        public void ClearMenuOptions()
        {
            foreach (var menuOption in MenuOptions)
            {
                if (menuOption == null) continue;

                DestroyImmediate(menuOption.gameObject);
            }

            if (DefaultSelected != null)
            {
                DestroyImmediate(DefaultSelected.gameObject);
            }

            MenuOptions.Clear();
        }

        /// <summary>
        ///     Deselects the currently selected menu option.
        /// </summary>
        public void Deselect()
        {
            var pointer = new PointerEventData(EventSystem.current);
            for (int i = 0; i < MenuOptions.Count; i++)
            {
                if (MenuOptions[i] == null) continue;

                ExecuteEvents.Execute(MenuOptions[i].gameObject, pointer, ExecuteEvents.deselectHandler);
            }

            CurrentlyActiveOption = null;
            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     Opens a sub-uPIe-menu.
        /// </summary>
        /// <param name="subMenu">The sub menu.</param>
        public void OpenSubMenu(uPIeMenu subMenu)
        {
            EnableSelecting = false;
            subMenu.gameObject.SetActive(true);
            subMenu.enabled = true;
            subMenu.EnableSelecting = true;
            for (int i = 0; i < MenuOptions.Count; i++)
            {
                if (MenuOptions[i] == null) continue;

                MenuOptions[i].interactable = false;
            }
        }

        /// <summary>
        ///     Closes this sub-uPIe-menu and retuns to the uPIe-menu that
        ///     is superordinated to this one.
        /// </summary>
        /// <param name="superMenu">The super menu.</param>
        public void ReturnToSuperMenu(uPIeMenu superMenu)
        {
            gameObject.SetActive(false);
            superMenu.EnableSelecting = true;
            superMenu.enabled = true;
            superMenu.gameObject.SetActive(true);
            for (int i = 0; i < superMenu.MenuOptions.Count; i++)
            {
                if (superMenu.MenuOptions[i] == null) continue;

                superMenu.MenuOptions[i].interactable = true;
            }
        }

        /// <summary>
        ///     Realigns all menu options.
        ///     The options set up as alignment options are used.
        ///     See <see cref="AlignRadius"/>, <see cref="AlignRotation"/>, <see cref="AlignUpDirection"/>, <see cref="AlignForwardDirection"/>
        /// </summary>
        public void Realign()
        {
            Realign(AlignRadius, AlignRotation, AlignUpDirection, AlignForwardDirection);
        }

        /// <summary>
        ///     Realigns all menu options by only using a different radius as set up in the alignment options.
        ///     See <see cref="AlignRadius"/>, <see cref="AlignRotation"/>, <see cref="AlignUpDirection"/>, <see cref="AlignForwardDirection"/>
        /// </summary>
        /// <param name="radius">The radius to align all options along.</param>
        public void Realign(float radius)
        {
            Realign(radius, AlignRotation, AlignUpDirection, alignForwardDirection);
        }

        /// <summary>
        ///     Realigns all menu options by given options
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="doAlignRotation">if set to <c>true</c> the menu options are also rotated.</param>
        /// <param name="upDirection">Local up direction of the menu options.</param>
        /// <param name="forwardDirection">Local forward direction of the menu options.</param>
        public void Realign(float radius, bool doAlignRotation, Vector3 upDirection, Vector3 forwardDirection)
        {
            var rectTrans = (RectTransform)transform;
            var anglePerPiece = CircleSize / MenuOptions.Count;
            var startDir = GetStartDirection(anglePerPiece / 2f);
            for (int i = 0; i < MenuOptions.Count; i++)
            {
                var pieceStartDir = Quaternion.Euler(0, 0, -anglePerPiece * i) * startDir;
                var direction = rectTrans.TransformDirection(pieceStartDir);
                var menuOption = MenuOptions[i];
                var menuOptionTransform = menuOption.transform;
                menuOptionTransform.position = rectTrans.position + (direction * radius);

                if (doAlignRotation)
                {
                    menuOptionTransform.rotation = Quaternion.LookRotation(rectTrans.TransformDirection(forwardDirection), direction.normalized);
                    menuOptionTransform.rotation = Quaternion.LookRotation(menuOptionTransform.forward, menuOptionTransform.TransformDirection(upDirection));
                }
            }
        }

        /// <summary>
        ///     Rescales all menu options.
        /// </summary>
        /// <param name="xScale">The x scale.</param>
        /// <param name="yScale">The y scale.</param>
        /// <param name="zScale">The z scale.</param>
        /// <param name="multiply">
        ///     if set to <c>true</c> the scale is multiplied by its current scale. If set to <c>false</c>
        ///     the scale value is directly set.
        /// </param>
        public void RescaleMenuOptions(float xScale, float yScale = 1, float zScale = 1, bool multiply = true)
        {
            for (int i = 0; i < MenuOptions.Count; i++)
            {
                var rectTrans = (RectTransform)MenuOptions[i].transform;
                var scale = rectTrans.localScale;
                if (multiply)
                {
                    scale.x *= xScale;
                    scale.y *= yScale;
                    scale.z *= zScale;
                }
                else
                {
                    scale.x = xScale;
                    scale.y = yScale;
                    scale.z = zScale;
                }

                rectTrans.localScale = scale;
            }
        }

        private void Awake()
        {
            CreateCallbacks();
        }

        private void Update()
        {
            if (!EnableSelecting && !ControlWithGamepad) return;

            PollInput();
            currentDirection = GetDirection();

            // Set default with gamepad
            if (ControlWithGamepad)
            {
                float curDirMag = CustomInput.sqrMagnitude;
                if (curDirMag < ControllerDeadzone)
                {
                    if (DefaultSelected != null || !keepSelectedOption)
                    {
                        if (DefaultSelected != null)
                        {
                            if (CurrentlyActiveOption != DefaultSelected)
                            {
                                CurrentlyActiveOption = DefaultSelected;
                                DefaultSelected.Select();
                            }
                        }

                        if (!keepSelectedOption)
                        {
                            Deselect();
                            CurrentlyActiveOption = null;
                        }

                        prevSelectedPieceId = -1;
                        SelectedPieceId = -1;
                        if (IndicatorGraphic != null)
                        {
                            IndicatorGraphic.enabled = false;
                        }
                    }

                    return;
                }
                else
                {
                    if (IndicatorGraphic != null)
                    {
                        if (!IndicatorGraphic.enabled)
                        {
                            IndicatorGraphic.enabled = true;
                        }
                    }
                }
            }

            var angle = GetAngle(currentDirection, StartDegOffset, true);
            SelectedPieceId = GetPieceByAngle(angle);
            if (SelectedPieceId != prevSelectedPieceId)
            {
                if (SelectedPieceId >= 0)
                {
                    CurrentlyActiveOption = SelectRelatedOption();
                }
                else
                {
                    if (DeselectOptionIfOutsideBorders && CircleSize < 360f)
                    {
                        Deselect();
                    }
                }
            }

            prevSelectedPieceId = SelectedPieceId;
            if (ConfirmButtonDown)
            {
                if (CurrentlyActiveOption != null && !currentlyHoveringWithMouse)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    var pointer = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(CurrentlyActiveOption.gameObject, pointer, ExecuteEvents.submitHandler);
                }

                ConfirmButtonDown = false;
            }

            // Indicator draw stuff
            if (IndicatorGraphic == null) return;

            Vector2 resultPosition;
            Vector2 constrainedDir = GetIndicatorPosition(currentDirection, out resultPosition);
            if (ApplyIndicatorRotation)
            {
                if (constrainedDir != Vector2.zero)
                {
                    indicatorGraphic.transform.right = constrainedDir;
                }
                else
                {
                    IndicatorGraphic.transform.right = currentDirection;
                }
            }

            IndicatorGraphic.transform.localPosition = resultPosition;
        }

        private float GetMenuOptionScaleRatio(bool scaleBigger)
        {
            if (MenuOptions.Count > 1)
            {
                var val = MenuOptions.Count / (MenuOptions.Count - 1f);
                return scaleBigger ? 1f / val : val;
            }

            return 1;
        }

        private uPIeEventTrigger GetOrAdduPieEventTrigger(Selectable target)
        {
            uPIeEventTrigger trigger = target.GetComponent<uPIeEventTrigger>();
            if (trigger == null)
            {
                trigger = target.gameObject.AddComponent<uPIeEventTrigger>();
            }

            return trigger;
        }

        private void OnDrawGizmos()
        {
            if (!DoDrawGizmos) return;
            if (DrawOnlyOnSelected) return;

            DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (!DoDrawGizmos) return;
            if (!DrawOnlyOnSelected) return;

            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Vector3 startDir = GetStartDirection();
            var rectTrans = (RectTransform)transform;
            var maxSize = Mathf.Max(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y);
            var deltaSize = new Vector3(maxSize, maxSize, 0);

            // Draw parts/ pieces
            var anglePerPiece = CircleSize / menuOptions.Count;
            Gizmos.color = Color.black;
            for (int i = 0; i < menuOptions.Count; i++)
            {
                var pieceStartDir = Quaternion.Euler(0, 0, -anglePerPiece * i) * startDir;
                Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector2.Scale(pieceStartDir, deltaSize)) * BoundaryLength);
            }

            // draw end line
            Gizmos.color = Color.yellow;
            Vector3 endDir = GetEndDirection(startDir);
            Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector2.Scale(endDir, deltaSize)) * BoundaryLength);

            // Draw start line. At last, so it is drawn above all other stuff
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector2.Scale(startDir, deltaSize)) * BoundaryLength);
        }
    }
}