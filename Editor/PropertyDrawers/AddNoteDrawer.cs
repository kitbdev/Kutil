using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(AddNoteAttribute))]
    public class AddNoteDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AddNoteAttribute anAtt = (AddNoteAttribute)attribute;
            // for some reason label gets cleared after get height
            var proplabel = new GUIContent(label);
            int numLines = anAtt.noteLabel.Count(c => c == '\n') + 1;
            float noteHeight = EditorGUIUtility.singleLineHeight * numLines;
            float propHeight = EditorGUI.GetPropertyHeight(property, proplabel);
            Rect noteRect = position;
            noteRect.height = noteHeight;
            Rect propRect = position;
            propRect.height = propHeight;

            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.REPLACE) {
                DrawNote(noteRect, anAtt, property);
                return;
            }
            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.BEFORE) {
                propRect.y += noteHeight;
                DrawNote(noteRect, anAtt, property);
            }
            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.LEFT
            || anAtt.noteLayout == AddNoteAttribute.NoteLayout.RIGHT) {
                // small button and move prop
                float labelWidth = anAtt.labelWidth;
                float spacing = 5;
                noteRect.width = labelWidth;
                propRect.width -= labelWidth - spacing;
                if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.LEFT) {
                    propRect.x = labelWidth + spacing;
                    EditorGUI.indentLevel += 1;
                    DrawNote(noteRect, anAtt, property);
                }
                if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.RIGHT) {
                    noteRect.x = propRect.width + spacing;
                    DrawNote(noteRect, anAtt, property);
                }
            }
            EditorGUI.PropertyField(propRect, property, proplabel, true);
            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.LEFT) {
                EditorGUI.indentLevel -= 1;
            }
            // EditorGUI.PropertyField(propRect, property, label, true);
            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.AFTER) {
                noteRect.y += noteHeight;
                DrawNote(noteRect, anAtt, property);
            }
        }
        void DrawNote(Rect rect, AddNoteAttribute anAtt, SerializedProperty property) {
            // if (anAtt.conditionField!=null){
            // }
            if (anAtt.style == AddNoteAttribute.NoteStyle.HELP ||
            anAtt.style == AddNoteAttribute.NoteStyle.HELPERROR ||
            anAtt.style == AddNoteAttribute.NoteStyle.HELPWARN ||
            anAtt.style == AddNoteAttribute.NoteStyle.HELPINFO) {
                var msg = MessageType.None;
                if (anAtt.style == AddNoteAttribute.NoteStyle.HELPERROR) msg = MessageType.Error;
                if (anAtt.style == AddNoteAttribute.NoteStyle.HELPWARN) msg = MessageType.Warning;
                if (anAtt.style == AddNoteAttribute.NoteStyle.HELPINFO) msg = MessageType.Info;
                EditorGUI.HelpBox(rect, anAtt.noteLabel, msg);
                return;
            }
            GUIStyle style;
            switch (anAtt.style) {
                case AddNoteAttribute.NoteStyle.DEFAULT:
                    style = EditorStyles.label;
                    break;
                case AddNoteAttribute.NoteStyle.BOLD:
                    style = EditorStyles.boldLabel;
                    break;
                case AddNoteAttribute.NoteStyle.CENTERGREY:
                    style = EditorStyles.centeredGreyMiniLabel;
                    break;
                case AddNoteAttribute.NoteStyle.MINI:
                    style = EditorStyles.miniLabel;
                    break;
                case AddNoteAttribute.NoteStyle.LARGE:
                    style = EditorStyles.largeLabel;
                    break;
                case AddNoteAttribute.NoteStyle.WHITE:
                    style = EditorStyles.whiteLabel;
                    break;
                default:
                    style = EditorStyles.label;
                    break;
            }
            style = new GUIStyle(style);
            if (anAtt.centered) {
                style.alignment = TextAnchor.UpperCenter;
            }
            GUI.Label(rect, anAtt.noteLabel, style);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            AddNoteAttribute anAtt = (AddNoteAttribute)attribute;
            int numLines = anAtt.noteLabel.Count(c => c == '\n') + 1;
            float lheight = EditorGUIUtility.singleLineHeight * numLines;
            float propHeight = EditorGUI.GetPropertyHeight(property, label);

            if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.NONE) {
                return propHeight;
            } else if (anAtt.noteLayout == AddNoteAttribute.NoteLayout.REPLACE) {
                return lheight;
            }
            // before or after
            float height = lheight + propHeight;
            return height;
        }

    }
}