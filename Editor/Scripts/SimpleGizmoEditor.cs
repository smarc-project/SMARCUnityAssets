using UnityEditor;
using UnityEngine;

using static SimpleGizmo;

namespace Editor.Scripts
{
    [CustomEditor(typeof(SimpleGizmo))]
    public class SimpleGizmoEditor : UnityEditor.Editor
    {
        SimpleGizmo container;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            container = (SimpleGizmo)target;

            if (GUILayout.Button("CreateObject"))
            {
                container.CreateObject();
            }
        }
    }
}