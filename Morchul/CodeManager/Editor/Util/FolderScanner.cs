#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// The FolderScanner is used to scan all scripts in a ScriptFolder for, in this folder defined, CleanCodeRules
    /// Then the FolderScanner will create and return every CleanCodeViolation found.
    /// </summary>
    public class FolderScanner
    {
        private ScriptFolder scriptFolder;
        private readonly CodeManagerSettings settings;

        private static Texture2D unwantedCodeImage;
        private static Texture2D codeGuidelineImage;
        private static Texture2D documentationImage;

        private readonly Dictionary<int, LinkedListNode<CodePiece>[]> searchedRegexes;

        public FolderScanner(CodeManagerSettings settings)
        {
            this.settings = settings;
            searchedRegexes = new Dictionary<int, LinkedListNode<CodePiece>[]>();
        }

        /// <summary>
        /// Scans every script in the Scriptfolder for every defined CleanCodeRule of this folder
        /// This will not scan scripts in sub-folders.
        /// </summary>
        /// <param name="scriptFolder">The Scriptfolder which should be scanned</param>
        /// <returns>All found CleanCodeViolations</returns>
        public CleanCodeViolation[] ScanFolder(ScriptFolder scriptFolder)
        {
            this.scriptFolder = scriptFolder;
            searchedRegexes.Clear();
            if (scriptFolder.CleanCodeRules.Count == 0 || settings.CleanCodeRules == null) return null; //Do not scan this folder

            List<CleanCodeViolation> ccvs = new List<CleanCodeViolation>();
            string[] scriptNames = GetAllSriptNames();
            CodeInspection scriptInspection;
            //For every script
            for (int i = 0; i < scriptNames.Length; ++i)
            {
                //Start CodeInspection
                scriptInspection = CodeInspector.InspectFile(scriptNames[i], InspectionMode.READ);
                scriptInspection.Settings = new CodeInspectionSettings()
                {
                    AddLineIndex = true,
                    RegexTimeout = System.TimeSpan.Zero,
                    RegexOptions = RegexOptions.None
                };

                Object scriptObj = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptNames[i]);

                CleanCodeViolation[] ccv;

                //Scan for every CleanCode rule
                foreach (uint ruleID in scriptFolder.CleanCodeRules)
                {
                    if(settings.CleanCodeRules.TryGetValue(ruleID, out ICleanCodeRule rule))
                    {
                        
                        switch (rule.GetType())
                        {
                            case ICleanCodeRule.CleanCodeRuleType.CodeDocumentation:
                                ccv = ScanCodeDocumentation((CodeDocumentation)rule, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                            case ICleanCodeRule.CleanCodeRuleType.UnwantedCode:
                                ccv = ScanUnwantedCode((UnwantedCode)rule, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                            case ICleanCodeRule.CleanCodeRuleType.CodingGuideline:
                                ccv = ScanCodeGuideline((CodeGuideline)rule, scriptInspection, scriptObj);
                                if (ccv != null)
                                    ccvs.AddRange(ccv);
                                break;
                        }
                    }
                }

                CodeInspector.StopFileInspection(scriptInspection);
                searchedRegexes.Clear();
            }

            return ccvs.ToArray();
        }

        /// <summary>
        /// Scans for unwanted code
        /// </summary>
        /// <param name="unwantedCode">The unwanted code rule</param>
        /// <param name="scriptInspection">The script inspection for the file</param>
        /// <param name="script">The script object which is inspected</param>
        /// <returns>All found CleanCodeViolations occured by violating the unwanted code rule</returns>
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

        /// <summary>
        /// Scans all Code documentation rules
        /// </summary>
        /// <param name="codeDocumentation">The code documentation rule</param>
        /// <param name="scriptInspection">The script inspection for the file</param>
        /// <param name="script">The script object which is inspected</param>
        /// <returns>All found CleanCodeViolations occured by violating the code documentation rule</returns>
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
                    if (codePiece.Previous == null || !Regex.IsMatch(codePiece.Previous.Value.Code, settings.DocumentationRegex.Regex))
                    {
                        ccvs.Add(CleanCodeViolation.CreateCodeDocumentationMessage(scriptInspection.GetLineIndex(codePieces[i]), script, codeDocumentation.Description, scriptInspection.FileName, GetCodeDocumentationImage()));
                    }
                }

                return ccvs.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Scans all Code guideline rules
        /// </summary>
        /// <param name="codeGuideline">The code guideline rule</param>
        /// <param name="scriptInspection">The script inspection for the file</param>
        /// <param name="script">The script object which is inspected</param>
        /// <returns>All found CleanCodeViolations occured by violating the code guideline rule</returns>
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

                    if (!Regex.IsMatch(groupValue, settings.Regexes[codeGuideline.MatchRegexIndex].Regex))
                    {
                        ccvs.Add(CleanCodeViolation.CreateCodeGuidelineMessage(scriptInspection.GetLineIndex(codePieces[i]), script, codeGuideline.Description, scriptInspection.FileName, GetCodeGuidelineImage()));
                    }
                }

                return ccvs.ToArray();
            }
            return null;
        }

        /// <summary>
        /// A FindAll wrapper to save already searched Regexes so you don't need to search the same regex twice
        /// </summary>
        /// <param name="regexIndex">Index of the regex in CleanCodeSettings.Regexes[]</param>
        /// <param name="scriptInspection">The script inspection for the file</param>
        /// <param name="searchResult">out all found results of the Regexes will be assingned to this array</param>
        /// <returns>True if something was found</returns>
        private bool FindAll(int regexIndex, CodeInspection scriptInspection, out LinkedListNode<CodePiece>[] searchResult)
        {
            if (searchedRegexes.TryGetValue(regexIndex, out searchResult))
            {
                return true;
            }

            if (scriptInspection.FindAll(settings.Regexes[regexIndex].Regex, out searchResult))
            {
                searchedRegexes.Add(regexIndex, searchResult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns all script name in this folder (No Sub-Directories)
        /// </summary>
        /// <returns>All script names with extension</returns>
        private string[] GetAllSriptNames()
        {
            SearchOption searchOption = scriptFolder.IncludeSubDirectory ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(CodeManagerUtility.ConvertToOpertingSystemPath(scriptFolder.Path), "*.cs", searchOption);
        }

        /// <summary>
        /// Returns the Image for UnwantedCode rule violation
        /// </summary>
        /// <returns>UnwantedCode rule violation image</returns>
        private Texture2D GetUnwantedCodeImage()
        {
            if(unwantedCodeImage == null)
                unwantedCodeImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.UnwantedCodeImagePath);
            return unwantedCodeImage;
        }

        /// <summary>
        /// Returns the Image for CodeDocumentation rule violation
        /// </summary>
        /// <returns>CodeDocumentation rule violation image</returns>
        private Texture2D GetCodeDocumentationImage()
        {
            if (documentationImage == null)
                documentationImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.DocumentationImagePath);
            return documentationImage;
        }

        /// <summary>
        /// Returns the Image for CodeGuideline rule violation
        /// </summary>
        /// <returns>CodeGuideline rule violation image</returns>
        private Texture2D GetCodeGuidelineImage()
        {
            if (codeGuidelineImage == null)
                codeGuidelineImage = AssetDatabase.LoadAssetAtPath<Texture2D>(CodeManagerEditorUtility.CodeGuidelineImagePath);
            return codeGuidelineImage;
        }
    }
}

#endif