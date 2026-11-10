#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds an installable Android APK from the enabled scenes in Build Settings.
/// Menu: Carrom > Build Android APK
/// CLI:   Unity -batchmode -quit -projectPath . -executeMethod AndroidBuilder.BuildApk -logFile build.log
/// </summary>
public static class AndroidBuilder
{
    private const string OutputDirectory = "Builds/Android";
    private const string OutputApk = OutputDirectory + "/Carrom.apk";

    [MenuItem("Carrom/Build Android APK")]
    public static void BuildFromMenu()
    {
        Build(false);
    }

    public static void BuildApk()
    {
        Build(true);
    }

    private static void Build(bool exitWhenDone)
    {
        bool success = false;

        try
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes in Build Settings.");
            }

            Directory.CreateDirectory(OutputDirectory);

            // An APK (not an App Bundle) is what sideloading via adb expects.
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputApk,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            success = report.summary.result == BuildResult.Succeeded;

            if (success)
            {
                Debug.Log("[AndroidBuilder] APK built: " + Path.GetFullPath(OutputApk));
            }
            else
            {
                Debug.LogError("[AndroidBuilder] Build failed: " + report.summary.result +
                               " (" + report.summary.totalErrors + " errors)");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (exitWhenDone)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }
    }
}
#endif
