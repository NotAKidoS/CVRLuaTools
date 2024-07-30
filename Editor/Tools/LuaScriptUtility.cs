#if UNITY_EDITOR && CVR_CCK_EXISTS
using System.IO;
using UnityEditor;

namespace NAK.LuaTools
{
    public static class LuaScriptUtility
    {
        public static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName;
        }

        public static void EnsureDirectoryExists(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directoryPath)) return;
            if (directoryPath == null) return;
            Directory.CreateDirectory(directoryPath);
            AssetDatabase.Refresh();
        }
        
        public static string PreventEscape(string input)
        {
            return input.Replace("\\", @"\\").Replace("\"", "\\\"");
        }
    }
}
#endif