using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class CleanCodeSettings : ScriptableObject
    {
        public UnwantedCode[] UnwantedCodes;
        public CodeGuideline[] CodeGuidelines;
        public CodeDocumentation[] CodeDocumentations;

        public CodeManagerRegex DocumentationRegex;

        public Dictionary<int, ICleanCodeRule> cleanCodeRules { get; private set; }

        private int rulesIDCounter = 0;

        private void OnEnable()
        {
            if (cleanCodeRules == null)
            {
                cleanCodeRules = new Dictionary<int, ICleanCodeRule>();
                UpdateRules();
            }
        }

        private void AddICleanCodeRule(ICleanCodeRule rules)
        {
            if(rules.IsValid())
                cleanCodeRules.Add(rules.GetID(), rules);
        }

        public void UpdateRules()
        {
            cleanCodeRules.Clear();
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

        public CodeManagerRegex[] Regexes;
    }
}
