using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    public enum WType { Auto, Semi, Rocket, Melee, Throw, Grenade }

    /// <summary>One weapon from the shop (the user's weapon sheet: АК-74, ТТ, РПГ-7, СВД, Мосин, ножи, сюрикены…).</summary>
    public class WeaponDef
    {
        public string id, name, cat;
        public WType type;
        public float dmg, rate, reload = 2f, spread, moveSpread = 0.03f, recoil = 0.01f, zoom, range = 200, speed, gravity, splash, fuse, reach = 2.4f;
        public int mag, reserve, price, pellets = 1;
        public string sound = "pistol";

        public bool Hitscan => type == WType.Auto || type == WType.Semi;
        public bool Projectile => type == WType.Rocket || type == WType.Throw || type == WType.Grenade;
        public bool UsesAmmo => type != WType.Melee;
    }

    public class SkinDef
    {
        public string id, name;
        public int price;
        public Color tint;
    }

    /// <summary>First-aid kit from the shop: bought into stock, used in a fight.</summary>
    public class MedDef
    {
        public string id, name;
        public int heal, price;
        public Color color;
    }

    public class MapDef
    {
        public string id, name, desc;
    }

    public static class Defs
    {
        public static readonly string[] Cats = { "primary", "pistol", "melee", "throw", "grenade" };
        public static readonly string[] CatNames = { "Основное", "Пистолеты", "Ножи", "Метательное", "Гранаты" };

        public static readonly List<WeaponDef> Weapons = new List<WeaponDef>
        {
            new WeaponDef { id = "ak74", name = "АК-74", cat = "primary", type = WType.Auto, dmg = 24, rate = 10, mag = 30, reserve = 150, reload = 2.2f, spread = 0.012f, moveSpread = 0.035f, recoil = 0.012f, price = 800, sound = "rifle" },
            new WeaponDef { id = "svd", name = "СВД", cat = "primary", type = WType.Semi, dmg = 75, rate = 2.2f, mag = 10, reserve = 40, reload = 2.8f, spread = 0.035f, moveSpread = 0.06f, recoil = 0.04f, zoom = 3.5f, range = 300, price = 1400, sound = "sniper" },
            new WeaponDef { id = "mosin", name = "Винтовка Мосина", cat = "primary", type = WType.Semi, dmg = 110, rate = 1f, mag = 5, reserve = 30, reload = 3.2f, spread = 0.04f, moveSpread = 0.07f, recoil = 0.06f, zoom = 4f, range = 300, price = 1200, sound = "sniper" },
            new WeaponDef { id = "rpg7", name = "РПГ-7", cat = "primary", type = WType.Rocket, dmg = 130, splash = 5, rate = 0.8f, mag = 1, reserve = 5, reload = 2.6f, speed = 42, recoil = 0.08f, price = 2500, sound = "rocket" },
            new WeaponDef { id = "m4a1", name = "M4A1", cat = "primary", type = WType.Auto, dmg = 22, rate = 11, mag = 30, reserve = 150, reload = 2f, spread = 0.01f, moveSpread = 0.03f, recoil = 0.01f, price = 900, sound = "rifle" },
            new WeaponDef { id = "mp5", name = "MP5", cat = "primary", type = WType.Auto, dmg = 17, rate = 13, mag = 30, reserve = 180, reload = 1.8f, spread = 0.016f, moveSpread = 0.02f, recoil = 0.007f, price = 650, sound = "pistol" },
            new WeaponDef { id = "pkm", name = "Пулемёт ПКМ", cat = "primary", type = WType.Auto, dmg = 26, rate = 11, mag = 100, reserve = 200, reload = 4.5f, spread = 0.02f, moveSpread = 0.06f, recoil = 0.014f, price = 2200, sound = "rifle" },
            new WeaponDef { id = "shotgun", name = "Дробовик Remington", cat = "primary", type = WType.Semi, dmg = 14, pellets = 8, rate = 1.1f, mag = 6, reserve = 36, reload = 2.8f, spread = 0.07f, moveSpread = 0.03f, recoil = 0.07f, range = 45, price = 1000, sound = "shotgun" },
            new WeaponDef { id = "saiga", name = "Сайга-12", cat = "primary", type = WType.Semi, dmg = 12, pellets = 8, rate = 3f, mag = 8, reserve = 40, reload = 2.4f, spread = 0.08f, moveSpread = 0.03f, recoil = 0.05f, range = 40, price = 1500, sound = "shotgun" },
            new WeaponDef { id = "awp", name = "AWP", cat = "primary", type = WType.Semi, dmg = 140, rate = 0.75f, mag = 5, reserve = 25, reload = 3.4f, spread = 0.05f, moveSpread = 0.1f, recoil = 0.07f, zoom = 5f, range = 350, price = 2800, sound = "sniper" },
            new WeaponDef { id = "tt", name = "ТТ", cat = "pistol", type = WType.Semi, dmg = 30, rate = 5, mag = 8, reserve = 48, reload = 1.5f, spread = 0.01f, recoil = 0.025f, price = 0, sound = "pistol" },
            new WeaponDef { id = "glock", name = "Glock 17", cat = "pistol", type = WType.Semi, dmg = 22, rate = 8, mag = 17, reserve = 85, reload = 1.4f, spread = 0.012f, recoil = 0.018f, price = 350, sound = "pistol" },
            new WeaponDef { id = "colt", name = "Кольт 1911", cat = "pistol", type = WType.Semi, dmg = 38, rate = 4, mag = 7, reserve = 42, reload = 1.6f, spread = 0.01f, recoil = 0.03f, price = 450, sound = "pistol" },
            new WeaponDef { id = "uzi", name = "УЗИ", cat = "pistol", type = WType.Auto, dmg = 15, rate = 14, mag = 25, reserve = 125, reload = 1.6f, spread = 0.02f, recoil = 0.006f, price = 600, sound = "pistol" },
            new WeaponDef { id = "deagle", name = "Desert Eagle", cat = "pistol", type = WType.Semi, dmg = 55, rate = 2.5f, mag = 7, reserve = 35, reload = 1.9f, spread = 0.012f, recoil = 0.05f, price = 900, sound = "sniper" },
            new WeaponDef { id = "nagant", name = "Револьвер Наган", cat = "pistol", type = WType.Semi, dmg = 42, rate = 2.5f, mag = 7, reserve = 42, reload = 2.6f, spread = 0.01f, recoil = 0.035f, price = 550, sound = "pistol" },
            new WeaponDef { id = "kabar", name = "Нож КА-БАР", cat = "melee", type = WType.Melee, dmg = 50, rate = 2.2f, reach = 2.5f, price = 0, sound = "melee" },
            new WeaponDef { id = "butterfly", name = "Нож-бабочка", cat = "melee", type = WType.Melee, dmg = 45, rate = 3f, reach = 2.3f, price = 500, sound = "melee" },
            new WeaponDef { id = "karambit", name = "Керамбит", cat = "melee", type = WType.Melee, dmg = 40, rate = 3.5f, reach = 2.2f, price = 800, sound = "melee" },
            new WeaponDef { id = "machete", name = "Мачете", cat = "melee", type = WType.Melee, dmg = 60, rate = 1.9f, reach = 2.8f, price = 600, sound = "melee" },
            new WeaponDef { id = "katana", name = "Катана", cat = "melee", type = WType.Melee, dmg = 75, rate = 1.6f, reach = 3.2f, price = 1200, sound = "melee" },
            new WeaponDef { id = "shuriken", name = "Сюрикен", cat = "throw", type = WType.Throw, dmg = 45, rate = 2.5f, mag = 6, speed = 38, gravity = 4, price = 350, sound = "throw" },
            new WeaponDef { id = "kunai", name = "Кунай", cat = "throw", type = WType.Throw, dmg = 60, rate = 1.8f, mag = 4, speed = 34, gravity = 6, price = 450, sound = "throw" },
            new WeaponDef { id = "axe", name = "Метательный топор", cat = "throw", type = WType.Throw, dmg = 90, rate = 1.1f, mag = 3, speed = 26, gravity = 12, price = 650, sound = "throw" },
            new WeaponDef { id = "f1", name = "Граната Ф-1", cat = "grenade", type = WType.Grenade, dmg = 140, splash = 6, rate = 1f, mag = 2, speed = 18, gravity = 18, fuse = 2.5f, price = 700, sound = "throw" },
            new WeaponDef { id = "rgd5", name = "Граната РГД-5", cat = "grenade", type = WType.Grenade, dmg = 110, splash = 5, rate = 1f, mag = 3, speed = 20, gravity = 18, fuse = 2f, price = 500, sound = "throw" },
        };

        public static readonly List<SkinDef> Skins = new List<SkinDef>
        {
            new SkinDef { id = "std", name = "Стандарт", price = 0, tint = new Color(0.25f, 0.27f, 0.3f) },
            new SkinDef { id = "digi", name = "Цифровой камуфляж", price = 150, tint = new Color(0.4f, 0.47f, 0.3f) },
            new SkinDef { id = "wood", name = "Гравировка по дереву", price = 200, tint = new Color(0.55f, 0.36f, 0.18f) },
            new SkinDef { id = "pixel", name = "Пиксель-арт", price = 250, tint = new Color(0.3f, 0.7f, 1f) },
            new SkinDef { id = "sticker", name = "Стикербомба", price = 300, tint = new Color(1f, 0.5f, 0.7f) },
            new SkinDef { id = "circuit", name = "Микросхема", price = 400, tint = new Color(0.1f, 0.5f, 0.25f) },
            new SkinDef { id = "jade", name = "Нефрит", price = 450, tint = new Color(0.2f, 0.65f, 0.45f) },
            new SkinDef { id = "lava", name = "Поток лавы", price = 500, tint = new Color(1f, 0.4f, 0.05f) },
            new SkinDef { id = "damascus", name = "Дамасская сталь", price = 550, tint = new Color(0.65f, 0.67f, 0.7f) },
            new SkinDef { id = "obsidian", name = "Обсидиан", price = 600, tint = new Color(0.2f, 0.12f, 0.3f) },
            new SkinDef { id = "skull", name = "Черепа", price = 650, tint = new Color(0.85f, 0.85f, 0.85f) },
            new SkinDef { id = "carbon", name = "Углепластик", price = 700, tint = new Color(0.15f, 0.15f, 0.17f) },
            new SkinDef { id = "rainbow", name = "Радуга", price = 800, tint = new Color(0.9f, 0.4f, 1f) },
            new SkinDef { id = "cosmic", name = "Космическая туманность", price = 1000, tint = new Color(0.45f, 0.2f, 0.9f) },
            new SkinDef { id = "golden", name = "Золотой", price = 1500, tint = new Color(1f, 0.8f, 0.25f) },
            new SkinDef { id = "tiger", name = "Тигр", price = 550, tint = new Color(1f, 0.55f, 0.1f) },
            new SkinDef { id = "flames", name = "Пламя", price = 700, tint = new Color(1f, 0.35f, 0.05f) },
            new SkinDef { id = "dragon", name = "Дракон", price = 1200, tint = new Color(0.2f, 0.6f, 0.3f) },
            new SkinDef { id = "neon", name = "Неон", price = 900, tint = new Color(0.3f, 0.9f, 1f) },
            new SkinDef { id = "hex", name = "Соты", price = 500, tint = new Color(0.9f, 0.7f, 0.2f) },
            new SkinDef { id = "zebra", name = "Зебра", price = 350, tint = Color.white },
            new SkinDef { id = "leopard", name = "Леопард", price = 450, tint = new Color(0.85f, 0.65f, 0.35f) },
            new SkinDef { id = "ice", name = "Лёд", price = 650, tint = new Color(0.6f, 0.85f, 1f) },
            new SkinDef { id = "toxic", name = "Токсик", price = 750, tint = new Color(0.4f, 1f, 0.2f) },
            new SkinDef { id = "money", name = "Доллары", price = 1300, tint = new Color(0.4f, 0.65f, 0.35f) },
            new SkinDef { id = "hearts", name = "Сердечки", price = 300, tint = new Color(1f, 0.5f, 0.7f) },
            new SkinDef { id = "snowcamo", name = "Зимний камуфляж", price = 250, tint = new Color(0.85f, 0.88f, 0.9f) },
            new SkinDef { id = "ocean", name = "Океан", price = 600, tint = new Color(0.1f, 0.4f, 0.8f) },
            new SkinDef { id = "graffiti", name = "Граффити", price = 850, tint = new Color(1f, 0.3f, 0.8f) },
            new SkinDef { id = "chrome", name = "Хром", price = 1100, tint = new Color(0.85f, 0.88f, 0.92f) },
        };

        public static readonly List<MedDef> Medkits = new List<MedDef>
        {
            new MedDef { id = "med10", name = "Малая аптечка", heal = 10, price = 40, color = new Color(1f, 0.55f, 0.55f) },
            new MedDef { id = "med50", name = "Аптечка", heal = 50, price = 180, color = new Color(0.9f, 0.2f, 0.2f) },
            new MedDef { id = "med100", name = "Большая аптечка", heal = 100, price = 400, color = new Color(0.6f, 0.05f, 0.1f) },
        };
        public static MedDef Med(string id) => Medkits.Find(m => m.id == id);

        /// <summary>Aiming zoom: snipers use their optics, everything else a red-dot sight.</summary>
        public static float AimZoom(WeaponDef w) => w.zoom > 0 ? w.zoom : w.cat == "primary" ? 1.6f : w.cat == "pistol" ? 1.35f : 1.1f;

        public static readonly List<MapDef> Maps = new List<MapDef>
        {
            new MapDef { id = "desert", name = "Пустыня", desc = "Каньон и два дома со вторым этажом" },
            new MapDef { id = "maze", name = "Лабиринт", desc = "Узкие коридоры, засады за углом" },
            new MapDef { id = "stadium", name = "Стадион", desc = "Гол мячом = +3 очка команде" },
            new MapDef { id = "forest", name = "Лес", desc = "Ёлки, заборы, аптечки у палатки" },
            new MapDef { id = "city", name = "Город", desc = "Улицы, дома и машины-укрытия" },
            new MapDef { id = "warehouse", name = "Склад", desc = "Контейнеры, можно залезть наверх" },
            new MapDef { id = "moon", name = "Лунная станция", desc = "Низкая гравитация — высокие прыжки" },
            new MapDef { id = "winter", name = "Зимняя деревня", desc = "Избы, снеговики, замёрзшее озеро" },
            new MapDef { id = "ship", name = "Пиратский корабль", desc = "Бой на палубе, за бортом — вода" },
        };

        public static readonly string[] BotNames = { "Бот Вася", "Бот Петя", "Бот Жора", "Бот Рома", "Бот Тима", "Бот Саня", "Бот Гоша", "Бот Лёха", "Бот Дима", "Бот Миша", "Бот Костя", "Бот Артём" };
        public static readonly string[] BotWeapons = { "ak74", "ak74", "m4a1", "mp5", "glock", "colt", "svd", "deagle", "uzi", "mosin" };

        public const int RewardKill = 25, RewardWin = 150, RewardDraw = 70, RewardLose = 40, RewardGoal = 30, StartCoins = 1000;

        static readonly Dictionary<string, float> catFactor = new Dictionary<string, float> { { "primary", 1.3f }, { "pistol", 0.8f }, { "melee", 1.1f }, { "throw", 0.7f }, { "grenade", 0.6f } };

        public static WeaponDef W(string id) => Weapons.Find(w => w.id == id);
        public static SkinDef S(string id) => Skins.Find(s => s.id == id) ?? Skins[0];
        public static MapDef M(string id) => Maps.Find(m => m.id == id) ?? Maps[0];
        public static int CatIndex(string cat) => System.Array.IndexOf(Cats, cat);

        /// <summary>Every weapon × skin has its own price.</summary>
        public static int SkinPrice(string weapon, string skin)
        {
            var s = S(skin);
            var w = W(weapon);
            if (s.price == 0 || w == null) return 0;
            return Mathf.RoundToInt(s.price * catFactor[w.cat] / 10f) * 10;
        }

        public static readonly Color TeamA = new Color(0.2f, 0.76f, 1f), TeamB = new Color(1f, 0.42f, 0.24f), Neutral = new Color(0.7f, 0.55f, 1f);
        public static Color TeamColor(int team) => team == 1 ? TeamA : team == 2 ? TeamB : Neutral;
        public static string TeamName(int team) => team == 1 ? "СИНИЕ" : team == 2 ? "ОРАНЖЕВЫЕ" : "";
    }
}
