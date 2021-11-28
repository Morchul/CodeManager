namespace Morchul.CodeManager
{
    /// <summary>
    /// A CodingGuideline is build the following:
    /// Search in code after SearchRegex. In the SearchRegex there must be a group named after GroupName.
    /// If something is found, the found group with GroupName will be matched with MatchRegex if this is false a CleanCodeViolation will be created
    /// </summary>
    [System.Serializable]
    public struct CodeGuideline : IScanable
    {
        public string Name;
        public int SearchRegexIndex;
        public string GroupName;
        public int MatchRegexIndex;

        public string Description;

        public int ID;

        public int GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return Name;
        }

        public bool IsValid()
        {
            return SearchRegexIndex >= 0 && 
                MatchRegexIndex >= 0 &&
                !string.IsNullOrEmpty(GroupName);
        }

        public void SetID(int ID)
        {
            this.ID = ID;
        }

        IScanable.ScanableType IScanable.GetType()
        {
            return IScanable.ScanableType.CodingGuideline;
        }
    }
}
