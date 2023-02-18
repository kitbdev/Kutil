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
            vecDrawField.vec2DrawInput.clampOne = vec2DData.clampOne;
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

            // vec2DrawInput.bindingPath = this.bindingPath;

            AddToClassList(ussClassName);
            AddToClassList(alignedFieldUssClassName);
            labelElement.AddToClassList(labelUssClassName);
            vec2DrawInput.AddToClassList(inputUssClassName);
            vec2DrawInput.RegisterValueChangedCallback<Vector2>(ChangeCallback);
        }

        public override void SetValueWithoutNotify(Vector2 newValue) {
            base.SetValueWithoutNotify(newValue);
            ((INotifyValueChanged<Vector2>)vec2DrawInput).SetValueWithoutNotify(rawValue);
        }

        private void ChangeCallback(ChangeEvent<Vector2> evt) {
            // Debug.Log("Value changed!");
            value = evt.newValue;
        }


        /// <summary>
        /// Draws a circle and a line representing the vector
        /// </summary>
        public class Vec2DDrawInput : BindableElement, INotifyValueChanged<Vector2> {


            private float _size = 100;
            public float spacing = 5;
            // public Color bgColor = new Color(0.2f, 0.2f, 0.2f);
            private Color _outlineColor = new Color(0.7f, 0.7f, 0.7f);
            private Color _lineColor = Color.green;
            private bool _normalize = false;
            private bool _clampOne = false;
            float _snapIncrementDeg = 15;

            protected Label vecLabel;

            Vector2 _value;
            //ref https://github.com/Unity-Technologies/UnityCsReference/blob/90a56242216dd93f531dcad02e824d3fea8a3ab1/ModuleOverrides/com.unity.ui/Core/TextElement.cs
            public Vector2 value {
                get { return _value; }
                set {
                    if (_value != value) {
                        if (panel != null) {
                            // Emit
                            using (ChangeEvent<Vector2> evt = ChangeEvent<Vector2>.GetPooled(this._value, value)) {
                                evt.target = this;
                                ((INotifyValueChanged<Vector2>)this).SetValueWithoutNotify(value);
                                SendEvent(evt);
                            }
                        } else {
                            ((INotifyValueChanged<Vector2>)this).SetValueWithoutNotify(value);
                        }
                    }
                }
            }

            // public IBinding binding { get; set; }
            // public string bindingPath { get; set; }

            public float size { get => _size; set => Resize(value); }
            public float snapIncrementDeg { get => _snapIncrementDeg; set => _snapIncrementDeg = value; }
            public bool normalize { get => _normalize; set { _normalize = value; UpdateVisual(); } }
            public bool clampOne { get => _clampOne; set { _clampOne = value; UpdateVisual(); } }
            public Color lineColor { get => _lineColor; set { _lineColor = value; UpdateVisual(); } }
            public Color outlineColor { get => _outlineColor; set { _outlineColor = value; UpdateVisual(); } }


            Vector2 centerPos => Vector2.one * (size / 2f);
            float radius => size / 2f - spacing;

            public Vec2DDrawInput() : this(100) { }
            public Vec2DDrawInput(float size) {
                RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
                this._size = size;
                generateVisualContent += DrawCanvas;
                vecLabel = new Label();
                vecLabel.style.position = Position.Absolute;
                vecLabel.style.right = 0;
                vecLabel.style.bottom = 0;
                // vecLabel.style.left = size;
                // vecLabel.style.top = 0;
                // vecLabel.style.top = size;
                vecLabel.text = "vec label";
                Add(vecLabel);

                pickingMode = PickingMode.Position;
            }

            private void GeometryChangedCallback(GeometryChangedEvent evt) {
                Resize(size);
            }

            private void UpdateVisual() {
                if (vecLabel == null) return;
                vecLabel.text = $"m:{value.magnitude:f2}";
                this.MarkDirtyRepaint();
            }

            public void SetValueWithoutNotify(Vector2 newValue) {
                _value = newValue;
                // Debug.Log("Value updated!");
                UpdateVisual();
            }

            public void Resize(float newSize) {
                // float textHeight = vecLabel.style.height.value.value;
                float textHeight = vecLabel.resolvedStyle.height;
                if (textHeight == float.NaN || textHeight == 0) {
                    Debug.LogWarning("invalid label height!");
                    textHeight = 20;
                }
                _size = newSize;
                style.width = size;
                style.height = size + textHeight;
                style.maxWidth = size;
                style.maxHeight = size + textHeight;
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
                        target = target.normalized;
                    } else if (clampOne) {
                        target = Vector2.ClampMagnitude(target, 1f);
                    }
                    // var targetPos = new Vector2(target.x, -target.y);
                    Vector2 lpos = CenterVec(target);
                    // Debug.Log($"target {value} {targetPos} {centerPos + targetPos * radius} {lineColor}");
                    painter2D.LineTo(lpos);
                    painter2D.Stroke();
                }
            }

            private Vector2 CenterVec(Vector2 brvec) {
                return (centerPos + new Vector2(brvec.x, -brvec.y) * radius);
            }
            private Vector2 UnCenterVec(Vector2 cvec) {
                Vector2 c = cvec - centerPos;
                return new Vector2(c.x, -c.y) / radius;
            }

            protected override void ExecuteDefaultAction(EventBase evt) {
                base.ExecuteDefaultAction(evt);

                if (!enabledSelf) {
                    return;
                }
                if (evt.eventTypeId == PointerDownEvent.TypeId()) {
                    PointerDownEvent downEvt = (PointerDownEvent)evt;
                    if (downEvt.button == 0) {
                        // clicked
                        // Debug.Log("clicked");
                        downEvt.target.CapturePointer(downEvt.pointerId);
                    }
                } else if (evt.eventTypeId == PointerUpEvent.TypeId()) {
                    PointerUpEvent upEvt = (PointerUpEvent)evt;
                    if (upEvt.button == 0) {
                        // released
                        bool snap = upEvt.ctrlKey;
                        SetValueFromLP(upEvt.localPosition, snap);
                        upEvt.target.ReleasePointer(upEvt.pointerId);
                    }
                } else if (evt.eventTypeId == PointerMoveEvent.TypeId()) {
                    PointerMoveEvent moveEvt = (PointerMoveEvent)evt;
                    // if ((moveEvt.pressedButtons & (1 << (int)MouseButton.LeftMouse)) > 0) {
                    if (moveEvt.target.HasPointerCapture(moveEvt.pointerId)) {
                        bool snap = moveEvt.ctrlKey;
                        SetValueFromLP(moveEvt.localPosition, snap);
                    }

                    // } else if (evt.eventTypeId == PointerOverEvent.TypeId()) {
                    //     PointerOverEvent overEvt = (PointerOverEvent)evt;

                    // } else if (evt.eventTypeId == PointerOutEvent.TypeId()) {
                    //     PointerOutEvent outEvt = (PointerOutEvent)evt;
                }
            }

            private Vector2 SetValueFromLP(Vector2 localPosition, bool snapping = false) {
                Vector2 newVal = UnCenterVec(localPosition);
                // todo multiply by existing mag?
                if (normalize) {
                    newVal = newVal.normalized;
                } else if (clampOne) {
                    newVal = Vector2.ClampMagnitude(newVal, 1f);
                }
                // hold ctrl to snap
                if (snapping) {
                    const float deadZoneSqr = 0.01f;
                    if (newVal.sqrMagnitude < deadZoneSqr) {
                        newVal = Vector2.zero;
                    } else {
                        // Debug.Log("snapping");
                        float v = Vector2.SignedAngle(Vector2.right, newVal.normalized);
                        v = Mathf.Floor(v / snapIncrementDeg) * snapIncrementDeg;
                        newVal = GetXYDirection(v, newVal.magnitude);
                    }
                }
                // Debug.Log("moved " + newVal + " from " + moveEvt.localPosition / size);
                value = newVal;
                return newVal;
            }
            static Vector2 GetXYDirection(float angle, float magnitude) {
                float angler = angle * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(angler), Mathf.Sin(angler)) * magnitude;
            }
        }
    }
}