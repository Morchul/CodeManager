#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class SelectCleanCodeRulesWindow : EditorWindow
    {
        private static SelectCleanCodeRulesWindow instance;

        private CodeManagerSettings settings;

        private Vector2 scrollPos;

        private const float cleanCodeRuleFieldWidth = 250;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private static ScriptFolder scriptFolder;

        public static void ShowWindow(ScriptFolder scriptFolder)
        {
            SelectCleanCodeRulesWindow.scriptFolder = scriptFolder;
            instance = CreateInstance<SelectCleanCodeRulesWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Clean Code Rules");
            instance.Show();
        }

        private void OnEnable()
        {
            settings = CodeManagerEditorUtility.LoadSettings();

            RemoveNotExistingCleanCodeRules();
        }

        private void RemoveNotExistingCleanCodeRules()
        {
            if (settings == null || scriptFolder == null || scriptFolder.CleanCodeRules == null) return;
            if (settings.CleanCodeRules.Count == 0) 
            {
                scriptFolder.CleanCodeRules.Clear();
                return;
            }

            for (int i = 0; i < scriptFolder.CleanCodeRules.Count; ++i)
            {
                if (!settings.CleanCodeRules.Keys.Contains(scriptFolder.CleanCodeRules[i]))
                {
                    scriptFolder.CleanCodeRules.RemoveAt(i);
                }
            }
        }

        #region Draw

        private void OnGUI()
        {
            if (settings == null || settings.CleanCodeRules.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no CleanCode rules created yet. You can create some in the Clean Code settings.", MessageType.Warning);
                return;
            }

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            int amountOfBoxesInX = (int)((position.width - BORDER_WIDTH * 2) / cleanCodeRuleFieldWidth);
            amountOfBoxesInX = Mathf.Max(1, Mathf.Min(amountOfBoxesInX, settings.CleanCodeRules.Count));

            int boxesInXCounter = 0;

            int amountOfBoxesInY = settings.CleanCodeRules.Count / amountOfBoxesInX + 1;
            ICleanCodeRule.CleanCodeRuleType currentType = ICleanCodeRule.CleanCodeRuleType.None;
            for(int y = 0; y < amountOfBoxesInY; ++y)
            {
                for(int x = 0; x < amountOfBoxesInX; ++x)
                {
                    if (boxesInXCounter == 0)
                        EditorGUILayout.BeginHorizontal();

                    int index = x + y * amountOfBoxesInX;
                    if (index < settings.CleanCodeRules.Count)
                    {
                        ICleanCodeRule rules = settings.CleanCodeRules.ElementAt(index).Value;
                        if(currentType != rules.GetType())
                        {
                            EditorGUILayout.EndHorizontal();
                            //New Type
                            boxesInXCounter = 0;
                            currentType = rules.GetType();
                            EditorGUILayout.LabelField(currentType.ToString());
                            EditorGUILayout.BeginHorizontal();
                        }

                        #region CleanCodeRuleBox
                        EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(cleanCodeRuleFieldWidth));
                        EditorGUILayout.LabelField(rules.GetName(), GUILayout.MaxWidth(cleanCodeRuleFieldWidth - 30));
                        bool selected = scriptFolder.CleanCodeRules.Contains(rules.GetID());
                        if (EditorGUILayout.Toggle(selected))
                        {
                            if (!selected)
                            {
                                scriptFolder.CleanCodeRules.Add(rules.GetID());
                                EditorUtility.SetDirty(settings);
                            }
                        }
                        else if(selected)
                        {
                            scriptFolder.CleanCodeRules.Remove(rules.GetID());
                            EditorUtility.SetDirty(settings);
                        }
                        EditorGUILayout.EndHorizontal();
                        #endregion

                        ++boxesInXCounter;

                        if (boxesInXCounter == amountOfBoxesInX)
                        {
                            EditorGUILayout.EndHorizontal();
                            boxesInXCounter = 0;
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        #endregion
    }
}
#endif
