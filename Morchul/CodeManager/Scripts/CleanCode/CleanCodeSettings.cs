using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class CleanCodeSettings : ScriptableObject
    {
        public DocumentationFlags DocumentationFlag;
        public UnwantedCode[] UnwantedCodes;
        public CodingGuidelines CodingGuidelines;

        public bool IsFlagSet(DocumentationFlags flag)
        {
            return (DocumentationFlag & flag) > 0;
        }
    }
}
