using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Arena
{
    public struct Combatant { public string id; public int team; public Vector3 pos; public bool alive; }

    public class Match
    {
        public string map;
        public int mode, seed, target;
        public float duration;
        public readonly Dictionary<string, int> teams = new Dictionary<string, int>();
        public readonly Dictionary<string, string> names = new Dictionary<string, string>();
        public readonly Dictionary<string, Vector2Int> stats = new Dictionary<string, Vector2Int>();   // kills, deaths
        public readonly List<string[]> botDefs = new List<string[]>();                                // id, name, team, weapon, skin
        public int[] score = new int[3];
        public int state;          // 0 countdown, 1 fight, 2 finished
        public float playAt, endAt;
        public int earned, kills, deaths, goals;
        public int Team(string id) => teams.TryGetValue(id, out var t) ? t : 0;
    }

    public class VoteView
    {
        public int mode, teamMode;
        public float endAt;
        public readonly Dictionary<string, string> votes = new Dictionary<string, string>();
        public string result;
    }

    /// <summary>
    /// The game flow: lobby → portal → location vote on every party member's screen → match → lobby.
    /// The room owner (host) decides the vote, runs bots / ball / medkits and keeps the score.
    /// </summary>
    public class Game : MonoBehaviour
    {
        public static Game I;
        public Map Lobby, Cur;
        public NavGrid Nav;
        public string SceneId = "lobby";
        public Match M;
        public VoteView V;
        VoteView hv;   // host's own vote state
        public readonly Dictionary<string, Avatar> Remotes = new Dictionary<string, Avatar>();
        readonly Dictionary<string, Avatar> botViews = new Dictionary<string, Avatar>();
        public readonly List<Bot> Bots = new List<Bot>();
        readonly List<Proj> projs = new List<Proj>();
        Transform actors, fx;
        bool shadows;
        float sendT, botSendT, ballSendT, kickT, lobbyPadCooldown;
        int lastPad = -1, lastCount = -1, pidSeq;
        public bool InShopZone;

        PlayerCtl P => PlayerCtl.I;
        Net N => Net.I;
        string Me => Save.P.id;
        public bool CanAct => M == null || M.state == 1;
        public bool IsHost => N.IsHost;

        public static void Create(bool shadows)
        {
            var go = new GameObject("Game");
            I = go.AddComponent<Game>();
            I.shadows = shadows;
            I.actors = new GameObject("Actors").transform;
            I.fx = new GameObject("Fx").transform;
            I.Lobby = Maps.Build("lobby", 1, null, shadows);
            I.Cur = I.Lobby;
            I.Lobby.Activate(PlayerCtl.I.Cam);
            Net.I.OnMessage += I.OnNet;
            Net.I.OnMemberLeft += I.MemberLeft;
        }

        public void EnterLobby()
        {
            P.team = 0;
            P.BuildLoadout(true);
            P.Spawn(Lobby.L[Random.Range(0, Lobby.L.Count)]);
        }

        // ================================================================== frame

        void Update()
        {
            float dt = Time.deltaTime;
            float now = Time.time;
            Cur?.tick?.Invoke(dt);

            if (M != null)
            {
                if (M.state == 0)
                {
                    int left = Mathf.CeilToInt(M.playAt - now);
                    if (left != lastCount) { lastCount = left; if (left > 0) { Hud.Center(left.ToString(), 0.9f, UIKit.Gold); AudioFx.Play("beep"); } }
                    if (now >= M.playAt) { M.state = 1; Hud.Center("В БОЙ!", 1.2f, UIKit.Lime); AudioFx.Play("go"); }
                }
                if (!P.alive && now >= P.respawnAt && M.state < 2) { P.Spawn(SpawnFor(P.team)); }
                if (IsHost) HostMatchTick(dt, now);
                MatchChecks(now);
            }
            else LobbyChecks(now);

            if (IsHost && hv != null && hv.result == null && now >= hv.endAt) HostFinishVote();
            if (!IsHost && !N.HostSeen && Time.unscaledTime - N.JoinedAt > 7f)
            {
                Hud.Toast("Отряд " + N.Room + " не найден. Проверь код.", UIKit.Red);
                N.LeaveRoom(); Menus.RefreshParty();
            }
            if (!IsHost && N.HostSeen && !N.Members.ContainsKey(N.Room))
            {
                Hud.Toast("Лидер отряда вышел", UIKit.Red);
                N.LeaveRoom(); Menus.RefreshParty();
                if (M != null) ToLobby();
                CloseVote();
            }

            UpdateProjectiles(dt);
            foreach (var kv in Remotes) kv.Value.Visible = kv.Value.Scene == SceneId;

            sendT += Time.unscaledDeltaTime;
            if (sendT >= 0.1f) { sendT = 0; SendState(); }
        }

        void SendState()
        {
            var p = P.Pos;
            var s = P.Cur;
            N.Send("S", SceneId, Net.F(p.x), Net.F(p.y), Net.F(p.z), Net.F(P.yaw), Net.F(P.pitch), s?.w.id ?? "", s?.skin ?? "std",
                ((int)P.hp).ToString(), P.alive ? "1" : "0", P.team.ToString(), Net.Name(Save.P.name));
        }

        public void FellOut()
        {
            if (M != null) P.Spawn(SpawnFor(P.team));
            else P.Spawn(Lobby.L[0]);
        }

        public Spawn SpawnFor(int team)
        {
            var list = team == 2 ? Cur.B : team == 1 ? Cur.A : Cur.L;
            if (list.Count == 0) list = Cur.L.Count > 0 ? Cur.L : Lobby.L;
            return list[Random.Range(0, list.Count)];
        }

        // ================================================================== lobby

        void LobbyChecks(float now)
        {
            var p = P.Pos;
            var xz = new Vector2(p.x, p.z);
            InShopZone = Lobby.shop.Contains(xz);
            int pad = -1;
            foreach (var (mode, area) in Lobby.pads) if (area.Contains(xz)) pad = mode;
            if (pad != lastPad && pad > 0 && V == null && now > lobbyPadCooldown) { Queue(pad); lobbyPadCooldown = now + 2; }
            lastPad = pad;
            if (InShopZone && !Application.isMobilePlatform && Input.GetKeyDown(KeyCode.E) && !P.InputBlocked) Menus.Shop();
        }

        public void Queue(int mode)
        {
            AudioFx.Play("portal");
            if (IsHost) HostQueue(mode, Me);
            else N.SendR("Q", mode.ToString());
        }

        void HostQueue(int mode, string from)
        {
            if (hv != null || M != null) return;
            hv = new VoteView { mode = mode, endAt = Time.time + 15f };
            HostAll("VS", mode.ToString(), "15", "0");
        }

        public void Vote(string map)
        {
            if (V == null || V.result != null) return;
            V.votes[Me] = map;
            AudioFx.Play("click");
            if (IsHost) HostVote(Me, map); else N.SendR("VT", map);
            Menus.UpdateVote();
        }

        void HostVote(string id, string map)
        {
            if (hv == null || hv.result != null) return;
            hv.votes[id] = map;
            BroadcastVotes();
            if (N.Members.Keys.All(m => hv.votes.ContainsKey(m))) hv.endAt = Mathf.Min(hv.endAt, Time.time + 1.2f);
        }

        public void SetTeamMode(int tm)
        {
            if (!IsHost || hv == null) return;
            hv.teamMode = tm;
            BroadcastVotes();
        }

        void BroadcastVotes()
        {
            var sb = new StringBuilder();
            foreach (var kv in hv.votes) sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
            HostAll("VU", hv.teamMode.ToString(), sb.ToString());
        }

        public void CancelVote()
        {
            if (IsHost && hv != null && hv.result == null) { hv = null; HostAll("VC"); }
        }

        void HostFinishVote()
        {
            var counts = new Dictionary<string, int>();
            foreach (var v in hv.votes.Values) counts[v] = (counts.TryGetValue(v, out var c) ? c : 0) + 1;
            string map;
            if (counts.Count == 0) map = Defs.Maps[Random.Range(0, Defs.Maps.Count)].id;
            else
            {
                int best = counts.Values.Max();
                var top = counts.Where(kv => kv.Value == best).Select(kv => kv.Key).ToList();
                map = top[Random.Range(0, top.Count)];
            }
            hv.result = map;
            HostAll("VR", map);
            Invoke(nameof(HostStartMatch), 2f);
        }

        void HostStartMatch()
        {
            if (hv == null) return;
            var v = hv;
            int mode = v.mode;
            var humans = N.Members.Values.OrderBy(m => m.id == Me ? 0 : 1).ThenBy(m => m.id).ToList();
            var teams = new List<(string id, int team, string name)>();
            int ca = 0, cb = 0;
            for (int i = 0; i < humans.Count; i++)
            {
                int t;
                if (v.teamMode == 1) t = ca < mode ? 1 : 2;
                else t = i % 2 == 0 ? 1 : 2;
                if (t == 1) ca++; else cb++;
                teams.Add((humans[i].id, t, humans[i].name ?? humans[i].id));
            }
            int size = Mathf.Max(mode, Mathf.Max(ca, cb));
            var sbT = new StringBuilder();
            foreach (var t in teams) sbT.Append(t.id).Append(':').Append(t.team).Append(':').Append(Net.Name(t.name)).Append(';');
            var sbB = new StringBuilder();
            var names = Defs.BotNames.OrderBy(_ => Random.value).ToList();
            int bi = 0;
            for (int team = 1; team <= 2; team++)
                for (int n = team == 1 ? ca : cb; n < size; n++, bi++)
                {
                    var w = Defs.BotWeapons[Random.Range(0, Defs.BotWeapons.Length)];
                    var sk = Defs.Skins[Random.Range(0, Defs.Skins.Count)].id;
                    sbB.Append("bot").Append(bi).Append(':').Append(names[bi % names.Count]).Append(':').Append(team).Append(':').Append(w).Append(':').Append(sk).Append(';');
                }
            int target = 10 + 5 * (mode - 1);
            int seed = Random.Range(1, 999999);
            HostAll("GO", v.result, mode.ToString(), seed.ToString(), target.ToString(), "240", sbT.ToString(), sbB.ToString());
            hv = null;
        }

        public void CloseVote()
        {
            V = null;
            Menus.CloseVote();
        }

        // ================================================================== match start / end

        void StartMatch(string[] a)
        {
            var m = new Match { map = a[0], mode = Net.PI(a[1]), seed = Net.PI(a[2]), target = Net.PI(a[3]), duration = Net.PF(a[4]) };
            foreach (var e in a[5].Split(';'))
            {
                var p = e.Split(':');
                if (p.Length < 3) continue;
                m.teams[p[0]] = Net.PI(p[1]); m.names[p[0]] = p[2]; m.stats[p[0]] = Vector2Int.zero;
            }
            foreach (var e in a[6].Split(';'))
            {
                var p = e.Split(':');
                if (p.Length < 5) continue;
                m.botDefs.Add(p);
                m.teams[p[0]] = Net.PI(p[2]); m.names[p[0]] = p[1]; m.stats[p[0]] = Vector2Int.zero;
            }
            CloseVote();
            if (!m.teams.ContainsKey(Me)) { Hud.Toast("Отряд начал бой без тебя — ты был не в лобби", UIKit.Gold); return; }
            if (M != null) ToLobby();
            Menus.CloseAll();
            M = m;
            SceneId = "m" + m.seed;
            Lobby.root.SetActive(false);
            Cur = Maps.Build(m.map, m.seed, null, shadows);
            Cur.Activate(P.Cam);
            Nav = IsHost ? NavGrid.Build(Cur) : null;
            if (Cur.ball != null) Cur.ball.simulate = IsHost;
            m.playAt = Time.time + 4f;
            m.endAt = m.playAt + m.duration;
            lastCount = -1;
            P.team = m.Team(Me);
            P.BuildLoadout(true);
            P.Spawn(SpawnFor(P.team));
            foreach (var d in m.botDefs)
            {
                if (IsHost)
                {
                    var b = Bot.Create(d[0], d[1], Net.PI(d[2]), d[3], d[4], actors);
                    b.Spawn(SpawnFor(b.team));
                    Bots.Add(b);
                }
                else
                {
                    var av = Avatar.Create(d[0], d[1], actors);
                    av.SetTeam(Net.PI(d[2])); av.SetWeapon(d[3], d[4]);
                    av.Scene = SceneId;
                    botViews[d[0]] = av;
                }
            }
            Hud.MatchStarted();
            Hud.Toast(Defs.M(m.map).name + " · " + m.mode + " VS " + m.mode + " · ты в команде " + Defs.TeamName(P.team), Defs.TeamColor(P.team));
            Menus.LockCursor();
        }

        public void ToLobby()
        {
            if (M != null && M.state < 2 && M.earned > 0) { Save.P.coins += M.earned; Save.Write(); }
            M = null;
            foreach (var b in Bots) Destroy(b.av.gameObject);
            Bots.Clear();
            foreach (var kv in botViews) Destroy(kv.Value.gameObject);
            botViews.Clear();
            foreach (var p in projs) if (p.go != null) Destroy(p.go);
            projs.Clear();
            for (int i = fx.childCount - 1; i >= 0; i--) Destroy(fx.GetChild(i).gameObject);
            if (Cur != null && Cur != Lobby) Cur.Destroy();
            Cur = Lobby;
            Nav = null;
            Lobby.root.SetActive(true);
            Lobby.Activate(P.Cam);
            SceneId = "lobby";
            hv = null;
            EnterLobby();
            Hud.MatchStarted();
            Menus.CloseEnd();
        }

        /// <summary>Pause menu "leave": the host ends the match for everyone.</summary>
        public void LeaveMatch()
        {
            if (M == null) return;
            if (IsHost && M.state < 2) HostEnd(true);
            else ToLobby();
        }

        void HostMatchTick(float dt, float now)
        {
            if (M.state == 1) foreach (var b in Bots) b.Tick(this, dt);
            botSendT += dt;
            if (botSendT >= 0.1f && Bots.Count > 0)
            {
                botSendT = 0;
                var sb = new StringBuilder();
                foreach (var b in Bots)
                {
                    var p = b.Pos;
                    sb.Append(b.id).Append(',').Append(Net.F(p.x)).Append(',').Append(Net.F(p.y)).Append(',').Append(Net.F(p.z)).Append(',')
                      .Append(Net.F(b.yaw)).Append(',').Append(Net.F(b.pitch)).Append(',').Append(b.alive ? 1 : 0).Append(';');
                }
                N.Send("B", sb.ToString());
            }
            var ball = Cur.ball;
            if (ball != null)
            {
                foreach (var b in Bots)
                {
                    if (!b.alive) continue;
                    var d = ball.transform.position - b.Pos; d.y = 0;
                    if (d.magnitude < Ball.R + 0.5f) ball.Kick(d.normalized * 7 * Time.deltaTime * 8);
                }
                ballSendT += dt;
                if (ballSendT >= 0.1f) { ballSendT = 0; var bp = ball.transform.position; N.Send("BL", Net.F(bp.x), Net.F(bp.z), Net.F(ball.vel.x), Net.F(ball.vel.z)); }
                var pos = ball.transform.position;
                if (M.state == 1 && Mathf.Abs(pos.x) > 34 + Ball.R * 0.6f && Mathf.Abs(pos.z) < 3.66f)
                {
                    int team = pos.x < 0 ? 2 : 1;
                    M.score[team] += 3;
                    ball.Reset();
                    HostAll("GL", team.ToString(), M.score[1].ToString(), M.score[2].ToString());
                    CheckWin();
                }
            }
            foreach (var pk in Cur.pickups)
                if (!pk.active && now >= pk.respawnAt) { pk.active = true; HostAll("PKS", pk.id.ToString(), "1", ""); }
            if (M.state == 1 && now >= M.endAt) HostEnd(false);
        }

        void MatchChecks(float now)
        {
            var ball = Cur.ball;
            if (ball != null && P.alive)
            {
                var d = ball.transform.position - P.Pos; d.y = 0;
                if (d.magnitude < Ball.R + 0.55f && now > kickT)
                {
                    kickT = now + 0.2f;
                    float sp = Mathf.Max(6, new Vector2(P.vel.x, P.vel.z).magnitude * 1.5f);
                    KickBall(d.normalized * sp);
                }
            }
            if (P.alive && P.hp < 100)
                foreach (var pk in Cur.pickups)
                    if (pk.active && Vector3.Distance(new Vector3(P.Pos.x, 0, P.Pos.z), pk.pos) < 1.3f && now > kickT)
                    {
                        kickT = now + 0.5f;
                        if (IsHost) HostPick(pk.id, Me); else N.SendR("PK", pk.id.ToString());
                    }
        }

        void HostPick(int id, string by)
        {
            var pk = Cur.pickups.Find(x => x.id == id);
            if (pk == null || !pk.active) return;
            pk.active = false; pk.respawnAt = Time.time + 20;
            HostAll("PKS", id.ToString(), "0", by);
        }

        public void KickBall(Vector3 v)
        {
            if (Cur.ball == null) return;
            if (IsHost) Cur.ball.Kick(v);
            else { N.Send("KK", Net.F(v.x), Net.F(v.z)); Cur.ball.netVel += new Vector3(v.x, 0, v.z) * 0.5f; }
        }

        void CheckWin()
        {
            if (M.score[1] >= M.target || M.score[2] >= M.target) HostEnd(false);
        }

        void HostEnd(bool abort)
        {
            if (M == null || M.state == 2) return;
            int w = M.score[1] > M.score[2] ? 1 : M.score[2] > M.score[1] ? 2 : 0;
            HostAll("END", w.ToString(), M.score[1].ToString(), M.score[2].ToString(), StatsStr(), abort ? "1" : "0");
        }

        string StatsStr()
        {
            var sb = new StringBuilder();
            foreach (var kv in M.stats) sb.Append(kv.Key).Append(':').Append(kv.Value.x).Append(':').Append(kv.Value.y).Append(';');
            return sb.ToString();
        }

        void ReadStats(string s)
        {
            foreach (var e in s.Split(';'))
            {
                var p = e.Split(':');
                if (p.Length >= 3) M.stats[p[0]] = new Vector2Int(Net.PI(p[1]), Net.PI(p[2]));
            }
        }

        // ================================================================== combat

        public bool IsEnemy(string id)
        {
            if (M == null) return false;
            return M.Team(id) != P.team && M.Team(id) != 0;
        }

        public IEnumerable<Combatant> Combatants()
        {
            if (M == null) yield break;
            yield return new Combatant { id = Me, team = P.team, pos = P.Pos, alive = P.alive };
            foreach (var kv in Remotes)
                if (kv.Value.Scene == SceneId) yield return new Combatant { id = kv.Key, team = M.Team(kv.Key), pos = kv.Value.transform.position, alive = kv.Value.Alive };
            foreach (var b in Bots) yield return new Combatant { id = b.id, team = b.team, pos = b.Pos, alive = b.alive };
            foreach (var kv in botViews) yield return new Combatant { id = kv.Key, team = M.Team(kv.Key), pos = kv.Value.transform.position, alive = kv.Value.Alive };
        }

        public Combatant? Find(string id)
        {
            foreach (var c in Combatants()) if (c.id == id) return c;
            return null;
        }

        public bool BallGoalFor(int team, out Vector3 goal)
        {
            goal = default;
            if (Cur?.ball == null) return false;
            goal = Cur.ball.transform.position + new Vector3(team == 1 ? -2 : 2, 0, 0);
            return true;
        }

        /// <summary>The local player hit someone.</summary>
        public void DealDamage(string target, float dmg, string weapon, bool head)
        {
            Hud.HitMarker(head);
            AudioFx.Play(head ? "head" : "hit");
            if (IsHost && IsBot(target)) BotDamage(target, dmg, Me, weapon);
            else N.SendR("HIT", target, Net.F(dmg), weapon, head ? "1" : "0", Me);
        }

        bool IsBot(string id) => id.StartsWith("bot");

        void BotDamage(string id, float dmg, string by, string weapon)
        {
            var b = Bots.Find(x => x.id == id);
            if (b == null || !b.alive || M == null || M.state != 1) return;
            b.hp -= dmg;
            b.Hurt(by);
            if (b.hp <= 0)
            {
                b.alive = false;
                b.respawnAt = Time.time + 3.5f;
                b.cc.enabled = false;
                b.av.SetState(b.Pos, b.yaw, 0, false, 0);
                HostDeath(id, by, weapon);
            }
        }

        public void BotShoot(Bot b, Combatant t, float dist)
        {
            var w = Defs.W(b.weapon);
            float chance = Mathf.Clamp(0.62f - dist * 0.009f + (w.zoom > 0 ? 0.12f : 0), 0.12f, 0.72f);
            if (t.id == Me && P.Moving) chance *= 0.8f;
            bool hit = Random.value < chance;
            bool head = hit && Random.value < 0.12f;
            var chest = t.pos + Vector3.up * (head ? 1.62f : 1.15f);
            var end = hit ? chest : chest + Random.insideUnitSphere * 1.6f;
            var from = b.av.MuzzlePos();
            if (Physics.Linecast(from, end, out var wh, Layers.SolidMask, QueryTriggerInteraction.Ignore)) { end = wh.point; hit = false; }
            b.av.MuzzleFlash();
            Tracer(from, end, false);
            AudioFx.At(w.sound, from, 0.9f);
            N.Send("F", Net.F(from.x), Net.F(from.y), Net.F(from.z), Net.F(end.x), Net.F(end.y), Net.F(end.z), b.weapon, b.id);
            if (!hit) return;
            float dmg = w.dmg * 0.7f * (head ? 2 : 1);
            if (t.id == Me) P.TakeDamage(dmg, b.id, b.weapon);
            else if (IsBot(t.id)) BotDamage(t.id, dmg, b.id, b.weapon);
            else N.SendR("HIT", t.id, Net.F(dmg), b.weapon, head ? "1" : "0", b.id);
        }

        public void LocalDied(string by, string weapon)
        {
            if (M == null) return;
            M.deaths++;
            Hud.Center("Тебя победил " + NameOf(by), 3f, UIKit.Red);
            if (IsHost) HostDeath(Me, by, weapon);
            else N.SendR("DIE", Me, by, weapon);
        }

        void HostDeath(string victim, string killer, string weapon)
        {
            if (M == null || M.state == 2) return;
            if (M.stats.TryGetValue(victim, out var sv)) M.stats[victim] = new Vector2Int(sv.x, sv.y + 1);
            int tk = M.Team(killer), tv = M.Team(victim);
            if (killer != victim && tk != 0 && tk != tv)
            {
                M.score[tk]++;
                if (M.stats.TryGetValue(killer, out var sk)) M.stats[killer] = new Vector2Int(sk.x + 1, sk.y);
            }
            HostAll("K", victim, killer, weapon, M.score[1].ToString(), M.score[2].ToString(), StatsStr());
            CheckWin();
        }

        public string NameOf(string id)
        {
            if (id == Me) return Save.P.name;
            if (M != null && M.names.TryGetValue(id, out var n)) return n;
            return N.NameOf(id);
        }

        // ================================================================== projectiles

        class Proj
        {
            public string pid, owner;
            public WeaponDef w;
            public int team;
            public Vector3 pos, vel;
            public GameObject go;
            public float life;
            public bool live, stuck;
        }

        public void LaunchProjectile(WeaponDef w, string skin, Vector3 pos, Vector3 vel, string owner, int team, bool live)
        {
            string pid = owner + "_" + (++pidSeq);
            Spawn(pid, w, skin, pos, vel, owner, team, live);
            N.SendR("PR", pid, w.id, skin, Net.F(pos.x), Net.F(pos.y), Net.F(pos.z), Net.F(vel.x), Net.F(vel.y), Net.F(vel.z), SceneId, team.ToString());
        }

        void Spawn(string pid, WeaponDef w, string skin, Vector3 pos, Vector3 vel, string owner, int team, bool live)
        {
            var go = w.type == WType.Rocket ? Models.Rocket(fx) : Models.Weapon(w.id, skin, fx, 0);
            go.transform.position = pos;
            if (w.type == WType.Rocket)
            {
                var trail = go.AddComponent<TrailRenderer>();
                trail.time = 0.35f; trail.startWidth = 0.18f; trail.endWidth = 0.02f;
                trail.material = Mats.Additive(new Color(1f, 0.6f, 0.25f, 0.6f), Tex.Glow());
            }
            projs.Add(new Proj { pid = pid, owner = owner, w = w, team = team, pos = pos, vel = vel, go = go, life = w.type == WType.Grenade ? w.fuse : 6, live = live });
        }

        readonly RaycastHit[] phits = new RaycastHit[16];

        void UpdateProjectiles(float dt)
        {
            for (int i = projs.Count - 1; i >= 0; i--)
            {
                var p = projs[i];
                p.life -= dt;
                if (p.go == null) { projs.RemoveAt(i); continue; }
                if (p.life <= 0)
                {
                    if (p.w.type == WType.Grenade && p.live) Explode(p);
                    Destroy(p.go); projs.RemoveAt(i); continue;
                }
                if (p.stuck) continue;
                var prev = p.pos;
                if (p.w.type != WType.Rocket) p.vel.y -= p.w.gravity * dt;
                p.pos += p.vel * dt;
                var d = p.pos - prev;
                float len = d.magnitude;
                bool done = false;
                if (len > 1e-4f)
                {
                    int n = Physics.RaycastNonAlloc(prev, d / len, phits, len, Layers.ShotMask, QueryTriggerInteraction.Collide);
                    System.Array.Sort(phits, 0, n, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
                    for (int k = 0; k < n && !done; k++)
                    {
                        var h = phits[k];
                        var ht = h.collider.GetComponent<Hittable>();
                        if (ht != null && ht.kind == 0)
                        {
                            if (ht.id == p.owner || (M != null && M.Team(ht.id) == p.team)) continue;
                            if (p.w.type == WType.Grenade) continue;
                            p.pos = h.point;
                            if (p.w.type == WType.Rocket) { if (p.live) Explode(p); done = true; }
                            else
                            {
                                if (p.live && p.owner == Me) { DealDamage(ht.id, p.w.dmg * (ht.head ? 1.5f : 1), p.w.id, ht.head); Blood(h.point); }
                                done = true;
                            }
                            break;
                        }
                        if (ht != null && ht.kind == 1)
                        {
                            if (p.w.type == WType.Grenade) continue;
                            p.pos = h.point;
                            if (p.live) ht.target.Hit();
                            if (p.w.type == WType.Rocket && p.live) Explode(p);
                            done = true;
                            break;
                        }
                        // world / ball
                        if (ht != null && ht.kind == 2 && p.live) KickBall(p.vel.normalized * 10);
                        if (p.w.type == WType.Rocket) { p.pos = h.point; if (p.live) Explode(p); done = true; }
                        else if (p.w.type == WType.Grenade)
                        {
                            p.pos = h.point + h.normal * 0.06f;
                            p.vel = Vector3.Reflect(p.vel, h.normal) * 0.45f;
                            if (p.vel.magnitude < 1f) p.vel = Vector3.zero;
                        }
                        else { p.pos = h.point; p.stuck = true; p.life = 4; Impact(h.point, h.normal); AudioFx.At("melee", h.point, 0.5f); }
                        break;
                    }
                }
                if (done) { Destroy(p.go); projs.RemoveAt(i); continue; }
                p.go.transform.position = p.pos;
                if (p.w.type == WType.Rocket) p.go.transform.rotation = Quaternion.LookRotation(p.vel);
                else if (!p.stuck) p.go.transform.Rotate(p.w.id == "shuriken" ? new Vector3(0, 1500 * dt, 0) : new Vector3(900 * dt, 0, 0), Space.Self);
            }
        }

        void Explode(Proj p)
        {
            var w = p.w;
            Boom(p.pos, w);
            N.SendR("BM", p.pid, Net.F(p.pos.x), Net.F(p.pos.y), Net.F(p.pos.z), w.id, SceneId);
            if (p.owner != Me) return;
            foreach (var c in Combatants().ToList())
            {
                if (!c.alive) continue;
                bool self = c.id == Me;
                if (!self && (M == null || M.Team(c.id) == p.team)) continue;
                var chest = c.pos + Vector3.up * 1.1f;
                float d = Vector3.Distance(chest, p.pos);
                if (d > w.splash) continue;
                if (Physics.Linecast(p.pos + (chest - p.pos).normalized * 0.2f, chest, Layers.SolidMask, QueryTriggerInteraction.Ignore)) continue;
                float dmg = w.dmg * Mathf.Pow(1 - d / w.splash, 0.7f);
                if (self) { if (M != null) P.TakeDamage(dmg * 0.5f, Me, w.id); P.vel += (chest - p.pos).normalized * 8 * (1 - d / w.splash); }
                else DealDamage(c.id, dmg, w.id, false);
            }
            if (M == null)
                foreach (var col in Physics.OverlapSphere(p.pos, w.splash, 1 << Layers.Hitbox, QueryTriggerInteraction.Collide))
                {
                    var ht = col.GetComponent<Hittable>();
                    if (ht != null && ht.kind == 1) ht.target.Hit();
                }
            if (Cur.ball != null)
            {
                var bd = Cur.ball.transform.position - p.pos;
                if (bd.magnitude < w.splash + Ball.R) KickBall(bd.normalized * 14);
            }
        }

        // ================================================================== effects

        public void Tracer(Vector3 a, Vector3 b, bool mine)
        {
            var go = new GameObject("Tracer");
            go.transform.SetParent(fx, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2; lr.SetPosition(0, a); lr.SetPosition(1, b);
            lr.startWidth = mine ? 0.03f : 0.05f; lr.endWidth = 0.02f;
            lr.material = Mats.Additive(new Color(1f, 0.85f, 0.4f, 0.8f));
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(go, 0.06f);
        }

        public void Impact(Vector3 p, Vector3 n)
        {
            var q = Prim.Make(PrimitiveType.Quad, fx, p + n * 0.03f, Vector3.one * 0.25f, Mats.Additive(new Color(1f, 0.8f, 0.4f, 0.8f), Tex.Glow()), "Spark");
            q.transform.rotation = Quaternion.LookRotation(-n);
            Object.Destroy(q.GetComponent<Collider>());
            Destroy(q, 0.12f);
        }

        public void Blood(Vector3 p)
        {
            var q = Prim.Make(PrimitiveType.Sphere, fx, p, Vector3.one * 0.25f, Mats.Additive(new Color(1f, 0.15f, 0.15f, 0.7f), Tex.Glow()), "Hit");
            Destroy(q, 0.12f);
        }

        void Boom(Vector3 p, WeaponDef w)
        {
            AudioFx.At("boom", p, 1.4f);
            var s = Prim.Make(PrimitiveType.Sphere, fx, p, Vector3.one * 0.5f, Mats.Additive(new Color(1f, 0.55f, 0.15f, 0.9f), Tex.Glow()), "Boom");
            s.AddComponent<BoomFx>().size = w.splash * 1.4f;
            var lg = new GameObject("BoomLight");
            lg.transform.SetParent(s.transform, false);
            var l = lg.AddComponent<Light>(); l.type = LightType.Point; l.color = new Color(1f, 0.6f, 0.3f); l.range = w.splash * 3; l.intensity = 8;
        }

        public void SendShot(Vector3 a, Vector3 b, string weapon)
        {
            N.Send("F", Net.F(a.x), Net.F(a.y), Net.F(a.z), Net.F(b.x), Net.F(b.y), Net.F(b.z), weapon, Me);
        }

        // ================================================================== network

        void HostAll(string type, params string[] args)
        {
            N.SendR(type, args);
            OnNet(type, Me, args);
        }

        void MemberLeft(string id)
        {
            if (Remotes.TryGetValue(id, out var a)) { Destroy(a.gameObject); Remotes.Remove(id); }
            Hud.Toast(NameOf(id) + " вышел из отряда", UIKit.Muted);
            Menus.RefreshParty();
        }

        void OnNet(string type, string from, string[] a)
        {
            bool fromHost = from == N.Room;
            switch (type)
            {
                case "HELLO":
                    Hud.Toast(a[0] + " вошёл в отряд!", UIKit.Lime);
                    AudioFx.Play("coin");
                    Menus.RefreshParty();
                    break;
                case "S" when a.Length >= 12:
                {
                    if (!Remotes.TryGetValue(from, out var av))
                    {
                        av = Avatar.Create(from, a[11], actors);
                        Remotes[from] = av;
                        Menus.RefreshParty();
                    }
                    N.AddMember(from, a[11]);
                    av.Scene = a[0];
                    av.SetName(a[11]);
                    av.SetTeam(a[0] == "lobby" ? 0 : Net.PI(a[10]));
                    av.SetWeapon(a[6], a[7]);
                    av.SetState(new Vector3(Net.PF(a[1]), Net.PF(a[2]), Net.PF(a[3])), Net.PF(a[4]), Net.PF(a[5]), a[9] == "1", Net.PI(a[8]));
                    break;
                }
                case "B" when fromHost && M != null:
                    foreach (var e in a[0].Split(';'))
                    {
                        var p = e.Split(',');
                        if (p.Length < 7 || !botViews.TryGetValue(p[0], out var av)) continue;
                        av.SetState(new Vector3(Net.PF(p[1]), Net.PF(p[2]), Net.PF(p[3])), Net.PF(p[4]), Net.PF(p[5]), p[6] == "1", 100);
                    }
                    break;
                case "BL" when fromHost && Cur?.ball != null && !IsHost:
                    Cur.ball.netPos = new Vector3(Net.PF(a[0]), Ball.R, Net.PF(a[1]));
                    Cur.ball.netVel = new Vector3(Net.PF(a[2]), 0, Net.PF(a[3]));
                    break;
                case "KK" when IsHost && Cur?.ball != null:
                    Cur.ball.Kick(new Vector3(Net.PF(a[0]), 0, Net.PF(a[1])));
                    break;
                case "F" when a.Length >= 8:
                {
                    var p0 = new Vector3(Net.PF(a[0]), Net.PF(a[1]), Net.PF(a[2]));
                    var p1 = new Vector3(Net.PF(a[3]), Net.PF(a[4]), Net.PF(a[5]));
                    Avatar shooter = Remotes.TryGetValue(a[7], out var r) ? r : botViews.TryGetValue(a[7], out var bv) ? bv : null;
                    if (shooter != null && shooter.Scene != SceneId && !botViews.ContainsKey(a[7])) break;
                    if (shooter != null) shooter.MuzzleFlash();
                    Tracer(p0, p1, false);
                    var w = Defs.W(a[6]);
                    AudioFx.At(w != null ? w.sound : "rifle", p0, 0.9f);
                    break;
                }
                case "PR" when a.Length >= 11:
                {
                    if (a[9] != SceneId) break;
                    var w = Defs.W(a[1]);
                    if (w == null) break;
                    Spawn(a[0], w, a[2], new Vector3(Net.PF(a[3]), Net.PF(a[4]), Net.PF(a[5])), new Vector3(Net.PF(a[6]), Net.PF(a[7]), Net.PF(a[8])), from, Net.PI(a[10]), false);
                    if (w.type == WType.Rocket) AudioFx.At("rocket", new Vector3(Net.PF(a[3]), Net.PF(a[4]), Net.PF(a[5])));
                    break;
                }
                case "BM" when a.Length >= 6:
                {
                    if (a[5] != SceneId) break;
                    var p = projs.Find(x => x.pid == a[0]);
                    if (p != null) { Destroy(p.go); projs.Remove(p); }
                    Boom(new Vector3(Net.PF(a[1]), Net.PF(a[2]), Net.PF(a[3])), Defs.W(a[4]) ?? Defs.W("f1"));
                    break;
                }
                case "HIT" when a.Length >= 5:
                    if (a[0] == Me) P.TakeDamage(Net.PF(a[1]), a[4], a[2]);
                    else if (IsHost && IsBot(a[0])) BotDamage(a[0], Net.PF(a[1]), a[4], a[2]);
                    break;
                case "DIE" when IsHost && a.Length >= 3:
                    HostDeath(a[0], a[1], a[2]);
                    break;
                case "K" when fromHost && M != null && a.Length >= 6:
                {
                    M.score[1] = Net.PI(a[3]); M.score[2] = Net.PI(a[4]);
                    ReadStats(a[5]);
                    Hud.KillFeed(NameOf(a[1]), M.Team(a[1]), NameOf(a[0]), M.Team(a[0]), Defs.W(a[2])?.name ?? "");
                    if (a[1] == Me && a[0] != Me) { M.earned += Defs.RewardKill; M.kills++; AudioFx.Play("kill"); Hud.Center("+" + Defs.RewardKill + " монет", 1f, UIKit.Gold); }
                    if (IsBot(a[0]) && botViews.TryGetValue(a[0], out var bv)) bv.SetState(bv.transform.position, bv.tYaw, 0, false, 0);
                    break;
                }
                case "GL" when fromHost && M != null:
                {
                    int team = Net.PI(a[0]);
                    M.score[1] = Net.PI(a[1]); M.score[2] = Net.PI(a[2]);
                    if (!IsHost) Cur.ball?.Reset();
                    AudioFx.Play("goal");
                    Hud.Center("ГОЛ! +3 команде " + Defs.TeamName(team), 2.5f, Defs.TeamColor(team));
                    if (team == P.team) M.earned += Defs.RewardGoal;
                    break;
                }
                case "PK" when IsHost && M != null:
                    HostPick(Net.PI(a[0]), from);
                    break;
                case "PKS" when fromHost && M != null:
                {
                    var pk = Cur.pickups.Find(x => x.id == Net.PI(a[0]));
                    if (pk == null) break;
                    pk.active = a[1] == "1";
                    if (pk.go != null) pk.go.SetActive(pk.active);
                    if (a[2] == Me) { P.Heal(50); AudioFx.Play("pickup"); Hud.Center("+50 здоровья", 1f, UIKit.Lime); }
                    break;
                }
                case "Q" when IsHost:
                    HostQueue(Net.PI(a[0]), from);
                    break;
                case "VS" when fromHost:
                    if (M != null) break;
                    V = new VoteView { mode = Net.PI(a[0]), endAt = Time.time + Net.PF(a[1]), teamMode = Net.PI(a[2]) };
                    Menus.ShowVote();
                    break;
                case "VT" when IsHost:
                    HostVote(from, a[0]);
                    break;
                case "VU" when fromHost && V != null:
                    V.teamMode = Net.PI(a[0]);
                    V.votes.Clear();
                    foreach (var e in a[1].Split(';')) { var p = e.Split(':'); if (p.Length == 2) V.votes[p[0]] = p[1]; }
                    Menus.UpdateVote();
                    break;
                case "VC" when fromHost:
                    CloseVote();
                    Hud.Toast("Выбор локации отменён", UIKit.Muted);
                    break;
                case "VR" when fromHost && V != null:
                    V.result = a[0];
                    AudioFx.Play("go");
                    Menus.UpdateVote();
                    break;
                case "GO" when fromHost && a.Length >= 7:
                    StartMatch(a);
                    break;
                case "END" when fromHost && M != null && a.Length >= 5:
                {
                    M.state = 2;
                    M.score[1] = Net.PI(a[1]); M.score[2] = Net.PI(a[2]);
                    ReadStats(a[3]);
                    int win = Net.PI(a[0]);
                    bool abort = a.Length > 4 && a[4] == "1";
                    int reward = abort ? 0 : win == 0 ? Defs.RewardDraw : win == P.team ? Defs.RewardWin : Defs.RewardLose;
                    int total = M.earned + reward;
                    Save.P.coins += total;
                    Save.P.matches++; Save.P.kills += M.kills; Save.P.deaths += M.deaths;
                    if (win == P.team && !abort) Save.P.wins++;
                    Save.Write();
                    M.earned = 0;
                    AudioFx.Play(win == P.team ? "win" : "lose");
                    Menus.ShowEnd(win, abort, reward, total);
                    break;
                }
            }
        }
    }

    /// <summary>Expanding, fading explosion ball.</summary>
    public class BoomFx : MonoBehaviour
    {
        public float size = 6;
        float t;
        Material m;
        void Start() { m = GetComponent<Renderer>().material; }
        void Update()
        {
            t += Time.deltaTime / 0.45f;
            transform.localScale = Vector3.one * Mathf.Lerp(0.5f, size, Mathf.Sqrt(t));
            var c = m.color; c.a = Mathf.Clamp01(1 - t) * 0.9f; m.color = c;
            var l = GetComponentInChildren<Light>(); if (l != null) l.intensity = 8 * (1 - t);
            if (t >= 1) Destroy(gameObject);
        }
    }
}
