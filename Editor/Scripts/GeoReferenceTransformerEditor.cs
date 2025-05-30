using UnityEditor;
using UnityEngine;

using GeoRef;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GeoReferenceTransformer))]
    public class GeoReferenceTransformerEditor : UnityEditor.Editor
    {
        GeoReferenceTransformer container;

        public override void OnInspectorGUI()
        {
            container = (GeoReferenceTransformer) target;
            DrawDefaultInspector();

            if(GUILayout.Button("Scale to RequiredDistanceBetweenUnityPoints"))
            {
                container.ScaleFromDistance();
            }

            if(GUILayout.Button("Place World Points"))
            {
                container.PlaceWorldPoints();
            }

            if(GUILayout.Button("Set Unity Points to Bounds"))
            {
                container.SetUnityFromBounds();
            }

            if(GUILayout.Button("Transform to Match Earth Points"))
            {
                container.TransformFromTwoPoints();
            }

        }
    }
}