using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Arena
{
    /// <summary>
    /// Self-test run by build_and_test.sh: walks through the lobby, shop, vote and all four locations with
    /// bots, writing PASS/FAIL lines and screenshots. "-mptest host|guest" runs two real copies through the relay.
    /// </summary>
    public class AutoTest : MonoBehaviour
    {
        string dir;
        readonly List<string> report = new List<string>();
        int fails;

        void Start()
        {
            dir = Boot.Arg("-shots") ?? Path.Combine(Application.persistentDataPath, "shots");
            Directory.CreateDirectory(dir);
            Application.logMessageReceived += (msg, st, type) => { if (type == LogType.Exception) Check("no exception: " + msg.Split('\n')[0], false); };
            StartCoroutine(Boot.Has("-mptest") ? Mp(Boot.Arg("-mptest")) : Run());
        }

        void Check(string what, bool ok)
        {
            report.Add((ok ? "PASS " : "FAIL ") + what);
            if (!ok) fails++;
            Debug.Log("[Test] " + (ok ? "PASS " : "FAIL ") + what);
            File.WriteAllLines(Path.Combine(dir, "autotest.txt"), report);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
            yield return null;
            yield return null;
        }

        IEnumerator Wait(float s) { float t = Time.time + s; while (Time.time < t) yield return null; }

        void Look(Vector3 pos, Vector3 at)
        {
            var p = PlayerCtl.I;
            p.Teleport(pos, 0);
            var d = at - (pos + Vector3.up * 1.6f);
            p.yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            p.pitch = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
        }

        IEnumerator Run()
        {
            var g = Game.I; var p = PlayerCtl.I;
            yield return Wait(1f);
            Check("lobby built", g.Lobby != null && g.Lobby.pads.Count == 5);
            Check("player has loadout", p.Slots.Count >= 2);
            yield return Shot("01_lobby_spawn");
            Look(new Vector3(0, 0.1f, 9), new Vector3(0, 1.5f, 28)); yield return Wait(0.3f); yield return Shot("02_portals");
            Look(new Vector3(0, 0.1f, -10), new Vector3(0, 1.5f, -20)); yield return Wait(0.3f); yield return Shot("03_ring");
            Look(new Vector3(8, 0.1f, -40), new Vector3(-4, 1.2f, -48)); yield return Wait(0.3f); yield return Shot("04_shop_room");
            Look(new Vector3(-16, 0.1f, -3), new Vector3(-16, 1.4f, 9)); yield return Wait(0.3f); yield return Shot("05_range");

            // shooting range: aim at a target and fire
            int before = RangeTarget.Hits;
            var tgt = Object.FindObjectsByType<RangeTarget>(FindObjectsSortMode.None).First(t => t.moveAmp == 0);
            Look(tgt.transform.position + new Vector3(0, 0, -7), tgt.transform.position + Vector3.up * 1.45f);
            yield return Wait(0.3f);
            p.FireTap = true; yield return Wait(0.6f);
            Check("range target hit", RangeTarget.Hits > before);

            // shop: buy weapons + skins
            Menus.Shop(); yield return Wait(0.6f); yield return Shot("06_shop");
            Window.Close();
            foreach (var id in new[] { "ak74", "svd", "rpg7", "glock", "butterfly", "kunai", "f1" })
            {
                var w = Defs.W(id);
                if (Save.Buy(w.price)) { Save.P.owned.Add(id); Save.SetSlot(w.cat, id); }
            }
            Save.P.skins.Add("ak74:golden"); Save.SetSkin("ak74", "golden");
            Save.SetSlot("primary", "ak74");
            p.BuildLoadout(false);
            Check("bought AK with golden skin", p.Slots[0].w.id == "ak74" && p.Slots[0].skin == "golden");
            Check("coins spent", Save.P.coins < 50000);
            yield return Wait(0.3f); yield return Shot("07_golden_ak");
            Menus.Shop(); yield return Wait(0.8f); yield return Shot("08_shop_owned"); Window.Close();
            foreach (var wd in Defs.Weapons) { int n = Menus.PartsOf(wd.id); Check("model " + wd.id + ": " + n + " parts", n >= 100); }
            yield return Gallery(Defs.Weapons.Select(x => (x.id, Defs.Skins[(Defs.Weapons.IndexOf(x) * 7 + 16) % Defs.Skins.Count].id)).ToList(), "11_weapons_gallery");
            yield return Gallery(Defs.Skins.Skip(15).Select(sk => ("ak74", sk.id)).Concat(Defs.Skins.Skip(1).Take(5).Select(sk => ("deagle", sk.id))).ToList(), "12_skins_gallery");
            Menus.Shop(); yield return Wait(0.8f);
            var sv = UIRoot.Open != null ? UIRoot.Open.GetComponentInChildren<UnityEngine.UI.ScrollRect>() : null;
            if (sv != null) { sv.verticalNormalizedPosition = 0; yield return Wait(0.3f); }
            yield return Shot("13_shop_skin_tiles"); Window.Close();
            // SVD optics + "УБРАТЬ ПРИЦЕЛ"
            Save.SetSlot("primary", "svd"); p.BuildLoadout(false);
            Look(new Vector3(0, 0.1f, 12), new Vector3(0, 1.5f, 28));
            p.ZoomToggle = true; yield return Wait(0.6f);
            Check("svd scope on", p.zoomed && p.AimOn);
            yield return Shot("14_svd_scope_button");
            p.AimOff(); yield return Wait(0.3f);
            Check("УБРАТЬ ПРИЦЕЛ turns the scope off", !p.zoomed && !p.AimOn);
            Save.SetSlot("primary", "ak74"); p.BuildLoadout(false);
            Menus.ShopSection("med"); yield return Wait(0.8f); yield return Shot("09_shop_medkits"); Window.Close();

            // vote → match on each map
            foreach (var map in new[] { "desert", "maze", "stadium", "forest" })
            {
                yield return Match(map, map == "desert" ? 2 : 1);
            }
            Check("TOTAL fails=" + fails, fails == 0);
            yield return Wait(0.5f);
            Application.Quit();
        }

        /// <summary>Lays out weapons (with skins) on a wall-like grid in front of the camera and takes a screenshot.</summary>
        IEnumerator Gallery(List<(string id, string skin)> items, string shot)
        {
            var p = PlayerCtl.I;
            Look(new Vector3(0, 0.1f, -10.6f), new Vector3(0, 1.6f, -16));
            var root = new GameObject("Gallery");
            int cols = 6;
            int rowsN = (items.Count + cols - 1) / cols;
            for (int i = 0; i < items.Count; i++)
            {
                var holder = new GameObject("H").transform;
                holder.SetParent(root.transform, false);
                var m = Models.Weapon(items[i].id, items[i].skin, holder, 0);
                var b = new Bounds(); bool first = true;
                foreach (var r in m.GetComponentsInChildren<Renderer>()) { if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
                float k = 0.7f / Mathf.Max(0.05f, Mathf.Max(b.size.x, b.size.y, b.size.z));
                m.transform.localScale = Vector3.one * k;
                m.transform.position -= b.center * k;
                holder.position = new Vector3(-1.9f + (i % cols) * 0.76f, 1.6f + (rowsN - 1) * 0.24f - (i / cols) * 0.48f, -12.5f);
                holder.rotation = Quaternion.Euler(0, -90, 0) * Quaternion.Euler(-15, 0, 0);
                if (items[i].id == "shuriken" || items[i].id.StartsWith("f1") || items[i].id == "rgd5") holder.rotation = Quaternion.Euler(-60, 20, 0);
            }
            var light = new GameObject("L").AddComponent<Light>();
            light.type = LightType.Point; light.range = 10; light.intensity = 6; light.transform.position = new Vector3(0, 2.5f, -10.5f);
            p.Slots.ForEach(_ => { });
            yield return Wait(0.4f);
            yield return Shot(shot);
            Destroy(root); Destroy(light.gameObject);
        }

        IEnumerator Match(string map, int mode)
        {
            var g = Game.I; var p = PlayerCtl.I;
            g.Queue(mode);
            yield return Wait(0.5f);
            Check(map + ": vote opened", g.V != null && Menus.OverlayOpen);
            if (map == "desert") yield return Shot("10_vote");
            g.Vote(map);
            yield return Wait(4f);
            Check(map + ": match started", g.M != null && g.M.map == map);
            if (g.M == null) yield break;
            Check(map + ": bots spawned " + g.Bots.Count, g.Bots.Count == mode * 2 - 1);
            yield return Wait(4.5f);
            Check(map + ": fight state", g.M.state == 1);
            p.invulnUntil = Time.time + 1000;
            yield return Shot("20_" + map + "_spawn");
            // overview from a high point
            var b = g.Cur.bounds;
            p.Teleport(new Vector3(b.center.x, 0.1f, b.center.y), 0);
            yield return Wait(4f);
            // hunt one bot: stand next to it and shoot until it dies
            var bot = g.Bots.FirstOrDefault(x => x.team != p.team && x.alive);
            int kills0 = g.M.stats[Save.P.id].x;
            for (int i = 0; i < 40 && bot != null && g.M.stats[Save.P.id].x == kills0; i++)
            {
                if (!bot.alive) break;
                if (map == "maze") { bot.cc.enabled = false; bot.av.transform.position = new Vector3(0, 0.05f, 2f); bot.cc.enabled = true; }
                var bp = bot.Pos;
                Look(bp + new Vector3(0, 0, -3.2f), bp + Vector3.up * 1.2f);
                p.Equip(0, true);
                p.Slots[0].next = 0; p.Slots[0].ammo = Mathf.Max(p.Slots[0].ammo, 5); p.Slots[0].reloadEnd = 0;
                p.FireTap = true;
                p.invulnUntil = Time.time + 10;
                yield return null; yield return null;
                if (i == 2) yield return Shot("21_" + map + "_fight");
            }
            yield return Wait(0.5f);
            Check(map + ": killed a bot (kills " + g.M.stats[Save.P.id].x + ")", g.M.stats[Save.P.id].x > kills0);
            Check(map + ": score counted", g.M.score[p.team] > 0);
            if (map == "stadium")
            {
                var ball = g.Cur.ball;
                ball.transform.position = new Vector3(30, Ball.R, 0); ball.vel = new Vector3(20, 0, 0);
                int s0 = g.M.score[1];
                yield return Wait(1.5f);
                Check("stadium: goal into east goal gives team A +3", g.M.score[1] == s0 + 3);
            }
            if (map == "maze")
            {
                // shop medkit: buy +50, use it in the fight
                int c0 = Save.P.coins;
                Save.Buy(Defs.Med("med50").price); Save.AddMeds("med50", 1);
                p.hp = 30;
                bool used = p.UseMedkit("med50");
                Check("maze: bought medkit +50 and healed 30→" + p.hp, used && Mathf.Approximately(p.hp, 80) && Save.Meds("med50") == 0 && Save.P.coins == c0 - 180);
                // sight on → aim at the bot, no trigger pull: it must shoot by itself
                for (int i = 0; i < 300 && !g.Bots.Any(x => x.team != p.team && x.alive); i++) yield return null;
                var bt = g.Bots.FirstOrDefault(x => x.team != p.team && x.alive);
                int k0 = g.M.stats[Save.P.id].x;
                p.ZoomToggle = true;
                for (int i = 0; i < 60 && bt != null && g.M.stats[Save.P.id].x == k0; i++)
                {
                    if (!bt.alive) break;
                    bt.cc.enabled = false; bt.av.transform.position = new Vector3(0, 0.05f, 2f); bt.cc.enabled = true;
                    Look(new Vector3(0, 0.1f, -2.5f), new Vector3(0, 1.2f, 2f));
                    p.Slots[0].ammo = Mathf.Max(p.Slots[0].ammo, 5); p.Slots[0].reloadEnd = 0;
                    yield return null; yield return null;
                    if (i == 3) yield return Shot("25_maze_sight");
                }
                Check("maze: sight auto-fires at the enemy (kills " + g.M.stats[Save.P.id].x + ")", g.M.stats[Save.P.id].x > k0);
                p.ZoomToggle = true;
                yield return null;
            }
            if (map == "forest")
            {
                p.hp = 40;
                var pk = g.Cur.pickups[0];
                p.Teleport(pk.pos + new Vector3(0, 0.1f, 0), 0);
                yield return Wait(1f);
                Check("forest: medkit heals", p.hp > 40 && !pk.active);
            }
            if (map == "desert")
            {
                // rocket + grenade + throwing knife against a bot
                int k1 = g.M.stats[Save.P.id].x;
                Save.SetSlot("primary", "rpg7"); p.BuildLoadout(false);
                var rpgIdx = p.Slots.FindIndex(s => s.w.id == "rpg7");
                var bt = g.Bots.FirstOrDefault(x => x.team != p.team && x.alive);
                if (rpgIdx >= 0 && bt != null)
                {
                    Look(bt.Pos + new Vector3(0, 0, -8), bt.Pos + Vector3.up * 0.8f);
                    p.Equip(rpgIdx, true); p.Slots[rpgIdx].next = 0;
                    p.FireTap = true;
                    yield return Wait(0.2f);
                    yield return Shot("22_rocket");
                    yield return Wait(1f);
                }
                Save.SetSlot("primary", "ak74"); p.BuildLoadout(false);
                Check("desert: rocket kills/damages (kills " + g.M.stats[Save.P.id].x + ")", g.M.stats[Save.P.id].x > k1 || (bt != null && bt.hp < 100));
                // climb the stairs of the building
                p.Teleport(new Vector3(-6.8f, 0.1f, -13.2f), 180);
                for (int i = 0; i < 90; i++) { p.MoveInput = new Vector2(0, 1); yield return null; }
                p.MoveInput = Vector2.zero;
                Check("desert: stairs climbable (y=" + p.Pos.y.ToString("0.0") + ")", p.Pos.y > 1.5f);
                yield return Shot("23_desert_stairs");
            }
            p.invulnUntil = 0;
            g.LeaveMatch();
            yield return Wait(0.8f);
            Check(map + ": end screen", Menus.OverlayOpen);
            if (map == "desert") yield return Shot("24_end");
            Menus.CloseEnd(); g.ToLobby();
            yield return Wait(0.8f);
            Check(map + ": back in lobby", g.M == null && g.Cur == g.Lobby);
        }

        // ------------------------------------------------------------ two real players through the relay

        IEnumerator Mp(string role)
        {
            var g = Game.I; var p = PlayerCtl.I; var n = Net.I;
            string hostId = Boot.Arg("-room") ?? "TSTHST";
            yield return Wait(2f);
            if (role == "guest")
            {
                n.JoinRoom(hostId);
                float t0 = Time.time;
                while (!n.HostSeen && Time.time - t0 < 25) yield return null;
                Check("guest: sees host", n.HostSeen);
                yield return Wait(2f);
                Check("guest: host avatar visible", g.Remotes.ContainsKey(hostId));
                yield return Shot("mp_guest_lobby");
                t0 = Time.time;
                while (g.V == null && Time.time - t0 < 40) yield return null;
                Check("guest: vote screen appeared", g.V != null);
                yield return Shot("mp_guest_vote");
                g.Vote("forest");
                t0 = Time.time;
                while (g.M == null && Time.time - t0 < 30) yield return null;
                Check("guest: match started on " + g.M?.map, g.M != null && g.M.map == "forest");
                if (g.M != null) Check("guest: in opposite team", g.M.Team(Save.P.id) != g.M.Team(hostId));
                yield return Wait(6f);
                yield return Shot("mp_guest_match");
                Check("guest: host visible in match", g.Remotes.TryGetValue(hostId, out var h) && h.Scene == g.SceneId);
            }
            else
            {
                float t0 = Time.time;
                while (n.Members.Count < 2 && Time.time - t0 < 40) yield return null;
                Check("host: guest joined", n.Members.Count >= 2);
                yield return Wait(3f);
                yield return Shot("mp_host_lobby");
                g.Queue(1);
                yield return Wait(1.5f);
                g.Vote("forest");
                yield return Shot("mp_host_vote");
                t0 = Time.time;
                while (g.M == null && Time.time - t0 < 30) yield return null;
                Check("host: match started", g.M != null && g.M.map == "forest");
                Check("host: no bots in 1v1 with a friend", g.Bots.Count == 0);
                yield return Wait(6f);
                yield return Shot("mp_host_match");
                var guest = g.Remotes.Keys.FirstOrDefault();
                Check("host: guest visible in match", guest != null && g.Remotes[guest].Scene == g.SceneId);
                // shoot the guest
                if (guest != null)
                {
                    var gp = g.Remotes[guest].transform.position;
                    for (int i = 0; i < 30 && g.M.score[p.team] == 0; i++)
                    {
                        gp = g.Remotes[guest].transform.position;
                        Look(gp + new Vector3(0, 0, -3), gp + Vector3.up * 1.2f);
                        p.Slots[0].next = 0; p.Slots[0].ammo = 8; p.FireTap = true;
                        yield return Wait(0.35f);
                    }
                    yield return Wait(2f);
                    Check("host: killed the guest through the network (score " + g.M.score[p.team] + ")", g.M.score[p.team] > 0);
                }
            }
            yield return Wait(4f);
            Check("TOTAL fails=" + fails, fails == 0);
            Application.Quit();
        }
    }
}
