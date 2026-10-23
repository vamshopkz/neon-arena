using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Arena
{
    /// <summary>Entry point (the only object in the scene): builds the whole game from code.</summary>
    public class Boot : MonoBehaviour
    {
        public static string[] Args = Array.Empty<string>();
        public static bool Testing;
        public static string Arg(string key)
        {
            int i = Array.IndexOf(Args, key);
            return i >= 0 && i + 1 < Args.Length ? Args[i + 1] : null;
        }
        public static bool Has(string key) => Array.IndexOf(Args, key) >= 0;

        bool wasLocked;

        void Awake()
        {
            Args = Environment.GetCommandLineArgs();
            Testing = Has("-autotest") || Has("-mptest");
            if (Testing) Save.FileName = "test_" + (Arg("-name") ?? "auto") + ".json";
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Save.Load();
            if (Testing)
            {
                Save.P = new Profile { id = Arg("-id") ?? Save.NewId(), name = Arg("-name") ?? "Тестер", coins = 50000 };
            }
            bool mobile = Application.isMobilePlatform;
            bool shadows = !mobile || !WebBridge.IsWeb;
            var urp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.renderScale = mobile ? (WebBridge.IsWeb ? 0.7f : 0.85f) : 1f;
                urp.shadowDistance = shadows ? 60 : 0;
                urp.msaaSampleCount = mobile ? 1 : 4;
                urp.supportsHDR = !mobile;
            }
            QualitySettings.shadows = shadows ? UnityEngine.ShadowQuality.All : UnityEngine.ShadowQuality.Disable;

            UIRoot.Create();
            AudioFx.Create();
            PlayerCtl.Create();
            PostFx(mobile);
            Net.Create();
            Game.Create(shadows);
            Hud.Create();
            Game.I.EnterLobby();

            if (Testing) gameObject.AddComponent<AutoTest>();
            else Menus.Main();
        }

        static void PostFx(bool mobile)
        {
            var go = new GameObject("PostFx");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            var p = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = p.Add<Bloom>(true);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(1.1f);
            bloom.scatter.Override(0.7f);
            bloom.highQualityFiltering.Override(false);
            var tm = p.Add<Tonemapping>(true);
            tm.mode.Override(TonemappingMode.ACES);
            var color = p.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.3f);
            color.contrast.Override(10f);
            color.saturation.Override(12f);
            var vig = p.Add<Vignette>(true);
            vig.intensity.Override(0.22f);
            vol.profile = p;
            var data = PlayerCtl.I.Cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = !(mobile && WebBridge.IsWeb);
            data.antialiasing = mobile ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        void Update()
        {
            if (Testing) return;
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIRoot.Open != null) { Window.Close(); }
                else if (!Menus.OverlayOpen) Menus.Pause();
            }
            else if (wasLocked && !locked && !Menus.AnyOpen) Menus.Pause();   // the browser released the mouse (Esc)
            if (!Hud.Touch && !locked && !Menus.AnyOpen && Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                Menus.LockCursor();
            if (!Hud.Touch && Menus.AnyOpen && locked) Menus.Unlock();
            wasLocked = Cursor.lockState == CursorLockMode.Locked;
        }

        void OnApplicationPause(bool paused) { if (paused) Save.Write(); }
        void OnApplicationQuit() { Save.Write(); }
    }
}
