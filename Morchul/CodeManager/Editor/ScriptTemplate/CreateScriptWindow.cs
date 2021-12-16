#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Morchul.CodeManager
{
    /// <summary>
    /// Window where script name and creation path are defined and where the final create command is executed.
    /// </summary>
    public class CreateScriptWindow : EditorWindow, IHasCustomMenu
    {
        private static CreateScriptWindow instance;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private string scriptName;

        private string selectedFolderName;

        private string informationText;

        private ScriptFolder[] scriptFolders;

        private string path;

        private ScriptTemplate scriptTemplate;

        private string errorMessage;

        /// <summary>
        /// Method to show the window
        /// </summary>
        /// <param name="scriptTemplate">The Scripttemplate from which the script should be created</param>
        /// <param name="path">The path where the script should be created, can still be changed in window</param>
        public static void ShowWindow(ScriptTemplate scriptTemplate, string path)
        {
            instance = GetWindow<CreateScriptWindow>();

            instance.scriptTemplate = scriptTemplate;
            instance.path = path;

            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Create new script");
            instance.Show();
        }

        private void LoadScriptFolders()
        {
            errorMessage = "";
            CodeManagerSettings settings = CodeManagerEditorUtility.LoadSettings();
            if (settings == null)
            {
                errorMessage = "There are no settings created yet for Script Templates. Please open Window: Code Manager -> Script Templates -> Settings once to auto create settings.";
                Debug.LogError(errorMessage);
                return;
            }
            scriptFolders = settings.ScriptFolders;
            if(scriptFolders.Length == 0)
            {
                errorMessage = "There is no Scriptfolder created yet. Please create a script folder under: Code Manager -> Script Templates -> Settings.";
                Debug.LogError(errorMessage);
                return;
            }
        }

        private void OnEnable()
        {
            LoadScriptFolders();
            selectedFolderName = "";
            informationText = "";
        }

        #region Draw

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                return;
            }
            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Script name: ", "The name which the script will have and which will placed for the placeholder %ScriptName%"), GUILayout.Width(100));
            scriptName = EditorGUILayout.TextField(scriptName, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Select Folder: ", "Select a folder from Scriptfolder to create the script in."), GUILayout.Width(100));
            
            if (EditorGUILayout.DropdownButton(new GUIContent(selectedFolderName), FocusType.Passive))
            {
                GenericMenu foldersToSelect = new GenericMenu();
                for (int i = 0; i < scriptFolders.Length; ++i)
                {
                    if(CodeManagerEditorUtility.IsValidAssetsFolderPath(scriptFolders[i].Path))
                        foldersToSelect.AddItem(new GUIContent(scriptFolders[i].Name), false, OnFolderSelected, scriptFolders[i]);
                }
                foldersToSelect.DropDown(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Select a folder from above or input the path manually:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Creation Path: ", "The path where the new script will be created."), GUILayout.Width(100));
            path = EditorGUILayout.TextField(path, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (CodeManagerEditorUtility.IsValidFolderPath(path))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Create Script"), GUILayout.Height(30), GUILayout.Width(130)))
                {
                    CreateNewScript();
                }
                EditorGUILayout.LabelField(informationText);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Invalid path!", MessageType.Error);
            }

            EditorGUILayout.EndVertical();

            GUILayout.EndArea();
        }

        private void CreateNewScript()
        {
            if(string.IsNullOrEmpty(path))
            {
                informationText = "Invalid creation Path!";
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

            ScriptCreator.CreateNewScript(scriptName, scriptTemplate, path);
            instance.Close();
        }

        private void OnFolderSelected(object folder)
        {
            if (folder is ScriptFolder scriptFolder)
            {
                selectedFolderName = scriptFolder.Name;
                path = scriptFolder.Path;
                Repaint();
            }
        }
        #endregion

        // Add help menu
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            GUIContent content = new GUIContent("Help");
            menu.AddItem(content, false, HelpCallback);
        }

        private void HelpCallback()
        {
            CodeManagerEditorUtility.ShowHelpDialog(CodeManagerEditorUtility.NewScriptPageNumber);
        }
    }
}

#endif