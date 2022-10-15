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

        public bool dynamic;
        public string noteLabel;
        public string sourceField;
        public NoteLayout noteLayout;
        public float labelWidth;
        public bool centered;
        public NoteStyle style;
        // note can combine with conditional hide and replace mode to have a conditional label

        public AddNoteAttribute(string label = "note", NoteLayout layout = NoteLayout.BEFORE, float width = 50, bool centered = true, NoteStyle style = NoteStyle.DEFAULT) {
            this.noteLabel = label;
            this.noteLayout = layout;
            this.labelWidth = width;
            this.style = style;
            this.centered = centered;
            // this.conditionField = conditionalField;
            dynamic = false;
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
    }
}