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

        public Dictionary<int, IScanable> scanables { get; private set; }

        private int scanableIDCounter = 0;

        private void OnEnable()
        {
            if (scanables == null)
            {
                scanables = new Dictionary<int, IScanable>();
                UpdateScanables();
            }
        }

        private void AddIScanable(IScanable scanable)
        {
            if(scanable.IsValid())
                scanables.Add(scanable.GetID(), scanable);
        }

        public void UpdateScanables()
        {
            scanables.Clear();
            if (UnwantedCodes == null || CodeGuidelines == null || CodeDocumentations == null) return;

            for (int i = 0; i < UnwantedCodes.Length; ++i)
            {
                if(UnwantedCodes[i].GetID() == 0)
                {
                    UnwantedCodes[i].SetID(++scanableIDCounter);
                }
                AddIScanable(UnwantedCodes[i]);
            }

            for (int i = 0; i < CodeGuidelines.Length; ++i)
            {
                if (CodeGuidelines[i].GetID() == 0)
                {
                    CodeGuidelines[i].SetID(++scanableIDCounter);
                }
                AddIScanable(CodeGuidelines[i]);
            }

            for (int i = 0; i < CodeDocumentations.Length; ++i)
            {
                if (CodeDocumentations[i].GetID() == 0)
                {
                    CodeDocumentations[i].SetID(++scanableIDCounter);
                }
                AddIScanable(CodeDocumentations[i]);
            }
        }

        public CodeManagerRegex[] Regexes;
    }
}
