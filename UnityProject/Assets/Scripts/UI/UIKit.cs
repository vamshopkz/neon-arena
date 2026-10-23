using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Arena
{
    /// <summary>Futuristic UI look generated at runtime: dark glass panels, neon buttons, Russo One font.</summary>
    public static class UIKit
    {
        public static readonly Color Cyan = Hex("35E0FF"), Pink = Hex("FF3FA4"), Lime = Hex("7CFF5A"), Gold = Hex("FFC83A"), Red = Hex("FF4E5E"),
            Ink = Hex("0A1020"), Muted = Hex("9FB3CC"), Violet = Hex("8B5CFF");
        public static TMP_FontAsset Font;
        public static Material TextOutline, TextPlain;
        public static Sprite Panel, Glass, Btn, Circle, Ring, White, Pill;

        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

        public static void Init()
        {
            if (Font != null) return;
            var ttf = Resources.Load<UnityEngine.Font>("Fonts/RussoOne-Regular");
            Font = TMP_FontAsset.CreateFontAsset(ttf, 64, 7, GlyphRenderMode.SDFAA, 2048, 1024, AtlasPopulationMode.Dynamic, true);
            Font.name = "RussoOne SDF";
            Font.TryAddCharacters("0123456789 +-−×%/.,:;!?()«»—–\"'№#@&*=<>[]_↻→←↑↓✓$●▲▼" +
                                  "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
                                  "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюяӘәҒғҚқҢңӨөҰұҮүҺһІі", out _);
            TextPlain = Font.material;
            TextOutline = new Material(Font.material) { name = "TMP Outline" };
            TextOutline.EnableKeyword("OUTLINE_ON");
            TextOutline.SetFloat("_OutlineWidth", 0.2f);
            TextOutline.SetColor("_OutlineColor", Hex("050914"));
            TextOutline.SetFloat("_FaceDilate", 0.1f);

            Panel = Round(128, 128, 28, 3, new Color(0.05f, 0.08f, 0.16f, 0.94f), new Color(0.03f, 0.05f, 0.1f, 0.94f), new Color(0.2f, 0.85f, 1f, 0.9f));
            Glass = Round(96, 96, 22, 2, new Color(0.06f, 0.1f, 0.2f, 0.72f), new Color(0.03f, 0.06f, 0.12f, 0.72f), new Color(0.3f, 0.8f, 1f, 0.55f));
            Btn = Round(128, 128, 26, 0, Color.white, new Color(0.82f, 0.82f, 0.82f), Color.white);
            Pill = Round(128, 64, 31, 0, Color.white, Color.white, Color.white);
            Circle = Disc(128, 0, Color.white, Color.white);
            Ring = Disc(256, 8, new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.65f));
            var w = new Texture2D(4, 4); var px = new Color32[16];
            for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
            w.SetPixels32(px); w.Apply();
            White = Sprite.Create(w, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));
        }

        static Sprite Round(int w, int h, float r, float border, Color top, Color bottom, Color bc)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            float hw = w / 2f, hh = h / 2f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - hw) - (hw - r), qy = Mathf.Abs(y + 0.5f - hh) - (hh - r);
                    float outside = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
                    float a = Mathf.Clamp01(0.5f - outside);
                    var c = Color.Lerp(bottom, top, (float)y / (h - 1));
                    float bm = border > 0 ? Mathf.Clamp01(border + outside + 0.5f) : 0;
                    c = Color.Lerp(c, bc, bm);
                    c.a *= a;
                    px[y * w + x] = c;
                }
            tex.SetPixels(px); tex.Apply();
            float b = r + border + 2;
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        static Sprite Disc(int s, float border, Color fill, Color bc)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
            var px = new Color[s * s];
            float r = s / 2f;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = new Vector2(x + 0.5f - r, y + 0.5f - r).magnitude;
                    float a = Mathf.Clamp01(r - d);
                    float bm = border > 0 ? Mathf.Clamp01(border - (r - d) + 0.5f) : 0;
                    var c = Color.Lerp(fill, bc, bm);
                    c.a *= a;
                    px[y * s + x] = c;
                }
            tex.SetPixels(px); tex.Apply(true);
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---------------- layout ----------------

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Fill(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        public static Image Img(Transform parent, Sprite s, Color c, string name = "Image", bool ray = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = s; img.color = c; img.raycastTarget = ray;
            if (s != null && s.border != Vector4.zero) img.type = Image.Type.Sliced;
            return img;
        }

        public static TextMeshProUGUI Txt(Transform parent, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool outline = true, string name = "Text")
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = Font;
            t.fontSharedMaterial = outline ? TextOutline : TextPlain;
            t.text = text; t.fontSize = size; t.color = color; t.alignment = align;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        public static UIButton Button(Transform parent, string label, Color color, Action onClick, float font = 30, string name = "Button")
        {
            var img = Img(parent, Btn, color, name, true);
            var b = img.gameObject.AddComponent<UIButton>();
            b.OnClick = onClick;
            b.Bg = img;
            b.Label = Txt(img.transform, label, font, Color.white);
            Fill(b.Label.rectTransform, 10, 4, 10, 4);
            b.Label.textWrappingMode = TextWrappingModes.NoWrap;
            b.Label.enableAutoSizing = true;
            b.Label.fontSizeMin = 14; b.Label.fontSizeMax = font;
            return b;
        }

        public static UIButton Round(Transform parent, string label, Color color, Action onClick, float size, float font = 28)
        {
            var img = Img(parent, Circle, color, "Round " + label, true);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            var ring = Img(img.transform, Ring, Color.white, "Ring");
            Fill(ring.rectTransform);
            var b = img.gameObject.AddComponent<UIButton>();
            b.OnClick = onClick; b.Bg = img;
            b.Label = Txt(img.transform, label, font, Color.white);
            Fill(b.Label.rectTransform, 6, 6, 6, 6);
            b.Label.enableAutoSizing = true; b.Label.fontSizeMin = 12; b.Label.fontSizeMax = font;
            return b;
        }

        public static string Num(long n)
        {
            var s = Math.Abs(n).ToString();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++) { if (i > 0 && (s.Length - i) % 3 == 0) sb.Append(' '); sb.Append(s[i]); }
            return (n < 0 ? "-" : "") + sb;
        }

        /// <summary>Text field (name, chat). Opens the on-screen keyboard on phones.</summary>
        public static TMP_InputField Input(Transform parent, string placeholder, int limit, float font = 30)
        {
            var bg = Img(parent, Glass, Color.white, "Input", true);
            var area = Rect("Text Area", bg.transform);
            Fill(area, 18, 6, 18, 6);
            area.gameObject.AddComponent<RectMask2D>();
            var ph = Txt(area, placeholder, font, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.MidlineLeft, false, "Placeholder");
            Fill(ph.rectTransform);
            ph.textWrappingMode = TextWrappingModes.NoWrap;
            var tx = Txt(area, "", font, Color.white, TextAlignmentOptions.MidlineLeft, false, "Text");
            Fill(tx.rectTransform);
            tx.textWrappingMode = TextWrappingModes.NoWrap;
            var f = bg.gameObject.AddComponent<TMP_InputField>();
            f.textViewport = area; f.textComponent = tx; f.placeholder = ph;
            f.characterLimit = limit;
            f.fontAsset = Font;
            f.pointSize = font;
            f.lineType = TMP_InputField.LineType.SingleLine;
            f.onSelect.AddListener(_ => UIRoot.TypingText = true);
            f.onDeselect.AddListener(_ => UIRoot.TypingText = false);
            f.onEndEdit.AddListener(_ => UIRoot.TypingText = false);
            return f;
        }
    }

    /// <summary>Button with a press animation; fires on release inside.</summary>
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public Action OnClick;
        public Image Bg;
        public TextMeshProUGUI Label;
        public bool Interactable = true;
        public bool FireOnDown;
        public bool Held { get; private set; }
        Vector3 baseScale = Vector3.one;

        void Awake() { baseScale = transform.localScale; }

        public void OnPointerDown(PointerEventData e)
        {
            Held = true;
            transform.localScale = baseScale * 0.93f;
            if (FireOnDown && Interactable) OnClick?.Invoke();
        }

        public void OnPointerUp(PointerEventData e)
        {
            Held = false;
            transform.localScale = baseScale;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (FireOnDown || !Interactable) return;
            AudioFx.Play("click");
            OnClick?.Invoke();
        }

        void OnDisable() { Held = false; transform.localScale = baseScale; }

        public void Set(string label, Color? color = null)
        {
            if (Label != null && Label.text != label) Label.text = label;
            if (color.HasValue && Bg != null) Bg.color = color.Value;
        }
    }

    /// <summary>Floating joystick in the bottom-left zone.</summary>
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Base, Knob;
        public float Radius = 110f;
        Vector2 home;
        int pointer = int.MinValue;
        public Vector2 Value { get; private set; }

        public void Init(RectTransform b, RectTransform k) { Base = b; Knob = k; home = b.anchoredPosition; }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var local);
            var r = ((RectTransform)transform).rect;
            Base.anchoredPosition = local - new Vector2(r.xMin, r.yMin);
            Knob.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Base, e.position, e.pressEventCamera, out var local);
            var v = Vector2.ClampMagnitude(local, Radius);
            Knob.anchoredPosition = v;
            Value = v / Radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            pointer = int.MinValue;
            Value = Vector2.zero;
            Knob.anchoredPosition = Vector2.zero;
            Base.anchoredPosition = home;
        }

        void OnDisable() { pointer = int.MinValue; Value = Vector2.zero; if (Knob) Knob.anchoredPosition = Vector2.zero; if (Base) Base.anchoredPosition = home; }

        bool wasActive;
        void Update()
        {
            var p = PlayerCtl.I;
            if (p == null) return;
            bool active = pointer != int.MinValue;
            if (active || wasActive) p.MoveInput = Value;
            wasActive = active;
        }
    }

    /// <summary>Drag anywhere on the right part of the screen to turn the camera.</summary>
    public class LookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData e) { if (pointer == int.MinValue) { pointer = e.pointerId; UIRoot.LookActive = true; } }
        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointer || PlayerCtl.I == null) return;
            float k = 1600f / Mathf.Max(800, Screen.width) * 0.9f;
            PlayerCtl.I.LookDelta += new Vector2(e.delta.x * k, -e.delta.y * k * 0.8f);
        }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == pointer) { pointer = int.MinValue; UIRoot.LookActive = false; } }
        void OnDisable() { pointer = int.MinValue; UIRoot.LookActive = false; }
    }

    /// <summary>Canvas, event system and the modal window used by every menu.</summary>
    public static class UIRoot
    {
        public static Canvas Canvas;
        public static RectTransform Root, HudLayer, WinLayer, TopLayer;
        public static bool TypingText, LookActive;
        public static Window Open;

        public static void Create()
        {
            UIKit.Init();
            var go = new GameObject("UI") { layer = 5 };
            Canvas = go.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1600, 900);
            sc.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();
            Root = (RectTransform)go.transform;
            HudLayer = UIKit.Fill(UIKit.Rect("HUD", Root));
            WinLayer = UIKit.Fill(UIKit.Rect("Windows", Root));
            TopLayer = UIKit.Fill(UIKit.Rect("Top", Root));
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        /// <summary>Safe area inset (notches) in canvas units.</summary>
        public static Vector4 Safe()
        {
            var s = Screen.safeArea;
            float k = 900f / Mathf.Max(1, Screen.height);
            return new Vector4(s.xMin * k, s.yMin * k, (Screen.width - s.xMax) * k, (Screen.height - s.yMax) * k);
        }
    }

    /// <summary>A modal window: title, close button and a scrolling list of rows.</summary>
    public class Window : MonoBehaviour
    {
        public RectTransform Content;
        public TextMeshProUGUI Title, Footer;
        public Action OnClose;
        Action rebuild;

        public static Window Show(string title, Action<Window> fill, float width = 1100, float height = 760)
        {
            Close();
            var dim = UIKit.Img(UIRoot.WinLayer, UIKit.White, new Color(0, 0, 0, 0.55f), "Dim", true);
            UIKit.Fill(dim.rectTransform);
            var panel = UIKit.Img(dim.transform, UIKit.Panel, Color.white, "Window", true);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
            var w = dim.gameObject.AddComponent<Window>();
            w.Title = UIKit.Txt(panel.transform, title, 40, UIKit.Cyan, TMPro.TextAlignmentOptions.MidlineLeft);
            UIKit.Place(w.Title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(34, -18), new Vector2(width - 150, 64));
            var close = UIKit.Button(panel.transform, "X", UIKit.Red, Close, 34, "Close");
            UIKit.Place(close.Bg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-18, -16), new Vector2(84, 70));
            w.Footer = UIKit.Txt(panel.transform, "", 24, UIKit.Muted, TMPro.TextAlignmentOptions.MidlineLeft, false);
            UIKit.Place(w.Footer.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(34, 10), new Vector2(width - 68, 40));

            var view = UIKit.Rect("Viewport", panel.transform);
            UIKit.Fill(view, 24, 54, 24, 96);
            view.gameObject.AddComponent<RectMask2D>();
            var vimg = view.gameObject.AddComponent<Image>(); vimg.color = new Color(0, 0, 0, 0.001f);
            w.Content = UIKit.Rect("Content", view);
            w.Content.anchorMin = new Vector2(0, 1); w.Content.anchorMax = new Vector2(1, 1); w.Content.pivot = new Vector2(0.5f, 1);
            w.Content.sizeDelta = Vector2.zero;
            var vl = w.Content.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 10; vl.childControlHeight = true; vl.childControlWidth = true; vl.childForceExpandHeight = false; vl.padding = new RectOffset(6, 6, 6, 6);
            w.Content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var sr = view.gameObject.AddComponent<ScrollRect>();
            sr.content = w.Content; sr.horizontal = false; sr.viewport = view; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 40;
            UIRoot.Open = w;
            w.rebuild = () => fill(w);
            fill(w);
            if (PlayerCtl.I != null) PlayerCtl.I.Frozen = true;
            return w;
        }

        /// <summary>Redraws the window (after buying something etc.).</summary>
        public static void Refresh()
        {
            var w = UIRoot.Open;
            if (w == null) return;
            for (int i = w.Content.childCount - 1; i >= 0; i--) Destroy(w.Content.GetChild(i).gameObject);
            w.rebuild?.Invoke();
        }

        public static void Close()
        {
            if (UIRoot.Open != null)
            {
                var w = UIRoot.Open;
                UIRoot.Open = null;
                w.OnClose?.Invoke();
                Destroy(w.gameObject);
            }
            if (PlayerCtl.I != null) PlayerCtl.I.Frozen = false;
            UIRoot.TypingText = false;
        }

        // ---------------- rows ----------------

        public TextMeshProUGUI Text(string text, float size = 28, Color? color = null)
        {
            var t = UIKit.Txt(Content, text, size, color ?? Color.white, TMPro.TextAlignmentOptions.TopLeft, false);
            t.overflowMode = TMPro.TextOverflowModes.Overflow;
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size * 1.35f;
            t.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        public UIButton Row(string title, string sub, string btn, Color btnColor, Action onClick, bool enabled = true, Color? swatch = null)
        {
            var bg = UIKit.Img(Content, UIKit.Glass, Color.white, "Row");
            bg.gameObject.AddComponent<LayoutElement>().minHeight = 104;
            float left = 22;
            if (swatch.HasValue)
            {
                var sw = UIKit.Img(bg.transform, UIKit.Circle, swatch.Value, "Swatch");
                UIKit.Place(sw.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(64, 64));
                left = 100;
            }
            var t = UIKit.Txt(bg.transform, title, 30, Color.white, TMPro.TextAlignmentOptions.TopLeft, false);
            t.rectTransform.anchorMin = new Vector2(0, 0); t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.offsetMin = new Vector2(left, 46); t.rectTransform.offsetMax = new Vector2(-300, -12);
            if (!string.IsNullOrEmpty(sub))
            {
                var s = UIKit.Txt(bg.transform, sub, 22, UIKit.Muted, TMPro.TextAlignmentOptions.TopLeft, false);
                s.rectTransform.anchorMin = new Vector2(0, 0); s.rectTransform.anchorMax = new Vector2(1, 0);
                s.rectTransform.pivot = new Vector2(0, 0);
                s.rectTransform.offsetMin = new Vector2(left, 8); s.rectTransform.offsetMax = new Vector2(-300, 48);
            }
            if (btn == null) return null;
            var b = UIKit.Button(bg.transform, btn, enabled ? btnColor : new Color(0.3f, 0.33f, 0.38f), enabled ? onClick : () => AudioFx.Play("error"), 26);
            UIKit.Place(b.Bg.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(270, 76));
            return b;
        }

        /// <summary>A row of small buttons (colour pickers, options).</summary>
        public void Chips(string title, IList<(string label, Color color, bool selected, Action act)> chips)
        {
            var bg = UIKit.Img(Content, UIKit.Glass, Color.white, "Chips");
            int perRow = 7;
            int rows = (chips.Count + perRow - 1) / perRow;
            bg.gameObject.AddComponent<LayoutElement>().minHeight = 60 + rows * 74;
            var t = UIKit.Txt(bg.transform, title, 26, UIKit.Cyan, TMPro.TextAlignmentOptions.TopLeft, false);
            t.rectTransform.anchorMin = new Vector2(0, 1); t.rectTransform.anchorMax = new Vector2(1, 1); t.rectTransform.pivot = new Vector2(0, 1);
            t.rectTransform.offsetMin = new Vector2(22, -50); t.rectTransform.offsetMax = new Vector2(-20, -10);
            for (int i = 0; i < chips.Count; i++)
            {
                var c = chips[i];
                var pos = new Vector2(20 + (i % perRow) * 146, -54 - (i / perRow) * 74);
                if (c.selected)
                {
                    var ring = UIKit.Img(bg.transform, UIKit.Btn, UIKit.Gold, "Sel");
                    UIKit.Place(ring.rectTransform, new Vector2(0, 1), new Vector2(0, 1), pos + new Vector2(-5, 5), new Vector2(146, 74));
                }
                var b = UIKit.Button(bg.transform, c.label, c.color, c.act, 22);
                UIKit.Place(b.Bg.rectTransform, new Vector2(0, 1), new Vector2(0, 1), pos, new Vector2(136, 64));
            }
        }
    }
}
