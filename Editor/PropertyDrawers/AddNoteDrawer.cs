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
        InspectorElement inspectorElement;

        AddNoteAttribute addNote => (AddNoteAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            property = null;

            note = new Label();
            note.AddToClassList(addNoteClass);
            note.enableRichText = addNote.richText;
            note.name = "AddNoteLabel";

            note.text = addNote.noteLabel ?? "missing label!";

            if (addNote.useField) {
                note.RegisterCallback<GeometryChangedEvent>(OnGeoChanged);
            }

            return note;
        }

        private void OnGeoChanged(GeometryChangedEvent ce) {
            note.UnregisterCallback<GeometryChangedEvent>(OnGeoChanged);

            if (addNote.dynamic) {
                var propertyField = note.GetFirstAncestorOfType<PropertyField>();
                if (propertyField == null) {
                    Debug.LogError($"{GetType().Name} decorator failed to find containing property!");
                    return;
                }
                // get inspector element to register an onvalidate callback
                inspectorElement = propertyField.GetFirstAncestorOfType<InspectorElement>();
                if (inspectorElement == null) {
                    Debug.LogError($"AddNote - inspectorElement null!");
                    return;
                }
                // this properly responds to all changes
                inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
                note.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            }
            UpdateLabel();
        }

        private void OnUpdate(SerializedPropertyChangeEvent changeEvent) => UpdateLabel();
        void OnDetach(DetachFromPanelEvent detachFromPanelEvent) {
            inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            property = null;
        }
        public void UpdateLabel() {
            note.text = GetNoteText(note);
            // Debug.Log($"note {note.name} changed to '{note.text}' from source {addNote.sourceField} on {property?.propertyPath??"none"}");
        }

        public string GetNoteText(VisualElement root) {
            if (addNote.useField) {
                if (property == null) property = SerializedPropertyExtensions.GetBindedPropertyFromDecorator(root);
                if (property == null) {
                    Debug.LogError("AddNote use field but no prop found for " + addNote.noteLabel);
                    return $"Error on property for {addNote.sourceField}";
                }
                if (property.TryGetValueOnPropRefl<string>(addNote.sourceField, out var v)) {
                    return v;
                }
                if (property.TryGetValueOnPropRefl<object>(addNote.sourceField, out var o)) {
                    return o.ToString();
                }
                return $"Error no value found: {addNote.sourceField}";
            }
            return addNote.noteLabel;
        }
    }
}