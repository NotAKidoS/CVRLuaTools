#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using System.Reflection;
using System.Linq;
using ABI.CCK.Components.ScriptableObjects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static ABI.CCK.Scripts.Editor.SharedComponentGUI;
using Object = UnityEngine.Object;
using NAK.LuaTools.Extensions;

namespace NAK.LuaTools
{
    public partial class NAKLuaWrapperEditor
    {
        private ReorderableList _luaScriptsList;
        private Vector2 _scrollPosition;
        
        private void Draw_LuaScripts()
        {
            using (new FoldoutScope(ref _guiLuaScriptsFoldout, "Script", s_BoldFoldoutStyle))
            {
                if (!_guiLuaScriptsFoldout) return;
                DrawLuaScripts();
            }
        }
        
        private void DrawLuaScripts()
        {
            if (_luaScriptsList == null) InitLuaScriptsList();
            
            DrawLuaScriptPreview();
            
            Separator();
            
            EditorGUI.HelpBox(EditorGUILayout.GetControlRect(),
                "All scripts attached will be merged into a single script prior to upload.",
                MessageType.Info);

            _luaScriptsList!.DoLayoutList();
        }

        #region Drawing Methods
        
        private void DrawLuaScriptPreview()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                if (!InnerFoldout(ref _guiLuaScriptsPreviewFoldout, "Preview", s_BoldFoldoutStyle)) 
                    return;
            }
            
            const float lineHeight = 24;
            int lineCount = CountLines(_luaWrapper.OutputScriptText);
            if (lineCount == 0)
            {
                EditorGUILayout.HelpBox("No scripts attached or all are empty.", MessageType.Warning);
                return;
            }
            
            float scrollViewHeight = Mathf.Min(lineCount * lineHeight, 200);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false,
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 30), GUILayout.Height(scrollViewHeight));

            using (new EditorGUI.DisabledScope(true))
            {
                using (new SetIndentLevelScope(0))
                    EditorGUILayout.TextArea(_luaWrapper.OutputScriptText, GUILayout.ExpandHeight(true));
            }

            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Refresh"))
                UpdateScriptPreview();
            
            return;
            static int CountLines(string text) => string.IsNullOrEmpty(text) ? 0 : 1 + text.Count(c => c == '\n');
        }
        
        #endregion Drawing Methods

        #region Reorderable List
        
        private void InitLuaScriptsList()
        {
            _luaScriptsList = new ReorderableList(serializedObject, m_Scripts, 
                true, true, true, true)
            {
                multiSelect = true,
                drawHeaderCallback = rect => EditorGUI.PropertyField(rect, m_ScriptName),
                drawElementCallback = (rect, index, _, _) =>
                {
                    SerializedProperty element = m_Scripts.GetArrayElementAtIndex(index);
                    rect.y += 3;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(m_Scripts.GetArrayElementAtIndex(index)),
                onAddCallback = list =>
                {
                    list.serializedProperty.AddWithDefaults<NAKLuaClientBehaviourWrapper.ScriptEntry>();
                    list.index = list.count - 1;
                },
            };
        }
        
        #endregion Reorderable List
    }
}
#endif