using UnityEditor;
using UnityEngine;

using GeoRef;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GeoReference))]
    public class GeoReferenceEditor : UnityEditor.Editor
    {
        GeoReference container;

        public override void OnInspectorGUI()
        {
            container = (GeoReference) target;
            DrawDefaultInspector();

            if(GUILayout.Button("Place in World from Lat/Lon"))
            {
                container.Place();
            }
        }
    }
}