#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using NAK.LuaTools.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static ABI.CCK.Scripts.Editor.SharedComponentGUI;
using Object = UnityEngine.Object;

namespace NAK.LuaTools
{
    public partial class NAKLuaWrapperEditor
    {
        private ReorderableList _boundEntriesList;
        
        private void Draw_BoundEntries()
        {
            using (new FoldoutScope(ref _guiBoundEntriesFoldout, "Bound Entries", s_BoldFoldoutStyle))
            {
                if (!_guiBoundEntriesFoldout) return;
                DrawBoundEntries();
            }
        }

        #region Drawing Methods
        
        private void DrawBoundEntries()
        {
            if (_boundEntriesList == null) InitBoundEntriesList();
            
            _boundEntriesList!.DoLayoutList();
        }
        
        #endregion Drawing Methods
        
        #region Bound Entries List

        private void InitBoundEntriesList()
        {
            _boundEntriesList = new ReorderableList(serializedObject, m_BoundEntries, 
                true, true, true, true)
            {
                multiSelect = true,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Bound Entries ({m_BoundEntries.arraySize})"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    SerializedProperty element = m_BoundEntries.GetArrayElementAtIndex(index);
                    rect.y += 3;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(m_BoundEntries.GetArrayElementAtIndex(index)),
                onAddCallback = list =>
                {
                    list.serializedProperty.AddWithDefaults<NAKLuaClientBehaviourWrapper.BoundItem>();
                    list.index = list.count - 1;
                },
            };
        }

        #endregion Bound Entries List
    }
}
#endif