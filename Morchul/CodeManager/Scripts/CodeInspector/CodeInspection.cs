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
    /// You create InspectionParts from the CodeInspection to then change the underlining File content or Text with the adjusted InspectionParts.
    /// There can always be only one InspectionPart active. Means after a Find() FindAll() or GetEverything() call you have to call Commit() before calling any of the three methods again.
    /// </summary>
    public class CodeInspection
    {
        public InspectionMode Mode { get; private set; }
        public InspectionType Type { get; private set; }

        public string Path { get; private set; }

        public string CompleteCode { get; private set; }

        public uint CodeInspectionID { get; private set; }

        private bool dirty = false;
        private bool inspectionPartActive = false;

        private List<string> codePieces;
        private LinkedList<int> indexMemory;
        private uint codeVersion;

        internal CodeInspection(uint codeInspectionID, InspectionMode mode, InspectionType type, string pathOrText)
        {
            CodeInspectionID = codeInspectionID;
            Mode = mode;
            Type = type;

            codePieces = new List<string>();
            indexMemory = new LinkedList<int>();
            codeVersion = 1;

            if (pathOrText != null)
            {
                if (Type == InspectionType.FILE)
                {
                    if (pathOrText != "")
                    {
                        Path = pathOrText;
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
        /// Commits all changes and let you create a new InspectionPart if your in READ_WRITE mode
        /// Writes the changes to the File or Text
        /// Will be called automaticly if your in READ mode
        /// </summary>
        /// <returns>True if the changes have been successfully written</returns>
        public bool Commit()
        {
            if (!dirty)
            {
                ResetCodeInspection();
                return true; //Nothing changed
            }

            string newCode = CreateCurrentCode();

            bool success = WriteNewContent(newCode);

            if (success)
            {
                ResetCodeInspection();
                dirty = false;
            }

            return success;
        }

        /// <summary>
        /// Returns the current code with all changes but does not write the changes to the file or text
        /// Can be used to check if all changes were made before commiting and writting to a file.
        /// </summary>
        /// <returns>The current Code with all changes</returns>
        public string CreateCurrentCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (int index in indexMemory)
            {
                if(!String.IsNullOrEmpty(codePieces[index]))
                    sb.Append(codePieces[index]);
            }

            return sb.ToString();
        }

        #region READ methods
        /// <summary>
        /// Find the first occurent of the regex in the File or Text and returns the information in an InspectionPart
        /// </summary>
        /// <param name="regex">The Regular expression</param>
        /// <param name="inspectionPart">The final InspectionPart will be assigned to this</param>
        /// <returns>True if there was a match</returns>
        public bool Find(string regex, out InspectionPart inspectionPart)
        {
            if (!HasAllReadRequirements())
            {
                inspectionPart = InspectionPart.Null;
                return false;
            }

            Match match = Regex.Match(CompleteCode, regex);
            if (match.Success)
            {
                int matchIndex = CutCompleteCode(match);
                inspectionPart = CreateInspectionPart(matchIndex, regex, true);
                if (Mode == InspectionMode.READ) Commit();
                return true;
            }
            else
            {
                inspectionPart = InspectionPart.Null;
                return false;
            }

            
        }

        /// <summary>
        /// Finds all occurents of the regex in the File or Text and returns the information in a InspectionPart array
        /// </summary>
        /// <param name="regex">The Regular expression</param>
        /// <param name="inspectionParts">The final InspectionParts array with all matches will be assigned to this</param>
        /// <returns>True if there was at least one match</returns>
        public bool FindAll(string regex, out InspectionPart[] inspectionParts)
        {
            if (!HasAllReadRequirements())
            {
                inspectionParts = new InspectionPart[0];
                return false;
            }

            MatchCollection matches = Regex.Matches(CompleteCode, regex);
            InspectionPart[] foundInspectionParts = new InspectionPart[matches.Count];
            int[] matchIndexes = CutCompleteCode(matches);
            for(int i = 0; i < matchIndexes.Length; ++i)
            {
                foundInspectionParts[i] = CreateInspectionPart(matchIndexes[i], regex, false);
            }

            inspectionParts = foundInspectionParts;
            if (Mode == InspectionMode.READ) Commit();
            return foundInspectionParts.Length > 0;
        }

        /// <summary>
        /// Returns the whole file content or Text as a InspectionPart
        /// </summary>
        /// <param name="inspectionPart">The InspectionPart which all the content of the File or Text</param>
        /// <returns>True if the CodeInspection is active</returns>
        public bool GetEverything(out InspectionPart inspectionPart)
        {
            if (!HasAllReadRequirements())
            {
                inspectionPart = InspectionPart.Null;
                return false;
            }
            AddCodePieceLast(CompleteCode);
            inspectionPart = CreateInspectionPart(0, @".", true);
            if (Mode == InspectionMode.READ) Commit();
            return true;
        }
        #endregion

        #region WRITE methods
        /// <summary>
        /// Replaces the code determinate in the InspectionPart with the text from the InspectionPart
        /// </summary>
        /// <param name="part">The InspectionPart which determinate the part which will be replaced and with which text it will be replaced.</param>
        /// <returns>True if the replacement was successful</returns>
        public bool Replace(InspectionPart part)
        {
            if (!HasAllWriteRequirements(part)) return false;

            codePieces[part.CodePieceIndex] = part.CodePiece;
            dirty = true;

            return true;
        }

        /// <summary>
        /// Deletes the code determinate in the InspectionPart
        /// </summary>
        /// <param name="part">The InspectionPart which determinate the part which will be deleted</param>
        /// <returns>True if the deletion was successful</returns>
        public bool Delete(InspectionPart part)
        {
            if (!HasAllWriteRequirements(part)) return false;

            return DeleteCodePiece(part.CodePieceIndex);
        }

        /// <summary>
        /// Delete the codePiece before the part determinate by the InspectionPart
        /// </summary>
        /// <param name="part">The part who determinate before which the codePiece will be deleted</param>
        /// <returns>True if the deletion was successful</returns>
        public bool DeleteBefore(InspectionPart part)
        {
            if (!HasAllWriteRequirements(part, true)) return false;

            return DeleteCodePiece(part.CodePieceIndex - 1);
        }

        /// <summary>
        /// Delete the codePiece after the part determinate by the InspectionPart
        /// </summary>
        /// <param name="part">The part who determinate after which the codePiece will be deleted</param>
        /// <returns>True if the deletion was successful</returns>
        public bool DeleteAfter(InspectionPart part)
        {
            if (!HasAllWriteRequirements(part, true)) return false;

            return DeleteCodePiece(part.CodePieceIndex + 1);
        }

        /// <summary>
        /// Add text after the part determinate by the InspectionPart.
        /// </summary>
        /// <param name="part">Defines the area behind which the additional text will be added</param>
        /// <param name="additionalText">The text which will be added after part</param>
        /// <param name="replaceToo">If true, AddAfter has additionaly the same functionality as replace</param>
        /// <returns>True if all changes have been made successfully</returns>
        public bool AddAfter(InspectionPart part, string additionalText, bool replaceToo = false)
        {
            if (!HasAllWriteRequirements(part)) return false;

            if (replaceToo)
            {
                if (!Replace(part)) return false;
            }

            AddCodePieceAfter(part.CodePieceIndex, additionalText);
            dirty = true;

            return true;
        }

        /// <summary>
        /// Add text before the part determinate by the InspectionPart.
        /// </summary>
        /// <param name="part">Defines the area in front of which the additional text will be added</param>
        /// <param name="additionalText">The text which will be added before part</param>
        /// <param name="replaceToo">If true, AddAfter has additionaly the same functionality as replace</param>
        /// <returns>True if all changes have been made successfully</returns>
        public bool AddBefore(InspectionPart part, string additionalText, bool replaceToo = false)
        {
            if (!HasAllWriteRequirements(part)) return false;

            if (replaceToo)
            {
                if (!Replace(part)) return false;
            }

            AddCodePieceBefore(part.CodePieceIndex, additionalText);
            dirty = true;

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
        /// Delete a CodePiece at specific index
        /// </summary>
        /// <param name="index">index of the code piece to delete</param>
        /// <returns>True</returns>
        private bool DeleteCodePiece(int index)
        {
            if (index < 0 || index >= codePieces.Count)
            {
                Debug.LogWarning("CodePieceIndex to Delete was out of bounds.");
            }

            codePieces[index] = "";
            dirty = true;
            return true;
        }

        /// <summary>
        /// Tests if a inspection part is currently active. If true you can't create a new one before calling Commit()
        /// </summary>
        /// <returns>True if there is no active inspectionPart</returns>
        private bool IsNoInspectionPartActive()
        {
            if (inspectionPartActive && Mode == InspectionMode.READ_WRITE)
            {
                Debug.LogError("The current CodeInspection has alredy created a InspectionPart. To create a new please first call Commit()");
                return false;
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
        /// Tests if the InspectionPart is created from this CodeInspection.
        /// Makes sure that the InspectionPart is from this File or Text
        /// </summary>
        /// <param name="part">The InspectionPart to test</param>
        /// <returns>True if the InspectionPart is created from this CodeInspection</returns>
        private bool IsCorrectCodeInspection(InspectionPart part)
        {
            if (part.CodeInspectionID != CodeInspectionID)
            {
                Debug.LogError("InspectionPart is not created from this CodeInspection and can't change any other CodeInspection!");
                return false;
            }

            if (part.CodeInspectionCodeVersion != codeVersion)
            {
                Debug.LogError("InspectionPart is created from an older version of this CodeInspection please get a new one with Find!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the InspectionPart is a singleton. Required for some write methods
        /// </summary>
        /// <param name="part">The InspectionPart to test</param>
        /// <returns>True if the InspectionPart is a singleton</returns>
        private bool IsSingleton(InspectionPart part)
        {
            if (!part.Singleton)
            {
                Debug.Log("InspectionPart is not a singleton and can't be used in called method. Search with Find to get a singleton InspectionPart");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if this CodeInspection has all permission to make write changes to the File or Text.
        /// </summary>
        /// <param name="part">The InspectionPart which represents the changes</param>
        /// <returns>True if CodeInspection has the permissions to write</returns>
        private bool HasAllWriteRequirements(InspectionPart part, bool singletonRequired = false)
        {
            if (part.IsNull())
            {
                Debug.LogError("InspectionPart is null!");
                return false;
            }

            return HasWritePermission() &&
                IsCodeInspectionActive() &&
                (!singletonRequired || IsSingleton(part)) &&
                IsCorrectCodeInspection(part);
        }

        /// <summary>
        /// Checks if this CodeInspection has all permission to read from the File or Text
        /// </summary>
        /// <return>True if CodeInspection has the permission to read</return></returns>
        private bool HasAllReadRequirements()
        {
            return IsNoInspectionPartActive() &&
                IsCodeInspectionActive();
        }

        /// <summary>
        /// Creates a InspectionPart from this CodeInspection
        /// </summary>
        /// <param name="match">The match which was made from the regex</param>
        /// <param name="regex">The regex which was used</param>
        /// <returns>The created InspectionPart</returns>
        private InspectionPart CreateInspectionPart(int matchIndex, string regex, bool singleton)
        {
            //Remove unnecessary delimiter at the end read by the StreamReader
            string text = codePieces[matchIndex];
            if (text[text.Length - 1] == '\r')
            {
                text = text.Remove(text.Length - 1);
            }
            inspectionPartActive = true;
            return new InspectionPart(text, matchIndex, codeVersion, CodeInspectionID, regex, singleton);
        }

        /// <summary>
        /// Adds a codePiece as Last item in the code
        /// </summary>
        /// <param name="newCodePiece">The code piece</param>
        /// <returns>The index of the code piece in the whole code</returns>
        private int AddCodePieceLast(string newCodePiece)
        {
            codePieces.Add(newCodePiece);
            indexMemory.AddLast(codePieces.Count - 1);
            //Debug.Log("AddCodePieceLast: " + newCodePiece + "/" + (codePieces.Count - 1));
            return codePieces.Count - 1;
        }

        /// <summary>
        /// Adds a code piece after another code piece
        /// </summary>
        /// <param name="index">The index of the code piece behind which the new code piece will be added</param>
        /// <param name="newCodePiece">The new code piece</param>
        private void AddCodePieceAfter(int index, string newCodePiece)
        {
            codePieces.Add(newCodePiece);
            indexMemory.AddAfter(indexMemory.Find(index), codePieces.Count - 1);
        }

        /// <summary>
        /// Adds a code piece before another code piece
        /// </summary>
        /// <param name="index">The index of the code piece in front of which the new code piece will be added</param>
        /// <param name="newCodePiece">The new code piece</param>
        private void AddCodePieceBefore(int index, string newCodePiece)
        {
            codePieces.Add(newCodePiece);
            indexMemory.AddBefore(indexMemory.Find(index), codePieces.Count - 1);
        }

        /// <summary>
        /// Cut the complete code in three pieces the part before the match, the match and the part after the match
        /// </summary>
        /// <param name="match">The matche of regex</param>
        /// <returns>The index of the matched part</returns>
        private int CutCompleteCode(Match match)
        {
            int startIndex = match.Index;
            int endIndex = match.Index + match.Length;
            if (startIndex > 0)
                AddCodePieceLast(CompleteCode.Substring(0, startIndex));
            int matchIndex = AddCodePieceLast(CompleteCode.Substring(startIndex, match.Length));
            if (endIndex < CompleteCode.Length)
                AddCodePieceLast(CompleteCode.Substring(endIndex));
            return matchIndex;
        }

        /// <summary>
        /// Cut the complete code in parts of the matches and parts between matches.
        /// </summary>
        /// <param name="matchCollection">The MatchCollection of regex</param>
        /// <returns>An array with all indexes of the matches in the same order as in matchCollection</returns>
        private int[] CutCompleteCode(MatchCollection matchCollection)
        {
            int[] codePieceIndexes = new int[matchCollection.Count];

            int currentStartIndex = 0;
            int startIndex, endIndex;
            for (int i = 0; i < matchCollection.Count; ++i)
            {
                Match match = matchCollection[i];
                startIndex = match.Index;
                endIndex = match.Index + match.Length;

                if (startIndex > currentStartIndex)
                {
                    AddCodePieceLast(CompleteCode.Substring(currentStartIndex, startIndex - currentStartIndex)); // If something is before the match add this
                }
                else if (startIndex < currentStartIndex)
                {
                    throw new Exception("The next Startindex is before the current startIndex!!");
                }

                codePieceIndexes[i] = AddCodePieceLast(CompleteCode.Substring(startIndex, match.Length)); //Add the match and save the index
                currentStartIndex = endIndex;
            }

            if (currentStartIndex < CompleteCode.Length)
                AddCodePieceLast(CompleteCode.Substring(currentStartIndex)); //Add the rest of the code after every match

            return codePieceIndexes;
        }

        /// <summary>
        /// Resets CodeInspection values so a new InspectionPart can be created
        /// Also counts codeVersion up to make old InspectionParts invalid
        /// </summary>
        private void ResetCodeInspection()
        {
            indexMemory.Clear();
            codePieces.Clear();

            inspectionPartActive = false;

            if (Mode == InspectionMode.READ_WRITE)
            {
                ++codeVersion;
            }
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
            if (inspectionPartActive)
            {
                Debug.LogError("The CodeInspection has active InspectionParts please call Commit() before granting write permissions");
                return false;
            }
            else
            {
                Mode = InspectionMode.READ_WRITE;
                ++codeVersion; //Increase code Version so previous InspectionParts from READ mode are invalid
                return true;
            }
        }
        #endregion

    }
}