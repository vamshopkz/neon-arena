using System.Collections.Generic;
using UnityEngine;

namespace Arena.Real
{
    /// <summary>
    /// The bones of a <see cref="Human"/>: a jointed mannequin, every body part a smooth mesh riding on
    /// its own bone (like an artist's wooden doll), so it bends at the joints for the animations.
    /// The character faces +Z; +X is its right.
    /// </summary>
    public class HumanRig : MonoBehaviour
    {
        public Transform rig, hips, torso, neck, head;
        public Transform uArmL, uArmR, fArmL, fArmR, handL, handR;
        public Transform thighL, thighR, shinL, shinR, footL, footR;
        public bool female;
        /// <summary>Metres of the finished character per unit of the reference 1.75 m body.</summary>
        public float scale = 1f;
        public Human.Shape shape;
        public Transform[] Joints => new[] { hips, torso, neck, head, uArmL, uArmR, fArmL, fArmR, handL, handR, thighL, thighR, shinL, shinR, footL, footR };
        /// <summary>2.17: finger bones (thumb, index, middle, ring, little; three joints each) of each hand.</summary>
        public Transform[][] fingersL, fingersR;
        /// <summary>How closed each hand is: 0 open … 1 a fist round a handle.</summary>
        public float curlL = 0.2f, curlR = 0.2f;
        /// <summary>Upper eyelids (they blink).</summary>
        public readonly List<Transform> lids = new List<Transform>();
    }

    /// <summary>
    /// 2.15: the realistic human the player now is — a smooth white mannequin (male or female) built
    /// from code, with a face, hair, clothes and shoes put on it from <see cref="HumanStyle"/>. All
    /// sizes are for a 1.75 m body and scaled to the height asked for.
    /// </summary>
    public static partial class Human
    {
        public const float RefHeight = 1.75f;

        /// <summary>Body proportions (male or female).</summary>
        public class Shape
        {
            public bool female;
            public float shoulder = 1f, chest = 1f, waist = 1f, hip = 1f, limb = 1f, headSize = 1f, bust;
            public Profile pelvis, torso, neck, head, uArm, fArm, hand, thigh, shin, foot;
            public float headTop, eyeY = 0.105f;
            public int face;
        }

        // =====================================================================
        // Profiles and lofts: a smooth tube along local Y, its cross-section an ellipse
        // =====================================================================

        public struct Key
        {
            public float y, rx, rz, cz, cx;
            public Key(float y, float rx, float rz, float cz = 0f, float cx = 0f) { this.y = y; this.rx = rx; this.rz = rz; this.cz = cz; this.cx = cx; }
        }

        /// <summary>A smooth (Catmull-Rom) radius profile along Y through the keys.</summary>
        public class Profile
        {
            public readonly Key[] keys;
            public Profile(params Key[] k) { keys = (Key[])k.Clone(); System.Array.Sort(keys, (a, b) => a.y.CompareTo(b.y)); }
            public float Y0 => keys[0].y;
            public float Y1 => keys[keys.Length - 1].y;

            public Key Eval(float y)
            {
                int n = keys.Length;
                if (y <= keys[0].y) return keys[0];
                if (y >= keys[n - 1].y) return keys[n - 1];
                int i = 0;
                while (i < n - 2 && y > keys[i + 1].y) i++;
                var a = keys[Mathf.Max(0, i - 1)]; var b = keys[i]; var c = keys[i + 1]; var d = keys[Mathf.Min(n - 1, i + 2)];
                float t = (y - b.y) / Mathf.Max(1e-5f, c.y - b.y);
                return new Key(y, CR(a.rx, b.rx, c.rx, d.rx, t), CR(a.rz, b.rz, c.rz, d.rz, t), CR(a.cz, b.cz, c.cz, d.cz, t), CR(a.cx, b.cx, c.cx, d.cx, t));
            }

            static float CR(float p0, float p1, float p2, float p3, float t)
            {
                float t2 = t * t, t3 = t2 * t;
                float v = 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                return Mathf.Max(0f, v);
            }

            /// <summary>The surface point at angle a (radians; 90° = the front) and height y, pushed out by `grow`.</summary>
            public Vector3 Point(float y, float a, float grow = 0f)
            {
                var k = Eval(y);
                return new Vector3(k.cx + Mathf.Cos(a) * (k.rx + grow), y, k.cz + Mathf.Sin(a) * (k.rz + grow));
            }

