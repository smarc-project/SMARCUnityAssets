using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(RopeGenerator))]
    public class RopeGeneratorEditor : UnityEditor.Editor
    {
        RopeGenerator container;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            container = (RopeGenerator) target;

            if(GUILayout.Button("(Re)Generate Rope"))
            {
                container.DestroyRope();
                container.SpawnRope();
            }
        }
    }
}