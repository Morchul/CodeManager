namespace Morchul.CodeManager
{
    /// <summary>
    /// An interface to identiy all CleanCodeRule with necessary methods to scan for the CleanCodeRule
    /// </summary>
    public interface ICleanCodeRule
    {
        /// <summary>
        /// Get the ID to identify the CleanCodeRule
        /// </summary>
        /// <returns>The CleanCodeRule ID</returns>
        public uint GetID();
        /// <summary>
        /// Set the ID to identiy the CleanCodeRule
        /// </summary>
        /// <param name="ID">The CleanCodeRule ID</param>
        public void SetID(uint ID);

        /// <summary>
        /// Get the name of the CleanCodeRule
        /// </summary>
        /// <returns>The name of the CleanCodeRule</returns>
        public string GetName();

        /// <summary>
        /// Gets the Type of the CleanCodeRule
        /// </summary>
        /// <returns>The CleanCodeRuleType</returns>
        public CleanCodeRuleType GetType();

        /// <summary>
        /// Tests if the CleanCodeRule is valid means all values to operate with the rule are set.
        /// </summary>
        /// <returns>True if the CleanCodeRule is valid</returns>
        public bool IsValid();

        /// <summary>
        /// All CleanCodeRuleTypes
        /// </summary>
        public enum CleanCodeRuleType
        {
            CodingGuideline,
            UnwantedCode,
            CodeDocumentation,
            None
        }
    }
}