            /// <summary>How far forward (z) the surface is at x on the slice at y.</summary>
            public float FrontZ(float y, float x)
            {
                var k = Eval(y);
                float u = Mathf.Clamp01(Mathf.Abs(x - k.cx) / Mathf.Max(1e-4f, k.rx));
                return k.cz + k.rz * Mathf.Sqrt(1f - u * u);
            }
        }

        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        /// <summary>
        /// A tube along the profile from y0 to y1, its cross-section grown by `grow` (clothes over the
        /// body), optionally closed with rounded ends, between angles a0..a1 (a full turn by default).
        /// UV: u around (0.25 = the front), v along.
        /// </summary>
        public static Mesh Loft(string key, Profile p, float y0, float y1, float grow, bool cap0, bool cap1, int rings = 18, int seg = 28, float a0 = 0f, float a1 = 360f, float squashBottom = -99f, System.Func<float, float, float> bump = null)
        {
            if (key != null && meshes.TryGetValue(key, out var cached)) return cached;
            bool full = Mathf.Approximately(a1 - a0, 360f);
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            int cols = seg + 1;
            for (int r = 0; r <= rings; r++)
            {
                float y = Mathf.Lerp(y0, y1, r / (float)rings);
                var k = p.Eval(y);
                for (int s = 0; s <= seg; s++)
                {
                    float ad = Mathf.Lerp(a0, a1, s / (float)seg);
                    float a = ad * Mathf.Deg2Rad;
                    float g = grow + (bump != null ? bump(ad % 360f, y) : 0f);
                    var v = new Vector3(k.cx + Mathf.Cos(a) * (k.rx + g), y, k.cz + Mathf.Sin(a) * (k.rz + g));
                    if (v.z < squashBottom) v.z = squashBottom;
                    verts.Add(v);
                    uvs.Add(new Vector2(Mathf.Lerp(a0, a1, s / (float)seg) / 360f, (y - y0) / Mathf.Max(1e-4f, y1 - y0)));
                }
            }
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < seg; s++)
                {
                    int i0 = r * cols + s, i1 = i0 + 1, i2 = i0 + cols, i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            void Cap(int ring, bool bottom)
            {
                var k = p.Eval(ring == 0 ? y0 : y1);
                float dome = Mathf.Min(k.rx, k.rz) * 0.55f + grow;
                int pole = verts.Count;
                verts.Add(new Vector3(k.cx, (ring == 0 ? y0 : y1) + (bottom ? -dome : dome), k.cz));
                uvs.Add(new Vector2(0.25f, bottom ? 0f : 1f));
                int start = ring * cols;
                for (int s = 0; s < seg; s++)
                {
                    if (bottom) { tris.Add(start + s); tris.Add(start + s + 1); tris.Add(pole); }
                    else { tris.Add(start + s + 1); tris.Add(start + s); tris.Add(pole); }
                }
            }
            if (cap0) Cap(0, true);
            if (cap1) Cap(rings, false);
            var m = new Mesh { name = key ?? "loft" };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            if (full)
            {
                // One smooth surface across the seam.
                var n = m.normals;
                for (int r = 0; r <= rings; r++)
                {
                    int a = r * cols, b = a + seg;
                    var avg = (n[a] + n[b]).normalized;
                    n[a] = avg; n[b] = avg;
                }
                m.normals = n;
            }
            m.RecalculateBounds();
            m.RecalculateTangents();
            if (key != null) meshes[key] = m;
            return m;
        }

        /// <summary>The same tube turned to run along +Z instead of +Y (feet, shoes).</summary>
        public static Mesh AlongZ(string key, Mesh src, float yShift = 0f)
        {
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var m = Object.Instantiate(src);
            m.name = key;
            var v = m.vertices;
            for (int i = 0; i < v.Length; i++) v[i] = new Vector3(v[i].x, -v[i].z + yShift, v[i].y);
            m.vertices = v;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();
            meshes[key] = m;
            return m;
        }

