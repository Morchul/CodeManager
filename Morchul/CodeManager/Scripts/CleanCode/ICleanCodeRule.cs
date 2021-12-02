namespace Morchul.CodeManager
{
    /// <summary>
    /// An interface to identiy all CleanCodeRule with necessary methods to scan for the CleanCodeRule
    /// </summary>
    public interface ICleanCodeRule
    {
        public uint GetID();
        public void SetID(uint ID);

        public string GetName();

        public CleanCodeRuleType GetType();

        public bool IsValid();

        public enum CleanCodeRuleType
        {
            CodingGuideline,
            UnwantedCode,
            CodeDocumentation,
            None
        }
    }
}
