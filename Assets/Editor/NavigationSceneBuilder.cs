#if UNITY_EDITOR
using System.Linq;
using ReCiclo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReCiclo.Editor
{
    [InitializeOnLoad]
    public static class NavigationSceneBuilder
    {
        private const string BuildKey = "ReCiclo_Navigation_v1";

        static NavigationSceneBuilder()
        {
            EditorApplication.delayCall += BuildOnce;
        }

        private static void BuildOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SessionState.GetBool(BuildKey, false)) return;
            BuildNavigationScenes();
            SessionState.SetBool(BuildKey, true);
        }

        [MenuItem("ReCiclo/Construir menú y mapa de 5 niveles", false, 0)]
        public static void BuildNavigationScenes()
        {
            string previousPath = SceneManager.GetActiveScene().path;
            CreateScene("Assets/Scenes/Inicio.unity");
            CreateScene("Assets/Scenes/MapaMundial.unity");

            string[] desired =
            {
                "Assets/Scenes/Inicio.unity",
                "Assets/Scenes/MapaMundial.unity",
                "Assets/Scenes/Reciclo2D_Game.unity"
            };
            var extras = EditorBuildSettings.scenes.Where(s => !desired.Contains(s.path));
            EditorBuildSettings.scenes = desired.Select(p => new EditorBuildSettingsScene(p, true)).Concat(extras).ToArray();

            if (!string.IsNullOrEmpty(previousPath)) EditorSceneManager.OpenScene(previousPath);
            else EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[ReCiclo] Menú inicial y mapa de cinco niveles creados. Inicio es ahora la escena 0.");
        }

        private static void CreateScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            new GameObject("Navigation", typeof(ReCicloNavigation));
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
#endif
