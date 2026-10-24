using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arena
{
    [Serializable]
    public class Profile
    {
        public string id = "";
        public string name = "";
        public int coins = Defs.StartCoins;
        public List<string> owned = new List<string> { "tt", "kabar" };
        public List<string> skins = new List<string>();          // "weapon:skin"
        public List<string> equip = new List<string>();          // "weapon:skin" currently worn
        public string primary = "", pistol = "tt", melee = "kabar", thrown = "", grenade = "";
        public float sens = 1f, volume = 0.8f;
        public int quality = -1;
        public int kills, deaths, wins, matches;
        public int med10, med50, med100;
        public string lastRoom = "";
    }

    /// <summary>Local profile: coins, bought weapons and skins, loadout, settings (JSON in persistentDataPath).</summary>
    public static class Save
    {
        public static Profile P = new Profile();
        public static string FileName = "neon_arena.json";
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static void Load()
        {
            try { if (File.Exists(Path)) P = JsonUtility.FromJson<Profile>(File.ReadAllText(Path)) ?? new Profile(); }
            catch (Exception e) { Debug.LogWarning("[Save] " + e.Message); P = new Profile(); }
            if (string.IsNullOrEmpty(P.id)) P.id = NewId();
        }

        public static string NewId()
        {
            const string abc = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var r = new System.Random(Guid.NewGuid().GetHashCode());
            var s = new char[6];
            for (int i = 0; i < 6; i++) s[i] = abc[r.Next(abc.Length)];
            return new string(s);
        }

        public static void Write()
        {
            if (Boot.Testing && !Boot.Has("-keepsave")) return;
            try { File.WriteAllText(Path, JsonUtility.ToJson(P)); WebBridge.SyncFiles(); }
            catch (Exception e) { Debug.LogWarning("[Save] " + e.Message); }
        }

        public static bool Owns(string w) => P.owned.Contains(w);
        public static bool OwnsSkin(string w, string s) => s == "std" || P.skins.Contains(w + ":" + s);

        public static string SkinOf(string w)
        {
            foreach (var e in P.equip) if (e.StartsWith(w + ":")) return e.Substring(w.Length + 1);
            return "std";
        }

        public static void SetSkin(string w, string s)
        {
            P.equip.RemoveAll(e => e.StartsWith(w + ":"));
            if (s != "std") P.equip.Add(w + ":" + s);
            Write();
        }

        public static string Slot(string cat) => cat switch
        {
            "primary" => P.primary, "pistol" => P.pistol, "melee" => P.melee, "throw" => P.thrown, _ => P.grenade,
        };

        public static void SetSlot(string cat, string w)
        {
            switch (cat)
            {
                case "primary": P.primary = w; break;
                case "pistol": P.pistol = w; break;
                case "melee": P.melee = w; break;
                case "throw": P.thrown = w; break;
                default: P.grenade = w; break;
            }
            Write();
        }

        public static int Meds(string id) => id == "med10" ? P.med10 : id == "med50" ? P.med50 : P.med100;

        public static void AddMeds(string id, int n)
        {
            if (id == "med10") P.med10 += n; else if (id == "med50") P.med50 += n; else P.med100 += n;
            Write();
        }

        public static bool Buy(int price)
        {
            if (P.coins < price) return false;
            P.coins -= price;
            Write();
            return true;
        }
    }
}
