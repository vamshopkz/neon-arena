using System.Collections.Generic;
using UnityEngine;

namespace Arena.Real
{
    /// <summary>
    /// The human's movements (2.15), made in code: idle breathing, walk, run, jump, fall, crouch,
    /// crouch-walk, the high jump with a front flip, collecting, the fork stab, waving and dancing.
    /// States crossfade like the old character's animation clips and take the same names
    /// ("idle", "walk", "sprint", "jump", "fall", "pick-up", "interact-right"), so the player and the
    /// friend's copy drive it the same way.
    /// Angles: −X swings a limb forward, +X bends the knee (shin) / leans the body forward,
    /// −X bends the elbow (forearm); +Z lifts the right arm sideways, −Z the left.
    /// </summary>
    public class HumanAnimator : MonoBehaviour
    {
        const int J = 16;
        const int Hips = 0, Torso = 1, Neck = 2, Head = 3, UArmL = 4, UArmR = 5, FArmL = 6, FArmR = 7, HandL = 8, HandR = 9,
                  ThighL = 10, ThighR = 11, ShinL = 12, ShinR = 13, FootL = 14, FootR = 15;

        class Pose
        {
            public readonly Vector3[] e = new Vector3[J];
            public Vector3 hipsOffset;
            public float flip;
            public void Clear() { for (int i = 0; i < J; i++) e[i] = Vector3.zero; hipsOffset = Vector3.zero; flip = 0f; }
        }

        public HumanRig h;
        Transform[] joints;
        Vector3[] restPos;
        readonly Pose a = new Pose(), b = new Pose(), o = new Pose();
        string cur = "idle", prev = "idle";
        float curT, prevT, fade = 0.2f, fadeT = 1f;
        float curSpeed = 1f, prevSpeed = 1f;
        /// <summary>A phone in the left hand: that arm holds it up in front (2.15).</summary>
        public bool HoldingPhone;
        float phoneW;
        /// <summary>How long the high jump is in the air (the flip fits into it).</summary>
        public float HighJumpTime = 1.1f;

        public string Current => cur;
        public static readonly string[] States = { "idle", "walk", "sprint", "jump", "fall", "crouch", "crouch-walk", "highjump", "pick-up", "interact-right", "wave", "dance", "sit", "drive", "type" };
        public bool Has(string s) => System.Array.IndexOf(States, s) >= 0;

        public static HumanAnimator Attach(HumanRig h)
        {
            var an = h.gameObject.GetComponent<HumanAnimator>();
            if (an == null) an = h.gameObject.AddComponent<HumanAnimator>();
            an.Init(h);
            return an;
        }

        void Init(HumanRig rig)
        {
            h = rig;
            joints = rig.Joints;
            restPos = new Vector3[J];
            for (int i = 0; i < J; i++) restPos[i] = joints[i] != null ? joints[i].localPosition : Vector3.zero;
            Evaluate(cur, 0f, 1f, b);
            Apply(b);
            Human.CurlHand(rig.fingersL, rig.curlL, -1f);
            Human.CurlHand(rig.fingersR, rig.curlR, 1f);
            lidRest.Clear();
            foreach (var l in rig.lids) lidRest.Add(l != null ? l.localPosition : Vector3.zero);
        }

        readonly List<Vector3> lidRest = new List<Vector3>();
        float curlL = 0.2f, curlR = 0.2f, blinkT = 2f, blink;

        /// <summary>Hands open and close smoothly; the eyes blink every few seconds.</summary>
        void HandsAndEyes(float dt)
        {
            curlL = Mathf.MoveTowards(curlL, HoldingPhone ? 0.55f : h.curlL, dt * 4f);
            curlR = Mathf.MoveTowards(curlR, h.curlR, dt * 4f);
            Human.CurlHand(h.fingersL, curlL, -1f);
            Human.CurlHand(h.fingersR, curlR, 1f);
            blinkT -= dt;
            if (blinkT <= 0f) { blinkT = Random.Range(2.2f, 5f); blink = 1f; }
            blink = Mathf.MoveTowards(blink, 0f, dt * 7f);
            float closed = Mathf.Sin(blink * Mathf.PI);
            for (int i = 0; i < h.lids.Count && i < lidRest.Count; i++)
                if (h.lids[i] != null) h.lids[i].localPosition = lidRest[i] + Vector3.down * (0.011f * closed);
        }

