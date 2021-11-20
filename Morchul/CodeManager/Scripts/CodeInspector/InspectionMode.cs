namespace Morchul.CodeManager
{
    /// <summary>
    /// InspectionMode determines if the CodeInspection has the right to write or not.
    /// With a CodeInspection with Write permissions the File will be blocked for other CodeInspection.
    /// </summary>
    public enum InspectionMode
    {
        READ,
        READ_WRITE
    }
}


