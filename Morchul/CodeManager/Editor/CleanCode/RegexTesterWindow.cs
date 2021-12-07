using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Morchul.CodeManager
{
    public class RegexTesterWindow : EditorWindow
    {
        private static RegexTesterWindow instance;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private CodeManagerSettings settings;

        private string regex;
        private string text;
        private RegexOptions regexOptions;

        private Color textColor = Color.white;
        private Color highlightColor = new Color(153f/255f, 81f/255f, 26f/255f, 0.5f);

        private Vector2 scrollPos;
        private Vector2 scrollPos2;

        private CodeInspection codeInspection;

        private int selectedRegexIndex;

        private RegexTesterMatches regexMatches;

        private RegexTesterMatch currentSelectedMatch;

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
            currentSelectedMatch = RegexTesterMatch.Null;

            settings = CodeManagerEditorUtility.LoadSettings();
            SelectRegex(selectedRegexIndex);

            regexMatches = ScriptableObject.CreateInstance<RegexTesterMatches>();

            if(codeInspection == null)
            {
                text = "";
                codeInspection = CodeInspector.InspectText(text);
            }
        }

        private void SelectRegex(int index)
        {
            if(settings != null && index >= 0 && index < settings.Regexes.Length)
            {
                regex = settings.Regexes[index].Regex;
            }
        }

        private void OnDisable()
        {
            if (selectedRegexIndex >= 0 && selectedRegexIndex < settings.Regexes.Length)
            {
                if (settings.Regexes[selectedRegexIndex].Regex != regex)
                {
                    if (EditorUtility.DisplayDialog("Replace regex?", "Should the selected Regex be replaced by the new regex?", "Yes", "No"))
                    {
                        if (selectedRegexIndex < settings.Regexes.Length)
                            settings.Regexes[selectedRegexIndex].Regex = regex;
                    }
                }
            }
        }

        #region Regex Matches
        private void Search()
        {
            if (string.IsNullOrEmpty(text)) return;

            codeInspection.SetEverything(text);
            codeInspection.Settings = GetCodeInspectionSettings();

            if(codeInspection.FindAll(regex, out LinkedListNode<CodePiece>[] codePieces))
            {
                regexMatches.Matches = GetMatches(codePieces);
            }
        }

        private CodeInspectionSettings GetCodeInspectionSettings()
        {
            return new CodeInspectionSettings()
            {
                AddLineIndex = true,
                RegexTimeout = System.TimeSpan.Zero,
                RegexOptions = regexOptions
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

            float LinePosIndex = 0;

            LinkedListNode<CodePiece> previous = codePiece.Previous;
            while (previous != null)
            {
                string previousCode = previous.Value.Code;
                int indexOfLastNewLine = previousCode.LastIndexOf("\n");
                string temp;
                if (indexOfLastNewLine >= 0)
                {
                    temp = previousCode.Substring(indexOfLastNewLine);                    
                    LinePosIndex += CodeManagerEditorUtility.CalcTextWidth(temp, GUI.skin.font);
                    break;
                }
                else
                {
                    temp = previousCode;
                    LinePosIndex += CodeManagerEditorUtility.CalcTextWidth(temp, GUI.skin.font);
                }
                previous = previous.Previous;

            }

            return new RegexTesterMatch()
            {
                MatchText = codePiece.Value.Code,
                LineIndex = codeInspection.GetLineIndex(codePiece),
                LinePosIndex = LinePosIndex,
                MatchGroups = matchGroups.ToArray()
            };
        }
        #endregion

        private GUIStyle GetTextAreaStyle()
        {
            GUIStyle style = new GUIStyle
            {
                imagePosition = ImagePosition.TextOnly,
                padding = new RectOffset(PADDING, PADDING, PADDING, PADDING),
            };
            style.normal.background = null;
            style.normal.textColor = textColor;
            style.active.background = null;
            style.active.textColor = textColor;
            style.hover.background = null;
            style.hover.textColor = textColor;
            style.focused.background = null;
            style.focused.textColor = textColor;
            return style;
        }

        #region Draw
        private void OnGUI()
        {

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            EditorGUILayout.BeginHorizontal();

            bool preventSelection = (Event.current.type == EventType.MouseDown);

            Color oldCursorColor = GUI.skin.settings.cursorColor;

            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            EditorGUILayout.BeginVertical(); //Left side

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Regex", GUILayout.Width(50));
            regex = EditorGUILayout.TextField(regex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Regex options", GUILayout.Width(100));
            regexOptions = (RegexOptions)EditorGUILayout.EnumFlagsField(regexOptions);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Search"))
            {
                Search();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical("TextArea");

            RegexTesterMatch selectedMatch = DrawMatches();
            if(!selectedMatch.IsNull())
            {
                currentSelectedMatch = selectedMatch;
            }

            text = EditorGUILayout.TextArea(text, GetTextAreaStyle(), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;

            if (!currentSelectedMatch.IsNull())
            {

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(300)); //right side
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);

                EditorGUILayout.LabelField("Match Text:");
                EditorGUILayout.LabelField(currentSelectedMatch.MatchText);
                EditorGUILayout.LabelField("Line Index:");
                EditorGUILayout.LabelField(currentSelectedMatch.LineIndex.ToString());

                if (currentSelectedMatch.MatchGroups.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Group name", GUILayout.MaxWidth(200));
                    EditorGUILayout.LabelField("Value", GUILayout.MaxWidth(200));
                    EditorGUILayout.EndHorizontal();
                    //Match Groups
                    foreach (RegexTesterGroup group in currentSelectedMatch.MatchGroups)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(group.Name, GUILayout.MaxWidth(200));
                        EditorGUILayout.LabelField(group.Value, GUILayout.MaxWidth(200));
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private const int LINE_HEIHGT = 15;
        private const int PADDING = 5;

        private RegexTesterMatch DrawMatches()
        {
            if (regexMatches.Matches == null || regexMatches.Matches.Length == 0) return RegexTesterMatch.Null;

            foreach(RegexTesterMatch match in regexMatches.Matches)
            {
                Vector2 size = new Vector2(CodeManagerEditorUtility.CalcTextWidth(match.MatchText, GUI.skin.font), 12);
                Rect rect = new Rect(match.LinePosIndex + (PADDING * 2), LINE_HEIHGT * (match.LineIndex - 1) + PADDING * 2, size.x, size.y);
                EditorGUI.DrawRect(rect, highlightColor);

                if(Event.current.type == EventType.MouseUp)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        return match;
                    }
                }
            }

            return RegexTesterMatch.Null;
        }
        #endregion
    }
}
