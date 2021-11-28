public interface IScanable
{
    public int GetID();
    public void SetID(int ID);

    public string GetName();

    public ScanableType GetType();

    public bool IsValid();

    public enum ScanableType
    {
        CodingGuideline,
        UnwantedCode,
        CodeDocumentation,
        None
    }
}
