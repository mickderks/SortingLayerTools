using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SortingLayerTools
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SortingLayerExposed))]
    public class SortingLayerExposedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // If there is no renderer, we can't do anything
            if (targets.Any(t => (target as SortingLayerExposed).GetComponent<Renderer>() == null))
            {
                EditorGUILayout.HelpBox("SortingLayerExposed must be added to a game object that has a renderer.", MessageType.Error);
                return;
            }

            // Treat all targets as renderers from now on
            var renderers = targets.Select(t => (t as SortingLayerExposed).GetComponent<Renderer>()).ToList();

            var sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();

            // Are all renderers using the same sorting layer?
            var useSameLayer = renderers.All(r => r.sortingLayerID == renderers[0].sortingLayerID);
            // Are all renderers using the same sorting order?
            var useSameOrder = renderers.All(r => r.sortingOrder == renderers[0].sortingOrder);

            if (useSameLayer)
            {
                // Since all renderers have the same sorting layer, we can change all of them if the user selects a new layer.
                var oldLayerIndex = Array.IndexOf(sortingLayerNames, SortingLayer.IDToName(renderers[0].sortingLayerID));
                int newLayerIndex = EditorGUILayout.Popup("Sorting Layer", oldLayerIndex, sortingLayerNames);
                if (newLayerIndex != oldLayerIndex)
                {
                    Undo.RecordObjects(renderers.ToArray(), "Edit Sorting Layer");
                    var newId = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);
                    foreach (var r in renderers)
                    {
                        r.sortingLayerID = newId;
                        EditorUtility.SetDirty(r);
                    }
                }
            }
            else
            {
                // Since not all renderers use the same sorting layer, we only change the ones that have actually changed.
                EditorGUI.showMixedValue = true; // Makes unity show the following items in the inspector as 'different'
                int newLayerIndex = EditorGUILayout.Popup("Sorting Layer", -1, sortingLayerNames);
                EditorGUI.showMixedValue = false;
                if (newLayerIndex != -1)
                {
                    var newId = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);
                    // Select only the renderers that have changed.
                    var toChange = renderers.Where(r => r.sortingLayerID != newId);
                    Undo.RecordObjects(toChange.ToArray(), "Edit Sorting Layer");
                    foreach (var r in toChange)
                    {
                        r.sortingLayerID = newId;
                        EditorUtility.SetDirty(r);
                    }
                }
            }

            if (useSameOrder)
            {
                // Since all renderers have the same sorting order, we can change all of them if the user selects a new layer.
                var oldLayerOrder = renderers[0].sortingOrder;
                var newLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderers[0].sortingOrder);
                if (newLayerOrder != oldLayerOrder)
                {
                    Undo.RecordObjects(renderers.ToArray(), "Edit Sorting Order");
                    foreach (var r in renderers)
                    {
                        r.sortingOrder = newLayerOrder;
                        EditorUtility.SetDirty(r);
                    }
                }
            }
            else
            {
                // Since not all renderers use the same sorting layer, we only change the ones that have actually changed.
                EditorGUI.BeginChangeCheck(); // Used this here for simplicity
                EditorGUI.showMixedValue = true; // Makes unity show the following items in the inspector as 'different'
                var newLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderers[0].sortingOrder);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    // Select only the renderers that have changed.
                    var toChange = renderers.Where(r => r.sortingOrder != newLayerOrder);
                    Undo.RecordObjects(toChange.ToArray(), "Edit Sorting Order");
                    foreach (var r in toChange)
                    {
                        r.sortingOrder = newLayerOrder;
                        EditorUtility.SetDirty(r);
                    }
                }
            }
        }
    }
}
