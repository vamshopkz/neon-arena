using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>Tiny software canvas for procedural textures (no image files needed).</summary>
    public class Canvas2
    {
        public readonly int W, H;
        public readonly Color[] px;
        public Canvas2(int w, int h, Color fill) { W = w; H = h; px = new Color[w * h]; for (int i = 0; i < px.Length; i++) px[i] = fill; }

        public void Blend(int x, int y, Color c)
        {
            x = ((x % W) + W) % W; y = ((y % H) + H) % H;   // wraps: textures tile seamlessly
            int i = y * W + x;
            float a = c.a;
            var o = px[i];
            px[i] = new Color(o.r + (c.r - o.r) * a, o.g + (c.g - o.g) * a, o.b + (c.b - o.b) * a, 1);
        }
        public void Set(int x, int y, Color c) { x = ((x % W) + W) % W; y = ((y % H) + H) % H; px[y * W + x] = c; }
        public Color Get(int x, int y) { x = ((x % W) + W) % W; y = ((y % H) + H) % H; return px[y * W + x]; }

        public void Rect(float x, float y, float w, float h, Color c)
        {
            for (int j = Mathf.FloorToInt(y); j < Mathf.CeilToInt(y + h); j++)
                for (int i = Mathf.FloorToInt(x); i < Mathf.CeilToInt(x + w); i++) Blend(i, j, c);
        }

        public void Circle(float cx, float cy, float r, Color c, float soft = 1f)
        {
            for (int j = Mathf.FloorToInt(cy - r - 1); j <= Mathf.CeilToInt(cy + r + 1); j++)
                for (int i = Mathf.FloorToInt(cx - r - 1); i <= Mathf.CeilToInt(cx + r + 1); i++)
                {
                    float d = Mathf.Sqrt((i + 0.5f - cx) * (i + 0.5f - cx) + (j + 0.5f - cy) * (j + 0.5f - cy));
                    float a = Mathf.Clamp01((r - d) / soft + 0.5f);
                    if (a > 0) Blend(i, j, new Color(c.r, c.g, c.b, c.a * a));
                }
        }

        public void Ring(float cx, float cy, float r, float w, Color c)
        {
            for (int j = Mathf.FloorToInt(cy - r - w); j <= Mathf.CeilToInt(cy + r + w); j++)
                for (int i = Mathf.FloorToInt(cx - r - w); i <= Mathf.CeilToInt(cx + r + w); i++)
                {
                    float d = Mathf.Abs(Mathf.Sqrt((i + 0.5f - cx) * (i + 0.5f - cx) + (j + 0.5f - cy) * (j + 0.5f - cy)) - r);
                    float a = Mathf.Clamp01(w * 0.5f - d + 0.5f);
                    if (a > 0) Blend(i, j, new Color(c.r, c.g, c.b, c.a * a));
                }
        }

        public void Line(float x0, float y0, float x1, float y1, float w, Color c)
        {
            float len = Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            int n = Mathf.Max(1, Mathf.CeilToInt(len / Mathf.Max(0.5f, w * 0.35f)));
            for (int k = 0; k <= n; k++) { float t = (float)k / n; Circle(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, w * 0.5f, c); }
        }

        public void Noise(Rng r, float amount)
        {
            for (int i = 0; i < px.Length; i++)
            {
                float n = (r.F() - 0.5f) * amount;
                var c = px[i];
                px[i] = new Color(c.r + n, c.g + n, c.b + n, 1);
            }
        }

        public Texture2D Tex(bool repeat = true, bool mip = true, FilterMode f = FilterMode.Trilinear)
        {
            var t = new Texture2D(W, H, TextureFormat.RGBA32, mip) { wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp, filterMode = f, anisoLevel = 4 };
            t.SetPixels(px);
            t.Apply(mip, true);
            return t;
        }
    }

    public static class Tex
    {
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
        static Texture2D C(string key, System.Func<Texture2D> make)
        {
            if (cache.TryGetValue(key, out var t) && t != null) return t;
            t = make(); t.name = key; cache[key] = t;
            return t;
        }
        public static Color Hex(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }

        /// <summary>Smooth value noise (tileable over <paramref name="period"/> cells).</summary>
        public static float VNoise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            float R(int a, int b) { a = ((a % period) + period) % period; b = ((b % period) + period) % period; uint h = (uint)(a * 374761393 + b * 668265263 + seed * 2246822519); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; }
            return Mathf.Lerp(Mathf.Lerp(R(x0, y0), R(x0 + 1, y0), fx), Mathf.Lerp(R(x0, y0 + 1), R(x0 + 1, y0 + 1), fx), fy);
        }

        public static float Fbm(float x, float y, int period, int seed, int oct = 4)
        {
            float s = 0, a = 0.5f, norm = 0;
            for (int o = 0; o < oct; o++) { s += VNoise(x, y, period, seed + o * 31) * a; norm += a; x *= 2; y *= 2; period *= 2; a *= 0.5f; }
            return s / norm;
        }

        // ---------------- world ----------------

        public static Texture2D Grid(string bg, string line, int cells = 4) => C("grid" + bg + line + cells, () =>
        {
            var cv = new Canvas2(256, 256, Hex(bg));
            var l = Hex(line);
            int s = 256 / cells;
            for (int i = 0; i < cells; i++)
            {
                cv.Rect(i * s - 1, 0, 3, 256, new Color(l.r, l.g, l.b, 0.35f));
                cv.Rect(0, i * s - 1, 256, 3, new Color(l.r, l.g, l.b, 0.35f));
                cv.Rect(i * s, 0, 1, 256, l);
                cv.Rect(0, i * s, 256, 1, l);
            }
            cv.Noise(new Rng(3), 0.03f);
            return cv.Tex();
        });

        public static Texture2D NeonWall() => C("nwall", () =>
        {
            var cv = new Canvas2(256, 256, Hex("141836"));
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++) { float k = y / 255f; cv.px[y * 256 + x] = Color.Lerp(Hex("0f122c"), Hex("1c2252"), k); }
            cv.Rect(6, 6, 244, 3, Hex("2c3478")); cv.Rect(6, 247, 244, 3, Hex("2c3478"));
            cv.Rect(6, 6, 3, 244, Hex("2c3478")); cv.Rect(247, 6, 3, 244, Hex("2c3478"));
            for (int y = 50; y < 240; y += 48) cv.Rect(12, y, 232, 1, Hex("3a46a0"));
            var cy = Hex("5fd4ff");
            cv.Line(0, 36, 90, 36, 3, cy); cv.Line(90, 36, 108, 56, 3, cy); cv.Line(108, 56, 256, 56, 3, cy);
            cv.Noise(new Rng(5), 0.025f);
            return cv.Tex();
        });

        public static Texture2D Stars() => C("stars", () =>
        {
            var r = new Rng(11);
            var cv = new Canvas2(512, 512, Hex("060a22"));
            for (int y = 0; y < 512; y++) for (int x = 0; x < 512; x++)
                {
                    float n = Fbm(x / 64f, y / 64f, 8, 7);
                    float m = Fbm(x / 48f + 9, y / 48f, 10, 3);
                    var c = Hex("060a22");
                    c = Color.Lerp(c, Hex("3a1a8a"), Mathf.Clamp01((n - 0.45f) * 2.4f));
                    c = Color.Lerp(c, Hex("1f5fbf"), Mathf.Clamp01((m - 0.55f) * 2.4f) * 0.8f);
                    cv.px[y * 512 + x] = c;
                }
            for (int i = 0; i < 700; i++) { float s = r.F() < 0.1f ? 1.6f : 0.7f; cv.Circle(r.F() * 512, r.F() * 512, s, new Color(1, 1, 1, r.F(0.5f, 1f))); }
            var lightning = Hex("b56bff");
            for (int k = 0; k < 5; k++)
            {
                float x = r.F() * 512, y = r.F() * 512;
                for (int s = 0; s < 14; s++) { float nx = x + r.F(-30, 30), ny = y + r.F(-30, 30); cv.Line(x, y, nx, ny, 2, new Color(lightning.r, lightning.g, lightning.b, 0.6f)); x = nx; y = ny; }
            }
            return cv.Tex();
        });

        static Texture2D Speckle(string key, string bas, string[] cols, int n, float smin, float smax, float noise, int seed) => C(key, () =>
        {
            var r = new Rng(seed);
            var cv = new Canvas2(256, 256, Hex(bas));
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++)
                {
                    float f = Fbm(x / 32f, y / 32f, 8, seed) - 0.5f;
                    var c = cv.px[y * 256 + x];
                    cv.px[y * 256 + x] = new Color(c.r + f * 0.12f, c.g + f * 0.12f, c.b + f * 0.1f, 1);
                }
            for (int i = 0; i < n; i++) { var c = Hex(cols[r.I(cols.Length)]); c.a = r.F(0.3f, 0.8f); cv.Circle(r.F() * 256, r.F() * 256, r.F(smin, smax), c); }
            cv.Noise(r, noise);
            return cv.Tex();
        });

        public static Texture2D Sand() => Speckle("sand", "d9c08f", new[] { "c4a46e", "ead3a3", "b8935c" }, 900, 0.6f, 1.8f, 0.05f, 21);
        public static Texture2D Adobe() => Speckle("adobe", "c99f72", new[] { "b78a5f", "d8b588", "a77a50" }, 600, 0.8f, 2.4f, 0.04f, 22);
        public static Texture2D MazeWall() => Speckle("mazewall", "e3cb8c", new[] { "d6bb78", "efdba4" }, 300, 1f, 3f, 0.025f, 23);
        public static Texture2D MazeFloor() => Speckle("mazefloor", "c9ad73", new[] { "b89a60", "d8be86" }, 700, 0.6f, 1.6f, 0.04f, 24);
        public static Texture2D Concrete() => Speckle("concrete", "8f949b", new[] { "7d8288", "a2a7ad", "6c7177" }, 900, 0.5f, 1.5f, 0.05f, 25);

        public static Texture2D Sandstone() => C("sandstone", () =>
        {
            var r = new Rng(31);
            var cv = new Canvas2(256, 256, Hex("d6bd8e"));
            for (int y = 0; y < 256; y++)
            {
                float band = Mathf.Sin(y * 0.19f + Mathf.Sin(y * 0.05f) * 2f) * 0.05f + Fbm(0, y / 16f, 16, 4) * 0.08f;
                for (int x = 0; x < 256; x++)
                {
                    float f = Fbm(x / 32f, y / 12f, 8, 9) * 0.1f;
                    var c = Hex("d6bd8e");
                    cv.px[y * 256 + x] = new Color(c.r - band - f, c.g - band - f * 1.1f, c.b - band * 1.2f - f, 1);
                }
            }
            for (int i = 0; i < 18; i++) { float x = r.F() * 256, y = r.F() * 256; cv.Line(x, y, x + r.F(-20, 20), y + r.F(-40, 0), 1.2f, new Color(0.45f, 0.35f, 0.22f, 0.35f)); }
            return cv.Tex();
        });

        public static Texture2D Grass() => C("grass", () =>
        {
            var r = new Rng(41);
            var cv = new Canvas2(256, 256, Hex("4f9a2f"));
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++)
                {
                    float f = Fbm(x / 40f, y / 40f, 6, 12) - 0.5f;
                    var c = Hex("4c962d");
                    cv.px[y * 256 + x] = new Color(c.r + f * 0.15f, c.g + f * 0.22f, c.b + f * 0.08f, 1);
                }
            string[] cols = { "6fbf3f", "3c7f22", "8ad04e", "2f6a1a" };
            for (int i = 0; i < 2200; i++)
            {
                float x = r.F() * 256, y = r.F() * 256;
                var c = Hex(cols[r.I(cols.Length)]); c.a = 0.7f;
                cv.Line(x, y, x + r.F(-2, 2), y + r.F(3, 7), 1f, c);
            }
            return cv.Tex();
        });

        public static Texture2D Planks(string bas = "7a4a24") => C("planks" + bas, () =>
        {
            var r = new Rng(51);
            var b = Hex(bas);
            var cv = new Canvas2(256, 256, b);
            int n = 6, w = 256 / n;
            for (int p = 0; p < n; p++)
            {
                float k = r.F(-0.08f, 0.08f);
                for (int y = 0; y < 256; y++) for (int x = p * w; x < (p + 1) * w; x++)
                    {
                        float g = Mathf.Sin((x + Mathf.Sin(y * 0.05f + p) * 3) * 0.9f) * 0.03f + Fbm(x / 8f, y / 48f, 32, p) * 0.12f;
                        cv.px[y * 256 + x] = new Color(b.r + k - g, b.g + k * 0.8f - g * 0.9f, b.b + k * 0.5f - g * 0.6f, 1);
                    }
                cv.Rect(p * w, 0, 2, 256, new Color(0.15f, 0.08f, 0.03f, 0.9f));
                cv.Circle(p * w + w / 2f, 30, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                cv.Circle(p * w + w / 2f, 226, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
            }
            return cv.Tex();
        });

        public static Texture2D Bark() => C("bark", () =>
        {
            var cv = new Canvas2(128, 128, Hex("5a3a22"));
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    float f = Fbm(x / 6f, y / 40f, 22, 77);
                    var c = Hex("5a3a22");
                    cv.px[y * 128 + x] = new Color(c.r * (0.6f + f * 0.7f), c.g * (0.6f + f * 0.7f), c.b * (0.6f + f * 0.7f), 1);
                }
            return cv.Tex();
        });

        public static Texture2D Leaves() => Speckle("leaves", "2f7a35", new[] { "3f9a45", "235e28", "4caf52" }, 1400, 0.8f, 2.4f, 0.05f, 61);

        public static Texture2D Fabric() => C("fabric", () =>
        {
            var cv = new Canvas2(128, 128, Hex("f2c230"));
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    float w = ((x + y) % 4 < 2 ? 0.03f : -0.03f) + (Fbm(x / 16f, y / 16f, 8, 5) - 0.5f) * 0.1f;
                    var c = Hex("f2c230");
                    cv.px[y * 128 + x] = new Color(c.r + w, c.g + w, c.b + w * 0.5f, 1);
                }
            return cv.Tex();
        });

        /// <summary>Football pitch: 80×54 m texture (mown stripes + white lines).</summary>
        public static Texture2D Field() => C("field", () =>
        {
            const float P = 12f; int W = (int)(80 * P), H = (int)(54 * P);
            var cv = new Canvas2(W, H, Hex("47963f"));
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) cv.px[y * W + x] = ((int)(x / (P * 5)) % 2 == 0) ? Hex("4a9a42") : Hex("418c3a");
            var wl = new Color(0.96f, 0.96f, 0.96f, 1);
            float X(float x) => (x + 40) * P; float Y(float z) => (z + 27) * P;
            void L(float x0, float z0, float x1, float z1) => cv.Line(X(x0), Y(z0), X(x1), Y(z1), 0.14f * P, wl);
            L(-34, -22, 34, -22); L(-34, 22, 34, 22); L(-34, -22, -34, 22); L(34, -22, 34, 22); L(0, -22, 0, 22);
            cv.Ring(X(0), Y(0), 9.15f * P, 0.14f * P, wl); cv.Circle(X(0), Y(0), 0.3f * P, wl);
            foreach (int s in new[] { -1, 1 })
            {
                float g = 34 * s;
                L(g, -20.15f, g - 16.5f * s, -20.15f); L(g - 16.5f * s, -20.15f, g - 16.5f * s, 20.15f); L(g - 16.5f * s, 20.15f, g, 20.15f);
                L(g, -9.15f, g - 5.5f * s, -9.15f); L(g - 5.5f * s, -9.15f, g - 5.5f * s, 9.15f); L(g - 5.5f * s, 9.15f, g, 9.15f);
                cv.Circle(X(g - 11 * s), Y(0), 0.3f * P, wl);
            }
            return cv.Tex(false);
        });

        public static Texture2D Ball() => C("ball", () =>
        {
            var cv = new Canvas2(256, 128, Color.white);
            float phi = (1 + Mathf.Sqrt(5)) / 2;
            var verts = new List<Vector3>();
            foreach (var a in new[] { -1, 1 }) foreach (var b in new[] { -1, 1 })
                { verts.Add(new Vector3(0, a, b * phi)); verts.Add(new Vector3(a, b * phi, 0)); verts.Add(new Vector3(b * phi, 0, a)); }
            foreach (var v0 in verts)
            {
                var v = v0.normalized;
                float u = Mathf.Atan2(v.z, v.x) / (2 * Mathf.PI) + 0.5f, w = Mathf.Acos(v.y) / Mathf.PI;
                float sin = Mathf.Max(0.25f, Mathf.Sin(w * Mathf.PI));
                for (int dy = -14; dy <= 14; dy++) for (int dx = -40; dx <= 40; dx++)
                    {
                        float ex = dx * sin / 14f, ey = dy / 14f;
                        if (ex * ex + ey * ey <= 1) cv.Set((int)(u * 256) + dx, (int)((1 - w) * 128) + dy, Hex("151515"));
                    }
            }
            return cv.Tex(true, true);
        });

        public static Texture2D Target() => C("target", () =>
        {
            var cv = new Canvas2(128, 128, new Color(0, 0, 0, 0));
            for (int i = 6; i >= 1; i--) cv.Circle(64, 64, i * 10.5f, i % 2 == 0 ? Hex("e8e8e8") : Hex("e0283c"));
            return cv.Tex(false);
        });

        public static Texture2D Board() => C("board", () =>
        {
            var cv = new Canvas2(256, 64, Hex("1d2a7a"));
            string[] cols = { "ff3d6e", "3dd2ff", "ffe23d", "7cff5a" };
            for (int i = 0; i < 4; i++) cv.Rect(i * 64, 0, 62, 64, Hex(cols[i]));
            cv.Noise(new Rng(2), 0.03f);
            return cv.Tex();
        });

        public static Texture2D Stands() => C("stands", () =>
        {
            var r = new Rng(91);
            var cv = new Canvas2(128, 128, Hex("8a8f96"));
            for (int y = 0; y < 128; y += 16) cv.Rect(0, y, 128, 3, Hex("5d6268"));
            string[] seat = { "c03030", "2f5fbf", "e8c030", "8a8f96" };
            for (int y = 4; y < 128; y += 16) for (int x = 2; x < 128; x += 8) if (r.F() < 0.8f) cv.Rect(x, y, 6, 9, Hex(seat[r.I(seat.Length)]));
            return cv.Tex();
        });

        public static Texture2D Glow() => C("glow", () =>
        {
            var cv = new Canvas2(64, 64, new Color(1, 1, 1, 0));
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32)) / 32f;
                    cv.px[y * 64 + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2));
                }
            return cv.Tex(false);
        });

        // ---------------- weapon skins ----------------

        public static Texture2D Skin(string id) => C("skin_" + id, () => MakeSkin(id));

        static Texture2D MakeSkin(string id)
        {
            const int S = 256;
            var r = new Rng(id.GetHashCode());
            var cv = new Canvas2(S, S, Color.gray);
            switch (id)
            {
                case "digi":
                {
                    string[] cols = { "4b5a3a", "6d7a52", "2e3826", "8c8f6a" };
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex(cols[0]);
                    for (int k = 0; k < 340; k++)
                    {
                        int x = r.I(S / 8) * 8, y = r.I(S / 8) * 8;
                        cv.Rect(x, y, (2 + r.I(5)) * 8, (1 + r.I(3)) * 8, Hex(cols[1 + r.I(3)]));
                    }
                    break;
                }
                case "wood":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float g = Mathf.Sin((y + Fbm(x / 32f, y / 32f, 8, 3) * 40) * 0.45f) * 0.5f + 0.5f;
                            cv.px[y * S + x] = Color.Lerp(Hex("8a5a2b"), Hex("b07a3f"), g * 0.6f);
                        }
                    for (int k = 0; k < 14; k++)
                    {
                        float cx = r.F() * S, cy = r.F() * S, rad = r.F(10, 26);
                        cv.Ring(cx, cy, rad, 2.2f, new Color(0.22f, 0.12f, 0.05f, 0.85f));
                        cv.Ring(cx, cy, rad * 0.55f, 1.6f, new Color(0.22f, 0.12f, 0.05f, 0.8f));
                        cv.Line(cx + rad, cy, cx + rad + 18, cy + r.F(-14, 14), 1.6f, new Color(0.22f, 0.12f, 0.05f, 0.8f));
                    }
                    break;
                case "pixel":
                {
                    string[] cols = { "ff3d6e", "3dd2ff", "ffe23d", "7cff5a", "b43dff", "ff8a3d", "ffffff", "1b1b3a" };
                    for (int y = 0; y < S; y += 16) for (int x = 0; x < S; x += 16) cv.Rect(x, y, 16, 16, Hex(cols[r.I(cols.Length)]));
                    break;
                }
                case "sticker":
                {
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("e9e9e9");
                    string[] cols = { "ff3d6e", "3dd2ff", "ffe23d", "7cff5a", "b43dff", "ff8a3d", "1b1b3a" };
                    for (int k = 0; k < 60; k++)
                    {
                        var c = Hex(cols[r.I(cols.Length)]);
                        float x = r.F() * S, y = r.F() * S, w = r.F(14, 46), h = r.F(10, 30);
                        cv.Rect(x - 2, y - 2, w + 4, h + 4, Color.white);
                        cv.Rect(x, y, w, h, c);
                        if (r.F() < 0.5f) cv.Circle(x + w / 2, y + h / 2, Mathf.Min(w, h) * 0.3f, Hex(cols[r.I(cols.Length)]));
                        else cv.Rect(x + 4, y + h / 2 - 2, w - 8, 4, Color.white);
                    }
                    break;
                }
                case "circuit":
                {
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("0f4d2a");
                    var gold = Hex("d8b44a"); var green = Hex("5fe08a");
                    for (int k = 0; k < 70; k++)
                    {
                        float x = r.I(S / 8) * 8, y = r.I(S / 8) * 8;
                        var c = r.F() < 0.6f ? gold : green;
                        cv.Circle(x, y, 3, c);
                        for (int s = 0; s < 4; s++)
                        {
                            float len = r.I(5) * 8 + 8;
                            int dir = r.I(4);
                            float nx = x + (dir == 0 ? len : dir == 1 ? -len : 0), ny = y + (dir == 2 ? len : dir == 3 ? -len : 0);
                            cv.Line(x, y, nx, ny, 2, c); x = nx; y = ny;
                        }
                        cv.Circle(x, y, 3.5f, c);
                    }
                    for (int k = 0; k < 8; k++) cv.Rect(r.F() * S, r.F() * S, 22, 14, Hex("1a1a1a"));
                    break;
                }
                case "jade":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float f = Fbm(x / 40f, y / 40f, 6, 5);
                            float vein = Mathf.Abs(Mathf.Sin((x + f * 120) * 0.05f + y * 0.02f));
                            var c = Color.Lerp(Hex("1f7d55"), Hex("49c08a"), f);
                            cv.px[y * S + x] = Color.Lerp(Hex("c8f5dc"), c, Mathf.Clamp01(vein * 6));
                        }
                    break;
                case "lava":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float f = Fbm(x / 32f, y / 32f, 8, 13);
                            float crack = 1 - Mathf.Clamp01(Mathf.Abs(f - 0.5f) * 14);
                            var c = Color.Lerp(Hex("1a0a06"), Hex("3a160a"), Fbm(x / 16f, y / 16f, 16, 2));
                            c = Color.Lerp(c, Hex("ff6a00"), crack);
                            c = Color.Lerp(c, Hex("ffd040"), Mathf.Clamp01(crack * 2 - 1.3f));
                            cv.px[y * S + x] = c;
                        }
                    break;
                case "damascus":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float v = Mathf.Sin(x * 0.12f + Mathf.Sin(y * 0.05f) * 3 + Fbm(x / 32f, y / 32f, 8, 3) * 6);
                            float g = 0.45f + v * 0.18f;
                            cv.px[y * S + x] = new Color(g, g * 1.01f, g * 1.04f, 1);
                        }
                    break;
                case "obsidian":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float f = Fbm(x / 48f, y / 48f, 6, 9);
                            cv.px[y * S + x] = Color.Lerp(Hex("07060b"), Hex("3b2363"), Mathf.Pow(f, 3) * 2.2f);
                        }
                    for (int k = 0; k < 20; k++) { float x = r.F() * S, y = r.F() * S; cv.Line(x, y, x + r.F(-40, 40), y + r.F(-40, 40), 1.2f, new Color(0.55f, 0.4f, 0.85f, 0.5f)); }
                    break;
                case "skull":
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("1c1c1c");
                    for (int gy = 0; gy < 4; gy++) for (int gx = 0; gx < 4; gx++)
                        {
                            float cx = gx * 64 + 32 + (gy % 2) * 16, cy = gy * 64 + 36;
                            var w = Hex("e8e8e8");
                            cv.Circle(cx, cy + 4, 17, w);
                            cv.Rect(cx - 10, cy - 18, 20, 12, w);
                            cv.Circle(cx - 7, cy + 4, 5.5f, Hex("1c1c1c"));
                            cv.Circle(cx + 7, cy + 4, 5.5f, Hex("1c1c1c"));
                            cv.Rect(cx - 1.5f, cy - 6, 3, 5, Hex("1c1c1c"));
                            for (int t = -2; t <= 2; t++) cv.Rect(cx + t * 4 - 1, cy - 18, 1.5f, 8, Hex("1c1c1c"));
                        }
                    break;
                case "carbon":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            int cx = x / 8, cy = y / 8;
                            bool a = (cx + cy) % 2 == 0;
                            float t = a ? (x % 8) / 8f : (y % 8) / 8f;
                            float g = 0.09f + t * 0.12f;
                            cv.px[y * S + x] = new Color(g, g, g * 1.1f, 1);
                        }
                    break;
                case "rainbow":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                            cv.px[y * S + x] = Color.HSVToRGB(((x + y) / (2f * S) + Fbm(x / 64f, y / 64f, 4, 2) * 0.15f) % 1f, 0.75f, 1f);
                    break;
                case "cosmic":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float n = Fbm(x / 40f, y / 40f, 6, 4), m = Fbm(x / 30f + 5, y / 30f, 8, 8);
                            var c = Hex("0a0420");
                            c = Color.Lerp(c, Hex("b0279a"), Mathf.Clamp01((n - 0.45f) * 3));
                            c = Color.Lerp(c, Hex("2a6bff"), Mathf.Clamp01((m - 0.5f) * 3));
                            cv.px[y * S + x] = c;
                        }
                    for (int k = 0; k < 160; k++) cv.Circle(r.F() * S, r.F() * S, r.F() < 0.1f ? 1.6f : 0.8f, Color.white);
                    break;
                case "golden":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float t = Mathf.Sin((x + y) * 0.03f) * 0.5f + 0.5f;
                            var c = Color.Lerp(Hex("c9952b"), Hex("ffe08a"), t);
                            float f = Fbm(x / 20f, y / 20f, 12, 6);
                            cv.px[y * S + x] = Color.Lerp(c, Hex("a87418"), Mathf.Clamp01((f - 0.6f) * 3) * 0.5f);
                        }
                    break;
                case "tiger":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                            cv.px[y * S + x] = Color.Lerp(Hex("f08a1c"), Hex("ffc45a"), Fbm(x / 64f, y / 64f, 4, 3) * 0.8f);
                    for (int k = 0; k < 14; k++)
                    {
                        float x0 = k * S / 14f + r.F(-6, 6), ph = r.F(0, 6), w = r.F(5, 11);
                        float y0 = r.F(0, S * 0.3f), y1 = y0 + r.F(S * 0.35f, S * 0.75f);
                        for (float y = y0; y < y1; y += 1.5f)
                        {
                            float t = (y - y0) / (y1 - y0);
                            float ww = w * Mathf.Sin(t * Mathf.PI) + 1;
                            cv.Circle(x0 + Mathf.Sin(y * 0.05f + ph) * 10, y, ww * 0.5f, Hex("140a04"));
                        }
                    }
                    break;
                case "flames":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float v = y / (float)S;
                            float f = Fbm(x / 28f, y / 18f, 9, 21) * 1.25f - v * 0.95f + 0.25f;
                            Color c = Hex("0d0606");
                            if (f > 0.25f) c = Color.Lerp(Hex("8a0d05"), Hex("ff3d00"), Mathf.Clamp01((f - 0.25f) * 6));
                            if (f > 0.42f) c = Color.Lerp(Hex("ff7a00"), Hex("ffe066"), Mathf.Clamp01((f - 0.42f) * 6));
                            if (f > 0.58f) c = Color.Lerp(Hex("ffe066"), Color.white, Mathf.Clamp01((f - 0.58f) * 5));
                            cv.px[y * S + x] = c;
                        }
                    break;
                case "dragon":
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("0c2a14");
                    for (int row = 13; row >= -1; row--)
                        for (int col = -1; col < 13; col++)
                        {
                            float cx = col * 22 + (row % 2) * 11, cy = row * 18;
                            for (int j = 0; j < 12; j++) cv.Circle(cx, cy - j * 0.4f, 12 - j * 0.9f, Color.Lerp(Hex("1d6b35"), Hex("7fd36a"), j / 11f));
                            cv.Ring(cx, cy, 11.5f, 1.6f, Hex("d6a83a"));
                        }
                    cv.Circle(180, 90, 30, Hex("ffcc33")); cv.Circle(180, 90, 22, Hex("ff8800"));
                    cv.Rect(176, 64, 8, 52, Hex("120800")); cv.Ring(180, 90, 30, 3, Hex("3a2000"));
                    break;
                case "neon":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++) cv.px[y * S + x] = Color.Lerp(Hex("05031a"), Hex("140a35"), y / (float)S);
                    for (int k = 0; k < 12; k++)
                    {
                        var c = k % 2 == 0 ? Hex("29f0ff") : Hex("ff2fd2");
                        float x = r.F() * S, y = r.F() * S;
                        for (int s = 0; s < 5; s++)
                        {
                            int dir = r.I(4); float len = r.F(20, 60);
                            float nx = x + (dir == 0 ? len : dir == 1 ? -len : len * 0.7f), ny = y + (dir == 2 ? len : dir == 3 ? -len : len * 0.7f * (s % 2 == 0 ? 1 : -1));
                            cv.Line(x, y, nx, ny, 9, new Color(c.r, c.g, c.b, 0.18f));
                            cv.Line(x, y, nx, ny, 3, c);
                            cv.Line(x, y, nx, ny, 1, Color.white);
                            x = nx; y = ny;
                        }
                        cv.Circle(x, y, 4, Color.white);
                    }
                    break;
                case "hex":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float hx = x / 18f, hy = y / 18f * 1.1547f;
                            float qx = hx - hy * 0.5f, qy = hy;
                            float rx = Mathf.Round(qx), ry = Mathf.Round(qy), rz = Mathf.Round(-qx - qy);
                            float dx = Mathf.Abs(rx - qx), dy = Mathf.Abs(ry - qy), dz = Mathf.Abs(rz + qx + qy);
                            float edge = Mathf.Max(dx, Mathf.Max(dy, dz));
                            var c = Color.Lerp(Hex("1b1b1b"), Hex("3a3020"), Fbm(rx * 3, ry * 3, 64, 4));
                            cv.px[y * S + x] = edge > 0.42f ? Hex("e8b923") : c;
                        }
                    break;
                case "zebra":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float v = Mathf.Sin(x * 0.11f + Fbm(x / 40f, y / 40f, 6, 8) * 9 + Mathf.Sin(y * 0.03f) * 2);
                            cv.px[y * S + x] = v > 0.25f ? Hex("101010") : Hex("f4f2ea");
                        }
                    break;
                case "leopard":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++) cv.px[y * S + x] = Color.Lerp(Hex("d9a85a"), Hex("f0cd8a"), Fbm(x / 50f, y / 50f, 5, 2));
                    for (int k = 0; k < 34; k++)
                    {
                        float cx = r.F() * S, cy = r.F() * S, rad = r.F(7, 12);
                        cv.Circle(cx, cy, rad * 0.8f, Hex("a8622a"));
                        for (int j = 0; j < 5; j++) { float a = j * 1.26f + r.F(); cv.Circle(cx + Mathf.Cos(a) * rad, cy + Mathf.Sin(a) * rad, r.F(2.5f, 4.5f), Hex("1a0f06")); }
                    }
                    break;
                case "ice":
                {
                    var pts = new List<Vector2>();
                    for (int k = 0; k < 24; k++) pts.Add(new Vector2(r.F() * S, r.F() * S));
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float d1 = 1e9f, d2 = 1e9f;
                            foreach (var p in pts) { float d = Vector2.Distance(p, new Vector2(x, y)); if (d < d1) { d2 = d1; d1 = d; } else if (d < d2) d2 = d; }
                            var c = Color.Lerp(Hex("bfe6ff"), Hex("6fb6ea"), d1 / 60f);
                            if (d2 - d1 < 1.6f) c = Color.white;
                            cv.px[y * S + x] = c;
                        }
                    for (int k = 0; k < 40; k++) cv.Circle(r.F() * S, r.F() * S, r.F(0.6f, 1.4f), Color.white);
                    break;
                }
                case "toxic":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float f = Fbm(x / 36f, y / 36f, 7, 33);
                            cv.px[y * S + x] = f > 0.55f ? Color.Lerp(Hex("6cff2a"), Hex("d4ff7a"), (f - 0.55f) * 4) : Color.Lerp(Hex("071a08"), Hex("0f3a12"), f);
                        }
                    for (int k = 0; k < 14; k++) { float x = r.F() * S, y = r.F() * S, len = r.F(15, 45); cv.Line(x, y, x, y - len, 5, Hex("6cff2a")); cv.Circle(x, y - len, 4.5f, Hex("b6ff5a")); }
                    for (int k = 0; k < 4; k++)
                    {
                        float cx = r.F() * S, cy = r.F() * S;
                        cv.Ring(cx, cy, 16, 4, Hex("ffe600"));
                        for (int j = 0; j < 3; j++) { float a = j * 2.094f; cv.Circle(cx + Mathf.Cos(a) * 9, cy + Mathf.Sin(a) * 9, 6, Hex("ffe600")); }
                        cv.Circle(cx, cy, 3.5f, Hex("071a08"));
                    }
                    break;
                case "money":
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("2e4a2a");
                    for (int row = 0; row < 6; row++)
                        for (int col = 0; col < 3; col++)
                        {
                            float bx = col * 90 + (row % 2) * 45 - 20, by = row * 45 - 10;
                            cv.Rect(bx, by, 84, 40, Hex("bcd3a8"));
                            cv.Rect(bx + 3, by + 3, 78, 34, Hex("8fb07a"));
                            cv.Rect(bx + 5, by + 5, 74, 30, Hex("d6e6c4"));
                            cv.Circle(bx + 42, by + 20, 11, Hex("6f8f5f"));
                            cv.Circle(bx + 42, by + 23, 5, Hex("d6e6c4")); cv.Rect(bx + 36, by + 12, 12, 7, Hex("d6e6c4"));
                            // "$" signs in the corners
                            foreach (var (sx, sy) in new[] { (bx + 13, by + 20), (bx + 71, by + 20) })
                            {
                                var g2 = Hex("2e5a2a");
                                cv.Line(sx + 5, sy + 7, sx - 4, sy + 7, 2.2f, g2); cv.Line(sx - 4, sy + 7, sx - 4, sy + 1, 2.2f, g2);
                                cv.Line(sx - 4, sy + 1, sx + 4, sy - 1, 2.2f, g2); cv.Line(sx + 4, sy - 1, sx + 4, sy - 7, 2.2f, g2);
                                cv.Line(sx + 4, sy - 7, sx - 5, sy - 7, 2.2f, g2); cv.Line(sx, sy + 10, sx, sy - 10, 1.6f, g2);
                            }
                        }
                    break;
                case "hearts":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++) cv.px[y * S + x] = Color.Lerp(Hex("ffb3d1"), Hex("ffd6e7"), y / (float)S);
                    for (int k = 0; k < 30; k++)
                    {
                        float cx = r.F() * S, cy = r.F() * S, sz = r.F(6, 15);
                        var c = k % 3 == 0 ? Hex("e8124f") : k % 3 == 1 ? Hex("ff4f8a") : Hex("ffffff");
                        cv.Circle(cx - sz * 0.5f, cy + sz * 0.3f, sz * 0.58f, c); cv.Circle(cx + sz * 0.5f, cy + sz * 0.3f, sz * 0.58f, c);
                        for (int j = 0; j < (int)(sz * 1.2f); j++) { float t = j / (sz * 1.2f); cv.Rect(cx - sz * (1.05f - t), cy - j + sz * 0.2f, sz * 2 * (1.05f - t), 1, c); }
                        if (k % 3 != 2) cv.Circle(cx - sz * 0.55f, cy + sz * 0.5f, sz * 0.18f, new Color(1, 1, 1, 0.7f));
                    }
                    break;
                case "snowcamo":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float a = Fbm(x / 30f, y / 30f, 8, 41), b = Fbm(x / 22f + 7, y / 22f, 12, 42);
                            var c = Hex("eef1f4");
                            if (a > 0.55f) c = Hex("b9c2cc");
                            if (b > 0.62f) c = Hex("7d8794");
                            if (a < 0.3f) c = Hex("dfe5ea");
                            cv.px[y * S + x] = c;
                        }
                    break;
                case "ocean":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float w = Mathf.Sin(x * 0.05f + Mathf.Sin(y * 0.09f) * 2 + y * 0.12f);
                            var c = Color.Lerp(Hex("063a7a"), Hex("1b8ad6"), y / (float)S + Fbm(x / 40f, y / 40f, 6, 5) * 0.4f - 0.2f);
                            if (w > 0.92f) c = Color.Lerp(c, Color.white, (w - 0.92f) * 12);
                            cv.px[y * S + x] = c;
                        }
                    for (int k = 0; k < 60; k++) cv.Circle(r.F() * S, r.F() * S, r.F(1, 2.5f), new Color(1, 1, 1, 0.6f));
                    break;
                case "graffiti":
                {
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++) cv.px[y * S + x] = Color.Lerp(Hex("6d7076"), Hex("8d9096"), Fbm(x / 20f, y / 20f, 12, 9));
                    string[] cols = { "ff2fd2", "29f0ff", "ffe600", "7cff5a", "ff6a00", "8b5cff" };
                    for (int k = 0; k < 9; k++)
                    {
                        var c = Hex(cols[k % cols.Length]);
                        float x = r.F() * S, y = r.F() * S;
                        for (int s = 0; s < 6; s++)
                        {
                            float nx = x + r.F(-40, 40), ny = y + r.F(-30, 30);
                            cv.Line(x, y, nx, ny, 14, Hex("111111")); cv.Line(x, y, nx, ny, 10, c);
                            x = nx; y = ny;
                        }
                        for (int d = 0; d < 3; d++) { float dx = x + r.F(-10, 10); cv.Line(dx, y, dx, y - r.F(10, 30), 3, c); }
                    }
                    for (int k = 0; k < 120; k++) cv.Circle(r.F() * S, r.F() * S, r.F(0.5f, 2f), Hex(cols[r.I(cols.Length)]));
                    break;
                }
                case "chrome":
                    for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
                        {
                            float v = (y + Mathf.Sin(x * 0.04f) * 10) / S;
                            Color c = v < 0.45f ? Color.Lerp(Hex("2a2d33"), Hex("9aa2ad"), v / 0.45f) : v < 0.5f ? Color.Lerp(Hex("f4f8ff"), Hex("ffffff"), (v - 0.45f) * 20) : Color.Lerp(Hex("cfd6e0"), Hex("6a7280"), (v - 0.5f) * 2);
                            cv.px[y * S + x] = c;
                        }
                    break;
                default:
                    for (int i = 0; i < cv.px.Length; i++) cv.px[i] = Hex("3a3d43");
                    cv.Noise(r, 0.04f);
                    break;
            }
            return cv.Tex();
        }

        static readonly Dictionary<string, Material> skinMats = new Dictionary<string, Material>();

        /// <summary>Weapon skin material (metal/gloss per skin; lava and cosmic glow).</summary>
        public static Material SkinMat(string id)
        {
            if (skinMats.TryGetValue(id, out var m) && m != null) return m;
            var t = Skin(id);
            switch (id)
            {
                case "golden": m = Mats.Emissive(t, Color.white, new Color(0.25f, 0.18f, 0.05f), t, 0.75f); m.SetFloat("_Metallic", 0.45f); break;
                case "damascus": m = Mats.Tex(t, Color.white, 0.7f, 0.5f); break;
                case "carbon": m = Mats.Tex(t, Color.white, 0.7f, 0.3f); break;
                case "obsidian": m = Mats.Tex(t, Color.white, 0.92f, 0.5f); break;
                case "jade": m = Mats.Tex(t, Color.white, 0.85f, 0.1f); break;
                case "lava": m = Mats.Emissive(t, Color.white, new Color(1.6f, 0.8f, 0.3f), t, 0.3f); break;
                case "cosmic": m = Mats.Emissive(t, Color.white, new Color(0.7f, 0.7f, 0.9f), t, 0.6f); break;
                case "circuit": m = Mats.Tex(t, Color.white, 0.5f, 0.3f); break;
                case "neon": m = Mats.Emissive(t, Color.white, new Color(1.4f, 1.4f, 1.4f), t, 0.6f); break;
                case "toxic": m = Mats.Emissive(t, Color.white, new Color(0.9f, 1.2f, 0.9f), t, 0.5f); break;
                case "flames": m = Mats.Emissive(t, Color.white, new Color(0.8f, 0.5f, 0.3f), t, 0.4f); break;
                case "chrome": m = Mats.Tex(t, Color.white, 0.95f, 0.5f); break;
                case "ice": m = Mats.Tex(t, Color.white, 0.9f, 0.15f); break;
                case "dragon": m = Mats.Tex(t, Color.white, 0.6f, 0.3f); break;
                default: m = Mats.Tex(t, Color.white, 0.45f, 0.2f); break;
            }
            skinMats[id] = m;
            return m;
        }
    }
}
