using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class FolderScanner
    {
        private ScriptFolder scriptFolder;
        private CleanCodeSettings cleanCodeSettings;

        public FolderScanner(ScriptFolder scriptFolder, CleanCodeSettings cleanCodeSettings)
        {
            this.scriptFolder = scriptFolder;
            this.cleanCodeSettings = cleanCodeSettings;
        }

        public CleanCodeViolation[] Scan()
        {
            if (scriptFolder.ScanFor == 0) return null; //Do not scan this folder

            string[] scriptNames = Directory.GetFiles(CodeManagerUtility.ConvertToOpertingSystemPath(scriptFolder.Path), "*.cs", SearchOption.TopDirectoryOnly);
            for(int i = 0; i < scriptNames.Length; ++i)
            {
                CodeInspection scriptInspection = CodeInspector.InspectFile(scriptNames[i], InspectionMode.READ);
                Object scriptObj = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptNames[i]);

                if (scriptFolder.ScanFor.HasFlag(ScanForFlags.Unwanted_Code))
                {
                    ScanForUnwantedCode(scriptInspection, scriptObj);
                }

                if(scriptFolder.ScanFor.HasFlag(ScanForFlags.Code_Guidelines) || scriptFolder.ScanFor.HasFlag(ScanForFlags.Documentation_on_Methods))
                {
                    ScanForMethods(scriptInspection, scriptObj);
                }
            }

            return null;
        }

        private List<CleanCodeViolation> ScanForMethods(CodeInspection scriptInspection, Object scriptObj)
        {
            List<CleanCodeViolation> cleanCodeViolations = new List<CleanCodeViolation>();

            if (scriptInspection.FindAll(CodeManagerUtility.MethodRegex, out LinkedListNode<CodePiece>[] inspectionParts))
            {
                foreach (LinkedListNode<CodePiece> codePieceNode in inspectionParts)
                {
                    if (scriptFolder.ScanFor.HasFlag(ScanForFlags.Code_Guidelines))
                    {
                        string methodName = codePieceNode.Value.Match.Groups["method"].Value;
                        if (!Regex.IsMatch(methodName, cleanCodeSettings.CodingGuidelines.MethodNameRegex))
                        {
                            //CleanCodeViolation.Create
                        }
                    }

                    if (scriptFolder.ScanFor.HasFlag(ScanForFlags.Code_Guidelines))
                    {
                        string methodName = codePieceNode.Value.Match.Groups["method"].Value;
                        if (Regex.IsMatch(methodName, cleanCodeSettings.CodingGuidelines.MethodNameRegex))
                        {

                        }
                    }
                }
            }


            return cleanCodeViolations;
        }

        private List<CleanCodeViolation> ScanForUnwantedCode(CodeInspection scriptInspection, Object scriptObj)
        {
            List<CleanCodeViolation> cleanCodeViolations = new List<CleanCodeViolation>();
            foreach (UnwantedCode unwantedCode in cleanCodeSettings.UnwantedCodes)
            {
                if (scriptInspection.FindAll(unwantedCode.Regex, out LinkedListNode<CodePiece>[] inspectionParts))
                {
                    foreach(LinkedListNode<CodePiece> codePieceNode in inspectionParts)
                    {
                        cleanCodeViolations.Add(
                            CleanCodeViolation.CreateUnwantedCodeMessage(codePieceNode.Value.LineCount, scriptObj, CodeManagerEditorUtility.GetFileNameInPathWithExtension(scriptInspection.Path))
                        );
                    }
                }
            }
            return cleanCodeViolations;
        }
    }
}
