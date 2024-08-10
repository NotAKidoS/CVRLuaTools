#if UNITY_EDITOR && CVR_CCK_EXISTS
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using NAK.LuaTools.Extensions;
using static ABI.CCK.Scripts.Editor.SharedComponentGUI;

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

            int lineCount = _luaWrapper.CachedLineCount;
            if (lineCount == 0)
            {
                EditorGUILayout.HelpBox("No scripts attached or all are empty.", MessageType.Warning);
                return;
            }
             
            const float lineHeight = 24;
            float scrollViewHeight = Mathf.Min(lineCount * lineHeight, 200);

            int maxDigits = (int)Mathf.Floor(Mathf.Log10(lineCount) + 1);
            float lineNumberWidth = maxDigits * 8 + 10; // 8 pixels per digit + padding

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false,
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 41), GUILayout.Height(scrollViewHeight));

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    using (new SetIndentLevelScope(0))
                    {
                        EditorGUILayout.TextArea(_luaWrapper.CachedLineNumberText, s_RightAlignedTextArea, GUILayout.Width(lineNumberWidth), GUILayout.ExpandHeight(true));
                        EditorGUILayout.TextArea(_luaWrapper.OutputScriptText, GUILayout.ExpandHeight(true));
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Refresh"))
                UpdateScriptPreview();
        }
        
        #endregion Drawing Methods

        #region Reorderable List
        
        private void InitLuaScriptsList()
        {
            _luaScriptsList = new ReorderableList(serializedObject, m_Scripts, 
                true, true, true, true)
            {
                multiSelect = true,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.PropertyField(rect, m_ScriptName);
                    ReorderableListCommands.HandleCommands(_luaScriptsList, _luaWrapper.scriptEntries);
                },
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