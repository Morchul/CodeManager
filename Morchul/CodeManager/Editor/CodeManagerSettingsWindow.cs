#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class CodeManagerSettingsWindow : EditorWindow
    {
        private static CodeManagerSettingsWindow instance;

        private CodeManagerSettings settings;

        private SerializedObject serializedSettings;

        private CustomReorderableList scriptFolderList;
        private CustomReorderableList placeHolderList;

        private CustomReorderableList unwantedCodeList;
        private CustomReorderableList codeDocumentationList;

        private CustomReorderableList codeGuidelineList;

        private CustomReorderableList regexList;

        private Vector2 scrollPos;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private readonly string[] tabNames = new string[] { "Script Templates", "Clean Code Rules", "Regexes" };
        private int selectedTab;

        private string[] regexNames;

        public static void ShowWindow()
        {
            instance = GetWindow<CodeManagerSettingsWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Code manager settings");
            instance.Show();
        }

        private void OnEnable()
        {
            settings = CodeManagerEditorUtility.LoadSettings(true);
            serializedSettings = new SerializedObject(settings);

            CreateScriptFolderList();
            CreatePlaceholderList();

            CreateUnwantedCodeList();
            CreateRegexesList();
            CreateCodeDocumentationList();
            CreateCodeGuidelineList();
        }

        private void OnDisable()
        {
            settings.UpdateRules();
        }

        #region Listcreation

        private void CreateScriptFolderList()
        {
            scriptFolderList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("ScriptFolders"), settings.ScriptFolders.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "New Folder";
                    element.FindPropertyRelative("Path").stringValue = "Assets/Scripts/";
                    element.FindPropertyRelative("IncludeSubDirectory").boolValue = false;
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
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Scriptfolder. Displayed when you create a new script and have to choose a folder"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 52, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Add Path
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Path", "The Path of the folder, has to end with a '/' e.g: Assets/Scripts/"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 150, EditorGUIUtility.singleLineHeight), pathProperty, GUIContent.none);

                        //Add Selectfolder button
                        if (GUI.Button(new Rect(rect.x + rect.width - 90, rect.y, 90, EditorGUIUtility.singleLineHeight), new GUIContent("Select Folder", "Browse for a folder in your explorer. The Folder has to be under Assets/")))
                        {
                            pathProperty.stringValue = CodeManagerEditorUtility.SelectFolderInAssets(pathProperty.stringValue);
                            Repaint();
                        }

                        //Add Include sub-directory toggle
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), new GUIContent("Include sub folders", "If set the Clean code rules also applie to all sub folders"));
                        EditorGUI.PropertyField(new Rect(rect.x + 110, rect.y, 40, EditorGUIUtility.singleLineHeight), scriptFolder.FindPropertyRelative("IncludeSubDirectory"), GUIContent.none);


                        //Add CleanCode rule selection button
                        if (GUI.Button(new Rect(rect.x + 180, rect.y, 230, EditorGUIUtility.singleLineHeight), "Select CleanCode rules for this folder"))
                        {
                            SelectCleanCodeRulesWindow.ShowWindow(settings.ScriptFolders[index]);
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
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "The name of the placeholder. Has to be written without % at the begin and end."));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Value
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Value", "The value through which the placeholder will be replaced by script creation."));
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

        private void CreateUnwantedCodeList()
        {
            unwantedCodeList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("UnwantedCodes"), settings.UnwantedCodes.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "Unwanted Code";
                    element.FindPropertyRelative("Description").stringValue = "Description";
                    element.FindPropertyRelative("RegexIndex").intValue = -1;
                    element.FindPropertyRelative("ID").intValue = 0;
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty unwantedCode = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = unwantedCode.FindPropertyRelative("Name");
                    SerializedProperty regexProperty = unwantedCode.FindPropertyRelative("RegexIndex");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], nameProperty.stringValue, false);
                    if (list.ElementExpanded[index])
                    {
                        //First Add name
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Unwanted Code and will be shown in the CleanCode rules in the ScriptFolders"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Description
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Description", "The description will be displayed in the CleanCode Console if a UnwantedCode Violation appears to show the user what is wrong."));
                        EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), unwantedCode.FindPropertyRelative("Description"), GUIContent.none);

                        //Third add Regex
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Regex", "If this regex matches something in the code a UnwantedCode message will be displayed in the CleanCode Console."));
                        if (regexProperty.intValue < 0)
                        {
                            Color defaultColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;
                            regexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), regexProperty.intValue, regexNames);
                            GUI.backgroundColor = defaultColor;
                        }
                        else
                        {
                            regexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), regexProperty.intValue, regexNames);
                        }
                    }
                },
                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 4;
                    }
                    else
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT;
                    }
                    return list.ElementHeights[index];
                }
            };
        }

        private void CreateCodeDocumentationList()
        {
            codeDocumentationList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("CodeDocumentations"), settings.CodeDocumentations.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "Code Documentation";
                    element.FindPropertyRelative("Description").stringValue = "Description";
                    element.FindPropertyRelative("RegexIndex").intValue = -1;
                    element.FindPropertyRelative("ID").intValue = 0;
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty codeDocumentation = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = codeDocumentation.FindPropertyRelative("Name");
                    SerializedProperty regexProperty = codeDocumentation.FindPropertyRelative("RegexIndex");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], nameProperty.stringValue, false);

                    if (list.ElementExpanded[index])
                    {
                        //First Add name
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Code Documentation and will be shown in the CleanCode rules in the ScriptFolders"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Description
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Description", "The description will be displayed in the CleanCode Console if a Code documentation violation appears to show the user what is wrong."));
                        EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), codeDocumentation.FindPropertyRelative("Description"), GUIContent.none);

                        //Third add Regex
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Regex", "This Regex searches for the Code which has to be documented. If something is found the text before will be tested with the Documentation Regex"));
                        if (regexProperty.intValue < 0)
                        {
                            Color defaultColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;
                            regexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), regexProperty.intValue, regexNames);
                            GUI.backgroundColor = defaultColor;
                        }
                        else
                        {
                            regexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), regexProperty.intValue, regexNames);
                        }
                    }
                },
                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 4;
                    }
                    else
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT;
                    }
                    return list.ElementHeights[index];
                }
            };
        }

        private void CreateCodeGuidelineList()
        {
            codeGuidelineList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("CodeGuidelines"), settings.CodeGuidelines.Length)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "Code Guideline";
                    element.FindPropertyRelative("Description").stringValue = "<Description>";
                    element.FindPropertyRelative("GroupName").stringValue = "<GroupName>";
                    element.FindPropertyRelative("SearchRegexIndex").intValue = -1;
                    element.FindPropertyRelative("MatchRegexIndex").intValue = -1;
                    element.FindPropertyRelative("ID").intValue = 0;
                },

                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty codeGuideline = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProperty = codeGuideline.FindPropertyRelative("Name");
                    SerializedProperty searchRegexProperty = codeGuideline.FindPropertyRelative("SearchRegexIndex");
                    SerializedProperty matchRegexProperty = codeGuideline.FindPropertyRelative("MatchRegexIndex");
                    SerializedProperty groupNameProperty = codeGuideline.FindPropertyRelative("GroupName");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], nameProperty.stringValue, false);

                    if (list.ElementExpanded[index])
                    {
                        //First Add name
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Code Guideline and will be shown in the CleanCode rules in the ScriptFolders"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Description
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Description", "The description will be displayed in the CleanCode Console if a CodeGuideline violation appears to show the user what is wrong."));
                        EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), codeGuideline.FindPropertyRelative("Description"), GUIContent.none);

                        //Third add SearchRegex and GroupName
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), new GUIContent("Search Regex", "This Regex searches for the Code which has to be checked for Code guideline. If something is found the text marked by the GroupName will be checked with the Match Regex."));

                        if (searchRegexProperty.intValue < 0)
                        {
                            Color defaultColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;
                            searchRegexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 90, rect.y, 300, EditorGUIUtility.singleLineHeight), searchRegexProperty.intValue, regexNames);
                            GUI.backgroundColor = defaultColor;
                        }
                        else
                        {
                            searchRegexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 90, rect.y, 300, EditorGUIUtility.singleLineHeight), searchRegexProperty.intValue, regexNames);
                        }

                        EditorGUI.LabelField(new Rect(rect.x + 400, rect.y, 80, EditorGUIUtility.singleLineHeight), new GUIContent("Group Name", "Defines which part of the SearchRegex will be testet with the MatchRegex."));

                        if (string.IsNullOrEmpty(groupNameProperty.stringValue))
                        {
                            Color defaultColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;
                            EditorGUI.PropertyField(new Rect(rect.x + 480, rect.y, rect.width - 480, EditorGUIUtility.singleLineHeight), groupNameProperty, GUIContent.none);
                            GUI.backgroundColor = defaultColor;
                        }
                        else
                        {
                            EditorGUI.PropertyField(new Rect(rect.x + 480, rect.y, rect.width - 480, EditorGUIUtility.singleLineHeight), groupNameProperty, GUIContent.none);
                        }

                        //Last add MatchRegex
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), new GUIContent("Match Regex", "The value of the GroupName will be tested with this Regex. If there is no match there is a code guideline violation."));
                        if (matchRegexProperty.intValue < 0)
                        {
                            Color defaultColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;
                            matchRegexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 90, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight), matchRegexProperty.intValue, regexNames);
                            GUI.backgroundColor = defaultColor;
                        }
                        else
                        {
                            matchRegexProperty.intValue = EditorGUI.Popup(new Rect(rect.x + 90, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight), matchRegexProperty.intValue, regexNames);
                        }


                    }
                },
                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT * 5;
                    }
                    else
                    {
                        list.ElementHeights[index] = list.LIST_ELEMENT_HEIGHT;
                    }
                    return list.ElementHeights[index];
                }
            };
        }

        private void CreateRegexesList()
        {
            regexList = new CustomReorderableList(serializedSettings, serializedSettings.FindProperty("Regexes"), settings.Regexes.Length, false)
            {
                onCreateNewItemCallback = (element) =>
                {
                    element.FindPropertyRelative("Name").stringValue = "Regex name";
                    element.FindPropertyRelative("Regex").stringValue = ".*";
                },

                //Adjust Regex indexes if a regex was deleted
                onElementRemovedCallback = (index) =>
                {
                    //Unwanted Code
                    for (int i = 0; i < settings.UnwantedCodes.Length; ++i)
                    {
                        //Regex was deleted
                        if (settings.UnwantedCodes[i].RegexIndex == index)
                        {
                            settings.UnwantedCodes[i].RegexIndex = -1;
                        }
                        //A Regex below was deleted adjust index
                        else if (settings.UnwantedCodes[i].RegexIndex > index)
                        {
                            settings.UnwantedCodes[i].RegexIndex -= 1;
                        }
                    }

                    //Code documentation
                    for (int i = 0; i < settings.CodeDocumentations.Length; ++i)
                    {
                        //Regex was deleted
                        if (settings.CodeDocumentations[i].RegexIndex == index)
                        {
                            settings.CodeDocumentations[i].RegexIndex = -1;
                        }
                        //A Regex below was deleted adjust index
                        else if (settings.CodeDocumentations[i].RegexIndex > index)
                        {
                            settings.CodeDocumentations[i].RegexIndex -= 1;
                        }
                    }

                    //Code guidelines
                    for (int i = 0; i < settings.CodeGuidelines.Length; ++i)
                    {
                        //Regex was deleted
                        if (settings.CodeGuidelines[i].SearchRegexIndex == index)
                        {
                            settings.CodeGuidelines[i].SearchRegexIndex = -1;
                        }
                        //A Regex below was deleted adjust index
                        else if (settings.CodeGuidelines[i].SearchRegexIndex > index)
                        {
                            settings.CodeGuidelines[i].SearchRegexIndex -= 1;
                        }

                        //Regex was deleted
                        if (settings.CodeGuidelines[i].MatchRegexIndex == index)
                        {
                            settings.CodeGuidelines[i].MatchRegexIndex = -1;
                        }
                        //A Regex below was deleted adjust index
                        else if (settings.CodeGuidelines[i].MatchRegexIndex > index)
                        {
                            settings.CodeGuidelines[i].MatchRegexIndex -= 1;
                        }
                    }

                    serializedSettings.Update();
                    serializedSettings.ApplyModifiedProperties();
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

                        //Second add Regex
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
            regexNames = GetRegexNames();

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            switch (selectedTab)
            {
                case 0: DrawScriptTemplateSettingsTab(); break;
                case 1: DrawCleanCodeSettingsTab(); break;
                case 2: DrawRegexesTab(); break;
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }


        private void DrawCleanCodeSettingsTab()
        {
            if (settings.Regexes.Length == 0)
            {
                EditorGUILayout.HelpBox("There are no regexes created yet. Please create some Regexes in the Regexes Tab.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button(new GUIContent("Update CleanCode Rules", "Creating CleanCode rules is a resource heavy task. To limit the amount of creations, the CleanCode rules will only be updated by closing this window or pressing this button.")))
            {
                settings.UpdateRules();
            }

            serializedSettings.Update();
            unwantedCodeList.Expanded = EditorGUILayout.Foldout(unwantedCodeList.Expanded, "Unwanted code");
            if (unwantedCodeList.Expanded)
            {
                unwantedCodeList.DoLayoutList();
            }

            EditorGUILayout.Space(5);

            codeDocumentationList.Expanded = EditorGUILayout.Foldout(codeDocumentationList.Expanded, "Code Documentation");
            if (codeDocumentationList.Expanded)
            {
                codeDocumentationList.DoLayoutList();
            }


            EditorGUILayout.Space(5);

            codeGuidelineList.Expanded = EditorGUILayout.Foldout(codeGuidelineList.Expanded, "Code Guidelines");
            if (codeGuidelineList.Expanded)
            {
                codeGuidelineList.DoLayoutList();
            }
            serializedSettings.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }

        private void DrawRegexesTab()
        {
            EditorGUILayout.BeginVertical();

            settings.DocumentationRegex.Regex = EditorGUILayout.TextField(new GUIContent("Documentation Regex", "This regex checks if it is a documentation for the next CodePiece"), settings.DocumentationRegex.Regex);

            regexList.Expanded = EditorGUILayout.Foldout(regexList.Expanded, "Regexes");
            if (regexList.Expanded)
            {
                serializedSettings.Update();
                regexList.DoLayoutList();
                serializedSettings.ApplyModifiedProperties();
            }

            if(GUILayout.Button(new GUIContent("Test Regex Window")))
            {
                RegexTesterWindow.ShowWindow(regexList.index);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawScriptTemplateSettingsTab()
        {

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
        }

        #endregion

        private string[] GetRegexNames()
        {
            string[] names = new string[settings.Regexes.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                names[i] = settings.Regexes[i].Name;
            }
            return names;
        }
    }
}
#endif