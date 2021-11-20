namespace Morchul.CodeManager
{
    /// <summary>
    /// Placeholder for script template
    /// Create a placeholder in a script template by writing %Name% by script creation this will replaced through Value
    /// </summary>
    [System.Serializable]
    public struct Placeholder
    {
        public string Name;
        public string Value;

        public override string ToString()
        {
            return "Placeholder: [" + Name + ": " + Value + "]";
        }
    }
}
