using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// All settings of CodeManager:
    /// The settings for ScriptTemplates where you can define all Placeholders and ScriptFolders.
    /// The settings for CleanCode where you can define all CleanCodeRules and Regexes
    /// The settings are saved as ScriptableObject under CodeManager/Resources/CodeManagerSettings
    /// </summary>
    public class CodeManagerSettings : ScriptableObject
    {
        #region ScriptTemplates variables
        public ScriptFolder[] ScriptFolders;
        public Placeholder[] Placeholders;
        #endregion

        #region CleanCode variables
        public UnwantedCode[] UnwantedCodes;
        public CodeGuideline[] CodeGuidelines;
        public CodeDocumentation[] CodeDocumentations;

        public CodeManagerRegex DocumentationRegex;

        public CodeManagerRegex[] Regexes;

        public Dictionary<uint, ICleanCodeRule> CleanCodeRules { get; private set; }

        private uint rulesIDCounter = 0;
        #endregion

        private event System.Action settingsReady;

        private void OnEnable()
        {
            if (CleanCodeRules == null)
            {
                CleanCodeRules = new Dictionary<uint, ICleanCodeRule>();
                UpdateRules();
                settingsReady?.Invoke();
                settingsReady = null;
            }
        }

        public void AddReadyListener(System.Action onReadyAction)
        {
            if (CleanCodeRules == null)
            {
                settingsReady += onReadyAction;
            }
            else
            {
                onReadyAction?.Invoke();
            }
        }

        #region CleanCode methods
        private void AddICleanCodeRule(ICleanCodeRule rules)
        {
            if (rules.IsValid())
                CleanCodeRules.Add(rules.GetID(), rules);
        }

        /// <summary>
        /// Update the CleanCodeRules Dictionary (UnwantedCode, CodeGuideline, CodeDocumentation)
        /// </summary>
        public void UpdateRules()
        {
            CleanCodeRules.Clear();
            if (UnwantedCodes == null || CodeGuidelines == null || CodeDocumentations == null) return;

            for (int i = 0; i < UnwantedCodes.Length; ++i)
            {
                if (UnwantedCodes[i].GetID() == 0)
                {
                    UnwantedCodes[i].SetID(++rulesIDCounter);
                }
                AddICleanCodeRule(UnwantedCodes[i]);
            }

            for (int i = 0; i < CodeGuidelines.Length; ++i)
            {
                if (CodeGuidelines[i].GetID() == 0)
                {
                    CodeGuidelines[i].SetID(++rulesIDCounter);
                }
                AddICleanCodeRule(CodeGuidelines[i]);
            }

            for (int i = 0; i < CodeDocumentations.Length; ++i)
            {
                if (CodeDocumentations[i].GetID() == 0)
                {
                    CodeDocumentations[i].SetID(++rulesIDCounter);
                }
                AddICleanCodeRule(CodeDocumentations[i]);
            }
        }
        #endregion
    }
}
