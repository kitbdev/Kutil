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
            container.generateVisualContent += DrawVec;
            // container.style.height = 50;
            // container.Add(new PieChart());

            var vec2Field = new Vector2Field();
            vec2Field.label = property.displayName;
            vec2Field.bindingPath = property.propertyPath;
            container.Add(vec2Field);
            Vec2DDrawField vecDrawField = new Vec2DDrawField();
            vecDrawField.label = property.displayName;
            vecDrawField.bindingPath = property.propertyPath;
            container.Add(vecDrawField);

            return container;
        }

        private void DrawVec(MeshGenerationContext ctx) {

        }
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

    Vec2DDrawInput vecDrawInput;

    public Vec2DDrawField() : this(null) { }
    public Vec2DDrawField(string label) : base(label, new Vec2DDrawInput()) {
        vecDrawInput = this.Q<Vec2DDrawInput>();

        AddToClassList(ussClassName);
        labelElement.AddToClassList(labelUssClassName);
        vecDrawInput.AddToClassList(inputUssClassName);

    }

    // todo event binding to allow setting

    public override void SetValueWithoutNotify(Vector2 newValue) {
        base.SetValueWithoutNotify(newValue);
        ((INotifyValueChanged<Vector2>)vecDrawInput).SetValueWithoutNotify(rawValue);
    }

}

class Vec2DDrawInput : VisualElement, INotifyValueChanged<Vector2> {

    public float size = 100;
    public float spacing = 5;
    public Color bgColor = new Color(0.2f, 0.2f, 0.2f);
    public Color outlineColor = new Color(0.7f, 0.7f, 0.7f);
    public Color lineColor = Color.green;

    protected Label vecLabel;

    public Vec2DDrawInput() {
        style.width = size;
        style.height = size;
        style.maxWidth = size;
        style.maxWidth = size;
        generateVisualContent += DrawCanvas;
        vecLabel = new Label();
        vecLabel.style.position = Position.Absolute;
        vecLabel.style.right = 0;
        vecLabel.style.bottom = 0;
        Add(vecLabel);
    }

    Vector2 _value;
    public Vector2 value {
        get { return _value; }
        set {
            _value = value;
            UpdateVisual();
        }
    }

    private void UpdateVisual() {
        vecLabel.text = $"m:{value.magnitude,2}";
        this.MarkDirtyRepaint();
    }

    public void SetValueWithoutNotify(Vector2 newValue) {
        value = newValue;
    }

    void DrawCanvas(MeshGenerationContext ctx) {
        Rect bgrect = ctx.visualElement.contentRect;
        var painter2D = ctx.painter2D;

        // painter2D.lineWidth = 0.0f;
        // painter2D.strokeColor = bgColor;
        painter2D.fillColor = bgColor;

        painter2D.BeginPath();
        painter2D.MoveTo(new Vector2(0, 0));
        painter2D.LineTo(new Vector2(0, size));
        painter2D.LineTo(new Vector2(size, size));
        painter2D.LineTo(new Vector2(size, 0));
        painter2D.Fill();

        Vector2 centerPos = new Vector2(size / 2, size / 2);
        float radius = size / 2 - spacing;
        painter2D.lineWidth = 1.0f;
        painter2D.strokeColor = outlineColor;
        painter2D.BeginPath();
        // painter2D.MoveTo(centerPos);
        painter2D.Arc(centerPos, radius, Angle.Turns(0), Angle.Turns(1));
        painter2D.Stroke();

        painter2D.strokeColor = lineColor;
        painter2D.fillColor = lineColor;
        painter2D.BeginPath();
        painter2D.MoveTo(centerPos);
        Vector2 target = value;
        if (target == Vector2.zero) {
            // painter2D.lineWidth = 2;
            painter2D.Arc(centerPos, 2, Angle.Turns(0), Angle.Turns(1));
            painter2D.Fill();
            // painter2D.LineTo(centerPos + Vector2.up * 0.001f);
        } else {
            var targetPos = new Vector2(target.x, -target.y);
            painter2D.LineTo(centerPos + targetPos * radius);
            painter2D.Stroke();
        }
    }
}
public class PieChart : VisualElement {
    float m_Radius = 100.0f;
    float m_Value = 40.0f;

    VisualElement m_Chart;

    public float radius {
        get => m_Radius;
        set {
            m_Radius = value;
            m_Chart.style.height = diameter;
            m_Chart.style.width = diameter;
            m_Chart.MarkDirtyRepaint();
        }
    }

    public float diameter => m_Radius * 2.0f;

    public float value {
        get { return m_Value; }
        set { m_Value = value; MarkDirtyRepaint(); }
    }

    public PieChart() {
        generateVisualContent += DrawCanvas;
        style.height = diameter;
    }

    void DrawCanvas(MeshGenerationContext ctx) {
        var painter = ctx.painter2D;
        painter.strokeColor = Color.white;
        painter.fillColor = Color.white;

        var percentage = m_Value;

        var percentages = new float[] {
            percentage, 100 - percentage
        };
        var colors = new Color32[] {
            new Color32(182,235,122,255),
            new Color32(251,120,19,255)
        };
        float angle = 0.0f;
        float anglePct = 0.0f;
        int k = 0;
        foreach (var pct in percentages) {
            anglePct += 360.0f * (pct / 100);

            painter.fillColor = colors[k++];
            painter.BeginPath();
            painter.MoveTo(new Vector2(m_Radius, m_Radius));
            painter.Arc(new Vector2(m_Radius, m_Radius), m_Radius, angle, anglePct);
            painter.Fill();

            angle = anglePct;
        }
    }
}