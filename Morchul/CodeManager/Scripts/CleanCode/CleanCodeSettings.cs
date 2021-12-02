using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// The settings for CleanCode where you can define all CleanCodeRules and Regexes
    /// The settings are saved as ScriptableObject under CodeManager/Resources/CleanCodeSettings
    /// </summary>
    public class CleanCodeSettings : ScriptableObject
    {
        public UnwantedCode[] UnwantedCodes;
        public CodeGuideline[] CodeGuidelines;
        public CodeDocumentation[] CodeDocumentations;

        public CodeManagerRegex DocumentationRegex;

        public CodeManagerRegex[] Regexes;

        public Dictionary<uint, ICleanCodeRule> CleanCodeRules { get; private set; }

        private uint rulesIDCounter = 0;

        private event System.Action CleanCodeSettingsReady;

        private void OnEnable()
        {
            if (CleanCodeRules == null)
            {
                CleanCodeRules = new Dictionary<uint, ICleanCodeRule>();
                UpdateRules();
                CleanCodeSettingsReady?.Invoke();
                CleanCodeSettingsReady = null;
            }
        }

        public void AddReadyListener(System.Action onReadyAction)
        {
            if (CleanCodeRules == null)
            {
                CleanCodeSettingsReady += onReadyAction;
            }
            else
            {
                onReadyAction?.Invoke();
            }
        }

        private void AddICleanCodeRule(ICleanCodeRule rules)
        {
            if(rules.IsValid())
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
                if(UnwantedCodes[i].GetID() == 0)
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
    }
}
