#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Morchul.CodeManager
{
    public class CleanCodeConsole : EditorWindow
    {
        private static CleanCodeConsole instance;

        private CodeManagerSettings settings;

        private FolderScanner folderScanner;

        private List<CleanCodeViolation> cleanCodeViolations;

        private Vector2 scrollPos;

        private int selectedScriptTemplateIndex;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private const float LINE_HEIGHT = 50;

        private bool cleared;

        public static void ShowWindow()
        {
            instance = CreateInstance<CleanCodeConsole>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("CleanCode Console");
            instance.Show();
        }

        private void OnEnable()
        {
            settings = CodeManagerEditorUtility.LoadSettings();
            selectedScriptTemplateIndex = -1;

            if (cleanCodeViolations == null)
            {
                cleanCodeViolations = new List<CleanCodeViolation>();
            }

            if(settings != null && folderScanner == null)
            {
                folderScanner = new FolderScanner(settings);
            }

            if(settings != null && folderScanner != null)
            {
                settings.AddReadyListener(ScanFolders);
            }
        }
        
        private void ScanFolders()
        {
            cleared = false;
            if (folderScanner != null)
            {
                cleanCodeViolations.Clear();
                foreach (ScriptFolder folder in settings.ScriptFolders)
                {
                    CleanCodeViolation[] ccvs = folderScanner.ScanFolder(folder);
                    if(ccvs != null)
                        cleanCodeViolations.AddRange(ccvs);
                }
            }
        }

        private void Clear()
        {
            cleared = true;
            cleanCodeViolations.Clear();
        }

        #region Draw
        private void OnGUI()
        {
            if(settings == null)
            {
                EditorGUILayout.HelpBox("ScriptTemplateSettings and CleanCodeSettings must be created. You can auto create the settings by once open the setting windows of CleanCode and ScriptTemplates.", MessageType.Warning);
                OnEnable();
                return;
            }

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Scan", "Press this button to scan all ScriptFolders for CleanCode violations")))
            {
                ScanFolders();
            }
            if (GUILayout.Button(new GUIContent("Clear", "Clear the CleanCode Console")))
            {
                Clear();
            }
            EditorGUILayout.EndHorizontal();

            //Draw CleanCodeViolations
            GUIContent[] guiContents = GetScriptTemplateGUIContents();
            if (guiContents.Length > 0)
                selectedScriptTemplateIndex = GUILayout.SelectionGrid(-1, guiContents, 1, GetScriptTemplateButtonGUIStyle());
            else if(!cleared)
                EditorGUILayout.HelpBox("No Clean Code violations found.", MessageType.Info);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            SelectedItemEvent();
        }

        private GUIContent[] GetScriptTemplateGUIContents()
        {
            GUIContent[] guiContents = new GUIContent[cleanCodeViolations.Count];
            for (int i = 0; i < guiContents.Length; ++i)
            {
                guiContents[i] = new GUIContent(cleanCodeViolations[i].ErrorMessage + "\n" + cleanCodeViolations[i].Description, cleanCodeViolations[i].Image);
            }
            return guiContents;
        }

        private GUIStyle GetScriptTemplateButtonGUIStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleLeft;
            style.imagePosition = ImagePosition.ImageLeft;
            style.fixedHeight = LINE_HEIGHT;
            return style;
        }
        #endregion

        private void SelectedItemEvent()
        {
            if (selectedScriptTemplateIndex != -1)
            {
                AssetDatabase.OpenAsset(cleanCodeViolations[selectedScriptTemplateIndex].Script, cleanCodeViolations[selectedScriptTemplateIndex].LineIndex);
                selectedScriptTemplateIndex = -1;
            }
        }
    }
}

#endif