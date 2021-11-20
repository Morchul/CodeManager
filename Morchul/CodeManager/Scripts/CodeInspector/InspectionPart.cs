namespace Morchul.CodeManager
{
    /// <summary>
    /// The InspectionPart is the Result of a CodeInspection and can be used to change or remove the found part.
    /// </summary>
    public struct InspectionPart
    {
        public static readonly InspectionPart Null;

        /// <summary>
        /// The CodePart can be changed freely. To change the codepart in the File or Text call Replace on the CodeInspection
        /// </summary>
        public string CodePiece;

        //The index of the CodePiece in the whole code
        public readonly int CodePieceIndex;

        //Variables to check if the InspectionPart is compatible with the CodeInspection
        public readonly uint CodeInspectionCodeVersion;
        public readonly uint CodeInspectionID;

        /// <summary>
        /// Used regex to find the CodePart
        /// </summary>
        public readonly string Regex;

        /// <summary>
        /// True if this InspectionPart is the only created InspectionPart
        /// Allows the use for DeleteBefore and DeleteAfter
        /// </summary>
        public readonly bool Singleton;

        internal InspectionPart(string codePart, int codePieceIndex, uint codeInspectionCodeVersion, uint codeInspectionID, string regex, bool singleton)
        {
            CodePiece = codePart;
            CodePieceIndex = codePieceIndex;
            CodeInspectionCodeVersion = codeInspectionCodeVersion;

            CodeInspectionID = codeInspectionID;
            Regex = regex;
            Singleton = singleton;
        }

        public override string ToString()
        {
            return "{ID:" + CodeInspectionID + "/V:" + CodeInspectionCodeVersion + "} [Regex: " + Regex + "] (Index:" + CodePieceIndex + "): " + CodePiece;
        }

        public bool IsNull()
        {
            return CodeInspectionID == 0;
        }
    }
}
