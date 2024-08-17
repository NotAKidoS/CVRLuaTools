#if UNITY_EDITOR && CVR_CCK_EXISTS
namespace NAK.LuaTools.HotReload
{
    public class ScriptInfo
    {
        public int LuaComponentId;
        public string AssetId;
        public string ScriptName;
        public string ScriptPath;
        public string ScriptText;
    }
}
#endif