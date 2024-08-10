#if UNITY_EDITOR && CVR_CCK_EXISTS
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NAK.LuaTools
{
    [CustomEditor(typeof(NAKLuaClientBehaviourWrapper))]
    public partial class NAKLuaWrapperEditor : Editor
    {
        #region Editor GUI

        private static bool _guiLuaConfigFoldout = true;
        private static bool _guiLuaScriptsFoldout = true;
        private static bool _guiLuaScriptsPreviewFoldout = true;
        private static bool _guiBoundEntriesFoldout = true;

        private static GUIStyle s_RightAlignedTextArea;

        #endregion Editor GUI
        
        #region Variables
        
        private NAKLuaClientBehaviourWrapper _luaWrapper;

        #endregion Variables

        #region Serialized Properties

        private SerializedProperty m_LocalOnly;
        //private SerializedProperty m_Obfuscate;
        
        private SerializedProperty m_BoundEntries;
        private SerializedProperty m_ScriptName;
        private SerializedProperty m_Scripts;
        
        #endregion Serialized Properties

        #region GUI Methods

        private void OnEnable()
        {
            if (target == null) return;
            _luaWrapper = (NAKLuaClientBehaviourWrapper)target;
            
            m_LocalOnly = serializedObject.FindProperty(nameof(NAKLuaClientBehaviourWrapper.localOnly));
            //m_Obfuscate = serializedObject.FindProperty(nameof(NAKLuaClientBehaviourWrapper.obfuscate));
            
            m_ScriptName = serializedObject.FindProperty(nameof(NAKLuaClientBehaviourWrapper.scriptName));
            m_Scripts = serializedObject.FindProperty(nameof(NAKLuaClientBehaviourWrapper.scriptEntries));
            
            m_BoundEntries = serializedObject.FindProperty(nameof(NAKLuaClientBehaviourWrapper.boundEntries));
            
            // listen for changes (only us)
            _luaWrapper.OnValidateMonoBehaviour = OnValidateMonoBehaviour;
            
            UpdateScriptPreview(); // initial update
        }

        private void OnDisable()
        {
            if (_luaWrapper == null) return; // fuck
            _luaWrapper.OnValidateMonoBehaviour = null;
        }

        public override void OnInspectorGUI()
        {
            if (_luaWrapper == null)
                return;
            
            s_RightAlignedTextArea ??= new GUIStyle(EditorStyles.textArea) { alignment = TextAnchor.UpperRight };
            
            serializedObject.UpdateIfRequiredOrScript();
            
            Draw_LuaConfig();
            Draw_LuaScripts();
            Draw_BoundEntries();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion GUI Methods

        #region Private Methods

        private void OnValidateMonoBehaviour()
        {
            if (!_guiLuaScriptsPreviewFoldout)
                return; // foldout is not open, no need to validate
            
            if (_luaWrapper.scriptEntries == null
                || _luaWrapper.boundEntries == null)
                return; // no script entries to validate

            bool hasChanged = false;
            foreach (NAKLuaClientBehaviourWrapper.ScriptEntry t in _luaWrapper.scriptEntries)
            {
                if (!t.IsValid || !t.IsDirty) continue;
                t.UpdateState();
                hasChanged = true;
            }

            if (!hasChanged
                && _luaWrapper.scriptEntries.Length > 0)
                return; 
            
            UpdateScriptPreview();
        }

        private void UpdateScriptPreview()
        {
            string scriptText = LuaScriptGenerator.GenerateScriptText(_luaWrapper.boundEntries, _luaWrapper.scriptEntries);
            bool isEmptyOrWhitespace = string.IsNullOrWhiteSpace(scriptText);
            int lineCount = isEmptyOrWhitespace ? 0 : CountLines(scriptText);
            if (_luaWrapper.CachedLineCount != lineCount)
            {
                _luaWrapper.CachedLineCount = lineCount;
                _luaWrapper.CachedLineNumberText = GetLineNumbers(lineCount);
            }
            _luaWrapper.OutputScriptText = isEmptyOrWhitespace ? string.Empty : scriptText;
        }
        
        private static int CountLines(string text) 
            => string.IsNullOrEmpty(text) ? 0 : 1 + text.Count(c => c == '\n');

        private static string GetLineNumbers(int lineCount)
            => string.Join("\n", Enumerable.Range(1, lineCount));

        #endregion Private Methods
    }
}
#endif