#if UNITY_EDITOR && CVR_CCK_EXISTS
using System.Collections.Generic;
using ABI.CCK.Scripts.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NAK.LuaTools
{
    [InitializeOnLoad]
    public class CCKBuildEventListener
    {
        static CCKBuildEventListener()
        {
            CCK_BuildUtility.PreAvatarBundleEvent.AddListener(OnPreBundleEvent);
            CCK_BuildUtility.PrePropBundleEvent.AddListener(OnPreBundleEvent);
            CCK_BuildUtility.PreWorldBundleEvent.AddListener(OnPreBundleWorldEvent); // TODO: how to handle properly
        }
        
        private static void OnPreBundleEvent(GameObject uploadedObject)
        {
            if (uploadedObject == null) return;
            var luaClientWrappers = uploadedObject.GetComponentsInChildren<NAKLuaClientBehaviourWrapper>(true);
            ProcessWrappers(luaClientWrappers);
        }
        
        private static void OnPreBundleWorldEvent(Scene scene)
        {
            var luaClientWrappers = new List<NAKLuaClientBehaviourWrapper>();
            var rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                var foundWrappers = rootObject.GetComponentsInChildren<NAKLuaClientBehaviourWrapper>(true);
                luaClientWrappers.AddRange(foundWrappers);
            }
            ProcessWrappers(luaClientWrappers, false);
        }
        
        private static void ProcessWrappers(IList<NAKLuaClientBehaviourWrapper> luaClientWrappers, bool safeToDestroy = true)
        {
            foreach (NAKLuaClientBehaviourWrapper wrapper in luaClientWrappers)
            {
                LuaScriptGenerator.CreateLuaClientBehaviourFromWrapper(wrapper);
                if (safeToDestroy) Object.DestroyImmediate(wrapper);
            }
        }
    }
}
#endif