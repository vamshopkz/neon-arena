using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arena
{
    public struct Spawn { public Vector3 pos; public float yaw; }

    /// <summary>One built location: geometry + colliders under <see cref="root"/>, spawns and special objects.</summary>
    public class Map
    {
        public string id;
        public GameObject root;
        public readonly List<Spawn> A = new List<Spawn>(), B = new List<Spawn>(), L = new List<Spawn>();
        public Rect bounds;
        public readonly List<(int mode, Rect area)> pads = new List<(int, Rect)>();
        public Rect shop;
        public readonly List<Pickup> pickups = new List<Pickup>();
        public Ball ball;
        public bool sky;
        public Color bg = Color.black, fog = Color.black, ambSky = Color.gray, ambEq = Color.gray, ambGround = Color.gray;
        public float fogStart = 60, fogEnd = 200;
        public Color vmLight = Color.white;
        public System.Action<float> tick;

        public void Activate(Camera cam)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fog;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambSky;
            RenderSettings.ambientEquatorColor = ambEq;
            RenderSettings.ambientGroundColor = ambGround;
            RenderSettings.skybox = sky ? Resources.Load<Material>("Materials/Sky") : null;
            if (cam != null)
            {
                cam.clearFlags = sky ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                cam.backgroundColor = bg;
                cam.farClipPlane = fogEnd + 50;
            }
        }

        public void Destroy() { if (root != null) Object.Destroy(root); }
    }

    public class Pickup
    {
        public int id;
        public Vector3 pos;
        public GameObject go;
        public bool active = true;
        public float respawnAt;
    }

    /// <summary>What a shot can hit besides walls: fighters (id), training targets, the ball.</summary>
    public class Hittable : MonoBehaviour
    {
        public string id;
        public bool head;
        public int kind;   // 0 fighter, 1 target, 2 ball
        public RangeTarget target;
    }

    /// <summary>Training range target: falls down when hit, stands up again.</summary>
    public class RangeTarget : MonoBehaviour
    {
        public Transform pivot;
        public float moveAmp, moveSpeed, baseX;
        float down, downUntil;
        public static int Hits;
        public bool IsDown => Time.time < downUntil;

        public void Hit()
        {
            if (Time.time < downUntil) return;
            downUntil = Time.time + 2f;
            Hits++;
        }

        void Update()
        {
            bool isDown = Time.time < downUntil;
            down = Mathf.MoveTowards(down, isDown ? 1 : 0, Time.deltaTime * (isDown ? 6 : 2));
            pivot.localRotation = Quaternion.Euler(-85 * down, 0, 0);
            if (moveAmp > 0)
            {
                var p = transform.localPosition;
                p.x = baseX + Mathf.Sin(Time.time * moveSpeed) * moveAmp;
                transform.localPosition = p;
            }
        }
    }

    /// <summary>The stadium ball: the host simulates it, everyone else follows the received position.</summary>
    public class Ball : MonoBehaviour
    {
        public const float R = 1.2f;
        public Vector3 vel;
        public Vector3 netPos, netVel;
        public bool simulate;
        Transform mesh;

        public static Ball Create(Transform parent)
        {
            var go = new GameObject("Ball") { layer = Layers.Ball };
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0, R, 0);
            var b = go.AddComponent<Ball>();
            var sc = go.AddComponent<SphereCollider>(); sc.radius = R;
            var h = go.AddComponent<Hittable>(); h.kind = 2; h.id = "ball";
            b.mesh = Prim.Make(PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * R * 2, Mats.Tex(Tex.Ball(), Color.white, 0.55f), "BallMesh").transform;
            b.netPos = go.transform.position;
            return b;
        }

        public void Kick(Vector3 impulse) { vel += new Vector3(impulse.x, 0, impulse.z); }

        public void Reset() { transform.position = new Vector3(0, R, 0); vel = Vector3.zero; netPos = transform.position; netVel = Vector3.zero; }

        void Update()
        {
            float dt = Time.deltaTime;
            var p = transform.position;
            if (simulate)
            {
                vel *= Mathf.Exp(-0.7f * dt);
                p += vel * dt;
                // walls of the stadium and the goal nets
                float wx = 37.5f - R, wz = 24.5f - R;
                bool inGoalMouth = Mathf.Abs(p.z) < 3.66f - 0.3f;
                float limX = inGoalMouth ? 36.5f - R : wx;
                if (Mathf.Abs(p.x) > limX) { p.x = Mathf.Sign(p.x) * limX; vel.x = -vel.x * 0.6f; }
                if (Mathf.Abs(p.z) > wz) { p.z = Mathf.Sign(p.z) * wz; vel.z = -vel.z * 0.6f; }
                // cover blocks
                foreach (var c in Physics.OverlapSphere(p, R, Layers.SolidMask))
                {
                    var cp = c.ClosestPoint(p);
                    var d = p - cp; d.y = 0;
                    if (d.sqrMagnitude < 1e-6f) continue;
                    var n = d.normalized;
                    p += n * (R - d.magnitude);
                    float vn = Vector3.Dot(vel, n);
                    if (vn < 0) vel -= n * vn * 1.6f;
                }
                p.y = R;
            }
            else
            {
                netPos += netVel * dt;
                p = Vector3.Lerp(p, netPos, 1 - Mathf.Exp(-dt * 10));
                vel = netVel;
            }
            var move = p - transform.position;
            transform.position = p;
            if (move.sqrMagnitude > 1e-6f)
            {
                var axis = Vector3.Cross(Vector3.up, move.normalized);
                mesh.Rotate(axis, move.magnitude / R * Mathf.Rad2Deg, Space.World);
            }
        }
    }

    public static class Maps
    {
        // ------------------------------------------------------------ helpers
        class W
        {
            public readonly Geo g = new Geo();
            public readonly Transform root, cols;
            public W(Transform root) { this.root = root; cols = new GameObject("Colliders").transform; cols.SetParent(root, false); }

            public void Solid(Vector3 c, Vector3 size, float yaw = 0)
            {
                var go = new GameObject("C") { layer = Layers.World };
                go.transform.SetParent(cols, false);
                go.transform.position = c;
                go.transform.rotation = Quaternion.Euler(0, yaw, 0);
                go.AddComponent<BoxCollider>().size = size;
            }

            public void Box(Material m, Vector3 c, Vector3 size, float yaw = 0, float tile = 2, bool col = true)
            {
                if (m != null) g.Box(m, c, size, yaw, tile);
                if (col) Solid(c, size, yaw);
            }

            /// <summary>Box standing on the ground: x/z centre, y from y0, size w (local x) × h × d (local z).</summary>
            public void Block(Material m, float x, float z, float w, float d, float h, float yaw = 0, float y0 = 0, float tile = 2, bool col = true)
                => Box(m, new Vector3(x, y0 + h / 2, z), new Vector3(w, h, d), yaw, tile, col);

            /// <summary>Wall from a to b (xz) of height h and thickness t.</summary>
            public void Wall(Material m, Vector2 a, Vector2 b, float h, float t, float y0 = 0, float tile = 2, bool col = true, float ext = -1)
            {
                var d = b - a;
                float len = d.magnitude;
                if (len < 0.01f) return;
                if (ext < 0) ext = t / 2;
                float yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
                var c = (a + b) / 2;
                Box(m, new Vector3(c.x, y0 + h / 2, c.y), new Vector3(t, h, len + ext * 2), yaw, tile, col);
            }

            public void Trunk(Material m, Vector3 b, float r, float h, float colR = -1, bool col = true)
            {
                if (m != null) g.Cyl(m, b, r, h, 10);
                if (!col) return;
                var go = new GameObject("T") { layer = Layers.World };
                go.transform.SetParent(cols, false);
                go.transform.position = b + Vector3.up * h / 2;
                var cc = go.AddComponent<CapsuleCollider>();
                cc.radius = colR > 0 ? colR : r; cc.height = h + cc.radius * 2;
            }

            public void Done(bool shadows) => g.Build("Geometry", root, shadows, Layers.World);
        }

        static Spawn SP(float x, float z, float tx = 0, float tz = 0) => new Spawn { pos = new Vector3(x, 0.1f, z), yaw = Mathf.Atan2(tx - x, tz - z) * Mathf.Rad2Deg };

        static Light Sun(Transform parent, Color c, float intensity, Vector3 euler, bool shadows)
        {
            var go = new GameObject("Sun");
            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.Euler(euler);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional; l.color = c; l.intensity = intensity;
            l.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            l.shadowStrength = 0.75f;
            return l;
        }

        static void PointLight(Transform parent, Vector3 pos, Color c, float intensity, float range)
        {
            var go = new GameObject("Light");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = c; l.intensity = intensity; l.range = range; l.shadows = LightShadows.None;
        }

        static Material Rep(Material m, float tile) { return m; }

        static GameObject Ground(Transform parent, float size, Material m, float tile)
        {
            var g = new Geo();
            g.Box(m, new Vector3(0, -0.05f, 0), new Vector3(size, 0.1f, size), Quaternion.identity, tile);
            var go = g.Build("Ground", parent, false, Layers.World);
            var col = new GameObject("GroundCol") { layer = Layers.World };
            col.transform.SetParent(parent, false);
            col.transform.position = new Vector3(0, -0.5f, 0);
            col.AddComponent<BoxCollider>().size = new Vector3(size, 1, size);
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>()) r.receiveShadows = true;
            return go;
        }

        static TextMeshPro Sign(Transform parent, string text, Vector3 pos, float yaw, float size, Color c)
        {
            var t = Prim.Text(parent, text, pos, size, c);
            t.transform.rotation = Quaternion.Euler(0, yaw, 0);
            t.rectTransform.sizeDelta = new Vector2(40, 6);
            return t;
        }

        static readonly string[] Graffiti = { "NEON", "ARENA", "2x2", "BOOM", "FRAG", "GG", "WAR", "SKILL", "RUSH", "HEADSHOT", "1VS1", "TOP" };
        static readonly Color[] Neon = { Tex.Hex("ff3df2"), Tex.Hex("3dd2ff"), Tex.Hex("ffe23d"), Tex.Hex("3dff9b"), Tex.Hex("ff8a3d"), Tex.Hex("b43dff") };

        static void Graf(Transform parent, Vector3 pos, float yaw, int i, float size = 9)
        {
            var t = Prim.Text(parent, Graffiti[i % Graffiti.Length], pos, size, Neon[i % Neon.Length]);
            t.transform.rotation = Quaternion.Euler(0, yaw, -6 + (i % 3) * 5);
            t.fontStyle = FontStyles.Italic | FontStyles.Bold;
            t.enableVertexGradient = true;
            var c2 = Neon[(i + 2) % Neon.Length];
            t.colorGradient = new VertexGradient(Color.white, Color.white, c2, c2);
            t.color = Neon[i % Neon.Length];
        }

        public static Map Build(string id, int seed, Transform parent, bool shadows)
        {
            var root = new GameObject("Map " + id);
            root.transform.SetParent(parent, false);
            var m = new Map { id = id, root = root };
            switch (id)
            {
                case "lobby": Lobby(m, shadows); break;
                case "desert": Desert(m, seed, shadows); break;
                case "maze": Maze(m, seed, shadows); break;
                case "stadium": Stadium(m, shadows); break;
                default: Forest(m, seed, shadows); break;
            }
            Physics.SyncTransforms();
            return m;
        }

        // ------------------------------------------------------------ LOBBY (picture 1)
        static void Lobby(Map m, bool shadows)
        {
            var w = new W(m.root.transform);
            m.sky = false;
            m.bg = Tex.Hex("05061a"); m.fog = Tex.Hex("070820"); m.fogStart = 40; m.fogEnd = 140;
            m.ambSky = Tex.Hex("5a5ab0"); m.ambEq = Tex.Hex("3a3478"); m.ambGround = Tex.Hex("1a1640");
            m.vmLight = new Color(0.75f, 0.75f, 1f);
            var floor = Mats.Emissive(Tex.Grid("232866", "8d9bff"), Color.white, new Color(0.35f, 0.35f, 0.55f));
            var floor2 = Mats.Emissive(Tex.Grid("2b2058", "c07dff"), Color.white, new Color(0.4f, 0.3f, 0.55f));
            var wall = Mats.Emissive(Tex.NeonWall(), Color.white, new Color(0.35f, 0.4f, 0.6f));
            var neonC = Mats.Glow(new Color(0.4f, 0.95f, 1.6f));
            var neonP = Mats.Glow(new Color(1.2f, 0.5f, 2f));
            var star = Mats.UnlitTex(Tex.Stars(), Color.white);
            var table = Mats.Lit(Tex.Hex("1c2048"), 0.7f, 0.5f);
            var voidM = Mats.Lit(Tex.Hex("0b0d22"), 0.1f);
            const float H = 4.2f, T = 0.4f;
            var gr = m.root.transform;

            void NWall(Vector2 a, Vector2 b, Material neon)
            {
                w.Wall(wall, a, b, H, T, 0, 4.2f);
                w.Wall(neon, a, b, 0.08f, T + 0.06f, H, 2, false);
                w.Wall(neon, a, b, 0.06f, T + 0.04f, 0.02f, 2, false);
            }
            void Side(Vector2 a, Vector2 b, List<Vector2> gaps, Material neon)
            {
                bool horiz = Mathf.Abs(a.y - b.y) < 0.01f;
                float a0 = horiz ? a.x : a.y, a1 = horiz ? b.x : b.y, cur = a0;
                gaps.Sort((p, q) => p.x.CompareTo(q.x));
                void Seg(float p, float q) { if (horiz) NWall(new Vector2(p, a.y), new Vector2(q, a.y), neon); else NWall(new Vector2(a.x, p), new Vector2(a.x, q), neon); }
                foreach (var gp in gaps) { if (gp.x > cur) Seg(cur, gp.x); cur = gp.y; }
                if (cur < a1) Seg(cur, a1);
            }
            void Room(float x0, float z0, float x1, float z1, Vector2? top, Vector2? bottom, Vector2? left, Vector2? right, Material fl, Material neon)
            {
                Side(new Vector2(x0, z0), new Vector2(x1, z0), top.HasValue ? new List<Vector2> { top.Value } : new List<Vector2>(), neon);
                Side(new Vector2(x0, z1), new Vector2(x1, z1), bottom.HasValue ? new List<Vector2> { bottom.Value } : new List<Vector2>(), neon);
                Side(new Vector2(x0, z0), new Vector2(x0, z1), left.HasValue ? new List<Vector2> { left.Value } : new List<Vector2>(), neon);
                Side(new Vector2(x1, z0), new Vector2(x1, z1), right.HasValue ? new List<Vector2> { right.Value } : new List<Vector2>(), neon);
                w.g.Box(fl, new Vector3((x0 + x1) / 2, -0.1f, (z0 + z1) / 2), new Vector3(x1 - x0, 0.2f, z1 - z0), Quaternion.identity, 4);
            }
            void Corridor(Vector2[] pts, float Wd, int grafBase)
            {
                int n = pts.Length;
                var L = new Vector2[n]; var R = new Vector2[n];
                Vector2 Dir(Vector2 a, Vector2 b) => (b - a).normalized;
                Vector2 Nrm(Vector2 d) => new Vector2(-d.y, d.x);
                for (int i = 0; i < n; i++)
                {
                    Vector2 off;
                    if (i == 0) off = Nrm(Dir(pts[0], pts[1])) * Wd / 2;
                    else if (i == n - 1) off = Nrm(Dir(pts[n - 2], pts[n - 1])) * Wd / 2;
                    else
                    {
                        var n1 = Nrm(Dir(pts[i - 1], pts[i])); var n2 = Nrm(Dir(pts[i], pts[i + 1]));
                        var mi = (n1 + n2).normalized;
                        off = mi * (Wd / 2 / Vector2.Dot(mi, n1));
                    }
                    L[i] = pts[i] + off; R[i] = pts[i] - off;
                }
                for (int i = 0; i < n - 1; i++)
                {
                    NWall(L[i], L[i + 1], neonP); NWall(R[i], R[i + 1], neonP);
                    var a = pts[i]; var b = pts[i + 1]; var d = b - a;
                    float yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
                    var c = (a + b) / 2;
                    w.g.Box(floor2, new Vector3(c.x, -0.1f + i * 0.004f, c.y), new Vector3(Wd + 0.3f, 0.2f, d.magnitude + Wd * 0.6f), Quaternion.Euler(0, yaw, 0), 4);
                    if (d.magnitude > 3.5f)
                    {
                        var nn = Nrm(d.normalized);
                        var lm = (L[i] + L[i + 1]) / 2 - nn * (T / 2 + 0.03f);
                        var rm = (R[i] + R[i + 1]) / 2 + nn * (T / 2 + 0.03f);
                        // text faces into the corridor: its front (-Z of TMP) must look away from the wall
                        Graf(gr, new Vector3(lm.x, 2.2f, lm.y), Mathf.Atan2(nn.x, nn.y) * Mathf.Rad2Deg, grafBase + i * 2, 7);
                        Graf(gr, new Vector3(rm.x, 2.2f, rm.y), Mathf.Atan2(-nn.x, -nn.y) * Mathf.Rad2Deg, grafBase + i * 2 + 1, 7);
                    }
                }
            }

            // void below everything
            w.g.Box(voidM, new Vector3(0, -0.6f, 0), new Vector3(220, 0.2f, 220), Quaternion.identity, 8);
            var gcol = new GameObject("GroundCol") { layer = Layers.World };
            gcol.transform.SetParent(gr, false); gcol.transform.position = new Vector3(0, -0.5f, 0);
            gcol.AddComponent<BoxCollider>().size = new Vector3(220, 1, 220);

            // shop (top), circle hall, corridors, training range (left), big hall with portals (bottom)
            Room(-18, -52, 18, -38, null, new Vector2(12, 16), null, null, floor, neonC);
            var cc = new Vector2(0, -16); float rr = 8;
            Corridor(new[] { new Vector2(14, -38.2f), new Vector2(14, -30), new Vector2(12.8f, -26.5f), new Vector2(10, -24), new Vector2(5.8f, -21f) }, 4, 0);
            Corridor(new[] { new Vector2(-7.6f, -13.9f), new Vector2(-10.9f, -13f), new Vector2(-14.5f, -11.3f), new Vector2(-16, -8), new Vector2(-16, -5.6f) }, 4, 5);
            Corridor(new[] { new Vector2(0, -8.4f), new Vector2(0, 6.2f) }, 4, 9);
            Room(-23, -5.5f, -9, 5.4f, new Vector2(-18, -14), null, null, null, floor, neonC);
            Room(-26, 6, 26, 34, new Vector2(-2, 2), null, null, null, floor, neonC);
            // ring
            {
                int N = 40;
                float[] gaps = { -40f * Mathf.Deg2Rad, 165 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad };
                for (int i = 0; i < N; i++)
                {
                    float a0 = i * Mathf.PI * 2 / N, a1 = (i + 1) * Mathf.PI * 2 / N, am = (a0 + a1) / 2;
                    bool gap = false;
                    foreach (var gp in gaps) if (Mathf.Abs(Mathf.DeltaAngle(am * Mathf.Rad2Deg, gp * Mathf.Rad2Deg)) < 17) gap = true;
                    if (gap) continue;
                    NWall(cc + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rr, cc + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rr, neonP);
                }
                w.g.Cyl(star, new Vector3(cc.x, -0.19f, cc.y), rr, 0.2f, 48, -1, true, 4);
                int gi = 0;
                foreach (float deg in new[] { 20f, 60f, 130f, 220f, 270f, 315f })
                {
                    float a = deg * Mathf.Deg2Rad;
                    var p = cc + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (rr - 0.45f);
                    // TMP text is readable from its -Z side, so the text's forward points away from the centre
                    Graf(gr, new Vector3(p.x, 2.3f, p.y), Mathf.Atan2(Mathf.Cos(a), Mathf.Sin(a)) * Mathf.Rad2Deg, gi++ + 3, 8);
                }
                PointLight(gr, new Vector3(cc.x, 3.5f, cc.y), new Color(0.7f, 0.4f, 1f), 6, 18);
            }
            // stair stripes in the bottom corridor
            for (int i = 0; i < 6; i++) w.g.Box(neonC, new Vector3(0, 0.02f, 1.5f + i * 0.8f), new Vector3(3.6f, 0.04f, 0.1f), Quaternion.identity);

            // --- shop: tables with weapons, wall rack, sign
            m.shop = Rect.MinMaxRect(-18, -52, 18, -38);
            var displays = new[] { "ak74", "svd", "mosin", "rpg7", "tt", "glock", "colt", "kabar", "butterfly", "shuriken", "kunai", "axe", "f1" };
            int di = 0;
            foreach (float tz in new[] { -47f, -42.5f })
                foreach (float tx in new[] { -10f, -3.4f, 3.4f, 10f })
                {
                    w.Block(table, tx, tz, 4.2f, 1.6f, 0.9f, 0, 0, 2);
                    w.g.Box(neonC, new Vector3(tx, 0.91f, tz + 0.8f), new Vector3(4.2f, 0.03f, 0.04f), Quaternion.identity);
                    for (int k = 0; k < 2 && di < displays.Length; k++, di++)
                    {
                        var wpn = Models.Weapon(displays[di], di % 3 == 1 ? Defs.Skins[(di * 3) % Defs.Skins.Count].id : "std", gr, Layers.World);
                        wpn.transform.position = new Vector3(tx - 1 + k * 2, 0.95f, tz);
                        wpn.transform.rotation = Quaternion.Euler(0, 90, 90);
                        wpn.transform.localScale = Vector3.one * 1.6f;
                    }
                }
            w.Block(table, 0, -51.4f, 30, 0.6f, 3.2f, 0, 0, 2, false);
            for (int k = 0; k < 9; k++)
            {
                var id = new[] { "ak74", "svd", "rpg7", "mosin", "ak74", "svd", "glock", "colt", "tt" }[k];
                var wpn = Models.Weapon(id, Defs.Skins[(k * 2 + 1) % Defs.Skins.Count].id, gr, Layers.World);
                wpn.transform.position = new Vector3(-12 + k * 3, 2f, -51f);
                wpn.transform.rotation = Quaternion.Euler(0, 90, 0);
                wpn.transform.localScale = Vector3.one * 1.8f;
            }
            Sign(gr, "МАГАЗИН ОРУЖИЯ", new Vector3(0, 3.55f, -50.9f), 180, 9, Tex.Hex("5fe0ff"));
            PointLight(gr, new Vector3(-8, 3.6f, -45), new Color(0.6f, 0.8f, 1f), 5, 16);
            PointLight(gr, new Vector3(8, 3.6f, -45), new Color(0.6f, 0.8f, 1f), 5, 16);

            // --- training range
            Sign(gr, "ТИР", new Vector3(-16, 3.4f, 5.0f), 0, 10, Tex.Hex("ffe23d"));
            float[] tx2 = { -21, -18.5f, -16, -13.5f, -11 };
            for (int i = 0; i < tx2.Length; i++)
            {
                var tgo = new GameObject("Target");
                tgo.transform.SetParent(gr, false);
                tgo.transform.position = new Vector3(tx2[i], 0, 3.6f - (i % 2) * 2f);
                var rt = tgo.AddComponent<RangeTarget>();
                rt.baseX = tx2[i];
                if (i == 1 || i == 3) { rt.moveAmp = 1.2f; rt.moveSpeed = 1.3f + i * 0.2f; }
                Prim.Make(PrimitiveType.Cube, tgo.transform, new Vector3(0, 0.05f, 0), new Vector3(0.9f, 0.1f, 0.5f), Mats.Lit(Tex.Hex("333a66"), 0.4f));
                var piv = new GameObject("Pivot").transform; piv.SetParent(tgo.transform, false); rt.pivot = piv;
                Prim.Make(PrimitiveType.Cylinder, piv, new Vector3(0, 0.6f, 0), new Vector3(0.08f, 0.6f, 0.08f), Mats.Lit(Tex.Hex("c0c0c0"), 0.6f, 0.8f));
                var disc = Prim.Make(PrimitiveType.Cylinder, piv, new Vector3(0, 1.45f, 0), new Vector3(1.1f, 0.03f, 1.1f), Mats.Lit(Color.white, 0.3f));
                disc.transform.localRotation = Quaternion.Euler(90, 0, 0);
                var face = Prim.Make(PrimitiveType.Quad, piv, new Vector3(0, 1.45f, -0.04f), new Vector3(1.08f, 1.08f, 1), Mats.UnlitTransparent(Color.white, Tex.Target()));
                face.transform.localRotation = Quaternion.identity;
                var hgo = new GameObject("Hit") { layer = Layers.Hitbox };
                hgo.transform.SetParent(piv, false);
                hgo.transform.localPosition = new Vector3(0, 1.45f, 0);
                var sc = hgo.AddComponent<SphereCollider>(); sc.radius = 0.58f; sc.isTrigger = true;
                var hh = hgo.AddComponent<Hittable>(); hh.kind = 1; hh.target = rt; hh.id = "target";
            }
            PointLight(gr, new Vector3(-16, 3.6f, 0), new Color(1f, 0.85f, 0.5f), 5, 16);

            // --- portals 1vs1 … 5vs5
            Sign(gr, "ВСТАНЬ В ПОРТАЛ — ВЫБЕРЕТЕ ЛОКАЦИЮ И НАЧНЁТСЯ БОЙ", new Vector3(0, 3.6f, 33.7f), 0, 5, Color.white);
            Sign(gr, "↑ МАГАЗИН И ТИР", new Vector3(0, 3.3f, 6.25f), 180, 6, Tex.Hex("5fe0ff"));
            for (int i = 0; i < 5; i++)
            {
                float px = -18 + i * 9, pz = 26.5f;
                var padM = Mats.UnlitTex(Tex.Stars(), new Color(0.75f, 0.75f, 1f));
                w.g.Box(padM, new Vector3(px, 0.03f, pz), new Vector3(6, 0.06f, 6), Quaternion.identity, 6);
                foreach (var e in new[] { new Vector4(0, -3, 6.2f, 0.12f), new Vector4(0, 3, 6.2f, 0.12f), new Vector4(-3, 0, 0.12f, 6.2f), new Vector4(3, 0, 0.12f, 6.2f) })
                    w.g.Box(neonP, new Vector3(px + e.x, 0.08f, pz + e.y), new Vector3(e.z, 0.08f, e.w), Quaternion.identity);
                w.Block(wall, px - 3, pz + 3.1f, 0.4f, 0.4f, 4f, 0, 0, 4.2f);
                w.Block(wall, px + 3, pz + 3.1f, 0.4f, 0.4f, 4f, 0, 0, 4.2f);
                w.Block(neonC, px, pz + 3.1f, 6.4f, 0.3f, 0.12f, 0, 3.9f, 2, false);
                int n = i + 1;
                Sign(gr, n + " VS " + n, new Vector3(px, 2.7f, pz + 2.85f), 0, 9, Color.white).color = Tex.Hex("e8e8ff");
                var glow = Prim.Make(PrimitiveType.Cylinder, gr, new Vector3(px, 1.5f, pz), new Vector3(5.2f, 1.5f, 5.2f), Mats.Additive(new Color(0.55f, 0.25f, 1f, 0.18f), Tex.Glow()), "PortalGlow");
                glow.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                var gt = glow.transform;
                m.tick += dt => { float k = 1 + Mathf.Sin(Time.time * 2 + n) * 0.06f; gt.localScale = new Vector3(5.2f * k, 1.5f, 5.2f * k); };
                m.pads.Add((n, Rect.MinMaxRect(px - 3, pz - 3, px + 3, pz + 3)));
            }
            for (int i = 0; i < 4; i++) Graf(gr, new Vector3(-20 + i * 13.5f, 2.3f, 6.25f), 180, 20 + i, 7);
            for (int i = 0; i < 3; i++) { Graf(gr, new Vector3(-25.75f, 2.3f, 12 + i * 8), -90, 30 + i, 7); Graf(gr, new Vector3(25.75f, 2.3f, 12 + i * 8), 90, 40 + i, 7); }
            PointLight(gr, new Vector3(-12, 3.8f, 20), new Color(0.6f, 0.5f, 1f), 6, 20);
            PointLight(gr, new Vector3(12, 3.8f, 20), new Color(0.6f, 0.5f, 1f), 6, 20);
            Sun(gr, new Color(0.65f, 0.65f, 1f), 0.35f, new Vector3(60, 30, 0), false);

            // stars around
            var r = new Rng(9);
            var sg = new Geo(); var sm = Mats.Glow(Color.white);
            for (int i = 0; i < 400; i++)
            {
                var d = Random.onUnitSphere; d.y = Mathf.Abs(d.y) * 0.9f + 0.05f;
                sg.Box(sm, d.normalized * 160, Vector3.one * r.F(0.3f, 0.8f), Quaternion.identity);
            }
            sg.Build("Stars", gr, false, 0);

            m.L.Add(SP(0, 12, 0, 30));
            m.L.Add(SP(-3, 13, 0, 30));
            m.L.Add(SP(3, 13, 0, 30));
            m.bounds = Rect.MinMaxRect(-27, -53, 27, 35);
            w.Done(false);
        }

        // ------------------------------------------------------------ DESERT (picture 2)
        static void Desert(Map m, int seed, bool shadows)
        {
            var w = new W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = Tex.Hex("e8dcc0"); m.fogStart = 70; m.fogEnd = 220; m.bg = m.fog;
            m.ambSky = Tex.Hex("b9d4f0"); m.ambEq = Tex.Hex("d8c49a"); m.ambGround = Tex.Hex("8a7450");
            Sun(gr, Tex.Hex("fff1d6"), 1.35f, new Vector3(52, -35, 0), shadows);
            var sand = Mats.Tex(Tex.Sand(), Color.white, 0.1f);
            var stone = Mats.Tex(Tex.Sandstone(), Color.white, 0.1f);
            var adobe = Mats.Tex(Tex.Adobe(), Color.white, 0.12f);
            var adobe2 = Mats.Tex(Tex.Adobe(), new Color(0.9f, 0.82f, 0.72f), 0.12f);
            var crate = Mats.Tex(Tex.Planks("9a6a3a"), Color.white, 0.2f);
            var rope = Mats.Lit(Tex.Hex("6a4a2a"), 0.2f);
            Ground(gr, 240, sand, 6);

            // zig-zag canyon wall
            var pts = new List<Vector2>();
            float X = 30, Z = 40, step = 5;
            int k = 0;
            for (float x = -X; x < X; x += step) pts.Add(new Vector2(x, -Z - (k++ % 2) * 3));
            for (float z = -Z; z < Z; z += step) pts.Add(new Vector2(X + (k++ % 2) * 3, z));
            for (float x = X; x > -X; x -= step) pts.Add(new Vector2(x, Z + (k++ % 2) * 3));
            for (float z = Z; z > -Z; z -= step) pts.Add(new Vector2(-X - (k++ % 2) * 3, z));
            for (int i = 0; i < pts.Count; i++) w.Wall(stone, pts[i], pts[(i + 1) % pts.Count], 6, 1.8f, 0, 3, true, 1.2f);
            // safety walls outside
            w.Solid(new Vector3(0, 5, -45), new Vector3(80, 10, 1)); w.Solid(new Vector3(0, 5, 45), new Vector3(80, 10, 1));
            w.Solid(new Vector3(-35, 5, 0), new Vector3(1, 10, 100)); w.Solid(new Vector3(35, 5, 0), new Vector3(1, 10, 100));

            void Building(float cx, float cz, float rot)
            {
                float S = 11, t = 0.6f, Hh = 4.4f, hx = S / 2;
                var q = Quaternion.Euler(0, rot, 0);
                Vector2 Tr(float lx, float lz) { var v = q * new Vector3(lx, 0, lz); return new Vector2(cx + v.x, cz + v.z); }
                void Wl(float x1, float z1, float x2, float z2, float h = 4.4f, float y0 = 0) => w.Wall(adobe, Tr(x1, z1), Tr(x2, z2), h, t, y0, 2);
                Wl(-hx, -hx, hx, -hx); Wl(-hx, -hx, -hx, hx); Wl(hx, -hx, hx, hx);
                Wl(-hx, hx, -1.3f, hx); Wl(1.3f, hx, hx, hx); Wl(-1.3f, hx, 1.3f, hx, 1.4f, Hh - 1.4f);
                // upper floor ledge along the back wall + stairs along the right wall
                var lc = Tr(0, -hx + 0.3f + 1.25f);
                w.Box(adobe2, new Vector3(lc.x, 2.7f + 0.175f, lc.y), new Vector3(S - 1.2f, 0.35f, 2.5f), rot, 2);
                for (int i = 0; i < 7; i++)
                {
                    float top = 0.435f * (i + 1);
                    var sc = Tr(hx - 1.3f, -hx + 0.3f + 2.5f + 0.4f + 0.8f * (6 - i));
                    w.Box(adobe2, new Vector3(sc.x, top / 2, sc.y), new Vector3(2, top, 0.8f), rot, 2);
                }
                // parapet blocks on the ledge edge (cover)
                for (int i = 0; i < 3; i++) { var pc = Tr(-3.5f + i * 2.6f, -hx + 2.7f); w.Box(adobe, new Vector3(pc.x, 3.05f + 0.4f, pc.y), new Vector3(1.2f, 0.8f, 0.3f), rot, 2); }
            }
            Building(-11, -17, 0);
            Building(11, 15, 180);
            w.Block(adobe, 14, -12, 1.8f, 11, 1.7f, 35, 0, 2);
            w.Block(adobe, -9, 9, 1.6f, 11, 1.7f, 0, 0, 2);
            foreach (var c in new[] { new Vector3(4.2f, 0, 15.5f), new Vector3(4.2f, 1.2f, 15.6f), new Vector3(3, 0, 16.8f), new Vector3(-20, 0, 4), new Vector3(20, 0, -6), new Vector3(-2, 0, -30), new Vector3(2, 0, 30) })
                w.Block(crate, c.x, c.z, 1.2f, 1.2f, 1.2f, 0, c.y, 1.2f);
            // zipline rope + trolley between the roofs (decor)
            w.g.Rod(rope, new Vector3(-6, 4.4f, -11.5f), new Vector3(6, 4.4f, 9.5f), 0.03f, 6);
            w.g.Box(crate, new Vector3(0, 4.1f, -1), new Vector3(0.6f, 0.5f, 0.6f), Quaternion.Euler(0, -30, 0), 1);

            float[,] a = { { -22, -32 }, { -14, -34 }, { -26, -24 }, { -4, -33 }, { -25, -10 }, { -18, -26 } };
            for (int i = 0; i < a.GetLength(0); i++) { m.A.Add(SP(a[i, 0], a[i, 1])); m.B.Add(SP(-a[i, 0], -a[i, 1])); }
            m.bounds = Rect.MinMaxRect(-34, -44, 34, 44);
            w.Done(shadows);
        }

        // ------------------------------------------------------------ MAZE (picture 3)
        static void Maze(Map m, int seed, bool shadows)
        {
            var w = new W(m.root.transform);
            var gr = m.root.transform;
            m.sky = false;
            m.bg = Tex.Hex("f2e6c8"); m.fog = Tex.Hex("eadcb8"); m.fogStart = 50; m.fogEnd = 140;
            m.ambSky = Tex.Hex("fff3d6"); m.ambEq = Tex.Hex("d9c79a"); m.ambGround = Tex.Hex("9a8456");
            m.vmLight = new Color(1f, 0.95f, 0.85f);
            Sun(gr, Tex.Hex("fff6e0"), 1.15f, new Vector3(70, 25, 0), shadows);
            var wallM = Mats.Tex(Tex.MazeWall(), Color.white, 0.1f);
            var floorM = Mats.Tex(Tex.MazeFloor(), Color.white, 0.1f);
            var ceil = Mats.Lit(Tex.Hex("efe6d0"), 0.1f);
            var lamp = Mats.Glow(new Color(1.5f, 1.5f, 1.4f));
            const int CW = 10, CH = 16; const float C = 4;
            float ox = -CW * C / 2, oz = -CH * C / 2;
            Ground(gr, 200, floorM, 4);
            var r = new Rng(seed);
            // h[y,x]: wall on the north side of cell (x,y) (y = 0..CH); v[y,x]: wall on the west side (x = 0..CW)
            var hw = new bool[CH + 1, CW]; var vw = new bool[CH, CW + 1];
            for (int y = 0; y <= CH; y++) for (int x = 0; x < CW; x++) hw[y, x] = true;
            for (int y = 0; y < CH; y++) for (int x = 0; x <= CW; x++) vw[y, x] = true;
            var seen = new bool[CH, CW];
            var stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(0, 0)); seen[0, 0] = true;
            while (stack.Count > 0)
            {
                var c = stack.Peek();
                var opts = new List<Vector2Int>();
                foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var nb = c + d;
                    if (nb.x >= 0 && nb.y >= 0 && nb.x < CW && nb.y < CH && !seen[nb.y, nb.x]) opts.Add(nb);
                }
                if (opts.Count == 0) { stack.Pop(); continue; }
                var nx = opts[r.I(opts.Count)];
                if (nx.x != c.x) vw[c.y, Mathf.Max(c.x, nx.x)] = false; else hw[Mathf.Max(c.y, nx.y), c.x] = false;
                seen[nx.y, nx.x] = true;
                stack.Push(nx);
            }
            // extra openings → loops; open plaza in the middle
            for (int y = 1; y < CH; y++) for (int x = 0; x < CW; x++) if (r.Chance(0.2f)) hw[y, x] = false;
            for (int y = 0; y < CH; y++) for (int x = 1; x < CW; x++) if (r.Chance(0.2f)) vw[y, x] = false;
            for (int y = 7; y <= 8; y++) for (int x = 4; x <= 5; x++) { if (y > 7) hw[y, x] = false; if (x > 4) vw[y, x] = false; }
            const float WH = 3.2f, WT = 0.4f;
            for (int y = 0; y <= CH; y++)
            {
                int run = -1;
                for (int x = 0; x <= CW; x++)
                {
                    bool on = x < CW && hw[y, x];
                    if (on && run < 0) run = x;
                    if (!on && run >= 0) { w.Wall(wallM, new Vector2(ox + run * C, oz + y * C), new Vector2(ox + x * C, oz + y * C), (y == 0 || y == CH) ? 6 : WH, WT, 0, 3); run = -1; }
                }
            }
            for (int x = 0; x <= CW; x++)
            {
                int run = -1;
                for (int y = 0; y <= CH; y++)
                {
                    bool on = y < CH && vw[y, x];
                    if (on && run < 0) run = y;
                    if (!on && run >= 0) { w.Wall(wallM, new Vector2(ox + x * C, oz + run * C), new Vector2(ox + x * C, oz + y * C), (x == 0 || x == CW) ? 6 : WH, WT, 0, 3); run = -1; }
                }
            }
            // room ceiling with light panels
            w.g.Box(ceil, new Vector3(0, 9, 0), new Vector3(CW * C + 20, 0.3f, CH * C + 20), Quaternion.identity, 4);
            for (int i = 0; i < 3; i++) for (int j = 0; j < 4; j++) w.g.Box(lamp, new Vector3(-12 + i * 12, 8.8f, -24 + j * 16), new Vector3(5, 0.1f, 2.5f), Quaternion.identity);
            for (int i = 0; i < 5; i++)
            {
                var c = new Vector2Int(i * 2, CH - 1); m.A.Add(SP(ox + (c.x + 0.5f) * C, oz + (c.y + 0.5f) * C));
                var cb = new Vector2Int(CW - 1 - i * 2, 0); m.B.Add(SP(ox + (cb.x + 0.5f) * C, oz + (cb.y + 0.5f) * C));
            }
            m.bounds = Rect.MinMaxRect(ox - 1, oz - 1, -ox + 1, -oz + 1);
            w.Done(shadows);
        }

        // ------------------------------------------------------------ STADIUM (picture 4)
        static void Stadium(Map m, bool shadows)
        {
            var w = new W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = Tex.Hex("cfe0f0"); m.fogStart = 90; m.fogEnd = 260; m.bg = m.fog;
            m.ambSky = Tex.Hex("bcd6f2"); m.ambEq = Tex.Hex("a8c0a0"); m.ambGround = Tex.Hex("5a7a4a");
            Sun(gr, Tex.Hex("fff4e2"), 1.3f, new Vector3(55, 30, 0), shadows);
            var field = Mats.Tex(Tex.Field(), Color.white, 0.1f);
            var grass = Mats.Tex(Tex.Grass(), new Color(0.75f, 0.85f, 0.75f), 0.05f);
            var stand = Mats.Tex(Tex.Stands(), Color.white, 0.15f);
            var concrete = Mats.Tex(Tex.Concrete(), Color.white, 0.1f);
            var board = Mats.Emissive(Tex.Board(), Color.white, new Color(0.4f, 0.4f, 0.4f));
            var brown = Mats.Lit(Tex.Hex("6b3f22"), 0.2f);
            var post = Mats.Lit(Color.white, 0.6f, 0.2f);
            var net = Mats.Transparent(new Color(1, 1, 1, 0.35f));
            Ground(gr, 240, grass, 4);
            w.g.Mesh(field, Prim.MeshOf(PrimitiveType.Quad), Matrix4x4.TRS(new Vector3(0, 0.01f, 0), Quaternion.Euler(90, 0, 0), new Vector3(80, 54, 1)));
            // ad boards = walls around the pitch
            w.Block(board, 0, -25.5f, 77, 0.4f, 1.6f, 0, 0, 16); w.Block(board, 0, 25.5f, 77, 0.4f, 1.6f, 0, 0, 16);
            w.Block(board, -38.5f, 0, 0.4f, 51.4f, 1.6f, 0, 0, 16); w.Block(board, 38.5f, 0, 0.4f, 51.4f, 1.6f, 0, 0, 16);
            w.Solid(new Vector3(0, 4, -25.5f), new Vector3(78, 8, 0.4f)); w.Solid(new Vector3(0, 4, 25.5f), new Vector3(78, 8, 0.4f));
            w.Solid(new Vector3(-38.5f, 4, 0), new Vector3(0.4f, 8, 52)); w.Solid(new Vector3(38.5f, 4, 0), new Vector3(0.4f, 8, 52));
            // stepped stands
            for (int t = 0; t < 7; t++)
            {
                float off = 1.5f + t * 1.6f, h = 1.2f + t * 0.9f;
                w.Block(stand, 0, -26 - off, 80 + off * 2, 1.6f, h, 0, 0, 3, false);
                w.Block(stand, 0, 26 + off, 80 + off * 2, 1.6f, h, 0, 0, 3, false);
                w.Block(stand, -39 - off, 0, 1.6f, 52 + off * 2, h, 0, 0, 3, false);
                w.Block(stand, 39 + off, 0, 1.6f, 52 + off * 2, h, 0, 0, 3, false);
            }
            w.Block(concrete, 0, -40, 110, 2, 9, 0, 0, 4, false); w.Block(concrete, 0, 40, 110, 2, 9, 0, 0, 4, false);
            w.Block(concrete, -53, 0, 2, 80, 9, 0, 0, 4, false); w.Block(concrete, 53, 0, 2, 80, 9, 0, 0, 4, false);
            // goals
            foreach (int s in new[] { -1, 1 })
            {
                float gx = 34 * s;
                w.Trunk(post, new Vector3(gx, 0, -3.66f), 0.07f, 2.44f, 0.12f);
                w.Trunk(post, new Vector3(gx, 0, 3.66f), 0.07f, 2.44f, 0.12f);
                w.g.Rod(post, new Vector3(gx, 2.44f, -3.66f), new Vector3(gx, 2.44f, 3.66f), 0.07f, 8);
                w.Box(net, new Vector3(gx + 2 * s, 1.22f, 0), new Vector3(0.05f, 2.44f, 7.32f), 0, 2);
                w.Box(net, new Vector3(gx + 1 * s, 1.22f, -3.66f), new Vector3(2, 2.44f, 0.05f), 0, 2);
                w.Box(net, new Vector3(gx + 1 * s, 1.22f, 3.66f), new Vector3(2, 2.44f, 0.05f), 0, 2);
                w.g.Box(net, new Vector3(gx + 1 * s, 2.44f, 0), new Vector3(2, 0.05f, 7.32f), Quaternion.identity);
            }
            // cover blocks (brown, like the picture)
            w.Block(brown, -13, -12, 1.6f, 5, 1.8f, 20);
            w.Block(brown, 0, -2, 1.6f, 5, 1.8f, -30);
            w.Block(brown, 13, -14, 1.6f, 5, 1.8f, 60);
            w.Block(brown, -3, 11, 5, 1.6f, 1.8f, 0);
            w.Block(brown, 18, 9, 1.6f, 5, 1.8f, 0);
            w.Block(brown, -20, 6, 5, 1.6f, 1.8f, 15);
            m.ball = Ball.Create(gr);
            for (int i = 0; i < 6; i++) { m.A.Add(SP(-31, -15 + i * 6, 0, -15 + i * 6)); m.B.Add(SP(31, -15 + i * 6, 0, -15 + i * 6)); }
            m.bounds = Rect.MinMaxRect(-38, -25, 38, 25);
            w.Done(shadows);
        }

        // ------------------------------------------------------------ FOREST (picture 5)
        static void Forest(Map m, int seed, bool shadows)
        {
            var w = new W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = Tex.Hex("cfe6d8"); m.fogStart = 45; m.fogEnd = 160; m.bg = m.fog;
            m.ambSky = Tex.Hex("bfe0ff"); m.ambEq = Tex.Hex("8fb27a"); m.ambGround = Tex.Hex("3a5a2a");
            Sun(gr, Tex.Hex("fff3dc"), 1.25f, new Vector3(50, -40, 0), shadows);
            var grass = Mats.Tex(Tex.Grass(), Color.white, 0.05f);
            var bark = Mats.Tex(Tex.Bark(), Color.white, 0.1f);
            var leaves = Mats.Tex(Tex.Leaves(), Color.white, 0.1f);
            var planks = Mats.Tex(Tex.Planks("6e4628"), Color.white, 0.15f);
            var fabric = Mats.Tex(Tex.Fabric(), Color.white, 0.2f);
            Ground(gr, 260, grass, 5);
            var r = new Rng(seed);
            void Tree(float x, float z, float s)
            {
                w.Trunk(bark, new Vector3(x, 0, z), 0.22f * s, 2.0f * s, 0.4f);
                w.g.Cyl(leaves, new Vector3(x, 1.1f * s, z), 1.9f * s, 2.4f * s, 9, 0);
                w.g.Cyl(leaves, new Vector3(x, 2.4f * s, z), 1.5f * s, 2.1f * s, 9, 0);
                w.g.Cyl(leaves, new Vector3(x, 3.6f * s, z), 1.05f * s, 1.8f * s, 9, 0);
            }
            var placed = new List<Vector2>();
            for (int i = 0; i < 400 && placed.Count < 46; i++)
            {
                var p = new Vector2(r.F(-30, 30), r.F(-30, 30));
                if (p.magnitude < 9) continue;
                if (Vector2.Distance(p, new Vector2(-25, 25)) < 7 || Vector2.Distance(p, new Vector2(25, -25)) < 7) continue;
                bool near = false;
                foreach (var q in placed) if (Vector2.Distance(p, q) < 4.2f) { near = true; break; }
                if (near) continue;
                placed.Add(p);
                Tree(p.x, p.y, r.F(0.85f, 1.25f));
            }
            // border: dense trees + invisible walls
            for (float t = -36; t <= 36; t += 4.5f)
            {
                Tree(t, -36, 1.2f); Tree(t, 36, 1.2f); Tree(-36, t, 1.2f); Tree(36, t, 1.2f);
            }
            w.Solid(new Vector3(0, 5, -34.5f), new Vector3(74, 10, 1)); w.Solid(new Vector3(0, 5, 34.5f), new Vector3(74, 10, 1));
            w.Solid(new Vector3(-34.5f, 5, 0), new Vector3(1, 10, 74)); w.Solid(new Vector3(34.5f, 5, 0), new Vector3(1, 10, 74));
            void Fence(float x, float z, float yaw)
            {
                w.Block(planks, x, z, 7.5f, 0.35f, 3.6f, yaw, 0, 3.6f);
                var q = Quaternion.Euler(0, yaw, 0);
                foreach (int s in new[] { -1, 1 }) { var p = new Vector3(x, 0, z) + q * new Vector3(s * 3.2f, 0, -0.6f); w.g.Box(planks, p + Vector3.up * 1.2f, new Vector3(0.15f, 2.6f, 0.15f), Quaternion.Euler(-25, yaw, 0), 2); }
            }
            Fence(-17, -21, 5); Fence(20, -22, 80); Fence(-23, 5, 92); Fence(14, 14, -35);
            // tent in the centre
            w.g.Cyl(fabric, new Vector3(0, 0, 0), 2.6f, 2.8f, 4, 0, true, 2, Quaternion.Euler(0, 45, 0));
            w.g.Box(Mats.Lit(Tex.Hex("2a1a08"), 0.1f), new Vector3(0, 0.7f, 1.32f), new Vector3(1.0f, 1.4f, 0.05f), Quaternion.identity);
            w.Solid(new Vector3(0, 1, 0), new Vector3(3.4f, 2, 3.4f));
            // first-aid kits (red pads around the tent)
            var red = Mats.Lit(Tex.Hex("d42a2a"), 0.4f);
            var white = Mats.Glow(Color.white);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4;
                var p = new Vector3(Mathf.Cos(a) * 5f, 0, Mathf.Sin(a) * 5f);
                var go = new GameObject("Medkit");
                go.transform.SetParent(gr, false);
                go.transform.position = p;
                Prim.Make(PrimitiveType.Cylinder, go.transform, new Vector3(0, 0.12f, 0), new Vector3(0.9f, 0.1f, 0.9f), red);
                Prim.Make(PrimitiveType.Cube, go.transform, new Vector3(0, 0.23f, 0), new Vector3(0.5f, 0.02f, 0.14f), white);
                Prim.Make(PrimitiveType.Cube, go.transform, new Vector3(0, 0.23f, 0), new Vector3(0.14f, 0.02f, 0.5f), white);
                m.pickups.Add(new Pickup { id = i, pos = p, go = go });
                var tr = go.transform;
                m.tick += dt => { if (tr.gameObject.activeSelf) tr.localRotation = Quaternion.Euler(0, Time.time * 60, 0); };
            }
            float[,] a2 = { { -26, 24 }, { -20, 28 }, { -28, 16 }, { -14, 27 }, { -24, 10 }, { -10, 30 } };
            for (int i = 0; i < a2.GetLength(0); i++) { m.A.Add(SP(a2[i, 0], a2[i, 1])); m.B.Add(SP(-a2[i, 0], -a2[i, 1])); }
            m.bounds = Rect.MinMaxRect(-34, -34, 34, 34);
            w.Done(shadows);
        }
    }
}
