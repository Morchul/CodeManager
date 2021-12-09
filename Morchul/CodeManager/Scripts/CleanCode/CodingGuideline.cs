namespace Morchul.CodeManager
{
    /// <summary>
    /// A CodingGuideline is build the following:
    /// Search in code after SearchRegex. In the SearchRegex there must be a group named after GroupName.
    /// If something is found, the found group with GroupName will be matched with MatchRegex if this is false a CleanCodeViolation will be created
    /// </summary>
    [System.Serializable]
    public struct CodeGuideline : ICleanCodeRule
    {
        /// <summary>
        /// The name of the CodeGuideline Rule
        /// </summary>
        public string Name;

        /// <summary>
        /// The Index of the Regex in the settings which is used as SearchRegex by the CodeGuideline
        /// </summary>
        public int SearchRegexIndex;

        /// <summary>
        /// GroupName of the value which should be checked with the MatchRegex
        /// </summary>
        public string GroupName;

        /// <summary>
        /// The Index of the Regex in the settings which is used as MatchRegex by the CodeGuideline
        /// </summary>
        public int MatchRegexIndex;

        /// <summary>
        /// CodeGuideline rule description
        /// </summary>
        public string Description;

        /// <summary>
        /// identifier of the CodeGuideline rule
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
        /// Valid if Search- and MatchRegex are set and GroupName is not null or empty
        /// </summary>
        /// <returns>True if CodingGuideline rule is valid</returns>
        public bool IsValid()
        {
            return SearchRegexIndex >= 0 && 
                MatchRegexIndex >= 0 &&
                !string.IsNullOrEmpty(GroupName);
        }

        public void SetID(uint ID)
        {
            this.ID = ID;
        }

        ICleanCodeRule.CleanCodeRuleType ICleanCodeRule.GetType()
        {
            return ICleanCodeRule.CleanCodeRuleType.CodingGuideline;
        }
    }
}
