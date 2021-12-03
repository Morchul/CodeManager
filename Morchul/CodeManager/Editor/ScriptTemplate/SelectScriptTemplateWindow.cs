#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Morchul.CodeManager
{
    public class SelectScriptTemplateWindow : EditorWindow
    {
        private static SelectScriptTemplateWindow instance;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float SCRIPT_TEMPLATE_BOX_WIDTH = 200;
        private const float SCRIPT_TEMPLATE_BOX_HEIGHT = 40;

        private const float BORDER_WIDTH = 10;

        private Vector2 scrollPos;

        private ScriptTemplate[] scriptTemplates;

        private int selectedScriptTemplateIndex;

        private string path;

        //private Texture2D scriptTemplateImage;

        public static void ShowWindow(string path = "")
        {
            instance = GetWindow<SelectScriptTemplateWindow>();

            instance.path = path;

            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Select script template");
            instance.Show();
        }

        private ScriptTemplate[] LoadScriptTemplates()
        {
            if (!AssetDatabase.IsValidFolder(CodeManagerUtility.ScriptTemplateFolderPath))
            {
                return new ScriptTemplate[0];
            }
            string[] allTemplateFiles = Directory.GetFiles(CodeManagerUtility.ScriptTemplatePath, "*.txt", SearchOption.AllDirectories);
            List<ScriptTemplate> scriptTemplateList = new List<ScriptTemplate>();
            foreach (string templatePath in allTemplateFiles)
            {
                string correctTemplatePath = templatePath.Replace('\\', '/');
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(correctTemplatePath);
                if(textAsset == null)
                {
                    Debug.LogWarning("Can't load template file: " + correctTemplatePath);
                    continue;
                }
                string name = CodeManagerUtility.GetFileNameInPathWithoutExtension(correctTemplatePath);

                scriptTemplateList.Add(new ScriptTemplate(textAsset, name));
            }

            return scriptTemplateList.ToArray();
        }

        private void OnEnable()
        {
            scriptTemplates = LoadScriptTemplates();

            //scriptTemplateImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.ScriptTemplateImage);
        }

        private GUIContent[] GetScriptTemplateGUIContents()
        {
            GUIContent[] guiContents = new GUIContent[scriptTemplates.Length];
            for(int i = 0; i < guiContents.Length; ++i)
            {
                string templateName = scriptTemplates[i].TemplateName;
                guiContents[i] = new GUIContent(templateName, "Press to create a new script from  the " + templateName + " template.");
            }
            return guiContents;
        }

        #region Draw
        /*private void OnInspectorUpdate()
        {
            Repaint();
        }*/

        private void OnGUI()
        {
            if (scriptTemplates.Length == 0)
            {
                EditorGUILayout.HelpBox("No templates created yet.", MessageType.Info);
                return;
            }

            int amountOfBoxesInX = (int)((position.width - BORDER_WIDTH * 2) / SCRIPT_TEMPLATE_BOX_WIDTH);
            amountOfBoxesInX = Mathf.Max(1, Mathf.Min(amountOfBoxesInX, scriptTemplates.Length));

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUIContent[] guiContents = GetScriptTemplateGUIContents();
            if(guiContents.Length > 0)
                selectedScriptTemplateIndex = GUILayout.SelectionGrid(-1, guiContents, amountOfBoxesInX, GetScriptTemplateButtonGUIStyle());

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            SelectedItemEvent();
        }

        private void SelectedItemEvent()
        {
            if(selectedScriptTemplateIndex != -1)
            {
                CreateScriptWindow.ShowWindow(scriptTemplates[selectedScriptTemplateIndex], path);
                selectedScriptTemplateIndex = -1;
                instance.Close();
            }
        }

        private GUIStyle GetScriptTemplateButtonGUIStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.imagePosition = ImagePosition.ImageAbove;
            style.stretchWidth = true;
            style.fixedHeight = SCRIPT_TEMPLATE_BOX_HEIGHT;
            return style;
        }
        #endregion
    }
}
#endif