using System.Text.RegularExpressions;

namespace Morchul.CodeManager
{
    /// <summary>
    /// The InspectionPart is the Result of a CodeInspection and can be used to change or remove the found part.
    /// </summary>
    public class CodePiece
    {
        /// <summary>
        /// The Code can be changed freely.
        /// </summary>
        private string code;
        public string Code
        {
            get => code;
            set
            {
                code = value;
                CalcLineCount();
            }
        }

        /// <summary>
        /// The Match returned by Regex class. Can be used to get Captures and Groups
        /// Can be null
        /// </summary>
        public readonly Match Match;

        /// <summary>
        /// How many lines the CodePiece contains
        /// </summary>
        private int lineCount;
        public int LineCount => lineCount;

        /// <summary>
        /// Does the CodePiece end with a \n character
        /// </summary>
        public bool EndWithNewLine { get; private set; }

        private readonly bool calcLineCount;

        internal CodePiece(Match match, bool calcLineCount)
        {
            Code = match.Value;
            Match = match;
            this.calcLineCount = calcLineCount;
            CalcLineCount();
        }

        internal CodePiece(string code, bool calcLineCount)
        {
            Code = code;
            Match = null;
            this.calcLineCount = calcLineCount;
            CalcLineCount();
        }

        private void CalcLineCount()
        {
            if(calcLineCount)
                EndWithNewLine = CodeManagerUtility.GetLineCount(Code, out lineCount);
            else
                lineCount = -1;
        }
    }
}
