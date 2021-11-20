#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Morchul.CodeManager
{
    public class MenuItems
    {
        [MenuItem("Code Manager/Clean Code/Manager %m")]
        private static void OpenCleanCodeManager()
        {

        }

        [MenuItem("Code Manager/Clean Code/Settings")]
        private static void OpenCleanCodeSettings()
        {
            CleanCodeSettingsWindow.ShowWindow();
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
            if (!CodeManagerUtility.IsValidScriptTemplateFolderPath(folderPath))
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

        [MenuItem("Code Manager/Script Templates/Settings")]
        private static void OpenScriptTemplateSettings()
        {
            ScriptTemplateSettingsWindow.ShowWindow();
        }
    }
}
#endif

