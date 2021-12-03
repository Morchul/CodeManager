#if UNITY_EDITOR

using UnityEditor;

namespace Morchul.CodeManager
{
    [CustomEditor(typeof(CodeManagerSettings))]
    public class CodeManagerSettingsInspector : Editor
    {
        public sealed override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Please change the settings of Code Manager under the menu: Code Manager => Settings.", MessageType.Info);
        }
    }
}

#endif
