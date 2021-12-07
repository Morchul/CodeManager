using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Morchul.CodeManager
{
    public class RegexTesterMatches : ScriptableObject
    {
        public RegexTesterMatch[] Matches;
    }

    [System.Serializable]
    public struct RegexTesterMatch
    {

        public static RegexTesterMatch Null;

        public string MatchText;

        public int LineIndex;
        public float LinePosIndex;

        public RegexTesterGroup[] MatchGroups;

        public bool IsNull()
        {
            return string.IsNullOrEmpty(MatchText);
        }
    }

    [System.Serializable]
    public struct RegexTesterGroup
    {
        public string Name;
        public string Value;
    }
}