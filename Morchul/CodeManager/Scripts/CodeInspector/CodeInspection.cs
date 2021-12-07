using System;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

namespace Morchul.CodeManager
{
    /// <summary>
    /// CodeInspection is a Handler for one File or Text to search, read and write.
    /// It can be handeld like a LinkedList. When you Search for a Match the code will be splittet and added to a LinkedList you then receive a Node with which you can change the Code.
    /// You create CodePieces from the CodeInspection to then change the underlining File content or Text with the adjusted CodePiece.
    /// If you are in READ_WRITE mode there can always be only one CodePiece active. Means after a Find() FindAll() or GetEverything() call you have to call Commit() or Cancel() before calling any of the three methods again.
    /// </summary>
    public class CodeInspection
    {
        public InspectionMode Mode { get; private set; }
        public InspectionType Type { get; private set; }

        private CodeInspectionSettings settings;
        public CodeInspectionSettings Settings
        {
            get => settings;
            set
            {
                if (IsNoEditationActive())
                {
                    settings = value;
                }
            }
        }


        public string Path { get; private set; }
        public string FileName { get; private set; }

        public string CompleteCode { get; private set; }

        public uint CodeInspectionID { get; private set; }

        private bool editationActive = false;

        private readonly LinkedList<CodePiece> codePiecesList;

        public LinkedListNode<CodePiece> First => codePiecesList.First;
        public LinkedListNode<CodePiece> Last => codePiecesList.Last;

