using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
    /// <summary>Main menu, shop (weapons + skins with prices), party, pause, location vote, results, scoreboard.</summary>
    public static class Menus
    {
        static GameObject main, vote, end;
        public static bool MainOpen => main != null;
        public static bool OverlayOpen => vote != null || end != null || main != null;
        public static bool AnyOpen => OverlayOpen || UIRoot.Open != null;
        public static bool ScoreboardPinned;

        public static void LockCursor()
        {
            if (Hud.Touch) return;
            if (AnyOpen) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void CloseAll()
        {
            Window.Close();
            CloseVote();
        }

        static RectTransform Overlay(string name, Color dim)
        {
            var bg = UIKit.Img(UIRoot.TopLayer, UIKit.White, dim, name, true);
            UIKit.Fill(bg.rectTransform);
            return bg.rectTransform;
        }

        // ================================================================== main menu

        public static void Main()
        {
            Unlock();
            var r = Overlay("MainMenu", new Color(0.02f, 0.03f, 0.1f, 0.78f));
            main = r.gameObject;
            var panel = UIKit.Img(r, UIKit.Panel, Color.white, "Panel", true);
            UIKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 700));
            var t = UIKit.Txt(panel.transform, "НЕОН <color=#FF3FA4>АРЕНА</color>", 92, UIKit.Cyan);
            t.richText = true;
            UIKit.Place(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(860, 120));
            var sub = UIKit.Txt(panel.transform, "Стрелялка с другом: лобби, магазин оружия и скинов, 4 локации, 1 VS 1 … 5 VS 5", 26, UIKit.Muted);
            UIKit.Place(sub.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(820, 70));
            var lbl = UIKit.Txt(panel.transform, "Твой ник:", 28, Color.white, TextAlignmentOptions.MidlineLeft);
            UIKit.Place(lbl.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-230, -250), new Vector2(300, 50));
            var input = UIKit.Input(panel.transform, "Например, Снайпер", 14, 34);
            UIKit.Place(input.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(90, -250), new Vector2(460, 74));
            input.text = Save.P.name;
            var play = UIKit.Button(panel.transform, "ИГРАТЬ", UIKit.Pink, () =>
            {
                var n = input.text.Trim();
                if (n.Length == 0) n = "Игрок " + Save.P.id.Substring(0, 3);
                Save.P.name = n;
                Save.Write();
                Net.I.AddMember(Save.P.id, n);
                Object.Destroy(main); main = null;
                AudioFx.Play("go");
                LockCursor();
                if (!string.IsNullOrEmpty(Save.P.lastRoom)) { Net.I.JoinRoom(Save.P.lastRoom); Hud.Toast("Возвращаюсь в отряд " + Save.P.lastRoom + "…", UIKit.Cyan); }
            }, 44);
            UIKit.Place(play.Bg.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -360), new Vector2(460, 100));
            var help = UIKit.Txt(panel.transform,
                (Hud.Touch ? "Левый палец — ходить, правый — смотреть, кнопка ОГОНЬ — стрелять.\n" :
                "WASD — ходить · мышь — целиться · ЛКМ — огонь · ПКМ — прицел · R — перезарядка\n1-5 / колесо — оружие · Shift — бег · Пробел — прыжок · E — магазин · Tab — счёт\n") +
                "Играть с другом: ОТРЯД → отправь другу свой код → он вводит его у себя.\nПотом встаньте в портал — у обоих появится выбор локации.", 23, UIKit.Muted);
            UIKit.Place(help.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(840, 190));
        }

        // ================================================================== pause

        public static void Pause()
        {
            if (OverlayOpen) return;
            Unlock();
            Window.Show("МЕНЮ", w =>
            {
                w.Row("Продолжить", null, "ИГРАТЬ", UIKit.Lime, () => { Window.Close(); LockCursor(); });
                var g = Game.I;
                if (g.M == null) w.Row("Магазин оружия и скинов", "Монеты: " + UIKit.Num(Save.P.coins), "ОТКРЫТЬ", UIKit.Gold, () => Shop());
                else w.Row("Выйти из боя в лобби", Net.I.IsHost ? "Ты лидер — бой закончится для всех" : "Бой продолжится без тебя", "ВЫЙТИ", UIKit.Red, () => { Window.Close(); g.LeaveMatch(); });
                w.Row("Отряд (игра с другом)", Net.I.IsHost ? "Твой код: " + Save.P.id : "Ты в отряде " + Net.I.Room, "ОТКРЫТЬ", UIKit.Violet, () => Party());
                var sens = new List<(string, Color, bool, System.Action)>();
                foreach (var v in new[] { 0.4f, 0.6f, 0.8f, 1f, 1.3f, 1.6f, 2f })
                {
                    float vv = v;
                    sens.Add((v.ToString("0.0"), new Color(0.2f, 0.3f, 0.5f), Mathf.Abs(Save.P.sens - v) < 0.01f, () => { Save.P.sens = vv; Save.Write(); Window.Refresh(); }));
                }
                w.Chips("Чувствительность", sens);
                var vol = new List<(string, Color, bool, System.Action)>();
                foreach (var v in new[] { 0f, 0.3f, 0.6f, 0.8f, 1f })
                {
                    float vv = v;
                    vol.Add((v == 0 ? "ВЫКЛ" : (int)(v * 100) + "%", new Color(0.2f, 0.3f, 0.5f), Mathf.Abs(Save.P.volume - v) < 0.01f, () => { Save.P.volume = vv; AudioFx.SetVolume(vv); Save.Write(); Window.Refresh(); }));
                }
                w.Chips("Громкость", vol);
                w.Text($"Статистика: боёв {Save.P.matches}, побед {Save.P.wins}, убийств {Save.P.kills}, смертей {Save.P.deaths}", 24, UIKit.Muted);
                w.Footer.text = "Неон Арена · ник " + Save.P.name;
            }, 1000, 780);
        }

        // ================================================================== party

        public static void Party()
        {
            if (OverlayOpen) return;
            Unlock();
            Window.Show("ОТРЯД — ИГРА С ДРУГОМ", w =>
            {
                var n = Net.I;
                if (n.IsHost)
                {
                    w.Text("Твой код отряда:", 28, Color.white);
                    w.Text("<size=80><color=#FFC83A>" + Save.P.id + "</color></size>", 80, Color.white);
                    w.Text("Скажи этот код другу — он нажмёт ОТРЯД и введёт его у себя. Вы окажетесь в одном лобби.", 24, UIKit.Muted);
                }
                else
                {
                    w.Text("Ты в отряде <color=#FFC83A>" + n.Room + "</color> (лидер: " + n.NameOf(n.Room) + ")", 32, Color.white);
                    w.Row("Выйти из отряда", "Вернуться в своё лобби", "ВЫЙТИ", UIKit.Red, () => { n.LeaveRoom(); Window.Refresh(); });
                }
                w.Text("Игроки (" + n.Members.Count + "):", 28, UIKit.Cyan);
                foreach (var m in n.Members.Values.OrderBy(x => x.id != n.Room))
                    w.Text((m.id == n.Room ? "★ " : "• ") + (m.name ?? m.id) + (m.id == Save.P.id ? " (ты)" : "") + (m.id == n.Room ? " — лидер" : ""), 26, Color.white);
                // join by code
                var bg = UIKit.Img(w.Content, UIKit.Glass, Color.white, "Join");
                bg.gameObject.AddComponent<LayoutElement>().minHeight = 110;
                var lbl = UIKit.Txt(bg.transform, "Код друга:", 28, Color.white, TextAlignmentOptions.MidlineLeft);
                UIKit.Place(lbl.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(200, 60));
                var input = UIKit.Input(bg.transform, "ABC123", 8, 34);
                UIKit.Place(input.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(220, 0), new Vector2(330, 76));
                var join = UIKit.Button(bg.transform, "ВОЙТИ", UIKit.Lime, () =>
                {
                    var code = input.text.Trim().ToUpperInvariant();
                    if (code.Length < 4) { AudioFx.Play("error"); return; }
                    if (code == Save.P.id) { Hud.Toast("Это твой собственный код", UIKit.Gold); return; }
                    if (Game.I.M != null) { Hud.Toast("Сначала выйди из боя", UIKit.Gold); return; }
                    n.JoinRoom(code);
                    Hud.Toast("Подключаюсь к отряду " + code + "…", UIKit.Cyan);
                    Window.Refresh();
                }, 30);
                UIKit.Place(join.Bg.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-16, 0), new Vector2(230, 76));
                w.Text(n.Connected ? "Связь с сервером: есть" : "Связь с сервером: подключение… (нужен интернет)", 22, n.Connected ? UIKit.Lime : UIKit.Gold);
                w.Footer.text = "Когда вы вместе — встаньте в портал, у всех появится выбор локации";
            }, 1000, 800);
        }

        public static void RefreshParty()
        {
            if (UIRoot.Open != null && UIRoot.Open.Title.text.StartsWith("ОТРЯД")) Window.Refresh();
        }

        // ================================================================== shop

        static string shopCat = "primary", shopW = "ak74", shopSkin = "std";
        static Camera previewCam;
        static RenderTexture previewRT;
        static Transform previewRoot;
        static GameObject previewModel;
        static string previewKey;

        public static void Shop()
        {
            if (OverlayOpen || Game.I.M != null) return;
            Unlock();
            if (Defs.W(shopW)?.cat != shopCat) shopW = Defs.Weapons.First(x => x.cat == shopCat).id;
            Window.Show("МАГАЗИН ОРУЖИЯ И СКИНОВ", FillShop, 1300, 860).OnClose = () => { if (previewCam != null) previewCam.enabled = false; LockCursor(); };
        }

        static void FillShop(Window w)
        {
            var cats = new List<(string, Color, bool, System.Action)>();
            for (int i = 0; i < Defs.Cats.Length; i++)
            {
                string c = Defs.Cats[i];
                cats.Add((Defs.CatNames[i], new Color(0.2f, 0.25f, 0.5f), shopCat == c, () => { shopCat = c; shopW = Defs.Weapons.First(x => x.cat == c).id; shopSkin = Save.SkinOf(shopW); Window.Refresh(); }));
            }
            w.Chips("Раздел", cats);
            var ws = new List<(string, Color, bool, System.Action)>();
            foreach (var x in Defs.Weapons.Where(x => x.cat == shopCat))
            {
                var id = x.id;
                ws.Add((x.name, Save.Owns(id) ? new Color(0.15f, 0.45f, 0.3f) : new Color(0.3f, 0.25f, 0.45f), shopW == id, () => { shopW = id; shopSkin = Save.SkinOf(id); Window.Refresh(); }));
            }
            w.Chips("Оружие", ws);

            var wd = Defs.W(shopW);
            // 3D preview
            var holder = UIKit.Img(w.Content, UIKit.Glass, Color.white, "Preview");
            holder.gameObject.AddComponent<LayoutElement>().minHeight = 300;
            var raw = UIKit.Rect("Raw", holder.transform).gameObject.AddComponent<RawImage>();
            UIKit.Fill(raw.rectTransform, 8, 8, 8, 8);
            raw.texture = Preview(wd.id, shopSkin);
            raw.raycastTarget = false;

            bool owned = Save.Owns(wd.id);
            bool equipped = Save.Slot(wd.cat) == wd.id;
            string stats = wd.type == WType.Melee ? $"Урон {wd.dmg} · ближний бой" : wd.Projectile ? $"Урон {wd.dmg}{(wd.splash > 0 ? " · взрыв " + wd.splash + " м" : "")} · штук {wd.mag}" : $"Урон {wd.dmg} · {wd.rate:0.#} выстр/с · магазин {wd.mag}{(wd.zoom > 0 ? " · оптика x" + wd.zoom : "")}";
            if (!owned)
                w.Row(wd.name, stats + " · цена " + UIKit.Num(wd.price), "КУПИТЬ " + UIKit.Num(wd.price), UIKit.Gold, () =>
                {
                    if (!Save.Buy(wd.price)) { AudioFx.Play("error"); Hud.Toast("Не хватает монет — побеждай в боях!", UIKit.Red); return; }
                    Save.P.owned.Add(wd.id);
                    Save.SetSlot(wd.cat, wd.id);
                    AudioFx.Play("buy");
                    PlayerCtl.I.BuildLoadout(true);
                    Window.Refresh();
                }, Save.P.coins >= wd.price);
            else
                w.Row(wd.name + "  (куплено)", stats, equipped ? "В БОЮ" : "ВЗЯТЬ В БОЙ", equipped ? new Color(0.2f, 0.5f, 0.35f) : UIKit.Lime, () =>
                {
                    Save.SetSlot(wd.cat, wd.id);
                    AudioFx.Play("click");
                    PlayerCtl.I.BuildLoadout(true);
                    Window.Refresh();
                });

            var sk = new List<(string, Color, bool, System.Action)>();
            foreach (var s in Defs.Skins)
            {
                var sid = s.id;
                bool has = Save.OwnsSkin(wd.id, sid);
                sk.Add(((has ? "" : "") + s.name, s.tint * (has ? 1f : 0.7f), shopSkin == sid, () => { shopSkin = sid; Window.Refresh(); }));
            }
            w.Chips("Скины для «" + wd.name + "» (нажми, чтобы примерить)", sk);
            var sd = Defs.S(shopSkin);
            int price = Defs.SkinPrice(wd.id, sd.id);
            bool hasSkin = Save.OwnsSkin(wd.id, sd.id);
            bool wearing = Save.SkinOf(wd.id) == sd.id;
            if (!owned) w.Row("Скин «" + sd.name + "»", "Сначала купи оружие · цена скина " + UIKit.Num(price), null, Color.gray, null);
            else if (!hasSkin)
                w.Row("Скин «" + sd.name + "»", "Цена " + UIKit.Num(price) + " монет", "КУПИТЬ " + UIKit.Num(price), UIKit.Gold, () =>
                {
                    if (!Save.Buy(price)) { AudioFx.Play("error"); Hud.Toast("Не хватает монет", UIKit.Red); return; }
                    Save.P.skins.Add(wd.id + ":" + sd.id);
                    Save.SetSkin(wd.id, sd.id);
                    AudioFx.Play("buy");
                    PlayerCtl.I.BuildLoadout(true);
                    Window.Refresh();
                }, Save.P.coins >= price);
            else
                w.Row("Скин «" + sd.name + "»", "Куплен", wearing ? "НАДЕТ" : "НАДЕТЬ", wearing ? new Color(0.2f, 0.5f, 0.35f) : UIKit.Lime, () =>
                {
                    Save.SetSkin(wd.id, sd.id);
                    AudioFx.Play("click");
                    PlayerCtl.I.BuildLoadout(true);
                    Window.Refresh();
                });
            w.Footer.text = "Монеты: " + UIKit.Num(Save.P.coins) + "  ·  +" + Defs.RewardKill + " за победу над врагом, +" + Defs.RewardWin + " за выигранный бой";
        }

        /// <summary>Renders the weapon with the chosen skin, slowly turning, into a texture for the shop.</summary>
        static RenderTexture Preview(string wid, string skin)
        {
            if (previewCam == null)
            {
                previewRT = new RenderTexture(1024, 300, 16) { name = "ShopPreview" };
                previewRoot = new GameObject("ShopPreview").transform;
                previewRoot.position = new Vector3(0, -500, 0);
                var cg = new GameObject("Cam");
                cg.transform.SetParent(previewRoot, false);
                cg.transform.localPosition = new Vector3(0, 0.05f, -1.6f);
                previewCam = cg.AddComponent<Camera>();
                previewCam.cullingMask = 1 << Layers.Icon;
                previewCam.clearFlags = CameraClearFlags.SolidColor;
                previewCam.backgroundColor = new Color(0.05f, 0.07f, 0.16f, 1);
                previewCam.fieldOfView = 28;
                previewCam.targetTexture = previewRT;
                var lg = new GameObject("Key"); lg.transform.SetParent(previewRoot, false); lg.transform.localPosition = new Vector3(0.6f, 0.8f, -1.2f);
                var l = lg.AddComponent<Light>(); l.type = LightType.Point; l.range = 5; l.intensity = 4; l.cullingMask = 1 << Layers.Icon;
                var lg2 = new GameObject("Rim"); lg2.transform.SetParent(previewRoot, false); lg2.transform.localPosition = new Vector3(-0.8f, 0.3f, 0.6f);
                var l2 = lg2.AddComponent<Light>(); l2.type = LightType.Point; l2.range = 5; l2.intensity = 3; l2.color = new Color(0.5f, 0.7f, 1f); l2.cullingMask = 1 << Layers.Icon;
                previewRoot.gameObject.AddComponent<Spin>();
            }
            previewCam.enabled = true;
            string key = wid + ":" + skin;
            if (key != previewKey)
            {
                previewKey = key;
                if (previewModel != null) Object.Destroy(previewModel);
                var holder = new GameObject("Model").transform;
                holder.SetParent(previewRoot, false);
                previewModel = holder.gameObject;
                var m = Models.Weapon(wid, skin, holder, Layers.Icon);
                var b = new Bounds();
                bool first = true;
                foreach (var r in m.GetComponentsInChildren<Renderer>()) { if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
                float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
                float k = 1.15f / Mathf.Max(0.05f, size);
                m.transform.localScale = Vector3.one * k;
                m.transform.position -= (b.center - holder.position) * k;
                if (wid == "shuriken") holder.localRotation = Quaternion.Euler(-70, 0, 0);
            }
            return previewRT;
        }

        class Spin : MonoBehaviour
        {
            void Update()
            {
                var m = transform.Find("Model");
                if (m != null) m.Rotate(0, 35 * Time.unscaledDeltaTime, 0, Space.World);
            }
        }

        // ================================================================== vote

        public static void ShowVote()
        {
            CloseVote();
            Window.Close();
            Unlock();
            var r = Overlay("Vote", new Color(0.02f, 0.02f, 0.08f, 0.82f));
            vote = r.gameObject;
            vote.AddComponent<VoteTicker>();
            UpdateVote();
            AudioFx.Play("portal");
        }

        public static void CloseVote()
        {
            if (vote != null) Object.Destroy(vote);
            vote = null;
            if (!AnyOpen) LockCursor();
        }

        class VoteTicker : MonoBehaviour
        {
            public TextMeshProUGUI timer;
            void Update()
            {
                var v = Game.I.V;
                if (v == null || timer == null) return;
                timer.text = v.result != null ? "Локация выбрана!" : "Осталось " + Mathf.CeilToInt(Mathf.Max(0, v.endAt - Time.time)) + " с";
                if (!Hud.Touch && v.result == null)
                    for (int i = 0; i < Defs.Maps.Count; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Game.I.Vote(Defs.Maps[i].id);
            }
        }

        public static void UpdateVote()
        {
            if (vote == null || Game.I.V == null) return;
            var v = Game.I.V;
            var r = (RectTransform)vote.transform;
            for (int i = r.childCount - 1; i >= 0; i--) Object.Destroy(r.GetChild(i).gameObject);
            var title = UIKit.Txt(r, $"{v.mode} VS {v.mode} — ВЫБЕРИТЕ ЛОКАЦИЮ", 54, UIKit.Cyan);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1500, 70));
            var timer = UIKit.Txt(r, "", 30, UIKit.Gold);
            UIKit.Place(timer.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(800, 44));
            vote.GetComponent<VoteTicker>().timer = timer;
            var n = Net.I;
            float cw = 360, gap = 24, total = Defs.Maps.Count * cw + (Defs.Maps.Count - 1) * gap;
            for (int i = 0; i < Defs.Maps.Count; i++)
            {
                var md = Defs.Maps[i];
                var id = md.id;
                bool mine = v.votes.TryGetValue(Save.P.id, out var my) && my == id;
                bool chosen = v.result == id;
                var card = UIKit.Img(r, UIKit.Panel, chosen ? UIKit.Gold : mine ? UIKit.Lime : Color.white, "Card " + id, true);
                UIKit.Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-total / 2 + i * (cw + gap), -10), new Vector2(cw, 560));
                var btn = card.gameObject.AddComponent<UIButton>();
                btn.Bg = card;
                btn.OnClick = () => Game.I.Vote(id);
                var img = UIKit.Rect("Img", card.transform).gameObject.AddComponent<RawImage>();
                img.texture = Resources.Load<Texture2D>("Maps/" + id);
                img.raycastTarget = false;
                UIKit.Place(img.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(cw - 28, 300));
                if (img.texture != null) { float asp = (float)img.texture.width / img.texture.height; img.uvRect = asp > (cw - 28) / 300f ? new Rect(0.5f - (cw - 28) / 300f / asp / 2, 0, (cw - 28) / 300f / asp, 1) : new Rect(0, 0.5f - asp / ((cw - 28) / 300f) / 2, 1, asp / ((cw - 28) / 300f)); }
                var nm = UIKit.Txt(card.transform, (Hud.Touch ? "" : (i + 1) + ". ") + md.name, 40, Color.white);
                UIKit.Place(nm.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(cw - 20, 56));
                var ds = UIKit.Txt(card.transform, md.desc, 22, UIKit.Muted, TextAlignmentOptions.Top, false);
                UIKit.Place(ds.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -378), new Vector2(cw - 30, 60));
                var voters = v.votes.Where(kv => kv.Value == id).Select(kv => kv.Key == Save.P.id ? "ты" : n.NameOf(kv.Key)).ToList();
                var vt = UIKit.Txt(card.transform, voters.Count > 0 ? "Голоса: " + string.Join(", ", voters) : "Нет голосов", 24, voters.Count > 0 ? UIKit.Lime : UIKit.Muted, TextAlignmentOptions.Top, false);
                UIKit.Place(vt.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(cw - 30, 100));
                if (chosen)
                {
                    var tag = UIKit.Txt(card.transform, "ВЫБРАНО!", 40, UIKit.Gold);
                    UIKit.Place(tag.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(cw, 60));
                }
            }
            // team mode + cancel
            string tm = v.teamMode == 0 ? "Команды: друзья ПРОТИВ друг друга (остальные места — боты)" : "Команды: друзья ВМЕСТЕ против ботов";
            if (n.IsHost && v.result == null)
            {
                var b1 = UIKit.Button(r, "ДРУГ ПРОТИВ ДРУГА", v.teamMode == 0 ? UIKit.Pink : new Color(0.25f, 0.28f, 0.45f), () => Game.I.SetTeamMode(0), 26);
                UIKit.Place(b1.Bg.rectTransform, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(-10, 40), new Vector2(380, 76));
                var b2 = UIKit.Button(r, "ВМЕСТЕ ПРОТИВ БОТОВ", v.teamMode == 1 ? UIKit.Pink : new Color(0.25f, 0.28f, 0.45f), () => Game.I.SetTeamMode(1), 26);
                UIKit.Place(b2.Bg.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(10, 40), new Vector2(400, 76));
                var c = UIKit.Button(r, "ОТМЕНА", UIKit.Red, () => Game.I.CancelVote(), 26);
                UIKit.Place(c.Bg.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 40), new Vector2(200, 76));
            }
            else
            {
                var t2 = UIKit.Txt(r, tm, 28, Color.white);
                UIKit.Place(t2.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1400, 50));
            }
            var who = UIKit.Txt(r, "В отряде: " + string.Join(", ", n.Members.Values.Select(m => m.id == Save.P.id ? "ты" : (m.name ?? m.id))) + (n.Members.Count == 1 ? "  (один — против ботов)" : ""), 24, UIKit.Muted);
            UIKit.Place(who.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 130), new Vector2(1400, 40));
        }

        // ================================================================== end of match

        public static void ShowEnd(int winner, bool abort, int reward, int total)
        {
            CloseEnd();
            Window.Close();
            Unlock();
            var g = Game.I;
            var m = g.M;
            var r = Overlay("End", new Color(0.02f, 0.02f, 0.08f, 0.85f));
            end = r.gameObject;
            var me = PlayerCtl.I.team;
            string head = abort ? "БОЙ ОСТАНОВЛЕН" : winner == 0 ? "НИЧЬЯ" : winner == me ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
            var t = UIKit.Txt(r, head, 100, abort ? UIKit.Muted : winner == me ? UIKit.Lime : winner == 0 ? UIKit.Gold : UIKit.Red);
            UIKit.Place(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(1400, 130));
            var sc = UIKit.Txt(r, $"<color=#{ColorUtility.ToHtmlStringRGB(Defs.TeamA)}>СИНИЕ {m.score[1]}</color>  :  <color=#{ColorUtility.ToHtmlStringRGB(Defs.TeamB)}>{m.score[2]} ОРАНЖЕВЫЕ</color>", 52, Color.white);
            sc.richText = true;
            UIKit.Place(sc.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1400, 70));
            var board = UIKit.Rect("Board", r);
            UIKit.Place(board, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1100, 400));
            FillScoreboard(board);
            var coins = UIKit.Txt(r, $"Монеты за бой: +{total}  (за победы над врагами {total - reward}, за итог {reward})", 32, UIKit.Gold);
            UIKit.Place(coins.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1400, 50));
            var b = UIKit.Button(r, "В ЛОББИ", UIKit.Pink, () => { CloseEnd(); Game.I.ToLobby(); LockCursor(); }, 40);
            UIKit.Place(b.Bg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(420, 96));
        }

        public static void CloseEnd()
        {
            if (end != null) Object.Destroy(end);
            end = null;
        }

        // ================================================================== scoreboard

        public static void ToggleScoreboard() { ScoreboardPinned = !ScoreboardPinned; }

        public static void FillScoreboard(RectTransform box)
        {
            var g = Game.I;
            var m = g.M;
            if (m == null) return;
            for (int i = box.childCount - 1; i >= 0; i--) Object.Destroy(box.GetChild(i).gameObject);
            var bg = UIKit.Img(box, UIKit.Panel, Color.white, "Bg");
            UIKit.Fill(bg.rectTransform);
            for (int team = 1; team <= 2; team++)
            {
                float x = team == 1 ? 30 : 565;
                var h = UIKit.Txt(bg.transform, Defs.TeamName(team) + "  " + m.score[team], 34, Defs.TeamColor(team), TextAlignmentOptions.MidlineLeft);
                UIKit.Place(h.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -20), new Vector2(500, 50));
                var hdr = UIKit.Txt(bg.transform, "убил / погиб", 20, UIKit.Muted, TextAlignmentOptions.MidlineRight, false);
                UIKit.Place(hdr.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -64), new Vector2(500, 30));
                int row = 0;
                foreach (var kv in m.stats.Where(kv => m.Team(kv.Key) == team).OrderByDescending(kv => kv.Value.x))
                {
                    bool me = kv.Key == Save.P.id;
                    var name = g.NameOf(kv.Key) + (me ? " (ты)" : "");
                    var t = UIKit.Txt(bg.transform, name, 26, me ? UIKit.Gold : Color.white, TextAlignmentOptions.MidlineLeft, false);
                    UIKit.Place(t.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -100 - row * 40), new Vector2(380, 38));
                    var k = UIKit.Txt(bg.transform, kv.Value.x + " / " + kv.Value.y, 26, Color.white, TextAlignmentOptions.MidlineRight, false);
                    UIKit.Place(k.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x + 380, -100 - row * 40), new Vector2(120, 38));
                    row++;
                }
            }
        }
    }
}
