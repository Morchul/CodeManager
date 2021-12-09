namespace Morchul.CodeManager
{
    /// <summary>
    /// A CodeDocumentation is build the follow:
    /// If the search with the Regex returns a Result the Code Part before the match will be checked with the DocumentationRegex in CleanCodeSettings if there is no match a CleanCodeViolation will be created.
    /// </summary>
    [System.Serializable]
    public struct CodeDocumentation : ICleanCodeRule
    {
        /// <summary>
        /// The name of the CodeDocumentation Rule
        /// </summary>
        public string Name;

        /// <summary>
        /// The Index of the Regex in the settings which is used by the CodeDocumentation rule
        /// </summary>
        public int RegexIndex;

        /// <summary>
        /// CodeDocumentation rule description
        /// </summary>
        public string Description;

        /// <summary>
        /// identifier of the CodeDocumentation rule
        /// </summary>
        public uint ID;

        public uint GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Valid if Regex is set
        /// </summary>
        /// <returns>True if CodeDocumentation rule is valid</returns>
        public bool IsValid()
        {
            return RegexIndex >= 0;
        }

        public void SetID(uint ID)
        {
            this.ID = ID;
        }

        ICleanCodeRule.CleanCodeRuleType ICleanCodeRule.GetType()
        {
            return ICleanCodeRule.CleanCodeRuleType.CodeDocumentation;
        }
    }
}
