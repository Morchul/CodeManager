namespace Morchul.CodeManager
{
    /// <summary>
    /// An UnwantedCode is build the follow:
    /// If the search with the Regex find somthing a CleanCodeVioletion will be created for this unwanted code
    /// </summary>
    [System.Serializable]
    public struct UnwantedCode : ICleanCodeRule
    {
        public string Name;
        public int RegexIndex;

        public string Description;

        public uint ID;

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
