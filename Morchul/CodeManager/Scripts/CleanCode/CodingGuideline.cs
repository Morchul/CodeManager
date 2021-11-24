using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    [System.Serializable]
    public struct CodingGuideline
    {
        public bool NewLineBeforeOpeningCurlyBrackets;

        public string PrivateFieldRegex;
        public string PublicFieldRegex;
        public string ProtectedFieldRegex;
        public string ConstFieldRegex;
        public string StaticFieldRegex;
        public string PropertieRegex;
        public string ClassNameRegex;
        public string MethodNameRegex;
    }
}
