namespace Morchul.CodeManager
{
    public static class DefaultSettings
    {

        #region Regexes
        public const string AccessRegex = @"\b(public\s*|private\s*|protected\s*|internal\s*)?";
        public const string ModifierRegex = @"\b(static\s*|virtual\s*|abstract\s*|readonly\s*|const\s*)?";
        public const string TypeRegex = @"\b([A-Za-z0-9_\[\]\<\>]+)";
        public const string IdentifierRegex = @"\b(?<identifier>[A-Za-z_][A-Za-z_0-9]*)";
        public const string GenericRegex = @"(\<[A-Z]\>)?";
        public const string ParameterRegex = @"\(\s*((params)?\s*(\b[A-Za-z_][A-Za-z_0-9\[\]\<\>]*)\s*(\b[A-Za-z_][A-Za-z_0-9]*)\s*\,?\s*)*\s*\)";


        public const string AccessRegex_Private = @"\b(private\s*)";

        public const string FieldRegex = AccessRegex + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex;
        public const string MethodRegex = AccessRegex + @"\b(async\s*)?" + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex + @"\s*" + GenericRegex + @"\s*" + ParameterRegex;
        public const string ClassRegex = @".*class.*" + IdentifierRegex;


        public const string DefaultDocumentationRegex = @"[^\S][\/]{2,3}.*\n?$";
        #endregion

        public static void SetDefaultSettings(CodeManagerSettings settings)
        {
            //Init all arrays
            settings.Regexes = new CodeManagerRegex[7];
            settings.UnwantedCodes = new UnwantedCode[1];
            settings.CodeGuidelines = new CodeGuideline[2];
            settings.CodeDocumentations = new CodeDocumentation[1];

            //Set default regexes
            settings.Regexes[0] = new CodeManagerRegex { Name = "Empty Codeblock regex", Regex = @"\{\s*\}" };
            settings.Regexes[1] = new CodeManagerRegex { Name = "Method regex", Regex = MethodRegex };
            settings.Regexes[2] = new CodeManagerRegex { Name = "Class regex", Regex = ClassRegex };
            settings.Regexes[3] = new CodeManagerRegex { Name = "Field regex", Regex = FieldRegex };
            settings.Regexes[4] = new CodeManagerRegex { Name = "Private Field regex", Regex = AccessRegex_Private + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex + @"(;|\s*=\s*[a-zA-Z0-9_])"};
            settings.Regexes[5] = new CodeManagerRegex { Name = "Method Name regex", Regex = @"\b[A-Z][a-zA-Z_0-9]*" };
            settings.Regexes[6] = new CodeManagerRegex { Name = "Private field name regex", Regex = @"\b[a-z][a-zA-Z_0-9]*" };
            settings.DocumentationRegex = new CodeManagerRegex() { Regex = DefaultDocumentationRegex };

            //Set default unwanted Codes
            settings.UnwantedCodes[0] = new UnwantedCode { Name = "Empty Codeblock", Description = "No empty code blocks allowed.", RegexIndex = 0 };

            //Set default code guidelines
            settings.CodeGuidelines[0] = new CodeGuideline
            {
                Name = "Method Name",
                Description = "Methods have to start with a capital letter",
                GroupName = "identifier",
                SearchRegexIndex = 1,
                MatchRegexIndex = 5
            };

            settings.CodeGuidelines[1] = new CodeGuideline
            {
                Name = "Private field name",
                Description = "private fields have to start with a lowercase letter",
                GroupName = "identifier",
                SearchRegexIndex = 4,
                MatchRegexIndex = 6
            };

            //Set default code documentations
            settings.CodeDocumentations[0] = new CodeDocumentation { Name = "Documentation on class", Description = "Classes must be documented", RegexIndex = 2 };
        }
    }
}
