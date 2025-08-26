using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MaterialShaderChanger : EditorWindow
    {
        private Shader newShader;
        private Shader oldShader;

        [MenuItem("SMaRC/Change All Materials Shader")]
        public static void ShowWindow()
        {
            GetWindow<MaterialShaderChanger>("Material Shader Changer");
        }

        void OnGUI()
        {
            GUILayout.Label("Change All Project Materials Shader", EditorStyles.boldLabel);

            string[] guids = AssetDatabase.FindAssets("t:Shader", new string[] { "Assets", "Packages" });
            Shader[] allShaders = guids.Select(g => AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(g))).ToArray();

            int currentIndex = newShader != null ? System.Array.IndexOf(allShaders, newShader) : 0;
            int selected = EditorGUILayout.Popup("Shader", currentIndex, allShaders.Select(s => s.name).ToArray());
            newShader = allShaders[selected];

            if (GUILayout.Button("Apply to All Materials"))
            {
                if (newShader == null)
                {
                    Debug.LogWarning("⚠ Please assign a shader first.");
                    return;
                }

                ChangeAllMaterialsShader(newShader);
            }
        }

        private static void ChangeAllMaterialsShader(Shader shader)
        {
            string[] guids = AssetDatabase.FindAssets("t:Material", new []{"Assets", "Packages/com.smarc.assets"});
            int changedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat != null && mat.shader != shader)
                {
                   // Undo.RecordObject(mat, "Change Material Shader");
                  //  mat.shader = shader;
                  //  EditorUtility.SetDirty(mat);
                    changedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"✅ Changed {changedCount} materials to shader: {shader.name}");
        }
    }
}