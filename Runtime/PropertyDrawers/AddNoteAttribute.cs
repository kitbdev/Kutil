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

        /// <summary>approximate readonly text color as a rich text prefix</summary>
        public static readonly string readOnlyRichTagPrefix = "<color=#aaa>";


        public bool useField { get; set; }
        public bool dynamic { get; set; }
        public string noteLabel { get; set; }
        public string sourceField { get; set; }
        public bool richText { get; set; }
        public Func<object, string> f { get; set; }

        // todo? multiline? or \n auto does that?
        // todo? T->string func - addnote data class? - this could just be a property, actually
        // todo tooltip option

        /// <summary>
        /// Shows a label from a source field.
        /// <a href="https://docs.unity3d.com/2022.2/Documentation/Manual/UIE-supported-tags.html">rich text ref:</a>
        /// &lt;color="red"&gt;&lt;color=#FF9900&gt;&lt;align="center"&gt;&lt;b&gt;&lt;font-weight="700"&gt;
        /// '&lt;'=&amp; lt; 
        /// </summary>
        /// <param name="sourceField">nameof(sourcefield) should be a string, can be a parameter</param>
        /// <param name="dynamic">should the note be updated whenever a field is updated?</param>
        /// <param name="richText"></param>
        public AddNoteAttribute(bool dynamic, string sourceField, bool richText = true) {
            this.dynamic = dynamic;
            this.sourceField = sourceField;
            this.noteLabel = sourceField;
            this.richText = richText;
            this.useField = true;
        }
        /// <summary>
        /// Shows a static label.
        /// rich text: 
        /// &lt;color="red"&gt;&lt;color=#FF9900&gt;&lt;align="center"&gt;&lt;b&gt;&lt;font-weight="700"&gt;
        /// '&lt;'=&amp; lt; 
        /// </summary>
        /// <param name="noteLabel">static text to use</param>
        /// <param name="richText"></param>
        public AddNoteAttribute(string noteLabel, bool richText = true) {
            this.sourceField = noteLabel;
            this.noteLabel = noteLabel;
            this.richText = richText;
            this.useField = false;
            this.dynamic = false;
        }
    }
}