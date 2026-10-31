using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Arena
{
    /// <summary>
    /// 1.1: updates from the menu. The game looks for a newer version on start and every 10 minutes;
    /// only when there is one does the main menu show the «ОБНОВИТЬ» button.
    /// Phone (APK): version.json + NeonArena.apk of the latest GitHub release (release.sh); the button
    /// downloads the APK, checks it (size, SHA-256, zip) and hands it to Android's installer — the save
    /// is kept, every build is signed with the same key.
    /// Browser: version.json next to the page (publish_web.sh); the button reloads the page.
    /// </summary>
    public class Updater : MonoBehaviour
    {
        public static Updater I;

        [Serializable]
        class VersionInfo { public string version = "", notes = "", apk = "", sha256 = ""; public long size; }

        public enum State { Off, Idle, Checking, UpToDate, Available, Downloading, Verifying, ReadyToInstall, Error }

        public State Status { get; private set; } = State.Idle;
        public string LatestVersion { get; private set; } = "";
        public float Progress { get; private set; }
        /// <summary>The menu shows the button only while there is something newer to get.</summary>
        public static bool HasUpdate => I != null && I.Status >= State.Available && !string.IsNullOrEmpty(I.LatestVersion);

        string baseUrl = "", apkName = "NeonArena.apk", expectedSha = "", downloadedPath, pendingInstall;
        long expectedSize;
        bool toasted;
        float nextCheck;

        static string UpdateDir => Path.Combine(Application.persistentDataPath, "update");

        public static void Create()
        {
            I = new GameObject("Updater").AddComponent<Updater>();
            DontDestroyOnLoad(I.gameObject);
        }

        void Awake()
        {
            if (WebBridge.IsWeb)
            {
                // the page's own folder: https://…github.io/neon-arena/
                var u = Application.absoluteURL ?? "";
                int q = u.IndexOfAny(new[] { '?', '#' });
                if (q >= 0) u = u.Substring(0, q);
                baseUrl = u.Substring(0, u.LastIndexOf('/') + 1);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                var cfg = Resources.Load<TextAsset>("update_base");
                baseUrl = cfg != null ? cfg.text.Trim() : "";
            }
            if (string.IsNullOrEmpty(baseUrl)) Status = State.Off;   // computers, the editor: nothing to update
            // anything downloaded earlier is either installed by now (we are running it) or stale
            try { if (Directory.Exists(UpdateDir)) Directory.Delete(UpdateDir, true); } catch { }
        }

        void Update()
        {
            if (Status == State.Off || Status == State.Checking || HasUpdate) return;
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 600f;
            StartCoroutine(Check());
        }

        IEnumerator Check()
        {
            Status = State.Checking;
            // cache-buster: the CDN may hand back a stale version.json for a while
            using var req = UnityWebRequest.Get(baseUrl + "version.json?t=" + DateTime.UtcNow.Ticks);
            req.timeout = 15;
            yield return req.SendWebRequest();
            VersionInfo info = null;
            if (req.result == UnityWebRequest.Result.Success)
                try { info = JsonUtility.FromJson<VersionInfo>(req.downloadHandler.text); } catch { }
            if (info == null || string.IsNullOrEmpty(info.version)) { Status = State.Idle; yield break; }   // no internet: try later
            if (Compare(info.version, Application.version) <= 0) { Status = State.UpToDate; yield break; }
            LatestVersion = info.version;
            expectedSize = info.size;
            expectedSha = (info.sha256 ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(info.apk)) apkName = info.apk;
            Status = State.Available;
            Debug.Log($"[Update] {Application.version} → {LatestVersion} available");
            if (!toasted && !Menus.MainOpen) { toasted = true; Hud.Toast("Вышла новая версия " + LatestVersion + " — кнопка «ОБНОВИТЬ» в меню", UIKit.Lime); }
        }

        /// <summary>Compare dotted versions numerically: "1.10" > "1.9".</summary>
        public static int Compare(string a, string b)
        {
            var pa = a.Split('.');
            var pb = b.Split('.');
            for (int i = 0; i < Mathf.Max(pa.Length, pb.Length); i++)
            {
                int x = i < pa.Length && int.TryParse(pa[i], out var xa) ? xa : 0;
                int y = i < pb.Length && int.TryParse(pb[i], out var yb) ? yb : 0;
                if (x != y) return x.CompareTo(y);
            }
            return 0;
        }

        /// <summary>What the menu button says right now.</summary>
        public string ButtonText => Status switch
        {
            State.Downloading => $"СКАЧИВАЮ… {Mathf.RoundToInt(Progress * 100)}%",
            State.Verifying => "ПРОВЕРЯЮ…",
            State.ReadyToInstall => "УСТАНОВИТЬ " + LatestVersion,
            State.Error => "ОБНОВИТЬ — ЕЩЁ РАЗ",
            _ => "ОБНОВИТЬ ДО " + LatestVersion,
        };

        /// <summary>The menu button.</summary>
        public void Press()
        {
            if (!HasUpdate || Status == State.Downloading || Status == State.Verifying) return;
            Save.Write();
            if (WebBridge.IsWeb) { WebBridge.SyncFiles(); WebBridge.Reload(); return; }
            if (Status == State.ReadyToInstall && downloadedPath != null && File.Exists(downloadedPath)) { Install(downloadedPath); return; }
            StartCoroutine(Download());
        }

        IEnumerator Download()
        {
            downloadedPath = null;
            Directory.CreateDirectory(UpdateDir);
            var part = Path.Combine(UpdateDir, "download.part");
            var final = Path.Combine(UpdateDir, $"NeonArena-{LatestVersion}.apk");
            string url = baseUrl.EndsWith("/releases/latest/download/")
                ? baseUrl.Substring(0, baseUrl.Length - "latest/download/".Length) + $"download/v{LatestVersion}/{apkName}"   // exactly the version we checked
                : baseUrl + apkName;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Progress = 0f;
                Status = State.Downloading;
                TryDelete(part);
                var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET) { downloadHandler = new DownloadHandlerFile(part) { removeFileOnAbort = true } };
                req.timeout = 900;
                var op = req.SendWebRequest();
                while (!op.isDone) { Progress = req.downloadProgress; yield return null; }
                bool ok = req.result == UnityWebRequest.Result.Success;
                // the file is complete and closed only once the request is disposed
                req.Dispose();
                yield return null;
                if (!ok) continue;
                Status = State.Verifying;
                if (expectedSize > 0 && new FileInfo(part).Length != expectedSize) continue;
                var check = Task.Run(() => Inspect(part, expectedSha));
                while (!check.IsCompleted) yield return null;
                if (check.Exception != null || check.Result != "") continue;
                try { TryDelete(final); File.Move(part, final); } catch { continue; }
                downloadedPath = final;
                Status = State.ReadyToInstall;
                Install(final);
                yield break;
            }
            TryDelete(part);
            Status = State.Error;
            Hud.Toast("Обновление не скачалось. Проверьте интернет и нажмите ещё раз", UIKit.Red);
        }

        /// <summary>A downloaded APK: the problem, or "" when it is fine.</summary>
        static string Inspect(string path, string sha)
        {
            using (var fs = File.OpenRead(path))
            {
                if (fs.Length < 1024) return "empty";
                var head = new byte[4];
                if (fs.Read(head, 0, 4) != 4 || head[0] != 0x50 || head[1] != 0x4B || head[2] != 3 || head[3] != 4) return "not an apk";
                // a complete zip ends with an End Of Central Directory record
                int n = (int)Math.Min(fs.Length, 66000);
                fs.Seek(-n, SeekOrigin.End);
                var tail = new byte[n];
                int read = 0;
                while (read < n) { int r = fs.Read(tail, read, n - read); if (r <= 0) break; read += r; }
                bool eocd = false;
                for (int i = read - 22; i >= 0 && !eocd; i--) eocd = tail[i] == 0x50 && tail[i + 1] == 0x4B && tail[i + 2] == 5 && tail[i + 3] == 6;
                if (!eocd) return "cut off";
            }
            if (!string.IsNullOrEmpty(sha))
            {
                using var fs = File.OpenRead(path);
                using var h = SHA256.Create();
                var hex = new StringBuilder(64);
                foreach (var b in h.ComputeHash(fs)) hex.Append(b.ToString("x2"));
                if (hex.ToString() != sha) return "damaged";
            }
            return "";
        }

        static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }

        void Install(string path, bool fromResume = false)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                string pkg = context.Call<string>("getPackageName");
                var pm = context.Call<AndroidJavaObject>("getPackageManager");
                if (!pm.Call<bool>("canRequestPackageInstalls"))
                {
                    if (fromResume) { Hud.Toast("Установка не разрешена. Нажмите «УСТАНОВИТЬ» ещё раз", UIKit.Gold); return; }
                    // Android asks once: "allow Neon Arena to install apps"; we carry on when the player comes back
                    pendingInstall = path;
                    using var uriClass = new AndroidJavaClass("android.net.Uri");
                    var pkgUri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + pkg);
                    activity.Call("startActivity", new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_UNKNOWN_APP_SOURCES", pkgUri));
                    Hud.Toast("Разрешите установку для Неон Арены и вернитесь в игру", UIKit.Cyan);
                    return;
                }
                var file = new AndroidJavaObject("java.io.File", path);
                using var provider = new AndroidJavaClass("androidx.core.content.FileProvider");
                var uri = provider.CallStatic<AndroidJavaObject>("getUriForFile", context, pkg + ".arenaupdate", file);
                var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
                intent.Call<AndroidJavaObject>("addFlags", 0x00000001 | 0x10000000);   // GRANT_READ_URI_PERMISSION | NEW_TASK
                activity.Call("startActivity", intent);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Update] install: " + e.Message);
                Status = State.Error;
                Application.OpenURL(baseUrl + apkName);   // fallback: the phone's browser downloads it
                Hud.Toast("Браузер скачает файл — откройте его и нажмите «Установить»", UIKit.Cyan);
            }
#endif
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus || pendingInstall == null) return;
            var p = pendingInstall;
            pendingInstall = null;
            Install(p, true);
        }

        /// <summary>Tests: pretend a newer version came out (or not).</summary>
        public void Fake(string version)
        {
            LatestVersion = version ?? "";
            Status = string.IsNullOrEmpty(version) ? State.UpToDate : State.Available;
        }
    }
}
