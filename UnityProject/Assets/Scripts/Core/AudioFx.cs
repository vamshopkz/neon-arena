using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>All sounds are synthesized at start (no audio files): shots, explosions, hits, UI.</summary>
    public class AudioFx : MonoBehaviour
    {
        static AudioFx I;
        AudioSource src, music;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        const int SR = 22050;

        public static void Create()
        {
            var go = new GameObject("Audio");
            DontDestroyOnLoad(go);
            I = go.AddComponent<AudioFx>();
            I.src = go.AddComponent<AudioSource>();
            I.music = go.AddComponent<AudioSource>();
            I.Make();
            SetVolume(Save.P.volume);
        }

        public static void SetVolume(float v) { AudioListener.volume = Mathf.Clamp01(v); }

        delegate float Wave(float t);

        void Clip(string name, float len, Wave w)
        {
            int n = (int)(SR * len);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(w((float)i / SR), -1, 1);
            var c = AudioClip.Create(name, n, 1, SR, false);
            c.SetData(data, 0);
            clips[name] = c;
        }

        static float Env(float t, float a, float d) => t < a ? t / a : Mathf.Exp(-(t - a) / d);

        void Make()
        {
            var rnd = new System.Random(5);
            float Noise() => (float)(rnd.NextDouble() * 2 - 1);
            float lp = 0;
            float Low(float k) { lp += (Noise() - lp) * k; return lp; }

            Clip("click", 0.06f, t => Mathf.Sin(t * 2 * Mathf.PI * 900) * Env(t, 0.002f, 0.015f) * 0.35f);
            Clip("coin", 0.35f, t => (Mathf.Sin(t * 2 * Mathf.PI * (t < 0.08f ? 1320 : 1760)) * 0.5f + Mathf.Sin(t * 2 * Mathf.PI * 2640) * 0.15f) * Env(t, 0.004f, 0.12f) * 0.45f);
            Clip("buy", 0.4f, t => (Mathf.Sin(t * 2 * Mathf.PI * 660) + Mathf.Sin(t * 2 * Mathf.PI * (t < 0.12f ? 880 : 1100))) * 0.25f * Env(t, 0.005f, 0.15f));
            Clip("error", 0.3f, t => Mathf.Sign(Mathf.Sin(t * 2 * Mathf.PI * 160)) * 0.18f * Env(t, 0.005f, 0.12f));
            Clip("rifle", 0.22f, t => (Low(0.6f) * 0.9f + Mathf.Sin(t * 2 * Mathf.PI * (140 - t * 300)) * 0.6f) * Env(t, 0.001f, 0.045f));
            Clip("pistol", 0.18f, t => (Low(0.75f) * 0.7f + Mathf.Sin(t * 2 * Mathf.PI * (220 - t * 600)) * 0.4f) * Env(t, 0.001f, 0.035f));
            Clip("shotgun", 0.5f, t => (Low(0.4f) * 1.3f + Mathf.Sin(t * 2 * Mathf.PI * (110 - t * 120)) * 0.7f) * Env(t, 0.001f, 0.09f));
            Clip("sniper", 0.6f, t => (Low(0.35f) * 1.1f + Mathf.Sin(t * 2 * Mathf.PI * (90 - t * 80)) * 0.7f) * Env(t, 0.001f, 0.12f));
            Clip("rocket", 0.7f, t => (Low(0.15f) * 0.9f + Mathf.Sin(t * 2 * Mathf.PI * (300 - t * 300)) * 0.15f) * Env(t, 0.01f, 0.25f));
            Clip("boom", 1.4f, t => (Low(0.06f) * 2.2f + Mathf.Sin(t * 2 * Mathf.PI * (70 - t * 40)) * 0.8f) * Env(t, 0.002f, 0.35f));
            Clip("throw", 0.2f, t => Noise() * Mathf.Sin(t * Mathf.PI / 0.2f) * 0.25f);
            Clip("melee", 0.18f, t => (Noise() * 0.4f) * Mathf.Sin(t * Mathf.PI / 0.18f) * (0.5f + t * 4));
            Clip("hit", 0.07f, t => Mathf.Sign(Mathf.Sin(t * 2 * Mathf.PI * 1200)) * Env(t, 0.001f, 0.02f) * 0.18f);
            Clip("head", 0.12f, t => (Mathf.Sin(t * 2 * Mathf.PI * 1800) + Mathf.Sin(t * 2 * Mathf.PI * 2400)) * Env(t, 0.001f, 0.04f) * 0.22f);
            Clip("kill", 0.35f, t => Mathf.Sin(t * 2 * Mathf.PI * (t < 0.1f ? 880 : 1320)) * Env(t, 0.002f, 0.12f) * 0.35f);
            Clip("hurt", 0.25f, t => Mathf.Sin(t * 2 * Mathf.PI * (200 - t * 400)) * Env(t, 0.005f, 0.08f) * 0.4f);
            Clip("reload", 0.5f, t => Noise() * ((t < 0.05f || (t > 0.3f && t < 0.36f)) ? 0.35f : 0));
            Clip("empty", 0.05f, t => Mathf.Sin(t * 2 * Mathf.PI * 2000) * Env(t, 0.001f, 0.01f) * 0.25f);
            Clip("beep", 0.15f, t => Mathf.Sin(t * 2 * Mathf.PI * 880) * Env(t, 0.005f, 0.08f) * 0.35f);
            Clip("go", 0.5f, t => Mathf.Sin(t * 2 * Mathf.PI * 1320) * Env(t, 0.005f, 0.2f) * 0.4f);
            Clip("portal", 0.8f, t => Mathf.Sin(t * 2 * Mathf.PI * (200 + t * 900)) * Env(t, 0.05f, 0.3f) * 0.3f);
            Clip("pickup", 0.3f, t => Mathf.Sin(t * 2 * Mathf.PI * (600 + t * 2000)) * Env(t, 0.005f, 0.1f) * 0.3f);
            Clip("goal", 1.2f, t => (Mathf.Sin(t * 2 * Mathf.PI * 523) + Mathf.Sin(t * 2 * Mathf.PI * 659) + Mathf.Sin(t * 2 * Mathf.PI * 784)) * Env(t, 0.01f, 0.5f) * 0.18f + Low(0.3f) * Env(t, 0.05f, 0.6f) * 0.3f);
            Clip("win", 1.2f, t => Mathf.Sin(t * 2 * Mathf.PI * (t < 0.2f ? 523 : t < 0.4f ? 659 : t < 0.6f ? 784 : 1046)) * Env(t, 0.01f, 0.6f) * 0.3f);
            Clip("lose", 1f, t => Mathf.Sin(t * 2 * Mathf.PI * (t < 0.3f ? 392 : t < 0.6f ? 330 : 262)) * Env(t, 0.01f, 0.5f) * 0.3f);
            Clip("jump", 0.2f, t => Mathf.Sin(t * 2 * Mathf.PI * (300 + t * 1200)) * Env(t, 0.005f, 0.07f) * 0.2f);
            Clip("step", 0.08f, t => Low(0.3f) * Env(t, 0.002f, 0.02f) * 0.25f);
        }

        public static void Play(string name, float vol = 1f)
        {
            if (I == null || !I.clips.TryGetValue(name, out var c)) return;
            I.src.PlayOneShot(c, vol);
        }

        /// <summary>A sound at a world position: quieter with distance from the listener.</summary>
        public static void At(string name, Vector3 pos, float vol = 1f)
        {
            var cam = Camera.main;
            float d = cam != null ? Vector3.Distance(cam.transform.position, pos) : 0;
            float k = vol / (1 + d * 0.06f);
            if (k > 0.02f) Play(name, k);
        }
    }
}
