// <copyright file="uPIeEventTrigger.cs" company="Yearntech">
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
// <summary>
//      Handles pointer enter / exit and submit events
//      (see uPIeMenu.cs for main logic code)
// </summary>

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uPIe
{
    /// <summary>
    ///     This handles pointer events
    /// </summary>
    public class uPIeEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler
    {
        /// <summary>
        ///     Pointer enter event
        /// </summary>
        public event Action<PointerEventData> PointerEnterEvent;

        /// <summary>
        ///     Pointer exit event
        /// </summary>
        public event Action<PointerEventData> PointerExitEvent;

        /// <summary>
        ///     Submit event
        /// </summary>
        public event Action<BaseEventData> SubmitEvent;

        /// <summary>
        ///     Called back on pointer enter
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PointerEnterEvent == null) return;

            PointerEnterEvent(eventData);
        }

        /// <summary>
        ///     Called back on pointer exit
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (PointerExitEvent == null) return;

            PointerExitEvent(eventData);
        }

        /// <summary>
        ///     Called back on submit
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSubmit(BaseEventData eventData)
        {
            if (SubmitEvent == null) return;

            SubmitEvent(eventData);
        }
    }
}