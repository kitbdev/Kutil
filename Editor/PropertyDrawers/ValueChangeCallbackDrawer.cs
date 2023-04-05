using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {

    /// <summary>
    /// Adds a button above the field
    /// </summary>
    [CustomPropertyDrawer(typeof(ValueChangeCallbackAttribute))]
    public class ValueChangeCallbackDrawer : ExtendedDecoratorDrawer {

        ValueChangeCallbackAttribute valueChange => (ValueChangeCallbackAttribute)attribute;

        protected override string decoratorName => "kutil-value-change-callback-decorator";

        protected override void Setup() {
            // todo causes inf loop
            // propertyField.RegisterValueChangeCallback(CallCallback);
        }

        void CallCallback(SerializedPropertyChangeEvent changeEvent) {
            // use reflection to support arrays and nesting too
            var property = changeEvent.changedProperty;
            string path = property.propertyPath.Replace(property.name, valueChange.callbackMethodName);
            Object[] targetObjects = property.serializedObject.targetObjects;
            foreach (var targetObj in targetObjects) {
                System.Object target = targetObj;
                if (ReflectionHelper.TryGetMemberInfo(ref target, path, ReflectionHelper.defFlags, out var memberInfo)) {
                    if (memberInfo is System.Reflection.MethodInfo mi) {
                        System.Reflection.ParameterInfo[] parameterInfos = mi.GetParameters();
                        if (parameterInfos != null) {
                            if (parameterInfos.Length == 1) {
                                // Debug.Log("calling with param:"+property.GetValue());
                                // && parameterInfos[0].ParameterType
                                // ReflectionHelper.TryCallMethod(target, mi, property.GetValue().InNewArray());
                                continue;
                            } else if (parameterInfos.Length > 1) {
                                Debug.LogError($"ValueChangeCallback invalid for Method with paramters: {parameterInfos.ToStringFull()}. path:{path} on {targetObj?.name}.");
                                continue;
                            }
                        }
                        // ReflectionHelper.TryCallMethod(target, mi);
                    } else if (memberInfo is System.Reflection.PropertyInfo pi) {
                        // ReflectionHelper.TrySetValue(changeEvent.changedProperty.GetValue(), target, pi);
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