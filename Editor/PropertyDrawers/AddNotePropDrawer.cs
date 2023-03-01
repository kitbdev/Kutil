using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(AddNotePropAttribute))]
    public class AddNotePropDrawer : PropertyDrawer {

        AddNotePropAttribute anAtt => (AddNotePropAttribute)attribute;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            
            var propField = new PropertyField(property);
            
            string noteText = GetNoteText(property);
            Label note = new Label();
            note.text = noteText;
            note.AddToClassList("add-note-label");
            // todo styling
            
            VisualElement relPropContainer = RelativePropertyDrawer.CreateRelPropertyGUI(propField, note, anAtt.labelLayout, anAtt.labelWeight);
            relPropContainer.name = "AddButton" + relPropContainer.name;
            return relPropContainer;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // AddNoteAttribute anAtt = (AddNoteAttribute)attribute;
            // for some reason label gets cleared after get height
            var proplabel = new GUIContent(label);
            string noteText = GetNoteText(property);

            int numLines = noteText.Count(c => c == '\n') + 1;
            float noteHeight = EditorGUIUtility.singleLineHeight * numLines;
            float propHeight = EditorGUI.GetPropertyHeight(property, proplabel);
            Rect noteRect = position;
            noteRect.height = noteHeight;
            Rect propRect = position;
            propRect.height = propHeight;

            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.REPLACE) {
                DrawNote(noteRect, property, noteText);
                return;
            }
            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.BEFORE) {
                propRect.y += noteHeight;
                DrawNote(noteRect, property, noteText);
            }
            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.LEFT
            || anAtt.noteLayout == AddNotePropAttribute.NoteLayout.RIGHT) {
                // small button and move prop
                float labelWidth = anAtt.labelWidth;
                float spacing = 5;
                noteRect.width = labelWidth;
                propRect.width -= labelWidth - spacing;
                if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.LEFT) {
                    propRect.x = labelWidth + spacing;
                    EditorGUI.indentLevel += 1;
                    DrawNote(noteRect, property, noteText);
                }
                if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.RIGHT) {
                    noteRect.x = propRect.width + spacing;
                    DrawNote(noteRect, property, noteText);
                }
            }
            EditorGUI.PropertyField(propRect, property, proplabel, true);
            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.LEFT) {
                EditorGUI.indentLevel -= 1;
            }
            // EditorGUI.PropertyField(propRect, property, label, true);
            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.AFTER) {
                noteRect.y += noteHeight;
                DrawNote(noteRect, property, noteText);
            }
        }
        void DrawNote(Rect rect, SerializedProperty property, string noteText) {
            // if (anAtt.conditionField!=null){
            // }
            if (anAtt.style == AddNotePropAttribute.NoteStyle.HELP ||
            anAtt.style == AddNotePropAttribute.NoteStyle.HELPERROR ||
            anAtt.style == AddNotePropAttribute.NoteStyle.HELPWARN ||
            anAtt.style == AddNotePropAttribute.NoteStyle.HELPINFO) {
                var msg = MessageType.None;
                if (anAtt.style == AddNotePropAttribute.NoteStyle.HELPERROR) msg = MessageType.Error;
                if (anAtt.style == AddNotePropAttribute.NoteStyle.HELPWARN) msg = MessageType.Warning;
                if (anAtt.style == AddNotePropAttribute.NoteStyle.HELPINFO) msg = MessageType.Info;
                EditorGUI.HelpBox(rect, noteText, msg);
                return;
            }
            GUIStyle style;
            switch (anAtt.style) {
                case AddNotePropAttribute.NoteStyle.DEFAULT:
                    style = EditorStyles.label;
                    break;
                case AddNotePropAttribute.NoteStyle.BOLD:
                    style = EditorStyles.boldLabel;
                    break;
                case AddNotePropAttribute.NoteStyle.CENTERGREY:
                    style = EditorStyles.centeredGreyMiniLabel;
                    break;
                case AddNotePropAttribute.NoteStyle.MINI:
                    style = EditorStyles.miniLabel;
                    break;
                case AddNotePropAttribute.NoteStyle.LARGE:
                    style = EditorStyles.largeLabel;
                    break;
                case AddNotePropAttribute.NoteStyle.WHITE:
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
            GUI.Label(rect, noteText, style);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // AddNoteAttribute anAtt = (AddNoteAttribute)attribute;
            // todo cache?
            int numLines = GetNoteText(property).Count(c => c == '\n') + 1;
            float lheight = EditorGUIUtility.singleLineHeight * numLines;
            float propHeight = EditorGUI.GetPropertyHeight(property, label);

            if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.NONE) {
                return propHeight;
            } else if (anAtt.noteLayout == AddNotePropAttribute.NoteLayout.REPLACE) {
                return lheight;
            }
            // before or after
            float height = lheight + propHeight;
            return height;
        }
        public string GetNoteText(SerializedProperty property) {
            if (anAtt.dynamic) {
                if (property.TryGetValueOnPropRefl<string>(anAtt.sourceField, out var v)) {
                    return v;
                }
            }
            return anAtt.noteLabel;
        }
    }
}