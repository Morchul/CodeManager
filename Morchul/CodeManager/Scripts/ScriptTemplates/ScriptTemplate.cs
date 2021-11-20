using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// A script template from which a script can be created
    /// </summary>
    public class ScriptTemplate
    {
        public readonly TextAsset Template;
        public readonly string TemplateName;

        public ScriptTemplate(TextAsset textAsset, string name)
        {
            Template = textAsset;
            TemplateName = name;
        }
    }
}