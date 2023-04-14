using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    [CustomPropertyDrawer(typeof(AddNoteAttribute))]
    public class AddNoteDrawer : ExtendedDecoratorDrawer {

        public static readonly string addNoteClass = "add-note-label";


        AddNoteAttribute addNote => (AddNoteAttribute)attribute;

        public override bool registerUpdateCall => addNote.dynamic;

        public override VisualElement CreatePropertyGUI() {
            ExtendedDecoratorData data = new();

            Label note = new Label();
            data.decorator = note;
            note.AddToClassList(addNoteClass);
            note.enableRichText = addNote.richText;
            note.name = "AddNoteLabel";

            note.text = addNote.noteLabel ?? "missing label!";

            return note;
        }
        protected override void Setup(ExtendedDecoratorData data) {
            base.Setup(data);

            if (!addNote.useField) {
                return;
            }

            UpdateLabel(data);
        }


        protected override void OnUpdate(SerializedPropertyChangeEvent changeEvent, ExtendedDecoratorData data) => UpdateLabel(data);
        
        public void UpdateLabel(ExtendedDecoratorData data) {
            Label note = (Label)data.decorator;
            note.text = GetNoteText(note, data);
            // Debug.Log($"note {note.name} changed to '{note.text}' from source {addNote.sourceField} on {property?.propertyPath??"none"}");
        }

        public string GetNoteText(VisualElement root, ExtendedDecoratorData data) {
            if (addNote.useField) {
                if (data.serializedProperty.TryGetValueOnPropRefl<string>(addNote.sourceField, out var v)) {
                    return v;
                }
                if (data.serializedProperty.TryGetValueOnPropRefl<object>(addNote.sourceField, out var o)) {
                    return o.ToString();
                }
                return $"Error no value found: {addNote.sourceField}";
            }
            return addNote.noteLabel;
        }
    }
}