        /// <summary>The standing pose straight after building (no animator needed for pictures).</summary>
        public static void ApplyRest(HumanRig rig)
        {
            var tmp = new GameObject("tmp").AddComponent<HumanAnimator>();
            tmp.Init(rig);
            if (Application.isPlaying) Destroy(tmp.gameObject); else DestroyImmediate(tmp.gameObject);
        }

        public void Play(string state, float fadeTime, bool restart = false)
        {
            if (!Has(state)) return;
            if (state == cur && !restart) return;
            prev = cur; prevT = curT; prevSpeed = curSpeed;
            cur = state; curT = 0f;
            fade = Mathf.Max(0.0001f, fadeTime); fadeT = fadeTime <= 0f ? 1f : 0f;
        }

        /// <summary>Freeze the body in one moment of a state (pictures, tests).</summary>
        public void Sample(string state, float t)
        {
            enabled = false;
            Evaluate(state, t, 1f, b);
            if (HoldingPhone && state != "pick-up" && state != "interact-right" && state != "highjump" && state != "wave" && state != "dance")
            {
                b.e[UArmL] = new Vector3(-28f, 0f, -12f); b.e[FArmL] = new Vector3(-100f, 0f, 0f); b.e[HandL] = new Vector3(0f, 55f, 0f);
            }
            Apply(b);
        }

        /// <summary>Walk / run cycle speed (1 = normal).</summary>
        public void SetSpeed(float s) => curSpeed = Mathf.Clamp(s, 0.3f, 2f);

        void Update()
        {
            if (h == null || joints == null) return;
            float dt = Time.deltaTime;
            curT += dt * (cur == "walk" || cur == "sprint" || cur == "crouch-walk" ? curSpeed : 1f);
            prevT += dt * (prev == "walk" || prev == "sprint" || prev == "crouch-walk" ? prevSpeed : 1f);
            fadeT = Mathf.Min(1f, fadeT + dt / fade);
            Evaluate(cur, curT, curSpeed, b);
            if (fadeT < 1f)
            {
                Evaluate(prev, prevT, prevSpeed, a);
                float w = Mathf.SmoothStep(0f, 1f, fadeT);
                for (int i = 0; i < J; i++) o.e[i] = Vector3.Lerp(a.e[i], b.e[i], w);
                o.hipsOffset = Vector3.Lerp(a.hipsOffset, b.hipsOffset, w);
                o.flip = Mathf.Lerp(a.flip, b.flip, w);
            }
            else Copy(b, o);
            // Holding a phone up (not while doing something with both hands).
            bool canHold = HoldingPhone && cur != "pick-up" && cur != "interact-right" && cur != "highjump" && cur != "wave" && cur != "dance";
            phoneW = Mathf.MoveTowards(phoneW, canHold ? 1f : 0f, dt * 4f);
            if (phoneW > 0f)
            {
                o.e[UArmL] = Vector3.Lerp(o.e[UArmL], new Vector3(-28f, 0f, -12f), phoneW);
                o.e[FArmL] = Vector3.Lerp(o.e[FArmL], new Vector3(-100f, 0f, 0f), phoneW);
                o.e[HandL] = Vector3.Lerp(o.e[HandL], new Vector3(0f, 55f, 0f), phoneW);
            }
            Apply(o);
            HandsAndEyes(dt);
        }

        static void Copy(Pose from, Pose to)
        {
            for (int i = 0; i < J; i++) to.e[i] = from.e[i];
            to.hipsOffset = from.hipsOffset; to.flip = from.flip;
        }

