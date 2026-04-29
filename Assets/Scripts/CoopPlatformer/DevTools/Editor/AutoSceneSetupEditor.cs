using UnityEditor;
using UnityEngine;

namespace CoopPlatformer.DevTools
{
    /// <summary>
    /// Custom Inspector for AutoSceneSetup to add a 'Build Scene' button.
    /// Senior Tip: Custom editors improve developer experience and prevent runtime errors.
    /// </summary>
    [CustomEditor(typeof(AutoSceneSetup))]
    public class AutoSceneSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AutoSceneSetup setup = (AutoSceneSetup)target;

            GUILayout.Space(20);
            GUI.backgroundColor = Color.green;
            
            if (GUILayout.Button("BUILD SPACE SHOOTER SCENE", GUILayout.Height(40)))
            {
                setup.BuildScene();
                // Mark scene as dirty so it can be saved
                EditorUtility.SetDirty(setup);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(setup.gameObject.scene);
            }
            
            GUI.backgroundColor = Color.white;
        }
    }
}
