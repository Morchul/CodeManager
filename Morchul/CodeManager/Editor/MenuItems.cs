#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Morchul.CodeManager
{
    //All MenuItems under CodeManager
    public class MenuItems
    {
        [MenuItem("Assets/Create/Script from template", priority = 1)]
        private static void CreateScript()
        {
            SelectScriptTemplateWindow.ShowWindow(AssetDatabase.GetAssetPath(Selection.activeObject) + "/");
        }

        [MenuItem("Assets/Create/Script from template", validate = true)]
        private static bool CreateScriptValidation()
        {
            return CodeManagerEditorUtility.IsValidFolderPath(AssetDatabase.GetAssetPath(Selection.activeObject) + "/");
        }


        [MenuItem("Code Manager/Clean Code/Console %m")]
        private static void OpenCleanCodeManager()
        {
            CleanCodeConsole.ShowWindow();
        }

        [MenuItem("Code Manager/Settings")]
        private static void OpenCodeManagerSettings()
        {
            CodeManagerSettingsWindow.ShowWindow();
        }

        [MenuItem("Code Manager/Script Templates/New script template")]
        private static void CreateNewScriptTemplate()
        {
            //Check if ScriptTemplate folder exist if not create folder
            if (!AssetDatabase.IsValidFolder(CodeManagerUtility.ScriptTemplateFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", CodeManagerUtility.ScriptTemplateFolder);
            }

            //Select path to Create script template
            string path = EditorUtility.SaveFilePanelInProject("New Script template", "ScriptTemplate", "txt", "Input the name of the script template.", CodeManagerUtility.ScriptTemplatePath);
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.Refresh();

            string folderPath = CodeManagerUtility.GetFolderNameInPath(path);

            //Script template must be created in the ScriptTemplate folder
            if (!CodeManagerEditorUtility.IsValidScriptTemplateFolderPath(folderPath))
            {
                Debug.LogError("A ScriptTemplate has to be under the Folder \"" + CodeManagerUtility.ScriptTemplatePath + "\".");
                return;
            }

            //Create new template
            ScriptCreator.CreateNewScriptTemplate(path);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Code Manager/Script Templates/New script %t")]
        private static void OpenCreateNewScriptWindow()
        {
            SelectScriptTemplateWindow.ShowWindow();
        }
    }
}
#endif