        public static GameObject Part(string name, Mesh mesh, Material mat, Transform parent, Vector3 pos = default, Quaternion? rot = null, Vector3? scale = null)
        {
            var go = MeshUtil.MeshObject(name, mesh, mat, parent, 0);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = scale ?? Vector3.one;
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        // =====================================================================
        // The body
        // =====================================================================

        /// <summary>Face shapes: (jaw width, cheekbone width, chin-to-eyes length, chin flatness).</summary>
        static readonly (float jaw, float cheek, float len, float chin)[] Faces =
        {
            (1f, 1f, 1f, 1f), (1.16f, 1.1f, 0.88f, 1.3f), (1.28f, 1.03f, 1f, 1.8f), (0.88f, 0.92f, 1.2f, 0.9f), (0.78f, 1.16f, 1.04f, 0.65f), (1.18f, 1.22f, 0.95f, 1.2f),
        };

        public static Shape MakeShape(bool female, int face = 0)
        {
            var s = new Shape { female = female, face = Mathf.Clamp(face, 0, Faces.Length - 1) };
            if (female) { s.shoulder = 0.87f; s.chest = 0.93f; s.waist = 0.84f; s.hip = 1.1f; s.limb = 0.9f; s.headSize = 0.96f; s.bust = 1f; }
            float sh = s.shoulder, ch = s.chest, wa = s.waist, hp = s.hip, lm = s.limb;
            // Pelvis (hips bone at 0.98 m): from the crotch up to the waist.
            s.pelvis = new Profile(
                new Key(-0.15f, 0.05f, 0.05f, 0.005f),
                new Key(-0.12f, 0.12f * hp, 0.085f, 0.005f),
                new Key(-0.06f, 0.16f * hp, 0.105f, 0.0f),
                new Key(0.01f, 0.158f * hp, 0.1f, 0.0f),
                new Key(0.07f, 0.145f * Mathf.Lerp(1f, hp, 0.6f) * Mathf.Lerp(1f, wa, 0.5f), 0.095f),
                new Key(0.11f, 0.13f * wa, 0.088f));
            // Torso (torso bone 8 cm above the hips): waist, ribs, chest, shoulders.
            float bust = s.bust;
            s.torso = new Profile(
                new Key(-0.06f, 0.135f * wa, 0.092f),
                new Key(0.02f, 0.13f * wa, 0.09f),
                new Key(0.1f, 0.142f * Mathf.Lerp(1f, ch, 0.5f), 0.098f, 0.004f),
                new Key(0.18f, 0.158f * ch, 0.108f + 0.02f * bust, 0.008f + 0.012f * bust),
                new Key(0.25f, 0.168f * Mathf.Lerp(ch, sh, 0.5f), 0.11f + 0.018f * bust, 0.006f + 0.012f * bust),
                new Key(0.31f, 0.178f * sh, 0.1f, 0.0f),
                new Key(0.355f, 0.168f * sh, 0.085f, -0.005f),
                new Key(0.39f, 0.12f * sh, 0.07f, -0.008f),
                new Key(0.415f, 0.06f, 0.05f, -0.005f));
            s.neck = new Profile(new Key(-0.02f, 0.052f, 0.05f), new Key(0.04f, 0.047f * Mathf.Lerp(1f, sh, 0.6f), 0.046f, 0.003f), new Key(0.1f, 0.048f, 0.048f, 0.008f));
            // Head (head bone at the top of the neck): chin, jaw, cheeks, the back of the skull, the crown.
            float hs = s.headSize;
            s.head = new Profile(
                new Key(-0.025f * hs, 0.02f * hs, 0.02f * hs, 0.05f * hs),
                new Key(-0.01f * hs, 0.042f * hs, 0.04f * hs, 0.042f * hs),
                new Key(0.02f * hs, 0.06f * hs, 0.065f * hs, 0.028f * hs),
                new Key(0.06f * hs, 0.071f * hs, 0.088f * hs, 0.012f * hs),
                new Key(0.105f * hs, 0.078f * hs, 0.1f * hs, 0.002f * hs),
                new Key(0.15f * hs, 0.08f * hs, 0.1f * hs, -0.006f * hs),
                new Key(0.19f * hs, 0.072f * hs, 0.088f * hs, -0.01f * hs),
                new Key(0.21f * hs, 0.06f * hs, 0.073f * hs, -0.01f * hs),
                new Key(0.224f * hs, 0.047f * hs, 0.057f * hs, -0.009f * hs),
                new Key(0.233f * hs, 0.028f * hs, 0.034f * hs, -0.008f * hs),
                new Key(0.237f * hs, 0.006f * hs, 0.007f * hs, -0.008f * hs));
            var fs = Faces[s.face];
            if (s.face != 0)
            {
                float ey0 = 0.105f * hs;
                for (int i = 0; i < s.head.keys.Length; i++)
                {
                    var k = s.head.keys[i];
                    float y = k.y;
                    float w = y < ey0 ? Mathf.Lerp(fs.jaw, fs.cheek, Mathf.InverseLerp(-0.02f * hs, 0.08f * hs, y)) : Mathf.Lerp(fs.cheek, 1f, Mathf.InverseLerp(0.12f * hs, 0.2f * hs, y));
                    if (i == 0) { w = fs.chin * Mathf.Lerp(1f, fs.jaw, 0.5f); }
                    if (y < ey0) y = ey0 - (ey0 - y) * fs.len;
                    s.head.keys[i] = new Key(y, k.rx * w, k.rz * Mathf.Lerp(1f, w, 0.35f), k.cz);
                }
            }
            s.headTop = 0.235f * hs;
            s.eyeY = 0.105f * hs;
            // Arms hang along −Y from the shoulder; hands are flat (thin in X, wide in Z).
            s.uArm = new Profile(new Key(0.066f, 0.005f, 0.005f), new Key(0.062f, 0.022f, 0.023f), new Key(0.052f, 0.035f * lm, 0.036f * lm), new Key(0.035f, 0.045f * lm, 0.047f * lm), new Key(0.012f, 0.05f * lm, 0.052f * lm), new Key(-0.04f, 0.052f * lm, 0.05f * lm), new Key(-0.16f, 0.044f * lm, 0.046f * lm), new Key(-0.27f, 0.036f * lm, 0.038f * lm), new Key(-0.3f, 0.033f * lm, 0.034f * lm));
            s.fArm = new Profile(new Key(0.02f, 0.034f * lm, 0.035f * lm), new Key(-0.05f, 0.04f * lm, 0.038f * lm), new Key(-0.14f, 0.034f * lm, 0.03f * lm), new Key(-0.23f, 0.026f * lm, 0.021f * lm), new Key(-0.26f, 0.025f * lm, 0.02f * lm));
            s.hand = new Profile(new Key(0.01f, 0.018f * lm, 0.026f * lm), new Key(-0.03f, 0.021f * lm, 0.04f * lm), new Key(-0.08f, 0.019f * lm, 0.044f * lm), new Key(-0.13f, 0.014f * lm, 0.038f * lm), new Key(-0.17f, 0.009f * lm, 0.022f * lm));
            // Legs along −Y from the hip joint; the calf bulges at the back.
            s.thigh = new Profile(new Key(0.05f, 0.05f, 0.05f), new Key(0.0f, 0.088f * hp, 0.088f), new Key(-0.08f, 0.085f * Mathf.Lerp(1f, hp, 0.5f), 0.084f, 0.004f), new Key(-0.28f, 0.065f, 0.066f, 0.006f), new Key(-0.41f, 0.05f, 0.052f, 0.004f), new Key(-0.445f, 0.046f, 0.048f));
            s.shin = new Profile(new Key(0.02f, 0.05f, 0.052f, 0.004f), new Key(-0.06f, 0.05f, 0.058f, -0.012f), new Key(-0.14f, 0.048f, 0.058f, -0.014f), new Key(-0.27f, 0.037f, 0.04f, -0.004f), new Key(-0.37f, 0.03f, 0.031f), new Key(-0.41f, 0.032f, 0.032f));
            // Foot along +Z (made along Y, then turned): heel at the back, toes in front.
            s.foot = new Profile(new Key(-0.06f, 0.03f, 0.03f), new Key(-0.035f, 0.038f, 0.04f), new Key(0.02f, 0.044f, 0.042f), new Key(0.09f, 0.048f, 0.033f), new Key(0.15f, 0.047f, 0.024f), new Key(0.195f, 0.03f, 0.016f));
            return s;
        }

        static string ShapeKey(Shape s) => s.female ? "f" : "m";
        static string HeadKey(Shape s) => ShapeKey(s) + s.face;

        // ---------------- anatomy: small bumps and hollows on the smooth parts ----------------

        static float G(float a, float a0, float aw, float y, float y0, float yw, float amp)
        {
            float da = Mathf.DeltaAngle(a, a0) / aw, dy = (y - y0) / yw;
            return amp * Mathf.Exp(-(da * da + dy * dy));
        }

        /// <summary>The face and skull: brow ridge, eye sockets, cheekbones, jaw, chin, the back of the head.</summary>
        public static System.Func<float, float, float> HeadBump(Shape s)
        {
            float hs = s.headSize, ey = s.eyeY;
            return (a, y) =>
                G(a, 90f, 32f, y, ey + 0.025f * hs, 0.011f * hs, 0.0045f)
                + G(a, 67f, 8f, y, ey, 0.011f * hs, -0.0035f) + G(a, 113f, 8f, y, ey, 0.011f * hs, -0.0035f)
                + G(a, 48f, 13f, y, ey - 0.022f * hs, 0.015f * hs, 0.0045f) + G(a, 132f, 13f, y, ey - 0.022f * hs, 0.015f * hs, 0.0045f)
                + G(a, 45f, 15f, y, ey - 0.05f * hs, 0.014f * hs, -0.0025f) + G(a, 135f, 15f, y, ey - 0.05f * hs, 0.014f * hs, -0.0025f)
                + G(a, 30f, 12f, y, 0.03f * hs, 0.02f * hs, 0.0035f) + G(a, 150f, 12f, y, 0.03f * hs, 0.02f * hs, 0.0035f)
                + G(a, 90f, 16f, y, -0.006f * hs, 0.013f * hs, 0.0035f)
                + G(a, 22f, 12f, y, ey + 0.03f * hs, 0.02f * hs, -0.0025f) + G(a, 158f, 12f, y, ey + 0.03f * hs, 0.02f * hs, -0.0025f)
                + G(a, 270f, 50f, y, 0.15f * hs, 0.05f * hs, 0.002f);
        }

        /// <summary>Chest, collarbones, shoulder blades and the groove of the spine.</summary>
        static System.Func<float, float, float> TorsoBump(Shape s)
        {
            float pec = s.female ? 0.002f : 0.006f;
            return (a, y) =>
                G(a, 66f, 20f, y, 0.24f, 0.04f, pec) + G(a, 114f, 20f, y, 0.24f, 0.04f, pec)
                + G(a, 60f, 16f, y, 0.355f, 0.008f, 0.0028f) + G(a, 120f, 16f, y, 0.355f, 0.008f, 0.0028f)
                + G(a, 240f, 18f, y, 0.26f, 0.05f, 0.0038f) + G(a, 300f, 18f, y, 0.26f, 0.05f, 0.0038f)
                + G(a, 270f, 5f, y, 0.15f, 0.2f, -0.0035f)
                + G(a, 90f, 12f, y, 0.06f, 0.08f, s.female ? 0f : 0.0015f) - G(a, 90f, 3f, y, 0.06f, 0.1f, s.female ? 0f : 0.0015f);
        }

        static float ThighBump(float a, float y) => G(a, 90f, 45f, y, -0.16f, 0.11f, 0.004f) + G(a, 270f, 40f, y, -0.12f, 0.1f, 0.0025f);
        static float ShinBump(float a, float y) => G(a, 270f, 45f, y, -0.09f, 0.06f, 0.0045f) + G(a, 0f, 12f, y, -0.395f, 0.012f, 0.004f) + G(a, 180f, 12f, y, -0.395f, 0.012f, 0.004f);
        static float UArmBump(float a, float y) => G(a, 90f, 45f, y, -0.13f, 0.06f, 0.003f) + G(a, 270f, 45f, y, -0.11f, 0.07f, 0.002f);
        static float FArmBump(float a, float y) => G(a, 90f, 50f, y, -0.06f, 0.05f, 0.0025f);

        /// <summary>The joints of the rest pose (in the rig's 1.75 m space).</summary>
        public static class Bones
        {
            public static readonly Vector3 Hips = new Vector3(0f, 0.98f, 0f);
            public static readonly Vector3 TorsoFromHips = new Vector3(0f, 0.08f, 0f);
            public static readonly Vector3 NeckFromTorso = new Vector3(0f, 0.395f, -0.006f);
            public static readonly Vector3 HeadFromNeck = new Vector3(0f, 0.085f, 0.006f);
            public static Vector3 ShoulderFromTorso(Shape s, float side) => new Vector3(side * 0.19f * s.shoulder, 0.335f, -0.006f);
            public static readonly Vector3 ElbowFromShoulder = new Vector3(0f, -0.295f, 0f);
            public static readonly Vector3 WristFromElbow = new Vector3(0f, -0.255f, 0f);
            public static Vector3 HipFromHips(Shape s, float side) => new Vector3(side * 0.088f * Mathf.Lerp(1f, s.hip, 0.7f), -0.07f, 0f);
            public static readonly Vector3 KneeFromHip = new Vector3(0f, -0.435f, 0f);
            public static readonly Vector3 AnkleFromKnee = new Vector3(0f, -0.405f, 0f);
        }

        static Transform Bone(string name, Transform parent, Vector3 local)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = local;
            return t;
        }

