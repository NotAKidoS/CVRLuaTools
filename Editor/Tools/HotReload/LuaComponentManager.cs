#if UNITY_EDITOR && CVR_CCK_EXISTS
using System.Collections.Generic;
using System.Linq;
using ABI.CCK.Components;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using ABI.CCK.Components.ScriptableObjects;
using Debug = UnityEngine.Debug;

/*
 *  Monitors .lua file changes in the project directory and sends the updated script to the client.
 * 
 *  On .lua file update -> look through all CVRLuaClientBehaviours -> find the script -> send the updated script to the client
 *
 *  Scripts are identified by AssetId -> LuaComponentId
 *
 */

namespace NAK.LuaTools.HotReload
{
    [InitializeOnLoad]
    public static class LuaComponentManager 
    {
        #region Hot Reload Menu

        private const string kHotReloadMenuPath = "NotAKid/LuaTools/Hot Reload";
     
        private static bool s_EnableHotReload
        {
            get => EditorPrefs.GetBool("NAK.LuaTools.EnableHotReload", false);
            set
            {
                Debug.Log($"[LuaComponentManager] Hot Reload is {(value ? "enabled" : "disabled")}.");
                EditorPrefs.SetBool("NAK.LuaTools.EnableHotReload", value);
                
                if (value) FindAllLuaClientBehaviours();
                else ForgetAllLuaClientBehaviours();
            }
        }
        
        [MenuItem(kHotReloadMenuPath, false, 1)]
        private static void EnableHotReload()
        {
            s_EnableHotReload = !s_EnableHotReload;
        }
        
        [MenuItem(kHotReloadMenuPath, true, 1)]
        private static bool EnableHotReloadValidate()
        {
            Menu.SetChecked(kHotReloadMenuPath, s_EnableHotReload);
            return true;
        }
        
        #endregion Hot Reload Menu

        #region Constructor
        
        static LuaComponentManager()
        {
            s_ApplicationDataPath = Application.dataPath.Replace('\\', '/');
            
            FindAllLuaClientBehaviours();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update -= OnEditorApplicationUpdate;
            EditorApplication.update += OnEditorApplicationUpdate;

            InitializeLuaFileWatcher(); // Initialize the Lua file watcher
        }

        #endregion Constructor
        
        #region Lua File Watcher

        private static FileSystemWatcher _luaFileWatcher;

        private static void InitializeLuaFileWatcher()
        {
            if (!Directory.Exists(s_ApplicationDataPath))
            {
                Debug.LogWarning($"[LuaComponentManager] Directory does not exist: {s_ApplicationDataPath}");
                return;
            }

            _luaFileWatcher?.Dispose();
            _luaFileWatcher = new FileSystemWatcher
            {
                Path = s_ApplicationDataPath,
                Filter = "*.lua", // this won't catch CVRLuaScript assets
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                IncludeSubdirectories = true
            };

            _luaFileWatcher.Changed += OnLuaFileChanged; 
            //_luaFileWatcher.Created += OnLuaFileChanged;
            //_luaFileWatcher.Deleted += OnLuaFileChanged;
            //_luaFileWatcher.Renamed += OnLuaFileRenamed;
            _luaFileWatcher.EnableRaisingEvents = true;

            Debug.Log($"[LuaComponentManager] Watching for .lua file changes in {s_ApplicationDataPath}");
        }
        
        private static void OnLuaFileChanged(object source, FileSystemEventArgs e)
        {
            var relativePath = GetRelativePath(e.FullPath);
            if (relativePath == null) return;
            
            Debug.Log($"[LuaComponentManager] Lua File: {relativePath} {e.ChangeType}");
            
            s_ScriptChangeQueue.Enqueue(relativePath);
        }

        private static string GetRelativePath(string fullPath)
        {
            fullPath = fullPath.Replace('\\', '/');
            if (fullPath.StartsWith(s_ApplicationDataPath))
            {
                string relativePath = fullPath.Substring(s_ApplicationDataPath.Length - 6); // -6 to keep "Assets"
                return relativePath;
            }

            Debug.LogWarning($"[LuaComponentManager] Could not get relative path for {fullPath}");
            return null;
        }

        #endregion Lua File Watcher

        #region Callbacks

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            => FindAllLuaClientBehaviours();
        
        private static void OnHierarchyChanged()
            => FindAllLuaClientBehaviours();
        
        private static void FindAllLuaClientBehaviours()
        {
            if (!s_EnableHotReload)
                return;
            
            // start stopwatch
            //Stopwatch stopwatch = Stopwatch.StartNew();

            // find normal CVRLuaClientBehaviour components
            s_AssetIdToLuaClientBehaviours.Clear();
            var luaClientBehaviours = Object.FindObjectsOfType<CVRLuaClientBehaviour>(true);
            foreach (CVRLuaClientBehaviour behaviour in luaClientBehaviours)
            {
                if (!behaviour.gameObject.scene.IsValid())
                    continue;

                // TODO: change to get World GUID in scene
                var assetId = behaviour.GetComponentInParent<CVRAssetInfo>(true)?.objectId ?? "SYSTEM";
                if (!s_AssetIdToLuaClientBehaviours.ContainsKey(assetId))
                    s_AssetIdToLuaClientBehaviours[assetId] = new List<CVRLuaClientBehaviour>();
                
                s_AssetIdToLuaClientBehaviours[assetId].Add(behaviour);
            }
            
            // stop stopwatch
            //stopwatch.Stop();
            //Debug.Log($"[LuaComponentManager] FindAllLuaClientBehaviours took {stopwatch.ElapsedMilliseconds:f}ms");
        }

