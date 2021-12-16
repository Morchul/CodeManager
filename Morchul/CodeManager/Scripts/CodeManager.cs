namespace Morchul.CodeManager
{
    /// <summary>
    /// Contains basic information about code manager
    /// </summary>
    public static class CodeManager
    {
        public const string Author = "Morchul";
        public const string Version = "1.0";
        public const string LastReleaseDate = "16.12.2021";

        public static string GetCodeManagerInformation()
        {
            return "Author: " + Author + "\n" +
                "Version: " + Version + "\n" +
                "Last release: " + LastReleaseDate;
        }
    }
}
