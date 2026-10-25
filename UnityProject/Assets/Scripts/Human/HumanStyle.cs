using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Real
{
    public static partial class Wardrobe
    {
        /// <summary>The human's own wardrobe (2.15): body, face, hair, clothes, shoes, accessories.</summary>
        static void AddHuman(List<SkinDef> list)
        {
            void A(Slot slot, string id, string name, string style, Color c, Rarity r = Rarity.Common, Color? c2 = null, Color? c3 = null, string pattern = null, int number = -1, bool none = false) =>
                list.Add(new SkinDef { slot = slot, id = id, name = name, style = style, color = c, color2 = c2 ?? c, color3 = c3 ?? Color.white, pattern = pattern, number = number, rarity = r, none = none, dept = "human" });

            list.Add(new SkinDef { slot = Slot.Character, id = "human", name = "Человек", style = "human", rarity = Rarity.Epic });

            A(Slot.HGender, "hg_m", "Парень", "m", Color.white);
            A(Slot.HGender, "hg_f", "Девушка", "f", Color.white);

            A(Slot.HFace, "hface_oval", "Овальное лицо", "0", Color.white);
            A(Slot.HFace, "hface_round", "Круглое лицо", "1", Color.white);
            A(Slot.HFace, "hface_square", "Квадратное, волевой подбородок", "2", Color.white);
            A(Slot.HFace, "hface_long", "Вытянутое лицо", "3", Color.white);
            A(Slot.HFace, "hface_heart", "Широкие скулы, узкий подбородок", "4", Color.white);
            A(Slot.HFace, "hface_chubby", "Пухлые щёки", "5", Color.white, Rarity.Uncommon);

            A(Slot.HSkin, "hs_mannequin", "Манекен", "mannequin", new Color(0.9f, 0.9f, 0.9f));
            A(Slot.HSkin, "hs_light", "Светлая кожа", "skin", new Color(0.98f, 0.82f, 0.71f));
            A(Slot.HSkin, "hs_peach", "Персиковая", "skin", new Color(0.94f, 0.74f, 0.6f));
            A(Slot.HSkin, "hs_tan", "Загорелая", "skin", new Color(0.82f, 0.6f, 0.44f));
            A(Slot.HSkin, "hs_olive", "Смуглая", "skin", new Color(0.7f, 0.5f, 0.35f));
            A(Slot.HSkin, "hs_brown", "Коричневая", "skin", new Color(0.5f, 0.33f, 0.22f));
            A(Slot.HSkin, "hs_dark", "Тёмная", "skin", new Color(0.33f, 0.21f, 0.15f));
            A(Slot.HSkin, "hs_grey", "Серый манекен", "mannequin", new Color(0.45f, 0.47f, 0.5f), Rarity.Rare);
            A(Slot.HSkin, "hs_gold", "Золотой", "gold", new Color(1f, 0.78f, 0.3f), Rarity.Legendary);
            A(Slot.HSkin, "hs_alien", "Инопланетянин", "skin", new Color(0.45f, 0.82f, 0.45f), Rarity.Epic);

            A(Slot.HHair, "hh_none", "Без волос", null, Color.white, none: true);
            var hairCols = new (string k, string n, Color c)[]
            {
                ("black", "чёрные", new Color(0.07f, 0.06f, 0.06f)), ("brown", "каштановые", new Color(0.32f, 0.19f, 0.1f)),
                ("blonde", "светлые", new Color(0.88f, 0.74f, 0.45f)), ("red", "рыжие", new Color(0.72f, 0.3f, 0.12f)),
                ("white", "белые", new Color(0.92f, 0.92f, 0.94f)), ("blue", "синие", new Color(0.18f, 0.38f, 0.92f)), ("pink", "розовые", new Color(0.96f, 0.45f, 0.72f)),
            };
            var hairStyles = new (string k, string n, Rarity r)[]
            {
                ("short", "Короткие", Rarity.Common), ("buzz", "Ёжик", Rarity.Common), ("bangs", "Чёлка", Rarity.Common), ("long", "Длинные", Rarity.Uncommon),
                ("ponytail", "Хвостик", Rarity.Uncommon), ("bun", "Пучок", Rarity.Uncommon), ("curly", "Кудри", Rarity.Rare), ("spiky", "Торчком", Rarity.Rare),
                ("mohawk", "Ирокез", Rarity.Epic), ("afro", "Афро", Rarity.Epic),
            };
            foreach (var st in hairStyles)
                foreach (var hc in hairCols)
                {
                    bool fun = hc.k == "blue" || hc.k == "pink" || hc.k == "white";
                    A(Slot.HHair, $"hh_{st.k}_{hc.k}", $"{st.n} {hc.n}", st.k, hc.c, fun && st.r < Rarity.Rare ? Rarity.Rare : st.r);
                }

            A(Slot.HEyes, "he_none", "Без глаз", null, Color.white, none: true);
            A(Slot.HEyes, "he_brown", "Карие глаза", "normal", new Color(0.35f, 0.2f, 0.1f));
            A(Slot.HEyes, "he_blue", "Голубые глаза", "normal", new Color(0.25f, 0.55f, 0.95f));
            A(Slot.HEyes, "he_green", "Зелёные глаза", "normal", new Color(0.25f, 0.65f, 0.35f));
            A(Slot.HEyes, "he_grey", "Серые глаза", "normal", new Color(0.5f, 0.55f, 0.6f));
            A(Slot.HEyes, "he_big", "Большие глаза", "big", new Color(0.3f, 0.55f, 1f), Rarity.Uncommon);
            A(Slot.HEyes, "he_happy", "Довольные", "happy", new Color(0.1f, 0.08f, 0.08f), Rarity.Uncommon);
            A(Slot.HEyes, "he_sleepy", "Сонные", "sleepy", new Color(0.35f, 0.2f, 0.1f), Rarity.Uncommon);
            A(Slot.HEyes, "he_angry", "Сердитые", "angry", new Color(0.35f, 0.2f, 0.1f), Rarity.Rare);
            A(Slot.HEyes, "he_wink", "Подмигивает", "wink", new Color(0.25f, 0.55f, 0.95f), Rarity.Rare);
            A(Slot.HEyes, "he_wow", "Удивлённые", "wow", new Color(0.25f, 0.45f, 0.3f), Rarity.Rare);
            A(Slot.HEyes, "he_lashes", "С ресницами", "lashes", new Color(0.3f, 0.5f, 0.85f), Rarity.Uncommon);

            A(Slot.HMouth, "hm_none", "Без рта", null, Color.white, none: true);
            A(Slot.HMouth, "hm_smile", "Улыбка", "smile", new Color(0.55f, 0.22f, 0.22f));
            A(Slot.HMouth, "hm_grin", "Улыбка до ушей", "grin", new Color(0.35f, 0.08f, 0.1f), Rarity.Uncommon);
            A(Slot.HMouth, "hm_neutral", "Спокойный", "neutral", new Color(0.5f, 0.25f, 0.25f));
            A(Slot.HMouth, "hm_open", "Открытый рот", "open", new Color(0.3f, 0.06f, 0.08f), Rarity.Uncommon);
            A(Slot.HMouth, "hm_tongue", "Язык", "tongue", new Color(0.55f, 0.22f, 0.22f), Rarity.Rare);
            A(Slot.HMouth, "hm_smirk", "Ухмылка", "smirk", new Color(0.55f, 0.22f, 0.22f), Rarity.Uncommon);
            A(Slot.HMouth, "hm_sad", "Грустный", "sad", new Color(0.5f, 0.25f, 0.25f), Rarity.Uncommon);
            A(Slot.HMouth, "hm_lips", "Красные губы", "lips", new Color(0.85f, 0.1f, 0.2f), Rarity.Rare);
            A(Slot.HMouth, "hm_beard", "Борода", "beard", new Color(0.3f, 0.18f, 0.1f), Rarity.Rare);

            Color P(string k) => Pal(k);
            A(Slot.HTop, "ht_none", "Без футболки", null, Color.white, none: true);
            foreach (var k in new[] { "white", "black", "red", "blue", "green", "yellow", "pink", "orange", "purple", "navy" })
                A(Slot.HTop, "ht_tee_" + k, Adj(k, "ая футболка"), "tee", P(k));
            A(Slot.HTop, "ht_tee_grey", "Серая футболка", "tee", new Color(0.55f, 0.56f, 0.58f));
            A(Slot.HTop, "ht_print_star", "Футболка со звездой", "tee", P("black"), Rarity.Uncommon, P("gold"), pattern: "star");
            A(Slot.HTop, "ht_print_heart", "Футболка с сердцем", "tee", P("white"), Rarity.Uncommon, P("red"), pattern: "heart");
            A(Slot.HTop, "ht_print_bolt", "Футболка с молнией", "tee", P("navy"), Rarity.Uncommon, P("yellow"), pattern: "bolt");
            A(Slot.HTop, "ht_print_smile", "Футболка со смайлом", "tee", P("yellow"), Rarity.Uncommon, P("black"), pattern: "smile");
            A(Slot.HTop, "ht_print_hay", "Футболка HAY HUNTER", "tee", P("green"), Rarity.Rare, P("white"), pattern: "hay");
            A(Slot.HTop, "ht_stripes", "Тельняшка", "tee", P("white"), Rarity.Uncommon, P("navy"), pattern: "stripes");
            A(Slot.HTop, "ht_stripes_red", "Полосатая красная", "tee", P("white"), Rarity.Uncommon, P("red"), pattern: "stripes");
            foreach (var k in new[] { "white", "black", "red", "blue" })
                A(Slot.HTop, "ht_tank_" + k, Adj(k, "ая майка"), "tank", P(k));
            foreach (var (k, n) in new[] { ("grey", "Серое худи"), ("black", "Чёрное худи"), ("red", "Красное худи"), ("blue", "Синее худи"), ("green", "Зелёное худи"), ("pink", "Розовое худи") })
                A(Slot.HTop, "ht_hoodie_" + k, n, "hoodie", k == "grey" ? new Color(0.55f, 0.56f, 0.58f) : P(k), Rarity.Uncommon);
            foreach (var (k, n, c) in new[] { ("red", "Красная клетчатая рубашка", P("red")), ("blue", "Синяя клетчатая рубашка", P("blue")), ("green", "Зелёная клетчатая рубашка", P("green")), ("black", "Чёрно-белая клетчатая", P("black")) })
                A(Slot.HTop, "ht_flannel_" + k, n, "flannel", c, Rarity.Rare, k == "black" ? P("white") : Color.Lerp(c, Color.black, 0.55f));
            A(Slot.HTop, "ht_shirt_white", "Белая рубашка", "shirt", P("white"), Rarity.Uncommon);
            A(Slot.HTop, "ht_shirt_sky", "Голубая рубашка", "shirt", P("sky"), Rarity.Uncommon);
            foreach (var (k, num, c, c2) in new[] { ("jersey_10", 10, P("red"), P("white")), ("jersey_7", 7, P("white"), P("red")), ("jersey_23", 23, P("yellow"), P("purple")), ("jersey_99", 99, P("blue"), P("white")) })
                A(Slot.HTop, "ht_" + k, "Спортивная майка №" + num, "jersey", c, Rarity.Rare, c2, number: num);

            A(Slot.HBottom, "hb_none", "Без штанов", null, Color.white, none: true);
            var denim = new Color(0.25f, 0.38f, 0.62f);
            foreach (var (k, n, c) in new[] { ("denim", "Джинсовые шорты", denim), ("black", "Чёрные шорты", P("black")), ("khaki", "Бежевые шорты", new Color(0.76f, 0.66f, 0.47f)),
                         ("red", "Красные шорты", P("red")), ("navy", "Тёмно-синие шорты", P("navy")), ("grey", "Серые шорты", new Color(0.55f, 0.56f, 0.58f)), ("green", "Зелёные шорты", P("green")), ("white", "Белые шорты", P("white")) })
                A(Slot.HBottom, "hb_shorts_" + k, n, "shorts", c, Rarity.Common, pattern: k == "denim" ? "denim" : null);
            A(Slot.HBottom, "hb_jeans_blue", "Синие джинсы", "pants", denim, Rarity.Common, pattern: "denim");
            A(Slot.HBottom, "hb_jeans_dark", "Тёмные джинсы", "pants", new Color(0.13f, 0.18f, 0.32f), Rarity.Common, pattern: "denim");
            A(Slot.HBottom, "hb_jeans_black", "Чёрные джинсы", "pants", new Color(0.12f, 0.12f, 0.13f), Rarity.Uncommon, pattern: "denim");
            A(Slot.HBottom, "hb_jeans_ripped", "Рваные джинсы", "pants", new Color(0.45f, 0.58f, 0.78f), Rarity.Rare, pattern: "ripped");
            foreach (var (k, n, c) in new[] { ("grey", "Серые спортивки", new Color(0.55f, 0.56f, 0.58f)), ("black", "Чёрные спортивки", P("black")), ("navy", "Синие спортивки", P("navy")), ("red", "Красные спортивки", P("red")), ("green", "Зелёные спортивки", new Color(0.15f, 0.45f, 0.25f)) })
                A(Slot.HBottom, "hb_joggers_" + k, n, "joggers", c, Rarity.Uncommon, P("white"), pattern: "stripes");
            A(Slot.HBottom, "hb_cargo_khaki", "Бежевые карго", "cargo", new Color(0.7f, 0.62f, 0.45f), Rarity.Uncommon);
            A(Slot.HBottom, "hb_cargo_olive", "Карго хаки", "cargo", new Color(0.36f, 0.4f, 0.24f), Rarity.Uncommon);
            A(Slot.HBottom, "hb_skirt_pink", "Розовая юбка", "skirt", P("pink"), Rarity.Uncommon);
            A(Slot.HBottom, "hb_skirt_black", "Чёрная юбка", "skirt", P("black"), Rarity.Uncommon);
            A(Slot.HBottom, "hb_skirt_plaid", "Юбка в клетку", "skirt", P("red"), Rarity.Rare, P("navy"), pattern: "plaid");

            A(Slot.HShoes, "hf_none", "Босиком", null, Color.white, none: true);
            foreach (var (k, n, c, c2, c3) in new[]
            {
                ("white", "Белые кроссовки", P("white"), P("white"), new Color(0.75f, 0.76f, 0.78f)), ("black", "Чёрные кроссовки", P("black"), P("white"), P("black")),
                ("red", "Красные кроссовки", P("red"), P("white"), P("white")), ("blue", "Синие кроссовки", P("blue"), P("white"), P("white")),
                ("green", "Зелёные кроссовки", P("green"), P("white"), P("white")), ("yellow", "Жёлтые кроссовки", P("yellow"), P("black"), P("black")),
                ("pink", "Розовые кроссовки", P("pink"), P("white"), P("white")), ("grey", "Серые кроссовки", new Color(0.6f, 0.61f, 0.64f), P("white"), P("orange")),
                ("whitered", "Белые с красным", P("white"), P("white"), P("red")), ("whiteblue", "Белые с синим", P("white"), P("white"), P("blue")),
                ("blackwhite", "Чёрные с белым", P("black"), P("black"), P("white")), ("orange", "Оранжевые кроссовки", P("orange"), P("white"), P("black")),
                ("neon", "Неоновые кроссовки", P("lime"), P("black"), P("pink")), ("gold", "Золотые кроссовки", P("gold"), P("white"), P("white")),
            })
                A(Slot.HShoes, "hf_sneakers_" + k, n, "sneakers", c, k == "gold" ? Rarity.Legendary : k == "neon" ? Rarity.Epic : k.Length > 6 ? Rarity.Uncommon : Rarity.Common, c2, c3);
            foreach (var (k, n, c) in new[] { ("white", "Белые кеды", P("white")), ("black", "Чёрные кеды", P("black")), ("red", "Красные кеды", P("red")), ("navy", "Синие кеды", P("navy")) })
                A(Slot.HShoes, "hf_hightop_" + k, n, "hightop", c, Rarity.Uncommon, P("white"), P("white"));
            A(Slot.HShoes, "hf_boots_brown", "Коричневые ботинки", "boots", new Color(0.42f, 0.26f, 0.14f), Rarity.Uncommon, new Color(0.18f, 0.12f, 0.08f), new Color(0.85f, 0.75f, 0.55f));
            A(Slot.HShoes, "hf_boots_black", "Чёрные ботинки", "boots", new Color(0.1f, 0.1f, 0.1f), Rarity.Uncommon, new Color(0.05f, 0.05f, 0.05f), new Color(0.3f, 0.3f, 0.3f));
            A(Slot.HShoes, "hf_boots_tan", "Рыжие ботинки", "boots", new Color(0.72f, 0.5f, 0.28f), Rarity.Rare, new Color(0.3f, 0.2f, 0.12f), new Color(0.95f, 0.9f, 0.8f));
            A(Slot.HShoes, "hf_cleats_black", "Бутсы чёрные", "cleats", P("black"), Rarity.Rare, P("gold"), P("gold"));
            A(Slot.HShoes, "hf_cleats_white", "Бутсы белые", "cleats", P("white"), Rarity.Rare, P("lime"), P("lime"));
            A(Slot.HShoes, "hf_cleats_red", "Бутсы красные", "cleats", P("red"), Rarity.Epic, P("white"), P("white"));

            // 2.16: football kits of famous players (colours, name and number — no club badges), the number can be changed.
            Color Cc(float r, float g, float b) => new Color(r, g, b);
            void Kit(string id, string name, Color main, Color trim, Color num, string pattern, int number, string back, Rarity r = Rarity.Epic) =>
                list.Add(new SkinDef { slot = Slot.HTop, id = "ht_kit_" + id, name = name, style = "kit", color = main, color2 = trim, color3 = num, pattern = pattern, number = number, model = back, rarity = r, dept = "human" });
            Kit("ronaldo_por", "Форма Роналду · Португалия", Cc(0.78f, 0.06f, 0.12f), Cc(0.05f, 0.42f, 0.22f), Cc(0.98f, 0.85f, 0.35f), null, 7, "RONALDO", Rarity.Legendary);
            Kit("ronaldo_nassr", "Форма Роналду · Аль-Наср", Cc(1f, 0.85f, 0.1f), Cc(0.1f, 0.25f, 0.65f), Cc(0.1f, 0.2f, 0.55f), null, 7, "RONALDO", Rarity.Legendary);
            Kit("ronaldo_real", "Форма Роналду · Реал", Cc(0.97f, 0.97f, 0.97f), Cc(0.85f, 0.7f, 0.3f), Cc(0.12f, 0.12f, 0.2f), null, 7, "RONALDO", Rarity.Legendary);
            Kit("messi_arg", "Форма Месси · Аргентина", Cc(0.97f, 0.97f, 0.97f), Cc(0.45f, 0.75f, 0.95f), Cc(0.1f, 0.12f, 0.25f), "vstripes", 10, "MESSI", Rarity.Legendary);
            Kit("messi_miami", "Форма Месси · Интер Майами", Cc(0.97f, 0.62f, 0.75f), Cc(0.1f, 0.1f, 0.12f), Cc(0.1f, 0.1f, 0.12f), null, 10, "MESSI", Rarity.Legendary);
            Kit("messi_barca", "Форма Месси · Барселона", Cc(0.1f, 0.25f, 0.6f), Cc(0.6f, 0.08f, 0.2f), Cc(0.98f, 0.85f, 0.35f), "vstripes", 10, "MESSI", Rarity.Legendary);
            Kit("neymar_bra", "Форма Неймара · Бразилия", Cc(1f, 0.85f, 0.1f), Cc(0.05f, 0.55f, 0.25f), Cc(0.05f, 0.35f, 0.2f), null, 10, "NEYMAR JR");
            Kit("mbappe_fra", "Форма Мбаппе · Франция", Cc(0.1f, 0.15f, 0.35f), Cc(0.9f, 0.15f, 0.2f), Cc(0.97f, 0.97f, 0.97f), null, 10, "MBAPPE");
            Kit("haaland", "Форма Холанда", Cc(0.45f, 0.72f, 0.92f), Cc(0.97f, 0.97f, 0.97f), Cc(0.97f, 0.97f, 0.97f), null, 9, "HAALAND");
            Kit("yamal_esp", "Форма Ямаля · Испания", Cc(0.78f, 0.08f, 0.12f), Cc(1f, 0.8f, 0.15f), Cc(1f, 0.82f, 0.2f), null, 19, "LAMINE YAMAL");
            Kit("salah", "Форма Салаха", Cc(0.8f, 0.05f, 0.12f), Cc(0.97f, 0.97f, 0.97f), Cc(0.97f, 0.97f, 0.97f), null, 11, "SALAH");
            Kit("bellingham", "Форма Беллингема", Cc(0.97f, 0.97f, 0.97f), Cc(0.12f, 0.12f, 0.3f), Cc(0.12f, 0.12f, 0.3f), null, 5, "BELLINGHAM");
            Kit("kaz", "Сборная Казахстана", Cc(0.3f, 0.7f, 0.95f), Cc(1f, 0.8f, 0.15f), Cc(1f, 0.82f, 0.2f), null, 10, "KAZAKHSTAN", Rarity.Epic);
            Kit("rus", "Сборная России", Cc(0.97f, 0.97f, 0.97f), Cc(0.15f, 0.3f, 0.75f), Cc(0.8f, 0.1f, 0.15f), null, 10, "RUSSIA", Rarity.Epic);
            Kit("ita", "Сборная Италии", Cc(0.1f, 0.35f, 0.75f), Cc(0.97f, 0.97f, 0.97f), Cc(0.97f, 0.97f, 0.97f), null, 10, "ITALIA", Rarity.Epic);
            Kit("chn", "Сборная Китая", Cc(0.85f, 0.08f, 0.1f), Cc(1f, 0.82f, 0.15f), Cc(1f, 0.82f, 0.2f), null, 10, "CHINA", Rarity.Epic);
            foreach (var (k, n, c) in new[] { ("pink", "Розовые футбольные шорты", Cc(0.97f, 0.62f, 0.75f)), ("sky", "Голубые футбольные шорты", Cc(0.45f, 0.72f, 0.92f)), ("blue", "Синие футбольные шорты", Cc(0.1f, 0.25f, 0.65f)),
                         ("dgreen", "Тёмно-зелёные шорты", Cc(0.05f, 0.35f, 0.2f)), ("fwhite", "Белые футбольные шорты", Cc(0.97f, 0.97f, 0.97f)), ("fblack", "Чёрные футбольные шорты", Cc(0.1f, 0.1f, 0.12f)), ("fnavy", "Тёмно-синие футбольные", Cc(0.08f, 0.12f, 0.3f)) })
                A(Slot.HBottom, "hb_fshorts_" + k, n, "fshorts", c, Rarity.Uncommon);
            foreach (var (k, n, sock) in new[] { ("red", "Бутсы и красные гетры", Cc(0.8f, 0.06f, 0.12f)), ("white", "Бутсы и белые гетры", Cc(0.97f, 0.97f, 0.97f)), ("yellow", "Бутсы и жёлтые гетры", Cc(1f, 0.85f, 0.1f)),
                         ("pink", "Бутсы и розовые гетры", Cc(0.97f, 0.62f, 0.75f)), ("blue", "Бутсы и синие гетры", Cc(0.1f, 0.25f, 0.65f)), ("sky", "Бутсы и голубые гетры", Cc(0.45f, 0.72f, 0.92f)), ("black", "Бутсы и чёрные гетры", Cc(0.1f, 0.1f, 0.12f)) })
                A(Slot.HShoes, "hf_kit_" + k, n, "kitboots", Cc(0.1f, 0.1f, 0.12f), Rarity.Rare, Cc(0.95f, 0.75f, 0.2f), sock);

            // 2.16: national clothes — Russian, Chinese, Italian, Kazakh.
            var gold = Cc(0.95f, 0.75f, 0.25f);
            A(Slot.HTop, "ht_kosovorotka", "Косоворотка", "kosovorotka", Cc(0.97f, 0.96f, 0.92f), Rarity.Epic, Cc(0.8f, 0.08f, 0.12f));
            A(Slot.HTop, "ht_kosovorotka_red", "Красная косоворотка", "kosovorotka", Cc(0.78f, 0.1f, 0.12f), Rarity.Epic, gold);
            A(Slot.HTop, "ht_sarafan", "Сарафан", "dress", Cc(0.78f, 0.08f, 0.12f), Rarity.Epic, gold, Cc(0.98f, 0.97f, 0.94f), pattern: "sarafan");
            A(Slot.HTop, "ht_sarafan_blue", "Синий сарафан", "dress", Cc(0.12f, 0.25f, 0.65f), Rarity.Epic, gold, Cc(0.98f, 0.97f, 0.94f), pattern: "sarafan");
            A(Slot.HTop, "ht_tang_red", "Китайский костюм", "tang", Cc(0.75f, 0.06f, 0.08f), Rarity.Epic, gold);
            A(Slot.HTop, "ht_tang_navy", "Синий китайский костюм", "tang", Cc(0.08f, 0.12f, 0.3f), Rarity.Epic, gold);
            A(Slot.HTop, "ht_qipao_red", "Ципао", "dress", Cc(0.8f, 0.05f, 0.1f), Rarity.Epic, gold, null, pattern: "qipao");
            A(Slot.HTop, "ht_qipao_jade", "Нефритовое ципао", "dress", Cc(0.1f, 0.5f, 0.4f), Rarity.Epic, gold, null, pattern: "qipao");
            A(Slot.HTop, "ht_gondolier", "Гондольер", "gondolier", Cc(0.97f, 0.97f, 0.97f), Rarity.Rare, Cc(0.1f, 0.12f, 0.3f), Cc(0.8f, 0.08f, 0.12f), pattern: "stripes");
            A(Slot.HTop, "ht_suit_navy", "Итальянский костюм", "blazer", Cc(0.1f, 0.14f, 0.3f), Rarity.Epic, Cc(0.97f, 0.97f, 0.97f), Cc(0.7f, 0.1f, 0.15f));
            A(Slot.HTop, "ht_suit_grey", "Серый костюм", "blazer", Cc(0.42f, 0.43f, 0.46f), Rarity.Epic, Cc(0.97f, 0.97f, 0.97f), Cc(0.15f, 0.3f, 0.7f));
            A(Slot.HTop, "ht_chapan_blue", "Чапан", "chapan", Cc(0.1f, 0.18f, 0.5f), Rarity.Legendary, gold);
            A(Slot.HTop, "ht_chapan_red", "Бордовый чапан", "chapan", Cc(0.45f, 0.06f, 0.12f), Rarity.Legendary, gold);
            A(Slot.HTop, "ht_koylek", "Казахское платье", "dress", Cc(0.97f, 0.96f, 0.94f), Rarity.Legendary, gold, Cc(0.55f, 0.06f, 0.15f), pattern: "koylek");
            A(Slot.HBottom, "hb_pants_black", "Чёрные брюки", "pants", Cc(0.1f, 0.1f, 0.12f), Rarity.Common);
            A(Slot.HBottom, "hb_pants_navy", "Тёмно-синие брюки", "pants", Cc(0.1f, 0.14f, 0.3f), Rarity.Common);
            A(Slot.HBottom, "hb_pants_grey", "Серые брюки", "pants", Cc(0.42f, 0.43f, 0.46f), Rarity.Common);
            A(Slot.HShoes, "hf_sapogi_black", "Чёрные сапоги", "sapogi", Cc(0.08f, 0.08f, 0.08f), Rarity.Rare, Cc(0.05f, 0.05f, 0.05f), Cc(0.3f, 0.3f, 0.3f));
            A(Slot.HShoes, "hf_sapogi_red", "Красные сапоги", "sapogi", Cc(0.6f, 0.06f, 0.08f), Rarity.Epic, Cc(0.2f, 0.05f, 0.05f), gold);
            A(Slot.HShoes, "hf_shoes_black", "Туфли", "loafers", Cc(0.08f, 0.08f, 0.08f), Rarity.Uncommon, Cc(0.05f, 0.05f, 0.05f), Cc(0.2f, 0.2f, 0.2f));
            A(Slot.HShoes, "hf_shoes_brown", "Коричневые туфли", "loafers", Cc(0.4f, 0.22f, 0.1f), Rarity.Uncommon, Cc(0.15f, 0.08f, 0.04f), Cc(0.2f, 0.2f, 0.2f));
            A(Slot.HAcc, "ha_kokoshnik", "Кокошник", "kokoshnik", Cc(0.78f, 0.08f, 0.12f), Rarity.Epic, gold);
            A(Slot.HAcc, "ha_kalpak", "Калпак", "kalpak", Cc(0.95f, 0.94f, 0.9f), Rarity.Epic, Cc(0.08f, 0.08f, 0.08f));
            A(Slot.HAcc, "ha_takiya", "Тюбетейка", "takiya", Cc(0.08f, 0.08f, 0.1f), Rarity.Rare, Cc(0.95f, 0.94f, 0.9f));
            A(Slot.HAcc, "ha_saukele", "Саукеле", "saukele", Cc(0.8f, 0.08f, 0.12f), Rarity.Legendary, gold);
            A(Slot.HAcc, "ha_conehat", "Китайская шляпа", "conehat", Cc(0.85f, 0.72f, 0.45f), Rarity.Rare, Cc(0.55f, 0.4f, 0.2f));
            A(Slot.HAcc, "ha_boater", "Шляпа гондольера", "boater", Cc(0.92f, 0.8f, 0.5f), Rarity.Rare, Cc(0.1f, 0.15f, 0.4f));

            A(Slot.HAcc, "ha_none", "Ничего", null, Color.white, none: true);
            foreach (var k in new[] { "red", "black", "blue", "white" }) A(Slot.HAcc, "ha_cap_" + k, Adj(k, "ая кепка"), "cap", P(k), Rarity.Common, Color.white);
            foreach (var k in new[] { "red", "grey", "yellow" }) A(Slot.HAcc, "ha_beanie_" + k, k == "grey" ? "Серая шапка" : Adj(k, "ая шапка"), "beanie", k == "grey" ? new Color(0.55f, 0.56f, 0.58f) : P(k), Rarity.Common, Color.white);
            A(Slot.HAcc, "ha_glasses", "Очки", "glasses", new Color(0.1f, 0.1f, 0.1f), Rarity.Uncommon);
            A(Slot.HAcc, "ha_glasses_round", "Круглые очки", "glasses_round", new Color(0.75f, 0.6f, 0.25f), Rarity.Uncommon);
            A(Slot.HAcc, "ha_sunglasses", "Тёмные очки", "sunglasses", new Color(0.05f, 0.05f, 0.06f), Rarity.Rare);
            A(Slot.HAcc, "ha_shades_gold", "Очки-авиаторы", "aviators", new Color(0.9f, 0.7f, 0.25f), Rarity.Epic);
            A(Slot.HAcc, "ha_headphones_black", "Чёрные наушники", "headphones", P("black"), Rarity.Rare, P("silver"));
            A(Slot.HAcc, "ha_headphones_pink", "Розовые наушники", "headphones", P("pink"), Rarity.Rare, P("white"));
        }

        /// <summary>
        /// 2.16: the cartoon farmer's hats, neck things and back things fit the human too (they are sized
        /// from the head box) — worn from the human's own slots.
        /// </summary>
        internal static void DressOnHuman(Built built, HumanRig h, SkinSet look) { }

        /// <summary>What a department of the wardrobe lists (the human borrows the farmer's hats, neck and back things).</summary>
        public static Slot ItemSlot(Slot s) => s switch { Slot.HHat => Slot.Head, Slot.HNeck => Slot.Neck, Slot.HBack => Slot.Back, _ => s };
        /// <summary>Picture of a thing as the human wears it.</summary>
        public static string HumanIconId(SkinDef d) => d.slot == Slot.Head || d.slot == Slot.Neck || d.slot == Slot.Back ? "skinh_" + d.id : IconId(d);

        static void AddNumbers(List<SkinDef> list)
        {
            list.Add(new SkinDef { slot = Slot.HNumber, id = "hn_none", name = "Номер формы", none = true, dept = "human" });
            for (int n = 0; n <= 99; n++) list.Add(new SkinDef { slot = Slot.HNumber, id = "hn_" + n, name = "Номер " + n, number = n, dept = "human" });
        }

        /// <summary>Wardrobe departments of the human.</summary>
        public static readonly (Slot slot, string dept, string name, string icon)[] HumanDepartments =
        {
            (Slot.HGender, null, "ПОЛ", "skin_hg_f"), (Slot.HSkin, null, "КОЖА", "skin_hs_tan"),
            (Slot.HHair, null, "ВОЛОСЫ", "skin_hh_long_brown"), (Slot.HEyes, null, "ГЛАЗА", "skin_he_blue"), (Slot.HMouth, null, "РОТ", "skin_hm_grin"),
            (Slot.HTop, null, "ФУТБОЛКИ", "skin_ht_print_star"), (Slot.HBottom, null, "ШТАНЫ И ШОРТЫ", "skin_hb_jeans_blue"), (Slot.HShoes, null, "КРОССОВКИ", "skin_hf_sneakers_whitered"),
            (Slot.HAcc, null, "КЕПКИ И ОЧКИ", "skin_ha_cap_red"), (Slot.HHat, null, "ШЛЯПЫ И КОРОНЫ", "skinh_crown"),
            (Slot.HNeck, null, "ШЕЯ", "skinh_bow_red"), (Slot.HBack, null, "СПИНА", "skinh_wings_white"), (Slot.HNumber, null, "НОМЕР", "skin_ht_kit_messi_arg"),
            (Slot.Hand, "phones", "ТЕЛЕФОНЫ", "skin_hand_iphone17pro_orange"),
        };

        public static bool IsHumanSlot(Slot s) => s >= Slot.HGender;
    }

    // =========================================================================
    // Dressing the human
    // =========================================================================

    public static partial class Human
    {
        /// <summary>Build the human for a look: body, face, hair, clothes, shoes, accessories, phone.</summary>
        public static Wardrobe.Built Build(SkinSet look, float height, bool posed)
        {
            var holder = new GameObject("Human");
            var result = new Wardrobe.Built { root = holder };
            bool female = look.hGender == "hg_f";
            var skinDef = Wardrobe.Find(look.hSkin) ?? Wardrobe.Find("hs_light");
            bool mannequin = skinDef.style != "skin";
            Color skin = skinDef.color;
            int face = int.TryParse(Wardrobe.Find(look.hFace)?.style, out var fc) ? fc : 0;
            var h = BuildBody(holder.transform, female, skin, mannequin || skinDef.style == "gold", height * 1.08f, face);
            if (skinDef.style == "gold") foreach (var r in holder.GetComponentsInChildren<Renderer>()) r.sharedMaterial = Mats.Shared("human_gold", () => Mats.Lit(skin, null, 0.75f, 0.85f));
            result.human = h;
            var ctx = new Dress { h = h, built = result, skin = skin, mannequin = mannequin };
            var hair = Wardrobe.Find(look.hHair);
            var acc = Wardrobe.Find(look.hAcc);
            DressEyes(ctx, Wardrobe.Find(look.hEyes), hair);
            DressMouth(ctx, Wardrobe.Find(look.hMouth), hair);
            bool hat = acc != null && !acc.none && (acc.style == "cap" || acc.style == "beanie");
            DressHair(ctx, hair, hat);
            var top = Wardrobe.Find(look.hTop);
            DressBottom(ctx, Wardrobe.Find(look.hBottom), top);
            int num = look.hNumber != null && look.hNumber.StartsWith("hn_") && int.TryParse(look.hNumber.Substring(3), out var nn) ? nn : -1;
            DressTop(ctx, top, num);
            DressTopExtras(ctx, top, num);
            DressShoes(ctx, Wardrobe.Find(look.hShoes));
            DressAcc(ctx, acc, hair);
            Wardrobe.DressOnHuman(result, h, look);
            var hand = Wardrobe.Find(look.hand);
            if (hand != null && !hand.none && hand.style == "phone") HoldPhone(ctx, hand);
            result.headBounds = HeadBounds(h, holder.transform);
            var an = HumanAnimator.Attach(h);
            an.HoldingPhone = ctx.phone != null;
            result.humanAnim = an;
            if (posed) an.enabled = false;
            return result;
        }

        /// <summary>
        /// 2.17: the player's own arm seen in first person: a fist (curled round the origin, the handle
        /// along local Z), the forearm going up local +Y, in the player's skin and sleeve.
        /// </summary>
        public static GameObject BuildViewArm(SkinSet look, float side)
        {
            var root = new GameObject(side > 0 ? "ViewArmR" : "ViewArmL");
            var skinDef = Wardrobe.Find(look.hSkin) ?? Wardrobe.Find("hs_light");
            var s = MakeShape(look.hGender == "hg_f");
            var mat = SkinMat(skinDef.color, skinDef.style != "skin");
            var hand = new GameObject("hand").transform;
            hand.SetParent(root.transform, false);
            hand.localPosition = new Vector3(side * 0.016f, 0.085f, 0f);
            var fingers = BuildHand(hand, s, mat, side);
            CurlHand(fingers, 1f, side);
            var fa = new GameObject("forearm").transform;
            fa.SetParent(hand, false);
            fa.localPosition = new Vector3(0f, 0.255f, 0f);
            Part("body_farm", Loft("farm2_" + (s.female ? "f" : "m"), s.fArm, s.fArm.Y0, s.fArm.Y1, 0f, true, true, 20, 32, bump: FArmBump), mat, fa);
            var top = Wardrobe.Find(look.hTop);
            if (top != null && !top.none && (top.style == "hoodie" || top.style == "flannel" || top.style == "shirt" || top.style == "kosovorotka" || top.style == "tang" || top.style == "chapan" || top.style == "blazer" || (top.style == "dress" && top.pattern != "qipao")))
            {
                var sleeveCol = top.style == "dress" && top.pattern == "sarafan" ? top.color3 : top.color;
                Part("view_sleeve", Loft("view_sleeve_" + (s.female ? "f" : "m"), s.fArm, -0.2f, s.fArm.Y1 + 0.02f, 0.012f, false, false, 10, 28), Cloth("plain", sleeveCol, sleeveCol), fa);
            }
            foreach (var r in root.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return root;
        }

        /// <summary>The head's box in the holder's space (for icon framing).</summary>
        static Bounds HeadBounds(HumanRig h, Transform holder)
        {
            var s = h.shape;
            var b = new Bounds(holder.InverseTransformPoint(h.head.TransformPoint(new Vector3(0f, s.headTop * 0.5f, 0f))), Vector3.zero);
            foreach (var p in new[] { new Vector3(0.09f, -0.02f, 0.11f), new Vector3(-0.09f, s.headTop + 0.01f, -0.11f) })
                b.Encapsulate(holder.InverseTransformPoint(h.head.TransformPoint(p)));
            return b;
        }

        class Dress
        {
            public HumanRig h;
            public Wardrobe.Built built;
            public Color skin;
            public bool mannequin;
            public GameObject phone;
        }

        static GameObject Acc(Dress x, string name, Mesh mesh, Material mat, Transform bone, Vector3 pos = default, Quaternion? rot = null, Vector3? scale = null)
        {
            var go = Part(name, mesh, mat, bone, pos, rot, scale);
            go.name = "acc_" + name;
            x.built.accessories.Add(go.GetComponent<Renderer>());
            return go;
        }

        static GameObject Prim(Dress x, PrimitiveType t, Transform bone, Vector3 pos, Vector3 scale, Material m, Quaternion? rot = null)
        {
            var go = MeshUtil.Primitive(t, bone, pos, scale, m, rot);
            go.name = "acc_part";
            x.built.accessories.Add(go.GetComponent<Renderer>());
            return go;
        }

        static Material Plain(Color c, float smooth = 0.3f, float metal = 0f) =>
            Mats.Shared($"hum_{ColorUtility.ToHtmlStringRGB(c)}_{smooth:0.00}_{metal:0.0}", () => Mats.Lit(c, null, smooth, metal));

        /// <summary>A bent tube through the points (mouths, glasses rims, headphone bands, laces).</summary>
        public static Mesh Tube(string key, IList<Vector3> pts, float r, int seg = 8, bool closed = false)
        {
            if (key != null && meshes.TryGetValue(key, out var cached)) return cached;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                var prev = pts[closed ? (i - 1 + n) % n : Mathf.Max(0, i - 1)];
                var next = pts[closed ? (i + 1) % n : Mathf.Min(n - 1, i + 1)];
                var t = (next - prev).normalized;
                var side = Vector3.Cross(t, Mathf.Abs(t.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
                var up = Vector3.Cross(side, t);
                for (int s = 0; s < seg; s++)
                {
                    float a = s / (float)seg * Mathf.PI * 2f;
                    verts.Add(pts[i] + (side * Mathf.Cos(a) + up * Mathf.Sin(a)) * r);
                }
            }
            int rings = closed ? n : n - 1;
            for (int i = 0; i < rings; i++)
                for (int s = 0; s < seg; s++)
                {
                    int i0 = i * seg + s, i1 = i * seg + (s + 1) % seg, j0 = ((i + 1) % n) * seg + s, j1 = ((i + 1) % n) * seg + (s + 1) % seg;
                    tris.Add(i0); tris.Add(j0); tris.Add(i1);
                    tris.Add(i1); tris.Add(j0); tris.Add(j1);
                }
            if (!closed)
            {
                foreach (var (ring, dir) in new[] { (0, -1), (n - 1, 1) })
                {
                    int c = verts.Count;
                    verts.Add(pts[ring]);
                    for (int s = 0; s < seg; s++)
                    {
                        int a0 = ring * seg + s, a1 = ring * seg + (s + 1) % seg;
                        if (dir < 0) { tris.Add(c); tris.Add(a1); tris.Add(a0); } else { tris.Add(c); tris.Add(a0); tris.Add(a1); }
                    }
                }
            }
            var m = new Mesh { name = key ?? "tube" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            if (key != null) meshes[key] = m;
            return m;
        }

        // ---------------- face ----------------

        static readonly Dictionary<string, Texture2D> irisTex = new Dictionary<string, Texture2D>();
        /// <summary>An iris: fine radial fibres, a darker rim, a lighter ring round a black pupil.</summary>
        static Texture2D IrisTex(Color c)
        {
            string key = ColorUtility.ToHtmlStringRGB(c);
            if (irisTex.TryGetValue(key, out var t)) return t;
            const int S = 64;
            t = new Texture2D(S, S, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "iris_" + key };
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int xx = 0; xx < S; xx++)
                {
                    float u = (xx + 0.5f) / S * 2f - 1f, v = (y + 0.5f) / S * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v), ang = Mathf.Atan2(v, u);
                    Color col;
                    if (r < 0.34f) col = new Color(0.02f, 0.02f, 0.025f);
                    else if (r < 1f)
                    {
                        float fib = 0.82f + 0.25f * Mathf.PerlinNoise(ang * 6f + 10f, r * 3f) + 0.12f * Mathf.Sin(ang * 37f);
                        col = c * fib;
                        col = Color.Lerp(col, Color.Lerp(c, Color.white, 0.4f), Mathf.Clamp01(1f - Mathf.Abs(r - 0.45f) / 0.08f) * 0.5f);
                        col = Color.Lerp(col, c * 0.35f, Mathf.Clamp01((r - 0.82f) / 0.18f));
                    }
                    else col = c * 0.3f;
                    col.a = 1f;
                    px[y * S + xx] = col;
                }
            t.SetPixels(px);
            t.Apply(true);
            irisTex[key] = t;
            return t;
        }

        static Mesh irisDisc;
        /// <summary>A slightly domed disc facing +Z with radial texture mapping (the iris).</summary>
        static Mesh IrisDisc()
        {
            if (irisDisc != null) return irisDisc;
            const int N = 28;
            var v = new List<Vector3> { new Vector3(0f, 0f, 0.25f) };
            var uv = new List<Vector2> { new Vector2(0.5f, 0.5f) };
            var tris = new List<int>();
            for (int i = 0; i <= N; i++)
            {
                float a = i / (float)N * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
                uv.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(a), 0.5f + 0.5f * Mathf.Sin(a)));
            }
            for (int i = 1; i <= N; i++) { tris.Add(0); tris.Add(i + 1); tris.Add(i); }
            irisDisc = new Mesh { name = "iris" };
            irisDisc.SetVertices(v); irisDisc.SetUVs(0, uv); irisDisc.SetTriangles(tris, 0);
            irisDisc.RecalculateNormals(); irisDisc.RecalculateBounds();
            return irisDisc;
        }

        static readonly Dictionary<string, Mesh> irisDomes = new Dictionary<string, Mesh>();
        /// <summary>The iris as a cap that follows the curve of the eyeball (radius er), ir across, a hair in front of it.</summary>
        static Mesh IrisDome(float er, float ir)
        {
            string key = $"{er:0.00000}_{ir:0.00000}";
            if (irisDomes.TryGetValue(key, out var m)) return m;
            const int R = 6, N = 28;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
            v.Add(new Vector3(0f, 0f, er + 0.0003f)); uv.Add(new Vector2(0.5f, 0.5f));
            for (int r = 1; r <= R; r++)
                for (int i = 0; i < N; i++)
                {
                    float rr = r / (float)R, a = i / (float)N * Mathf.PI * 2f;
                    float px = Mathf.Cos(a) * rr * ir, py = Mathf.Sin(a) * rr * ir;
                    v.Add(new Vector3(px, py, Mathf.Sqrt(Mathf.Max(0f, er * er - px * px - py * py)) + 0.0003f));
                    uv.Add(new Vector2(0.5f + 0.5f * rr * Mathf.Cos(a), 0.5f + 0.5f * rr * Mathf.Sin(a)));
                }
            for (int i = 0; i < N; i++) { tris.Add(0); tris.Add(1 + (i + 1) % N); tris.Add(1 + i); }
            for (int r = 1; r < R; r++)
                for (int i = 0; i < N; i++)
                {
                    int a0 = 1 + (r - 1) * N + i, a1 = 1 + (r - 1) * N + (i + 1) % N, b0 = a0 + N, b1 = a1 + N;
                    tris.Add(a0); tris.Add(a1); tris.Add(b0);
                    tris.Add(a1); tris.Add(b1); tris.Add(b0);
                }
            m = new Mesh { name = "iris_dome" };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tris, 0);
            m.RecalculateNormals(); m.RecalculateBounds();
            irisDomes[key] = m;
            return m;
        }

        static void DressEyes(Dress x, Wardrobe.SkinDef d, Wardrobe.SkinDef hair)
        {
            if (d == null || d.none) return;
            var s = x.h.shape; var head = x.h.head;
            float hs = s.headSize, ey = s.eyeY, ex = 0.031f * hs;
            var white = Plain(new Color(0.96f, 0.95f, 0.93f), 0.85f);
            var iris = Mats.Shared("hum_iris3_" + ColorUtility.ToHtmlStringRGB(d.color), () => { var m = Mats.Lit(Color.white, IrisTex(d.color), 0.35f); m.SetFloat("_Cull", 0f); return m; });
            var cornea = Mats.Shared("hum_cornea2", () => { var m = Mats.Transparent(new Color(1f, 1f, 1f, 0.035f)); m.SetFloat("_Smoothness", 0.98f); return m; });
            var lidMat = SkinMat(x.skin, x.mannequin);
            var lash = Plain(new Color(0.06f, 0.045f, 0.04f), 0.3f);
            Color browC = hair != null && !hair.none ? Color.Lerp(hair.color, Color.black, 0.3f) : new Color(0.22f, 0.14f, 0.09f);
            if (x.mannequin && (hair == null || hair.none)) browC = new Color(0.55f, 0.55f, 0.57f);
            var brow = HairMat(browC);
            float er = 0.0125f * hs;                       // eyeball radius
            float ir = (d.style == "big" ? 0.0074f : d.style == "wow" ? 0.0052f : 0.0064f) * hs;
            foreach (var side in new[] { -1f, 1f })
            {
                var c = FacePoint(s, side * ex, ey);
                bool closed = d.style == "happy" || (d.style == "wink" && side < 0f);
                var ball = c + new Vector3(0f, 0f, -er - 0.001f);     // set into the eye socket
                Prim(x, PrimitiveType.Sphere, head, ball, Vector3.one * er * 2f, white);
                // Looking straight ahead: the iris on the front of the eyeball, the clear cornea over it.
                float irz = er * Mathf.Sqrt(Mathf.Max(0f, 1f - (ir / er) * (ir / er)));   // where the iris rim meets the eyeball
                Acc(x, "eye_iris", IrisDome(er, ir), iris, head, ball);
                Prim(x, PrimitiveType.Sphere, head, ball + new Vector3(0f, 0f, irz), new Vector3(ir * 2.25f, ir * 2.25f, (er - irz) * 2f + 0.003f * hs), cornea);
                Prim(x, PrimitiveType.Sphere, head, ball + new Vector3(side * -0.0018f * hs, 0.0022f * hs, er * 1.02f), Vector3.one * 0.0016f * hs, Plain(Color.white, 1f));
                // Eyelids: thin skin over the eyeball — the upper one (it blinks) and the lower one; how open
                // the eye is depends on the style. Each lid is a sphere a little bigger than the eyeball, shifted.
                float off = d.style == "sleepy" ? 0.62f : d.style == "wow" ? 1.15f : d.style == "angry" ? 0.82f : 0.98f;
                if (closed) off = 0.22f;
                var lid = Prim(x, PrimitiveType.Sphere, head, ball + new Vector3(0f, off * er, 0f), new Vector3(2.02f, 2.1f, 2.1f) * er, lidMat);
                lid.name = "acc_eyelid";
                x.h.lids.Add(lid.transform);
                Prim(x, PrimitiveType.Sphere, head, ball + new Vector3(0f, -1.08f * er, 0f), new Vector3(1.92f, 2.05f, 2.05f) * er, lidMat);
                float upperY = (off - 0.34f) * er / hs;      // the edge of the upper lid at the front (in head units)
                // Lashes along the edge of the upper lid (longer with "lashes"), a crease above it.
                var lashPts = new List<Vector3>(); var lashR = new List<float>();
                for (int i = 0; i <= 8; i++)
                {
                    float u = i / 8f * 2f - 1f;
                    float lx = c.x + u * 0.0125f * hs;
                    float ly = ey + upperY * hs + 0.0022f * hs * (1f - u * u) * (closed ? -0.6f : 1f);
                    lashPts.Add(new Vector3(lx, ly, ball.z + er * 1.07f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - u * u * 0.55f))));
                    lashR.Add((d.style == "lashes" ? 0.0011f : 0.0007f) * hs * (0.4f + 0.6f * (1f - u * u * 0.6f)));
                }
                Acc(x, "eye_lash", TubeVar(lashPts, lashR, 6), lash, head);
                if (d.style == "lashes")
                    for (int k = -2; k <= 2; k++)
                    {
                        var lp = lashPts[4 + k * 2 > 8 ? 8 : 4 + k * 2 < 0 ? 0 : 4 + k * 2];
                        Prim(x, PrimitiveType.Capsule, head, lp + new Vector3(side * 0.0012f * k, 0.0022f * hs, 0.001f), new Vector3(0.0007f, 0.0028f, 0.0007f) * hs, lash, Quaternion.Euler(-25f, 0f, side * k * 12f));
                    }
                // The eyebrow: thick at the inner end, thin at the outer, along the brow ridge.
                float browY = ey + (d.style == "wow" ? 0.03f : 0.023f) * hs;
                float tiltIn = d.style == "angry" ? -0.006f : d.style == "sad" ? 0.004f : 0f;
                var bp = new List<Vector3>(); var br = new List<float>();
                for (int i = 0; i <= 8; i++)
                {
                    float u = i / 8f;                                   // 0 inner … 1 outer
                    float bx = side * (0.012f + u * 0.028f) * hs;
                    float by = browY + (Mathf.Sin(u * Mathf.PI) * 0.004f + tiltIn * (1f - u) - 0.002f * u) * hs;
                    bp.Add(new Vector3(bx, by, s.head.FrontZ(by, bx) + 0.0052f));
                    br.Add((0.0026f - u * 0.0016f) * hs);
                }
                Acc(x, "eye_brow", TubeVar(bp, br, 6, new Vector3(1f, 1f, 0.55f)), brow, head);
            }
        }

        /// <summary>A stitched seam: short raised dashes along the points, all in one mesh.</summary>
        public static Mesh Stitches(IList<Vector3> pts, float r, float dash = 0.006f, float gap = 0.004f)
        {
            var verts = new List<Vector3>(); var tris = new List<int>();
            float walked = 0f; bool on = true; float next = dash;
            var cur = new List<Vector3>();
            void Flush()
            {
                if (cur.Count < 2) { cur.Clear(); return; }
                var m = Tube(null, cur, r, 4);
                int b0 = verts.Count;
                verts.AddRange(m.vertices);
                foreach (var t in m.triangles) tris.Add(b0 + t);
                UnityEngine.Object.Destroy(m);
                cur.Clear();
            }
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                var a = pts[i]; var b = pts[i + 1];
                float len = Vector3.Distance(a, b), t0 = 0f;
                while (t0 < len)
                {
                    float step = Mathf.Min(next - walked, len - t0);
                    var p0 = Vector3.Lerp(a, b, t0 / len); var p1 = Vector3.Lerp(a, b, (t0 + step) / len);
                    if (on) { if (cur.Count == 0) cur.Add(p0); cur.Add(p1); }
                    t0 += step; walked += step;
                    if (walked >= next - 1e-5f) { if (on) Flush(); on = !on; next = walked + (on ? dash : gap); }
                }
            }
            Flush();
            var mesh = new Mesh { name = "stitches" };
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>A tube whose thickness changes along it (lips).</summary>
        public static Mesh TubeVar(IList<Vector3> pts, IList<float> radii, int seg = 10, Vector3? squash = null)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            int n = pts.Count;
            var sq = squash ?? Vector3.one;
            for (int i = 0; i < n; i++)
            {
                var t = (pts[Mathf.Min(n - 1, i + 1)] - pts[Mathf.Max(0, i - 1)]).normalized;
                var side = Vector3.Cross(t, Vector3.forward).normalized;
                if (side.sqrMagnitude < 0.5f) side = Vector3.up;
                var fwd = Vector3.Cross(side, t).normalized;
                for (int k = 0; k < seg; k++)
                {
                    float a = k / (float)seg * Mathf.PI * 2f;
                    var off = side * Mathf.Cos(a) * radii[i] * sq.y + fwd * Mathf.Sin(a) * radii[i] * sq.z;
                    verts.Add(pts[i] + off);
                }
            }
            for (int i = 0; i < n - 1; i++)
                for (int k = 0; k < seg; k++)
                {
                    int i0 = i * seg + k, i1 = i * seg + (k + 1) % seg, j0 = i0 + seg, j1 = i1 + seg;
                    tris.Add(i0); tris.Add(j0); tris.Add(i1);
                    tris.Add(i1); tris.Add(j0); tris.Add(j1);
                }
            foreach (var (ring, dir) in new[] { (0, -1), (n - 1, 1) })
            {
                int c = verts.Count;
                verts.Add(pts[ring]);
                for (int k = 0; k < seg; k++)
                {
                    int a0 = ring * seg + k, a1 = ring * seg + (k + 1) % seg;
                    if (dir < 0) { tris.Add(c); tris.Add(a1); tris.Add(a0); } else { tris.Add(c); tris.Add(a0); tris.Add(a1); }
                }
            }
            var m = new Mesh { name = "lip" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static void DressMouth(Dress x, Wardrobe.SkinDef d, Wardrobe.SkinDef hair)
        {
            if (d == null || d.none) return;
            var s = x.h.shape; var head = x.h.head;
            float hs = s.headSize, my = s.eyeY - 0.066f * hs;
            // Lips a little redder and darker than the skin (red for "lips"; grey on the mannequin).
            Color lipC = d.style == "lips" ? d.color : x.mannequin ? new Color(0.62f, 0.6f, 0.6f) : Color.Lerp(x.skin, new Color(0.72f, 0.28f, 0.3f), 0.5f);
            var lip = Plain(lipC, d.style == "lips" ? 0.65f : 0.4f);
            var line = Plain(new Color(0.22f, 0.07f, 0.08f), 0.3f);
            var dark = Plain(new Color(0.2f, 0.04f, 0.06f), 0.3f);
            var teeth = Plain(new Color(0.98f, 0.97f, 0.93f), 0.6f);
            var tongue = Plain(new Color(0.92f, 0.42f, 0.48f), 0.5f);
            // The curve of the mouth: corners up (smile), down (sad), one side up (smirk).
            float curve = d.style switch { "smile" => 0.0065f, "lips" => 0.004f, "grin" => 0.008f, "tongue" => 0.005f, "sad" => -0.005f, "smirk" => 0.002f, _ => 0.0008f } * hs;
            float tilt = d.style == "smirk" ? 0.004f * hs : 0f;
            float w = (d.style == "grin" ? 0.024f : d.style == "open" ? 0.014f : 0.0195f) * hs;
            float open = d.style switch { "grin" => 0.011f, "open" => 0.017f, _ => 0f } * hs;
            Vector3 P(float u, float dy, float lift) { float px = u * w; float py = my + dy + curve * u * u + tilt * u; return new Vector3(px, py, s.head.FrontZ(py, px) + lift); }
            const int N = 13;
            var up = new List<Vector3>(); var upR = new List<float>();
            var lo = new List<Vector3>(); var loR = new List<float>();
            var mid = new List<Vector3>(); var midR = new List<float>();
            for (int i = 0; i < N; i++)
            {
                float u = i / (float)(N - 1) * 2f - 1f;
                float body = Mathf.Pow(Mathf.Max(0f, 1f - u * u), 0.55f);
                // Cupid's bow: a small dip in the middle of the upper lip.
                float bow = -0.0012f * hs * Mathf.Exp(-(u / 0.18f) * (u / 0.18f)) + 0.0006f * hs * Mathf.Exp(-((Mathf.Abs(u) - 0.3f) / 0.15f) * ((Mathf.Abs(u) - 0.3f) / 0.15f));
                up.Add(P(u, open * 0.5f + 0.0024f * hs * body + bow, 0.0008f)); upR.Add(0.0005f + 0.0023f * hs * body);
                lo.Add(P(u, -open * 0.5f - 0.0032f * hs * body, 0.0006f)); loR.Add(0.0005f + 0.0031f * hs * body);
                mid.Add(P(u * 1.04f, 0f, 0.0012f)); midR.Add(0.0006f * hs + 0.0003f * body);
            }
            var flat = new Vector3(1f, 0.8f, 0.55f);
            Acc(x, "lip_upper", TubeVar(up, upR, 10, flat), lip, head);
            Acc(x, "lip_lower", TubeVar(lo, loR, 10, flat), lip, head);
            if (open <= 0f) Acc(x, "lip_line", TubeVar(mid, midR, 6), line, head);
            var mz = s.head.FrontZ(my, 0f);
            switch (d.style)
            {
                case "open":
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, my, mz - 0.004f), new Vector3(w * 2.1f, open * 1.25f, 0.012f * hs), dark);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, my - open * 0.3f, mz - 0.002f), new Vector3(w * 1.2f, open * 0.45f, 0.008f * hs), tongue);
                    Prim(x, PrimitiveType.Cube, head, new Vector3(0f, my + open * 0.38f, mz - 0.0015f), new Vector3(w * 1.2f, open * 0.22f, 0.004f), teeth);
                    break;
                case "grin":
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, my + curve * 0.3f, mz - 0.004f), new Vector3(w * 2.05f, open * 1.2f, 0.012f * hs), dark);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, my + open * 0.22f + curve * 0.3f, mz - 0.0012f), new Vector3(w * 1.75f, open * 0.5f, 0.007f * hs), teeth);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, my - open * 0.3f, mz - 0.0025f), new Vector3(w * 1.0f, open * 0.35f, 0.007f * hs), tongue);
                    break;
                case "tongue":
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0.002f, my - 0.0075f * hs, mz + 0.004f), new Vector3(0.012f, 0.014f, 0.006f) * hs, tongue, Quaternion.Euler(-35f, 0f, 0f));
                    break;
                case "beard":
                {
                    var hc = hair != null && !hair.none ? hair.color : d.color;
                    var hm = HairMat(hc);
                    var beard = Shell("beard_" + (s.female ? "f" : "m"), s.head, -0.03f * hs, my + 0.03f * hs, 0.009f * hs,
                        a => a > 20f && a < 160f ? my - 0.004f * hs + Mathf.Pow(Mathf.Abs(Mathf.Cos(a * Mathf.Deg2Rad)), 1.5f) * 0.05f * hs : 99f, true);
                    Acc(x, "beard", beard, hm, head);
                    var mus = new List<Vector3>(); var musR = new List<float>();
                    for (int i = 0; i < N; i++) { float u = i / (float)(N - 1) * 2f - 1f; mus.Add(P(u * 1.1f, 0.0085f * hs - 0.004f * hs * u * u, 0.003f)); musR.Add(0.001f + 0.004f * hs * Mathf.Pow(1f - u * u, 0.5f)); }
                    Acc(x, "moustache", TubeVar(mus, musR, 8), hm, head);
                    break;
                }
            }
        }

        // ---------------- hair ----------------

        static Material HairMat(Color c) => Mats.Shared("hum_hair_" + ColorUtility.ToHtmlStringRGB(c), () =>
        {
            var m = Mats.Lit(c, HairTex(), 0.38f);
            m.SetTextureScale("_BaseMap", new Vector2(6f, 1f));
            return m;
        });

        /// <summary>The same hair, drawn from both sides (thin falling hair has no inside).</summary>
        static Material HairMat2(Color c) => Mats.Shared("hum_hair2_" + ColorUtility.ToHtmlStringRGB(c), () =>
        {
            var m = new Material(HairMat(c));
            m.SetFloat("_Cull", 0f);
            m.doubleSidedGI = true;
            return m;
        });

        static Texture2D hairTex;
        /// <summary>Fine streaks along the strands (light in the middle, so the hair colour shows through).</summary>
        static Texture2D HairTex()
        {
            if (hairTex != null) return hairTex;
            const int W = 128, H = 128;
            var t = new Texture2D(W, H, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, name = "hair" };
            var rnd = new System.Random(7);
            var col = new float[W];
            for (int i = 0; i < W; i++) col[i] = 0.78f + (float)rnd.NextDouble() * 0.3f;
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float v = col[x] * (0.93f + 0.07f * Mathf.Sin(y * 0.3f + x)) ;
                    v = Mathf.Clamp01(v);
                    byte b = (byte)(v * 255f);
                    px[y * W + x] = new Color32(b, b, b, 255);
                }
            t.SetPixels32(px);
            t.Apply(true);
            hairTex = t;
            return t;
        }

        /// <summary>
        /// A shell over part of a profile (hair, beards, hats): every point below the hairline at its angle
        /// is pulled up onto the line, so one smooth piece has a shaped edge.
        /// </summary>
        public static Mesh Shell(string key, Profile p, float yMin, float yMax, float grow, Func<float, float> cutAt, bool keepBelow = false, Func<float, float, float> extra = null, int rings = 20, int seg = 36, float taper = 0f)
        {
            if (key != null && meshes.TryGetValue(key, out var cached)) return cached;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            int cols = seg + 1;
            for (int r = 0; r <= rings; r++)
            {
                float y = Mathf.Lerp(yMin, yMax, r / (float)rings);
                for (int sI = 0; sI <= seg; sI++)
                {
                    float ad = sI / (float)seg * 360f;
                    float a = ad * Mathf.Deg2Rad;
                    float cut = cutAt(ad);
                    float yy = keepBelow ? Mathf.Min(y, cut) : Mathf.Max(y, cut);
                    yy = Mathf.Clamp(yy, yMin, yMax);
                    var k = p.Eval(yy);
                    float g = grow + (extra != null ? extra(ad, yy) : 0f);
                    // Hair thins out to nothing at the hairline (no hard helmet edge).
                    if (taper > 0f && !keepBelow)
                    {
                        g *= Mathf.Lerp(0.15f, 1f, Mathf.SmoothStep(0f, 1f, (yy - cut) / taper));
                        // Clumps of strands running from the crown down.
                        g += grow * 0.18f * (Mathf.Sin(ad * Mathf.Deg2Rad * 44f + yy * 40f) * 0.6f + Mathf.Sin(ad * Mathf.Deg2Rad * 23f - yy * 25f) * 0.4f);
                    }
                    verts.Add(new Vector3(k.cx + Mathf.Cos(a) * (k.rx + g), yy, k.cz + Mathf.Sin(a) * (k.rz + g)));
                    uvs.Add(new Vector2(ad / 360f, (yy - yMin) / Mathf.Max(1e-4f, yMax - yMin)));
                }
            }
            for (int r = 0; r < rings; r++)
                for (int sI = 0; sI < seg; sI++)
                {
                    int i0 = r * cols + sI, i1 = i0 + 1, i2 = i0 + cols, i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            // Close the crown.
            int pole = verts.Count;
            var top = p.Eval(yMax);
            verts.Add(new Vector3(top.cx, yMax + grow * 0.85f, top.cz));
            uvs.Add(new Vector2(0.5f, 1f));
            if (!keepBelow)
                for (int sI = 0; sI < seg; sI++) { tris.Add(rings * cols + sI + 1); tris.Add(rings * cols + sI); tris.Add(pole); }
            var m = new Mesh { name = key ?? "shell" };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            var n = m.normals;
            for (int r = 0; r <= rings; r++) { int a0 = r * cols, b0 = a0 + seg; var avg = (n[a0] + n[b0]).normalized; n[a0] = avg; n[b0] = avg; }
            m.normals = n;
            m.RecalculateBounds();
            if (key != null) meshes[key] = m;
            return m;
        }

        /// <summary>How far down the hair comes at each angle (90° = the forehead, 270° = the nape).</summary>
        static Func<float, float> Hairline(Shape s, float front, float side, float back)
        {
            float hs = s.headSize;
            return ad =>
            {
                float f = Mathf.Max(0f, Mathf.Sin(ad * Mathf.Deg2Rad));        // 1 at the front
                float b = Mathf.Max(0f, -Mathf.Sin(ad * Mathf.Deg2Rad));       // 1 at the back
                return (front * f + back * b + side * (1f - f - b)) * hs;
            };
        }

        static void DressHair(Dress x, Wardrobe.SkinDef d, bool underHat)
        {
            if (d == null || d.none) return;
            var s = x.h.shape; var head = x.h.head;
            float hs = s.headSize;
            string g = s.female ? "f" : "m";
            var mat = HairMat(d.color);
            float yMin = -0.02f * hs, yMax = s.head.Y1;
            switch (d.style)
            {
                case "buzz":
                    Acc(x, "hair", Shell("hair2_buzz_" + g, s.head, yMin, yMax, 0.003f * hs, Hairline(s, 0.17f, 0.12f, 0.05f), taper: 0.035f * hs), mat, head);
                    break;
                case "short":
                    Acc(x, "hair", Shell("hair2_short_" + g, s.head, yMin, yMax, 0.011f * hs, Hairline(s, 0.165f, 0.125f, 0.045f), false, (a, y) => Mathf.Max(0f, y - 0.14f * hs) * 0.15f, taper: 0.035f * hs), mat, head);
                    break;
                case "bangs":
                    Acc(x, "hair", Shell("hair2_bangs_" + g, s.head, yMin, yMax, 0.013f * hs, Hairline(s, 0.125f, 0.11f, 0.04f), taper: 0.035f * hs), mat, head);
                    break;
                case "spiky" when underHat:
                case "mohawk" when underHat:
                case "afro" when underHat:
                case "curly" when underHat:
                    Acc(x, "hair", Shell("hair2_short_" + g, s.head, yMin, yMax, 0.011f * hs, Hairline(s, 0.165f, 0.125f, 0.045f), false, (a, y) => Mathf.Max(0f, y - 0.14f * hs) * 0.15f, taper: 0.035f * hs), mat, head);
                    break;
                case "spiky":
                {
                    Acc(x, "hair", Shell("hair2_short_" + g, s.head, yMin, yMax, 0.011f * hs, Hairline(s, 0.165f, 0.125f, 0.045f), false, (a, y) => Mathf.Max(0f, y - 0.14f * hs) * 0.15f, taper: 0.035f * hs), mat, head);
                    var spike = Loft("hair_spike", new Profile(new Key(0f, 0.016f, 0.016f), new Key(0.03f, 0.01f, 0.01f), new Key(0.055f, 0.001f, 0.001f)), 0f, 0.055f, 0f, true, false, 6, 8);
                    for (int i = 0; i < 11; i++)
                    {
                        float ad = 20f + i * 32.7f, el = i % 2 == 0 ? 0.18f : 0.205f;
                        var pnt = s.head.Point(el * hs, ad * Mathf.Deg2Rad, 0.008f * hs);
                        var outDir = (pnt - new Vector3(0f, 0.12f * hs, -0.01f)).normalized;
                        Acc(x, "hair_spike", spike, mat, head, pnt, Quaternion.FromToRotation(Vector3.up, outDir), Vector3.one * hs);
                    }
                    Acc(x, "hair_spike", spike, mat, head, new Vector3(0f, s.headTop - 0.006f, -0.005f), null, Vector3.one * hs * 1.2f);
                    break;
                }
                case "mohawk":
                {
                    Acc(x, "hair", Shell("hair2_buzz_" + g, s.head, yMin, yMax, 0.003f * hs, Hairline(s, 0.17f, 0.12f, 0.05f), taper: 0.035f * hs), Plain(Color.Lerp(d.color, Color.black, 0.3f), 0.2f), head);
                    for (int i = 0; i < 9; i++)
                    {
                        float t = i / 8f;
                        float ang = Mathf.Lerp(45f, 205f, t) * Mathf.Deg2Rad;      // from the forehead over the top to the back
                        var pnt = new Vector3(0f, 0.125f * hs + Mathf.Sin(ang) * 0.108f * hs, -0.004f + Mathf.Cos(ang) * 0.1f * hs);
                        var outDir = (pnt - new Vector3(0f, 0.12f * hs, -0.005f)).normalized;
                        Prim(x, PrimitiveType.Capsule, head, pnt + outDir * 0.02f * hs, new Vector3(0.016f, 0.035f + 0.01f * Mathf.Sin(t * Mathf.PI), 0.022f) * hs, mat, Quaternion.FromToRotation(Vector3.up, outDir));
                    }
                    break;
                }
                case "long":
                case "ponytail":
                case "bun":
                {
                    bool longH = d.style == "long";
                    Acc(x, "hair", Shell("hair2_cap_" + g, s.head, yMin, yMax, 0.012f * hs, Hairline(s, 0.17f, 0.115f, 0.04f), false, (a, y) => 0.002f, taper: 0.03f * hs), mat, head);
                    if (longH)
                    {
                        // Hair falling down the back and over the shoulders, a little wider at the ends.
                        var fall = new Profile(new Key(-0.21f * hs, 0.118f, 0.1f, -0.035f), new Key(-0.06f * hs, 0.1f, 0.095f, -0.022f), new Key(0.05f * hs, 0.09f, 0.1f, -0.006f), new Key(0.13f * hs, 0.092f, 0.108f, -0.008f));
                        Acc(x, "hair_long", Loft("hair_long_" + g, fall, -0.21f * hs, 0.13f * hs, 0.006f, false, false, 12, 30, 125f, 415f), HairMat2(d.color), head);
                    }
                    else if (d.style == "ponytail")
                    {
                        var pts = new List<Vector3> { new Vector3(0f, 0.16f * hs, -0.09f * hs), new Vector3(0f, 0.15f * hs, -0.13f * hs), new Vector3(0f, 0.09f * hs, -0.15f * hs), new Vector3(0f, 0.0f, -0.14f * hs), new Vector3(0f, -0.07f * hs, -0.12f * hs) };
                        Acc(x, "hair_tail", Tube("hair_tail_" + g, pts, 0.022f * hs, 10), mat, head);
                        Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, -0.075f * hs, -0.118f * hs), Vector3.one * 0.03f * hs, mat);
                        Prim(x, PrimitiveType.Cylinder, head, new Vector3(0f, 0.155f * hs, -0.112f * hs), new Vector3(0.05f, 0.008f, 0.05f) * hs, Plain(new Color(0.9f, 0.2f, 0.4f), 0.4f), Quaternion.Euler(-60f, 0f, 0f));
                    }
                    else
                    {
                        Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, 0.215f * hs, -0.06f * hs), new Vector3(0.075f, 0.065f, 0.075f) * hs, mat);
                        Prim(x, PrimitiveType.Cylinder, head, new Vector3(0f, 0.195f * hs, -0.05f * hs), new Vector3(0.06f, 0.008f, 0.06f) * hs, Plain(Color.Lerp(d.color, Color.black, 0.5f), 0.3f), Quaternion.Euler(-30f, 0f, 0f));
                    }
                    break;
                }
                case "curly":
                {
                    Acc(x, "hair", Shell("hair2_cap_curl_" + g, s.head, yMin, yMax, 0.018f * hs, Hairline(s, 0.165f, 0.11f, 0.04f), taper: 0.03f * hs), mat, head);
                    var rnd = new System.Random(11);
                    for (int i = 0; i < 46; i++)
                    {
                        float ad = (float)rnd.NextDouble() * 360f;
                        float y = Mathf.Lerp(0.05f, 0.225f, (float)rnd.NextDouble()) * hs;
                        if (y < Hairline(s, 0.17f, 0.11f, 0.04f)(ad) + 0.01f) continue;
                        var pnt = s.head.Point(y, ad * Mathf.Deg2Rad, 0.02f * hs);
                        Prim(x, PrimitiveType.Sphere, head, pnt, Vector3.one * Mathf.Lerp(0.026f, 0.036f, (float)rnd.NextDouble()) * hs, mat);
                    }
                    break;
                }
                case "afro":
                    Acc(x, "hair", Shell("hair2_buzz_" + g, s.head, yMin, yMax, 0.003f * hs, Hairline(s, 0.17f, 0.12f, 0.05f), taper: 0.035f * hs), mat, head);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, 0.17f * hs, -0.025f * hs), new Vector3(0.25f, 0.21f, 0.24f) * hs, mat);
                    break;
            }
            if (underHat) { }   // the hat simply sits over the hair
        }

        // ---------------- clothes ----------------

        static readonly Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// The cloth: knitted cotton (tees), denim with stitching, a check, stripes, and prints on the
        /// chest (u = 0.25 is the front of a loft). Light so the material colour tints it.
        /// </summary>
        static Texture2D ClothTex(string kind, Color baseC, Color accent, string print, int number, bool body)
        {
            string key = $"{kind}_{ColorUtility.ToHtmlStringRGB(baseC)}_{ColorUtility.ToHtmlStringRGB(accent)}_{print}_{number}_{body}";
            if (texCache.TryGetValue(key, out var t)) return t;
            // 0.4: bodies at 512 px with weave, seams, stitched hems, pockets and rivets
            int W = body ? 512 : 256, H = W;
            t = new Texture2D(W, H, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, name = "cloth_" + key, anisoLevel = 2 };
            var px = new Color[W * H];
            var rnd = new System.Random(key.GetHashCode());
            int k2 = W / 256;
            for (int yh = 0; yh < H; yh++)
                for (int xh = 0; xh < W; xh++)
                {
                    int x = xh / k2, y = yh / k2;
                    float u = xh / (float)W, v = yh / (float)H;
                    Color c = baseC;
                    float n = Mathf.PerlinNoise(x * 0.35f, y * 0.35f) * 0.06f - 0.03f;
                    switch (kind)
                    {
                        case "denim":
                        case "ripped":
                        {
                            float tw = ((x + y) % 4 < 2) ? 0.05f : -0.03f;
                            float fade = Mathf.PerlinNoise(x * 0.02f + 3f, y * 0.03f) * 0.18f;
                            c = Color.Lerp(baseC, Color.white, fade) * (1f + tw + n);
                            // Seams down the sides (u 0 and 0.5) in orange thread.
                            float du = Mathf.Min(Mathf.Abs(u - 0f), Mathf.Abs(u - 1f), Mathf.Abs(u - 0.5f));
                            if (du < 0.008f && (y / 3) % 2 == 0) c = new Color(0.85f, 0.6f, 0.25f);
                            if (kind == "ripped" && body)
                            {
                                float kx = Mathf.Abs(u - 0.25f) / 0.09f, ky = Mathf.Abs(v - 0.05f) / 0.06f;
                                if (kx * kx + ky * ky < 1f && (y % 5 != 0)) c = new Color(0.94f, 0.94f, 0.92f) * (0.92f + 0.08f * Mathf.Sin(x * 2f));
                            }
                            break;
                        }
                        case "check":
                        {
                            bool a = (x / 22) % 2 == 0, b = (y / 22) % 2 == 0;
                            c = a && b ? Color.Lerp(baseC, accent, 0.65f) : a || b ? Color.Lerp(baseC, accent, 0.3f) : baseC;
                            if (x % 22 == 11 || y % 22 == 11) c = Color.Lerp(c, Color.white, 0.18f);
                            c *= 1f + n;
                            break;
                        }
                        case "stripes":
                            c = ((y / 14) % 2 == 0 ? baseC : accent) * (1f + n);
                            break;
                        case "vstripes":
                            c = ((x / 12) % 2 == 0 ? baseC : accent) * (1f + n);
                            break;
                        case "ornament":
                        {
                            // Silk / velvet with a repeating gold pattern of rings and petals.
                            int cx = x % 32 - 16, cy = y % 32 - 16;
                            float r = Mathf.Sqrt(cx * cx + cy * cy);
                            float ang = Mathf.Atan2(cy, cx);
                            bool ringOn = Mathf.Abs(r - 9f) < 1.2f;
                            bool petal = r < 8f && Mathf.Abs(Mathf.Sin(ang * 4f)) * 7f > r && r > 2.5f;
                            bool dotOn = r < 1.6f;
                            c = (ringOn || petal || dotOn ? Color.Lerp(baseC, accent, 0.85f) : baseC) * (1f + n * 0.6f);
                            break;
                        }
                        case "sidestripes":
                        {
                            float du = Mathf.Min(Mathf.Abs(u - 0f), Mathf.Abs(u - 1f), Mathf.Abs(u - 0.5f));
                            c = (du < 0.03f && du > 0.012f ? accent : baseC) * (1f + n);
                            break;
                        }
                        default:
                            c = baseC * (1f + n + ((x + (y % 2)) % 2 == 0 ? 0.015f : -0.015f));
                            break;
                    }
                    // fine weave: threads crossing every pixel pair
                    c *= 1f + (((xh + (yh >> 1)) & 1) == 0 ? 0.02f : -0.02f);
                    px[yh * W + xh] = c;
                }
            if (body) Stitching(px, W, H, kind, baseC);
            if (body && (!string.IsNullOrEmpty(print) || number >= 0)) DrawPrint(px, W, H, print, accent, number);
            for (int i = 0; i < px.Length; i++) { var c = px[i]; px[i] = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f); }
            t.SetPixels(px);
            t.Apply(true);
            texCache[key] = t;
            return t;
        }

        /// <summary>Stitched hems and side seams for every garment; pockets and copper rivets for denim.</summary>
        static void Stitching(Color[] px, int W, int H, string kind, Color baseC)
        {
            bool denim = kind == "denim" || kind == "ripped";
            var thread = denim ? new Color(0.85f, 0.6f, 0.25f) : Color.Lerp(baseC, Color.white, 0.25f);
            var seamShade = baseC * 0.72f;
            void P(int x, int y, Color c) { if (y >= 0 && y < H) px[y * W + ((x % W) + W) % W] = c; }
            void Dash(float u0, float v0, float u1, float v1)
            {
                int n = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(u1 - u0) * W, Mathf.Abs(v1 - v0) * H));
                for (int i = 0; i <= n; i++) if ((i / 3) % 2 == 0) P(Mathf.RoundToInt(Mathf.Lerp(u0, u1, i / (float)n) * W), Mathf.RoundToInt(Mathf.Lerp(v0, v1, i / (float)n) * H), thread);
            }
            void Seam(float u0, float v0, float u1, float v1)
            {
                int n = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(u1 - u0) * W, Mathf.Abs(v1 - v0) * H));
                for (int i = 0; i <= n; i++) P(Mathf.RoundToInt(Mathf.Lerp(u0, u1, i / (float)n) * W), Mathf.RoundToInt(Mathf.Lerp(v0, v1, i / (float)n) * H), seamShade);
            }
            // side seams (u 0 / 0.5) with a stitch line beside each
            foreach (var su in new[] { 0f, 0.5f }) { Seam(su, 0, su, 1); Dash(su + 0.006f, 0, su + 0.006f, 1); }
            // double-stitched hem at the bottom and a single line at the top
            foreach (var hv in new[] { 0.025f, 0.04f }) Dash(0, hv, 1, hv);
            Seam(0, 0.05f, 1, 0.05f);
            Dash(0, 0.975f, 1, 0.975f);
            if (!denim) return;
            // back pockets (u 0.75 ± 0.07): outlined patches with a chevron, front pocket arcs, rivets
            foreach (var cu in new[] { 0.69f, 0.81f })
            {
                Dash(cu - 0.045f, 0.82f, cu + 0.045f, 0.82f);
                Dash(cu - 0.045f, 0.82f, cu - 0.04f, 0.6f); Dash(cu + 0.045f, 0.82f, cu + 0.04f, 0.6f);
                Dash(cu - 0.04f, 0.6f, cu, 0.55f); Dash(cu, 0.55f, cu + 0.04f, 0.6f);
                Dash(cu - 0.03f, 0.72f, cu, 0.68f); Dash(cu, 0.68f, cu + 0.03f, 0.72f);
            }
            foreach (var s in new[] { -1f, 1f })
            {
                float pu = 0.25f + s * 0.1f;
                for (int i = 0; i <= 20; i++) { float a = i / 20f * Mathf.PI * 0.5f; P(Mathf.RoundToInt((pu - s * Mathf.Cos(a) * 0.05f) * W), Mathf.RoundToInt((0.95f - Mathf.Sin(a) * 0.2f) * H), seamShade); }
                var rivet = new Color(0.78f, 0.5f, 0.28f);
                int rx = Mathf.RoundToInt((pu + s * 0.05f) * W), ry = Mathf.RoundToInt(0.93f * H);
                for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++) if (dx * dx + dy * dy <= 4) P(rx + dx, ry + dy, dx + dy < 0 ? rivet * 1.25f : rivet);
            }
            // fly stitching down the front
            Dash(0.235f, 0.97f, 0.235f, 0.72f); Dash(0.235f, 0.72f, 0.25f, 0.68f);
        }

        /// <summary>A print on the chest (front at u = 0.25), and the number on the back too for sports tops.</summary>
        static void DrawPrint(Color[] px, int W, int H, string print, Color c, int number)
        {
            void Dot(float u, float v, float r)
            {
                int cx = Mathf.RoundToInt(u * W), cy = Mathf.RoundToInt(v * H), rr = Mathf.CeilToInt(r * W);
                for (int y = cy - rr; y <= cy + rr; y++)
                    for (int x = cx - rr; x <= cx + rr; x++)
                    {
                        if (y < 0 || y >= H) continue;
                        float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / Mathf.Max(1, rr);
                        if (d <= 1f) px[y * W + ((x % W) + W) % W] = c;
                    }
            }
            void Line(float u0, float v0, float u1, float v1, float r) { for (int i = 0; i <= 30; i++) Dot(Mathf.Lerp(u0, u1, i / 30f), Mathf.Lerp(v0, v1, i / 30f), r); }
            float fu = 0.25f, fv = 0.6f;
            switch (print)
            {
                case "star":
                    for (int i = 0; i < 5; i++)
                    {
                        float a0 = (90f + i * 144f) * Mathf.Deg2Rad, a1 = (90f + (i + 1) * 144f) * Mathf.Deg2Rad;
                        Line(fu + Mathf.Cos(a0) * 0.07f, fv + Mathf.Sin(a0) * 0.16f, fu + Mathf.Cos(a1) * 0.07f, fv + Mathf.Sin(a1) * 0.16f, 0.012f);
                    }
                    break;
                case "heart":
                    for (float tt = 0f; tt < Mathf.PI * 2f; tt += 0.02f)
                    {
                        float hx = 16f * Mathf.Pow(Mathf.Sin(tt), 3f), hy = 13f * Mathf.Cos(tt) - 5f * Mathf.Cos(2f * tt) - 2f * Mathf.Cos(3f * tt) - Mathf.Cos(4f * tt);
                        for (float k = 0f; k <= 1f; k += 0.1f) Dot(fu + hx * 0.0042f * k, fv + hy * 0.0095f * k, 0.006f);
                    }
                    break;
                case "bolt":
                    Line(fu + 0.03f, fv + 0.16f, fu - 0.025f, fv + 0.0f, 0.016f);
                    Line(fu - 0.025f, fv + 0.0f, fu + 0.02f, fv + 0.0f, 0.016f);
                    Line(fu + 0.02f, fv + 0.0f, fu - 0.03f, fv - 0.17f, 0.016f);
                    break;
                case "smile":
                    for (float tt = 0f; tt < Mathf.PI * 2f; tt += 0.03f) Dot(fu + Mathf.Cos(tt) * 0.075f, fv + Mathf.Sin(tt) * 0.17f, 0.008f);
                    Dot(fu - 0.025f, fv + 0.05f, 0.012f); Dot(fu + 0.025f, fv + 0.05f, 0.012f);
                    for (float tt = 3.6f; tt < 5.8f; tt += 0.04f) Dot(fu + Mathf.Cos(tt) * 0.045f, fv - 0.01f + Mathf.Sin(tt) * 0.09f, 0.007f);
                    break;
                case "hay":
                    // "HH" in big block letters.
                    foreach (var ox in new[] { -0.045f, 0.03f })
                    {
                        Line(fu + ox, fv - 0.13f, fu + ox, fv + 0.13f, 0.01f);
                        Line(fu + ox + 0.03f, fv - 0.13f, fu + ox + 0.03f, fv + 0.13f, 0.01f);
                        Line(fu + ox, fv, fu + ox + 0.03f, fv, 0.01f);
                    }
                    break;
            }
            if (number >= 0)
            {
                foreach (var cu in new[] { 0.25f, 0.75f })
                {
                    string ns = number.ToString();
                    float w = 0.055f;
                    for (int i = 0; i < ns.Length; i++) Digit(Dot, ns[i] - '0', cu + (i - (ns.Length - 1) * 0.5f) * w * 1.5f, cu < 0.5f ? 0.55f : 0.6f, w, cu < 0.5f ? 0.3f : 0.4f);
                }
            }
        }

        /// <summary>A seven-segment digit centred at (u, v), w wide and h tall.</summary>
        static void Digit(Action<float, float, float> dot, int d, float u, float v, float w, float h)
        {
            // Segments a b c d e f g.
            int[] map = { 0b1111110, 0b0110000, 0b1101101, 0b1111001, 0b0110011, 0b1011011, 0b1011111, 0b1110000, 0b1111111, 0b1111011 };
            int m = map[Mathf.Clamp(d, 0, 9)];
            float l = u - w * 0.5f, r = u + w * 0.5f, t = v + h * 0.5f, mid = v, b = v - h * 0.5f, th = w * 0.16f;
            void Seg(float u0, float v0, float u1, float v1) { for (int i = 0; i <= 20; i++) dot(Mathf.Lerp(u0, u1, i / 20f), Mathf.Lerp(v0, v1, i / 20f), th); }
            if ((m & 0b1000000) != 0) Seg(l, t, r, t);
            if ((m & 0b0100000) != 0) Seg(r, t, r, mid);
            if ((m & 0b0010000) != 0) Seg(r, mid, r, b);
            if ((m & 0b0001000) != 0) Seg(l, b, r, b);
            if ((m & 0b0000100) != 0) Seg(l, mid, l, b);
            if ((m & 0b0000010) != 0) Seg(l, t, l, mid);
            if ((m & 0b0000001) != 0) Seg(l, mid, r, mid);
        }

        static Material Cloth(string kind, Color c, Color accent, string print = null, int number = -1, bool body = false, float smooth = 0.12f)
        {
            string key = $"hum_cloth_{kind}_{ColorUtility.ToHtmlStringRGB(c)}_{ColorUtility.ToHtmlStringRGB(accent)}_{print}_{number}_{body}";
            return Mats.Shared(key, () => Mats.LitNormal(Color.white, ClothTex(kind, c, accent, print, number, body), ClothNormal(), 0.9f, smooth));
        }

        static Texture2D clothN;
        /// <summary>The relief of the cloth: fine knitted ribs and soft creases (a normal map).</summary>
        static Texture2D ClothNormal()
        {
            if (clothN != null) return clothN;
            const int S = 512;
            var h = new float[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float knit = Mathf.Sin(x * Mathf.PI * 2f * 128f / S) * 0.5f + 0.5f + (((x + (y >> 1)) & 1) == 0 ? 0.3f : 0f);
                    float creases = ProcTex.PeriodicNoise(x, y, S, 6, 3.1f) * 0.6f + ProcTex.PeriodicNoise(x, y, S, 12, 7.7f) * 0.4f;
                    float stretch = ProcTex.PeriodicNoise(x, y * 3 % S, S, 8, 1.9f);
                    h[y * S + x] = knit * 0.05f + creases * 0.7f + stretch * 0.25f;
                }
            clothN = ProcTex.NormalFromHeight(h, S, 1.8f, "hum_clothN");
            return clothN;
        }

        /// <summary>Folds hanging down a top, the side seams, a little flare at the hem.</summary>
        static float TopFolds(float a, float y)
        {
            float hang = Mathf.Clamp01((0.22f - y) / 0.3f);
            float folds = (Mathf.Sin(a * Mathf.Deg2Rad * 11f + 0.6f) * 0.6f + Mathf.Sin(a * Mathf.Deg2Rad * 17f + 2f) * 0.4f) * 0.0028f * hang;
            float da = Mathf.Min(Mathf.Abs(Mathf.DeltaAngle(a, 0f)), Mathf.Abs(Mathf.DeltaAngle(a, 180f)));
            float seam = 0.0018f * Mathf.Exp(-(da / 5f) * (da / 5f));
            return folds + seam;
        }

        static float HemFolds(float a, float y) => (0.5f + 0.5f * (Mathf.Sin(a * Mathf.Deg2Rad * 11f + 0.6f) * 0.6f + Mathf.Sin(a * Mathf.Deg2Rad * 17f + 2f) * 0.4f)) * 0.004f + Mathf.Max(0f, -0.01f - y) * 0.12f;
        static float SleeveFolds(float a, float y) => Mathf.Sin(a * Mathf.Deg2Rad * 7f) * 0.0018f + 0.0022f * Mathf.Exp(-((y + 0.02f) / 0.03f) * ((y + 0.02f) / 0.03f)) * Mathf.Sin(y * 160f);
        static float LegFolds(float a, float y) => Mathf.Sin(a * Mathf.Deg2Rad * 6f + 1f) * 0.0016f + 0.0024f * Mathf.Exp(-((y + 0.42f) / 0.05f) * ((y + 0.42f) / 0.05f)) * Mathf.Sin(y * 110f);
        static float AnkleFolds(float a, float y) => Mathf.Sin(a * Mathf.Deg2Rad * 6f + 1f) * 0.0015f + (y < -0.28f ? 0.0028f * Mathf.Sin(y * 120f) * Mathf.Clamp01((-0.28f - y) / 0.06f) : 0f);

        static void DressTop(Dress x, Wardrobe.SkinDef d, int numberOverride = -1)
        {
            if (d == null || d.none) return;
            var h = x.h; var s = h.shape; string g = s.female ? "f" : "m";
            string st = d.style;
            bool ornate = st == "tang" || st == "chapan" || st == "kosovorotka" || st == "dress";
            string kind = st == "flannel" ? "check" : d.pattern == "stripes" ? "stripes" : d.pattern == "vstripes" ? "vstripes" : st == "tang" || st == "chapan" ? "ornament" : "plain";
            string print = st == "jersey" || st == "kit" || st == "gondolier" ? null : d.pattern != "stripes" && d.pattern != "vstripes" && !ornate ? d.pattern : null;
            var body = Cloth(kind, d.color, d.color2, print, -1, true, st == "chapan" ? 0.45f : st == "tang" || st == "dress" ? 0.4f : 0.12f);
            var plain = Cloth(kind, d.color, d.color2, null, -1, false, st == "chapan" ? 0.45f : st == "tang" || st == "dress" ? 0.4f : 0.12f);
            if (st == "blazer") { body = Cloth("plain", d.color2, d.color2); }   // the shirt under the jacket
            if (st == "dress" && d.pattern == "koylek") { body = Cloth("plain", d.color, d.color2, null, -1, false, 0.3f); plain = body; }
            if (st == "dress" && d.pattern == "sarafan") plain = Cloth("plain", d.color3, d.color3, null, -1, false, 0.2f);
            if (st == "blazer") plain = Cloth("plain", d.color, d.color, null, -1, false, 0.25f);
            float gr = st == "hoodie" || st == "chapan" ? 0.02f : st == "flannel" || st == "shirt" || st == "kosovorotka" || st == "tang" ? 0.014f : 0.011f;
            bool sleeves = st != "tank" && st != "jersey";
            bool longSleeves = st == "hoodie" || st == "flannel" || st == "shirt" || st == "kosovorotka" || st == "tang" || st == "chapan" || st == "blazer" || (st == "dress" && d.pattern != "qipao");
            float topY = st == "tank" ? 0.36f : st == "jersey" ? 0.39f : 0.403f;
            float hemY = st == "kosovorotka" || st == "tang" ? -0.1f : -0.035f;
            Acc(x, "top", Loft($"top2_{g}_{st}", s.torso, -0.075f, topY, gr, false, false, 24, 48, bump: TopFolds), body, h.torso);
            if (st != "dress")
            {
                Acc(x, "top_hem", Loft($"top3_low_{g}_{st}_{hemY:0.00}", s.pelvis, hemY, 0.12f, gr + 0.016f, false, false, 10, 48, bump: HemFolds), plain, h.hips);
                var hemRing = new List<Vector3>();
                for (int i = 0; i < 40; i++) { float a = i / 40f * 360f; hemRing.Add(s.pelvis.Point(hemY, a * Mathf.Deg2Rad, gr + 0.016f + HemFolds(a, hemY) + 0.001f)); }
                Acc(x, "top_hemline", Tube($"hem3_{g}_{st}_{hemY:0.00}", hemRing, ornate ? 0.009f : 0.0042f, 5, true), ornate ? Cloth("ornament", d.color2, d.color, null, -1, false, 0.5f) : Plain(Color.Lerp(d.color, Color.black, 0.18f), 0.1f), h.hips);
            }
            // A ribbed collar round the neck opening.
            var rib = Plain(Color.Lerp(d.color, st == "jersey" || st == "kit" ? d.color2 : Color.black, st == "jersey" || st == "kit" ? 0.8f : 0.12f), 0.1f);
            var ring = new List<Vector3>();
            for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; ring.Add(s.torso.Point(topY, a, gr + 0.002f)); }
            Acc(x, "top_collar", Tube($"collar_{g}_{d.style}", ring, 0.007f, 6, true), rib, h.torso);
            // A second rib below the collar and stitched seams: shoulders, round the hem.
            var ring2 = new List<Vector3>();
            for (int i = 0; i < 40; i++) { float a = i / 40f * Mathf.PI * 2f; ring2.Add(s.torso.Point(topY - 0.016f, a, gr + 0.0028f)); }
            var thread = Plain(Color.Lerp(d.color, d.color.grayscale > 0.5f ? Color.black : Color.white, 0.22f), 0.2f);
            Acc(x, "top_stitch", Stitches(ring2, 0.0011f), thread, h.torso);
            foreach (var sd in new[] { -1f, 1f })
            {
                var sh = new List<Vector3>();
                for (int i = 0; i <= 10; i++)
                {
                    float u = i / 10f;
                    float xx = sd * Mathf.Lerp(0.07f, 0.175f * s.shoulder, u), y = Mathf.Lerp(topY - 0.012f, 0.34f, u);
                    var k = s.torso.Eval(y);
                    float top2 = Mathf.Sqrt(Mathf.Max(0f, 1f - (xx / (k.rx + gr)) * (xx / (k.rx + gr))));
                    sh.Add(new Vector3(xx, y + 0.004f, k.cz + (k.rz + gr) * top2 * 0.2f - 0.006f));
                }
                Acc(x, "top_stitch", Stitches(sh, 0.0012f), thread, h.torso);
            }
            if (sleeves)
            {
                foreach (var (ua, fa, side) in new[] { (h.uArmL, h.fArmL, -1f), (h.uArmR, h.fArmR, 1f) })
                {
                    Acc(x, "top_shoulder", Loft($"top_delt_{g}_{gr:0.000}", s.uArm, -0.06f, s.uArm.Y1, gr + 0.006f, false, true, 8), plain, h.torso, Human.Bones.ShoulderFromTorso(s, side));
                    float sy = longSleeves ? s.uArm.Y0 - 0.01f : -0.14f;
                    Acc(x, "top_sleeve", Loft($"top2_sleeve_{g}_{gr:0.000}_{longSleeves}", s.uArm, sy, 0.03f, gr, false, false, 14, 32, bump: SleeveFolds), plain, ua);
                    if (!longSleeves)
                    {
                        var sr = new List<Vector3>();
                        for (int i = 0; i < 24; i++) { float a = i / 24f * 360f; sr.Add(s.uArm.Point(sy, a * Mathf.Deg2Rad, gr + SleeveFolds(a, sy) + 0.0015f)); }
                        Acc(x, "top_sleevehem", Tube($"sleevehem_{g}_{gr:0.000}", sr, 0.0038f, 5, true), Plain(Color.Lerp(d.color, Color.black, 0.18f), 0.1f), ua);
                    }
                    if (longSleeves)
                    {
                        Acc(x, "top_forearm", Loft($"top_farm_{g}_{gr:0.000}", s.fArm, -0.225f, s.fArm.Y1 + 0.01f, gr * 0.85f, false, false, 10), plain, fa);
                        var cuff = new List<Vector3>();
                        for (int i = 0; i < 16; i++) { float a = i / 16f * Mathf.PI * 2f; cuff.Add(s.fArm.Point(-0.225f, a, gr * 0.85f + 0.002f)); }
                        Acc(x, "top_cuff", Tube($"cuff_{g}_{gr:0.000}", cuff, 0.006f, 6, true), rib, fa);
                    }
                }
            }
            if (d.style == "tank")
            {
                // Straps over the shoulders.
                foreach (var sx in new[] { -1f, 1f })
                {
                    var pts = new List<Vector3>();
                    for (int i = 0; i <= 8; i++)
                    {
                        float a = Mathf.Lerp(80f, 280f, i / 8f) * Mathf.Deg2Rad;     // front, over the top, down the back
                        float y = 0.35f + Mathf.Sin(Mathf.Lerp(0f, Mathf.PI, i / 8f)) * 0.06f;
                        var k = s.torso.Eval(Mathf.Min(y, 0.39f));
                        pts.Add(new Vector3(sx * 0.085f * s.shoulder, y, k.cz + Mathf.Sin(a) * (k.rz + gr + 0.004f) * (y > 0.39f ? 0.7f : 1f)));
                    }
                    Acc(x, "top_strap", Tube(null, pts, 0.008f, 6), body, h.torso);
                }
            }
            if (d.style == "hoodie")
            {
                // The hood lying on the back, a front pocket and the drawstrings.
                Prim(x, PrimitiveType.Sphere, h.torso, new Vector3(0f, 0.36f, -0.085f), new Vector3(0.24f, 0.12f, 0.11f) * (s.female ? 0.9f : 1f), plain);
                float pz = s.torso.FrontZ(0.02f, 0f) + gr;
                Prim(x, PrimitiveType.Cube, h.torso, new Vector3(0f, 0.02f, pz), new Vector3(0.16f, 0.09f, 0.012f), plain);
                foreach (var sx in new[] { -0.03f, 0.03f })
                {
                    float z = s.torso.FrontZ(0.3f, sx) + gr + 0.004f;
                    Prim(x, PrimitiveType.Capsule, h.torso, new Vector3(sx, 0.27f, z), new Vector3(0.006f, 0.05f, 0.006f), Plain(Color.white, 0.3f));
                }
            }
            if (d.style == "flannel" || d.style == "shirt")
            {
                // Collar points and buttons down the front.
                foreach (var sx in new[] { -1f, 1f })
                {
                    var cp = s.torso.Point(0.36f, (90f + sx * 30f) * Mathf.Deg2Rad, gr + 0.006f);
                    Prim(x, PrimitiveType.Cube, h.torso, cp + new Vector3(0f, -0.01f, 0.004f), new Vector3(0.05f, 0.006f, 0.035f), plain, Quaternion.Euler(-25f, sx * 30f, sx * 20f));
                }
                var btn = Plain(d.style == "shirt" ? new Color(0.92f, 0.92f, 0.9f) : new Color(0.9f, 0.88f, 0.8f), 0.6f);
                for (int i = 0; i < 6; i++)
                {
                    float y = 0.33f - i * 0.065f;
                    float z = s.torso.FrontZ(y, 0f) + gr + 0.002f;
                    Prim(x, PrimitiveType.Cylinder, h.torso, new Vector3(0f, y, z), new Vector3(0.012f, 0.002f, 0.012f), btn, Quaternion.Euler(90f, 0f, 0f));
                }
                // A chest pocket.
                float pzz = s.torso.FrontZ(0.24f, 0.06f) + gr + 0.002f;
                Prim(x, PrimitiveType.Cube, h.torso, new Vector3(0.06f, 0.24f, pzz), new Vector3(0.05f, 0.055f, 0.004f), plain);
            }
        }

        /// <summary>The national costumes, suits and football kits on top of the basic top (2.16).</summary>
        static void DressTopExtras(Dress x, Wardrobe.SkinDef d, int numberOverride)
        {
            if (d == null || d.none) return;
            var h = x.h; var s = h.shape; string g = s.female ? "f" : "m";
            var gold = Cloth("ornament", d.color2, d.color, null, -1, false, 0.55f);
            var trim = Plain(d.color2, 0.45f, d.color2.r > 0.8f && d.color2.b < 0.4f ? 0.6f : 0f);
            switch (d.style)
            {
                case "kit":
                case "jersey":
                {
                    int num = numberOverride >= 0 ? numberOverride : d.number;
                    string name = d.style == "kit" ? d.model : null;
                    KitText(x, name, num, d.style == "kit" ? d.color3 : d.color2);
                    break;
                }
                case "kosovorotka":
                {
                    // Embroidered side placket, a woven sash with tassels.
                    var col = Plain(d.color2, 0.3f);
                    for (int i = 0; i < 5; i++)
                    {
                        var pnt = s.torso.Point(0.39f - i * 0.03f, 70f * Mathf.Deg2Rad, 0.016f);
                        Prim(x, PrimitiveType.Sphere, h.torso, pnt, Vector3.one * 0.008f, col);
                    }
                    var band = new List<Vector3>();
                    for (int i = 0; i < 30; i++) { float a = i / 30f * Mathf.PI * 2f; band.Add(s.torso.Point(0.4f, a, 0.016f)); }
                    Acc(x, "kos_collar", Tube($"kos_collar_{g}", band, 0.008f, 6, true), gold, h.torso);
                    var sash = new List<Vector3>();
                    for (int i = 0; i < 36; i++) { float a = i / 36f * Mathf.PI * 2f; sash.Add(s.pelvis.Point(0.05f, a, 0.024f)); }
                    Acc(x, "kos_sash", Tube($"kos_sash_{g}", sash, 0.011f, 8, true), col, h.hips);
                    foreach (var sx in new[] { -1f, 1f })
                    {
                        var tp = s.pelvis.Point(0.04f, (90f + 35f) * Mathf.Deg2Rad, 0.03f) + new Vector3(sx * 0.012f, -0.06f, 0f);
                        Prim(x, PrimitiveType.Capsule, h.hips, tp, new Vector3(0.012f, 0.05f, 0.012f), col);
                    }
                    break;
                }
                case "tang":
                {
                    MandarinCollar(x, gold);
                    var knot = Plain(d.color2, 0.5f, 0.3f);
                    for (int i = 0; i < 5; i++)
                    {
                        float y = 0.36f - i * 0.075f;
                        float z = s.torso.FrontZ(y, 0f) + 0.016f;
                        Prim(x, PrimitiveType.Capsule, h.torso, new Vector3(0f, y, z), new Vector3(0.008f, 0.022f, 0.008f), knot, Quaternion.Euler(0f, 0f, 90f));
                        Prim(x, PrimitiveType.Sphere, h.torso, new Vector3(0f, y, z + 0.004f), Vector3.one * 0.01f, knot);
                    }
                    break;
                }
                case "gondolier":
                {
                    // A red neckerchief tied at the front.
                    var red = Plain(d.color3, 0.3f);
                    var nk = new List<Vector3>();
                    for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; nk.Add(s.torso.Point(0.395f, a, 0.02f)); }
                    Acc(x, "gond_scarf", Tube($"gond_{g}", nk, 0.012f, 6, true), red, h.torso);
                    float fz = s.torso.FrontZ(0.36f, 0f) + 0.03f;
                    Prim(x, PrimitiveType.Sphere, h.torso, new Vector3(0f, 0.375f, fz), new Vector3(0.03f, 0.025f, 0.02f), red);
                    foreach (var sx in new[] { -1f, 1f })
                        Prim(x, PrimitiveType.Cube, h.torso, new Vector3(sx * 0.018f, 0.335f, fz - 0.004f), new Vector3(0.03f, 0.06f, 0.006f), red, Quaternion.Euler(0f, 0f, sx * 18f));
                    break;
                }
                case "blazer":
                {
                    // The jacket: open at the front over the white shirt, lapels, a tie, two buttons.
                    var jacket = Cloth("plain", d.color, d.color, null, -1, false, 0.25f);
                    Acc(x, "jacket", Loft($"jacket_{g}", s.torso, -0.075f, 0.395f, 0.02f, false, false, 22, 48, 112f, 428f, bump: (a, y) => TopFolds(a, y) * 0.6f), jacket, h.torso);
                    Acc(x, "jacket_low", Loft($"jacket_low_{g}", s.pelvis, -0.11f, 0.12f, 0.026f, false, false, 10, 48, 104f, 436f), jacket, h.hips);
                    foreach (var sx in new[] { -1f, 1f })
                    {
                        var lp = s.torso.Point(0.3f, (90f + sx * 16f) * Mathf.Deg2Rad, 0.024f);
                        Prim(x, PrimitiveType.Cube, h.torso, lp, new Vector3(0.035f, 0.16f, 0.008f), jacket, Quaternion.Euler(0f, sx * 18f, sx * -14f));
                    }
                    var tie = Plain(d.color3, 0.45f);
                    float tz = s.torso.FrontZ(0.25f, 0f) + 0.016f;
                    Prim(x, PrimitiveType.Cube, h.torso, new Vector3(0f, 0.37f, s.torso.FrontZ(0.37f, 0f) + 0.016f), new Vector3(0.022f, 0.02f, 0.012f), tie);
                    Prim(x, PrimitiveType.Cube, h.torso, new Vector3(0f, 0.24f, tz), new Vector3(0.032f, 0.24f, 0.006f), tie, Quaternion.Euler(-4f, 0f, 0f));
                    var btn = Plain(new Color(0.12f, 0.12f, 0.14f), 0.7f);
                    foreach (var by in new[] { 0.06f, 0.0f })
                        Prim(x, PrimitiveType.Cylinder, h.torso, new Vector3(0.035f, by, s.torso.FrontZ(by, 0.035f) + 0.022f), new Vector3(0.014f, 0.002f, 0.014f), btn, Quaternion.Euler(90f, 0f, 0f));
                    break;
                }
                case "chapan":
                {
                    // A long robe down to the knees with gold borders and a sash.
                    var robe = new Profile(new Key(-0.55f, 0.24f * s.hip, 0.2f, -0.01f), new Key(-0.3f, 0.2f * s.hip, 0.155f), new Key(-0.05f, 0.18f * s.hip, 0.125f), new Key(0.12f, 0.16f, 0.112f));
                    var velvet = Cloth("ornament", d.color, d.color2, null, -1, false, 0.45f);
                    Acc(x, "chapan_robe", Loft($"chapan_{g}", robe, -0.55f, 0.12f, 0.004f, false, false, 14, 48, bump: HemFolds), velvet, h.hips);
                    var hem = new List<Vector3>();
                    for (int i = 0; i < 48; i++) { float a = i / 48f * Mathf.PI * 2f; hem.Add(robe.Point(-0.55f, a, 0.008f)); }
                    Acc(x, "chapan_hem", Tube($"chapan_hem_{g}", hem, 0.014f, 6, true), gold, h.hips);
                    foreach (var y0 in new[] { -0.5f, 0.1f })
                    {
                        var pts = new List<Vector3>();
                        for (int i = 0; i <= 10; i++) { float y = Mathf.Lerp(y0, y0 == -0.5f ? 0.12f : 0.4f, i / 10f); pts.Add((y0 == -0.5f ? robe : s.torso).Point(y, 98f * Mathf.Deg2Rad, 0.03f)); }
                        Acc(x, "chapan_edge", Tube(null, pts, 0.01f, 6), gold, y0 == -0.5f ? h.hips : h.torso);
                    }
                    var sash = new List<Vector3>();
                    for (int i = 0; i < 36; i++) { float a = i / 36f * Mathf.PI * 2f; sash.Add(robe.Point(0.06f, a, 0.012f)); }
                    Acc(x, "chapan_sash", Tube($"chapan_sash_{g}", sash, 0.016f, 8, true), trim, h.hips);
                    MandarinCollar(x, gold);
                    break;
                }
                case "dress":
                {
                    bool qipao = d.pattern == "qipao", sarafan = d.pattern == "sarafan", koylek = d.pattern == "koylek";
                    float len = qipao ? -0.62f : -0.82f;
                    var skirt = new Profile(new Key(len, (qipao ? 0.17f : 0.3f) * s.hip, qipao ? 0.13f : 0.26f, qipao ? 0f : -0.01f), new Key(-0.3f, (qipao ? 0.17f : 0.22f) * s.hip, qipao ? 0.125f : 0.17f),
                        new Key(-0.05f, 0.17f * s.hip, 0.115f), new Key(0.12f, 0.15f, 0.1f));
                    var cloth = koylek ? Cloth("plain", d.color, d.color2, null, -1, false, 0.3f) : Cloth(qipao ? "ornament" : "plain", d.color, d.color2, null, -1, false, 0.4f);
                    Acc(x, "dress_skirt", Loft($"skirt_{g}_{d.pattern}", skirt, len, 0.12f, 0.006f, false, false, 16, 48, bump: (a, y) => koylek || sarafan ? Mathf.Sin(a * Mathf.Deg2Rad * 14f) * 0.006f * Mathf.Clamp01(-y / 0.6f) : 0f), cloth, h.hips);
                    var hem = new List<Vector3>();
                    for (int i = 0; i < 56; i++) { float a = i / 56f * Mathf.PI * 2f; hem.Add(skirt.Point(len, a, 0.01f + (koylek || sarafan ? Mathf.Sin(a * 14f) * 0.006f : 0f))); }
                    Acc(x, "dress_hem", Tube($"dress_hem_{g}_{d.pattern}", hem, koylek ? 0.02f : 0.012f, 6, true), koylek ? Plain(d.color, 0.3f) : gold, h.hips);
                    if (sarafan)
                    {
                        // The white blouse sleeves come from the colour of the skirt's straps; a gold band down the front.
                        var pts = new List<Vector3>();
                        for (int i = 0; i <= 12; i++) { float y = Mathf.Lerp(len, 0.12f, i / 12f); pts.Add(skirt.Point(y, Mathf.PI * 0.5f, 0.012f)); }
                        Acc(x, "sarafan_band", Tube(null, pts, 0.012f, 6), gold, h.hips);
                        var chest = new List<Vector3>();
                        for (int i = 0; i < 30; i++) { float a = i / 30f * Mathf.PI * 2f; chest.Add(s.torso.Point(0.3f, a, 0.016f)); }
                        Acc(x, "sarafan_top", Tube($"sarafan_top_{g}", chest, 0.012f, 6, true), gold, h.torso);
                    }
                    if (qipao)
                    {
                        MandarinCollar(x, gold);
                        var knot = Plain(d.color2, 0.5f, 0.3f);
                        for (int i = 0; i < 4; i++)
                        {
                            float a = (90f - 12f * i) * Mathf.Deg2Rad;
                            var pnt = s.torso.Point(0.37f - i * 0.03f, a, 0.016f);
                            Prim(x, PrimitiveType.Capsule, h.torso, pnt, new Vector3(0.007f, 0.018f, 0.007f), knot, Quaternion.Euler(0f, 0f, 90f));
                        }
                    }
                    if (koylek)
                    {
                        // A velvet camisole over the white dress, with gold edges.
                        var vel = Cloth("ornament", d.color3, d.color2, null, -1, false, 0.45f);
                        Acc(x, "kamzol", Loft($"kamzol_{g}", s.torso, -0.07f, 0.37f, 0.02f, false, false, 18, 48, 104f, 436f), vel, h.torso);
                        Acc(x, "kamzol_low", Loft($"kamzol_low_{g}", s.pelvis, -0.06f, 0.12f, 0.024f, false, false, 8, 48, 104f, 436f), vel, h.hips);
                        foreach (var sx in new[] { -1f, 1f })
                        {
                            var pts = new List<Vector3>();
                            for (int i = 0; i <= 10; i++) { float y = Mathf.Lerp(-0.07f, 0.37f, i / 10f); pts.Add(s.torso.Point(y, (90f + sx * 14f) * Mathf.Deg2Rad, 0.026f)); }
                            Acc(x, "kamzol_edge", Tube(null, pts, 0.008f, 6), gold, h.torso);
                        }
                    }
                    break;
                }
            }
        }

        static void MandarinCollar(Dress x, Material m)
        {
            var s = x.h.shape;
            var ring = new List<Vector3>();
            for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; ring.Add(s.neck.Point(0.01f, a, 0.012f)); }
            Acc(x, "collar_stand", Tube($"mandarin_{(s.female ? "f" : "m")}", ring, 0.016f, 6, true), m, x.h.neck);
        }

        /// <summary>The player's name and number on the back of a kit, the number small on the chest too.</summary>
        static void KitText(Dress x, string name, int number, Color c)
        {
            var s = x.h.shape;
            var t = x.h.torso;
            float Bz(float y) { var k = s.torso.Eval(y); return k.cz - k.rz - 0.017f; }
            float bz = Bz(0.17f);
            if (number >= 0)
            {
                var back = WorldBuilder.WorldText(t, new Vector3(0f, 0.17f, bz), Quaternion.identity, number.ToString(), 1.55f, c);
                back.outlineWidth = 0f; back.fontStyle = TMPro.FontStyles.Bold; back.name = "acc_kitnum";
                var front = WorldBuilder.WorldText(t, new Vector3(0.06f, 0.26f, s.torso.FrontZ(0.26f, 0.06f) + 0.018f), Quaternion.Euler(0f, 180f, 0f), number.ToString(), 0.55f, c);
                front.outlineWidth = 0f; front.name = "acc_kitnum_front";
                x.built.accessories.Add(back.GetComponent<Renderer>());
            }
            if (!string.IsNullOrEmpty(name))
            {
                var n = WorldBuilder.WorldText(t, new Vector3(0f, 0.3f, Bz(0.3f)), Quaternion.Euler(-8f, 0f, 0f), name, name.Length > 9 ? 0.32f : 0.42f, c);
                n.outlineWidth = 0f; n.name = "acc_kitname";
                n.characterSpacing = 4f;
            }
        }

        static void DressBottom(Dress x, Wardrobe.SkinDef d, Wardrobe.SkinDef top)
        {
            if (d == null || d.none) return;
            var h = x.h; var s = h.shape; string g = s.female ? "f" : "m";
            string kind = d.pattern == "denim" ? "denim" : d.pattern == "ripped" ? "ripped" : d.pattern == "plaid" ? "check" : d.style == "joggers" ? "sidestripes" : "plain";
            var mat = Cloth(kind, d.color, d.color2);
            var knees = Cloth(kind, d.color, d.color2, null, -1, true);
            if (d.style == "fshorts") { d = new Wardrobe.SkinDef { style = "shorts", color = d.color, color2 = d.color2, pattern = "fshorts" }; }
            float gr = d.style == "shorts" ? 0.016f : d.style == "cargo" ? 0.016f : 0.011f;
            var band = Plain(Color.Lerp(d.color, Color.black, 0.2f), 0.2f);
            if (d.style == "skirt")
            {
                var skirt = new Profile(new Key(-0.27f, 0.21f * s.hip, 0.17f), new Key(-0.12f, 0.18f * s.hip, 0.135f), new Key(0.04f, 0.155f * s.hip, 0.105f), new Key(0.1f, 0.14f, 0.098f));
                Acc(x, "skirt", Loft("skirt_" + g, skirt, -0.27f, 0.1f, 0.006f, false, false, 10, 36), mat, h.hips);
                Acc(x, "skirt_under", Loft("skirt_in_" + g, s.pelvis, s.pelvis.Y0, 0.1f, 0.006f, true, false, 10), mat, h.hips);
                return;
            }
            Acc(x, "pants", Loft($"pants_{g}_{gr:0.000}", s.pelvis, s.pelvis.Y0, 0.095f, gr, true, false, 12, 32), mat, h.hips);
            var ring = new List<Vector3>();
            for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; ring.Add(s.pelvis.Point(0.09f, a, gr + 0.003f)); }
            Acc(x, "pants_band", Tube($"band_{g}_{gr:0.000}", ring, 0.008f, 6, true), band, h.hips);
            bool full = d.style != "shorts";
            bool jeans = kind == "denim" || kind == "ripped";
            var thread = Plain(jeans ? new Color(0.85f, 0.6f, 0.25f) : Color.Lerp(d.color, Color.white, 0.25f), 0.2f);
            // Belt loops round the waistband, a fly seam down the front, pocket openings on the hips.
            for (int i = 0; i < 6; i++)
            {
                float a = (i < 3 ? 40f + i * 50f : 220f + (i - 3) * 50f) * Mathf.Deg2Rad;
                var lp = s.pelvis.Point(0.085f, a, gr + 0.008f);
                Prim(x, PrimitiveType.Cube, h.hips, lp, new Vector3(0.008f, 0.025f, 0.004f), band, Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, 0f));
            }
            var fly = new List<Vector3>();
            for (int i = 0; i <= 8; i++) { float y = Mathf.Lerp(0.075f, -0.07f, i / 8f); fly.Add(s.pelvis.Point(y, (98f - (i > 6 ? (i - 6) * 6f : 0f)) * Mathf.Deg2Rad, gr + 0.0015f)); }
            Acc(x, "pants_fly", Stitches(fly, 0.0012f), thread, h.hips);
            foreach (var sd in new[] { -1f, 1f })
            {
                var pk = new List<Vector3>();
                for (int i = 0; i <= 8; i++) { float u = i / 8f; float a = 90f + sd * Mathf.Lerp(28f, 62f, u); float y = Mathf.Lerp(0.075f, 0.0f, Mathf.Sqrt(u)); pk.Add(s.pelvis.Point(y, a * Mathf.Deg2Rad, gr + 0.002f)); }
                Acc(x, "pants_pocket", Tube(null, pk, 0.0018f, 4), band, h.hips);
                if (jeans)
                {
                    // Back pocket outlined in orange stitching and copper rivets at the front pockets.
                    var bp = new List<Vector3>();
                    float ac = 270f + sd * 35f;
                    foreach (var (da, y) in new[] { (-14f, 0.04f), (-12f, -0.035f), (0f, -0.05f), (12f, -0.035f), (14f, 0.04f) })
                        bp.Add(s.pelvis.Point(y, (ac + da) * Mathf.Deg2Rad, gr + 0.0018f));
                    Acc(x, "pants_backpocket", Stitches(bp, 0.0012f), thread, h.hips);
                    Prim(x, PrimitiveType.Sphere, h.hips, s.pelvis.Point(0.07f, (90f + sd * 30f) * Mathf.Deg2Rad, gr + 0.002f), Vector3.one * 0.006f, Plain(new Color(0.75f, 0.5f, 0.3f), 0.8f, 0.8f));
                }
                if (d.style == "joggers" || d.style == "shorts")
                {
                    // The drawstring hanging from the front of the waistband.
                    var cord = new List<Vector3> { s.pelvis.Point(0.085f, (90f + sd * 6f) * Mathf.Deg2Rad, gr + 0.01f), s.pelvis.Point(0.03f, (90f + sd * 8f) * Mathf.Deg2Rad, gr + 0.014f), s.pelvis.Point(-0.01f, (90f + sd * 7f) * Mathf.Deg2Rad, gr + 0.014f) };
                    Acc(x, "pants_cord", Tube(null, cord, 0.0022f, 5), Plain(Color.white, 0.3f), h.hips);
                }
            }
            foreach (var (th, sh) in new[] { (h.thighL, h.shinL), (h.thighR, h.shinR) })
            {
                float ty = full ? s.thigh.Y0 : d.pattern == "fshorts" ? -0.17f : -0.24f;
                Acc(x, "pants_leg", Loft($"pants2_thigh_{g}_{gr:0.000}_{full}_{ty:0.00}", s.thigh, ty, s.thigh.Y1, gr + (full ? 0f : 0.008f), false, true, 18, 36, bump: LegFolds), full ? knees : mat, th);
                if (full)
                {
                    float gs = d.style == "joggers" ? gr * 0.6f : gr + 0.004f;
                    Acc(x, "pants_shin", Loft($"pants2_shin_{g}_{gs:0.000}", s.shin, -0.36f, s.shin.Y1 + 0.01f, gs, false, false, 16, 32, bump: AnkleFolds), mat, sh);
                    if (d.style == "joggers")
                    {
                        var cuff = new List<Vector3>();
                        for (int i = 0; i < 16; i++) { float a = i / 16f * Mathf.PI * 2f; cuff.Add(s.shin.Point(-0.36f, a, gs + 0.004f)); }
                        Acc(x, "pants_cuff", Tube($"jcuff_{g}", cuff, 0.008f, 6, true), band, sh);
                    }
                    if (d.style == "cargo")
                    {
                        float side = th == h.thighR ? 1f : -1f;
                        var k = s.thigh.Eval(-0.2f);
                        Prim(x, PrimitiveType.Cube, th, new Vector3(side * (k.rx + gr + 0.01f), -0.2f, 0f), new Vector3(0.02f, 0.09f, 0.08f), Plain(Color.Lerp(d.color, Color.black, 0.1f), 0.1f));
                    }
                }
            }
        }

        static void DressShoes(Dress x, Wardrobe.SkinDef d)
        {
            if (d == null || d.none) return;
            var h = x.h; var s = h.shape;
            var upper = Plain(d.color, d.style == "boots" ? 0.35f : 0.3f);
            var sole = Plain(d.style == "boots" ? d.color2 : d.color2, 0.2f);
            var accent = Plain(d.color3, 0.4f);
            var lace = Plain(d.style == "boots" ? d.color3 : d.color.grayscale > 0.6f ? new Color(0.92f, 0.92f, 0.92f) : Color.white, 0.3f);
            foreach (var (foot, shin, side) in new[] { (h.footL, h.shinL, -1f), (h.footR, h.shinR, 1f) })
            {
                if (d.style == "kitboots")
                {
                    // Long football socks up to below the knee.
                    Acc(x, "sock", Loft($"sock_{s.female}", s.shin, -0.42f, -0.07f, 0.007f, false, false, 12, 28, bump: (a, y) => y > -0.12f ? 0.003f : 0f), Cloth("plain", d.color3, d.color3), shin);
                    var top = new List<Vector3>();
                    for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; top.Add(s.shin.Point(-0.075f, a, 0.012f)); }
                    Acc(x, "sock_top", Tube($"socktop_{s.female}", top, 0.007f, 5, true), Plain(Color.Lerp(d.color3, Color.white, 0.5f), 0.2f), shin);
                }
                if (d.style == "sapogi")
                {
                    Acc(x, "boot_shaft", Loft($"bootshaft_{s.female}", s.shin, -0.43f, -0.1f, 0.016f, false, false, 12, 28, bump: (a, y) => y < -0.3f ? 0.002f * Mathf.Sin(y * 90f) : 0f), Plain(d.color, 0.55f), shin);
                    var top = new List<Vector3>();
                    for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; top.Add(s.shin.Point(-0.1f, a, 0.02f)); }
                    Acc(x, "boot_top", Tube($"boottop_{s.female}", top, 0.008f, 5, true), Plain(d.color3, 0.5f, 0.3f), shin);
                }
                Acc(x, "shoe", FootMesh(s, 0.012f), upper, foot);
                // The sole: a thicker, flat slab under the shoe.
                Acc(x, "shoe_sole", FootMesh(s, 0.02f, "sole"), sole, foot, new Vector3(0f, -0.054f, 0f), null, new Vector3(1f, d.style == "boots" ? 0.42f : 0.34f, 1.02f));
                float collarTop = d.style == "boots" ? -0.25f : d.style == "hightop" ? -0.29f : d.style == "loafers" ? -0.395f : -0.355f;
                if (d.style != "sapogi") Acc(x, "shoe_collar", Loft($"shoe_collar_{s.female}_{collarTop:0.00}", s.shin, -0.43f, collarTop, 0.014f, false, false, 6, 24), upper, shin);
                // Laces across the top of the foot.
                for (int i = 0; i < (d.style == "loafers" || d.style == "sapogi" ? 0 : 4); i++)
                {
                    float z = 0.015f + i * 0.025f;
                    float yTop = 0.008f - i * 0.006f;
                    Prim(x, PrimitiveType.Capsule, foot, new Vector3(0f, yTop, z), new Vector3(0.006f, 0.022f, 0.006f), lace, Quaternion.Euler(0f, 0f, 90f));
                }
                if (d.style == "loafers") Prim(x, PrimitiveType.Cube, foot, new Vector3(0f, 0.0f, 0.06f), new Vector3(0.05f, 0.006f, 0.02f), accent);
                if (d.style == "sneakers" || d.style == "hightop" || d.style == "boots" || d.style == "cleats" || d.style == "kitboots")
                {
                    // Metal eyelets either side of the laces, the padded tongue, a stitched line round the toe.
                    var eyelet = Plain(new Color(0.75f, 0.76f, 0.8f), 0.85f, 0.8f);
                    for (int i = 0; i < 4; i++)
                        foreach (var sx in new[] { -1f, 1f })
                            Prim(x, PrimitiveType.Cylinder, foot, new Vector3(sx * 0.02f, 0.006f - i * 0.006f, 0.015f + i * 0.025f), new Vector3(0.006f, 0.0015f, 0.006f), eyelet, Quaternion.Euler(-12f, 0f, 0f));
                    Prim(x, PrimitiveType.Cube, foot, new Vector3(0f, 0.024f, -0.005f), new Vector3(0.036f, 0.04f, 0.012f), upper, Quaternion.Euler(-25f, 0f, 0f));
                    var toeLine = new List<Vector3>();
                    for (int i = 0; i <= 12; i++) { float a = Mathf.Lerp(-70f, 70f, i / 12f) * Mathf.Deg2Rad; toeLine.Add(new Vector3(Mathf.Sin(a) * 0.05f, -0.03f, 0.17f + Mathf.Cos(a) * 0.025f - 0.025f)); }
                    Acc(x, "shoe_stitch", Stitches(toeLine, 0.0011f), Plain(Color.Lerp(d.color, Color.black, 0.3f), 0.2f), foot);
                }
                if (d.style == "sneakers" || d.style == "cleats" || d.style == "kitboots")
                {
                    // The side stripe (both sides) and a heel tab.
                    foreach (var sx in new[] { -1f, 1f })
                        Prim(x, PrimitiveType.Capsule, foot, new Vector3(sx * 0.058f, -0.035f, 0.06f), new Vector3(0.006f, 0.05f, 0.016f), accent, Quaternion.Euler(70f, 0f, 0f));
                    Prim(x, PrimitiveType.Cube, foot, new Vector3(0f, -0.03f, -0.07f), new Vector3(0.03f, 0.04f, 0.012f), accent);
                }
                if (d.style == "hightop")
                {
                    // A white rubber toe cap and the round ankle patch.
                    Prim(x, PrimitiveType.Sphere, foot, new Vector3(0f, -0.05f, 0.17f), new Vector3(0.09f, 0.045f, 0.07f), sole);
                    Prim(x, PrimitiveType.Cylinder, shin, new Vector3(side * 0.05f, -0.34f, 0f), new Vector3(0.03f, 0.002f, 0.03f), Plain(Color.white, 0.4f), Quaternion.Euler(0f, 0f, 90f));
                }
                if (d.style == "cleats" || d.style == "kitboots")
                    for (int i = 0; i < 6; i++)
                        Prim(x, PrimitiveType.Cylinder, foot, new Vector3((i % 2 == 0 ? -1f : 1f) * 0.025f, -0.082f, -0.03f + (i / 2) * 0.07f), new Vector3(0.016f, 0.008f, 0.016f), accent);
            }
        }

        static void DressAcc(Dress x, Wardrobe.SkinDef d, Wardrobe.SkinDef hair)
        {
            if (d == null || d.none) return;
            var h = x.h; var s = h.shape; var head = h.head; float hs = s.headSize;
            string g = s.female ? "f" : "m";
            float hairLift = hair != null && !hair.none ? (hair.style == "afro" ? 0.05f : hair.style == "curly" || hair.style == "spiky" ? 0.02f : 0.012f) : 0f;
            var c = Plain(d.color, 0.25f);
            switch (d.style)
            {
                case "cap":
                {
                    Acc(x, "cap", Shell($"cap_{g}_{hairLift:0.00}", s.head, 0.1f * hs, s.head.Y1, (0.016f + hairLift) * hs, a => 0.15f * hs + Mathf.Max(0f, -Mathf.Sin(a * Mathf.Deg2Rad)) * 0.01f * hs), c, head);
                    float fz = s.head.FrontZ(0.155f * hs, 0f) + (0.016f + hairLift) * hs;
                    // A curved bill: a shell cut from a wide ellipse, bent down at the sides.
                    var bill = new List<Vector3>(); var billR = new List<float>();
                    for (int i = 0; i <= 12; i++) { float u = i / 12f * 2f - 1f; bill.Add(new Vector3(u * 0.075f * hs, 0.152f * hs - u * u * 0.012f * hs, fz + 0.04f * hs * (1f - u * u * 0.7f))); billR.Add(0.03f * hs * (1f - u * u * 0.85f) + 0.004f); }
                    Acc(x, "cap_bill", TubeVar(bill, billR, 10, new Vector3(1f, 0.16f, 1f)), c, head);
                    // Six panel seams from the button down, and little air holes.
                    var seamM = Plain(Color.Lerp(d.color, Color.black, 0.25f), 0.2f);
                    for (int k = 0; k < 6; k++)
                    {
                        float a = (90f + 30f + k * 60f) * Mathf.Deg2Rad;
                        var sm = new List<Vector3>();
                        for (int i = 0; i <= 8; i++) { float y = Mathf.Lerp(s.head.Y1 - 0.004f, 0.155f * hs, i / 8f); sm.Add(s.head.Point(y, a, (0.017f + hairLift) * hs)); }
                        Acc(x, "cap_seam", Stitches(sm, 0.0011f * hs, 0.004f, 0.003f), seamM, head);
                        Prim(x, PrimitiveType.Cylinder, head, s.head.Point(0.2f * hs, a + 0.25f, (0.0172f + hairLift) * hs), new Vector3(0.006f, 0.001f, 0.006f) * hs, seamM, Quaternion.FromToRotation(Vector3.up, s.head.Point(0.2f * hs, a + 0.25f, 0f).normalized));
                    }
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, s.headTop + (0.014f + hairLift) * hs, -0.008f), Vector3.one * 0.016f * hs, c);
                    // A white logo on the front.
                    var lp = s.head.Point(0.19f * hs, Mathf.PI * 0.5f, (0.018f + hairLift) * hs);
                    Prim(x, PrimitiveType.Cylinder, head, lp, new Vector3(0.026f, 0.002f, 0.026f) * hs, Plain(d.color2, 0.4f), Quaternion.Euler(70f, 0f, 0f));
                    break;
                }
                case "beanie":
                {
                    Acc(x, "beanie", Shell($"beanie_{g}_{hairLift:0.00}", s.head, 0.08f * hs, s.head.Y1, (0.02f + hairLift) * hs, a => 0.13f * hs - Mathf.Max(0f, -Mathf.Sin(a * Mathf.Deg2Rad)) * 0.02f * hs), Cloth("stripes", d.color, Color.Lerp(d.color, Color.black, 0.15f)), head);
                    var ring = new List<Vector3>();
                    for (int i = 0; i < 28; i++) { float a = i / 28f * Mathf.PI * 2f; float y = 0.13f * hs - Mathf.Max(0f, -Mathf.Sin(a)) * 0.02f * hs; ring.Add(s.head.Point(y + 0.012f, a, (0.026f + hairLift) * hs)); }
                    Acc(x, "beanie_cuff", Tube($"beanie_cuff_{g}_{hairLift:0.00}", ring, 0.009f * hs, 8, true), c, head);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, s.headTop + (0.03f + hairLift) * hs, -0.01f), Vector3.one * 0.05f * hs, Plain(d.color2, 0.1f));
                    break;
                }
                case "glasses":
                case "glasses_round":
                case "sunglasses":
                case "aviators":
                {
                    bool round = d.style == "glasses_round" || d.style == "aviators";
                    float ey = s.eyeY, ex = 0.031f * hs;
                    var frame = Plain(d.color, d.style == "aviators" || round ? 0.8f : 0.4f, d.style == "aviators" || round ? 0.8f : 0f);
                    bool dark = d.style == "sunglasses" || d.style == "aviators";
                    var lens = Mats.Shared(dark ? "hum_lens_dark" : "hum_lens", () => { var m = Mats.Transparent(dark ? new Color(0.05f, 0.05f, 0.08f, 0.85f) : new Color(0.8f, 0.9f, 1f, 0.22f)); m.SetFloat("_Smoothness", 0.95f); return m; });
                    foreach (var side in new[] { -1f, 1f })
                    {
                        var cpt = FacePoint(s, side * ex, ey, 0.012f * hs);
                        var rim = new List<Vector3>();
                        float rx = (round ? 0.019f : 0.021f) * hs, ry = (round ? 0.019f : 0.014f) * hs;
                        if (d.style == "aviators") ry = 0.018f * hs;
                        for (int i = 0; i < 20; i++) { float a = i / 20f * Mathf.PI * 2f; rim.Add(cpt + new Vector3(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry - (d.style == "aviators" ? Mathf.Max(0f, -Mathf.Sin(a)) * 0.004f * hs : 0f), 0f)); }
                        Acc(x, "glasses_rim", Tube(null, rim, 0.0022f * hs, 6, true), frame, head);
                        Prim(x, PrimitiveType.Sphere, head, cpt, new Vector3(rx * 2f, ry * 2f, 0.002f), lens);
                        // Temples back to the ears.
                        var k = s.head.Eval(ey);
                        var ear = new Vector3(side * (k.rx + 0.004f), ey + 0.004f, k.cz - 0.03f * hs);
                        Acc(x, "glasses_temple", Tube(null, new List<Vector3> { cpt + new Vector3(side * rx, 0.004f, 0f), ear }, 0.0018f * hs, 5), frame, head);
                    }
                    var bridge = new List<Vector3> { FacePoint(s, -ex + 0.019f * hs, ey + 0.004f, 0.013f * hs), FacePoint(s, 0f, ey + 0.008f, 0.014f * hs), FacePoint(s, ex - 0.019f * hs, ey + 0.004f, 0.013f * hs) };
                    Acc(x, "glasses_bridge", Tube(null, bridge, 0.002f * hs, 5), frame, head);
                    break;
                }
                case "kokoshnik":
                {
                    // A tall crescent standing over the forehead, pearls round its edge and a jewel.
                    float top = s.headTop + hairLift * hs;
                    var crest = Plain(d.color, 0.45f);
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, top - 0.04f * hs, 0.01f * hs), new Vector3(0.24f, 0.24f, 0.035f) * hs, crest);
                    var pearl = Plain(new Color(0.97f, 0.96f, 0.92f), 0.85f);
                    for (int i = 0; i <= 14; i++)
                    {
                        float a = Mathf.Lerp(10f, 170f, i / 14f) * Mathf.Deg2Rad;
                        Prim(x, PrimitiveType.Sphere, head, new Vector3(Mathf.Cos(a) * 0.12f * hs, top - 0.04f * hs + Mathf.Sin(a) * 0.12f * hs, 0.03f * hs), Vector3.one * 0.013f * hs, pearl);
                    }
                    Prim(x, PrimitiveType.Sphere, head, new Vector3(0f, top + 0.02f * hs, 0.035f * hs), new Vector3(0.04f, 0.05f, 0.015f) * hs, Plain(d.color2, 0.8f, 0.8f));
                    for (int k = -1; k <= 1; k++)
                        Prim(x, PrimitiveType.Sphere, head, new Vector3(k * 0.06f * hs, top - 0.01f * hs - Mathf.Abs(k) * 0.03f * hs, 0.032f * hs), Vector3.one * 0.022f * hs, Plain(d.color2, 0.8f, 0.8f));
                    break;
                }
                case "kalpak":
                {
                    // A tall white felt hat with a black brim turned up and split at the front.
                    var felt = Plain(d.color, 0.15f);
                    var k = new Profile(new Key(0f, 0.095f, 0.11f), new Key(0.08f, 0.09f, 0.1f), new Key(0.17f, 0.06f, 0.07f), new Key(0.21f, 0.025f, 0.03f), new Key(0.22f, 0.004f, 0.004f));
                    Acc(x, "kalpak", Loft("kalpak", k, 0f, 0.22f, 0f, false, true, 10, 28), felt, head, new Vector3(0f, 0.15f * hs + hairLift * hs, -0.008f), null, Vector3.one * hs);
                    var brim = new List<Vector3>();
                    for (int i = 0; i <= 20; i++) { float a = Mathf.Lerp(105f, 435f, i / 20f) * Mathf.Deg2Rad; brim.Add(new Vector3(Mathf.Cos(a) * 0.1f * hs, 0.165f * hs + hairLift * hs, -0.008f + Mathf.Sin(a) * 0.115f * hs)); }
                    Acc(x, "kalpak_brim", Tube(null, brim, 0.018f * hs, 6), Plain(d.color2, 0.3f), head);
                    break;
                }
                case "takiya":
                {
                    var cap = Shell($"takiya_{g}_{hairLift:0.00}", s.head, 0.15f * hs, s.head.Y1, (0.012f + hairLift) * hs, a => 0.17f * hs);
                    Acc(x, "takiya", cap, Cloth("ornament", d.color, d.color2, null, -1, false, 0.4f), head);
                    var ring = new List<Vector3>();
                    for (int i = 0; i < 28; i++) { float a = i / 28f * Mathf.PI * 2f; ring.Add(s.head.Point(0.172f * hs, a, (0.014f + hairLift) * hs)); }
                    Acc(x, "takiya_band", Tube($"takiya_band_{g}_{hairLift:0.00}", ring, 0.008f * hs, 6, true), Plain(d.color2, 0.3f), head);
                    break;
                }
                case "saukele":
                {
                    // The tall Kazakh bridal hat: a red cone with gold bands, a feather on top, pendants and a veil.
                    var cone = new Profile(new Key(0f, 0.1f, 0.115f), new Key(0.1f, 0.085f, 0.095f), new Key(0.3f, 0.05f, 0.055f), new Key(0.42f, 0.02f, 0.022f), new Key(0.44f, 0.004f, 0.004f));
                    var basePos = new Vector3(0f, 0.15f * hs + hairLift * hs, -0.01f);
                    Acc(x, "saukele", Loft("saukele", cone, 0f, 0.44f, 0f, false, true, 14, 28), Cloth("ornament", d.color, d.color2, null, -1, false, 0.45f), head, basePos, null, Vector3.one * hs);
                    var goldM = Plain(d.color2, 0.7f, 0.7f);
                    foreach (var by in new[] { 0.02f, 0.14f, 0.26f })
                    {
                        var ring = new List<Vector3>();
                        for (int i = 0; i < 24; i++) { float a = i / 24f * Mathf.PI * 2f; ring.Add(basePos + cone.Point(by, a, 0.004f) * hs); }
                        Acc(x, "saukele_band", Tube(null, ring, 0.007f * hs, 5, true), goldM, head);
                    }
                    Prim(x, PrimitiveType.Capsule, head, basePos + new Vector3(0f, 0.5f, -0.01f) * hs, new Vector3(0.02f, 0.08f, 0.02f) * hs, Plain(new Color(0.97f, 0.97f, 0.97f), 0.2f), Quaternion.Euler(-15f, 0f, 0f));
                    foreach (var sx in new[] { -1f, 1f })
                        for (int i = 0; i < 4; i++)
                            Prim(x, PrimitiveType.Sphere, head, basePos + new Vector3(sx * 0.105f, -0.02f - i * 0.035f, 0.02f) * hs, Vector3.one * 0.016f * hs, goldM);
                    var veil = Mats.Shared("hum_veil", () => { var m = Mats.Transparent(new Color(1f, 1f, 1f, 0.45f)); m.SetFloat("_Cull", 0f); return m; });
                    var vp = new Profile(new Key(-0.4f, 0.16f, 0.14f, -0.05f), new Key(0f, 0.11f, 0.12f, -0.02f), new Key(0.05f, 0.105f, 0.12f));
                    Acc(x, "saukele_veil", Loft("saukele_veil", vp, -0.4f, 0.05f, 0f, false, false, 8, 24, 180f, 360f), veil, head, basePos, null, Vector3.one * hs);
                    break;
                }
                case "conehat":
                {
                    var cone = new Profile(new Key(0f, 0.3f, 0.3f), new Key(0.06f, 0.16f, 0.16f), new Key(0.13f, 0.01f, 0.01f));
                    var straw = Mats.Shared("hum_straw", () => { var m = Mats.Lit(d.color, HairTex(), 0.2f); m.SetTextureScale("_BaseMap", new Vector2(10f, 2f)); m.SetFloat("_Cull", 0f); return m; });
                    Acc(x, "conehat", Loft("conehat", cone, 0f, 0.13f, 0f, false, true, 8, 40), straw, head, new Vector3(0f, s.headTop - 0.035f * hs + hairLift * hs, 0f), null, Vector3.one * hs);
                    break;
                }
                case "boater":
                {
                    var straw = Mats.Shared("hum_boater", () => { var m = Mats.Lit(d.color, HairTex(), 0.25f); m.SetTextureScale("_BaseMap", new Vector2(12f, 2f)); return m; });
                    float by = 0.17f * hs + hairLift * hs;
                    Prim(x, PrimitiveType.Cylinder, head, new Vector3(0f, by, -0.005f), new Vector3(0.3f, 0.004f, 0.32f) * hs, straw);
                    Prim(x, PrimitiveType.Cylinder, head, new Vector3(0f, by + 0.035f * hs, -0.005f), new Vector3(0.19f, 0.035f, 0.21f) * hs, straw);
                    Prim(x, PrimitiveType.Cylinder, head, new Vector3(0f, by + 0.015f * hs, -0.005f), new Vector3(0.195f, 0.012f, 0.215f) * hs, Plain(d.color2, 0.3f));
                    break;
                }
                case "headphones":
                {
                    var band = new List<Vector3>();
                    var k = s.head.Eval(0.11f * hs);
                    for (int i = 0; i <= 16; i++)
                    {
                        float a = Mathf.Lerp(0f, 180f, i / 16f) * Mathf.Deg2Rad;
                        band.Add(new Vector3(Mathf.Cos(a) * (k.rx + 0.03f * hs), 0.11f * hs + Mathf.Sin(a) * (s.headTop - 0.11f * hs + 0.03f * hs + hairLift), k.cz - 0.01f));
                    }
                    Acc(x, "headphones_band", Tube($"hp_band_{g}_{hairLift:0.00}", band, 0.008f * hs, 8), Plain(d.color2, 0.6f, 0.5f), head);
                    foreach (var side in new[] { -1f, 1f })
                    {
                        var cp = new Vector3(side * (k.rx + 0.02f * hs), 0.1f * hs, k.cz - 0.01f);
                        Prim(x, PrimitiveType.Cylinder, head, cp, new Vector3(0.065f, 0.018f, 0.075f) * hs, c, Quaternion.Euler(0f, 0f, 90f));
                        Prim(x, PrimitiveType.Cylinder, head, cp + new Vector3(side * 0.02f * hs, 0f, 0f), new Vector3(0.04f, 0.004f, 0.045f) * hs, Plain(d.color2, 0.6f, 0.5f), Quaternion.Euler(0f, 0f, 90f));
                    }
                    break;
                }
            }
        }

        /// <summary>A phone in the left hand, screen towards the face (the arm holds it up — see HumanAnimator).</summary>
        static void HoldPhone(Dress x, Wardrobe.SkinDef d) { }
    }
}
