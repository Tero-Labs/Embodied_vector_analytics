// <copyright file="uPIeUpgradeTool.cs" company="Yearntech">
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
// <summary>Upgrade tool</summary>

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace uPIe
{
    /// <summary>
    ///     This provides upgrade tools, when upgrading uPIe dll-version to sourcecode-version. Before upgrading all menus in a
    ///     scene can be saved automatically when executing "uPIe Tools/Upgrade Helper/Save Scene Menus". If every scene is saved
    ///     the uPIe dll-version can be deleted and replaced with the pro-version.
    ///     After that calling "uPIe Tools/Upgrade Helper/Load Scene Menus" in each scene sets the menus up, the way they were before
    /// </summary>
    public class uPIeUpgradeTool : Editor
    {
        private const string tempSaveDataDirectoryName = "TEMP_uPIeUpgradeData";

        [MenuItem("uPIe Tools/Upgrade Helper/Save Scene Menus")]
        private static void SaveSceneMenus()
        {
            try
            {
                var uPIesInScene = GetUPIeSceneInstances();
                var targetDirectoryInfo = GetCurrentDataDirectory();

                // clear old files (if there are some)
                foreach (var fi in targetDirectoryInfo.GetFiles())
                {
                    fi.Delete();
                }

                for (int i = 0; i < uPIesInScene.Count; i++)
                {
                    SerializeObject(SerializeMenu(uPIesInScene[i]), Path.Combine(targetDirectoryInfo.FullName, i.ToString()));
                }

                Debug.Log("<color=green>Successfully saved " + uPIesInScene.Count + " uPIe menu(s) to \"" + targetDirectoryInfo.FullName + "\"</color>\n" +
                          "When done upgrading and loading the data with the upgraded version of uPIe the directory \"" + targetDirectoryInfo.Parent.FullName + "\" can be deleted");
            }
            catch (Exception e)
            {
                Debug.LogWarning("<color=red>Error</color> while trying to save uPIe scene data: " + e.Message);
            }
        }

        [MenuItem("uPIe Tools/Upgrade Helper/Load Scene Menus")]
        private static void LoadSceneMenus()
        {
            if (!EditorUtility.DisplayDialog("Replace all uPIe Settings in scene?",
                                             "Are you sure you want to replace the settings of all uPIe menus in this scene?",
                                             "Yes", "Cancel"))
            {
                return;
            }

            try
            {
                var uPIesInScene = GetUPIeSceneInstances();
                var targetDirectoryInfo = GetCurrentDataDirectory();
                var dataFiles = targetDirectoryInfo.GetFiles().Where(fi => fi.Name.EndsWith(".xml")).ToList();
                var data = dataFiles.Select(fi => DeserializeObject<uPIeMenuSerializable>(fi.FullName)).ToList();
                if (data.Count <= 0)
                {
                    throw new Exception("No saved menus found for this scene. Are you sure you saved them before?");
                }

                for (int i = 0; i < uPIesInScene.Count; i++)
                {
                    DeserializeMenu(data[i], uPIesInScene[i]);
                }

                Debug.Log("<color=green>Successfully set up " + data.Count + " uPIe menu(s)</color>\n" +
                          "When done setting up all scenes, the directory \"" + targetDirectoryInfo.Parent.FullName + "\" is no longer of use and can be deleted");
            }
            catch (Exception e)
            {
                Debug.LogWarning("<color=red>Error</color> while trying to load uPIe scene data: " + e.Message);
            }
        }

        [MenuItem("uPIe Tools/Upgrade Helper/Remove Temp Data")]
        private static void RemoveTempData()
        {
            if (!EditorUtility.DisplayDialog("Remove temp upgrade data?",
                                             "Are you sure you want to remove all saved menu data?",
                                             "Yes, delete it", "Cancel"))
            {
                return;
            }

            try
            {
                var targetDirectoryInfo = GetCurrentDataDirectory().Parent;
                targetDirectoryInfo.Delete(true);
                Debug.Log("<color=green>Successfully deleted temp upgrade data</color>");
            }
            catch (Exception e)
            {
                Debug.LogWarning("<color=red>Error</color> while trying to remove temporary uPIe upgrade data: " + e.Message);
            }
        }

        /// <summary>
        ///     Returns all instances of uPIe menus in the current scene
        /// </summary>
        /// <returns></returns>
        private static List<uPIeMenu> GetUPIeSceneInstances()
        {
            var uPIesInScene = AllSceneObjects().Where(t => t.GetComponent<uPIeMenu>() != null).Select(t => t.GetComponent<uPIeMenu>()).ToList();
            if (uPIesInScene.Count <= 0)
            {
                throw new Exception("No uPIe Menus found in this scene");
            }

            return uPIesInScene;
        }

        /// <summary>
        ///     Returns the directory where the uPIe data is / should be stored for
        ///     the current scene
        /// </summary>
        /// <returns></returns>
        private static DirectoryInfo GetCurrentDataDirectory()
        {
            var mainDirectoryPath = Path.Combine(Application.dataPath, tempSaveDataDirectoryName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            var targetDir = Path.Combine(mainDirectoryPath, GetCurrentSceneName());

            // this only actually creates the directory if it does not already exist
            return Directory.CreateDirectory(targetDir);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        private static void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }
            fileName = Path.ChangeExtension(fileName, ".xml");

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error while serializing: " + e.Message);
            }
        }

        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static T DeserializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader stringReader = new StringReader(xmlString))
                {
                    Type outType = typeof(T);
                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(stringReader))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    stringReader.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error while deserializing: " + e.Message);
            }

            return objectOut;
        }

        /// <summary>
        ///     Gets the XmlDeclaration if it exists, creates a new if not.
        /// </summary>
        ///     <param name="xmlDocument"></param>
        /// <returns></returns>
        private static XmlDeclaration GetOrCreateXmlDeclaration(ref XmlDocument xmlDocument, string encoding)
        {
            XmlDeclaration xmlDeclaration = null;
            if (xmlDocument.HasChildNodes)
                xmlDeclaration = xmlDocument.FirstChild as XmlDeclaration;

            if (xmlDeclaration != null)
                return xmlDeclaration;

            //Create an XML declaration. 
            xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", encoding, null);

            //Add the new node to the document.
            XmlElement root = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(xmlDeclaration, root);
            return xmlDeclaration;
        }

        /// <summary>
        ///     Retuns the current scene's root objects
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<GameObject> SceneRoots()
        {
            var prop = new HierarchyProperty(HierarchyType.GameObjects);
            var expanded = new int[0];
            while (prop.Next(expanded))
            {
                yield return prop.pptrValue as GameObject;
            }
        }

        /// <summary>
        ///     Returns a list with all objects in the scene
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Transform> AllSceneObjects()
        {
            var queue = new Queue<Transform>();

            foreach (var root in SceneRoots())
            {
                var tf = root.transform;
                yield return tf;
                queue.Enqueue(tf);
            }

            while (queue.Count > 0)
            {
                foreach (Transform child in queue.Dequeue())
                {
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        ///     Returns the name of the currently open scene
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentSceneName()
        {
#if UNITY_5_3_OR_NEWER
            return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
#else
            return Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
#endif
        }

        /// <summary>
        ///     Serializes the data of a uPIe menu instance
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        private static uPIeMenuSerializable SerializeMenu(uPIeMenu menu)
        {
            var data = new uPIeMenuSerializable();
            data.ControlWithGamepad = menu.ControlWithGamepad;
            data.ControllerDeadzone = menu.ControllerDeadzone;
            data.UseCustomInputSystem = menu.UseCustomInputSystem;
            data.CustomInput = menu.CustomInput;
            data.ConfirmInputName = menu.ConfirmInputName;
            data.HorizontalInputName = menu.HorizontalInputName;
            data.VerticalInputName = menu.VerticalInputName;

            if (menu.IndicatorGraphic != null)
            {
                data.IndicatorGraphicInstanceId = menu.IndicatorGraphic.GetInstanceID();
            }

            data.ApplyIndicatorRotation = menu.ApplyIndicatorRotation;
            data.StartDegOffset = menu.StartDegOffset;
            if (menu.MenuOptionPrefab != null)
            {
                data.MenuOptionPrefabId = menu.MenuOptionPrefab.GetInstanceID();
            }

            foreach (var menuOption in menu.MenuOptions)
            {
                data.MenuOptionsInstanceIds.Add(menuOption.GetInstanceID());
            }

            data.CircleSize = menu.CircleSize;
            data.ConstrainIndicatorPosition = menu.ConstrainIndicatorPosition;
            data.KeepSelectedOption = menu.KeepSelectedOption;
            if (menu.DefaultSelected != null)
            {
                data.DefaultSelectedInstanceId = menu.DefaultSelected.GetInstanceID();
            }

            data.DeselectOptionIfOutsideBorders = menu.DeselectOptionIfOutsideBorders;
            data.AlignRadius = menu.AlignRadius;
            data.AlignRotation = menu.AlignRotation;
            data.AlignUpDirection = menu.AlignUpDirection;
            data.AlignForwardDirection = menu.AlignForwardDirection;
            data.EnableSelecting = menu.EnableSelecting;
            data.CurrentDirection = menu.CurrentDirection;

            data.DoDrawGizmos = menu.DoDrawGizmos;
            data.DrawOnlyOnSelected = menu.DrawOnlyOnSelected;
            data.BoundaryLength = menu.BoundaryLength;
            data.DebugFoldout = menu.DebugFoldout;

            data.SetCircleSizeDirectly = menu.SetCircleSizeDirectly;
            data.SelectedCircleSizeId = menu.SelectedCircleSizeId;
            data.ButtonsFoldout = menu.ButtonsFoldout;
            data.UseControllerDeadzoneSlider = menu.UseControllerDeadzoneSlider;
            return data;
        }

        /// <summary>
        ///     Replaces the data of a uPIeMenu instance with the data serialized before
        /// </summary>
        /// <param name="from"></param>
        /// <param name="target"></param>
        private static void DeserializeMenu(uPIeMenuSerializable from, uPIeMenu target)
        {
            target.ControlWithGamepad = from.ControlWithGamepad;
            target.ControllerDeadzone = from.ControllerDeadzone;
            target.UseCustomInputSystem = from.UseCustomInputSystem;
            target.CustomInput = from.CustomInput;
            target.ConfirmInputName = from.ConfirmInputName;
            target.HorizontalInputName = from.HorizontalInputName;
            target.VerticalInputName = from.VerticalInputName;

            if (from.IndicatorGraphicInstanceId != -1)
            {
                target.IndicatorGraphic = EditorUtility.InstanceIDToObject(from.IndicatorGraphicInstanceId) as Graphic;
            }

            target.ApplyIndicatorRotation = from.ApplyIndicatorRotation;
            target.StartDegOffset = from.StartDegOffset;
            if (from.MenuOptionPrefabId != -1)
            {
                target.MenuOptionPrefab = EditorUtility.InstanceIDToObject(from.MenuOptionPrefabId) as GameObject;
            }

            if (from.MenuOptionsInstanceIds.Count > 0)
            {
                target.MenuOptions.Clear();
            }

            foreach (var menuOptionInstanceId in from.MenuOptionsInstanceIds)
            {
                target.MenuOptions.Add(EditorUtility.InstanceIDToObject(menuOptionInstanceId) as Selectable);
            }

            target.CircleSize = from.CircleSize;
            target.ConstrainIndicatorPosition = from.ConstrainIndicatorPosition;
            target.KeepSelectedOption = from.KeepSelectedOption;
            if (from.DefaultSelectedInstanceId != -1)
            {
                target.DefaultSelected = EditorUtility.InstanceIDToObject(from.DefaultSelectedInstanceId) as Selectable;
            }

            target.DeselectOptionIfOutsideBorders = from.DeselectOptionIfOutsideBorders;
            target.AlignRadius = from.AlignRadius;
            target.AlignRotation = from.AlignRotation;
            target.AlignUpDirection = from.AlignUpDirection;
            target.AlignForwardDirection = from.AlignForwardDirection;
            target.EnableSelecting = from.EnableSelecting;
            target.CurrentDirection = from.CurrentDirection;

            target.DoDrawGizmos = from.DoDrawGizmos;
            target.DrawOnlyOnSelected = from.DrawOnlyOnSelected;
            target.BoundaryLength = from.BoundaryLength;
            target.DebugFoldout = from.DebugFoldout;

            target.SetCircleSizeDirectly = from.SetCircleSizeDirectly;
            target.SelectedCircleSizeId = from.SelectedCircleSizeId;
            target.ButtonsFoldout = from.ButtonsFoldout;
            target.UseControllerDeadzoneSlider = from.UseControllerDeadzoneSlider;
        }
    }
}
#endif