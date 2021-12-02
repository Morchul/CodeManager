#if UNITY_EDITOR

using UnityEditor;

namespace Morchul.CodeManager {

    [CustomEditor(typeof(CleanCodeSettings))]
    public class CleanCodeSettingsInspector : Editor
    {
        public sealed override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Please change the settings of Clean Code under the menu: Code Manager => Clean Code => Settings.", MessageType.Info);
        }
    }
}

#endif
