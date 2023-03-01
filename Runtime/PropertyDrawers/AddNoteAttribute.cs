using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Shows a Message in the inspector around the property
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class AddNoteAttribute : PropertyAttribute {

        // public enum NoteStyle {
        //     DEFAULT, BOLD, CENTERGREY, MINI, LARGE, WHITE,
        //     HELP, HELPERROR, HELPWARN, HELPINFO
        // }

        public bool dynamic { get; set; }
        public string noteLabel { get; set; }
        public string sourceField { get; set; }
        // public float labelWeight { get; set; }
        // public bool centered;
        // public NoteStyle style;

        public AddNoteAttribute(string sourceField, bool dynamic = false) {
            this.dynamic = dynamic;
            this.sourceField = sourceField;
            this.noteLabel = sourceField;
        }
    }
}