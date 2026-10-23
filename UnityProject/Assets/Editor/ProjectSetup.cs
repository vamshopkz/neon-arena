#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot (idempotent) project configuration, run from the command line:
///   Unity -batchmode -projectPath . -executeMethod ProjectSetup.Run -quit
/// Layers, URP asset, base materials, importer settings, TMP resources, player settings and
/// the single bootstrap scene.
/// </summary>
public static class ProjectSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    public static void Run()
    {
        ImportTmpEssentials();
        SetupLayers();
        var urp = SetupURP();
        SetupImporters();
        AssetDatabase.Refresh();
        SetupMaterials();
        SetupGraphicsSettings();
        SetupPlayer();
        SetupScene();
        AssetDatabase.SaveAssets();
        Debug.Log("[Setup] Done");
    }

    // ---------------------------------------------------------------------

    static void ImportTmpEssentials()
    {
        if (Directory.Exists("Assets/TextMesh Pro")) return;
        string[] candidates =
        {
            "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage",
            "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage",
        };
        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(full))
            {
                AssetDatabase.ImportPackage(full, false);
                Debug.Log("[Setup] Imported TMP essentials from " + c);
                return;
            }
        }
        Debug.LogError("[Setup] TMP Essential Resources package not found");
    }

    static void SetupLayers()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tm.FindProperty("layers");
        void Set(int i, string n) { var p = layers.GetArrayElementAtIndex(i); if (p.stringValue != n) p.stringValue = n; }
        Set(8, "World"); Set(9, "Hitbox"); Set(10, "ViewModel"); Set(11, "Body"); Set(12, "Ball"); Set(13, "Unused13"); Set(14, "Unused14"); Set(15, "LocalPlayer"); Set(31, "Icon");
        tm.ApplyModifiedPropertiesWithoutUndo();
    }

    static UniversalRenderPipelineAsset SetupURP()
    {
        Directory.CreateDirectory("Assets/Settings");
        const string assetPath = "Assets/Settings/HayURP.asset";
        var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
        if (urp == null)
        {
            var rd = ScriptableObject.CreateInstance<UniversalRendererData>();
            var ppd = AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if (ppd != null) rd.postProcessData = ppd;
            AssetDatabase.CreateAsset(rd, "Assets/Settings/HayRenderer.asset");
            urp = UniversalRenderPipelineAsset.Create(rd);
            AssetDatabase.CreateAsset(urp, assetPath);
        }
        var so = new SerializedObject(urp);
        void F(string name, float v) { var p = so.FindProperty(name); if (p != null) p.floatValue = v; else Debug.LogWarning("[Setup] URP prop missing " + name); }
        void I(string name, int v) { var p = so.FindProperty(name); if (p != null) p.intValue = v; else Debug.LogWarning("[Setup] URP prop missing " + name); }
        void B(string name, bool v) { var p = so.FindProperty(name); if (p != null) p.boolValue = v; else Debug.LogWarning("[Setup] URP prop missing " + name); }
        B("m_SupportsHDR", true);
        I("m_MSAA", 2);
        F("m_RenderScale", 1f);
        F("m_ShadowDistance", 70f);
        I("m_ShadowCascadeCount", 1);
        I("m_MainLightShadowmapResolution", 2048);
        B("m_SoftShadowsSupported", true);
        I("m_AdditionalLightsRenderingMode", 1);   // per pixel (icon key light)
        I("m_AdditionalLightsPerObjectLimit", 4);
        B("m_AdditionalLightShadowsSupported", false);
        B("m_RequireDepthTexture", false);
        B("m_RequireOpaqueTexture", false);
        B("m_UseSRPBatcher", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(urp);

        GraphicsSettings.defaultRenderPipeline = urp;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = urp;
        }
        QualitySettings.SetQualityLevel(current, false);
        return urp;
    }

    static void SetupImporters() { }

    // ---------------------------------------------------------------------

    static Material Save(Material m, string name)
    {
        Directory.CreateDirectory("Assets/Resources/Materials");
        var path = $"Assets/Resources/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.shader = m.shader;
            existing.CopyPropertiesFromMaterial(m);
            existing.shaderKeywords = m.shaderKeywords;
            existing.renderQueue = m.renderQueue;
            existing.enableInstancing = m.enableInstancing;
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static void MakeTransparent(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent;
        m.SetShaderPassEnabled("ShadowCaster", false);
        m.SetShaderPassEnabled("DepthOnly", false);
    }

    static void SetupMaterials()
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        var unlit = Shader.Find("Universal Render Pipeline/Unlit");
        var particles = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        var m = new Material(lit) { enableInstancing = true };
        m.SetFloat("_Smoothness", 0.2f);
        Save(m, "Lit");

        m = new Material(lit) { enableInstancing = true };
                m.EnableKeyword("_NORMALMAP");
        Save(m, "LitNormal");

        // Ground: photo base map + large-scale colour variation as a ×2 detail map (breaks up tiling).
        m = new Material(lit) { enableInstancing = true };
                        m.EnableKeyword("_NORMALMAP");
        m.EnableKeyword("_DETAIL_MULX2");
        Save(m, "LitDetail");

        m = new Material(lit) { enableInstancing = true };
        m.SetColor("_EmissionColor", Color.white);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        Save(m, "LitEmissive");

        m = new Material(lit);
        MakeTransparent(m);
        Save(m, "LitTransparent");

        m = new Material(unlit);
        Save(m, "Unlit");

        m = new Material(unlit);
        MakeTransparent(m);
        Save(m, "UnlitTransparent");

        m = new Material(particles);
        MakeTransparent(m);
        Save(m, "ParticleAlpha");

        m = new Material(particles);
        MakeTransparent(m);
        m.SetFloat("_Blend", 2f);
        m.SetFloat("_DstBlend", (float)BlendMode.One);
        Save(m, "ParticleAdd");

        var sky = Shader.Find("Skybox/Procedural");
        if (sky != null) Save(new Material(sky), "Sky");
        AssetDatabase.SaveAssets();
    }

    static void SetupGraphicsSettings()
    {
        var gs = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        // Fog is switched on at runtime, so keep linear-fog variants rather than letting the build strip them.
        var fogStrip = gs.FindProperty("m_FogStripping");
        if (fogStrip != null) fogStrip.intValue = 1;
        var keepLinear = gs.FindProperty("m_FogKeepLinear");
        if (keepLinear != null) keepLinear.boolValue = true;
        var inst = gs.FindProperty("m_InstancingStripping");
        if (inst != null) inst.intValue = 2; // keep all instancing variants

        var arr = gs.FindProperty("m_AlwaysIncludedShaders");
        foreach (var name in new[] { "TextMeshPro/Mobile/Distance Field", "TextMeshPro/Distance Field", "UI/Default", "Skybox/Procedural" })
        {
            var sh = Shader.Find(name);
            if (sh == null) { Debug.LogWarning("[Setup] Shader not found " + name); continue; }
            bool present = false;
            for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) present = true;
            if (present) continue;
            arr.InsertArrayElementAtIndex(arr.arraySize);
            arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
        }
        gs.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetupPlayer()
    {
        PlayerSettings.companyName = "Aleksandr Kuzmenko";
        PlayerSettings.productName = "Neon Arena";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.aleksandrkuzmenko.neonarena");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.aleksandrkuzmenko.neonarena");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
        PlayerSettings.Android.forceInternetPermission = true;   // multiplayer: Wi-Fi UDP + internet relay
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.Android.renderOutsideSafeArea = true;
        PlayerSettings.Android.startInFullscreen = true;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
        PlayerSettings.gpuSkinning = true;

        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;

        // Voice chat records the microphone (macOS / iOS builds refuse to build without a reason shown to the player).
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.aleksandrkuzmenko.neonarena");

        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/AppIcon.png");
        if (icon != null)
        {
            // Default icon: used by every platform (incl. Android legacy icons) without an override.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        }
        // Android adaptive icon (background = sky, foreground = farmer on the tractor by the hay pile).
        // Without it MIUI and other launchers draw their own placeholder instead of the app icon.
        var iconBg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/AppIconBg.png");
        var iconFg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/AppIconFg.png");
        if (iconBg != null && iconFg != null)
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                if (!kind.ToString().Contains("Adaptive")) continue;
                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (var pi in icons)
                {
                    pi.SetTexture(iconBg, 0);
                    pi.SetTexture(iconFg, 1);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
                Debug.Log($"[Setup] Android {kind} icons: {icons.Length}");
            }
    }

    static void SetupScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("Bootstrap");
        go.AddComponent<Arena.Boot>();
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 160f;
        RenderSettings.fogEndDistance = 330f;
        RenderSettings.fogColor = new Color(0.74f, 0.84f, 0.94f);
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/Sky.mat");
        RenderSettings.ambientMode = AmbientMode.Trilight;
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }
}
#endif
