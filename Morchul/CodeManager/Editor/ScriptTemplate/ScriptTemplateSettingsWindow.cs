#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Morchul.CodeManager
{
    public class ScriptTemplateSettingsWindow : EditorWindow
    {
        private static ScriptTemplateSettingsWindow instance;

        private ScriptTemplateSettings settings;

        private SerializedObject serializedSettings;

        private CustomReorderableList scriptFolderList;
        private CustomReorderableList placeHolderList;

        private Vector2 scrollPos;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        public static void ShowWindow()
        {
            instance = CreateInstance<ScriptTemplateSettingsWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Script template settings");
            instance.Show();
        }

        private void OnEnable()
        {
            LoadSettings();

            CreateScriptFolderList();
            CreatePlaceholderList();
        }

        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<ScriptTemplateSettings>(CodeManagerUtility.ScriptTemplateSettingsObject);
            if (settings == null) //Settings do not exist create new
            {
                settings = ScriptableObject.CreateInstance<ScriptTemplateSettings>();
                AssetDatabase.CreateAsset(settings, CodeManagerUtility.ScriptTemplateSettingsObject);
            }

            serializedSettings = new SerializedObject(settings);
        }

        #region List creation
        private void CreateScriptFolderList()
        {
            scriptFolderList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("ScriptFolders"), settings.ScriptFolders.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "New Folder";
                    element.FindPropertyRelative("Path").stringValue = "Assets/Scripts/";
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty scriptFolder = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = scriptFolder.FindPropertyRelative("Name");
                    SerializedProperty pathProperty = scriptFolder.FindPropertyRelative("Path");
                    SerializedProperty documentationFlags = scriptFolder.FindPropertyRelative("ScanFor");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], nameProperty.stringValue, false);
                    if (list.ElementExpanded[index])
                    {
                        //Add name
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Name");
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 52, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Add Path
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Path");
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 150, EditorGUIUtility.singleLineHeight), pathProperty, GUIContent.none);

                        //Add Selectfolder button
                        if (GUI.Button(new Rect(rect.x + rect.width - 90, rect.y, 90, EditorGUIUtility.singleLineHeight), new GUIContent("Select Folder")))
                        {
                            pathProperty.stringValue = CodeManagerEditorUtility.SelectFolderInAssets(pathProperty.stringValue);
                            Repaint();
                        }

                        //Add Scan selection button
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        if(GUI.Button(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), "Select scans for this folder"))
                        {
                            SelectScansWindow.ShowWindow(settings.ScriptFolders[index]);
                        }

                        //Add errorbox by wrong path name
                        if (!CodeManagerEditorUtility.IsValidAssetsFolderPath(pathProperty.stringValue))
                        {
                            rect.y += list.LIST_ELEMENT_HEIGHT;
                            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, 40), "This is not a valid folder path.", MessageType.Error);
                        }
                    }
                },

                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        SerializedProperty scriptFolder = list.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty pathProperty = scriptFolder.FindPropertyRelative("Path");

                        if (!CodeManagerEditorUtility.IsValidAssetsFolderPath(pathProperty.stringValue))
                        {
                            list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 4 + 40;
                        }
                        else
                        {
                            list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 4;
                        }
                    }
                    else
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT;
                    }
                    return list.ElementHeights[index];
                }
            };

        }

        private void CreatePlaceholderList()
        {
            placeHolderList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("Placeholders"), settings.Placeholders.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "New Placeholder";
                    element.FindPropertyRelative("Value").stringValue = "";
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty placeholder = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = placeholder.FindPropertyRelative("Name");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], nameProperty.stringValue, false);

                    if (list.ElementExpanded[index])
                    {
                        //First Add name
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Name");
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Value
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Value");
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), placeholder.FindPropertyRelative("Value"), GUIContent.none);

                        //Add errorbox by wrong path name
                        if (ScriptCreator.IsDefaultPlaceholderName(nameProperty.stringValue))
                        {
                            rect.y += list.LIST_ELEMENT_HEIGHT;
                            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, 40), "The Placeholder name: \"" + nameProperty.stringValue + "\" is a default placeholder name and can not be used!", MessageType.Error);
                        }
                    }
                },
                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        SerializedProperty scriptFolder = list.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty nameProperty = scriptFolder.FindPropertyRelative("Name");

                        if (ScriptCreator.IsDefaultPlaceholderName(nameProperty.stringValue))
                        {
                            list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 3 + 40;
                        }
                        else
                        {
                            list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 3;
                        }
                    }
                    else
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT;
                    }
                    return list.ElementHeights[index];
                }
            };
        }
        #endregion

        #region Draw
        private void OnInspectorUpdate()
        {
            //Repaint();
        }

        private void OnGUI()
        {
            if (settings == null) return;

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            scriptFolderList.Expanded = EditorGUILayout.Foldout(scriptFolderList.Expanded, "Script Folders");
            if (scriptFolderList.Expanded)
            {
                serializedSettings.Update();
                scriptFolderList.DoLayoutList();
                serializedSettings.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(10);

            placeHolderList.Expanded = EditorGUILayout.Foldout(placeHolderList.Expanded, "Placeholders");
            if (placeHolderList.Expanded)
            {
                serializedSettings.Update();
                placeHolderList.DoLayoutList();
                serializedSettings.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        #endregion
    }
}

#endif