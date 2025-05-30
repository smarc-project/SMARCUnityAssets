using UnityEditor;
using UnityEngine;

using GeoRef;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GlobalReferencePoint))]
    public class GlobalReferencePointEditor : UnityEditor.Editor
    {
        GlobalReferencePoint container;

        public override void OnInspectorGUI()
        {
            container = (GlobalReferencePoint) target;
            DrawDefaultInspector();

            if(GUILayout.Button("Update Geo-referenced objects in scene"))
            {
                container.UpdateGeoRefObjects();
            }
        }
    }
}