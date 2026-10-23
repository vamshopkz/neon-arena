#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Command-line builds: BuildScript.BuildMac (test build), BuildScript.BuildAndroid (APK), BuildScript.BuildWebGL (browser).</summary>
public static class BuildScript
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    static void Finish(BuildReport report, string what)
    {
        var s = report.summary;
        Debug.Log($"[Build] {what}: {s.result}, {s.totalSize / (1024 * 1024)} MB, {s.totalTime.TotalSeconds:0}s, errors {s.totalErrors}, warnings {s.totalWarnings} → {s.outputPath}");
        if (s.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }

    public static void BuildMac()
    {
        ApplyVersionArgs();
        Directory.CreateDirectory("Builds/Mac");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Mac/NeonArena.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        });
        Finish(report, "macOS");
    }

    /// <summary>Optional "-appVersion 1.2 -versionCode 102" from release.sh.</summary>
    static void ApplyVersionArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, "-appVersion");
        if (i >= 0 && i + 1 < args.Length) PlayerSettings.bundleVersion = args[i + 1];
        int j = System.Array.IndexOf(args, "-versionCode");
        if (j >= 0 && j + 1 < args.Length && int.TryParse(args[j + 1], out var code)) PlayerSettings.Android.bundleVersionCode = code;
        Debug.Log($"[Build] version {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
    }

    /// <summary>
    /// Browser version (Builds/WebGL): page template Assets/WebGLTemplates/Arena, gzip with
    /// in-page decompression (static hosts like GitHub Pages don't send the right headers),
    /// ASTC textures for phones.
    /// </summary>
    public static void BuildWebGL()
    {
        ApplyVersionArgs();
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
        PlayerSettings.WebGL.template = "PROJECT:Arena";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.showDiagnostics = false;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        if (Directory.Exists("Builds/WebGL")) Directory.Delete("Builds/WebGL", true);
        Directory.CreateDirectory("Builds/WebGL");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
        Finish(report, "WebGL");
    }

    public static void BuildAndroid()
    {
        ApplyVersionArgs();
        Directory.CreateDirectory("Builds/Android");
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Android/NeonArena.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None,
        });
        Finish(report, "Android");
    }
}
#endif