        private static void ForgetAllLuaClientBehaviours()
        {
            s_AssetIdToLuaClientBehaviours.Clear();
        }
        
        #endregion Callbacks
        
        private static readonly string s_ApplicationDataPath;
        private static readonly Dictionary<string, List<CVRLuaClientBehaviour>> s_AssetIdToLuaClientBehaviours = new();
        private static readonly Queue<string> s_ScriptChangeQueue = new(); // can't access Unity objects from FileSystemWatcher thread
        private static readonly Queue<ScriptInfo> s_OutboundScriptInfoQueue = new();
        
        // private static string SanitizePath(string path)
        //     => string.IsNullOrEmpty(path) ? string.Empty : path.Trim().Replace('\\', '/').ToLowerInvariant();

        private static void OnEditorApplicationUpdate()
        {
            if (s_OutboundScriptInfoQueue.Count != 0)
            {
                //Stopwatch stopwatch = Stopwatch.StartNew();

                while (s_OutboundScriptInfoQueue.Count > 0)
                {
                    ScriptInfo scriptInfo = s_OutboundScriptInfoQueue.Dequeue();
                    NamedPipeClient.Send(scriptInfo);
                }

                //stopwatch.Stop();
                //Debug.Log($"[LuaComponentManager] OnEditorApplicationUpdate took {stopwatch.ElapsedMilliseconds:f}ms");
            }
            
            if (s_ScriptChangeQueue.Count != 0)
            {
                //Stopwatch stopwatch = Stopwatch.StartNew();

                while (s_ScriptChangeQueue.Count > 0)
                {
                    var changedScriptPath = s_ScriptChangeQueue.Dequeue();

                    // look through all the CVRLuaClientBehaviours for the script
                    foreach (var (assetId, behaviours) in s_AssetIdToLuaClientBehaviours)
                    {
                        if (behaviours == null) continue;

                        // look through all the referenced CVRLuaScripts on the behaviour
                        foreach (CVRLuaClientBehaviour behaviour in behaviours)
                        {
                            if (behaviour == null) continue;
                            
                            if (behaviour is NAKLuaClientBehaviourWrapper wrapper)
                            {
                                var scripts = wrapper.scriptEntries.Where(script => script.script != null).ToArray();
                                foreach (NAKLuaClientBehaviourWrapper.ScriptEntry scriptEntry in scripts)
                                {
                                    if (scriptEntry.script == null) continue;
                                    if (scriptEntry.script.m_ScriptPath != changedScriptPath) 
                                        continue;
                                    
                                    Debug.Log($"[LuaComponentManager] Script within {assetId} on wrapper {behaviour.gameObject.name} changed!");
                                    
                                    var (scriptPath, scriptText) = LuaScriptGenerator.CreateScriptTextFromWrapper(wrapper); 
                                    
                                    s_OutboundScriptInfoQueue.Enqueue(new ScriptInfo
                                    {
                                        AssetId = assetId,
                                        LuaComponentId = GetGameObjectPathHashCode(behaviour.transform),
                                        ScriptName = wrapper.scriptName,
                                        ScriptPath = scriptPath,
                                        ScriptText = scriptText
                                    });
                                }
                                continue;
                            }
                            
                            CVRLuaScript script = behaviour.asset;
                            if (script == null) continue;
                            if (script.m_ScriptPath != changedScriptPath) 
                                continue;
                            
                            Debug.Log($"[LuaComponentManager] Script within {assetId} on wrapper {behaviour.gameObject.name} changed!");
                            s_OutboundScriptInfoQueue.Enqueue(new ScriptInfo
                            {
                                AssetId = assetId,
                                LuaComponentId = GetGameObjectPathHashCode(behaviour.transform),
                                ScriptName = script.name,
                                ScriptPath = script.m_ScriptPath,
                                ScriptText = File.ReadAllText(s_ApplicationDataPath.Substring(0, s_ApplicationDataPath.Length - 6) + script.m_ScriptPath)
                            });
                        }
                    }
                }

                //stopwatch.Stop();
                //Debug.Log($"[LuaComponentManager] OnEditorApplicationUpdate took {stopwatch.ElapsedMilliseconds:f}ms");
            }
        }

        #region Private Methods
        
        private static int GetGameObjectPathHashCode(Transform transform)
        {
            // Attempt to find the root component transform in one step

            Transform rootComponentTransform = null;
        
            // both CVRAvatar & CVRSpawnable *should* have an asset info component
            CVRAssetInfo rootComponent = transform.GetComponentInParent<CVRAssetInfo>(true);
            if (rootComponent != null && rootComponent.type != CVRAssetInfo.AssetType.World)
                rootComponentTransform = rootComponent.transform;
        
            // easy case, no need to crawl up the hierarchy
            if (rootComponentTransform == transform)
                return 581452743; // hash code for "[Root]"

            StringBuilder pathBuilder = new(transform.name);
            Transform parentTransform = transform.parent;
        
            while (parentTransform != null)
            {
                // reached root component
                // due to object loader renaming root, we can't rely on transform name, so we use "[Root]" instead
                if (parentTransform == rootComponentTransform)
                {
                    pathBuilder.Insert(0, "[Root]/");
                    break;
                }

                pathBuilder.Insert(0, parentTransform.name + "/");
                parentTransform = parentTransform.parent;
            }

            string path = pathBuilder.ToString();

            //Debug.Log($"[LuaComponentManager] Path: {path}");

            return path.GetHashCode();
        }
        
        #endregion Private Methods
    }
}
#endif