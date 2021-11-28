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
        private readonly CleanCodeSettings cleanCodeSettings;

        private static Texture2D unwantedCodeImage;
        private static Texture2D codeGuidelineImage;
        private static Texture2D documentationImage;

        private readonly Dictionary<int, LinkedListNode<CodePiece>[]> searchedRegexes;

        public FolderScanner(CleanCodeSettings cleanCodeSettings)
        {
            this.cleanCodeSettings = cleanCodeSettings;
            searchedRegexes = new Dictionary<int, LinkedListNode<CodePiece>[]>();
        }

        public CleanCodeViolation[] ScanFolder(ScriptFolder scriptFolder)
        {
            this.scriptFolder = scriptFolder;
            searchedRegexes.Clear();

            if (scriptFolder.ScanFor.Count == 0 || cleanCodeSettings.scanables == null) return null; //Do not scan this folder

            List<CleanCodeViolation> ccvs = new List<CleanCodeViolation>();
            string[] scriptNames = GetAllSriptNames();

            //For every script
            for(int i = 0; i < scriptNames.Length; ++i)
            {
                //Start CodeInspection
                CodeInspection scriptInspection = CodeInspector.InspectFile(scriptNames[i], InspectionMode.READ);
                scriptInspection.Settings = new CodeInspectionSettings()
                {
                    AddLineIndex = true,
                    RegexTimeout = System.TimeSpan.Zero,
                    RegexOptions = RegexOptions.None
                };

                Object scriptObj = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptNames[i]);

                CleanCodeViolation[] ccv;

                //Scan for every scanable
                foreach (int scanableID in scriptFolder.ScanFor)
                {
                    if(cleanCodeSettings.scanables.TryGetValue(scanableID, out IScanable scanable))
                    {
                        
                        switch (scanable.GetType())
                        {
                            case IScanable.ScanableType.CodeDocumentation:
                                ccv = ScanCodeDocumentation((CodeDocumentation)scanable, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                            case IScanable.ScanableType.UnwantedCode:
                                ccv = ScanUnwantedCode((UnwantedCode)scanable, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                            case IScanable.ScanableType.CodingGuideline:
                                ccv = ScanCodeGuideline((CodeGuideline)scanable, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                        }
                    }
                }
            }

            return ccvs.ToArray();
        }

        private CleanCodeViolation[] ScanUnwantedCode(UnwantedCode unwantedCode, CodeInspection scriptInspection, Object script)
        {
            if(FindAll(unwantedCode.RegexIndex, scriptInspection, out LinkedListNode<CodePiece>[] codePieces))
            {
                if (codePieces == null) return null;

                CleanCodeViolation[] ccvs = new CleanCodeViolation[codePieces.Length];
                for(int i = 0; i < ccvs.Length; ++i)
                {
                    ccvs[i] = CleanCodeViolation.CreateUnwantedCodeMessage(scriptInspection.GetLineIndex(codePieces[i]), script, unwantedCode.Description, scriptInspection.FileName, GetUnwantedCodeImage());
                }

                return ccvs;
            }
            return null;
        }

        private CleanCodeViolation[] ScanCodeDocumentation(CodeDocumentation codeDocumentation, CodeInspection scriptInspection, Object script)
        {
            if (FindAll(codeDocumentation.RegexIndex, scriptInspection, out LinkedListNode<CodePiece>[] codePieces))
            {
                if (codePieces == null) return null;

                List<CleanCodeViolation> ccvs = new List<CleanCodeViolation>();
                for (int i = 0; i < codePieces.Length; ++i)
                {
                    //Check if documented
                    LinkedListNode<CodePiece> codePiece = codePieces[i];
                    if (codePiece.Previous == null || !Regex.IsMatch(codePiece.Previous.Value.Code, cleanCodeSettings.DocumentationRegex.Regex))
                    {
                        ccvs.Add(CleanCodeViolation.CreateCodeDocumentationMessage(scriptInspection.GetLineIndex(codePieces[i]), script, codeDocumentation.Description, scriptInspection.FileName, GetCodeDocumentationImage()));
                    }
                }

                return ccvs.ToArray();
            }
            return null;
        }

        private CleanCodeViolation[] ScanCodeGuideline(CodeGuideline codeGuideline, CodeInspection scriptInspection, Object script)
        {
            if (FindAll(codeGuideline.SearchRegexIndex, scriptInspection, out LinkedListNode<CodePiece>[] codePieces))
            {
                if (codePieces == null) return null;

                List<CleanCodeViolation> ccvs = new List<CleanCodeViolation>();
                for (int i = 0; i < codePieces.Length; ++i)
                {
                    //Check if documented
                    LinkedListNode<CodePiece> codePiece = codePieces[i];
                    string groupValue = codePiece.Value.Match.Groups[codeGuideline.GroupName].Value;

                    if (!Regex.IsMatch(groupValue, cleanCodeSettings.Regexes[codeGuideline.MatchRegexIndex].Regex))
                    {
                        ccvs.Add(CleanCodeViolation.CreateCodeGuidelineMessage(scriptInspection.GetLineIndex(codePieces[i]), script, codeGuideline.Description, scriptInspection.FileName, GetCodeGuidelineImage()));
                    }
                }

                return ccvs.ToArray();
            }
            return null;
        }

        //A FindAll wrapper to save already searched Regexes so you don't need to search the same regex twice
        private bool FindAll(int regexIndex, CodeInspection scriptInspection, out LinkedListNode<CodePiece>[] searchResult)
        {
            if (searchedRegexes.TryGetValue(regexIndex, out searchResult))
            {
                return true;
            }

            if (scriptInspection.FindAll(cleanCodeSettings.Regexes[regexIndex].Regex, out searchResult))
            {
                searchedRegexes.Add(regexIndex, searchResult);
                return true;
            }

            return false;
        }

        private string[] GetAllSriptNames()
        {
            return Directory.GetFiles(CodeManagerUtility.ConvertToOpertingSystemPath(scriptFolder.Path), "*.cs", SearchOption.TopDirectoryOnly);
        }

        private Texture2D GetUnwantedCodeImage()
        {
            if(unwantedCodeImage == null)
                unwantedCodeImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.UnwantedCodeImage);
            return unwantedCodeImage;
        }

        private Texture2D GetCodeDocumentationImage()
        {
            if (documentationImage == null)
                documentationImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.DocumentationImage);
            return documentationImage;
        }

        private Texture2D GetCodeGuidelineImage()
        {
            if (codeGuidelineImage == null)
                codeGuidelineImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.CodeGuidelineImage);
            return codeGuidelineImage;
        }
    }
}
