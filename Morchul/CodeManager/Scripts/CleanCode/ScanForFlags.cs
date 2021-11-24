namespace Morchul.CodeManager
{
    [System.Serializable, System.Flags]
    public enum ScanForFlags : int
    {
        Documentation_on_Fields = 1,
        Documentation_on_Methods = 2,
        Documentation_on_Classes = 4,
        Code_Guidelines = 8,
        Unwanted_Code = 16,
        Everything = 31
    }
}
