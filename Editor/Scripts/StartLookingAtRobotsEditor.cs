using UnityEditor;
using UnityEngine;

using SmarcGUI.WorldSpace;

namespace Editor.Scripts
{
    [CustomEditor(typeof(StartLookingAtRobots))]
    public class StartLookingAtRobotsEditor : UnityEditor.Editor
    {
        StartLookingAtRobots container;

        public override void OnInspectorGUI()
        {
            container = (StartLookingAtRobots) target;
            DrawDefaultInspector();

            if(GUILayout.Button("Look at some robots NOW!"))
            {
                container.Look();
            }
        }
    }
}