        internal CodeInspection(uint codeInspectionID, InspectionMode mode, InspectionType type, string pathOrText)
        {
            this.settings = CodeInspectionSettings.Default;
            CodeInspectionID = codeInspectionID;
            Mode = mode;
            Type = type;

            codePiecesList = new LinkedList<CodePiece>();

            if (pathOrText != null)
            {
                if (Type == InspectionType.FILE)
                {
                    if (pathOrText != "")
                    {
                        Path = pathOrText;
                        FileName = CodeManagerUtility.GetFileNameInPathWithExtension(Path);
                        if (!ReadFile(Path))
                        {
                            throw new ArgumentException("The file: " + pathOrText + " could not be read!");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("The Parameter pathOrText can not be null or empty if the InspectionType is File!");
                    }
                }
                else if (Type == InspectionType.TEXT)
                {
                    CompleteCode = pathOrText;
                }
            }
            else
            {
                throw new ArgumentException("The Parameter pathOrText can not be null!");
            }
                
        }

        /// <summary>
        /// Commits all changes and let you create new CodePieces if your in READ_WRITE mode
        /// Writes the changes to the File or Text
        /// </summary>
        /// <returns>True if the changes have been successfully written</returns>
        public bool Commit()
        {
            if(Mode == InspectionMode.READ)
            {
                ResetCodeInspection();
                return true;
            }
            else
            {
                if (!HasAllWriteRequirements()) return false;

                string newCode = CreateCurrentCode();

                bool success = WriteNewContent(newCode);

                if (success)
                {
                    ResetCodeInspection();
                }
                return success;
            }
        }

        /// <summary>
        /// Discards all changes and let you create new CodePieces if your in READ_WRITE mode
        /// </summary>
        public void Cancel()
        {
            ResetCodeInspection();
        }

        /// <summary>
        /// Returns the current code with all changes but does not write the changes to the file or text
        /// Can be used to check if all changes were made before commiting and writting to a file.
        /// </summary>
        /// <returns>The current Code with all changes</returns>
        public string CreateCurrentCode()
        {
            StringBuilder sb = new StringBuilder();
            
            LinkedListNode<CodePiece> codePiece = codePiecesList.First;
            do
            {
                if (!string.IsNullOrEmpty(codePiece.Value.Code))
                    sb.Append(codePiece.Value.Code);

                codePiece = codePiece.Next;

            } while (codePiece != null);

            return sb.ToString();
        }

        /// <summary>
        /// Returns the Line index of the first line of the codePiece
        /// </summary>
        /// <param name="codePieceNode">The CodePiece node</param>
        /// <returns>index of the Line where the first line of the codePiece occurs</returns>
        public int GetLineIndex(LinkedListNode<CodePiece> codePieceNode)
        {
            if (Settings.AddLineIndex)
            {
                int lineCounter = 1;
                LinkedListNode<CodePiece> previous = codePieceNode.Previous;
                //Add the LineCount of previous codePieces.
                while (previous != null)
                {
                    //If the previous does not end with new line add one line less than counted
                    lineCounter += previous.Value.EndWithNewLine ? previous.Value.LineCount : previous.Value.LineCount -1;
                    previous = previous.Previous;
                }
                return lineCounter;
            }
            else
            {
                Debug.LogWarning("Line index can not be calculated. Please set settings.AddLineIndex to true if you want to use the GetLineIndex or CodeLineCount");
                return -1;
            }
        }

        #region WRITE methods
        /// <summary>
        /// Sets the whole code. Works only for InspectionType Text, not for file. The purpose is to reuse the CodeInspection instance for text strings.
        /// </summary>
        /// <param name="newCode">The new Text</param>
        /// <returns>True if InspectionType is Text else false</returns>
        public bool SetEverything(string newCode)
        {
            if(Type == InspectionType.FILE)
            {
                Debug.LogError("SetEverything only works for inspection of Texts to reuse the same instance of CodeInspection. For File you have GetEverything change the Code and Commit.");
                return false;
            }

            ResetCodeInspection();
            CompleteCode = newCode;
            return true;
        }

        /// <summary>
        /// Adds a codePiece as First item in the code
        /// </summary>
        /// <param name="newCodePiece">The code piece</param>
        /// <returns>The new CodePiece node</returns>
        public LinkedListNode<CodePiece> AddFirst(string newCodePiece)
        {
            if (!HasAllWriteRequirements()) return null;
            return codePiecesList.AddFirst(CreateCodePiece(newCodePiece));
        }

        /// <summary>
        /// Adds a codePiece as Last item in the code
        /// </summary>
        /// <param name="newCodePiece">The code piece</param>
        /// <returns>The new CodePiece node</returns>
        public LinkedListNode<CodePiece> AddLast(string newCodePiece)
        {
            if (!HasAllWriteRequirements()) return null;
            return codePiecesList.AddLast(CreateCodePiece(newCodePiece));
        }

        /// <summary>
        /// Adds a code piece after another code piece
        /// </summary>
        /// <param name="node">The CodePiece node of the code piece behind which the new code piece will be added</param>
        /// <param name="newCodePiece">The new code piece</param>
        /// <returns>The new CodePiece node</returns>
        public LinkedListNode<CodePiece> AddAfter(LinkedListNode<CodePiece> node, string newCodePiece)
        {
            if (!HasAllWriteRequirements()) return null;
            return codePiecesList.AddAfter(node, CreateCodePiece(newCodePiece));
        }

        /// <summary>
        /// Adds a code piece before another code piece
        /// </summary>
        /// <param name="node">The CodePiece node of the code piece in front of which the new code piece will be added</param>
        /// <param name="newCodePiece">The new code piece</param>
        /// <returns>The new CodePiece node</returns>
        public LinkedListNode<CodePiece> AddBefore(LinkedListNode<CodePiece> node, string newCodePiece)
        {
            if (!HasAllWriteRequirements()) return null;
            return codePiecesList.AddBefore(node, CreateCodePiece(newCodePiece));
        }
        #endregion

        #region READ methods
        /// <summary>
        /// Find the first occurent of the regex in the File or Text and returns the information in an CodePiece node
        /// </summary>
        /// <param name="regex">The Regular expression</param>
        /// <param name="codePiece">out The final CodePiece node will be assigned to this</param>
        /// <returns>True if there was a match</returns>
        public bool Find(string regex, out LinkedListNode<CodePiece> codePiece)
        {
            if (!HasAllReadRequirements())
            {
                codePiece = null;
                return false;
            }

            Match match =
                (Settings.RegexTimeout == TimeSpan.Zero)
                ? Regex.Match(CompleteCode, regex, Settings.RegexOptions)
                : Regex.Match(CompleteCode, regex, Settings.RegexOptions, Settings.RegexTimeout);

            if (match.Success)
            {
                codePiece = CutCompleteCode(match);
                return true;
            }
            else
            {
                codePiece = null;
                return false;
            }
        }


        /// <summary>
        /// Finds all occurents of the regex in the File or Text and returns the information in a CodePiece node array
        /// </summary>
        /// <param name="regex">The Regular expression</param>
        /// <param name="codePieces"> out The final CodePiece nodes array with all matches will be assigned to this. Can be null</param>
        /// <returns>True if there was at least one match</returns>
        public bool FindAll(string regex, out LinkedListNode<CodePiece>[] codePieces)
        {
            if (!HasAllReadRequirements())
            {
                codePieces = null;
                return false;
            }
            MatchCollection matches =
                (Settings.RegexTimeout == TimeSpan.Zero)
                ? Regex.Matches(CompleteCode, regex, Settings.RegexOptions)
                : Regex.Matches(CompleteCode, regex, Settings.RegexOptions, Settings.RegexTimeout);

            if (settings.RegexOptions.HasFlag(RegexOptions.RightToLeft))
            {
                //RegexOptions.RightToLeft is curretly not supported
                //codePieces = CutCompleteCodeRightToLeft(matches);
                Debug.LogError("Regex option: RightToLeft is currently not supported.");
                codePieces = null;
                return false;
            }
            
            codePieces = CutCompleteCode(matches);

            return codePieces.Length > 0;
        }

        /// <summary>
        /// Returns the whole file content or Text as a CodePiece node
        /// </summary>
        /// <param name="codePiece">out The CodePiece with all the content of the File or Text</param>
        /// <returns>True if the CodeInspection has read permissions</returns>
        public bool GetEverything(out LinkedListNode<CodePiece> codePiece)
        {
            if (!HasAllReadRequirements())
            {
                codePiece = null;
                return false;
            }
            codePiece = codePiecesList.AddLast(CreateCodePiece(CompleteCode));
            return true;
        }
        #endregion

        #region FileHandling
        /// <summary>
        /// Tries to load the File and reads its content.
        /// </summary>
        /// <param name="path">The path to the File</param>
        /// <returns>True if the File was successfully read</returns>
        private bool ReadFile(string path)
        {
            //Read the text from directly from the test.txt file
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    CompleteCode = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ERROR! Can't read File. " + e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to write to a File
        /// </summary>
        /// <param name="path">The path to the File</param>
        /// <param name="newContent">The content which should be written in the File</param>
        /// <returns>True if the writing was successful</returns>
        private bool WriteFile(string path, string newContent)
        {
            //Replaces all New lines through OS specific Newline
            newContent = Regex.Replace(newContent, @"\r\n?|\n", Environment.NewLine, RegexOptions.Multiline);

            try
            {
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.Write(newContent);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ERROR! Can't read File. " + e.Message);
                return false;
            }
            CompleteCode = newContent;
            return true;
        }
        #endregion

        #region private
        /// <summary>
        /// Writes the new content either in a File or changes the currentText
        /// </summary>
        /// <param name="newContent"></param>
        /// <returns>True if the writing was successful</returns>
        private bool WriteNewContent(string newContent)
        {
            if (Type == InspectionType.TEXT)
            {
                CompleteCode = newContent;
                return true;
            }
            else if (Type == InspectionType.FILE)
            {
                return WriteFile(Path, newContent);
            }

            return true;
        }

        /// <summary>
        /// Tests if a inspection part is currently active. If true you can't create a new one before calling Commit()
        /// </summary>
        /// <returns>True if there is no edit made</returns>
        private bool IsNoEditationActive()
        {
            if (editationActive)
            {
                if (Mode == InspectionMode.READ_WRITE)
                {
                    Debug.LogError("The current CodeInspection has alredy created CodePieces. To create new ones or change Settings please first call Commit() or Cancel()");
                    return false;
                }
                else if(Mode == InspectionMode.READ)
                {
                    return Commit();
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the CodeInspection has Write permissions
        /// If InspectionType is Text you always have Write permissions
        /// </summary>
        /// <returns>True if the CodeInspection has write permissions</returns>
        private bool HasWritePermission()
        {
            if (Type == InspectionType.FILE && Mode != InspectionMode.READ_WRITE)
            {
                Debug.LogError("You do not have write permissions with CodeInspection: " + CodeInspectionID);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if the CodeInspection is still active. If not the CodeInspection can't read, find or write anything.
        /// This is for safety reasons. Is a CodeInspection inactive it does not longer block a file.
        /// If InspectionType is Text CodeInspection is always active
        /// </summary>
        /// <returns>True if the CodeInspection is active</returns>
        private bool IsCodeInspectionActive()
        {
            if (Type == InspectionType.FILE && CodeInspectionID == 0)
            {
                Debug.LogError("CodeInspection is Inactive. Create a new one");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if this CodeInspection has all permission to make write changes to the File or Text.
        /// </summary>
        /// <returns>True if CodeInspection has the permissions to write</returns>
        private bool HasAllWriteRequirements()// CodePiece part, bool singletonRequired = false)
        {
            return
                HasWritePermission() &&
                IsCodeInspectionActive();
        }

        /// <summary>
        /// Checks if this CodeInspection has all permission to read from the File or Text
        /// </summary>
        /// <return>True if CodeInspection has the permission to read</return></returns>
        private bool HasAllReadRequirements()
        {
            return
                IsNoEditationActive() && 
                IsCodeInspectionActive();
        }

        /// <summary>
        /// Creates a codePiece not created through a Regex match
        /// </summary>
        /// <param name="code">Code string</param>
        /// <returns>New created CodePiece</returns>
        private CodePiece CreateCodePiece(string code)
        {
            editationActive = true;
            return new CodePiece(code, Settings.AddLineIndex);
        }

        /// <summary>
        /// Creates a codePiece created through a Regex match
        /// </summary>
        /// <param name="match">The Regex Match</param>
        /// <returns>New created CodePiece</returns>
        private CodePiece CreateCodePiece(Match match)
        {
            editationActive = true;
            return new CodePiece(match, Settings.AddLineIndex);
        }

        /// <summary>
        /// Cut the complete code in three pieces the part before the match, the match and the part after the match
        /// </summary>
        /// <param name="match">The matche of regex</param>
        /// <returns>The CodePiece node of the matched part</returns>
        private LinkedListNode<CodePiece> CutCompleteCode(Match match)
        {
            int startIndex = match.Index;
            int endIndex = match.Index + match.Length;

            if (startIndex > 0)
                codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(0, startIndex)));
            LinkedListNode<CodePiece> matchCodePiece = codePiecesList.AddLast(CreateCodePiece(match));
            if (endIndex < CompleteCode.Length)
                codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(endIndex)));
            return matchCodePiece;

        }

        /// <summary>
        /// Cut the complete code in parts of the matches and parts between matches.
        /// </summary>
        /// <param name="matchCollection">The MatchCollection of regex</param>
        /// <returns>An array with all CodePiece nodes in MatchCollection</returns>
        private LinkedListNode<CodePiece>[] CutCompleteCode(MatchCollection matchCollection)
        {
            LinkedListNode<CodePiece>[] codePieces = new LinkedListNode<CodePiece>[matchCollection.Count];

            int currentStartIndex = 0;
            int startIndex, endIndex;
            for (int i = 0; i < matchCollection.Count; ++i)
            {
                Match match = matchCollection[i];
                startIndex = match.Index;
                endIndex = match.Index + match.Length;

                if (startIndex > currentStartIndex)
                {
                    codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(currentStartIndex, startIndex - currentStartIndex))); // If something is before the match add this
                }
                else if (startIndex < currentStartIndex)
                {
                    throw new Exception("The next Startindex is before the current startIndex!!");
                }

                codePieces[i] = codePiecesList.AddLast(CreateCodePiece(match)); //Add the match
                currentStartIndex = endIndex;
            }

            if (currentStartIndex < CompleteCode.Length)
            {
                codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(currentStartIndex))); //Add the rest of the code after every match
            }

            return codePieces;
        }

        // Same as CutCompleteCode but with RegexOption RightToLeft. Currently not supported
        /*private LinkedListNode<CodePiece>[] CutCompleteCodeRightToLeft(MatchCollection matchCollection)
        {
            LinkedListNode<CodePiece>[] codePieces = new LinkedListNode<CodePiece>[matchCollection.Count];

            int currentStartIndex = CompleteCode.Length;
            int startIndex, endIndex;
            for (int i = 0; i < matchCollection.Count; ++i)
            {
                Match match = matchCollection[i];
                startIndex = match.Index;
                endIndex = match.Index + match.Length;

                if (endIndex < currentStartIndex)
                {
                    codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(endIndex, currentStartIndex - endIndex))); // If something is before the match add this
                }
                else if (endIndex > currentStartIndex)
                {
                    throw new Exception("The next endIndex is after the last startIndex!!");
                }

                codePieces[i] = codePiecesList.AddLast(CreateCodePiece(match)); //Add the match
                currentStartIndex = startIndex;
            }

            if (currentStartIndex > 0)
            {
                codePiecesList.AddLast(CreateCodePiece(CompleteCode.Substring(0, currentStartIndex))); //Add the rest of the code after every match
            }

            return codePieces;
        }*/

        /// <summary>
        /// Resets CodeInspection values so new CodePieces can be created
        /// </summary>
        private void ResetCodeInspection()
        {
            codePiecesList.Clear();
            editationActive = false;
        }
        #endregion

        #region internal
        /// <summary>
        /// Set the CodeInspection inactive
        /// </summary>
        internal void SetInactive()
        {
            CodeInspectionID = 0;
        }

        /// <summary>
        /// Changes the InspectionMode to READ_WRITE
        /// </summary>
        internal bool GrantWritePermission()
        {
            if (IsNoEditationActive())
            {
                Mode = InspectionMode.READ_WRITE;
                ResetCodeInspection();
                return true;
            }
            else
            {
                Debug.LogError("The CodeInspection has active CodePieces please call Commit() or Cancel() before granting write permissions");
                return false;
            }
        }
        #endregion

    }
}