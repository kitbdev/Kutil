using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace Kutil {
    [CustomPropertyDrawer(typeof(Vector2DDrawAttribute))]
    public class Vector2DDrawDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.name = "Vector2DDraw";

            Vector2DDrawAttribute vec2DData = (Vector2DDrawAttribute)attribute;

            var vec2Field = new Vector2Field();
            vec2Field.label = property.displayName;
            vec2Field.bindingPath = property.propertyPath;
            container.Add(vec2Field);
            Vec2DDrawField vecDrawField = new Vec2DDrawField();
            // vecDrawField.label = property.displayName;
            vecDrawField.bindingPath = property.propertyPath;
            // if (vec2DData.color != null) {
            //     vecDrawField.vec2DrawInput.lineColor = vec2DData.color;
            // }
            vecDrawField.vec2DrawInput.size = vec2DData.height;
            vecDrawField.vec2DrawInput.normalize = vec2DData.normalize;
            container.Add(vecDrawField);

            return container;
        }
    }
    public class Vec2DDrawField : BaseField<Vector2> {
        // ref https://github.com/Unity-Technologies/UnityCsReference/blob/664dfe30cee8ee2ef7dd8c5e9db6235915245ecb/ModuleOverrides/com.unity.ui/Core/Controls/InputField/TextField.cs

        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "kutil-vec2d-draw-field";
        /// <summary>
        /// USS class name of labels in elements of this type.
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";
        /// <summary>
        /// USS class name of input elements in elements of this type.
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        public Vec2DDrawInput vec2DrawInput;

        public Vec2DDrawField() : this(null) { }
        public Vec2DDrawField(string label) : base(label, new Vec2DDrawInput()) {
            vec2DrawInput = this.Q<Vec2DDrawInput>();

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            vec2DrawInput.AddToClassList(inputUssClassName);

        }

        // todo event binding to allow setting

        public override void SetValueWithoutNotify(Vector2 newValue) {
            base.SetValueWithoutNotify(newValue);
            ((INotifyValueChanged<Vector2>)vec2DrawInput).SetValueWithoutNotify(rawValue);
        }

    }

    /// <summary>
    /// Draws a circle and a line representing the vector
    /// </summary>
    public class Vec2DDrawInput : VisualElement, INotifyValueChanged<Vector2> {

        private float _size = 100;
        public float spacing = 5;
        // public Color bgColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _outlineColor = new Color(0.7f, 0.7f, 0.7f);
        private Color _lineColor = Color.green;
        private bool _normalize = false;

        protected Label vecLabel;

        public Vec2DDrawInput() : this(100) { }
        public Vec2DDrawInput(float size) {
            this._size = size;
            generateVisualContent += DrawCanvas;
            vecLabel = new Label();
            vecLabel.style.position = Position.Absolute;
            vecLabel.style.right = 0;
            vecLabel.style.bottom = 0;
            vecLabel.style.top = size;
            vecLabel.text = "vec label";
            Add(vecLabel);
            Resize(size);
        }

        Vector2 _value;
        public Vector2 value {
            get { return _value; }
            set {
                _value = value;
                UpdateVisual();
            }
        }
        public float size { get => _size; set => Resize(value); }
        public bool normalize { get => _normalize; set { _normalize = value; UpdateVisual(); } }
        public Color lineColor { get => _lineColor; set { _lineColor = value; UpdateVisual(); } }
        public Color outlineColor { get => _outlineColor; set { _outlineColor = value; UpdateVisual(); } }

        private void UpdateVisual() {
            if (vecLabel == null) return;
            vecLabel.text = $"m:{value.magnitude:f2}";
            this.MarkDirtyRepaint();
        }

        public void SetValueWithoutNotify(Vector2 newValue) {
            value = newValue;
        }

        public void Resize(float newSize) {
            // vecLabel.RegisterCallback<GeometryChangedEvent>();
            _size = newSize;
            style.width = size;
            style.height = size;
            style.maxWidth = size;
            style.maxWidth = size;
            UpdateVisual();
        }

        void DrawCanvas(MeshGenerationContext ctx) {
            Rect bgrect = ctx.visualElement.contentRect;
            var painter2D = ctx.painter2D;

            // bg
            // painter2D.fillColor = bgColor;
            // painter2D.BeginPath();
            // painter2D.MoveTo(new Vector2(0, 0));
            // painter2D.LineTo(new Vector2(0, size));
            // painter2D.LineTo(new Vector2(size, size));
            // painter2D.LineTo(new Vector2(size, 0));
            // painter2D.Fill();

            // outline circle
            Vector2 centerPos = new Vector2(size / 2, size / 2);
            float radius = size / 2 - spacing;
            // Debug.Log($"s{size} c{centerPos} r{radius}");
            painter2D.lineWidth = 1.0f;
            painter2D.strokeColor = outlineColor;
            painter2D.BeginPath();
            // painter2D.MoveTo(centerPos);
            painter2D.Arc(centerPos, radius, Angle.Turns(0), Angle.Turns(1));
            painter2D.Stroke();

            // line
            painter2D.strokeColor = lineColor;
            painter2D.fillColor = lineColor;
            painter2D.BeginPath();
            painter2D.MoveTo(centerPos);
            Vector2 target = value;
            if (target == Vector2.zero) {
                // Debug.Log("target is zero" + value);
                // draw circle in center instead
                painter2D.Arc(centerPos, 2, Angle.Turns(0), Angle.Turns(1));
                painter2D.Fill();
                // painter2D.LineTo(centerPos + Vector2.up * 0.001f);
            } else {
                if (normalize) {
                    target = Vector2.ClampMagnitude(target, 1f);
                }
                var targetPos = new Vector2(target.x, -target.y);
                // Debug.Log($"target {value} {targetPos} {centerPos + targetPos * radius} {lineColor}");
                painter2D.LineTo(centerPos + targetPos * radius);
                painter2D.Stroke();
            }
        }
        // protected override void ExecuteDefaultAction(EventBase evt)
        // {
        //     base.ExecuteDefaultAction(evt);
        // }
    }
}