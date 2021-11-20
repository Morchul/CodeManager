namespace Morchul.CodeManager
{
    /// <summary>
    /// A Scriptfolder where you can create script template and which is scaned by CleanCode if Scan is set to true
    /// </summary>
    [System.Serializable]
    public struct ScriptFolder
    {
        public string Name;
        public string Path;
        public bool Scan;

        public bool IsNull()
        {
            return string.IsNullOrEmpty(Path);
        }
    }
}

