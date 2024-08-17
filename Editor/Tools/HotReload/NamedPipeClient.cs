#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NAK.LuaTools.HotReload
{
    public static class NamedPipeClient
    {
        private const string PipeName = "UnityPipe";
        
        public static void Send(ScriptInfo scriptInfo)
        {
            Debug.Log("Sending ScriptInfo... " + scriptInfo.ScriptName + " " + scriptInfo.LuaComponentId);
            Task.Run(() => SendInternal(scriptInfo));
        }

        private static async Task SendInternal(ScriptInfo script) 
        {
            try
            {
                await using NamedPipeClientStream stream = new(".", PipeName, PipeDirection.Out);
                await stream.ConnectAsync();
                stream.Write(BitConverter.GetBytes(script.LuaComponentId));
                await stream.WriteAsync(Encoding.UTF8.GetBytes(script.AssetId));
                await stream.WriteAsync(Encoding.UTF8.GetBytes(script.ScriptName));
                await stream.WriteAsync(Encoding.UTF8.GetBytes(script.ScriptPath));
                await stream.WriteAsync(Encoding.UTF8.GetBytes(script.ScriptText));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send script info: " + e.Message);
            }
        }
    }
}
#endif