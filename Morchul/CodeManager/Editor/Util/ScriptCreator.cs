#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// ScriptCreator is an utility class to assist in the creation of script templates and scripts from these templates.
    /// The replacement of placeholder will also be done within the ScriptCreator
    /// </summary>
    public static class ScriptCreator
    {
        private readonly static string[] defaultPlaceholders = new string[] { "ScriptName", "TemplateName" };

        /// <summary>
        /// Tests if the placeholderName is in the defaultPlaceholders
        /// </summary>
        /// <param name="placeholderName">The placeholderName</param>
        /// <returns>True if the placeholderName is in the defaultPlaceholders</returns>
        public static bool IsDefaultPlaceholderName(string placeholderName)
        {
            for(int i = 0; i < defaultPlaceholders.Length; ++i)
            {
                if (defaultPlaceholders[i] == placeholderName)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Create a new ScriptTemplate (.txt) file
        /// </summary>
        /// <param name="filePath">The file path of the script template which should be created</param>
        /// <returns>True if the creation was succesfful</returns>
        public static bool CreateNewScriptTemplate(string filePath)
        {
            if (File.Exists(filePath))
            {
                Debug.LogError("The File already Exists!");
                return false;
            }
            else
            {
                using (StreamWriter outfile = new StreamWriter(filePath))
                {
                    outfile.WriteLine("using UnityEngine;");
                    outfile.WriteLine("using System.Collections;");
                    outfile.WriteLine("");
                    outfile.WriteLine("public class %ScriptName% : MonoBehaviour");
                    outfile.WriteLine("{");
                    outfile.WriteLine("");
                    outfile.WriteLine("\t// Use this for initialization");
                    outfile.WriteLine("\tvoid Start () {");
                    outfile.WriteLine("\t");
                    outfile.WriteLine("\t}");
                    outfile.WriteLine("");
                    outfile.WriteLine("");
                    outfile.WriteLine("\t// Update is called once per frame");
                    outfile.WriteLine("\tvoid Update () {");
                    outfile.WriteLine("");
                    outfile.WriteLine("\t}");
                    outfile.WriteLine("}");
                }
                return true;
            }
        }

        /// <summary>
        /// Create a new C# Script from a ScriptTemplate in ScriptFolder with the Name: scriptName.cs
        /// </summary>
        /// <param name="scriptName">The name of the file and class</param>
        /// <param name="scriptTemplate">The script template from which the script will be created</param>
        /// <param name="scriptFolder">The scriptfolder in which the script will be created</param>
        public static void CreateNewScript(string scriptName, ScriptTemplate scriptTemplate, string path)
        {
            path += scriptName + ".cs";
            if (File.Exists(path))
            {
                Debug.LogError("The File already Exists!");
            }
            else
            {
                //Replace all placeholders
                CodeInspection codeInspection = CodeInspector.InspectText(scriptTemplate.Template.text);
                codeInspection.FindAll(@"%.*%", out LinkedListNode<CodePiece>[] foundPlaceholders);

                if (foundPlaceholders.Length > 0)
                {
                    if (!LoadPlaceholderValues(out Placeholder[] placeholderValues, scriptName, scriptTemplate.TemplateName))
                    {
                        Debug.LogError("Can't create script. See Error message above.");
                        return;
                    }

                    for (int i = 0; i < foundPlaceholders.Length; ++i)
                    {
                        CodePiece inspectionPart = foundPlaceholders[i].Value;

                        string placeHolderName = inspectionPart.Code.Substring(1, inspectionPart.Code.Length - 2);
                        string placeHolderValue = FindPlaceholderValue(placeHolderName, placeholderValues);
                        if (string.IsNullOrEmpty(placeHolderValue))
                        {
                            Debug.LogError("Can't create script. Can't find a placeholder value for: " + placeHolderName);
                            return;
                        }
                        inspectionPart.Code = placeHolderValue;
                    }

                    codeInspection.Commit();
                }

                using (StreamWriter outfile = new StreamWriter(path))
                {
                    outfile.WriteLine(codeInspection.CompleteCode);
                }

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                Debug.Log("Script: " + path + " created");
            }
        }

        /// <summary>
        /// Load the Placeholder values from the ScriptTemplateSettings and defaultPlaceholders
        /// Placeholders from ScripTemplateSettings which are the same as defaultPlaceholders will not be added.
        /// </summary>
        /// <param name="placeholders">all placeholders found in the ScriptTemplateSettings and defaultPlaceholders</param>
        /// <returns>true if settings could be loaded</returns>
        private static bool LoadPlaceholderValues(out Placeholder[] placeholders, params string[] defaultValues)
        {
            CodeManagerSettings settings = CodeManagerEditorUtility.LoadSettings();
            if (settings == null)
            {
                Debug.LogError("There are no settings created yet for Script Templates. Please open Window: Code Manager -> Script Templates -> Settings once to auto create settings.");
                placeholders = new Placeholder[0];
                return false;
            }

            placeholders = new Placeholder[settings.Placeholders.Length + defaultPlaceholders.Length];
            for (int i = 0; i < settings.Placeholders.Length; ++i)
            {
                if (!IsDefaultPlaceholderName(settings.Placeholders[i].Name))
                    placeholders[i] = settings.Placeholders[i];
            }

            for (int i = 0; i < defaultValues.Length; ++i)
            {
                placeholders[i + settings.Placeholders.Length] = new Placeholder() { Value = defaultValues[i], Name = defaultPlaceholders[i] };
            }

            return true;
        }

        /// <summary>
        /// Find the value of a placeholder name
        /// </summary>
        /// <param name="placeHolderName">The name of the placeholder</param>
        /// <param name="placeholders">Placeholder array in which the placeholder will be searched.</param>
        /// <returns>The placeholder value if found else null</returns>
        private static string FindPlaceholderValue(string placeHolderName, Placeholder[] placeholders)
        {
            foreach (Placeholder ph in placeholders)
            {
                if (ph.Name == placeHolderName)
                    return ph.Value;
            }
            return null;
        }
    }
}

#endif