using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>The second set of battle worlds: city, warehouse, moon station, winter village, pirate ship.</summary>
    public static class MapsMore
    {
        static Color H(string h) => Tex.Hex(h);

        static void Bounds(Maps.W w, float hx, float hz, float h = 10)
        {
            w.Solid(new Vector3(0, h / 2, -hz), new Vector3(hx * 2 + 2, h, 1)); w.Solid(new Vector3(0, h / 2, hz), new Vector3(hx * 2 + 2, h, 1));
            w.Solid(new Vector3(-hx, h / 2, 0), new Vector3(1, h, hz * 2 + 2)); w.Solid(new Vector3(hx, h / 2, 0), new Vector3(1, h, hz * 2 + 2));
        }

        static void Spawns(Map m, float[,] a)
        {
            for (int i = 0; i < a.GetLength(0); i++) { m.A.Add(Maps.SP(a[i, 0], a[i, 1])); m.B.Add(Maps.SP(-a[i, 0], -a[i, 1])); }
        }

        /// <summary>Car: body, cabin, windows, wheels, lights (the body is a solid cover).</summary>
        static void Car(Maps.W w, float x, float z, float yaw, Color paint)
        {
            var q = Quaternion.Euler(0, yaw, 0);
            var body = Mats.Lit(paint, 0.75f, 0.4f);
            var glass = Mats.Lit(H("1d2a38"), 0.95f, 0.3f);
            var tyre = Mats.Lit(H("111111"), 0.1f);
            var rim = Mats.Lit(H("b9bec6"), 0.8f, 0.9f);
            Vector3 P(float lx, float ly, float lz) => new Vector3(x, 0, z) + q * new Vector3(lx, ly, lz);
            w.g.Box(body, P(0, 0.75f, 0), new Vector3(1.9f, 0.7f, 4.4f), q);
            w.g.Box(body, P(0, 1.35f, -0.2f), new Vector3(1.7f, 0.55f, 2.3f), q);
            w.g.Box(glass, P(0, 1.36f, -0.2f), new Vector3(1.72f, 0.42f, 2.1f), q);
            w.g.Box(Mats.Glow(new Color(1.4f, 1.3f, 1f)), P(0.6f, 0.85f, 2.21f), new Vector3(0.35f, 0.15f, 0.02f), q);
            w.g.Box(Mats.Glow(new Color(1.4f, 1.3f, 1f)), P(-0.6f, 0.85f, 2.21f), new Vector3(0.35f, 0.15f, 0.02f), q);
            w.g.Box(Mats.Glow(new Color(1.4f, 0.1f, 0.1f)), P(0.65f, 0.85f, -2.21f), new Vector3(0.3f, 0.12f, 0.02f), q);
            w.g.Box(Mats.Glow(new Color(1.4f, 0.1f, 0.1f)), P(-0.65f, 0.85f, -2.21f), new Vector3(0.3f, 0.12f, 0.02f), q);
            foreach (var sx in new[] { -1, 1 }) foreach (var sz in new[] { -1.4f, 1.4f })
                {
                    w.g.Cyl(tyre, P(sx * 0.95f, 0.36f, sz) + q * new Vector3(-0.13f * sx, 0, 0), 0.36f, 0.26f, 14, -1, true, 1, q * Quaternion.Euler(0, 0, sx * 90));
                    w.g.Cyl(rim, P(sx * 1.08f, 0.36f, sz), 0.2f, 0.02f, 10, -1, true, 1, q * Quaternion.Euler(0, 0, sx * 90));
                }
            w.Solid(P(0, 0.9f, 0), new Vector3(1.9f, 1.8f, 4.4f), yaw);
        }

        // ------------------------------------------------------------------ CITY
        public static void City(Map m, int seed, bool shadows)
        {
            var w = new Maps.W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = H("c7d3df"); m.fogStart = 60; m.fogEnd = 200; m.bg = m.fog;
            m.ambSky = H("b8cbe0"); m.ambEq = H("9aa3ad"); m.ambGround = H("4a4c52");
            Maps.Sun(gr, H("fff0dc"), 1.3f, new Vector3(48, 35, 0), shadows);
            var asphalt = Mats.Tex(Tex.Asphalt(), Color.white, 0.25f);
            var walk = Mats.Tex(Tex.Concrete(), new Color(0.85f, 0.85f, 0.85f), 0.1f);
            var line = Mats.Lit(H("f2f2f2"), 0.3f);
            var yellow = Mats.Lit(H("f2c230"), 0.3f);
            Maps.Ground(gr, 220, asphalt, 6);
            var r = new Rng(seed);
            // pavements around four blocks, buildings on them
            string[] walls = { "c9b8a2", "a7b4c2", "d6c7b0", "9e9a92", "b88c74", "c4c9cf" };
            foreach (int sx in new[] { -1, 1 })
                foreach (int sz in new[] { -1, 1 })
                {
                    float cx = sx * 20, cz = sz * 20;
                    w.g.Box(walk, new Vector3(cx, 0.08f, cz), new Vector3(28, 0.16f, 28), Quaternion.identity, 3);
                    w.Solid(new Vector3(cx, 0.08f, cz), new Vector3(28, 0.16f, 28));
                    // two buildings per block, the corner towards the centre stays open (courtyard)
                    for (int b = 0; b < 2; b++)
                    {
                        float bx = cx + sx * (b == 0 ? 8 : -2), bz = cz + sz * (b == 0 ? -2 : 8);
                        float bw = b == 0 ? 10 : 18, bd = b == 0 ? 18 : 10, bh = r.F(9, 24);
                        var fac = Mats.Tex(Tex.Facade(walls[r.I(walls.Length)]), Color.white, 0.2f);
                        w.Block(fac, bx, bz, bw, bd, bh, 0, 0.16f, 4);
                        w.g.Box(Mats.Lit(H("50555c"), 0.3f), new Vector3(bx, bh + 0.31f, bz), new Vector3(bw + 0.4f, 0.3f, bd + 0.4f), Quaternion.identity);
                        w.g.Box(Mats.Lit(H("6f747a"), 0.4f, 0.4f), new Vector3(bx + 1, bh + 1.2f, bz + 1), new Vector3(2.4f, 1.6f, 2.4f), Quaternion.identity);   // roof AC unit
                    }
                    // street lamp + bench in the courtyard
                    var lp = new Vector3(cx - sx * 11, 0, cz - sz * 11);
                    w.Trunk(Mats.Lit(H("2b2f36"), 0.5f, 0.6f), lp, 0.08f, 5, 0.15f);
                    w.g.Box(Mats.Glow(new Color(1.5f, 1.4f, 1.1f)), lp + new Vector3(0, 5.05f, 0), new Vector3(0.5f, 0.15f, 0.5f), Quaternion.identity);
                    w.Block(Mats.Tex(Tex.Planks("8a5a2b"), Color.white, 0.3f), cx - sx * 6, cz - sz * 11, 2.4f, 0.6f, 0.5f, 0, 0.16f, 1);
                }
            // road markings
            for (float t = -34; t <= 34; t += 4)
            {
                if (Mathf.Abs(t) < 7) continue;
                w.g.Box(line, new Vector3(t, 0.01f, 0), new Vector3(2, 0.02f, 0.18f), Quaternion.identity);
                w.g.Box(line, new Vector3(0, 0.01f, t), new Vector3(0.18f, 0.02f, 2), Quaternion.identity);
            }
            for (int i = 0; i < 8; i++)   // zebra crossings
            {
                w.g.Box(line, new Vector3(-3.5f + i, 0.01f, 7.5f), new Vector3(0.5f, 0.02f, 2.6f), Quaternion.identity);
                w.g.Box(line, new Vector3(-3.5f + i, 0.01f, -7.5f), new Vector3(0.5f, 0.02f, 2.6f), Quaternion.identity);
            }
            // cars and barriers as cover
            Color[] paints = { H("c0392b"), H("2e86de"), H("f1c40f"), H("ecf0f1"), H("27ae60"), H("2c3e50"), H("e67e22") };
            float[,] cars = { { -2.5f, -24, 0 }, { 2.6f, 16, 180 }, { -18, 2.5f, 90 }, { 24, -2.6f, -90 }, { 3, -4, 30 }, { -30, -2.5f, 90 }, { 30, 2.6f, -90 }, { -2.6f, 31, 0 } };
            for (int i = 0; i < cars.GetLength(0); i++) Car(w, cars[i, 0], cars[i, 1], cars[i, 2], paints[i % paints.Length]);
            for (int i = 0; i < 4; i++) w.Block(yellow, -9 + i * 6, 0, 1.6f, 0.5f, 0.9f, 45, 0, 1);
            // bus
            var bus = Mats.Lit(H("2ecc71"), 0.6f, 0.3f);
            w.Block(bus, 12, -3, 2.6f, 11, 3.1f, 90, 0.3f, 2);
            for (int i = 0; i < 5; i++) w.g.Box(Mats.Lit(H("1d2a38"), 0.95f), new Vector3(8 + i * 2, 2.3f, -1.68f), new Vector3(1.5f, 1, 0.05f), Quaternion.identity);
            Bounds(w, 36, 36, 20);
            Spawns(m, new float[,] { { -30, -6 }, { -32, 4 }, { -26, 9 }, { -34, -9 }, { -28, -3 }, { -24, 5 } });
            m.bounds = Rect.MinMaxRect(-36, -36, 36, 36);
            w.Done(shadows);
        }

        // ------------------------------------------------------------------ WAREHOUSE
        public static void Warehouse(Map m, int seed, bool shadows)
        {
            var w = new Maps.W(m.root.transform);
            var gr = m.root.transform;
            m.sky = false;
            m.bg = H("20242b"); m.fog = H("2a2f37"); m.fogStart = 40; m.fogEnd = 120;
            m.ambSky = H("a9b3c2"); m.ambEq = H("6b7380"); m.ambGround = H("3a3f47");
            m.vmLight = new Color(0.9f, 0.92f, 1f);
            Maps.Sun(gr, H("f4f7ff"), 0.9f, new Vector3(75, 20, 0), shadows);
            var floor = Mats.Tex(Tex.Concrete(), new Color(0.75f, 0.75f, 0.78f), 0.25f);
            var wallM = Mats.Tex(Tex.Corrugated("8a9099"), Color.white, 0.4f, 0.4f);
            var stripe = Mats.Lit(H("f2c230"), 0.3f);
            var lamp = Mats.Glow(new Color(1.6f, 1.6f, 1.5f));
            var steel = Mats.Lit(H("3d434c"), 0.5f, 0.7f);
            Maps.Ground(gr, 140, floor, 4);
            const float X = 32, Z = 24, WH = 11;
            w.Block(wallM, 0, -Z, X * 2, 0.6f, WH, 0, 0, 4); w.Block(wallM, 0, Z, X * 2, 0.6f, WH, 0, 0, 4);
            w.Block(wallM, -X, 0, 0.6f, Z * 2, WH, 0, 0, 4); w.Block(wallM, X, 0, 0.6f, Z * 2, WH, 0, 0, 4);
            var roofG = new Geo();
            roofG.Box(Mats.Lit(H("3a3f47"), 0.3f), new Vector3(0, WH, 0), new Vector3(X * 2, 0.3f, Z * 2), Quaternion.identity, 4);
            for (int i = -2; i <= 2; i++) for (int j = -1; j <= 1; j++) roofG.Box(lamp, new Vector3(i * 12, WH - 0.3f, j * 14), new Vector3(4, 0.15f, 1), Quaternion.identity);
            for (int i = -6; i <= 6; i++) roofG.Box(steel, new Vector3(i * 5, WH - 0.8f, 0), new Vector3(0.3f, 0.6f, Z * 2), Quaternion.identity);   // roof trusses
            roofG.Build("Roof", gr, shadows, Layers.World);
            // painted lanes
            for (float t = -28; t <= 28; t += 2) { w.g.Box(stripe, new Vector3(t, 0.01f, -6), new Vector3(1, 0.02f, 0.15f), Quaternion.identity); w.g.Box(stripe, new Vector3(t, 0.01f, 6), new Vector3(1, 0.02f, 0.15f), Quaternion.identity); }
            // containers (some stacked) + a staircase onto the central stack
            string[] cols = { "b33a2a", "2a6fb3", "2a8a4a", "c98a1c", "7a3ab3", "c0c0c0" };
            void Container(float x, float z, float y0, float yaw, int c)
            {
                var mat = Mats.Tex(Tex.Corrugated(cols[c % cols.Length]), Color.white, 0.35f, 0.3f);
                w.Block(mat, x, z, 2.6f, 6.1f, 2.6f, yaw, y0, 2.6f);
                var q = Quaternion.Euler(0, yaw, 0);
                var door = new Vector3(x, y0 + 1.3f, z) + q * new Vector3(0, 0, 3.07f);
                w.g.Box(Mats.Lit(H("2b2b2b"), 0.4f, 0.6f), door, new Vector3(2.5f, 2.5f, 0.04f), q);
                for (int b = -1; b <= 1; b += 2) w.g.Box(steel, door + q * new Vector3(b * 0.4f, 0, 0.03f), new Vector3(0.06f, 2.4f, 0.06f), q);
            }
            int ci = 0;
            float[,] ct = { { -20, -12, 0, 0 }, { -20, -12, 2.6f, 0 }, { -14, 10, 0, 90 }, { -6, -14, 0, 90 }, { 6, 14, 0, 90 }, { 14, -10, 0, 90 }, { 20, 12, 0, 0 }, { 20, 12, 2.6f, 0 }, { 0, 0, 0, 0 }, { 0, 0, 2.6f, 0 }, { -24, 8, 0, 0 }, { 25, -6, 0, 0 } };
            for (int i = 0; i < ct.GetLength(0); i++) Container(ct[i, 0], ct[i, 1], ct[i, 2], ct[i, 3], ci++);
            for (int i = 0; i < 12; i++) { float top = 0.44f * (i + 1); w.Block(steel, 0, -8.4f + i * 0.45f, 1.6f, 0.46f, top, 0, 0, 1); }   // stairs up to the stack (5.2 m)
            // pallets with boxes, barrels, forklift
            var crate = Mats.Tex(Tex.Planks("a97a45"), Color.white, 0.3f);
            var barrel = Mats.Lit(H("1f5aa6"), 0.5f, 0.3f);
            var r = new Rng(seed);
            for (int i = 0; i < 10; i++)
            {
                float x = r.F(-28, 28), z = r.F(-20, 20);
                if (Mathf.Abs(x) < 4 && Mathf.Abs(z) < 5) continue;
                w.Block(crate, x, z, 1.2f, 1.2f, 0.15f, 0, 0, 1);
                w.Block(crate, x, z, 1.1f, 1.1f, 1.0f, 0, 0.15f, 1.1f);
            }
            for (int i = 0; i < 8; i++) { float x = r.F(-28, 28), z = r.F(-20, 20); w.g.Cyl(barrel, new Vector3(x, 0, z), 0.32f, 0.95f, 14); w.Solid(new Vector3(x, 0.47f, z), new Vector3(0.64f, 0.95f, 0.64f)); }
            var fork = Mats.Lit(H("f2c230"), 0.5f, 0.3f);
            w.Block(fork, -10, 2, 1.2f, 2.2f, 1.4f, 0, 0, 1);
            w.g.Box(steel, new Vector3(-10, 1.8f, 3.3f), new Vector3(1.0f, 3.6f, 0.15f), Quaternion.identity);
            w.g.Box(steel, new Vector3(-10, 0.1f, 4.0f), new Vector3(0.9f, 0.08f, 1.4f), Quaternion.identity);
            Spawns(m, new float[,] { { -28, -20 }, { -28, -4 }, { -28, 18 }, { -22, -2 }, { -26, 4 }, { -24, -18 } });
            m.bounds = Rect.MinMaxRect(-X, -Z, X, Z);
            w.Done(shadows);
        }

        // ------------------------------------------------------------------ MOON
        public static void Moon(Map m, int seed, bool shadows)
        {
            var w = new Maps.W(m.root.transform);
            var gr = m.root.transform;
            m.sky = false;
            m.bg = H("03040a"); m.fog = H("05060c"); m.fogStart = 80; m.fogEnd = 240;
            m.ambSky = H("6b7080"); m.ambEq = H("4a4e58"); m.ambGround = H("2a2c32");
            m.gravity = 7f;
            Maps.Sun(gr, Color.white, 1.6f, new Vector3(28, -60, 0), shadows);
            var ground = Mats.Tex(Tex.MoonGround(), Color.white, 0.05f);
            var hull = Mats.Lit(H("e6e8ec"), 0.6f, 0.2f);
            var panel = Mats.Emissive(Tex.Grid("1b2d5a", "4fa3ff", 6), Color.white, new Color(0.3f, 0.5f, 0.9f));
            var orange = Mats.Lit(H("f08a1c"), 0.5f);
            var glass = Mats.Transparent(new Color(0.5f, 0.8f, 1f, 0.35f));
            Maps.Ground(gr, 260, ground, 8);
            var r = new Rng(seed);
            // craters (raised rims) and rocks
            for (int i = 0; i < 10; i++)
            {
                float x = r.F(-30, 30), z = r.F(-30, 30), rad = r.F(3, 6);
                if (new Vector2(x, z).magnitude < 8) continue;
                for (int k = 0; k < 12; k++)
                {
                    float a = k * Mathf.PI / 6;
                    w.Block(ground, x + Mathf.Cos(a) * rad, z + Mathf.Sin(a) * rad, rad * 0.55f, 1.2f, 0.7f, -a * Mathf.Rad2Deg + 90, 0, 2);
                }
            }
            for (int i = 0; i < 18; i++)
            {
                float x = r.F(-32, 32), z = r.F(-32, 32), s = r.F(0.6f, 1.8f);
                w.g.Mesh(ground, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, s * 0.3f, z), Quaternion.Euler(r.F(0, 90), r.F(0, 360), 0), new Vector3(s * 1.4f, s, s * 1.2f)));
                w.Solid(new Vector3(x, s * 0.45f, z), new Vector3(s * 1.1f, s * 0.9f, s));
            }
            // station: central dome + modules as cover
            w.g.Mesh(hull, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(12, 9, 12)));
            w.g.Mesh(glass, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(0, 3.6f, 0), Quaternion.identity, new Vector3(5, 3.5f, 5)));
            w.Solid(new Vector3(0, 2, 0), new Vector3(9, 4, 9));
            w.Solid(new Vector3(0, 2, 0), new Vector3(11, 2.2f, 4)); w.Solid(new Vector3(0, 2, 0), new Vector3(4, 2.2f, 11));
            foreach (var (x, z, yaw) in new[] { (-16f, 6f, 0f), (16f, -6f, 0f), (-6f, -18f, 90f), (6f, 18f, 90f) })
            {
                var q = Quaternion.Euler(0, yaw, 0);
                w.g.Cyl(hull, new Vector3(x, 1.6f, z) + q * new Vector3(0, 0, -5), 1.6f, 10, 16, -1, true, 2, q * Quaternion.Euler(90, 0, 0));
                for (int k = 0; k < 4; k++) w.g.Box(panel, new Vector3(x, 1.6f, z) + q * new Vector3(1.6f, 0, -3.6f + k * 2.4f), new Vector3(0.05f, 0.8f, 1.6f), q);
                for (int k = 0; k < 2; k++) w.g.Cyl(orange, new Vector3(x, 1.6f, z) + q * new Vector3(0, 0, -5.1f + k * 10.1f), 1.75f, 0.15f, 16, -1, true, 2, q * Quaternion.Euler(90, 0, 0));
                w.Solid(new Vector3(x, 1.6f, z), new Vector3(3.2f, 3.2f, 10), yaw);
            }
            // solar panels, antenna, rover
            for (int i = 0; i < 4; i++)
            {
                float x = -24 + i * 16;
                w.Trunk(hull, new Vector3(x, 0, 26), 0.1f, 1.6f, 0.2f);
                w.g.Box(panel, new Vector3(x, 1.8f, 26), new Vector3(5, 0.08f, 2.4f), Quaternion.Euler(25, 0, 0));
            }
            w.Trunk(hull, new Vector3(26, 0, -24), 0.12f, 8, 0.25f);
            w.g.Mesh(hull, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(26, 8.2f, -24), Quaternion.Euler(-40, 30, 0), new Vector3(3, 0.6f, 3)));
            w.Block(orange, -22, -22, 2.4f, 3.6f, 1.2f, 30, 0.6f, 2);
            // the Earth in the sky + stars
            var earth = Prim.Make(PrimitiveType.Sphere, gr, new Vector3(-90, 70, 150), Vector3.one * 40, Mats.Emissive(Tex.Water(), new Color(0.6f, 0.8f, 1f), new Color(0.25f, 0.4f, 0.7f)), "Earth");
            Object.Destroy(earth.GetComponent<Collider>());
            var sg = new Geo(); var sm = Mats.Glow(Color.white);
            for (int i = 0; i < 500; i++) { var d = Random.onUnitSphere; d.y = Mathf.Abs(d.y) + 0.05f; sg.Box(sm, d.normalized * 180, Vector3.one * Random.Range(0.3f, 0.9f), Quaternion.identity); }
            sg.Build("Stars", gr, false, 0);
            Bounds(w, 34, 34, 30);
            Spawns(m, new float[,] { { -28, -10 }, { -30, 2 }, { -26, 12 }, { -32, -20 }, { -22, -4 }, { -28, 20 } });
            m.bounds = Rect.MinMaxRect(-34, -34, 34, 34);
            w.Done(shadows);
        }

        // ------------------------------------------------------------------ WINTER VILLAGE
        public static void Winter(Map m, int seed, bool shadows)
        {
            var w = new Maps.W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = H("dfe8f2"); m.fogStart = 35; m.fogEnd = 140; m.bg = m.fog;
            m.ambSky = H("d8e6f5"); m.ambEq = H("b9c7d6"); m.ambGround = H("8fa0b2");
            Maps.Sun(gr, H("fff6ea"), 1.1f, new Vector3(30, 60, 0), shadows);
            var snow = Mats.Tex(Tex.Snow(), Color.white, 0.3f);
            var logs = Mats.Tex(Tex.Logs(), Color.white, 0.2f);
            var roof = Mats.Tex(Tex.Snow(), new Color(0.95f, 0.97f, 1f), 0.4f);
            var ice = Mats.Lit(H("a9d6f5"), 0.95f, 0.1f);
            var leaves = Mats.Tex(Tex.Leaves(), new Color(0.7f, 0.85f, 0.75f), 0.1f);
            var bark = Mats.Tex(Tex.Bark(), Color.white, 0.1f);
            var carrot = Mats.Lit(H("f07a1c"), 0.4f);
            var coal = Mats.Lit(H("111111"), 0.2f);
            var window = Mats.Emissive(null, H("ffd27a"), new Color(1.2f, 0.8f, 0.3f));
            Maps.Ground(gr, 240, snow, 5);
            // houses (log cabins with snowy gable roofs, lit windows)
            void House(float x, float z, float yaw)
            {
                var q = Quaternion.Euler(0, yaw, 0);
                w.Block(logs, x, z, 6, 8, 3.2f, yaw, 0, 2);
                for (int s = -1; s <= 1; s += 2)
                    w.g.Box(roof, new Vector3(x, 3.95f, z) + q * new Vector3(s * 1.7f, 0, 0), new Vector3(4.1f, 0.25f, 8.8f), q * Quaternion.Euler(0, 0, -s * 32));
                w.g.Box(logs, new Vector3(x, 3.6f, z) + q * new Vector3(0, 0, 3.95f), new Vector3(5.6f, 1.6f, 0.1f), q);
                w.g.Box(logs, new Vector3(x, 3.6f, z) + q * new Vector3(0, 0, -3.95f), new Vector3(5.6f, 1.6f, 0.1f), q);
                foreach (var lz in new[] { -2f, 2f }) foreach (var s in new[] { -1, 1 })
                        w.g.Box(window, new Vector3(x, 1.7f, z) + q * new Vector3(s * 3.01f, 0, lz), new Vector3(0.04f, 0.9f, 1.0f), q);
                w.g.Box(Mats.Lit(H("4a2a14"), 0.3f), new Vector3(x, 1.1f, z) + q * new Vector3(0, 0, 4.01f), new Vector3(1.1f, 2.2f, 0.05f), q);
                w.Block(Mats.Lit(H("6a6a6a"), 0.2f), (new Vector3(x, 0, z) + q * new Vector3(1.6f, 0, -2)).x, (new Vector3(x, 0, z) + q * new Vector3(1.6f, 0, -2)).z, 0.8f, 0.8f, 5.5f, yaw, 0, 1);   // chimney
            }
            House(-18, -14, 0); House(-20, 12, 90); House(16, -16, 90); House(19, 13, 0); House(-4, -24, 90); House(4, 24, 90);
            // frozen pond in the middle, snowmen, fences, trees
            w.g.Cyl(ice, new Vector3(0, 0, 0), 9, 0.04f, 32);
            void Snowman(float x, float z)
            {
                w.g.Mesh(snow, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, 0.6f, z), Quaternion.identity, Vector3.one * 1.3f));
                w.g.Mesh(snow, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, 1.45f, z), Quaternion.identity, Vector3.one * 0.95f));
                w.g.Mesh(snow, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, 2.1f, z), Quaternion.identity, Vector3.one * 0.62f));
                w.g.Cyl(carrot, new Vector3(x, 2.12f, z + 0.28f), 0.05f, 0.3f, 8, 0, true, 1, Quaternion.Euler(90, 0, 0));
                for (int e = -1; e <= 1; e += 2) w.g.Mesh(coal, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x + e * 0.1f, 2.22f, z + 0.27f), Quaternion.identity, Vector3.one * 0.07f));
                for (int b = 0; b < 3; b++) w.g.Mesh(coal, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, 1.25f + b * 0.2f, z + 0.46f), Quaternion.identity, Vector3.one * 0.08f));
                w.g.Cyl(coal, new Vector3(x, 2.35f, z), 0.25f, 0.35f, 12);
                w.Solid(new Vector3(x, 1.1f, z), new Vector3(1.1f, 2.2f, 1.1f));
            }
            Snowman(-8, 10); Snowman(9, -9); Snowman(12, 4); Snowman(-11, -4);
            var fence = Mats.Tex(Tex.Planks("6e4628"), Color.white, 0.2f);
            foreach (var (x, z, yaw) in new[] { (-6f, -12f, 0f), (7f, 12f, 0f), (-26f, 0f, 90f), (26f, 0f, 90f) }) w.Block(fence, x, z, 7, 0.25f, 1.3f, yaw, 0, 2);
            var r = new Rng(seed);
            for (int i = 0; i < 40; i++)
            {
                float x = r.F(-34, 34), z = r.F(-34, 34);
                if (new Vector2(x, z).magnitude < 12 || Mathf.Abs(x) < 30 && Mathf.Abs(z) < 30 && r.F() < 0.6f) continue;
                float s = r.F(0.9f, 1.4f);
                w.Trunk(bark, new Vector3(x, 0, z), 0.22f * s, 2 * s, 0.4f);
                w.g.Cyl(leaves, new Vector3(x, 1.1f * s, z), 1.9f * s, 2.4f * s, 9, 0);
                w.g.Cyl(snow, new Vector3(x, 2.4f * s, z), 1.5f * s, 2.1f * s, 9, 0);
                w.g.Cyl(leaves, new Vector3(x, 3.4f * s, z), 1.0f * s, 1.8f * s, 9, 0);
            }
            for (int i = 0; i < 12; i++) { float x = r.F(-30, 30), z = r.F(-30, 30); if (new Vector2(x, z).magnitude < 10) continue; w.g.Mesh(snow, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, 0, z), Quaternion.identity, new Vector3(3, 1.4f, 2.4f))); w.Solid(new Vector3(x, 0.3f, z), new Vector3(2.2f, 0.6f, 1.8f)); }   // snow drifts
            Bounds(w, 34, 34);
            Spawns(m, new float[,] { { -30, -28 }, { -24, -30 }, { -30, -20 }, { -16, -30 }, { -28, -10 }, { -10, -31 } });
            m.bounds = Rect.MinMaxRect(-34, -34, 34, 34);
            w.Done(shadows);
        }

        // ------------------------------------------------------------------ PIRATE SHIP
        public static void Ship(Map m, int seed, bool shadows)
        {
            var w = new Maps.W(m.root.transform);
            var gr = m.root.transform;
            m.sky = true;
            m.fog = H("cfe3f2"); m.fogStart = 70; m.fogEnd = 260; m.bg = m.fog;
            m.ambSky = H("bcd8f2"); m.ambEq = H("8fb5cf"); m.ambGround = H("2a5a7a");
            m.fallY = -2.5f;
            Maps.Sun(gr, H("fff2dc"), 1.35f, new Vector3(45, 120, 0), shadows);
            var deck = Mats.Tex(Tex.Planks("9a6a3a"), Color.white, 0.3f);
            var hullM = Mats.Tex(Tex.Planks("5a3418"), Color.white, 0.25f);
            var dark = Mats.Lit(H("2a1a0c"), 0.3f);
            var sail = Mats.Lit(H("f2ead6"), 0.1f);
            var iron = Mats.Lit(H("1b1b1b"), 0.5f, 0.7f);
            var rope = Mats.Lit(H("8a6a3a"), 0.2f);
            var water = Mats.Emissive(Tex.Water(), Color.white, new Color(0.05f, 0.12f, 0.2f));
            // the sea (no collider: falling overboard = respawn)
            var sea = new Geo();
            sea.Box(water, new Vector3(0, -3, 0), new Vector3(400, 0.1f, 400), Quaternion.identity, 10);
            sea.Build("Sea", gr, false, 0);
            const float L = 30, B = 7;
            // hull and deck
            w.Box(deck, new Vector3(0, -0.15f, 0), new Vector3(L * 2, 0.3f, B * 2), 0, 2);
            w.g.Box(hullM, new Vector3(0, -2.2f, 0), new Vector3(L * 2, 4, B * 2 - 0.4f), Quaternion.identity, 2);
            for (int i = 0; i < 4; i++) w.g.Box(hullM, new Vector3(L + 1.5f + i * 1.2f, -0.5f - i * 0.3f, 0), new Vector3(1.4f, 3 - i * 0.5f, B * 2 - 2 - i * 3), Quaternion.identity, 2);   // bow
            w.g.Rod(hullM, new Vector3(L + 4, 0.5f, 0), new Vector3(L + 12, 4, 0), 0.25f, 10);                                                                                // bowsprit
            // rails (low cover), raised stern deck with stairs, forecastle
            for (int s = -1; s <= 1; s += 2) { w.Block(hullM, 0, s * (B - 0.15f), L * 2, 0.3f, 1.1f, 0, 0, 2); for (int i = 0; i < 20; i++) w.g.Box(dark, new Vector3(-L + 1.5f + i * 3, 0.55f, s * (B + 0.02f)), new Vector3(0.12f, 1.1f, 0.05f), Quaternion.identity); }
            w.Block(hullM, -L, 0, 0.3f, B * 2, 1.1f, 0, 0, 2);
            w.Block(hullM, L, 0, 0.3f, B * 2, 1.1f, 0, 0, 2);
            w.Box(deck, new Vector3(-L + 5, 1.25f, 0), new Vector3(10, 2.5f, B * 2 - 0.6f), 0, 2);       // stern castle (top 2.5)
            w.Box(deck, new Vector3(L - 4, 1.0f, 0), new Vector3(8, 2.0f, B * 2 - 0.6f), 0, 2);          // forecastle (top 2.0)
            for (int i = 0; i < 6; i++) { float t = 0.42f * (i + 1); w.Box(deck, new Vector3(-L + 10.2f + (5 - i) * 0.5f, t / 2, -4), new Vector3(0.5f, t, 2), 0, 1); }
            for (int i = 0; i < 5; i++) { float t = 0.42f * (i + 1); w.Box(deck, new Vector3(L - 8.2f - (4 - i) * 0.5f, t / 2, 4), new Vector3(0.5f, t, 2), 0, 1); }
            w.g.Box(Mats.Glow(new Color(1.4f, 1.1f, 0.6f)), new Vector3(-L + 0.2f, 1.6f, 0), new Vector3(0.05f, 0.8f, 2.4f), Quaternion.identity);   // stern windows
            // masts with sails and ropes
            foreach (var mx in new[] { -10f, 6f, 20f })
            {
                float h = mx == 6 ? 18 : 14;
                w.Trunk(hullM, new Vector3(mx, 0, 0), 0.35f, h, 0.45f);
                for (int k = 0; k < 2; k++)
                {
                    float y = 5 + k * (h - 7) / 1.2f;
                    w.g.Rod(hullM, new Vector3(mx, y + 4, -5), new Vector3(mx, y + 4, 5), 0.15f, 8);
                    w.g.Box(sail, new Vector3(mx + 0.3f, y + 2, 0), new Vector3(0.05f, 4, 9.6f - k * 1.5f), Quaternion.Euler(0, 0, 4));
                }
                w.g.Cyl(dark, new Vector3(mx, h * 0.8f, 0), 1.0f, 0.6f, 12);   // crow's nest
                for (int s = -1; s <= 1; s += 2) { w.g.Rod(rope, new Vector3(mx, h * 0.8f, 0), new Vector3(mx - 3, 0.5f, s * (B - 0.3f)), 0.04f, 6); w.g.Rod(rope, new Vector3(mx, h * 0.8f, 0), new Vector3(mx + 3, 0.5f, s * (B - 0.3f)), 0.04f, 6); }
            }
            // cannons along the sides, barrels and crates as cover
            for (int i = 0; i < 6; i++)
                foreach (var s in new[] { -1, 1 })
                {
                    float x = -14 + i * 5.5f;
                    w.Block(dark, x, s * (B - 1.3f), 1.2f, 1.6f, 0.5f, 0, 0, 1);
                    w.g.Cyl(iron, new Vector3(x, 0.7f, s * (B - 2)), 0.22f, 2.2f, 12, 0.16f, true, 1, Quaternion.Euler(-s * 90, 0, 0));
                    w.Solid(new Vector3(x, 0.5f, s * (B - 1.5f)), new Vector3(1, 1, 2));
                }
            var barrelM = Mats.Tex(Tex.Planks("7a4a24"), Color.white, 0.3f);
            var crate = Mats.Tex(Tex.Planks("a97a45"), Color.white, 0.3f);
            foreach (var (x, z) in new[] { (-4f, 2f), (-3f, -2.5f), (12f, -1f), (13f, 2.5f), (1f, 0f), (26f, -3f) })
            {
                w.g.Cyl(barrelM, new Vector3(x, 0, z), 0.45f, 1.1f, 14);
                w.g.Cyl(iron, new Vector3(x, 0.2f, z), 0.47f, 0.06f, 14); w.g.Cyl(iron, new Vector3(x, 0.85f, z), 0.47f, 0.06f, 14);
                w.Solid(new Vector3(x, 0.55f, z), new Vector3(0.9f, 1.1f, 0.9f));
            }
            foreach (var (x, z) in new[] { (-8f, 3.5f), (16f, -3.5f), (8f, 4f), (-16f, -3.5f) }) { w.Block(crate, x, z, 1.4f, 1.4f, 1.4f, 15, 0, 1.4f); w.Block(crate, x, z, 1.0f, 1.0f, 1.0f, 40, 1.4f, 1.0f); }
            // jolly roger
            var flag = Prim.Make(PrimitiveType.Quad, gr, new Vector3(6, 19.2f, 0.9f), new Vector3(0.05f, 1.2f, 1.8f), Mats.Lit(H("111111"), 0.1f), "Flag");
            flag.transform.localScale = new Vector3(1.8f, 1.2f, 1); flag.transform.rotation = Quaternion.Euler(0, 90, 0);
            Object.Destroy(flag.GetComponent<Collider>());
            for (int i = 0; i < 6; i++)
            {
                m.A.Add(Maps.SP(-18 + (i % 2) * 2.5f, -4 + i * 1.6f, 0, 0));
                m.B.Add(Maps.SP(18 - (i % 2) * 2.5f, -4 + i * 1.6f, 0, 0));
            }
            m.bounds = Rect.MinMaxRect(-L - 1, -B - 1, L + 1, B + 1);
            w.Done(shadows);
        }
    }
}
