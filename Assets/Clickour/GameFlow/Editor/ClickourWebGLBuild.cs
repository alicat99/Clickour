using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Clickour.GameFlow.Editor
{
    public static class ClickourWebGLBuild
    {
        [MenuItem("Clickour/Build Next WebGL")]
        public static void BuildNext()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;

            var player_settings = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            player_settings.FindProperty("webWasm2023").boolValue = true;
            player_settings.FindProperty("webGLAnalyzeBuildSize").boolValue = false;
            player_settings.ApplyModifiedProperties();

            var output = NextOutputPath();
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
                scenes[i] = EditorBuildSettings.scenes[i].path;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"WebGL build failed: {report.summary.result}");

            CopyBalanceYaml(output);
            Debug.Log($"Clickour WebGL build complete: {output}");
        }

        static void CopyBalanceYaml(string output)
        {
            var streaming_assets = Path.Combine(output, "StreamingAssets");
            Directory.CreateDirectory(streaming_assets);
            File.Copy(
                "Assets/Clickour/Balance/StreamingAssets/clickour_balance.yaml",
                Path.Combine(streaming_assets, "clickour_balance.yaml"));
        }

        static string NextOutputPath()
        {
            Directory.CreateDirectory("Builds");
            for (var index = 1; ; index++)
            {
                var path = $"Builds/Web{index:000}";
                if (!Directory.Exists(path))
                    return path;
            }
        }
    }
}