        /// <summary>Material of the bare body: the white mannequin or a skin tone.</summary>
        public static Material SkinMat(Color c, bool mannequin) =>
            Mats.Shared("human_skin_" + ColorUtility.ToHtmlStringRGB(c) + (mannequin ? "m" : ""), () => Mats.Lit(c, null, mannequin ? 0.42f : 0.28f));

        static readonly Texture2D[] faceAlb = new Texture2D[2];
        static Texture2D faceNrm;

        /// <summary>
        /// Skin of the face: a white-based tint (multiplied by the skin tone) with blush on the cheeks,
        /// soft shadows round the eyes, a warmer nose and lips area, stubble on men, and fine pores.
        /// Laid out like the head loft: u = angle round the head (front = 0.25), v = height.
        /// </summary>
        public static Material FaceMat(Color skin, bool female)
        {
            return Mats.Shared("human_face_" + ColorUtility.ToHtmlStringRGB(skin) + (female ? "f" : "m"), () =>
            {
                int fi = female ? 1 : 0;
                const int S = 256;
                if (faceAlb[fi] == null)
                {
                    var px = new Color[S * S];
                    float ey = 0.105f;
                    for (int yy = 0; yy < S; yy++)
                        for (int xx = 0; xx < S; xx++)
                        {
                            float a = (xx + 0.5f) / S * 360f, y = Mathf.Lerp(-0.03f, 0.237f, (yy + 0.5f) / S);
                            float blush = G(a, 52f, 13f, y, ey - 0.032f, 0.018f, 1f) + G(a, 128f, 13f, y, ey - 0.032f, 0.018f, 1f);
                            float socket = G(a, 68f, 9f, y, ey + 0.004f, 0.012f, 1f) + G(a, 112f, 9f, y, ey + 0.004f, 0.012f, 1f);
                            float nose = G(a, 90f, 7f, y, ey - 0.03f, 0.014f, 1f);
                            float lips = G(a, 90f, 14f, y, 0.022f, 0.008f, 1f);
                            float n = Mathf.PerlinNoise(xx * 0.35f, yy * 0.35f) - 0.5f;
                            var c = Color.white * (1f + n * 0.025f);
                            c = Color.Lerp(c, new Color(1f, 0.78f, 0.76f), blush * (female ? 0.55f : 0.35f));
                            c = Color.Lerp(c, new Color(0.84f, 0.74f, 0.76f), socket * (female ? 0.55f : 0.45f));
                            c = Color.Lerp(c, new Color(1f, 0.86f, 0.84f), nose * 0.35f);
                            c = Color.Lerp(c, new Color(0.95f, 0.72f, 0.72f), lips * 0.5f);
                            if (!female)
                            {   // stubble: speckles on the jaw, chin and upper lip
                                float jaw = Mathf.Clamp01(1f - Mathf.Abs(Mathf.DeltaAngle(a, 90f)) / 70f) * Mathf.Clamp01((ey - 0.04f - y) / 0.02f) * Mathf.Clamp01((y + 0.025f) / 0.02f);
                                float cheekGap = blush * 1.2f;
                                float speck = Mathf.PerlinNoise(xx * 1.7f, yy * 1.7f) > 0.48f ? 1f : 0.45f;
                                c = Color.Lerp(c, new Color(0.78f, 0.74f, 0.74f), Mathf.Clamp01(jaw - cheekGap) * 0.35f * (0.7f + 0.3f * speck));
                            }
                            c.a = 1f;
                            px[yy * S + xx] = c;
                        }
                    var tex = new Texture2D(S, S, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "face_" + fi };
                    tex.SetPixels(px);
                    tex.Apply(true);
                    faceAlb[fi] = tex;
                }
                if (faceNrm == null)
                {
                    var h = new float[S * S];
                    for (int yy = 0; yy < S; yy++)
                        for (int xx = 0; xx < S; xx++)
                            h[yy * S + xx] = ProcTex.PeriodicNoise(xx, yy, S, 128, 3.1f) * 0.5f + ProcTex.PeriodicNoise(xx, yy, S, 32, 7.7f) * 0.5f;
                    faceNrm = ProcTex.NormalFromHeight(h, S, 1.2f, "face_pores");
                }
                return Mats.LitNormal(skin, faceAlb[fi], faceNrm, 0.12f, 0.3f);
            });
        }