        void Apply(Pose p)
        {
            for (int i = 0; i < J; i++)
            {
                var t = joints[i];
                if (t == null) continue;
                t.localRotation = Quaternion.Euler(p.e[i]);
            }
            if (joints[Hips] != null) joints[Hips].localPosition = restPos[Hips] + p.hipsOffset;
            // The flip turns the whole body round its middle.
            var rig = h.rig;
            if (rig != null)
            {
                var q = Quaternion.Euler(p.flip, 0f, 0f);
                var pivot = new Vector3(0f, 0.95f, 0f) * rig.localScale.x;
                rig.localRotation = q;
                rig.localPosition = pivot - q * pivot;
            }
        }

        // =====================================================================
        // The poses
        // =====================================================================

        static float S(float x) => Mathf.Sin(x);
        static float C(float x) => Mathf.Cos(x);
        static float Pos(float x) => Mathf.Max(0f, x);

        void Base(Pose p, float t)
        {
            p.Clear();
            p.e[UArmL] = new Vector3(0f, 0f, -7f);
            p.e[UArmR] = new Vector3(0f, 0f, 7f);
            p.e[FArmL] = new Vector3(-10f, 0f, 0f);
            p.e[FArmR] = new Vector3(-10f, 0f, 0f);
            p.e[HandL] = new Vector3(0f, 0f, 0f);
            p.e[HandR] = new Vector3(0f, 0f, 0f);
        }

        void Evaluate(string state, float t, float speed, Pose p)
        {
            Base(p, t);
            switch (state)
            {
                case "idle": Idle(p, t); break;
                case "walk": Walk(p, t, 1.75f, 26f, 48f, 20f, 18f, 4f, 0.016f); break;
                case "sprint": Walk(p, t, 2.6f, 48f, 95f, 50f, 88f, 15f, 0.032f); break;
                case "jump": Jump(p, t); break;
                case "fall": Fall(p, t); break;
                case "crouch": Crouch(p, t, false); break;
                case "crouch-walk": Crouch(p, t, true); break;
                case "highjump": HighJump(p, t); break;
                case "pick-up": PickUp(p, t, false); break;
                case "interact-right": PickUp(p, t, true); break;
                case "wave": Wave(p, t); break;
                case "dance": Dance(p, t); break;
                case "sit": Sit(p, t, 0); break;
                case "drive": Sit(p, t, 1); break;
                case "type": Sit(p, t, 2); break;
                default: Idle(p, t); break;
            }
        }

        /// <summary>Seated (GAD 2-2 X): mode 0 hands on the knees, 1 hands on a steering wheel, 2 typing at a desk.</summary>
        static void Sit(Pose p, float t, int mode)
        {
            p.e[Hips] = new Vector3(0f, 0f, 0f);
            p.e[Torso] = new Vector3(mode == 1 ? -8f : 4f + S(t * 1.5f) * 1.5f, 0f, 0f);
            p.e[ThighL] = new Vector3(-88f, 0f, -4f);
            p.e[ThighR] = new Vector3(-88f, 0f, 4f);
            p.e[ShinL] = new Vector3(mode == 1 ? 70f : 88f, 0f, 0f);
            p.e[ShinR] = new Vector3(mode == 1 ? 62f : 88f, 0f, 0f);
            p.e[FootL] = new Vector3(mode == 1 ? -10f : 0f, 0f, 0f);
            p.e[FootR] = new Vector3(mode == 1 ? -20f : 0f, 0f, 0f);
            if (mode == 1)
            {
                p.e[UArmL] = new Vector3(-55f, 0f, -14f); p.e[UArmR] = new Vector3(-55f, 0f, 14f);
                p.e[FArmL] = new Vector3(-35f, 0f, 0f); p.e[FArmR] = new Vector3(-35f, 0f, 0f);
            }
            else if (mode == 2)
            {
                float k = S(t * 9f) * 4f;
                p.e[UArmL] = new Vector3(-30f, 0f, -8f); p.e[UArmR] = new Vector3(-30f, 0f, 8f);
                p.e[FArmL] = new Vector3(-62f + k, 0f, 0f); p.e[FArmR] = new Vector3(-62f - k, 0f, 0f);
            }
            else
            {
                p.e[UArmL] = new Vector3(-18f, 0f, -6f); p.e[UArmR] = new Vector3(-18f, 0f, 6f);
                p.e[FArmL] = new Vector3(-55f, 0f, 0f); p.e[FArmR] = new Vector3(-55f, 0f, 0f);
            }
            p.hipsOffset = new Vector3(0f, -0.46f, -0.05f);
        }

