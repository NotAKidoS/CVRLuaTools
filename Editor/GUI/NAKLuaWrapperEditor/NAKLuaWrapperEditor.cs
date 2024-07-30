#if UNITY_EDITOR && CVR_CCK_EXISTS
using UnityEditor;

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
                || _luaWrapper.scriptEntries.Length == 0)
                return; // no script entries to validate

            bool hasChanged = false;
            foreach (NAKLuaClientBehaviourWrapper.ScriptEntry t in _luaWrapper.scriptEntries)
            {
                if (!t.IsValid || !t.IsDirty) continue;
                t.UpdateState();
                hasChanged = true;
            }

            if (!hasChanged) 
                return; 
            
            UpdateScriptPreview();
        }

        private void UpdateScriptPreview()
        {
            _luaWrapper.OutputScriptText = LuaScriptGenerator.GenerateScriptText(_luaWrapper.boundEntries, _luaWrapper.scriptEntries);
        }

        #endregion Private Methods
    }
}
#endif