        /// <summary>A bare body with all its bones (no face, hair or clothes yet).</summary>
        public static HumanRig BuildBody(Transform holder, bool female, Color skin, bool mannequin, float height, int face = 0)
        {
            var s = MakeShape(female, face);
            string sk = ShapeKey(s);
            var mat = SkinMat(skin, mannequin);
            var rig = Bone("HumanRig", holder, Vector3.zero);
            float scale = height / (RefHeight * (female ? 0.95f : 1f));
            rig.localScale = Vector3.one * scale * (female ? 0.95f : 1f);
            var h = holder.gameObject.AddComponent<HumanRig>();
            h.rig = rig; h.female = female; h.shape = s; h.scale = rig.localScale.x;

            h.hips = Bone("hips", rig, Bones.Hips);
            Part("body_pelvis", Loft("pelvis2_" + sk, s.pelvis, s.pelvis.Y0, s.pelvis.Y1, 0f, true, true, 22, 40), mat, h.hips);
            h.torso = Bone("torso", h.hips, Bones.TorsoFromHips);
            Part("body_torso", Loft("torso2_" + sk, s.torso, s.torso.Y0, s.torso.Y1, 0f, true, true, 40, 56, bump: TorsoBump(s)), mat, h.torso);
            // Collarbones and shoulder caps round the top of the chest off.
            foreach (var side in new[] { -1f, 1f })
                Part("body_shoulder", Loft("deltoid_" + sk, s.uArm, -0.06f, s.uArm.Y1, 0.004f, true, true, 8), mat, h.torso, Bones.ShoulderFromTorso(s, side));
            h.neck = Bone("neck", h.torso, Bones.NeckFromTorso);
            Part("body_neck", Loft("neck2_" + sk, s.neck, s.neck.Y0, s.neck.Y1, 0f, true, true, 10, 32, bump: (a, y) => G(a, 70f, 10f, y, 0.04f, 0.04f, 0.0025f) + G(a, 110f, 10f, y, 0.04f, 0.04f, 0.0025f) + (s.female ? 0f : G(a, 90f, 8f, y, 0.05f, 0.012f, 0.004f))), mat, h.neck);
            h.head = Bone("head", h.neck, Bones.HeadFromNeck);
            BuildHead(h, mannequin ? mat : FaceMat(skin, female));

            foreach (var side in new[] { -1f, 1f })
            {
                bool right = side > 0f;
                var ua = Bone(right ? "arm-right" : "arm-left", h.torso, Bones.ShoulderFromTorso(s, side));
                Part("body_uarm", Loft("uarm2_" + sk, s.uArm, s.uArm.Y0 - 0.012f, s.uArm.Y1, 0f, true, true, 24, 32, bump: UArmBump), mat, ua);
                var fa = Bone(right ? "forearm-right" : "forearm-left", ua, Bones.ElbowFromShoulder);
                Part("body_farm", Loft("farm2_" + sk, s.fArm, s.fArm.Y0, s.fArm.Y1, 0f, true, true, 20, 32, bump: FArmBump), mat, fa);
                var hd = Bone(right ? "hand-right" : "hand-left", fa, Bones.WristFromElbow);
                var fingers = BuildHand(hd, s, mat, side);
                if (right) h.fingersR = fingers; else h.fingersL = fingers;
                var th = Bone(right ? "leg-right" : "leg-left", h.hips, Bones.HipFromHips(s, side));
                Part("body_thigh", Loft("thigh2_" + sk, s.thigh, s.thigh.Y0, s.thigh.Y1, 0f, true, true, 28, 36, bump: ThighBump), mat, th);
                var sh = Bone(right ? "shin-right" : "shin-left", th, Bones.KneeFromHip);
                Part("body_shin", Loft("shin2_" + sk, s.shin, s.shin.Y0, s.shin.Y1, 0f, true, true, 28, 36, bump: ShinBump), mat, sh);
                // A small knee cap.
                Part("body_knee", Loft("knee", new Profile(new Key(0.03f, 0.02f, 0.012f), new Key(0f, 0.034f, 0.02f), new Key(-0.035f, 0.02f, 0.012f)), -0.035f, 0.03f, 0f, true, true, 6, 16), mat, sh, new Vector3(0f, 0.005f, 0.04f));
                var ft = Bone(right ? "foot-right" : "foot-left", sh, Bones.AnkleFromKnee);
                Part("body_foot", FootMesh(s, 0f), mat, ft);
                // Toes: five little rounded bumps along the front of the foot.
                var toe = Loft("toe", new Profile(new Key(-0.012f, 0.006f, 0.006f), new Key(0f, 0.008f, 0.0075f), new Key(0.012f, 0.0065f, 0.006f), new Key(0.017f, 0.002f, 0.002f)), -0.012f, 0.017f, 0f, true, true, 6, 12);
                for (int i = 0; i < 5; i++)
                {
                    float tx = -side * (-0.03f + i * 0.0145f) * (i == 0 ? 1.05f : 1f);
                    float tz = 0.19f - Mathf.Abs(i - 1.2f) * 0.009f - (i == 0 ? 0.004f : 0f);
                    Part("body_toe", toe, mat, ft, new Vector3(tx, -0.062f, tz), Quaternion.Euler(90f, 0f, 0f), Vector3.one * (i == 0 ? 1.35f : 1f - i * 0.05f));
                }
                if (right) { h.uArmR = ua; h.fArmR = fa; h.handR = hd; h.thighR = th; h.shinR = sh; h.footR = ft; }
                else { h.uArmL = ua; h.fArmL = fa; h.handL = hd; h.thighL = th; h.shinL = sh; h.footL = ft; }
            }
            // A relaxed stance: arms a little away from the body.
            HumanAnimator.ApplyRest(h);
            return h;
        }

