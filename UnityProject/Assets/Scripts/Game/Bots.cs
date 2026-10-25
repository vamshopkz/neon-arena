using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>Walkable grid (1 m cells) of the current map for bot path finding (A*).</summary>
    public class NavGrid
    {
        public float x0, z0, cell = 1f;
        public int W, H;
        public bool[] blocked;

        public static NavGrid Build(Map m)
        {
            var n = new NavGrid { x0 = m.bounds.xMin, z0 = m.bounds.yMin };
            n.W = Mathf.CeilToInt(m.bounds.width / n.cell); n.H = Mathf.CeilToInt(m.bounds.height / n.cell);
            n.blocked = new bool[n.W * n.H];
            for (int j = 0; j < n.H; j++)
                for (int i = 0; i < n.W; i++)
                {
                    var c = new Vector3(n.x0 + (i + 0.5f) * n.cell, 0, n.z0 + (j + 0.5f) * n.cell);
                    n.blocked[j * n.W + i] = Physics.CheckCapsule(c + Vector3.up * 0.6f, c + Vector3.up * 1.4f, 0.45f, Layers.SolidMask, QueryTriggerInteraction.Ignore)
                        || !Physics.Raycast(c + Vector3.up * 0.5f, Vector3.down, 1.5f, Layers.SolidMask, QueryTriggerInteraction.Ignore);   // no floor (ship: the sea)
                }
            return n;
        }

        bool Free(int i, int j) => i >= 0 && j >= 0 && i < W && j < H && !blocked[j * W + i];
        Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.Clamp(Mathf.FloorToInt((p.x - x0) / cell), 0, W - 1), Mathf.Clamp(Mathf.FloorToInt((p.z - z0) / cell), 0, H - 1));
        Vector3 Center(int i, int j) => new Vector3(x0 + (i + 0.5f) * cell, 0, z0 + (j + 0.5f) * cell);

        Vector2Int Nearest(Vector2Int c)
        {
            if (Free(c.x, c.y)) return c;
            for (int r = 1; r < 6; r++)
                for (int dj = -r; dj <= r; dj++)
                    for (int di = -r; di <= r; di++)
                        if (Free(c.x + di, c.y + dj)) return new Vector2Int(c.x + di, c.y + dj);
            return c;
        }

        public List<Vector3> Path(Vector3 from, Vector3 to)
        {
            var s = Nearest(Cell(from)); var t = Nearest(Cell(to));
            int N = W * H, si = s.y * W + s.x, ti = t.y * W + t.x;
            var g = new float[N]; var came = new int[N]; var closed = new bool[N];
            for (int i = 0; i < N; i++) { g[i] = float.MaxValue; came[i] = -1; }
            g[si] = 0;
            var open = new List<(float f, int i)> { (0, si) };
            int guard = 0;
            while (open.Count > 0 && guard++ < 9000)
            {
                int bi = 0;
                for (int k = 1; k < open.Count; k++) if (open[k].f < open[bi].f) bi = k;
                int cur = open[bi].i;
                open[bi] = open[open.Count - 1]; open.RemoveAt(open.Count - 1);
                if (closed[cur]) continue;
                closed[cur] = true;
                if (cur == ti) break;
                int cx = cur % W, cy = cur / W;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx, ny = cy + dy;
                        if (!Free(nx, ny)) continue;
                        if (dx != 0 && dy != 0 && (!Free(cx + dx, cy) || !Free(cx, cy + dy))) continue;
                        int ni = ny * W + nx;
                        float ng = g[cur] + (dx != 0 && dy != 0 ? 1.414f : 1f);
                        if (ng >= g[ni]) continue;
                        g[ni] = ng; came[ni] = cur;
                        float hx = Mathf.Abs(nx - t.x), hy = Mathf.Abs(ny - t.y);
                        open.Add((ng + Mathf.Max(hx, hy) + 0.414f * Mathf.Min(hx, hy), ni));
                    }
            }
            var path = new List<Vector3>();
            if (came[ti] < 0 && ti != si) return path;
            for (int c = ti; c != -1 && c != si; c = came[c]) path.Add(Center(c % W, c / W));
            path.Reverse();
            // drop collinear points
            var res = new List<Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                if (i > 0 && i < path.Count - 1)
                {
                    var d1 = path[i] - path[i - 1]; var d2 = path[i + 1] - path[i];
                    if ((d1 - d2).sqrMagnitude < 0.01f) continue;
                }
                res.Add(path[i]);
            }
            return res;
        }
    }

    /// <summary>A bot (simulated only by the host): finds enemies, walks paths, strafes and shoots.</summary>
    public class Bot
    {
        public string id, name, weapon, skin;
        public int team;
        public Avatar av;
        public CharacterController cc;
        public float hp = 100, respawnAt, yaw, pitch, vy;
        public bool alive = true;
        List<Vector3> path;
        int pi;
        float pathAt, think, nextShot, strafe = 1, strafeT, react, stuckT;
        string targetId;
        Vector3 lastPos;
        int ammo;

        public Vector3 Pos => av.transform.position;

        public static Bot Create(string id, string name, int team, string weapon, string skin, Transform parent)
        {
            var b = new Bot { id = id, name = name, team = team, weapon = weapon, skin = skin };
            b.av = Avatar.Create(id, name, parent);
            b.av.Smooth = false;
            b.av.gameObject.layer = Layers.Body;
            b.av.SetTeam(team);
            b.av.SetWeapon(weapon, skin);
            b.cc = b.av.gameObject.AddComponent<CharacterController>();
            b.cc.height = 1.8f; b.cc.radius = 0.35f; b.cc.center = new Vector3(0, 0.9f, 0); b.cc.stepOffset = 0.48f; b.cc.slopeLimit = 50;
            b.ammo = Defs.W(weapon).mag;
            return b;
        }

        public void Spawn(Spawn s)
        {
            cc.enabled = false;
            av.transform.position = s.pos + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            cc.enabled = true;
            yaw = s.yaw; hp = 100; alive = true; path = null; targetId = null;
            av.SetState(av.transform.position, yaw, 0, true, 100);
            ammo = Defs.W(weapon).mag;
        }

        public void Hurt(string by) { if (targetId == null) { targetId = by; react = Time.time + 0.3f; } }

        public void Tick(Game g, float dt)
        {
            if (!alive)
            {
                if (Time.time >= respawnAt) Spawn(g.SpawnFor(team));
                return;
            }
            var w = Defs.W(weapon);
            var pos = Pos;
            var eye = pos + Vector3.up * 1.55f;
            think -= dt;
            if (think <= 0)
            {
                think = 0.2f + Random.value * 0.15f;
                string best = null; float bd = 1e9f;
                foreach (var e in g.Combatants())
                {
                    if (!e.alive || e.team == team || e.id == id) continue;
                    float d = Vector3.Distance(e.pos, pos);
                    if (d > 75 || d >= bd) continue;
                    var chest = e.pos + Vector3.up * 1.2f;
                    if (Physics.Linecast(eye, chest, Layers.SolidMask, QueryTriggerInteraction.Ignore)) continue;
                    best = e.id; bd = d;
                }
                if (best != null && best != targetId) react = Time.time + 0.35f + Random.value * 0.35f;
                targetId = best;
                if (best == null && Time.time > pathAt)
                {
                    pathAt = Time.time + 1.5f + Random.value;
                    Vector3? goal = null; float nd = 1e9f;
                    foreach (var e in g.Combatants())
                    {
                        if (!e.alive || e.team == team) continue;
                        float d = Vector3.Distance(e.pos, pos);
                        if (d < nd) { nd = d; goal = e.pos; }
                    }
                    if (g.BallGoalFor(team, out var ballGoal) && Random.value < 0.35f) goal = ballGoal;
                    path = goal.HasValue && g.Nav != null ? g.Nav.Path(pos, goal.Value) : null;
                    pi = 0;
                }
            }

            Vector2 mv = Vector2.zero;
            float wantYaw = yaw;
            var tgt = targetId != null ? g.Find(targetId) : null;
            if (tgt != null && tgt.Value.alive)
            {
                var t = tgt.Value;
                var to = t.pos - pos;
                float d = new Vector2(to.x, to.z).magnitude;
                wantYaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                pitch = -Mathf.Atan2(t.pos.y + 1.2f - eye.y, d) * Mathf.Rad2Deg;
                strafeT -= dt;
                if (strafeT <= 0) { strafe = Random.value < 0.5f ? -1 : 1; strafeT = 0.6f + Random.value * 1.2f; }
                float pref = w.zoom > 0 ? 28 : w.cat == "pistol" ? 11 : 17;
                mv = new Vector2(strafe * 0.8f, d > pref + 4 ? 1 : d < pref - 6 ? -0.7f : 0);
                if (Time.time > react && Time.time >= nextShot && Mathf.Abs(Mathf.DeltaAngle(yaw, wantYaw)) < 12)
                {
                    nextShot = Time.time + 1f / w.rate * (1.5f + Random.value * 0.9f);
                    g.BotShoot(this, t, d);
                    if (--ammo <= 0) { ammo = w.mag; nextShot += w.reload; }
                }
            }
            else
            {
                targetId = null;
                pitch = 0;
                if (path != null && pi < path.Count)
                {
                    var p = path[pi]; var d = p - pos; d.y = 0;
                    if (d.magnitude < 0.8f) pi++;
                    else { wantYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; mv = new Vector2(0, 1); }
                }
            }
            yaw = Mathf.MoveTowardsAngle(yaw, wantYaw, dt * 420);
            var q = Quaternion.Euler(0, yaw, 0);
            var v = q * new Vector3(mv.x, 0, mv.y) * 4.6f;
            if (cc.isGrounded && vy < 0) vy = -2;
            vy -= g.Cur.gravity * dt;
            v.y = vy;
            cc.Move(v * dt);
            if (pos.y < g.Cur.fallY) Spawn(g.SpawnFor(team));
            stuckT += dt;
            if (stuckT > 1)
            {
                if ((Pos - lastPos).magnitude < 0.4f && mv.sqrMagnitude > 0.1f) { if (cc.isGrounded) vy = 6.2f; pathAt = 0; strafe = -strafe; }
                lastPos = Pos; stuckT = 0;
            }
            av.tYaw = yaw; av.tPitch = pitch;
            av.SetState(Pos, yaw, pitch, true, hp);
        }
    }
}
