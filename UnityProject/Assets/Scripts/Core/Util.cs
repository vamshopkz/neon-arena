using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arena
{
    /// <summary>Seeded RNG: every player builds the same maze / forest from the match seed.</summary>
    public class Rng
    {
        uint s;
        public Rng(int seed) { s = (uint)seed * 2654435761u + 1u; if (s == 0) s = 1; Next(); }
        public uint Next() { s ^= s << 13; s ^= s >> 17; s ^= s << 5; return s; }
        public float F() => (Next() & 0xFFFFFF) / 16777216f;
        public float F(float a, float b) => a + (b - a) * F();
        public int I(int n) => (int)(Next() % (uint)Mathf.Max(1, n));
        public bool Chance(float p) => F() < p;
    }

    /// <summary>Shared materials made from the URP base materials created by ProjectSetup (Resources/Materials).</summary>
    public static class Mats
    {
        static Material lit, litT, litE, unlit, unlitT, partA, partAdd;
        static readonly Dictionary<long, Material> colors = new Dictionary<long, Material>();
        static readonly Dictionary<int, Material> glows = new Dictionary<int, Material>();

        static Material Base(ref Material m, string name)
        {
            if (m == null) m = Resources.Load<Material>("Materials/" + name);
            return m;
        }

        public static Material Lit(Color c, float smooth = 0.2f, float metal = 0f)
        {
            long key = ((long)(c.r * 255) << 32) | ((long)(c.g * 255) << 24) | ((long)(c.b * 255) << 16) | ((long)(smooth * 100) << 8) | (long)(metal * 100);
            if (colors.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(Base(ref lit, "Lit")) { color = c, enableInstancing = true };
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            colors[key] = m;
            return m;
        }

        public static Material Tex(Texture2D t, Color tint, float smooth = 0.15f, float metal = 0f)
        {
            var m = new Material(Base(ref lit, "Lit")) { color = tint, mainTexture = t, enableInstancing = true };
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            return m;
        }

        /// <summary>Lit + glowing (neon edges, lava skin, portals).</summary>
        public static Material Emissive(Texture2D main, Color tint, Color emission, Texture2D emitMap = null, float smooth = 0.3f)
        {
            var m = new Material(Base(ref litE, "LitEmissive")) { color = tint };
            if (main != null) m.mainTexture = main;
            m.SetTexture("_EmissionMap", emitMap != null ? emitMap : main);
            m.SetColor("_EmissionColor", emission);
            m.SetFloat("_Smoothness", smooth);
            m.EnableKeyword("_EMISSION");
            return m;
        }

        public static Material Glow(Color c)
        {
            int key = ((int)(c.r * 63) << 12) | ((int)(c.g * 63) << 6) | (int)(c.b * 63);
            if (glows.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(Base(ref unlit, "Unlit")) { color = c };
            glows[key] = m;
            return m;
        }

        public static Material UnlitTex(Texture2D t, Color c)
        {
            var m = new Material(Base(ref unlit, "Unlit")) { color = c, mainTexture = t };
            return m;
        }

        public static Material UnlitTransparent(Color c, Texture2D t = null)
        {
            var m = new Material(Base(ref unlitT, "UnlitTransparent")) { color = c };
            if (t != null) m.mainTexture = t;
            return m;
        }

        public static Material Additive(Color c, Texture2D t = null)
        {
            var m = new Material(Base(ref partAdd, "ParticleAdd")) { color = c };
            if (t != null) m.mainTexture = t;
            return m;
        }

        public static Material Transparent(Color c)
        {
            var m = new Material(Base(ref litT, "LitTransparent")) { color = c };
            return m;
        }

        public static Material Particles(Texture2D t = null)
        {
            var m = new Material(Base(ref partA, "ParticleAlpha"));
            if (t != null) m.mainTexture = t;
            return m;
        }
    }

    /// <summary>
    /// Builds static geometry: boxes (any yaw), cylinders, cones and arbitrary meshes are baked into one
    /// mesh per material. Box UVs are in metres / tile so wall textures keep their real size.
    /// </summary>
    public class Geo
    {
        class Part
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<int> t = new List<int>();
        }
        readonly Dictionary<Material, Part> parts = new Dictionary<Material, Part>();
        readonly List<Material> order = new List<Material>();

        Part P(Material m)
        {
            if (!parts.TryGetValue(m, out var p)) { p = new Part(); parts[m] = p; order.Add(m); }
            return p;
        }

        void Quad(Part p, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 nrm, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            int s = p.v.Count;
            p.v.Add(a); p.v.Add(b); p.v.Add(c); p.v.Add(d);
            p.n.Add(nrm); p.n.Add(nrm); p.n.Add(nrm); p.n.Add(nrm);
            p.uv.Add(ua); p.uv.Add(ub); p.uv.Add(uc); p.uv.Add(ud);
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), nrm) >= 0) { p.t.Add(s); p.t.Add(s + 1); p.t.Add(s + 2); p.t.Add(s); p.t.Add(s + 2); p.t.Add(s + 3); }
            else { p.t.Add(s); p.t.Add(s + 2); p.t.Add(s + 1); p.t.Add(s); p.t.Add(s + 3); p.t.Add(s + 2); }
        }

        static readonly Vector3[] faceN = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        static readonly Vector3[] faceU = { Vector3.forward, Vector3.forward, Vector3.right, Vector3.right, Vector3.right, Vector3.right };
        static readonly Vector3[] faceV = { Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up };

        /// <summary>Box centred at <paramref name="c"/>, rotated by <paramref name="rot"/>.</summary>
        public void Box(Material m, Vector3 c, Vector3 size, Quaternion rot, float tile = 2f, bool bottom = false)
        {
            var p = P(m);
            var h = size * 0.5f;
            for (int f = 0; f < 6; f++)
            {
                if (f == 3 && !bottom) continue;
                var n = faceN[f]; var u = faceU[f]; var v = faceV[f];
                float hn = Vector3.Dot(Abs(n), h), hu = Vector3.Dot(u, h), hv = Vector3.Dot(v, h);
                var fc = n * hn;
                Vector3 W(float a, float b) => c + rot * (fc + u * a + v * b);
                float su = hu * 2 / tile, sv = hv * 2 / tile;
                Quad(p, W(-hu, -hv), W(hu, -hv), W(hu, hv), W(-hu, hv), rot * n,
                    new Vector2(0, 0), new Vector2(su, 0), new Vector2(su, sv), new Vector2(0, sv));
            }
        }

        public void Box(Material m, Vector3 c, Vector3 size, float yawDeg = 0, float tile = 2f) => Box(m, c, size, Quaternion.Euler(0, yawDeg, 0), tile);

        /// <summary>Box with min/max corners (axis aligned).</summary>
        public void BoxMM(Material m, Vector3 min, Vector3 max, float tile = 2f) => Box(m, (min + max) * 0.5f, max - min, Quaternion.identity, tile);

        static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        /// <summary>Cylinder / cone (rTop = 0) standing on <paramref name="b"/>, axis = rot * up.</summary>
        public void Cyl(Material m, Vector3 b, float r, float h, int seg = 12, float rTop = -1, bool caps = true, float tile = 2f, Quaternion? rot = null)
        {
            if (rTop < 0) rTop = r;
            var q = rot ?? Quaternion.identity;
            var p = P(m);
            float circ = 2 * Mathf.PI * r;
            float slope = (r - rTop) / Mathf.Max(0.001f, h);
            Vector3 up = q * Vector3.up;
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2 / seg, a1 = (i + 1) * Mathf.PI * 2 / seg;
                var d0 = q * new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)); var d1 = q * new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                var n = ((d0 + d1) * 0.5f).normalized;
                n = (n + up * slope).normalized;
                int s = p.v.Count;
                Vector3 A = b + d0 * r, B = b + d1 * r, C = b + d1 * rTop + up * h, D = b + d0 * rTop + up * h;
                float u0 = circ * i / seg / tile, u1 = circ * (i + 1) / seg / tile, vv = h / tile;
                Quad(p, A, B, C, D, n, new Vector2(u0, 0), new Vector2(u1, 0), new Vector2(u1, vv), new Vector2(u0, vv));
                var nA = (d0 + up * slope).normalized; var nB = (d1 + up * slope).normalized;
                p.n[s] = nA; p.n[s + 1] = nB; p.n[s + 2] = nB; p.n[s + 3] = nA;
                if (!caps) continue;
                for (int e = 0; e < 2; e++)
                {
                    float rr = e == 0 ? rTop : r;
                    if (rr < 0.001f) continue;
                    var ctr = e == 0 ? b + up * h : b;
                    var cn = e == 0 ? up : -up;
                    int c0 = p.v.Count;
                    p.v.Add(ctr); p.v.Add(ctr + d0 * rr); p.v.Add(ctr + d1 * rr);
                    p.n.Add(cn); p.n.Add(cn); p.n.Add(cn);
                    p.uv.Add(new Vector2(0.5f, 0.5f)); p.uv.Add(new Vector2(0.5f + Mathf.Cos(a0) * 0.5f, 0.5f + Mathf.Sin(a0) * 0.5f)); p.uv.Add(new Vector2(0.5f + Mathf.Cos(a1) * 0.5f, 0.5f + Mathf.Sin(a1) * 0.5f));
                    if (Vector3.Dot(Vector3.Cross(p.v[c0 + 1] - p.v[c0], p.v[c0 + 2] - p.v[c0]), cn) >= 0) { p.t.Add(c0); p.t.Add(c0 + 1); p.t.Add(c0 + 2); }
                    else { p.t.Add(c0); p.t.Add(c0 + 2); p.t.Add(c0 + 1); }
                }
            }
        }

        /// <summary>Cylinder between two points.</summary>
        public void Rod(Material m, Vector3 a, Vector3 b, float r, int seg = 10, float rB = -1, float tile = 2f)
        {
            var d = b - a;
            Cyl(m, a, r, d.magnitude, seg, rB, true, tile, Quaternion.FromToRotation(Vector3.up, d.normalized));
        }

        /// <summary>Flat polygon (in the XY plane) extruded along Z by depth, placed with mx. Polygon may be concave.</summary>
        public void Extrude(Material m, Vector2[] poly, float depth, Matrix4x4 mx, float uvScale = 4f)
        {
            var p = P(m);
            int n = poly.Length;
            float area = 0;
            for (int i = 0; i < n; i++) { var a = poly[i]; var b = poly[(i + 1) % n]; area += a.x * b.y - b.x * a.y; }
            var tris = EarClip(poly, area > 0);
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? depth * 0.5f : -depth * 0.5f;
                var nn = mx.MultiplyVector(face == 0 ? Vector3.forward : Vector3.back).normalized;
                int s = p.v.Count;
                foreach (var v in poly) { p.v.Add(mx.MultiplyPoint3x4(new Vector3(v.x, v.y, z))); p.n.Add(nn); p.uv.Add(v * uvScale); }
                for (int t = 0; t < tris.Count; t += 3)
                {
                    int a = s + tris[t], b = s + tris[t + 1], c = s + tris[t + 2];
                    if (Vector3.Dot(Vector3.Cross(p.v[b] - p.v[a], p.v[c] - p.v[a]), nn) >= 0) { p.t.Add(a); p.t.Add(b); p.t.Add(c); }
                    else { p.t.Add(a); p.t.Add(c); p.t.Add(b); }
                }
            }
            for (int i = 0; i < n; i++)
            {
                var a = poly[i]; var b = poly[(i + 1) % n];
                Vector3 A = mx.MultiplyPoint3x4(new Vector3(a.x, a.y, depth * 0.5f)), B = mx.MultiplyPoint3x4(new Vector3(b.x, b.y, depth * 0.5f));
                Vector3 C = mx.MultiplyPoint3x4(new Vector3(b.x, b.y, -depth * 0.5f)), D = mx.MultiplyPoint3x4(new Vector3(a.x, a.y, -depth * 0.5f));
                var edge = b - a;
                var ln = new Vector3(edge.y, -edge.x, 0).normalized * (area > 0 ? 1 : -1);
                Quad(p, A, B, C, D, mx.MultiplyVector(ln).normalized, new Vector2(0, 0), new Vector2(edge.magnitude * uvScale, 0), new Vector2(edge.magnitude * uvScale, depth * uvScale), new Vector2(0, depth * uvScale));
            }
        }

        static List<int> EarClip(Vector2[] poly, bool ccw)
        {
            var idx = new List<int>();
            for (int i = 0; i < poly.Length; i++) idx.Add(i);
            var res = new List<int>();
            int guard = 0;
            while (idx.Count > 3 && guard++ < 1000)
            {
                bool clipped = false;
                for (int i = 0; i < idx.Count; i++)
                {
                    int i0 = idx[(i + idx.Count - 1) % idx.Count], i1 = idx[i], i2 = idx[(i + 1) % idx.Count];
                    Vector2 a = poly[i0], b = poly[i1], c = poly[i2];
                    float cr = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                    if (ccw ? cr <= 0 : cr >= 0) continue;
                    bool inside = false;
                    foreach (int j in idx)
                    {
                        if (j == i0 || j == i1 || j == i2) continue;
                        if (InTri(poly[j], a, b, c)) { inside = true; break; }
                    }
                    if (inside) continue;
                    res.Add(i0); res.Add(i1); res.Add(i2);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
            if (idx.Count == 3) { res.Add(idx[0]); res.Add(idx[1]); res.Add(idx[2]); }
            return res;
        }

        static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        /// <summary>Any mesh (sphere, extrusion) transformed into place.</summary>
        public void Mesh(Material m, Mesh src, Matrix4x4 mx)
        {
            var p = P(m);
            int s = p.v.Count;
            var vs = src.vertices; var ns = src.normals; var us = src.uv;
            for (int i = 0; i < vs.Length; i++)
            {
                p.v.Add(mx.MultiplyPoint3x4(vs[i]));
                p.n.Add(ns.Length > i ? mx.MultiplyVector(ns[i]).normalized : Vector3.up);
                p.uv.Add(us.Length > i ? us[i] : Vector2.zero);
            }
            foreach (var t in src.triangles) p.t.Add(s + t);
        }

        public bool Empty => parts.Count == 0;

        public GameObject Build(string name, Transform parent, bool shadows = true, int layer = 8)
        {
            var root = new GameObject(name) { layer = layer };
            root.transform.SetParent(parent, false);
            foreach (var m in order)
            {
                var p = parts[m];
                if (p.v.Count == 0) continue;
                var mesh = new Mesh { indexFormat = p.v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16, name = name + " " + m.name };
                mesh.SetVertices(p.v); mesh.SetNormals(p.n); mesh.SetUVs(0, p.uv); mesh.SetTriangles(p.t, 0);
                mesh.RecalculateBounds();
                var go = new GameObject(m.name) { layer = layer };
                go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = m;
                r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            parts.Clear(); order.Clear();
            return root;
        }
    }

    /// <summary>Primitive helpers.</summary>
    public static class Prim
    {
        static readonly Dictionary<PrimitiveType, Mesh> meshes = new Dictionary<PrimitiveType, Mesh>();
        static readonly System.Type[] keep = { typeof(SphereCollider), typeof(CapsuleCollider), typeof(BoxCollider), typeof(MeshCollider) };

        public static Mesh MeshOf(PrimitiveType t)
        {
            if (meshes.TryGetValue(t, out var m) && m != null) return m;
            var go = GameObject.CreatePrimitive(t);
            m = go.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(go);
            meshes[t] = m;
            return m;
        }

        public static GameObject Make(PrimitiveType t, Transform parent, Vector3 localPos, Vector3 scale, Material mat, string name = null, int layer = -1)
        {
            var go = new GameObject(name ?? t.ToString());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = MeshOf(t);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            if (layer >= 0) go.layer = layer;
            return go;
        }

        public static GameObject MeshGO(Mesh mesh, Transform parent, Material mat, string name = "Mesh")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        /// <summary>World text (signs, name tags).</summary>
        public static TextMeshPro Text(Transform parent, string text, Vector3 pos, float size, Color c, bool outline = true)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.font = UIKit.Font; t.fontSharedMaterial = outline ? UIKit.TextOutline : UIKit.TextPlain;
            t.text = text; t.fontSize = size; t.color = c; t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.rectTransform.sizeDelta = new Vector2(20, 3);
            return t;
        }

        public static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
        }
    }

    public static class Layers
    {
        public const int World = 8, Hitbox = 9, ViewModel = 10, Body = 11, Ball = 12, LocalPlayer = 15, Icon = 31;
        public static readonly int ShotMask = (1 << World) | (1 << Hitbox) | (1 << Ball);
        public static readonly int SolidMask = (1 << World);
        public static readonly int MoveMask = (1 << World) | (1 << Ball);
    }
}
