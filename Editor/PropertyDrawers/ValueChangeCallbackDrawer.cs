using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {

    /// <summary>
    /// calls a value change callback
    /// </summary>
    [CustomPropertyDrawer(typeof(ValueChangeCallbackAttribute))]
    public class ValueChangeCallbackDrawer : ExtendedDecoratorDrawer {

        ValueChangeCallbackAttribute valueChange => (ValueChangeCallbackAttribute)attribute;

        protected override string decoratorName => "kutil-value-change-callback-decorator";

        protected override void Setup(ExtendedDecoratorData data) {
            // todo causes inf loop
            // propertyField.RegisterValueChangeCallback(CallCallback);
        }
        protected override void OnDetach(DetachFromPanelEvent detachFromPanelEvent, ExtendedDecoratorData data) {
            base.OnDetach(detachFromPanelEvent, data);
            // propertyField.unr(CallCallback);
        }

        void CallCallback(SerializedPropertyChangeEvent changeEvent, ExtendedDecoratorData data) {
            // use reflection to support arrays and nesting too
            var property = changeEvent.changedProperty;
            string path = property.propertyPath.Replace(property.name, valueChange.callbackMethodName);
            Object[] targetObjects = property.serializedObject.targetObjects;
            Debug.Log($"callback triggered on {property.propertyPath} targets:{targetObjects.ToStringFull(g => g.name, true)}");
            foreach (var targetObj in targetObjects) {
                System.Object target = targetObj;
                if (ReflectionHelper.TryGetMemberInfo(ref target, path, ReflectionHelper.defFlags, out var memberInfo)) {
                    if (memberInfo is System.Reflection.MethodInfo mi) {
                        System.Reflection.ParameterInfo[] parameterInfos = mi.GetParameters();
                        if (parameterInfos != null) {
                            if (parameterInfos.Length == 1) {
                                // System.Type parameterType = parameterInfos[0].ParameterType;
                                // object propValue = property.GetValue();
                                // if (parameterType.IsAssignableFrom)
                                Debug.Log($"calling method '{path}' with arg ");
                                // ReflectionHelper.TryCallMethod(target, mi, propValue.InNewArray());
                                continue;
                            } else if (parameterInfos.Length > 1) {
                                Debug.LogError($"ValueChangeCallback invalid for Method with paramters: {parameterInfos.ToStringFull()}. path:{path} on {targetObj?.name}.");
                                continue;
                            }
                        }
                        Debug.Log($"calling method '{path}'");
                        ReflectionHelper.TryCallMethod(target, mi);
                    } else if (memberInfo is System.Reflection.PropertyInfo pi) {
                        object propValue = property.GetValue();
                        Debug.Log($"calling prop '{path}' with {propValue}");
                        // ReflectionHelper.TrySetValue(propValue, target, pi);
                    } else {
                        Debug.LogError($"ValueChangeCallback invalid for Fields: {path} on {targetObj?.name}.");
                    }
                    // if (!valueChange.allowMultipleCalls) {
                    //     break;
                    // }
                } else {
                    Debug.LogError($"ValueChangeCallback failed to find {path} on {targetObj?.name}.");
                }
            }
        }

    }
}