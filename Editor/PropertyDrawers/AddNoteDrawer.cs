using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(AddNoteAttribute))]
    public class AddNoteDrawer : DecoratorDrawer {

        AddNoteAttribute addNote => (AddNoteAttribute)attribute;

        SerializedProperty property;

        public override VisualElement CreatePropertyGUI() {

            Label note = new Label();
            string noteText = GetNoteText(note);
            note.text = noteText;
            note.AddToClassList("add-note-label");
            // todo styling

            // todo updating dynamically
            note.RegisterCallback<ClickEvent>(ce => {
                // update on click?
                Debug.Log($"note {note.name} {note.text} clicked!");
            });

            return note;
        }

        public string GetNoteText(VisualElement root) {
            if (addNote.dynamic) {
                property ??= SerializedPropertyExtensions.GetBindedSPropFromDecorator(root);
                if (property.TryGetValueOnPropRefl<string>(addNote.sourceField, out var v)) {
                    return v;
                }
            }
            return addNote.noteLabel;
        }
    }
}