        /// <summary>The foot (or a shoe over it, grown by `grow`): heel at the ankle's back, flat sole on the floor.</summary>
        public static Mesh FootMesh(Shape s, float grow, string tag = "")
        {
            string key = "foot_" + ShapeKey(s) + "_" + grow.ToString("0.000") + tag;
            if (meshes.TryGetValue(key, out var m)) return m;
            var along = Loft(key + "_y", s.foot, s.foot.Y0 - grow * 0.6f, s.foot.Y1 + grow, grow, true, true, 14, 24, 0f, 360f);
            // Turned to run along +Z, lifted so the sole sits ~7 cm under the ankle, the bottom flattened.
            m = Object.Instantiate(along);
            m.name = key;
            var v = m.vertices;
            float floorY = -0.072f - grow * 0.4f;
            for (int i = 0; i < v.Length; i++)
            {
                var p = new Vector3(v[i].x, -v[i].z - 0.035f, v[i].y + 0.012f);
                if (p.y < floorY) p.y = floorY;
                v[i] = p;
            }
            m.vertices = v;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();
            meshes[key] = m;
            return m;
        }

        static void BuildHead(HumanRig h, Material mat)
        {
            var s = h.shape;
            string sk = ShapeKey(s);
            Part("body_head", Loft("head2_" + HeadKey(s), s.head, s.head.Y0, s.head.Y1, 0f, true, true, 44, 64, bump: HeadBump(s)), mat, h.head);
            float hs = s.headSize;
            // Nose: a soft wedge on the middle of the face.
            float ny = s.eyeY - 0.03f * hs;
            float nz = s.head.FrontZ(ny, 0f);
            Part("body_nose", Loft("nose2", new Profile(new Key(0.026f, 0.004f, 0.003f), new Key(0.016f, 0.0065f, 0.007f), new Key(0.0f, 0.009f, 0.0105f), new Key(-0.016f, 0.0125f, 0.0135f), new Key(-0.024f, 0.011f, 0.011f), new Key(-0.028f, 0.006f, 0.006f)), -0.028f, 0.026f, 0f, true, true, 12, 24),
                mat, h.head, new Vector3(0f, ny + 0.006f * hs, nz - 0.007f), Quaternion.Euler(-12f, 0f, 0f), Vector3.one * hs);
            // The wings of the nose and the nostrils under them.
            var dark = Mats.Shared("hum_nostril", () => Mats.Lit(new Color(0.32f, 0.16f, 0.13f), null, 0.2f));
            foreach (var side in new[] { -1f, 1f })
            {
                Part("body_nosewing", Loft("nosewing", new Profile(new Key(-0.006f, 0.004f, 0.004f), new Key(0f, 0.0065f, 0.006f), new Key(0.006f, 0.004f, 0.004f)), -0.006f, 0.006f, 0f, true, true, 6, 14), mat, h.head,
                    new Vector3(side * 0.0085f * hs, ny - 0.017f * hs, nz + 0.002f), Quaternion.Euler(0f, 0f, 90f), Vector3.one * hs * 0.8f);
                Part("body_nostril", Loft("nostril", new Profile(new Key(-0.003f, 0.0022f, 0.0035f), new Key(0f, 0.0028f, 0.004f), new Key(0.003f, 0.0022f, 0.0035f)), -0.003f, 0.003f, 0f, true, true, 4, 10), dark, h.head,
                    new Vector3(side * 0.0055f * hs, ny - 0.021f * hs, nz + 0.0065f), Quaternion.Euler(80f, 0f, 0f), Vector3.one * hs * 0.7f);
            }
            // Ears on the sides, level with the eyes.
            foreach (var side in new[] { -1f, 1f })
            {
                var k = s.head.Eval(s.eyeY - 0.005f);
                Part("body_ear", Loft("ear2", new Profile(new Key(0.03f, 0.005f, 0.012f), new Key(0.022f, 0.007f, 0.018f), new Key(0.0f, 0.0085f, 0.021f), new Key(-0.02f, 0.008f, 0.016f), new Key(-0.03f, 0.007f, 0.01f), new Key(-0.036f, 0.003f, 0.005f)), -0.036f, 0.03f, 0f, true, true, 12, 24,
                    bump: (a, y) => side * Mathf.Cos(a * Mathf.Deg2Rad) > 0.4f ? -0.003f * Mathf.Clamp01(1f - Mathf.Abs(y) / 0.025f) : 0f),
                    mat, h.head, new Vector3(side * (k.rx - 0.002f), s.eyeY - 0.005f, k.cz - 0.012f), Quaternion.Euler(0f, side * 12f, 0f), Vector3.one * hs);
            }
        }

