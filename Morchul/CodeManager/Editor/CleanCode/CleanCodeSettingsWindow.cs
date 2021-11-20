using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class CleanCodeSettingsWindow : EditorWindow
    {
        private static CleanCodeSettingsWindow instance;

        private CleanCodeSettings settings;

        private SerializedObject serializedSettings;

        private CustomReorderableList unwantedCodeList;

        private Vector2 scrollPos;

        private string[] tabNames = new string[] { "Documentation", "Code guidelines", "Unwanted Code" };
        private int selectedTab;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        public static void ShowWindow()
        {
            instance = CreateInstance<CleanCodeSettingsWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Clean code settings");
            instance.Show();
        }

        private void OnEnable()
        {
            LoadSettings();

            CreateUnwantedCodeList();
        }

        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<CleanCodeSettings>(CodeManagerUtility.CleanCodeSettingsObject);
            if (settings == null) //Settings do not exist create new
            {
                settings = ScriptableObject.CreateInstance<CleanCodeSettings>();
                AssetDatabase.CreateAsset(settings, CodeManagerUtility.CleanCodeSettingsObject);
            }

            serializedSettings = new SerializedObject(settings);
        }

        #region List creation

        private void CreateUnwantedCodeList()
        {
            unwantedCodeList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("UnwantedCodes"), settings.UnwantedCodes.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "Unwanted Code";
                    element.FindPropertyRelative("Regex").stringValue = @".*";
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty unwantedCode = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = unwantedCode.FindPropertyRelative("Name");
                    SerializedProperty regexProperty = unwantedCode.FindPropertyRelative("Regex");

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

                        //Second add regex
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Regex");
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), regexProperty, GUIContent.none);

                        //Add errorbox by wrong path name
                        if (!CodeManagerUtility.IsValidRegex(regexProperty.stringValue))
                        {
                            rect.y += list.LIST_ELEMENT_HEIGHT;
                            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, 40), "Invalid Regex!", MessageType.Error);
                        }
                    }
                },
                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        SerializedProperty unwantedCode = list.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty regexProperty = unwantedCode.FindPropertyRelative("Regex");

                        if (!CodeManagerUtility.IsValidRegex(regexProperty.stringValue))
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

        private void OnGUI()
        {
            if (settings == null) return;

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            switch (selectedTab)
            {
                case 0: DrawDocumentationTab(); break;
                case 1: DrawCodeGuideLinesTab(); break;
                case 2: DrawUnwantedCodeTag(); break;
            }
            
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawDocumentationTab()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Documentation required for:");
            ChangeDocumentationFlag(DocumentationFlags.FIELDS, EditorGUILayout.Toggle("Fields", settings.IsFlagSet(DocumentationFlags.FIELDS)));
            ChangeDocumentationFlag(DocumentationFlags.METHODS, EditorGUILayout.Toggle("Methods", settings.IsFlagSet(DocumentationFlags.METHODS)));
            ChangeDocumentationFlag(DocumentationFlags.CLASSES, EditorGUILayout.Toggle("Classes", settings.IsFlagSet(DocumentationFlags.CLASSES)));

            EditorGUILayout.EndVertical();
        }

        private void ChangeDocumentationFlag(DocumentationFlags flag, bool setFlag)
        {
            if (setFlag)
            {
                settings.DocumentationFlag |= flag;
            }
            else
            {
                settings.DocumentationFlag &= ~flag;
            }
        }

        private void DrawCodeGuideLinesTab()
        {

        }

        private void DrawUnwantedCodeTag()
        {
            EditorGUILayout.BeginVertical();

            unwantedCodeList.Expanded = EditorGUILayout.Foldout(unwantedCodeList.Expanded, "Unwanted code");
            if (unwantedCodeList.Expanded)
            {
                serializedSettings.Update();
                unwantedCodeList.DoLayoutList();
                serializedSettings.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}
