using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(AddNoteAttribute))]
    public class AddNoteDrawer : DecoratorDrawer {

        public static readonly string addNoteClass = "add-note-label";

        SerializedProperty property;
        Label note;

        AddNoteAttribute addNote => (AddNoteAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            
            note = new Label();
            note.AddToClassList(addNoteClass);
            note.enableRichText = addNote.richText;

            note.text = GetNoteText(note);

            if (addNote.dynamic) {
                // todo update dynamically if dynamic
                note.RegisterCallback<GeometryChangedEvent>(ce => {
                    note.text = GetNoteText(note);
                    Debug.Log($"note {note.name} {note.text} changed!");
                });
            }
            
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