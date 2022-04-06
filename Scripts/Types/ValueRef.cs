using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Kutil {
    [System.Serializable]
    public class ValueRef {
        public class AnyType { }// todo store Object value?
        public static BindingFlags flags = BindingFlags.Public
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Instance
                                           | BindingFlags.Static;
    }
    /// <summary>
    /// hold a reference to a value in another script. 
    /// Uses Reflection so might not be the best for performance.
    /// Use ValueRef.AnyType to support any type
    /// </summary>
    /// <typeparam name="T">the type of value</typeparam>
    [System.Serializable]
    public class ValueRef<T> {

        // for inspector to get type
        [SerializeField, HideInInspector] T fieldType;

        [SerializeField] protected UnityEngine.Object target;
        [SerializeField] protected string fieldName;
        [SerializeField] protected T lastVal = default;
        // todo instead of anytype, use System.Object?
        public bool IsAnyType => typeof(T) == typeof(ValueRef.AnyType);

        /// <summary>
        /// is the reference valid
        /// the target and fieldname cannot be null
        /// does not check if field exists
        /// </summary>
        /// <returns></returns>
        public bool IsValid() {
            return target != null && !string.IsNullOrEmpty(fieldName);
        }

        /// <summary>
        /// Tries to set the value of the reference.
        /// </summary>
        /// <param name="newValue">the new value</param>
        /// <returns>value was set successfully</returns>
        public bool SetValue(T newValue) {
            if (!IsValid()) {
                return false;
            }
            MemberInfo memberInfo = GetMemberInfo(target.GetType(), fieldName);
            if (memberInfo is FieldInfo) {
                FieldInfo fieldInfo = memberInfo as FieldInfo;
                var targetObj = fieldInfo.IsStatic ? null : target;
                fieldInfo.SetValue(targetObj, newValue);
                return true;
            } else if (memberInfo is PropertyInfo) {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo.CanWrite && propertyInfo.SetMethod != null) {
                    propertyInfo.SetValue(target, newValue);
                    return true;
                } else {
                    Debug.LogWarning("Cannot set value to a property without a setter " + target.name + "." + fieldName);
                    return false;
                }
            } else if (memberInfo is MethodInfo) {
                return false;
            } else {
                Debug.LogWarning("Failed to find valid member info on " + target.name + "." + fieldName);
                return false;
            }
        }
        /// <summary>
        /// Get value of reference
        /// Throws exception if fails
        /// </summary>
        /// <returns>value</returns>
        public T GetValue() {
            if (TryGetValue(out var result)) {
                return result;
            }
            // Debug.Log("Failed to get value");
            throw new Exception("Failed to get ValueRef value. Make sure it is valid.");
            // return default;
        }


        /// <summary>
        /// Get the value if it has changed since last time this method was called
        /// not garunteed to work the first time
        /// Try to get the value of the reference. Must be valid.
        /// </summary>
        /// <param name="newValue">out value</param>
        /// <returns>true if value gotten successfully and value has changed</returns>
        public bool TryGetValueChanged(out T newValue) {
            if (TryGetValue(out newValue)) {
                if (!lastVal.Equals(newValue)) {
                    lastVal = newValue;
                    return true;
                }
                lastVal = newValue;
            }
            return false;
        }

        public bool TryGetAnyValue(out object newValue) {
            newValue = default;
            if (!IsValid()) {
                return false;
            }
            MemberInfo memberInfo = GetMemberInfo(target.GetType(), fieldName);
            if (memberInfo is FieldInfo) {
                FieldInfo fieldInfo = memberInfo as FieldInfo;
                var targetObj = fieldInfo.IsStatic ? null : target;
                newValue = (object)fieldInfo.GetValue(targetObj);
                return true;
            } else if (memberInfo is PropertyInfo) {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                newValue = (object)propertyInfo.GetValue(target);
                return true;
            } else if (memberInfo is MethodInfo) {
                MethodInfo methodInfo = memberInfo as MethodInfo;
                var targetObj = methodInfo.IsStatic ? null : target;
                newValue = (object)methodInfo.Invoke(targetObj, new object[0]);
                return true;
            } else {
                Debug.LogWarning("Failed to find valid member info on " + target + " " + fieldName);
                return false;
            }
        }
        // todo use new reflection helper
        /// <summary>
        /// Try to get the value of the reference. Must be valid
        /// </summary>
        /// <param name="newValue">out value</param>
        /// <returns>true if value gotten successfully</returns>
        public bool TryGetValue(out T newValue) {
            newValue = default;
            if (!IsValid()) {
                return false;
            }
            MemberInfo memberInfo = GetMemberInfo(target.GetType(), fieldName);
            if (memberInfo is FieldInfo) {
                FieldInfo fieldInfo = memberInfo as FieldInfo;
                var targetObj = fieldInfo.IsStatic ? null : target;
                newValue = (T)fieldInfo.GetValue(targetObj);
                return true;
            } else if (memberInfo is PropertyInfo) {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                newValue = (T)propertyInfo.GetValue(target);
                return true;
            } else if (memberInfo is MethodInfo) {
                MethodInfo methodInfo = memberInfo as MethodInfo;
                var targetObj = methodInfo.IsStatic ? null : target;
                newValue = (T)methodInfo.Invoke(targetObj, new object[0]);
                return true;
            } else {
                Debug.LogWarning("Failed to find valid member info on " + target + " " + fieldName);
                return false;
            }
        }

        static MemberInfo GetMemberInfo(Type objectType, string fname) {
            bool isAnyType = typeof(T) == typeof(ValueRef.AnyType);
            while (objectType != null && objectType != typeof(object)) {
                MemberInfo[] memberInfos = objectType.GetMember(fname, ValueRef.flags);
                if (memberInfos.Length > 0) {
                    MemberInfo memberInfo = memberInfos[0];
                    // Debug.Log("type is " + memberInfo.MemberType.ToString());

                    switch (memberInfo.MemberType) {
                        case MemberTypes.Field:
                            FieldInfo fieldInfo = objectType.GetField(fname, ValueRef.flags);
                            if (fieldInfo.FieldType != typeof(T) && !isAnyType) {
                                Debug.LogWarning($"ValueRef GetValidFieldInfo Type Mismatch: expected:{typeof(T)} wanted:{fieldInfo.FieldType}");
                            } else {
                                return fieldInfo;
                            }
                            break;
                        case MemberTypes.Property:
                            PropertyInfo propertyInfo = objectType.GetProperty(fname, ValueRef.flags);
                            if (propertyInfo.PropertyType != typeof(T) && !isAnyType) {
                                Debug.LogWarning($"ValueRef GetMemberInfo Type Mismatch: expected:{typeof(T)} wanted:{propertyInfo.PropertyType}");
                            } else {
                                return propertyInfo;
                            }

                            break;
                        case MemberTypes.Method:
                            MethodInfo methodInfo = objectType.GetMethod(fname, ValueRef.flags);
                            if (methodInfo.GetParameters().Length > 0) {
                                continue;
                            }
                            if (methodInfo.ReturnType != typeof(T) && !isAnyType) {
                                Debug.LogWarning($"ValueRef GetMemberInfo Type Mismatch: expected:{typeof(T)} wanted:{methodInfo.ReturnType}");
                            } else {
                                return methodInfo;
                            }
                            break;
                        default:
                            break;
                    }
                }
                objectType = objectType.BaseType;
            }
            return null;
        }
    }
}