namespace Morchul.CodeManager
{
    /// <summary>
    /// An UnwantedCode is build the follow:
    /// If the search with the Regex find somthing a CleanCodeVioletion will be created for this unwanted code
    /// </summary>
    [System.Serializable]
    public struct UnwantedCode : ICleanCodeRule
    {
        /// <summary>
        /// The name of the UnwantedCode Rule
        /// </summary>
        public string Name;

        /// <summary>
        /// The Index of the Regex in the settings which is used by the UnwantedCode rule
        /// </summary>
        public int RegexIndex;

        /// <summary>
        /// UnwantedCode rule description
        /// </summary>
        public string Description;

        /// <summary>
        /// identifier of the UnwantedCode rule
        /// </summary>
        public uint ID;

        /// <summary>
        /// Valid if Regex is set
        /// </summary>
        /// <returns>True if UnwantedCode rule is valid</returns>
        public bool IsValid()
        {
            return RegexIndex >= 0;
        }

        public uint GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return Name;
        }

        public void SetID(uint ID)
        {
            this.ID = ID;
        }

        ICleanCodeRule.CleanCodeRuleType ICleanCodeRule.GetType()
        {
            return ICleanCodeRule.CleanCodeRuleType.UnwantedCode;
        }
    }
}
