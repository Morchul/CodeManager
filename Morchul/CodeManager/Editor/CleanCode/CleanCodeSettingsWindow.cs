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
        private CustomReorderableList codeDocumentationList;

        private CustomReorderableList codeGuidelineList;

        private CustomReorderableList regexList;

        private Vector2 scrollPos;

        private readonly string[] tabNames = new string[] { "Clean Code Settings", "Regexes" };
        private int selectedTab;

        private string[] regexNames;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        public static void ShowWindow()
        {
            instance = CreateInstance<CleanCodeSettingsWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("CleanCode settings");
            instance.Show();
        }

        private void OnEnable()
        {
            LoadSettings();

            CreateUnwantedCodeList();
            CreateRegexesList();
            CreateCodeDocumentationList();
            CreateCodeGuidelineList();
        }

        private void OnDisable()
        {
            settings.UpdateScanables();
        }

        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<CleanCodeSettings>(CodeManagerUtility.CleanCodeSettingsObject);
            if (settings == null) //Settings do not exist create new
            {
                settings = ScriptableObject.CreateInstance<CleanCodeSettings>();
                DefaultSettings.SetDefaultCleanCodeSettings(settings);
                AssetDatabase.CreateAsset(settings, CodeManagerUtility.CleanCodeSettingsObject);
                settings.UpdateScanables();
            }

            serializedSettings = new SerializedObject(settings);
        }

        private string[] GetRegexNames()
        {
            string[] names = new string[settings.Regexes.Length];
            for(int i = 0; i < names.Length; ++i)
            {
                names[i] = settings.Regexes[i].Name;
            }
            return names;
        }

        #region List creation
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
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Unwanted Code and will be shown in the scanables in the ScriptFolders"));
                        EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                        //Second add Description
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Description","The description will be displayed in the CleanCode Console if a UnwantedCode Violation appears to show the user what is wrong."));
                        EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), unwantedCode.FindPropertyRelative("Description"), GUIContent.none);

                        //Third add Regex
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Regex", "If this regex matches something in the code a UnwantedCode message will be displayed in the CleanCode Console."));
                        if(regexProperty.intValue < 0)
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
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Code Documentation and will be shown in the scanables in the ScriptFolders"));
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
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Name of the Code Guideline and will be shown in the scanables in the ScriptFolders"));
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
                        if(settings.UnwantedCodes[i].RegexIndex == index)
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
                case 0: DrawCleanCodeSettingsTab(); break;
                case 1: DrawRegexesTab(); break;
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

            if (GUILayout.Button(new GUIContent("Update Scanables", "Creating scanables is a resource heavy task. To limit the amount of creations, the Scanables will only be updated by closing this window or pressing this button.")))
            {
                settings.UpdateScanables();
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

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
