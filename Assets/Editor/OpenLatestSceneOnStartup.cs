#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReCiclo.Editor
{
    /// <summary>
    /// Opens the current entry scene once when the Unity project starts.
    /// It never rebuilds or overwrites scene contents.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenLatestSceneOnStartup
    {
        private const string StartupScene = "Assets/Scenes/Inicio.unity";
        private const string SessionKey = "ReCiclo_StartupSceneOpened_20261016";

        static OpenLatestSceneOnStartup()
        {
            EditorApplication.delayCall += OpenStartupSceneOnce;
        }

        private static void OpenStartupSceneOnce()
        {
            if (SessionState.GetBool(SessionKey, false) ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += OpenStartupSceneOnce;
                return;
            }

            SessionState.SetBool(SessionKey, true);

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == StartupScene)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(StartupScene, OpenSceneMode.Single);
        }
    }
}
#endif
