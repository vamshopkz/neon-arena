using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arena
{
    /// <summary>In-game overlay: crosshair, health, ammo, weapon slots, score, kill feed, touch controls.</summary>
    public class Hud : MonoBehaviour
    {
        public static Hud I;
        public static bool Blocking => Menus.OverlayOpen;
        RectTransform root;
        Image dmgFlash, hpFill, hit, scope;
        RectTransform cross, reticle, medRow;
        Image reticleRing, reticleDot;
        readonly List<UIButton> medBtns = new List<UIButton>();
        TextMeshProUGUI hpText, ammoText, wName, scoreA, scoreB, timer, coins, center, hint, toast, partyLbl, rangeTxt, modeTxt;
        RectTransform feed, slotsRow, touchRoot, scoreboard;
        UIButton shopBtn, partyBtn, menuBtn, fireBtn, zoomBtn, scoreBtn, aimOffBtn;
        readonly List<UIButton> slotBtns = new List<UIButton>();
        float dmgT, hitT, centerUntil, toastUntil;
        bool slotsDirty = true;
        public static bool Touch => Application.isMobilePlatform || Boot.Has("-touch");

        public static void Create()
        {
            var go = new GameObject("Hud");
            I = go.AddComponent<Hud>();
            I.Build();
        }

        TextMeshProUGUI T(string s, float size, Color c, TextAlignmentOptions a, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 sz, bool outline = true)
        {
            var t = UIKit.Txt(root, s, size, c, a, outline);
            UIKit.Place(t.rectTransform, anchor, pivot, pos, sz);
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        void Build()
        {
            root = UIRoot.HudLayer;
            var safe = UIRoot.Safe();
            dmgFlash = UIKit.Img(root, UIKit.White, new Color(1, 0, 0, 0), "Damage");
            UIKit.Fill(dmgFlash.rectTransform);
            scope = UIKit.Img(root, UIKit.Ring, new Color(0, 0, 0, 0.95f), "Scope");
            UIKit.Place(scope.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 1100));
            var sl = UIKit.Img(scope.transform, UIKit.White, Color.black, "H"); UIKit.Place(sl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 2));
            var sv = UIKit.Img(scope.transform, UIKit.White, Color.black, "V"); UIKit.Place(sv.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, 1100));
            foreach (var side in new[] { -1, 1 })
            {
                var b = UIKit.Img(scope.transform, UIKit.White, Color.black, "Side");
                UIKit.Place(b.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(side < 0 ? 1 : 0, 0.5f), new Vector2(side * 540, 0), new Vector2(3000, 3000));
            }
            scope.gameObject.SetActive(false);

            cross = UIKit.Rect("Cross", root);
            UIKit.Place(cross, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
            foreach (var v in new[] { new Vector4(0, 13, 3, 12), new Vector4(0, -13, 3, 12), new Vector4(13, 0, 12, 3), new Vector4(-13, 0, 12, 3), new Vector4(0, 0, 4, 4) })
            {
                var b = UIKit.Img(cross, UIKit.White, new Color(1, 1, 1, 0.9f), "Bar");
                UIKit.Place(b.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(v.x, v.y), new Vector2(v.z, v.w));
                var sh = b.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.8f);
            }
            // red-dot sight shown while aiming (any weapon); turns red over an enemy = automatic fire
            reticle = UIKit.Rect("Reticle", root);
            UIKit.Place(reticle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));
            reticleRing = UIKit.Img(reticle, UIKit.Ring, new Color(0.3f, 1f, 0.5f, 0.95f), "Ring");
            UIKit.Fill(reticleRing.rectTransform);
            reticleDot = UIKit.Img(reticle, UIKit.Circle, new Color(0.3f, 1f, 0.5f, 1f), "Dot");
            UIKit.Place(reticleDot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(12, 12));
            reticle.gameObject.SetActive(false);

            // leave the scope (SVD / Mosin optics and every other sight)
            aimOffBtn = UIKit.Button(root, Touch ? "УБРАТЬ ПРИЦЕЛ" : "УБРАТЬ ПРИЦЕЛ (ПКМ)", new Color(0.85f, 0.15f, 0.25f, 0.9f), () => PlayerCtl.I.AimOff(), 28);
            UIKit.Place(aimOffBtn.Bg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 205 + safe.y), new Vector2(360, 72));
            aimOffBtn.FireOnDown = true;
            aimOffBtn.gameObject.SetActive(false);

            hit = UIKit.Img(root, UIKit.Ring, new Color(1, 1, 1, 0), "HitMarker");
            UIKit.Place(hit.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44));

            // top bar
            scoreA = T("0", 54, Defs.TeamA, TextAlignmentOptions.Right, new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(-90, -10 - safe.w), new Vector2(160, 70));
            scoreB = T("0", 54, Defs.TeamB, TextAlignmentOptions.Left, new Vector2(0.5f, 1), new Vector2(0, 1), new Vector2(90, -10 - safe.w), new Vector2(160, 70));
            timer = T("", 34, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18 - safe.w), new Vector2(170, 50));
            modeTxt = T("", 22, UIKit.Muted, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70 - safe.w), new Vector2(600, 32));
            coins = T("", 34, UIKit.Gold, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24 + safe.x, -14 - safe.w), new Vector2(360, 50));
            partyLbl = T("", 22, UIKit.Cyan, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24 + safe.x, -62 - safe.w), new Vector2(520, 34));
            rangeTxt = T("", 26, UIKit.Gold, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24 + safe.x, -100 - safe.w), new Vector2(520, 34));

            menuBtn = UIKit.Button(root, "МЕНЮ", new Color(0.25f, 0.28f, 0.45f), () => Menus.Pause(), 24);
            UIKit.Place(menuBtn.Bg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16 - safe.z, -12 - safe.w), new Vector2(130, 62));
            partyBtn = UIKit.Button(root, "ОТРЯД", UIKit.Violet, () => Menus.Party(), 24);
            UIKit.Place(partyBtn.Bg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-156 - safe.z, -12 - safe.w), new Vector2(150, 62));
            shopBtn = UIKit.Button(root, "МАГАЗИН", new Color(0.9f, 0.6f, 0.1f), () => Menus.Shop(), 24);
            UIKit.Place(shopBtn.Bg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-316 - safe.z, -12 - safe.w), new Vector2(170, 62));
            scoreBtn = UIKit.Button(root, "СЧЁТ", new Color(0.25f, 0.28f, 0.45f), () => Menus.ToggleScoreboard(), 24);
            UIKit.Place(scoreBtn.Bg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-316 - safe.z, -12 - safe.w), new Vector2(170, 62));

            feed = UIKit.Rect("KillFeed", root);
            UIKit.Place(feed, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16 - safe.z, -88 - safe.w), new Vector2(560, 300));
            var vl = feed.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.childAlignment = TextAnchor.UpperRight; vl.childControlHeight = true; vl.childControlWidth = true; vl.childForceExpandHeight = false; vl.spacing = 4;

            center = T("", 64, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1400, 90));
            toast = T("", 30, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110 - safe.w), new Vector2(1300, 50));
            hint = T("", 28, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150 + safe.y), new Vector2(1200, 44));

            // health + ammo
            var hpBg = UIKit.Img(root, UIKit.Glass, Color.white, "HP");
            UIKit.Place(hpBg.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(24 + safe.x, 20 + safe.y), new Vector2(330, 64));
            hpFill = UIKit.Img(hpBg.transform, UIKit.White, UIKit.Lime, "Fill");
            UIKit.Place(hpFill.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(84, 0), new Vector2(226, 18));
            hpText = UIKit.Txt(hpBg.transform, "100", 34, Color.white, TextAlignmentOptions.MidlineLeft);
            UIKit.Place(hpText.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(80, 50));

            wName = T("", 26, UIKit.Muted, TextAlignmentOptions.Right, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24 - safe.z, 70 + safe.y), new Vector2(400, 36));
            ammoText = T("", 48, Color.white, TextAlignmentOptions.Right, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24 - safe.z, 14 + safe.y), new Vector2(400, 60));

            // first-aid kits from the shop (keys 6/7/8 or tap)
            medRow = UIKit.Rect("Medkits", root);
            UIKit.Place(medRow, new Vector2(0, 0), new Vector2(0, 0), new Vector2(24 + safe.x, 94 + safe.y), new Vector2(420, 60));
            var mh = medRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            mh.spacing = 8; mh.childControlWidth = false; mh.childControlHeight = false; mh.childAlignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < Defs.Medkits.Count; i++)
            {
                var md = Defs.Medkits[i];
                var b = UIKit.Button(medRow, "", md.color * 0.85f, () => PlayerCtl.I.UseMedkit(md.id), 20);
                b.Bg.rectTransform.sizeDelta = new Vector2(132, 56);
                b.FireOnDown = Touch;
                medBtns.Add(b);
            }

            slotsRow = UIKit.Rect("Slots", root);
            UIKit.Place(slotsRow, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 18 + safe.y), new Vector2(900, 74));
            var hl = slotsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleCenter; hl.spacing = 10; hl.childControlWidth = false; hl.childControlHeight = false;

            scoreboard = UIKit.Rect("Scoreboard", root);
            UIKit.Place(scoreboard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 620));
            scoreboard.gameObject.SetActive(false);

            if (Touch) BuildTouch(safe);
            MatchStarted();
        }

        void BuildTouch(Vector4 safe)
        {
            touchRoot = UIKit.Rect("Touch", root);
            UIKit.Fill(touchRoot);
            touchRoot.SetAsFirstSibling();
            var look = UIKit.Img(touchRoot, UIKit.White, new Color(0, 0, 0, 0.001f), "LookArea", true);
            look.rectTransform.anchorMin = new Vector2(0.35f, 0); look.rectTransform.anchorMax = new Vector2(1, 0.86f);
            look.rectTransform.offsetMin = look.rectTransform.offsetMax = Vector2.zero;
            look.gameObject.AddComponent<LookArea>();
            var zone = UIKit.Img(touchRoot, UIKit.White, new Color(0, 0, 0, 0.001f), "JoyZone", true);
            zone.rectTransform.anchorMin = new Vector2(0, 0); zone.rectTransform.anchorMax = new Vector2(0.35f, 0.75f);
            zone.rectTransform.offsetMin = zone.rectTransform.offsetMax = Vector2.zero;
            var baseImg = UIKit.Img(zone.transform, UIKit.Ring, new Color(1, 1, 1, 0.5f), "Base");
            UIKit.Place(baseImg.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(190 + safe.x, 190 + safe.y), new Vector2(230, 230));
            var knob = UIKit.Img(baseImg.transform, UIKit.Circle, new Color(1, 1, 1, 0.55f), "Knob");
            UIKit.Place(knob.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 100));
            zone.gameObject.AddComponent<Joystick>().Init(baseImg.rectTransform, knob.rectTransform);

            UIButton RB(string label, Color c, Vector2 pos, float size, System.Action act, bool onDown = true)
            {
                var b = UIKit.Round(touchRoot, label, c, act, size, size * 0.3f);
                UIKit.Place(b.Bg.rectTransform, new Vector2(1, 0), new Vector2(0.5f, 0.5f), pos + new Vector2(-safe.z, safe.y), new Vector2(size, size));
                b.FireOnDown = onDown;
                return b;
            }
            fireBtn = RB("ОГОНЬ", new Color(0.95f, 0.25f, 0.3f, 0.75f), new Vector2(-170, 260), 190, () => { var p = PlayerCtl.I; p.FireTap = true; });
            fireBtn.gameObject.AddComponent<FireHold>().btn = fireBtn;
            RB("ПРЫЖОК", new Color(0.2f, 0.6f, 1f, 0.7f), new Vector2(-90, 110), 120, () => PlayerCtl.I.JumpTap = true);
            RB("R", new Color(0.3f, 0.3f, 0.4f, 0.75f), new Vector2(-330, 140), 100, () => PlayerCtl.I.ReloadTap = true);
            zoomBtn = RB("ПРИЦЕЛ", new Color(0.3f, 0.3f, 0.4f, 0.75f), new Vector2(-350, 300), 110, () => PlayerCtl.I.ZoomToggle = true);
        }

        /// <summary>Keeps the fire flag set while the finger stays on the button (auto weapons), and lets the finger also aim.</summary>
        class FireHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
        {
            public UIButton btn;
            public void OnPointerDown(PointerEventData e) { PlayerCtl.I.FireHeld = true; }
            public void OnPointerUp(PointerEventData e) { PlayerCtl.I.FireHeld = false; }
            public void OnDrag(PointerEventData e)
            {
                float k = 1600f / Mathf.Max(800, Screen.width) * 0.9f;
                PlayerCtl.I.LookDelta += new Vector2(e.delta.x * k, -e.delta.y * k * 0.8f);
            }
            void OnDisable() { if (PlayerCtl.I != null) PlayerCtl.I.FireHeld = false; }
        }

        public static void SlotsChanged() { if (I != null) I.slotsDirty = true; }

        void RebuildSlots()
        {
            slotsDirty = false;
            foreach (var b in slotBtns) Destroy(b.gameObject);
            slotBtns.Clear();
            var p = PlayerCtl.I;
            for (int i = 0; i < p.Slots.Count; i++)
            {
                int idx = i;
                var s = p.Slots[i];
                var b = UIKit.Button(slotsRow, (Touch ? "" : (i + 1) + " ") + s.w.name, i == p.cur ? UIKit.Cyan * 0.8f : new Color(0.12f, 0.15f, 0.28f, 0.85f), () => p.Equip(idx), 20);
                b.Bg.rectTransform.sizeDelta = new Vector2(Touch ? 150 : 170, 62);
                b.FireOnDown = Touch;
                slotBtns.Add(b);
            }
        }

        public static void MatchStarted()
        {
            if (I == null) return;
            bool match = Game.I != null && Game.I.M != null;
            I.scoreA.gameObject.SetActive(match); I.scoreB.gameObject.SetActive(match); I.timer.gameObject.SetActive(match);
            I.shopBtn.gameObject.SetActive(!match);
            I.scoreBtn.gameObject.SetActive(match);
            I.partyBtn.gameObject.SetActive(!match);
            foreach (Transform c in I.feed) Destroy(c.gameObject);
            slotsDirtyStatic();
        }
        static void slotsDirtyStatic() { if (I != null) I.slotsDirty = true; }

        public static void HitMarker(bool head)
        {
            if (I == null) return;
            I.hitT = 0.25f;
            I.hit.color = head ? UIKit.Red : Color.white;
        }

        public static void Damage() { if (I != null) I.dmgT = 0.45f; }

        public static void Center(string text, float dur, Color c)
        {
            if (I == null) return;
            I.center.text = text; I.center.color = c;
            I.centerUntil = Time.time + dur;
        }

        public static void Toast(string text, Color c)
        {
            if (I == null) return;
            I.toast.text = text; I.toast.color = c;
            I.toastUntil = Time.time + 4f;
        }

        public static void KillFeed(string killer, int kt, string victim, int vt, string weapon)
        {
            if (I == null) return;
            var row = UIKit.Img(I.feed, UIKit.Glass, Color.white, "Kill");
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
            string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
            var t = UIKit.Txt(row.transform, $"<color=#{Hex(Defs.TeamColor(kt))}>{killer}</color>  <size=20><color=#9FB3CC>[{weapon}]</color></size>  <color=#{Hex(Defs.TeamColor(vt))}>{victim}</color>", 24, Color.white, TextAlignmentOptions.MidlineRight, false);
            UIKit.Fill(t.rectTransform, 12, 2, 12, 2);
            t.richText = true;
            Destroy(row.gameObject, 6f);
            if (I.feed.childCount > 6) Destroy(I.feed.GetChild(0).gameObject);
        }

        void Update()
        {
            var p = PlayerCtl.I;
            var g = Game.I;
            if (p == null || g == null) return;
            float dt = Time.deltaTime;
            bool menu = Menus.MainOpen;
            root.gameObject.SetActive(!menu);
            if (menu) return;
            if (slotsDirty) RebuildSlots();
            for (int i = 0; i < slotBtns.Count; i++) slotBtns[i].Bg.color = i == p.cur ? new Color(0.2f, 0.7f, 0.9f, 0.9f) : new Color(0.12f, 0.15f, 0.28f, 0.85f);

            dmgT = Mathf.Max(0, dmgT - dt);
            dmgFlash.color = new Color(1, 0, 0, dmgT * 0.6f + (p.alive ? (p.hp < 30 ? 0.12f : 0) : 0.25f));
            hitT = Mathf.Max(0, hitT - dt);
            var hc = hit.color; hc.a = hitT * 4; hit.color = hc;
            bool sniper = p.zoomed && p.Cur != null && p.Cur.w.zoom > 0;
            scope.gameObject.SetActive(sniper);
            reticle.gameObject.SetActive(p.zoomed && p.alive);
            reticleRing.gameObject.SetActive(!sniper);
            var rc = p.EnemyInSight ? new Color(1f, 0.2f, 0.25f, 1f) : new Color(0.3f, 1f, 0.5f, 0.95f);
            reticleRing.color = rc; reticleDot.color = rc;
            cross.gameObject.SetActive(!p.zoomed && p.alive);
            aimOffBtn.gameObject.SetActive(p.AimOn && p.alive && !Menus.AnyOpen);
            bool inMatch = g.M != null;
            medRow.gameObject.SetActive(inMatch);
            for (int i = 0; i < medBtns.Count; i++)
            {
                var md = Defs.Medkits[i];
                int cnt = Save.Meds(md.id);
                medBtns[i].Set((Touch ? "" : (6 + i) + ": ") + "+" + md.heal + " x" + cnt);
                medBtns[i].Bg.color = cnt > 0 ? md.color * 0.85f : new Color(0.25f, 0.25f, 0.3f, 0.8f);
            }

            hpText.text = Mathf.CeilToInt(p.hp).ToString();
            hpFill.rectTransform.sizeDelta = new Vector2(226 * Mathf.Clamp01(p.hp / 100), 18);
            hpFill.color = p.hp > 60 ? UIKit.Lime : p.hp > 30 ? UIKit.Gold : UIKit.Red;
            var s = p.Cur;
            if (s != null)
            {
                wName.text = s.w.name + (s.skin != "std" ? " · " + Defs.S(s.skin).name : "");
                ammoText.text = s.w.type == WType.Melee ? "" : s.reloadEnd > 0 ? "ПЕРЕЗАРЯДКА" : s.w.Hitscan || s.w.type == WType.Rocket ? s.ammo + " / " + s.reserve : "x" + s.ammo;
            }
            else { wName.text = ""; ammoText.text = ""; }

            var m = g.M;
            coins.text = "МОНЕТЫ: " + UIKit.Num(Save.P.coins) + (m != null && m.earned > 0 ? "  <color=#7CFF5A>+" + m.earned + "</color>" : "");
            var n = Net.I;
            partyLbl.text = m != null ? "" : (n.IsHost ? "Код отряда: " + Save.P.id : "Отряд " + n.Room) + "  ·  игроков: " + n.Members.Count + (n.Connected ? "" : "  · нет связи");
            if (m != null)
            {
                scoreA.text = m.score[1].ToString(); scoreB.text = m.score[2].ToString();
                float left = m.state == 0 ? m.duration : Mathf.Max(0, m.endAt - Time.time);
                timer.text = $"{(int)left / 60}:{(int)left % 60:00}";
                modeTxt.text = Defs.M(m.map).name + " · " + m.mode + " VS " + m.mode + " · до " + m.target + " · ты: " + Defs.TeamName(p.team);
                modeTxt.color = Defs.TeamColor(p.team);
                rangeTxt.text = "";
            }
            else
            {
                modeTxt.text = "ЛОББИ";
                modeTxt.color = UIKit.Muted;
                var pos = new Vector2(p.Pos.x, p.Pos.z);
                rangeTxt.text = Rect.MinMaxRect(-23, -5.5f, -9, 5.4f).Contains(pos) ? "Тир: попаданий " + RangeTarget.Hits : "";
            }

            center.gameObject.SetActive(Time.time < centerUntil || (!p.alive && m != null));
            if (!p.alive && m != null && Time.time >= centerUntil) { center.text = "Возрождение через " + Mathf.CeilToInt(Mathf.Max(0, p.respawnAt - Time.time)); center.color = Color.white; }
            toast.gameObject.SetActive(Time.time < toastUntil);

            string h = "";
            if (!Touch && Cursor.lockState != CursorLockMode.Locked && !Menus.AnyOpen) h = "Кликни мышкой, чтобы играть";
            else if (m == null && g.InShopZone) h = Touch ? "Нажми МАГАЗИН, чтобы купить оружие и скины" : "Нажми E — магазин оружия и скинов";
            else if (m == null && p.Pos.z > 18) h = "Встань в портал 1 VS 1 … 5 VS 5";
            else if (m != null && m.state == 0) h = "Приготовься!";
            else if (p.zoomed) h = "ПРИЦЕЛ ВКЛЮЧЁН — наведи на соперника, оружие выстрелит само";
            hint.text = h;
            if (zoomBtn != null) zoomBtn.Bg.color = p.zoomed ? new Color(1f, 0.25f, 0.6f, 0.85f) : new Color(0.3f, 0.3f, 0.4f, 0.75f);
            if (fireBtn != null) fireBtn.Set(s != null && s.w.type == WType.Melee ? "УДАР" : s != null && !s.w.Hitscan && s.w.type != WType.Rocket ? "БРОСОК" : "ОГОНЬ");
            if (touchRoot != null) touchRoot.gameObject.SetActive(!Menus.AnyOpen);

            bool tab = (!Touch && Input.GetKey(KeyCode.Tab)) || Menus.ScoreboardPinned;
            scoreboard.gameObject.SetActive(tab && m != null);
            if (tab && m != null) Menus.FillScoreboard(scoreboard);
        }
    }
}
