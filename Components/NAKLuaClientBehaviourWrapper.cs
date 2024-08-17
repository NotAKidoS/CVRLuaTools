#if CVR_CCK_EXISTS
using System;
using ABI.CCK.Components;
using ABI.CCK.Components.ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations;

namespace NAK.LuaTools
{
    public class NAKLuaClientBehaviourWrapper : CVRLuaClientBehaviour // inheriting for faster lookup
    {
        #region Bound Definitions

        public enum BoundItemType
        {
            Object,
            Integer,
            Boolean,
            Float,
            String,
            Color,
            LayerMask,
            Vector2,
            Vector3,
            Rect,
            Bounds,
            Quaternion,
            Vector2Int,
            Vector3Int,
            RectInt,
            BoundsInt,
            Table
        }

        [Serializable]
        public class BoundItem
        {
            public bool IsValid => !string.IsNullOrWhiteSpace(name);
            
            public string name;
            public BoundItemType type;
            
            // Object
            public UnityEngine.Object objectReference;
            
            // Variable
            public int intValue;
            public bool boolValue;
            public float floatValue;
            public string stringValue;
            public Color colorValue;
            public LayerMask layerMaskValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Rect rectValue;
            public Bounds boundsValue;
            public Quaternion quaternionValue;
            public Vector2Int vector2IntValue;
            public Vector3Int vector3IntValue;
            public RectInt rectIntValue;
            public BoundsInt boundsIntValue;
            
            // Table
            public BoundItem[] boundEntries;
        }
        
        #endregion Bound Definitions
        
        #region Script Definitions
        
        [Serializable]
        public class ScriptEntry
        {
            public bool IsValid => script != null;
            public bool IsValidAndActive => IsValid && isActive;
            public bool IsDirty => _wasActive != isActive || _fileHash != script.GetHashCode();
            public void UpdateState()
            {
                _wasActive = isActive;
                _fileHash = script.GetHashCode();
            }

            public CVRLuaScript script;
            public bool isActive = true;
            
            private bool _wasActive;
            private int _fileHash;
        }
        
        #endregion Script Definitions
        
        #region Serialized Fields

        // [SerializeField] [NotKeyable]
        // public bool localOnly = true;
        
        //[SerializeField] [NotKeyable] // TODO: uh
        //public bool obfuscate;
        
        [SerializeField] [NotKeyable]
        public BoundItem[] boundEntries;
        [SerializeField] [NotKeyable]
        public ScriptEntry[] scriptEntries;

        [SerializeField] [NotKeyable]
        public string scriptName = "New Lua Script";
        // [SerializeField] [NotKeyable] // TODO: determine if needed
        // public string scriptPath;

        #endregion Serialized Fields

        #region Editor Fields & Methods
        
        // preview script text
        public int CachedLineCount { get; set; }
        public string CachedLineNumberText { get; set; }
        public string OutputScriptText { get; set; }

        // forwarding onvalidate to the editor
        public Action OnValidateMonoBehaviour { get; set; }
        private void OnValidate() => OnValidateMonoBehaviour?.Invoke();
        
        #endregion Editor Fields & Methods
    }
}
#endif