        /// <summary>
        /// 2.17: a real hand — the palm with knuckles, four fingers of three joints and a thumb of two —
        /// on the wrist bone. Returns the finger joints (thumb first) so they can curl into a fist.
        /// </summary>
        public static Transform[][] BuildHand(Transform hand, Shape s, Material mat, float side)
        {
            float lm = s.limb;
            var palm = new Profile(new Key(0.012f, 0.016f * lm, 0.024f * lm), new Key(-0.01f, 0.019f * lm, 0.036f * lm), new Key(-0.05f, 0.017f * lm, 0.043f * lm), new Key(-0.085f, 0.014f * lm, 0.043f * lm), new Key(-0.097f, 0.008f * lm, 0.036f * lm));
            Part("body_hand", Loft("palm_" + ShapeKey(s), palm, palm.Y0, palm.Y1, 0f, true, true, 14, 28,
                bump: (a, y) => G(a, side > 0 ? 0f : 180f, 40f, y, -0.04f, 0.04f, 0.003f)), mat, hand);
            var fingers = new Transform[5][];
            float[] len = { 0.06f, 0.074f, 0.082f, 0.077f, 0.06f };
            float[] zs = { 0f, 0.026f, 0.0085f, -0.0085f, -0.025f };
            float[] rad = { 0.0105f, 0.0088f, 0.0092f, 0.0086f, 0.0075f };
            for (int f = 0; f < 5; f++)
            {
                bool thumb = f == 0;
                int joints = 3;
                var chain = new Transform[joints];
                Transform parent = hand;
                Vector3 at = thumb ? new Vector3(-side * 0.006f, -0.022f, 0.034f * lm) : new Vector3(0f, -0.093f, zs[f] * lm);
                float[] part = thumb ? new[] { 0.42f, 0.33f, 0.25f } : new[] { 0.45f, 0.3f, 0.25f };
                for (int j = 0; j < joints; j++)
                {
                    var b = Bone("finger" + f + "_" + j, parent, j == 0 ? at : new Vector3(0f, -len[f] * part[j - 1] * lm, 0f));
                    if (thumb && j == 0) b.localRotation = Quaternion.Euler(-35f, side * 25f, 0f);
                    float l = len[f] * part[j] * lm, r = rad[f] * lm * (1f - j * 0.12f);
                    var seg = new Profile(new Key(0.004f, r * 0.9f, r * 0.85f), new Key(-l * 0.5f, r * 0.95f, r * 0.82f), new Key(-l, r * 0.85f, r * 0.78f), new Key(-l - r * 0.6f, r * 0.45f, r * 0.4f));
                    Part("body_finger", Loft($"phal_{f}_{j}_{ShapeKey(s)}", seg, -l - r * 0.6f, 0.004f, 0f, true, true, 6, 14), mat, b);
                    if (j == joints - 1 && !thumb)
                    {
                        // A fingernail on the back of the fingertip.
                        var nail = Mats.Shared("hum_nail", () => Mats.Lit(new Color(0.95f, 0.85f, 0.8f), null, 0.75f));
                        Part("body_nail", Loft("nail", new Profile(new Key(-0.005f, 0.0045f, 0.0012f), new Key(0f, 0.005f, 0.0015f), new Key(0.004f, 0.004f, 0.001f)), -0.005f, 0.004f, 0f, true, true, 3, 10), nail, b,
                            new Vector3(side * r * 0.75f, -l * 0.55f, 0f), Quaternion.Euler(0f, 0f, 90f));
                    }
                    chain[j] = b;
                    parent = b;
                }
                fingers[f] = chain;
            }
            return fingers;
        }

        /// <summary>Close a hand: 0 open, 0.2 relaxed, 1 a fist (round a handle).</summary>
        public static void CurlHand(Transform[][] fingers, float curl, float side)
        {
            if (fingers == null) return;
            for (int f = 0; f < fingers.Length; f++)
            {
                var chain = fingers[f];
                if (chain == null) continue;
                for (int j = 0; j < chain.Length; j++)
                {
                    if (chain[j] == null) continue;
                    if (f == 0)
                    {
                        float tb = curl * (j == 0 ? 15f : 40f);
                        chain[j].localRotation = (j == 0 ? Quaternion.Euler(-35f, side * 25f, 0f) : Quaternion.identity) * Quaternion.Euler(0f, 0f, -side * tb);
                        continue;
                    }
                    float ang = curl * (j == 0 ? 80f : j == 1 ? 95f : 70f) + 6f;
                    chain[j].localRotation = Quaternion.Euler(0f, 0f, -side * ang);
                }
            }
        }

        /// <summary>A point on the face (x across, y up in the head bone's space), just on its surface.</summary>
        public static Vector3 FacePoint(Shape s, float x, float y, float lift = 0f) => new Vector3(x, y, s.head.FrontZ(y, x) + lift);
    }
}
