#if UNITY_EDITOR && CVR_CCK_EXISTS
using UnityEditor;
using static ABI.CCK.Scripts.Editor.SharedComponentGUI;

namespace NAK.LuaTools
{
    public partial class NAKLuaWrapperEditor
    {
        private void Draw_LuaConfig()
        {
            using (new FoldoutScope(ref _guiLuaConfigFoldout, "Configuration", s_BoldFoldoutStyle))
            {
                if (!_guiLuaConfigFoldout) return;
                using (new EditorGUI.IndentLevelScope()) DrawLuaConfig();
            }
        }

        #region Drawing Methods
        
        private void DrawLuaConfig()
        {
            EditorGUILayout.PropertyField(m_LocalOnly);
            //EditorGUILayout.PropertyField(m_Obfuscate);
        }
        
        #endregion Drawing Methods
    }
}
#endif