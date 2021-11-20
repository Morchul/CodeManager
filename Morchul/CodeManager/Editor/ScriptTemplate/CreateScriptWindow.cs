#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Morchul.CodeManager
{
    public class CreateScriptWindow : EditorWindow
    {
        private static CreateScriptWindow instance;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 140;

        private const float BORDER_WIDTH = 10;

        private string scriptName;

        private string selectedFolderName;

        private string informationText;

        private ScriptFolder[] scriptFolders;

        private ScriptFolder selectedScriptFolder;

        private static ScriptTemplate scriptTemplate;

        private bool closeWindow;

        public static void ShowWindow(ScriptTemplate scriptTemplate)
        {
            CreateScriptWindow.scriptTemplate = scriptTemplate;
            instance = CreateInstance<CreateScriptWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Create new script");
            instance.Show();
        }

        private void LoadScriptFolders()
        {
            ScriptTemplateSettings settings = AssetDatabase.LoadAssetAtPath<ScriptTemplateSettings>(CodeManagerUtility.ScriptTemplateSettingsObject);
            if (settings == null)
            {
                Debug.LogError("There are no settings created yet for Script Templates. Please open Window: Code Manager -> Script Templates -> Settings once to auto create settings.");
                closeWindow = true;
                return;
            }
            scriptFolders = settings.ScriptFolders;
            if(scriptFolders.Length == 0)
            {
                Debug.LogError("There is no Scriptfolder created yet. Please create a script folder under: Code Manager -> Script Templates -> Settings.");
                closeWindow = true;
                return;
            }
        }

        private void OnEnable()
        {
            LoadScriptFolders();
            selectedFolderName = "SelectFolder";
            informationText = "";
        }

        #region Draw
        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (closeWindow)
            {
                instance.Close();
                return;
            }
            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Script name: ", GUILayout.Width(100));
            scriptName = EditorGUILayout.TextField(scriptName, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Folder: ", GUILayout.Width(100));
            
            if (EditorGUILayout.DropdownButton(new GUIContent(selectedFolderName), FocusType.Passive))
            {
                GenericMenu foldersToSelect = new GenericMenu();
                for (int i = 0; i < scriptFolders.Length; ++i)
                {
                    if(CodeManagerUtility.IsValidAssetsFolderPath(scriptFolders[i].Path))
                        foldersToSelect.AddItem(new GUIContent(scriptFolders[i].Name), false, OnFolderSelected, scriptFolders[i]);
                }
                foldersToSelect.DropDown(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();
            

            Rect buttonRect = new Rect(0, BORDER_WIDTH + EditorGUIUtility.singleLineHeight * 3, 150, 30);
            if (GUI.Button(buttonRect, new GUIContent("Create Script")))
            {
                CreateNewScript();
            }

            Rect informationRect = new Rect(0, buttonRect.y + 40, position.width - BORDER_WIDTH * 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(informationRect, informationText);

            GUILayout.EndArea();
        }

        private void CreateNewScript()
        {
            if(selectedScriptFolder.IsNull())
            {
                informationText = "No Folder selected!";
                Debug.LogWarning(informationText);
                return;
            }

            if (string.IsNullOrEmpty(scriptName))
            {
                informationText = "Script name is missing!";
                Debug.LogWarning(informationText);
                return;
            }

            if (!Regex.IsMatch(scriptName, @"^[_a-zA-Z][_a-zA-Z0-9]*")) //Test if scriptName is valid
            {
                informationText = "Script name is invalid!";
                Debug.LogWarning(informationText);
                return;
            }

            ScriptCreator.CreateNewScript(scriptName, scriptTemplate, selectedScriptFolder);
            instance.Close();
        }

        private void OnFolderSelected(object folder)
        {
            if (folder is ScriptFolder scriptFolder)
            {
                selectedFolderName = scriptFolder.Name;
                selectedScriptFolder = scriptFolder;
            }
        }
        #endregion
    }
}

#endif