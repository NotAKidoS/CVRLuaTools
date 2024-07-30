#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NAK.LuaTools.Extensions
{
    public static class SerializedPropertyExtensions
    {
        // Method to create a serialized property that uses the default values it should (recursive too)
        public static SerializedProperty AddWithDefaults<T>(this SerializedProperty property) where T : new()
        {
            property.arraySize++;
            SerializedProperty newArrayElement = property.GetArrayElementAtIndex(property.arraySize - 1);
            newArrayElement.SetDefaultValues(typeof(T));
            return newArrayElement;
        }

        private static void SetDefaultValues(this SerializedProperty property, Type type)
        {
            if (property.isArray)
            {
                Type elementType = type.GetElementType();
                if (elementType == null)
                {
                    Debug.LogWarning($"Unable to determine element type for array property {property.name}");
                    return;
                }

                const int DefaultArraySize = 0; // TODO: figure out how to determine this
                property.arraySize = DefaultArraySize;
                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty elementProperty = property.GetArrayElementAtIndex(i);
                    elementProperty.SetDefaultValues(elementType);
                }
                return;
            }
            
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogWarning($"Type {type.Name} does not have a default constructor");
                return;
            }
            
            object instance = Activator.CreateInstance(type);

            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                SerializedProperty fieldProperty = property.FindPropertyRelative(fieldInfo.Name);
                if (fieldProperty == null)
                {
                    //Debug.LogWarning($"Field {fieldInfo.Name} not found in serialized property");
                    continue;
                }
                
                SetFieldValue(fieldProperty, fieldInfo.FieldType, fieldInfo.GetValue(instance));
            }

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                SerializedProperty fieldProperty = property.FindPropertyRelative(propertyInfo.Name);
                if (fieldProperty == null)
                {
                    //Debug.LogWarning($"Property {propertyInfo.Name} not found in serialized property");
                    continue;
                }

                SetFieldValue(fieldProperty, propertyInfo.PropertyType, propertyInfo.GetValue(instance));
            }
        }
        
        private static void SetFieldValue(SerializedProperty fieldProperty, Type fieldType, object value)
        {
            // if (value == null)
            // {
            //     //Debug.LogWarning($"Value is null for field {fieldProperty.name}");
            //     if (fieldType.IsClass) fieldProperty.SetDefaultValues(fieldType);
            //     return;
            // }

            if (value == null) value = default;

            // TODO: double check this supports all types that SerializedProperty supports (gui-wise) by default
            
            if (fieldType == typeof(int))
            {
                fieldProperty.intValue = (int)value;
            }
            else if (fieldType == typeof(bool))
            {
                fieldProperty.boolValue = (bool)value;
            }
            else if (fieldType == typeof(float))
            {
                fieldProperty.floatValue = (float)value;
            }
            else if (fieldType == typeof(string))
            {
                fieldProperty.stringValue = (string)value;
            }
            else if (fieldType == typeof(Color))
            {
                fieldProperty.colorValue = (Color)value;
            }
            else if (fieldType == typeof(Object))
            {
                fieldProperty.objectReferenceValue = (Object)value;
            }
            else if (fieldType == typeof(LayerMask))
            {
                fieldProperty.intValue = (value is LayerMask mask ? mask : default).value;
            }
            else if (fieldType.IsEnum)
            {
                fieldProperty.enumValueIndex = (int)value; // todo: verify
                fieldProperty.intValue = (int)value;
            }
            else if (fieldType == typeof(Vector2))
            {
                fieldProperty.vector2Value = (Vector2)value;
            }
            else if (fieldType == typeof(Vector3))
            {
                fieldProperty.vector3Value = (Vector3)value;
            }
            else if (fieldType == typeof(Vector4))
            {
                fieldProperty.vector4Value = (Vector4)value;
            }
            else if (fieldType == typeof(Rect))
            {
                fieldProperty.rectValue = (Rect)value;
            }
            else if (fieldType == typeof(char))
            {
                fieldProperty.intValue = (int)value;
            }
            else if (fieldType == typeof(AnimationCurve))
            {
                fieldProperty.animationCurveValue = (AnimationCurve)value;
            }
            else if (fieldType == typeof(Bounds))
            {
                fieldProperty.boundsValue = (Bounds)value;
            }
            else if (fieldType == typeof(Gradient))
            {
                //fieldProperty.gradientValue = (Gradient)value; // is internal?
            }
            else if (fieldType.IsClass)
            {
                fieldProperty.SetDefaultValues(fieldType);
            }
            else if (fieldType == typeof(Vector2Int))
            {
                fieldProperty.vector2IntValue = (Vector2Int)value;
            }
            else if (fieldType == typeof(Vector3Int))
            {
                fieldProperty.vector3IntValue = (Vector3Int)value;
            }
            else if (fieldType == typeof(RectInt))
            {
                fieldProperty.rectIntValue = (RectInt)value;
            }
            else if (fieldType == typeof(BoundsInt))
            {
                fieldProperty.boundsIntValue = (BoundsInt)value;
            }
            else if (fieldType == typeof(Quaternion))
            {
                fieldProperty.quaternionValue = (Quaternion)value;
            }
            else
            {
                Debug.LogWarning($"Type {fieldType.Name} not supported for setting default values");
            }
        }
    }
}
#endif
