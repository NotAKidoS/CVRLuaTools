#if UNITY_EDITOR && CVR_CCK_EXISTS
using UnityEditor;
using UnityEngine;

namespace NAK.LuaTools
{
    [CustomPropertyDrawer(typeof(NAKLuaClientBehaviourWrapper.ScriptEntry))]
    public class ScriptEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            rect.height = EditorGUIUtility.singleLineHeight;
        
            SerializedProperty scriptProp = property.FindPropertyRelative(nameof(NAKLuaClientBehaviourWrapper.ScriptEntry.script));
            SerializedProperty isActiveProp = property.FindPropertyRelative(nameof(NAKLuaClientBehaviourWrapper.ScriptEntry.isActive));
            
            using (new EditorGUI.DisabledScope(!isActiveProp.boolValue))
            {
                Rect propertyRect = new(rect.x, rect.y, rect.width - 25, rect.height);
                
                if (DragAndDropHelper.CheckDragAndDrop(propertyRect))
                    DragAndDropHelper.ApplyDragAndDropToProperty(scriptProp);
                
                EditorGUI.PropertyField(propertyRect, scriptProp, GUIContent.none);
            }

            Rect toggleRect = new(rect.x + rect.width - 20, rect.y, 20, rect.height);
            isActiveProp.boolValue = EditorGUI.Toggle(toggleRect, isActiveProp.boolValue, EditorStyles.toggle);
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
    }
}
#endif