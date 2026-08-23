using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clickour.GameFlow.Editor
{
    public static class GameViewCapture
    {
        [MenuItem("Clickour/Capture Game View")]
        public static void Capture()
        {
            var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/DesignReview"));
            Directory.CreateDirectory(directory);

            var scene_name = SceneManager.GetActiveScene().name.ToLowerInvariant();
            var path = Path.Combine(directory, $"{scene_name}_gameview_latest.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"Game View capture queued: {path}");
        }
    }
}
