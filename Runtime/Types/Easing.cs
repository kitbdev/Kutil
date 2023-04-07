using Unity.Mathematics;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Kutil {

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Easing))]
    public class EasingDrawer : PropertyDrawer {

        public static readonly string easingName = "kutil-easing";
        VisualElement root;
        PropertyField shapeField;
        PropertyField inOutField;
        // PropertyField customCurveField;
        CurveField customCurveField;
        CurveField previewCurveField;
        Toggle previewToggle;
        string easeFieldPath = null;

        bool showPreview = false;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            easeFieldPath = property.propertyPath;
            SerializedProperty easingShape = property.FindPropertyRelative(nameof(Easing.shape));
            SerializedProperty easingInOut = property.FindPropertyRelative(nameof(Easing.inOut));
            SerializedProperty easingCustomCurve = property.FindPropertyRelative(nameof(Easing.customCurve));

            root = new VisualElement();
            root.name = "EasingDrawer";
            var easeContainer = new VisualElement();
            easeContainer.name = "EaseHContainer";
            root.Add(easeContainer);

            shapeField = new PropertyField(easingShape, property.displayName);
            shapeField.name = "EaseShapeField";
            inOutField = new PropertyField(easingInOut, "");
            inOutField.name = "EaseInOutField";
            easeContainer.Add(shapeField);
            easeContainer.Add(inOutField);
            easeContainer.style.flexDirection = FlexDirection.Row;
            easeContainer.style.justifyContent = Justify.FlexStart;


            previewToggle = new Toggle();
            previewToggle.name = "EasePreviewToggle";
            previewToggle.tooltip = "preview";
            previewToggle.value = showPreview;
            previewToggle.viewDataKey = property.propertyPath + " ease preview toggle vdk";
            // todo toggle align right?
            // toggle.AddToClassList(Button.ussClassName);
            easeContainer.Add(previewToggle);
            previewToggle.RegisterValueChangedCallback(ce => {
                showPreview = ce.newValue;
                UpdateProp(property);
            });
            const int indentAmount = 10;

            // customCurveField = new PropertyField(easingCustomCurve, "Easing Curve");
            customCurveField = new CurveField("Easing Curve");
            customCurveField.name = "EasingCustomCurve";
            customCurveField.tooltip = "Curve evalutated from 0 - 1";
            customCurveField.AddToClassList(CurveField.alignedFieldUssClassName);
            customCurveField.style.paddingLeft = indentAmount;
            customCurveField.bindingPath = easingCustomCurve.propertyPath;
            customCurveField.style.display = DisplayStyle.None;
            root.Add(customCurveField);

            previewCurveField = new CurveField("Preview Curve");
            previewCurveField.name = "EasingPreviewCurve";
            previewCurveField.AddToClassList(CurveField.alignedFieldUssClassName);
            // todo make look disabled?
            // previewCurveField.AddToClassList(CurveField.disabledUssClassName);
            // previewCurveField.AddToClassList(PropertyField.disabledUssClassName);
            previewCurveField.style.paddingLeft = indentAmount;
            previewCurveField.style.display = DisplayStyle.None;
            root.Add(previewCurveField);

            // ? show additional settings for certain easing types

            // ? show preview of easing as a graph popup on hover...

            shapeField.RegisterValueChangeCallback(UpdateProp);
            inOutField.RegisterValueChangeCallback(UpdateProp);
            // previewCurveField.RegisterValueChangedCallback(c => UpdateProp(property));

            UpdateProp(property);
            return root;
        }

        private void UpdateProp(SerializedPropertyChangeEvent evt) {
            UpdateProp(evt.changedProperty.serializedObject.FindProperty(easeFieldPath));
        }

        private void UpdateProp(SerializedProperty property) {
            if (property == null) {
                Debug.LogError("Easing Drawer cant update property null");
                return;
            }
            SerializedProperty easeShapeProp = property.FindPropertyRelative(nameof(Easing.shape));
            SerializedProperty easingInOut = property.FindPropertyRelative(nameof(Easing.inOut));
            SerializedProperty easingCustomCurve = property.FindPropertyRelative(nameof(Easing.customCurve));

            if (easeShapeProp.propertyType != SerializedPropertyType.Enum) return;


            int propEnumValue = easeShapeProp.enumValueFlag;
            bool showCustomCurve = propEnumValue == (int)Easing.Shape.Custom;
            // if easing type is linear, hide inout field
            bool showInOut = propEnumValue != (int)Easing.Shape.Linear && !showCustomCurve;

            inOutField.SetDisplay(showInOut);

            if (showPreview && !showCustomCurve) {
                customCurveField.SetDisplay(false);
                previewCurveField.SetDisplay(true);
                previewToggle.SetDisplay(true);
                // if (customCurveField.IsBound()) customCurveField.Unbind();
                // customCurveField.
                // todo dont use a curve field, show a readonly preview somehow else
                // ? internal...
                // CurveEditorSettings
                // CurveEditorWindow
                // customCurveField.SetEnabled(false);
                Easing easing = new Easing((Easing.Shape)easeShapeProp.enumValueFlag, (Easing.InOut)easingInOut.enumValueFlag);
                previewCurveField.value = easing.ToAnimationCurve();
            } else {
                // if (!customCurveField.IsBound()) customCurveField.Bind();
                previewCurveField.SetDisplay(false);
                customCurveField.SetDisplay(showCustomCurve);
                previewToggle.SetDisplay(!showCustomCurve);
            }
            // Debug.Log($"scc:{showCustomCurve} ev{propEnumValue} {(int)Easing.Shape.Custom}");
        }
    }
