using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Shows a Message in the inspector around the property
    /// </summary>
    public class AddNoteAttribute : PropertyAttribute {

        public enum NoteLayout {
            BEFORE, REPLACE, AFTER,
            NONE, LEFT, RIGHT
        }
        public enum NoteStyle {
            DEFAULT, BOLD, CENTERGREY, MINI, LARGE, WHITE,
            HELP, HELPERROR, HELPWARN, HELPINFO
        }

        public bool dynamic { get; set; }
        public string noteLabel { get; set; }
        public string sourceField { get; set; }
        public NoteLayout noteLayout;
        public float labelWidth;
        public bool centered;
        public NoteStyle style;
        public PropLayout labelLayout { get; set; }
        public float labelWeight { get; set; }

        // public float labelWeight;

        // note can combine with conditional hide and replace mode to have a conditional label

        public AddNoteAttribute(string label = "note", NoteLayout layout = NoteLayout.BEFORE, float width = 50, bool centered = true, NoteStyle style = NoteStyle.DEFAULT) {
            this.dynamic = false;
            this.noteLabel = label;
            this.noteLayout = layout;
            this.labelWidth = width;
            this.style = style;
            this.centered = centered;
            // this.conditionField = conditionalField;
        }
        public AddNoteAttribute(string sourceField, int dynamic, NoteLayout layout = NoteLayout.BEFORE, float width = 50, bool centered = true, NoteStyle style = NoteStyle.DEFAULT) {
            this.dynamic = true;
            this.sourceField = sourceField;
            this.noteLabel = "AddNote Property error";
            this.noteLayout = layout;
            this.labelWidth = width;
            this.style = style;
            this.centered = centered;
            // this.conditionField = conditionalField;
        }
        public AddNoteAttribute(string label = "note", PropLayout labelLayout = PropLayout.Above, float labelWeight = 1) {
            this.dynamic = false;
            this.noteLabel = label;
            this.labelLayout = labelLayout;
            this.labelWeight = labelWeight;
        }
        public AddNoteAttribute(string sourceField, int dynamic, PropLayout labelLayout = PropLayout.Above, float labelWeight = 1) {
            this.dynamic = true;
            this.sourceField = sourceField;
            this.noteLabel = "AddNote Property error";
            this.labelLayout = labelLayout;
            this.labelWeight = labelWeight;
        }
    }
}