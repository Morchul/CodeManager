#if UNITY_EDITOR

using UnityEditor;

namespace Morchul.CodeManager
{
    [CustomEditor(typeof(ScriptTemplateSettings))]
    public class ScriptTemplateSettingsInspector : Editor
    {
        public sealed override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Please change the settings of Script Templates under the menu: Code Manager => Script Templates => Settings.", MessageType.Info);
        }
    }
}

#endif
