using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class CleanCodeSettings : ScriptableObject
    {
        public UnwantedCode[] UnwantedCodes;
        public CodingGuideline CodingGuidelines;
    }
}
