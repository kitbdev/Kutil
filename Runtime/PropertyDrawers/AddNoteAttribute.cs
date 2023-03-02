using System;
using UnityEngine;

namespace Kutil {
    // example [Kutil.AddNote("<align=\"center\"><size=\"18\"><font-weight=\"700\">SpellWeb<br>Editor")]
    /// <summary>
    /// Shows a Label in the Inspector above the property.
    /// Rich text ref: https://docs.unity3d.com/2022.2/Documentation/Manual/UIE-supported-tags.html
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class AddNoteAttribute : PropertyAttribute {

        public bool dynamic { get; set; }
        public string noteLabel { get; set; }
        public string sourceField { get; set; }
        public bool richText { get; set; }

        /// <summary>
        /// Shows a label. 
        /// <a href="https://docs.unity3d.com/2022.2/Documentation/Manual/UIE-supported-tags.html">rich text ref:</a>
        /// &lt;color="red"&gt; &lt;align="center"&gt; &lt;b&gt; &lt;font-weight="700"&gt; '&lt;'=&amp; lt;
        /// </summary>
        /// <param name="sourceField"></param>
        /// <param name="dynamic"></param>
        /// <param name="richText"></param>
        public AddNoteAttribute(string sourceField, bool dynamic = false, bool richText = true) {
            this.dynamic = dynamic;
            this.sourceField = sourceField;
            this.noteLabel = sourceField;
            this.richText = richText;
        }
    }
}