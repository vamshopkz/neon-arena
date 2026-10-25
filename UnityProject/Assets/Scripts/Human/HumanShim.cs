using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Real
{
    /// <summary>The realistic human's outfit (ids from the human wardrobe catalogue in HumanStyle).</summary>
    [Serializable]
    public class SkinSet
    {
        public string character = "human", hand = "hand_none";
        public string hGender = "hg_m", hSkin = "hs_light", hHair = "hh_short_brown", hEyes = "he_brown", hMouth = "hm_smile",
                      hFace = "hface_oval", hTop = "ht_tee_white", hBottom = "hb_jeans_blue", hShoes = "hf_sneakers_white", hAcc = "ha_none",
                      hHat = "head_none", hNeck = "neck_none", hBack = "back_none", hNumber = "hn_none";

        public string Encode() => string.Join(";", hGender, hSkin, hHair, hEyes, hMouth, hTop, hBottom, hShoes, hAcc, hFace);

        public static SkinSet Decode(string s)
        {
            var k = new SkinSet();
            if (string.IsNullOrEmpty(s)) return k;
            var q = s.Split(';');
            if (q.Length >= 9) { k.hGender = q[0]; k.hSkin = q[1]; k.hHair = q[2]; k.hEyes = q[3]; k.hMouth = q[4]; k.hTop = q[5]; k.hBottom = q[6]; k.hShoes = q[7]; k.hAcc = q[8]; }
            if (q.Length >= 10) k.hFace = q[9];
            return k;
        }

        public string Get(Wardrobe.Slot slot) => slot switch
        {
            Wardrobe.Slot.HGender => hGender, Wardrobe.Slot.HSkin => hSkin, Wardrobe.Slot.HHair => hHair, Wardrobe.Slot.HEyes => hEyes,
            Wardrobe.Slot.HMouth => hMouth, Wardrobe.Slot.HTop => hTop, Wardrobe.Slot.HBottom => hBottom, Wardrobe.Slot.HShoes => hShoes,
            Wardrobe.Slot.HAcc => hAcc, Wardrobe.Slot.HFace => hFace, _ => null,
        };

        public void Set(Wardrobe.Slot slot, string id)
        {
            switch (slot)
            {
                case Wardrobe.Slot.HGender: hGender = id; break;
                case Wardrobe.Slot.HSkin: hSkin = id; break;
                case Wardrobe.Slot.HHair: hHair = id; break;
                case Wardrobe.Slot.HEyes: hEyes = id; break;
                case Wardrobe.Slot.HMouth: hMouth = id; break;
                case Wardrobe.Slot.HTop: hTop = id; break;
                case Wardrobe.Slot.HBottom: hBottom = id; break;
                case Wardrobe.Slot.HShoes: hShoes = id; break;
                case Wardrobe.Slot.HAcc: hAcc = id; break;
                case Wardrobe.Slot.HFace: hFace = id; break;
            }
        }

        public SkinSet Clone() => Decode(Encode());
    }

    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    public static partial class Wardrobe
    {
        public enum Slot { Character, Head, Eyes, Mouth, Neck, Shirt, Shorts, Back, Kit, Number, Hand, Legs, Costume, HGender, HSkin, HHair, HEyes, HMouth, HTop, HBottom, HShoes, HAcc, HHat, HNeck, HBack, HNumber, HFace }

        public class SkinDef
        {
            public string id, name, style, model;
            public Slot slot;
            public Color color, color2, color3, numColor = Color.white;
            public string pattern;
            public Rarity rarity;
            public int number = -1;
            public bool none;
            public string dept;
        }

        public class Built
        {
            public GameObject root;
            public Bounds headBounds;
            public HumanRig human;
            public HumanAnimator humanAnim;
            public readonly List<Renderer> accessories = new List<Renderer>();
        }

        static List<SkinDef> all;
        static Dictionary<string, SkinDef> byId;

        static readonly (string key, string name, Color c)[] Palette =
        {
            ("red", "Красн", new Color(0.88f, 0.15f, 0.13f)), ("blue", "Син", new Color(0.18f, 0.42f, 0.95f)),
            ("green", "Зелён", new Color(0.2f, 0.72f, 0.25f)), ("yellow", "Жёлт", new Color(1f, 0.82f, 0.12f)),
            ("black", "Чёрн", new Color(0.13f, 0.13f, 0.15f)), ("white", "Бел", new Color(0.95f, 0.95f, 0.95f)),
            ("pink", "Розов", new Color(1f, 0.45f, 0.72f)), ("purple", "Фиолетов", new Color(0.58f, 0.3f, 0.9f)),
            ("orange", "Оранжев", new Color(1f, 0.55f, 0.1f)), ("cyan", "Бирюзов", new Color(0.15f, 0.8f, 0.85f)),
            ("brown", "Коричнев", new Color(0.5f, 0.3f, 0.15f)),
            ("sky", "Небесн", new Color(0.45f, 0.75f, 1f)), ("maroon", "Бордов", new Color(0.5f, 0.07f, 0.15f)),
            ("lime", "Лаймов", new Color(0.62f, 0.95f, 0.15f)), ("gold", "Золот", new Color(1f, 0.76f, 0.15f)),
            ("navy", "Тёмно-син", new Color(0.08f, 0.13f, 0.38f)), ("silver", "Серебрист", new Color(0.78f, 0.8f, 0.84f)),
        };

        static Color Pal(string key) { foreach (var p in Palette) if (p.key == key) return p.c; return Color.white; }
        static string Adj(string key, string ending)
        {
            foreach (var p in Palette)
                if (p.key == key) return p.name + (key == "blue" || key == "navy" ? Soften(ending) : ending);
            return key;
        }
        static string Soften(string e) =>
            e.StartsWith("ая") ? "яя" + e.Substring(2) : e.StartsWith("ые") ? "ие" + e.Substring(2) : e.StartsWith("ый") ? "ий" + e.Substring(2) : e.StartsWith("ое") ? "ее" + e.Substring(2) : e;

        public static List<SkinDef> All
        {
            get
            {
                if (all != null) return all;
                all = new List<SkinDef>();
                AddHuman(all);
                AddNumbers(all);
                byId = new Dictionary<string, SkinDef>();
                foreach (var d in all) byId[d.id] = d;
                return all;
            }
        }

        public static SkinDef Find(string id)
        {
            if (all == null) _ = All;
            return id != null && byId.TryGetValue(id, out var d) ? d : null;
        }

        public static string IconId(SkinDef d) => "skin_" + d.id;

        public static List<SkinDef> OfSlot(Slot s) => All.FindAll(d => d.slot == s);
    }

    public static partial class Mats
    {
        static readonly Dictionary<string, Material> shared = new Dictionary<string, Material>();
        static Material litN;

        public static Material Shared(string key, Func<Material> make)
        {
            if (shared.TryGetValue(key, out var m) && m != null) return m;
            m = make();
            shared[key] = m;
            return m;
        }

        public static Material Lit(Color color, Texture tex, float smoothness = 0.2f, float metallic = 0f)
        {
            var m = new Material(Resources.Load<Material>("Materials/Lit")) { enableInstancing = true };
            m.SetColor("_BaseColor", color);
            if (tex != null) m.SetTexture("_BaseMap", tex);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            return m;
        }

        public static Material LitNormal(Color color, Texture albedo, Texture normal, float normalScale = 1f, float smoothness = 0.15f)
        {
            if (litN == null) litN = Resources.Load<Material>("Materials/LitNormal");
            var m = new Material(litN) { enableInstancing = true };
            m.SetColor("_BaseColor", color);
            if (albedo != null) m.SetTexture("_BaseMap", albedo);
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", normalScale);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        public static Material Transparent(Color color)
        {
            var m = new Material(Resources.Load<Material>("Materials/LitTransparent"));
            m.SetColor("_BaseColor", color);
            return m;
        }
    }

    public static class ProcTex
    {
        static int Mod(int a, int n) => (a % n + n) % n;

        public static Texture2D NormalFromHeight(float[] h, int S, float strength, string name)
        {
            var n = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float l = h[y * S + Mod(x - 1, S)], r = h[y * S + Mod(x + 1, S)];
                    float d = h[Mod(y - 1, S) * S + x], u = h[Mod(y + 1, S) * S + x];
                    var v = new Vector3((l - r) * strength, (d - u) * strength, 1f).normalized;
                    n[y * S + x] = new Color(v.x * 0.5f + 0.5f, v.y * 0.5f + 0.5f, v.z * 0.5f + 0.5f, 1f);
                }
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, true, true) { wrapMode = TextureWrapMode.Repeat, name = name };
            tex.SetPixels(n);
            tex.Apply(true);
            return tex;
        }

        public static float PeriodicNoise(int x, int y, int size, int cells, float seed)
        {
            float fx = (float)x / size * cells, fy = (float)y / size * cells;
            int x0 = Mathf.FloorToInt(fx), y0 = Mathf.FloorToInt(fy);
            float tx = fx - x0, ty = fy - y0;
            tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty);
            float a = Hash(x0 % cells, y0 % cells, seed), b = Hash((x0 + 1) % cells, y0 % cells, seed);
            float c = Hash(x0 % cells, (y0 + 1) % cells, seed), d = Hash((x0 + 1) % cells, (y0 + 1) % cells, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        static float Hash(int x, int y, float seed)
        {
            float h = Mathf.Sin(x * 127.1f + y * 311.7f + seed * 74.7f) * 43758.5453f;
            return h - Mathf.Floor(h);
        }
    }

    public static class MeshUtil
    {
        public static GameObject MeshObject(string name, Mesh mesh, Material mat, Transform parent, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            if (parent != null) go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go;
        }

        public static GameObject Primitive(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Quaternion? rot = null)
        {
            var go = Prim.Make(type, parent, localPos, localScale, mat);
            if (rot.HasValue) go.transform.localRotation = rot.Value;
            return go;
        }
    }

    public static partial class WorldBuilder
    {
        public static TMPro.TextMeshPro WorldText(Transform parent, Vector3 localPos, Quaternion localRot, string text, float size, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            var t = go.AddComponent<TMPro.TextMeshPro>();
            UIKit.Init();
            t.font = UIKit.Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TMPro.TextAlignmentOptions.Center;
            t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            t.rectTransform.sizeDelta = new Vector2(10f, 2f);
            return t;
        }
    }
}