        static void Idle(Pose p, float t)
        {
            float br = S(t * 1.6f);
            p.e[Torso] = new Vector3(1.5f + br * 0.9f, 0f, 0f);
            p.e[Neck] = new Vector3(-1f, 0f, 0f);
            p.e[Head] = new Vector3(2f * S(t * 0.5f), 12f * S(t * 0.31f) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 0.5f)), 0f);
            p.e[UArmL].z -= 0.8f * br; p.e[UArmR].z += 0.8f * br;
            p.e[Hips] = new Vector3(0f, 0f, 1.2f * S(t * 0.4f));
            p.e[ThighL] = new Vector3(0f, 0f, -1.2f * S(t * 0.4f));
            p.e[ThighR] = new Vector3(0f, 0f, -1.2f * S(t * 0.4f));
            p.e[ShinL] = new Vector3(2f, 0f, 0f); p.e[ShinR] = new Vector3(2f, 0f, 0f);
        }

        static void Walk(Pose p, float t, float hz, float legAmp, float knee, float armAmp, float elbow, float lean, float bob)
        {
            float ph = t * hz * Mathf.PI * 2f;
            float s = S(ph), c = C(ph);
            p.e[ThighR] = new Vector3(-legAmp * s, 0f, 0f);
            p.e[ThighL] = new Vector3(legAmp * s, 0f, 0f);
            p.e[ShinR] = new Vector3(6f + knee * Pos(c), 0f, 0f);
            p.e[ShinL] = new Vector3(6f + knee * Pos(-c), 0f, 0f);
            p.e[FootR] = new Vector3(-legAmp * 0.35f * s - knee * 0.15f * Pos(c), 0f, 0f);
            p.e[FootL] = new Vector3(legAmp * 0.35f * s - knee * 0.15f * Pos(-c), 0f, 0f);
            p.e[UArmR] = new Vector3(armAmp * s, 0f, 8f);
            p.e[UArmL] = new Vector3(-armAmp * s, 0f, -8f);
            p.e[FArmR] = new Vector3(-elbow - armAmp * 0.3f * Pos(-s), 0f, 0f);
            p.e[FArmL] = new Vector3(-elbow - armAmp * 0.3f * Pos(s), 0f, 0f);
            p.e[Hips] = new Vector3(lean * 0.3f, 6f * s, 2f * c);
            p.e[Torso] = new Vector3(lean, -9f * s, -2f * c);
            p.e[Head] = new Vector3(-lean * 0.6f, 3f * s, 0f);
            p.hipsOffset = new Vector3(0f, -bob + bob * C(ph * 2f), 0f);
        }

        static void Jump(Pose p, float t)
        {
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.18f));
            p.e[ThighL] = new Vector3(-45f * k, 0f, -4f);
            p.e[ThighR] = new Vector3(-20f * k, 0f, 4f);
            p.e[ShinL] = new Vector3(75f * k, 0f, 0f);
            p.e[ShinR] = new Vector3(40f * k, 0f, 0f);
            p.e[FootL] = new Vector3(15f * k, 0f, 0f);
            p.e[FootR] = new Vector3(15f * k, 0f, 0f);
            p.e[UArmL] = new Vector3(-140f * k, 0f, -18f);
            p.e[UArmR] = new Vector3(-140f * k, 0f, 18f);
            p.e[FArmL] = new Vector3(-20f, 0f, 0f);
            p.e[FArmR] = new Vector3(-20f, 0f, 0f);
            p.e[Torso] = new Vector3(-6f * k, 0f, 0f);
            p.e[Head] = new Vector3(-8f * k, 0f, 0f);
        }

        static void Fall(Pose p, float t)
        {
            float f = S(t * 9f);
            p.e[ThighL] = new Vector3(-25f, 0f, -6f);
            p.e[ThighR] = new Vector3(-8f, 0f, 6f);
            p.e[ShinL] = new Vector3(35f, 0f, 0f);
            p.e[ShinR] = new Vector3(18f, 0f, 0f);
            p.e[UArmL] = new Vector3(-20f, 0f, -78f - 10f * f);
            p.e[UArmR] = new Vector3(-20f, 0f, 78f + 10f * f);
            p.e[FArmL] = new Vector3(-15f, 0f, 0f);
            p.e[FArmR] = new Vector3(-15f, 0f, 0f);
            p.e[Torso] = new Vector3(6f, 0f, 0f);
            p.e[Head] = new Vector3(8f, 0f, 0f);
        }

        /// <summary>A squat: hips down and back, knees forward, the body leaning forward, arms out in front.</summary>
        static void Crouch(Pose p, float t, bool walking)
        {
            float d = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.18f));
            float w = walking ? S(t * 1.6f * Mathf.PI * 2f) : 0f;
            float thigh = -80f, shin = 110f;
            p.e[ThighL] = new Vector3((thigh + 14f * w) * d, 0f, -6f * d);
            p.e[ThighR] = new Vector3((thigh - 14f * w) * d, 0f, 6f * d);
            p.e[ShinL] = new Vector3((shin - 10f * w) * d, 0f, 0f);
            p.e[ShinR] = new Vector3((shin + 10f * w) * d, 0f, 0f);
            p.e[FootL] = new Vector3(-30f * d, 0f, 0f);
            p.e[FootR] = new Vector3(-30f * d, 0f, 0f);
            p.e[Torso] = new Vector3(32f * d, 0f, 0f);
            p.e[Hips] = new Vector3(4f * d, 0f, 0f);
            p.e[Neck] = new Vector3(-12f * d, 0f, 0f);
            p.e[Head] = new Vector3(-16f * d, 0f, 0f);
            p.e[UArmL] = new Vector3(-58f * d + 10f * w, 0f, -10f);
            p.e[UArmR] = new Vector3(-58f * d - 10f * w, 0f, 10f);
            p.e[FArmL] = new Vector3(-30f * d, 0f, 0f);
            p.e[FArmR] = new Vector3(-30f * d, 0f, 0f);
            p.hipsOffset = new Vector3(0f, -0.4f * d, -0.2f * d);
        }

        /// <summary>The high jump: a push off, a tuck and a full front flip, opening up to land.</summary>
        void HighJump(Pose p, float t)
        {
            float u = t / Mathf.Max(0.3f, HighJumpTime);
            float tuck = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.25f, u)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 0.95f, u)));
            float up = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.15f, u));
            p.e[ThighL] = new Vector3(-118f * tuck, 0f, -8f * tuck);
            p.e[ThighR] = new Vector3(-118f * tuck, 0f, 8f * tuck);
            p.e[ShinL] = new Vector3(135f * tuck + 5f, 0f, 0f);
            p.e[ShinR] = new Vector3(135f * tuck + 5f, 0f, 0f);
            p.e[FootL] = new Vector3(20f * tuck, 0f, 0f);
            p.e[FootR] = new Vector3(20f * tuck, 0f, 0f);
            p.e[Torso] = new Vector3(30f * tuck - 8f * up, 0f, 0f);
            p.e[Head] = new Vector3(22f * tuck, 0f, 0f);
            p.e[UArmL] = new Vector3(-160f * up - 75f * tuck, 0f, -14f);
            p.e[UArmR] = new Vector3(-160f * up - 75f * tuck, 0f, 14f);
            p.e[FArmL] = new Vector3(-80f * tuck - 10f, 0f, 0f);
            p.e[FArmR] = new Vector3(-80f * tuck - 10f, 0f, 0f);
            p.flip = 360f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.12f, 0.9f, u));
        }

        /// <summary>Collecting: bend down, scoop, straighten (with the fork: a stab with both hands).</summary>
        static void PickUp(Pose p, float t, bool fork)
        {
            float u = Mathf.Clamp01(t / 0.55f);
            float bend = S(u * Mathf.PI);
            p.e[Torso] = new Vector3(48f * bend, 0f, 0f);
            p.e[Hips] = new Vector3(14f * bend, 0f, 0f);
            p.e[Head] = new Vector3(-20f * bend, 0f, 0f);
            p.e[ThighL] = new Vector3(-28f * bend, 0f, -4f);
            p.e[ThighR] = new Vector3(-10f * bend, 0f, 4f);
            p.e[ShinL] = new Vector3(40f * bend, 0f, 0f);
            p.e[ShinR] = new Vector3(18f * bend, 0f, 0f);
            p.hipsOffset = new Vector3(0f, -0.08f * bend, -0.04f * bend);
            if (fork)
            {
                float stab = S(Mathf.Clamp01(u * 1.4f) * Mathf.PI);
                p.e[UArmR] = new Vector3(-30f - 45f * stab, 0f, 12f);
                p.e[FArmR] = new Vector3(-70f + 40f * stab, 0f, 0f);
                p.e[UArmL] = new Vector3(-50f - 30f * stab, 0f, -20f);
                p.e[FArmL] = new Vector3(-60f + 30f * stab, 0f, 0f);
            }
            else
            {
                p.e[UArmR] = new Vector3(-75f * bend, 0f, 10f);
                p.e[UArmL] = new Vector3(-75f * bend, 0f, -10f);
                p.e[FArmR] = new Vector3(-25f * bend - 10f, 0f, 0f);
                p.e[FArmL] = new Vector3(-25f * bend - 10f, 0f, 0f);
            }
        }

        static void Wave(Pose p, float t)
        {
            Idle(p, t);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.3f));
            p.e[UArmR] = new Vector3(-10f * k, 0f, 7f + 140f * k);
            p.e[FArmR] = new Vector3(-10f, 0f, (25f * S(t * 11f)) * k);
            p.e[Head] = new Vector3(0f, 10f * k, -6f * k);
        }

        static void Dance(Pose p, float t)
        {
            float bt = t * 2.2f * Mathf.PI;          // ~2 beats a second
            float s = S(bt), s2 = S(bt * 0.5f);
            p.hipsOffset = new Vector3(0.05f * s2, -0.04f + 0.04f * Mathf.Abs(C(bt * 0.5f)), 0f);
            p.e[Hips] = new Vector3(0f, 12f * s2, 8f * s2);
            p.e[Torso] = new Vector3(4f, -16f * s2, -6f * s2);
            p.e[Head] = new Vector3(8f * Mathf.Abs(s), 0f, 10f * s2);
            p.e[ThighL] = new Vector3(-22f * Pos(s2), 0f, -6f);
            p.e[ThighR] = new Vector3(-22f * Pos(-s2), 0f, 6f);
            p.e[ShinL] = new Vector3(40f * Pos(s2), 0f, 0f);
            p.e[ShinR] = new Vector3(40f * Pos(-s2), 0f, 0f);
            p.e[UArmL] = new Vector3(-90f - 50f * s, 0f, -25f);
            p.e[UArmR] = new Vector3(-90f + 50f * s, 0f, 25f);
            p.e[FArmL] = new Vector3(-70f, 0f, 0f);
            p.e[FArmR] = new Vector3(-70f, 0f, 0f);
        }
    }
}
