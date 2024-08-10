#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NAK.LuaTools.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

namespace NAK.LuaTools
{
    public static class ReorderableListCommands
    {
        private static readonly List<object> s_clipboard = new();

        /// <summary>
        /// Checks if the user has issued a copy, paste, or duplicate command on the provided ReorderableList.
        /// </summary>
        public static bool HandleCommands(ReorderableList list, IList rawList)
        {
            Event curEvent = Event.current;
            if (curEvent.type != EventType.ValidateCommand
                && curEvent.type != EventType.ExecuteCommand)
                return false;
            
            if (list.selectedIndices.Count == 0 
                || !list.HasKeyboardControl())
                return false;
            
            bool isExecute = curEvent.type == EventType.ExecuteCommand;
            switch (curEvent.commandName)
            {
                case "Copy":
                    if (isExecute) CopySelected(rawList, list.selectedIndices);
                    curEvent.Use();
                    return true;
                case "Paste":
                    if (isExecute) PasteSelected(list);
                    curEvent.Use();
                    return true;
                case "Duplicate":
                    if (isExecute) DuplicateSelected(list, rawList);
                    curEvent.Use();
                    return true;
                default:
                    return false;
            }
        }

        private static void CopySelected(IList list, ReadOnlyCollection<int> selectedIndices)
        {
            Assert.IsNotNull(list, "ReorderableList list is null. Are you assigning the .list property?");
            
            s_clipboard.Clear();
            foreach (var index in selectedIndices)
                if (index >= 0 && index < list.Count) s_clipboard.Add(list[index]);
        }

        private static void PasteSelected(ReorderableList list)
        {
            Assert.IsNotNull(list, "ReorderableList list is null. Are you assigning the .list property?");
            
            if (s_clipboard.Count == 0) 
                return;

            foreach (var item in s_clipboard)
            {
                if (item == null) continue; // we don't really "copy" to a clipboard, so if source is null, rip
                SerializedProperty newElement = list.serializedProperty.AddWithDefaults(item.GetType());
                newElement.CopyValuesFromInstance(item);
            }

            // adjust selection
            if (list.multiSelect) list.SelectRange(list.count - s_clipboard.Count, list.count - 1);
            else list.index = list.count - 1;
        }

        private static void DuplicateSelected(ReorderableList list, IList rawList)
        {
            Assert.IsNotNull(list, "ReorderableList list is null. Are you assigning the .list property?");
            
            var selectedItems = new List<object>();
            foreach (var index in list.selectedIndices)
                if (index >= 0 && index < rawList.Count) selectedItems.Add(rawList[index]);

            foreach (var item in selectedItems)
            {
                SerializedProperty newElement = list.serializedProperty.AddWithDefaults(item.GetType());
                newElement.CopyValuesFromInstance(item);
            }

            // adjust selection
            if (list.multiSelect) list.SelectRange(list.count - s_clipboard.Count, list.count - 1);
            else list.index = list.count - 1;
        }
    }
}
#endif