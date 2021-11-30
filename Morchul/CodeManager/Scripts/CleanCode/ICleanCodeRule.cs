public interface ICleanCodeRule
{
    public int GetID();
    public void SetID(int ID);

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
