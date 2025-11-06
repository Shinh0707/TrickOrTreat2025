using System;
using UnityEngine;

namespace Halloween.Utility.Property
{
    [Serializable]
    public struct RichTextProperty
    {
        [TextArea]
        public string text;
    }
}