// <copyright file="AutomaticUPIeVersionSetter.cs" company="Yearntech">
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
//      This is just needed to stay version independent and
//      backwards compatible (editor only)
//      (see uPIeMenu.cs for main logic code)
// </summary>

#if UNITY_EDITOR
#pragma warning disable 162
#pragma warning disable 414

using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace uPIe
{
    [InitializeOnLoad]
    public class AutomaticUPIeVersionSetter
    {
        // If you don't want this tool to check
        private const bool enabled = true;
        private const bool keepQuiet = true;
        private static readonly bool alreadyRun;

        private static readonly string infoMessage = "If you don't want this message to show up anymore set the field \"enabled\" in AutomaticUPIeVersionSetter.cs to false. " +
                                                     "Make sure to set it back to true, when switching to an other unity version (from 4.6 to 5.X or vice versa)";

        static AutomaticUPIeVersionSetter()
        {
            if (alreadyRun) return;
            if (!enabled) return;

#if UNITY_4_6
        if (!keepQuiet)
        {
            Debug.LogWarning("<color=orange>uPI(e):</color> Automatically set up uPI(e) menu for unity versions 4.6.x and above, but below version 5.0\n" + infoMessage);
        }
#else
            if (!keepQuiet)
            {
                Debug.LogWarning("<color=orange>uPI(e):</color> Automatically set up uPI(e) menu for unity versions 5.x\n" + infoMessage);
            }
#endif

#if UNITY_EDITOR
            uPIeMenu.CreatePrefabInstance = (prefab) =>
            {
#if UNITY_4_6
            return (GameObject)EditorUtility.InstantiatePrefab(prefab);
#else
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#endif
        };
#endif

            alreadyRun = true;
        }
    }
}
#endif