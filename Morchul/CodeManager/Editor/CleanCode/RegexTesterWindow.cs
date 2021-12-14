#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace Morchul.CodeManager
{
    /// <summary>
    /// A simple window to test regexes and correct existing ones.
    /// </summary>
    public class RegexTesterWindow : EditorWindow
    {
        private static RegexTesterWindow instance;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private const int FONT_SIZE = 14;

        private const float MATCH_FIELD_WIDTH = 370;

        private const float MATCH_GROUP_WIDTH = MATCH_FIELD_WIDTH - 10;

        private const float MAX_MATCH_GROUP_HEIGHT = 100;

        private CodeManagerSettings settings;

        private GUIStyle boldLabelStyle;
        private GUIStyle regexAreaStyle;
        private GUIStyle textAreaStyle;

        private Values values;

        #region Regex Tester Window Values
        [System.Serializable]
        private class Values : ScriptableObject
        {
            public string regex;
            public string text;
            public RegexOptions regexOptions;
            public bool autoSearch;
        };
        #endregion
        
        private bool searched;

        private Vector2 scrollPos;
        private Vector2 scrollPos2;

        private LinkedListNode<CodePiece>[] currentFoundCodePieces;

        private CodeInspection codeInspection;

        private int selectedRegexIndex;

        private RegexTesterMatches regexMatches;

        private CustomReorderableList matchesList;

        private SerializedObject serializedMatches;

        private Vector2[] elementGroupListScrollPos;

        private readonly string[] colorArray = new string[] { "#FF00FFFF", "#0000FFFF", "#ffa500ff", "#008000ff", "#800080ff", "#008080ff" };

        /// <summary>
        /// Method to show the window
        /// </summary>
        /// <param name="selectedRegexIndex">Index of the regex in Settings.Regexes[] which should be edited</param>
        public static void ShowWindow(int selectedRegexIndex)
        {
            instance = GetWindow<RegexTesterWindow>();

            instance.selectedRegexIndex = selectedRegexIndex;
            instance.SelectRegex(selectedRegexIndex);

            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Regex tester");
            instance.Show();
        }

        private void OnEnable()
        {
            values = ScriptableObject.CreateInstance<Values>();
            values.autoSearch = true;

            searched = false;
            settings = CodeManagerEditorUtility.LoadSettings();
            SelectRegex(selectedRegexIndex);

            regexMatches = ScriptableObject.CreateInstance<RegexTesterMatches>();

            serializedMatches = new SerializedObject(regexMatches);
            
            if(boldLabelStyle == null)
            {
                boldLabelStyle = new GUIStyle("Label")
                {
                    fontStyle = FontStyle.Bold,
                };
            }

            if(codeInspection == null)
            {
                values.text = "";
                codeInspection = CodeInspector.InspectText(values.text);
            }
        }

        private void SelectRegex(int index)
        {
            if(settings != null && index >= 0 && index < settings.Regexes.Length)
            {
                values.regex = settings.Regexes[index].Regex;
            }
        }

        private void OnDisable()
        {
            if (selectedRegexIndex >= 0 && selectedRegexIndex < settings.Regexes.Length)
            {
                if (settings.Regexes[selectedRegexIndex].Regex != values.regex)
                {
                    if (EditorUtility.DisplayDialog("Replace regex?", "Should the selected Regex be replaced by the new regex?", "Yes", "No"))
                    {
                        if (selectedRegexIndex < settings.Regexes.Length)
                            settings.Regexes[selectedRegexIndex].Regex = values.regex;
                    }
                }
            }
        }

        #region List Creation
        private void CreateMatchesList()
        {
            elementGroupListScrollPos = new Vector2[regexMatches.Matches.Length];

            for(int i = 0; i < regexMatches.Matches.Length; ++i)
            {
                elementGroupListScrollPos[i] = Vector2.zero;
            }

            matchesList = new CustomReorderableList(serializedMatches, serializedMatches.FindProperty("Matches"), regexMatches.Matches.Length, false, false, false)
            {
                onElementDrawCallback = (Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list) =>
                {
                    SerializedProperty matchProperty = list.serializedProperty.GetArrayElementAtIndex(index);
                    SerializedProperty matchTextProperty = matchProperty.FindPropertyRelative("MatchText");
                    SerializedProperty matchGroupArrayProperty = matchProperty.FindPropertyRelative("MatchGroups");

                    Rect foldoutRect = rect;
                    if (list.ElementExpanded[index])
                        foldoutRect.y -= list.ElementHeights[index] / 2 - list.LIST_ELEMENT_HEIGHT / 2;

                    list.ElementExpanded[index] = EditorGUI.Foldout(foldoutRect, list.ElementExpanded[index], "Match " + (index + 1), false);
                    if (list.ElementExpanded[index])
                    {
                        
                        float matchTextHeight = list.ElementHeights[index] - MAX_MATCH_GROUP_HEIGHT - list.LIST_ELEMENT_HEIGHT * 3;

                        //Add MatchText
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), new GUIContent("Match text:", "The text which was matched by the regex."), boldLabelStyle);
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, MATCH_FIELD_WIDTH, matchTextHeight), new GUIContent(matchTextProperty.stringValue, matchTextProperty.stringValue));

                        //rect.y += list.LIST_ELEMENT_HEIGHT;
                        rect.y += matchTextHeight;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, MATCH_GROUP_WIDTH / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Group name", "The names of all groups found in this match. The coresponding value is on the left."), boldLabelStyle);
                        EditorGUI.LabelField(new Rect(rect.x + MATCH_GROUP_WIDTH / 2, rect.y, MATCH_GROUP_WIDTH / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Group value", "The values of all groups found in this match. The coresponding name is on the right."), boldLabelStyle);

                        //Add Groups
                        rect.y += list.LIST_ELEMENT_HEIGHT;
                        float groupHeight = list.LIST_ELEMENT_HEIGHT * matchGroupArrayProperty.arraySize;
                        Rect viewRect = new Rect(rect.x, rect.y, MATCH_GROUP_WIDTH, groupHeight);

                        elementGroupListScrollPos[index] = GUI.BeginScrollView(new Rect(rect.x, rect.y, MATCH_GROUP_WIDTH, MAX_MATCH_GROUP_HEIGHT), elementGroupListScrollPos[index], viewRect, false, false);

                        for (int i = 0; i < matchGroupArrayProperty.arraySize; ++i)
                        {
                            //rect.y += list.LIST_ELEMENT_HEIGHT;
                            SerializedProperty groupProperty = matchGroupArrayProperty.GetArrayElementAtIndex(i);
                            SerializedProperty groupNameProperty = groupProperty.FindPropertyRelative("Name");
                            SerializedProperty groupValueProperty = groupProperty.FindPropertyRelative("Value");

                            EditorGUI.LabelField(new Rect(rect.x, rect.y, MATCH_GROUP_WIDTH / 2, list.LIST_ELEMENT_HEIGHT), new GUIContent(groupNameProperty.stringValue, groupNameProperty.stringValue));
                            EditorGUI.LabelField(new Rect(rect.x + MATCH_GROUP_WIDTH / 2, rect.y, MATCH_GROUP_WIDTH / 2, list.LIST_ELEMENT_HEIGHT), new GUIContent(groupValueProperty.stringValue, groupValueProperty.stringValue));
                            rect.y += list.LIST_ELEMENT_HEIGHT;
                        }

                        GUI.EndScrollView();
                    }
                },

                onElementHeightCallback = (int index, CustomReorderableList list) =>
                {
                    if (list.ElementExpanded[index])
                    {
                        SerializedProperty match = list.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty matchProperty = match.FindPropertyRelative("MatchText");

                        CodeManagerUtility.GetLineCount(matchProperty.stringValue, out int lineCount);
                        float matchTextHeight = lineCount * EditorGUIUtility.singleLineHeight;
                        list.ElementHeights[index] = matchTextHeight + MAX_MATCH_GROUP_HEIGHT + list.LIST_ELEMENT_HEIGHT * 3;
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

        #region Regex Matches

        #region class and structs
        private class RegexTesterMatches : ScriptableObject
        {
            public RegexTesterMatch[] Matches;
        }

        [System.Serializable]
        private struct RegexTesterMatch
        {
            public string MatchText;
            public RegexTesterGroup[] MatchGroups;
        }

        [System.Serializable]
        private struct RegexTesterGroup
        {
            public string Name;
            public string Value;
        }
        #endregion

        private CodeInspectionSettings GetCodeInspectionSettings()
        {
            return new CodeInspectionSettings()
            {
                AddLineIndex = true,
                RegexTimeout = System.TimeSpan.Zero,
                RegexOptions = values.regexOptions
            };
        }

        private RegexTesterMatch[] GetMatches(LinkedListNode<CodePiece>[] codePieces)
        {
            RegexTesterMatch[] matches = new RegexTesterMatch[codePieces.Length];

            for (int i = 0; i < matches.Length; ++i)
            {
                matches[i] = CreateRegexTesterMatch(codePieces[i]);
            }

            return matches;
        }

        private RegexTesterMatch CreateRegexTesterMatch(LinkedListNode<CodePiece> codePiece)
        {
            List<RegexTesterGroup> matchGroups = new List<RegexTesterGroup>();

            foreach (Group group in codePiece.Value.Match.Groups)
            {
                if (string.IsNullOrEmpty(group.Name) || Regex.IsMatch(group.Name, @"[0-9]+")) continue;
                matchGroups.Add(new RegexTesterGroup() { Name = group.Name, Value = group.Value });
            }

            return new RegexTesterMatch()
            {
                MatchText = codePiece.Value.Code,
                MatchGroups = matchGroups.ToArray()
            };
        }
        #endregion

        private void Search()
        {
            if (string.IsNullOrEmpty(values.text) || !CodeManagerUtility.IsValidRegex(values.regex))
            {
                regexMatches.Matches = new RegexTesterMatch[0];
                return;
            }

            codeInspection.SetEverything(values.text);
            codeInspection.Settings = GetCodeInspectionSettings();

            if (codeInspection.FindAll(values.regex, out currentFoundCodePieces))
            {
                searched = true;
                regexMatches.Matches = GetMatches(currentFoundCodePieces);
                CreateMatchesList();
            }
            else
            {
                regexMatches.Matches = new RegexTesterMatch[0];
            }
        }

        private string CreateRichText()
        {
            LinkedListNode<CodePiece> next = codeInspection.First;
            if (next == null) return values.text;

            StringBuilder sb = new StringBuilder();
            
            while(next != null)
            {
                if (currentFoundCodePieces.Contains(next))
                {
                    int colorCounter = -1;
                    string text = next.Value.Code;
                    foreach(Group group in next.Value.Match.Groups)
                    {
                        if (string.IsNullOrEmpty(group.Value)) continue;

                        if (text.Contains(group.Value))
                        {
                            string newText = "<color=" + colorArray[++colorCounter] + ">" + group.Value + "</color>";
                            text = text.Replace(group.Value, newText);
                        }

                        if(colorCounter == colorArray.Length - 1) colorCounter = -1;
                    }
                    sb.Append(text);
                }
                else
                {
                    sb.Append(next.Value.Code);
                }

                next = next.Next;
            }

            return sb.ToString();
        }

        private void CreateStyles()
        {
            if (regexAreaStyle == null)
            {
                regexAreaStyle = new GUIStyle("TextArea")
                {
                    wordWrap = true,
                };
            }

            if (textAreaStyle == null)
            {
                textAreaStyle = new GUIStyle("TextArea")
                {
                    fontSize = FONT_SIZE,
                    richText = true,
                    wordWrap = true,
                };

                textAreaStyle.normal.textColor = Color.white;
            }
        }

        #region Draw
        private void OnGUI()
        {
            CreateStyles();

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            EditorGUILayout.BeginHorizontal();

            bool preventSelection = (Event.current.type == EventType.MouseDown);
            Color oldCursorColor = GUI.skin.settings.cursorColor;
            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            EditorGUILayout.BeginVertical(); //Left side

            EditorGUILayout.LabelField("Regex", GUILayout.Width(50));

            EditorGUI.BeginChangeCheck();

            string regexTmp = EditorGUILayout.TextArea(values.regex, regexAreaStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Regex options", GUILayout.Width(100));
            values.regexOptions = (RegexOptions)EditorGUILayout.EnumFlagsField(values.regexOptions);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
            {
                Search();
            }
            EditorGUILayout.LabelField(new GUIContent("Auto search", "If this is active a search will be started automatically if the regex or text changes. Turn off for performance improvement"), GUILayout.Width(100));
            bool autoSearchTmp = EditorGUILayout.Toggle(values.autoSearch, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            string textTmp = EditorGUILayout.TextArea(values.text, textAreaStyle, GUILayout.ExpandHeight(true));

            if (searched)
            {
                Rect rect = GUILayoutUtility.GetLastRect();//get the text area position
                EditorGUI.TextArea(rect, CreateRichText(), textAreaStyle);//Rich text
            }

            //Check for changes
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(values, "Test Regex Window changes");

                //assign new values
                values.text = textTmp;
                values.regex = regexTmp;
                values.autoSearch = autoSearchTmp;

                //update search
                searched = false;
                if (values.autoSearch)
                    Search();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;

            if (matchesList != null && regexMatches.Matches.Length > 0)
            {

                EditorGUILayout.BeginVertical(GUILayout.Width(MATCH_FIELD_WIDTH)); //right side
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);

                matchesList.Expanded = EditorGUILayout.Foldout(matchesList.Expanded, "Matches");
                if (matchesList.Expanded)
                {
                    serializedMatches.Update();
                    matchesList.DoLayoutList();
                    serializedMatches.ApplyModifiedProperties();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
        #endregion
    }
}

#endif