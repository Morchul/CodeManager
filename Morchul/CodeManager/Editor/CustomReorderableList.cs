#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// A custom version of ReorderableList specific for Code Manager
    /// </summary>
    public class CustomReorderableList : ReorderableList
    {

        public bool Expanded { get; set; }
        public List<bool> ElementExpanded { get; private set; }
        public List<float> ElementHeights { get; private set; }

        public float LIST_ELEMENT_HEIGHT { get; private set; }

        public CustomReorderableList(SerializedObject serializedObject, SerializedProperty serializedProperty, int initialLength, bool draggable = true, bool displayHeader = false, bool displayAddRemoveButtons = true) : base(serializedObject, serializedProperty, draggable, displayHeader, displayAddRemoveButtons, displayAddRemoveButtons)
        {
            LIST_ELEMENT_HEIGHT = EditorGUIUtility.singleLineHeight + 5f;
            Expanded = false;
            ElementExpanded = new List<bool>();
            ElementHeights = new List<float>();

            for (int i = 0; i < initialLength; ++i)
                UpdateReorderableStatusLists_Add();

            headerHeight = 0;

            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => onElementDrawCallback(rect, index, isActive, isFocused, this);
            elementHeightCallback = (index) => onElementHeightCallback(index, this);
            onAddCallback = AddItem;
            onRemoveCallback = RemoveItem;
            onReorderCallbackWithDetails = ReorderItem;
        }

        private void UpdateReorderableStatusLists_Update(int oldIndex, int newIndex)
        {
            bool oldExpandValue = ElementExpanded[oldIndex];
            ElementExpanded[oldIndex] = ElementExpanded[newIndex];
            ElementExpanded[newIndex] = oldExpandValue;

            float oldHeightValue = ElementHeights[oldIndex];
            ElementHeights[oldIndex] = ElementHeights[newIndex];
            ElementHeights[newIndex] = oldHeightValue;
        }

        private void UpdateReorderableStatusLists_Add()
        {
            ElementExpanded.Add(false);
            ElementHeights.Add(LIST_ELEMENT_HEIGHT);
        }

        private void UpdateReorderableStatusLists_Remove(ReorderableList list)
        {
            ElementExpanded.RemoveAt(list.index);
            ElementHeights.RemoveAt(list.index);
        }

        private void ReorderItem(ReorderableList list, int oldIndex, int newIndex)
        {
            UpdateReorderableStatusLists_Update(oldIndex, newIndex);
        }

        private void AddItem(ReorderableList list)
        {
            UpdateReorderableStatusLists_Add();

            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            onCreateNewItemCallback(element);
        }

        private void RemoveItem(ReorderableList list)
        {
            onElementRemovedCallback?.Invoke(index);
            UpdateReorderableStatusLists_Remove(list);
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        public CreateNewItemDelegate onCreateNewItemCallback;
        public delegate void CreateNewItemDelegate(SerializedProperty element);

        public ElementeHeightCallback onElementHeightCallback;
        public delegate float ElementeHeightCallback(int index, CustomReorderableList list);

        public ElementeDrawCallback onElementDrawCallback;
        public delegate void ElementeDrawCallback(Rect rect, int index, bool isActive, bool isFocused, CustomReorderableList list);

        public ElementRemovedCallback onElementRemovedCallback;
        public delegate void ElementRemovedCallback(int index);
    }
}

#endif