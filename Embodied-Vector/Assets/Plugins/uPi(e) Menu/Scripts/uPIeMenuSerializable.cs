// <copyright file="uPIeMenuSerializable.cs" company="Yearntech">
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
// <summary>Serializable version of uPIeMenu</summary>

using UnityEngine;
using System.Collections.Generic;

namespace uPIe
{
    /// <summary>
    ///     Monobehaviours can't be serialized. This scripts hols all the data
    ///     stored in a uPIeMenu, so it can be serialized. (for upgrade helper tool)
    /// </summary>
    public class uPIeMenuSerializable
    {
        #region general menu settings
        public bool ControlWithGamepad;
        public float ControllerDeadzone;
        public bool UseCustomInputSystem;
        public Vector2 CustomInput;
        public string ConfirmInputName;
        public string HorizontalInputName;
        public string VerticalInputName;
        public int IndicatorGraphicInstanceId = -1;
        public bool ApplyIndicatorRotation;
        public float StartDegOffset;
        public int MenuOptionPrefabId;
        public List<int> MenuOptionsInstanceIds = new List<int>();
        public float CircleSize;
        public bool ConstrainIndicatorPosition;
        public bool KeepSelectedOption;
        public int DefaultSelectedInstanceId;
        public bool DeselectOptionIfOutsideBorders;
        public float AlignRadius;
        public bool AlignRotation;
        public Vector3 AlignUpDirection;
        public Vector3 AlignForwardDirection;
        public bool EnableSelecting;
        public Vector2 CurrentDirection;
        #endregion

        #region debug options
        public bool DoDrawGizmos;
        public bool DrawOnlyOnSelected;
        public float BoundaryLength;
        public bool DebugFoldout;
        #endregion

        #region inspector helper fields
        public bool SetCircleSizeDirectly;
        public int SelectedCircleSizeId;
        public bool ButtonsFoldout;
        public bool UseControllerDeadzoneSlider;
        #endregion
    }
}