using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class SelectScansWindow : EditorWindow
    {
        private static SelectScansWindow instance;

        private CleanCodeSettings settings;

        private Vector2 scrollPos;

        private const float scanableFieldWidth = 250;

        private const float MIN_WIDTH = 200;
        private const float MIN_HEIGHT = 150;

        private const float BORDER_WIDTH = 10;

        private static ScriptFolder scriptFolder;

        public static void ShowWindow(ScriptFolder scriptFolder)
        {
            SelectScansWindow.scriptFolder = scriptFolder;
            instance = CreateInstance<SelectScansWindow>();
            instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            instance.titleContent = new GUIContent("Script template settings");
            instance.Show();
        }

        private void OnEnable()
        {
            LoadSettings();

            RemoveNotExistingScanables();
        }

        private void RemoveNotExistingScanables()
        {
            if (settings == null || scriptFolder == null || scriptFolder.ScanFor == null) return;
            if (settings.scanables.Count == 0) 
            {
                scriptFolder.ScanFor.Clear();
                return;
            }

            for (int i = 0; i < scriptFolder.ScanFor.Count; ++i)
            {
                if (!settings.scanables.Keys.Contains(scriptFolder.ScanFor[i]))
                {
                    scriptFolder.ScanFor.RemoveAt(i);
                }
            }
        }

        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<CleanCodeSettings>(CodeManagerUtility.CleanCodeSettingsObject);
            if (settings == null)
            {
                return;
            }
        }

        #region Draw

        private void OnGUI()
        {
            if (settings == null || settings.scanables.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no Scanables created yet. You can create some in the Clean Code settings.", MessageType.Warning);
                return;
            }

            GUILayout.BeginArea(new Rect(BORDER_WIDTH, BORDER_WIDTH, position.width - BORDER_WIDTH * 2, position.height - BORDER_WIDTH * 2));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            int amountOfBoxesInX = (int)((position.width - BORDER_WIDTH * 2) / scanableFieldWidth);
            amountOfBoxesInX = Mathf.Max(1, Mathf.Min(amountOfBoxesInX, settings.scanables.Count));

            int boxesInXCounter = 0;

            int amountOfBoxesInY = settings.scanables.Count / amountOfBoxesInX + 1;
            IScanable.ScanableType currentType = IScanable.ScanableType.None;
            for(int y = 0; y < amountOfBoxesInY; ++y)
            {
                for(int x = 0; x < amountOfBoxesInX; ++x)
                {
                    if (boxesInXCounter == 0)
                        EditorGUILayout.BeginHorizontal();

                    int index = x + y * amountOfBoxesInX;
                    if (index < settings.scanables.Count)
                    {
                        IScanable scanable = settings.scanables.ElementAt(index).Value;
                        if(currentType != scanable.GetType())
                        {
                            EditorGUILayout.EndHorizontal();
                            //New Type
                            boxesInXCounter = 0;
                            currentType = scanable.GetType();
                            EditorGUILayout.LabelField(currentType.ToString());
                            EditorGUILayout.BeginHorizontal();
                        }

                        #region ScanableBox
                        EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(scanableFieldWidth));
                        EditorGUILayout.LabelField(scanable.GetName(), GUILayout.MaxWidth(scanableFieldWidth - 30));
                        bool selected = scriptFolder.ScanFor.Contains(scanable.GetID());
                        if (EditorGUILayout.Toggle(selected))
                        {
                            if (!selected)
                            {
                                scriptFolder.ScanFor.Add(scanable.GetID());
                                EditorUtility.SetDirty(settings);
                            }
                        }
                        else if(selected)
                        {
                            scriptFolder.ScanFor.Remove(scanable.GetID());
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
