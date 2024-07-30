#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace NAK.LuaTools
{
    public static class DragAndDropHelper
    {
        // TODO: prevent dropping invalid types onto a field, it creates empty entries when resizing array
        // https://discussions.unity.com/t/a-smarter-way-to-get-the-type-of-serializedproperty/186674/2

        #region Public Methods
        
        public static bool CheckDragAndDrop(Rect rect)
        {
            Event curEvent = Event.current;
            if (curEvent.type is not (EventType.DragUpdated or EventType.DragPerform)) 
                return false;
            
            if (!rect.Contains(curEvent.mousePosition))
                return false;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (curEvent.type != EventType.DragPerform) 
                return false;
                
            DragAndDrop.AcceptDrag(); // Need to call for changes to SerializedProperty to take effect :3
            return true;
        }
        
        public static void ApplyDragAndDropToProperty(SerializedProperty objectReferenceProperty, SerializedProperty objectNameProperty = null)
        {
            Assert.IsTrue(objectReferenceProperty.propertyType == SerializedPropertyType.ObjectReference, "Provided objectReferenceProperty is not an ObjectReference.");
            if (objectNameProperty != null) Assert.IsTrue(objectNameProperty.propertyType == SerializedPropertyType.String, "Provided objectNameProperty is not a String.");
            
            int draggedObjectsCount = DragAndDrop.objectReferences.Length;
            
            if (draggedObjectsCount == 1) ApplySingleDragAndDrop(objectReferenceProperty, objectNameProperty);
            else if (draggedObjectsCount > 1) ApplyMultipleDragAndDrop(objectReferenceProperty, objectNameProperty);
            
            return;
            
            #region DragAndDrop Logic
            
            void ApplySingleDragAndDrop(SerializedProperty referenceProperty, SerializedProperty nameProperty = null)
            {
                if (referenceProperty.isArray)
                {
                    referenceProperty.arraySize++;
                    SerializedProperty element = referenceProperty.GetArrayElementAtIndex(referenceProperty.arraySize - 1);
                    element.objectReferenceValue = DragAndDrop.objectReferences[0];
                    if (nameProperty != null)
                    {
                        nameProperty.arraySize++;
                        SerializedProperty nameElement = nameProperty.GetArrayElementAtIndex(nameProperty.arraySize - 1);
                        nameElement.stringValue = DragAndDrop.objectReferences[0].name;
                    }
                }
                else
                {
                    referenceProperty.objectReferenceValue = DragAndDrop.objectReferences[0];
                    if (nameProperty != null) nameProperty.stringValue = DragAndDrop.objectReferences[0].name;
                }
            }

            void ApplyMultipleDragAndDrop(SerializedProperty referenceProperty, SerializedProperty nameProperty)
            {
                if (!referenceProperty.isArray)
                {
                    AdjustArraySizeAndSetValues(referenceProperty, nameProperty);
                }
                else
                {
                    referenceProperty.ClearArray();
                    nameProperty?.ClearArray();

                    for (int i = 0; i < draggedObjectsCount; i++)
                    {
                        referenceProperty.arraySize++;
                        SerializedProperty element = referenceProperty.GetArrayElementAtIndex(i);
                        element.objectReferenceValue = DragAndDrop.objectReferences[i];
                        if (nameProperty != null)
                        {
                            nameProperty.arraySize++;
                            SerializedProperty nameElement = nameProperty.GetArrayElementAtIndex(i);
                            nameElement.stringValue = DragAndDrop.objectReferences[i].name;
                        }
                    }
                }
            }

            void AdjustArraySizeAndSetValues(SerializedProperty referenceProperty, SerializedProperty nameProperty)
            {
                SerializedObject serializedObject = referenceProperty.serializedObject;
                var propertyPath = referenceProperty.propertyPath;

                // Extract the index of the objectReference property path
                int startIndex = propertyPath.LastIndexOf('[') + 1;
                int endIndex = propertyPath.LastIndexOf(']');
                int index = int.Parse(propertyPath.Substring(startIndex, endIndex - startIndex));
                
                // Remove until ".Array", keeping the part before ".data"
                string arrayPath = propertyPath.Substring(0, propertyPath.LastIndexOf(".data", StringComparison.Ordinal));
                
                SerializedProperty arrayProperty = serializedObject.FindProperty(arrayPath);
                if (arrayProperty == null)
                {
                    Debug.LogError("Array property not found: " + arrayPath);
                    return;
                }

                // Adjust array size to accommodate new object references
                int originalSize = arrayProperty.arraySize;
                arrayProperty.arraySize += draggedObjectsCount - 1;

                // Shift existing elements if needed
                for (int i = originalSize - 1; i >= index; i--)
                {
                    string oldPath = $"{arrayPath}.data[{i}].{referenceProperty.name}";
                    string newPath = $"{arrayPath}.data[{i + draggedObjectsCount}].{referenceProperty.name}";
                    SerializedProperty oldElement = serializedObject.FindProperty(oldPath);
                    SerializedProperty newElement = serializedObject.FindProperty(newPath);
                    if (newElement != null && oldElement != null)
                        newElement.objectReferenceValue = oldElement.objectReferenceValue;

                    if (nameProperty != null)
                    {
                        string oldNamePath = $"{arrayPath}.data[{i}].{nameProperty.name}";
                        string newNamePath = $"{arrayPath}.data[{i + draggedObjectsCount}].{nameProperty.name}";
                        SerializedProperty oldNameElement = serializedObject.FindProperty(oldNamePath);
                        SerializedProperty newNameElement = serializedObject.FindProperty(newNamePath);
                        if (newNameElement != null && oldNameElement != null)
                            newNameElement.stringValue = oldNameElement.stringValue;
                    }
                }

                // Insert new elements
                for (int i = 0; i < draggedObjectsCount; i++)
                {
                    string newPath = $"{arrayPath}.data[{index + i}].{referenceProperty.name}";
                    SerializedProperty element = serializedObject.FindProperty(newPath);

                    if (element is { propertyType: SerializedPropertyType.ObjectReference })
                        element.objectReferenceValue = DragAndDrop.objectReferences[i];
                    else
                        Debug.LogError("Element is not an ObjectReference or not found: " + newPath);
                    
                    if (nameProperty != null)
                    {
                        string namePath = $"{arrayPath}.data[{index + i}].{nameProperty.name}";
                        SerializedProperty nameElement = serializedObject.FindProperty(namePath);

                        if (nameElement != null && nameElement.propertyType == SerializedPropertyType.String)
                            nameElement.stringValue = DragAndDrop.objectReferences[i].name;
                        else
                            Debug.LogError("Name element is not a String or not found: " + namePath);
                    }
                }
            }
            
            #endregion DragAndDrop Logic
        }
        
        #endregion Public Methods
    }
}
#endif