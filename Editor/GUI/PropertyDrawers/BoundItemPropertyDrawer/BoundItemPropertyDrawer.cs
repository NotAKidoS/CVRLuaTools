#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NAK.LuaTools
{
    [CustomPropertyDrawer(typeof(NAKLuaClientBehaviourWrapper.BoundItem))]
    public class BoundItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            rect.height = EditorGUIUtility.singleLineHeight;
            
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty typeProp = property.FindPropertyRelative("type");

            var halfWidth = rect.width / 2;
            var nameWidth = halfWidth * 0.65f;
            var typeWidth = halfWidth * 0.35f;

            Rect nameRect = new(rect.x, rect.y, nameWidth, rect.height);
            Rect typeRect = new(rect.x + nameWidth + 2, rect.y, typeWidth - 2, rect.height);
            Rect valueRect = new(rect.x + halfWidth + 2, rect.y, halfWidth - 2, rect.height);
            
            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
            
            var itemType = (NAKLuaClientBehaviourWrapper.BoundItemType)typeProp.enumValueIndex;
            switch (itemType)
            {
                case NAKLuaClientBehaviourWrapper.BoundItemType.Object:
                    SerializedProperty objectReference = property.FindPropertyRelative("objectReference");

                    if (DragAndDropHelper.CheckDragAndDrop(nameRect))
                        DragAndDropHelper.ApplyDragAndDropToProperty(objectReference, nameProp);
                    
                    EditorGUI.PropertyField(valueRect, objectReference, GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Integer:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Boolean:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boolValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Float:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.String:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Color:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("colorValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.LayerMask:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("layerMaskValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector2Value"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector3Value"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Rect:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("rectValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Bounds:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boundsValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Quaternion:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("quaternionValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2Int:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector2IntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3Int:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector3IntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.RectInt:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("rectIntValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.BoundsInt:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boundsIntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Table:
                    EditorGUI.LabelField(valueRect, "Table has entries");
                    rect.y += EditorGUIUtility.singleLineHeight + 2f;
                    SerializedProperty boundEntriesProp = property.FindPropertyRelative("boundEntries");
                    EditorGUI.PropertyField(rect, boundEntriesProp, GUIContent.none);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            switch ((NAKLuaClientBehaviourWrapper.BoundItemType)typeProp.enumValueIndex)
            {
                case NAKLuaClientBehaviourWrapper.BoundItemType.Rect:
                case NAKLuaClientBehaviourWrapper.BoundItemType.Bounds:
                case NAKLuaClientBehaviourWrapper.BoundItemType.RectInt:
                case NAKLuaClientBehaviourWrapper.BoundItemType.BoundsInt:
                    return (height * 2) + 4;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Table:
                    return height + 4 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("boundEntries"));
                default:
                    return height + 4;
            }
        }
    }
}
#endif