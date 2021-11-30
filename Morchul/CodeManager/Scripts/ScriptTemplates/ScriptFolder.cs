using System.Collections.Generic;

namespace Morchul.CodeManager
{
    /// <summary>
    /// A Scriptfolder where you can create script template and which is scaned by CleanCode if Scan is set to true
    /// </summary>
    [System.Serializable]
    public class ScriptFolder
    {
        public string Name;
        public string Path;
        public List<int> CleanCodeRules;

        public bool IsNull()
        {
            return string.IsNullOrEmpty(Path);
        }
    }
}