#endif

    [System.Serializable]
    public struct Easing : IEquatable<Easing> {
        public Shape shape;// = EasingTypeShape.Linear;
        public InOut inOut;// = EasingTypeInOut.InOut;

        [Tooltip("Curve evalutated from 0 - 1")]
        public AnimationCurve customCurve;// = AnimationCurve.Linear;
        public float option;

        // ? show additional settings for certain easing types

        // public Easing(){}
        public Easing(Shape shape, InOut inOut = InOut.InOut) {
            this.shape = shape;
            this.inOut = inOut;
            this.customCurve = AnimationCurve.Linear(0, 0, 1, 1);
            this.option = 0;
        }

        public EasingType easingType => 
            shape == Shape.Linear ? EasingType.Linear :
            shape == Shape.Custom ? EasingType.Custom :
            (EasingType)(((int)shape) * 3 + (int)inOut);

        /// <summary>
        /// Ease a value between 0 and 1 by this easing type
        /// </summary>
        /// <param name="x"></param>
        /// <returns>eased value</returns>
        public float Ease(float x) {
            if (shape == Shape.Custom) {
                return customCurve.Evaluate(x);
            }
            return EaseByType(x, easingType);
        }
        public float EaseClamped(float x) {
            if (shape == Shape.Custom) {
                return Mathf.Clamp01(customCurve.Evaluate(x));
            }
            return Mathf.Clamp01(EaseByType(x, easingType));
        }
        /// <summary>Ease between start and end values, where x is between 0 and 1</summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="x"></param>
        /// <returns>eased value between start and end, unclamped</returns>
        public float Ease(float start, float end, float x) {
            if (shape == Shape.Custom) {
                return Mathf.LerpUnclamped(start, end, customCurve.Evaluate(x));
            }
            return Mathf.LerpUnclamped(start, end, EaseByType(x, easingType));
        }
        public float EaseInverseLerp(float start, float end, float value) {
            return EaseByTypeLerp(start, end, value, easingType);
        }

        bool UsesOptionData() {
            // todo
            return false;
        }


        public AnimationCurve ToAnimationCurve(float accuracy = 0.1f) {
            if (shape == Shape.Custom) {
                return customCurve;
            }
            if (shape == Shape.Linear) {
                return AnimationCurve.Linear(0, 0, 1, 1);
            }
            // if (shape == Shape.Sine && inOut== InOut.InOut) {
            //     return AnimationCurve.EaseInOut(0,0,1,1);
            // }
            // todo better for various shapes using tangents


            if (accuracy <= 0) {
                return AnimationCurve.Linear(0, 0, 1, 1);
            }
            int numKeyFrames = Mathf.CeilToInt(1 / accuracy);
            Keyframe[] keyframes = new Keyframe[numKeyFrames];
            for (int i = 0; i < numKeyFrames - 1; i++) {
                float time = i / (numKeyFrames - 1f);
                keyframes[i] = new Keyframe(time, Ease(time));
                keyframes[i].weightedMode = WeightedMode.None;
            }
            keyframes[numKeyFrames - 1] = new Keyframe(1f, 1f);
            // for (float i = 0; i < 1; i += accuracy) {
            // }
            AnimationCurve animCurve = new(keyframes);
            for (int i = 0; i < numKeyFrames; i++) {
                AnimationUtility.SetKeyBroken(animCurve, i, true);
                AnimationUtility.SetKeyLeftTangentMode(animCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(animCurve, i, AnimationUtility.TangentMode.Auto);
                // animCurve[i].weightedMode = WeightedMode.Both;
                // animCurve.SmoothTangents(i, 1f);
            }
            return animCurve;
        }

        public override string ToString() => $"Easing {easingType}";

        public override int GetHashCode() {
            if (this.shape == Shape.Custom) return HashCode.Combine(shape, customCurve.GetHashCode());
            if (this.shape == Shape.Linear) return HashCode.Combine(shape);
            if (this.UsesOptionData()) return HashCode.Combine(shape, inOut, option);
            return HashCode.Combine(shape, inOut);
            //? not equal?
            // return HashCode.Combine(shape, inOut, customCurve, option);
        }
        public override bool Equals(object obj) {
            if (obj is Easing other) return this.Equals(other);
            return false;
        }

        public bool Equals(Easing other) {
            if (this.shape != other.shape) return false;
            if (this.shape == Shape.Custom) return this.customCurve.Equals(other.customCurve);
            if (this.shape == Shape.Linear) return true;
            if (this.UsesOptionData()) return this.inOut == other.inOut && this.option == other.option;
            return this.inOut == other.inOut;
        }


        public static implicit operator EasingType(Easing easing) => easing.easingType;
        public static implicit operator Easing(EasingType easingType) =>
            easingType == EasingType.Linear ? new Easing(Shape.Linear) :
            easingType == EasingType.Custom ? new Easing(Shape.Custom) :
            new Easing(
                    (Shape)((int)easingType / 3),
                    (InOut)((int)easingType % 3));
        public static implicit operator Easing(AnimationCurve animationCurve) =>
            new Easing(Shape.Custom) { customCurve = animationCurve };

        public static bool operator ==(Easing a, Easing b) => a.Equals(b);
        public static bool operator !=(Easing a, Easing b) => !(a.Equals(b));


        public enum Shape {
            Custom = -1,
            Linear = 0,
            Sine,
            Cubic,
            Expo,
            Circ,
            Back,
            Elastic,
            Bounce,
        }
        public enum InOut {
            InOut,
            In,
            Out,
        }
        public enum EasingType {
            Linear = 0,
            Custom = 1,
            InOutSine = 3,
            InSine,
            OutSine,
            InOutCubic,
            InCubic,
            OutCubic,
            InOutExpo,
            InExpo,
            OutExpo,
            InOutCirc,
            InCirc,
            OutCirc,
            InOutBack,
            InBack,
            OutBack,
            InOutElastic,
            InElastic,
            OutElastic,
            InOutBounce,
            InBounce,
            OutBounce,
        }
        public static float EaseByTypeLerp(float start, float end, float value, EasingType easingType) {
            float x = Mathf.InverseLerp(start, end, value);
            return Mathf.LerpUnclamped(start, end, EaseByType(x, easingType));
        }
        // public static float EaseByType(float x, EasingTypeShape easingTypeShape, EasingTypeInOut easingTypeInOut = EasingTypeInOut.InOut) {
        //     if (easingTypeShape == EasingTypeShape.Linear) return x;
        //     return EaseByType(x, (EasingType)(((int)easingTypeShape) * 3 + (int)easingTypeInOut));
        // }
        public static float EaseByType(float x, EasingType easingType, float option = -1) {
            switch (easingType) {
                case EasingType.Linear:
                    return x;
                case EasingType.InOutSine:
                    return EaseInOutSine(x);
                case EasingType.InSine:
                    return EaseInSine(x);
                case EasingType.OutSine:
                    return EaseOutSine(x);
                case EasingType.InOutCubic:
                    return EaseInOutCubic(x);
                case EasingType.InCubic:
                    return EaseInCubic(x);
                case EasingType.OutCubic:
                    return EaseOutCubic(x);
                case EasingType.InOutCirc:
                    return EaseInOutCirc(x);
                case EasingType.InOutExpo:
                    return EaseInOutExpo(x);
                case EasingType.InExpo:
                    return EaseInExpo(x);
                case EasingType.OutExpo:
                    return EaseOutExpo(x);
                case EasingType.InCirc:
                    return EaseInCirc(x);
                case EasingType.OutCirc:
                    return EaseOutCirc(x);
                case EasingType.InOutBack:
                    return option >= 0 ? EaseInOutBack(x, option) : EaseInOutBounce(x);
                case EasingType.InBack:
                    return option >= 0 ? EaseInBack(x, option) : EaseInBack(x);
                case EasingType.OutBack:
                    return option >= 0 ? EaseOutBack(x, option) : EaseOutBack(x);
                case EasingType.InOutElastic:
                    return EaseInOutElastic(x);
                case EasingType.InElastic:
                    return EaseInElastic(x);
                case EasingType.OutElastic:
                    return EaseOutElastic(x);
                case EasingType.InOutBounce:
                    return EaseInOutBounce(x);
                case EasingType.InBounce:
                    return EaseInBounce(x);
                case EasingType.OutBounce:
                    return EaseOutBounce(x);
            }
            Debug.LogError($"Unhandled easing type {easingType}! {x}");
            return x;
        }

        // https://easings.net/

        /// <summary>
        /// Takes a value from 0-1 and eases it along an ease in out sin curve
        /// </summary>
        public static float EaseInOutSine(float x) {
            return -(math.cos(math.PI * x) - 1) / 2;
        }
        public static float EaseInSine(float x) {
            return 1 - math.cos((x * math.PI) / 2);
        }
        public static float EaseOutSine(float x) {
            return math.sin((x * math.PI) / 2);
        }
        public static float EaseInOutCubic(float x) {
            return x < 0.5f ? 4 * x * x * x : 1 - math.pow(-2 * x + 2, 3) / 2;
        }
        public static float EaseInCubic(float x) {
            return x * x * x;
        }
        public static float EaseOutCubic(float x) {
            return 1 - math.pow(1 - x, 3);
        }
        public static float EaseInOutExpo(float x) {
            return x == 0
              ? 0
              : x == 1
              ? 1
              : x < 0.5f ? math.pow(2, 20 * x - 10) / 2
              : (2 - math.pow(2, -20 * x + 10)) / 2;
        }
        public static float EaseInExpo(float x) {
            return x == 0 ? 0 : math.pow(2, 10 * x - 10);
        }
        public static float EaseOutExpo(float x) {
            return x == 1 ? 1 : 1 - math.pow(2, -10 * x);
        }
        public static float EaseInOutCirc(float x) {
            return x < 0.5f
              ? (1 - math.sqrt(1 - math.pow(2 * x, 2))) / 2
              : (math.sqrt(1 - math.pow(-2 * x + 2, 2)) + 1) / 2;
        }
        public static float EaseInCirc(float x) {
            return 1 - math.sqrt(1 - math.pow(x, 2));
        }
        public static float EaseOutCirc(float x) {
            return math.sqrt(1 - math.pow(x - 1, 2));
        }
        public static float EaseInOutBack(float x, float bounce = 0.1f) {
            float c1 = 1.70158f * bounce * 10f;
            float c2 = c1 * 1.525f;

            return x < 0.5f
              ? (math.pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
              : (math.pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }
        public static float EaseInBack(float x, float bounce = 0.1f) {
            float c1 = 1.70158f * bounce * 10f;
            float c3 = c1 + 1;

            return c3 * x * x * x - c1 * x * x;
        }
        /// <summary>ease overshoot target, default - 10%</summary>
        public static float EaseOutBack(float x, float bounce = 0.1f) {
            float c1 = 1.70158f * bounce * 10f;
            float c3 = c1 + 1;

            return 1 + c3 * math.pow(x - 1, 3) + c1 * math.pow(x - 1, 2);
        }
        public static float EaseInOutElastic(float x) {
            const float c5 = (2 * math.PI) / 4.5f;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : x < 0.5f
              ? -(math.pow(2, 20 * x - 10) * math.sin((20 * x - 11.125f) * c5)) / 2
              : (math.pow(2, -20 * x + 10) * math.sin((20 * x - 11.125f) * c5)) / 2 + 1;
        }
        public static float EaseInElastic(float x) {
            const float c4 = (2 * math.PI) / 3;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : -math.pow(2, 10 * x - 10) * math.sin((x * 10 - 10.75f) * c4);
        }
        public static float EaseOutElastic(float x) {
            const float c4 = (2 * math.PI) / 3;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : math.pow(2, -10 * x) * math.sin((x * 10 - 0.75f) * c4) + 1;
        }
        public static float EaseInOutBounce(float x) {
            return x < 0.5f
              ? (1 - EaseOutBounce(1 - 2 * x)) / 2
              : (1 + EaseOutBounce(2 * x - 1)) / 2;
        }
        public static float EaseInBounce(float x) {
            return 1 - EaseOutBounce(1 - x);
        }
        public static float EaseOutBounce(float x) {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (x < 1 / d1) {
                return n1 * x * x;
            } else if (x < 2 / d1) {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            } else if (x < 2.5f / d1) {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            } else {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }

        public static float EaseCustom(float x, AnimationCurve curve) {
            return curve.Evaluate(x);
        }
    }

}