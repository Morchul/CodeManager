namespace Morchul.CodeManager
{
    /// <summary>
    /// A CodeDocumentation is build the follow:
    /// If the search with the Regex returns a Result the Code Part before the match will be checked with the DocumentationRegex in CleanCodeSettings if there is no match a CleanCodeViolation will be created.
    /// </summary>
    [System.Serializable]
    public struct CodeDocumentation : IScanable
    {
        public string Name;
        public int RegexIndex;

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
            return RegexIndex >= 0;
        }

        public void SetID(int ID)
        {
            this.ID = ID;
        }

        IScanable.ScanableType IScanable.GetType()
        {
            return IScanable.ScanableType.CodeDocumentation;
        }
    }
}
