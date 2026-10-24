using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>How many separate parts a built weapon model has (shown in the shop, checked by the autotest).</summary>
    public class WeaponInfo : MonoBehaviour { public int Parts; }

    /// <summary>
    /// Detailed procedural weapons (barrel along +Z, a "Muzzle" child). Every model is assembled from
    /// 100+ parts — rails with ridges, screws and rivets, vents, ribbed magazines, sights, scopes — and
    /// baked into one mesh per material, so the detail costs only a few draw calls.
    /// </summary>
    public static class WeaponModels
    {
        static Material metal, dark, wood, grip, steel, olive, glass, brass, red, rocketMat, rubber;

        static void Init()
        {
            if (metal != null) return;
            metal = Mats.Lit(new Color(0.17f, 0.18f, 0.2f), 0.55f, 0.85f);
            dark = Mats.Lit(new Color(0.06f, 0.065f, 0.075f), 0.4f, 0.6f);
            wood = Mats.Tex(Tex.Planks("8a4f22"), Color.white, 0.35f);
            grip = Mats.Lit(new Color(0.09f, 0.09f, 0.09f), 0.15f);
            rubber = Mats.Lit(new Color(0.05f, 0.05f, 0.05f), 0.05f);
            steel = Mats.Lit(new Color(0.82f, 0.84f, 0.87f), 0.85f, 1f);
            olive = Mats.Lit(new Color(0.29f, 0.33f, 0.19f), 0.25f);
            glass = Mats.Emissive(null, new Color(0.15f, 0.35f, 0.6f), new Color(0.05f, 0.15f, 0.3f), null, 0.95f);
            brass = Mats.Lit(new Color(0.8f, 0.6f, 0.25f), 0.7f, 0.9f);
            red = Mats.Glow(new Color(1.6f, 0.1f, 0.1f));
            rocketMat = Mats.Lit(new Color(0.3f, 0.36f, 0.22f), 0.3f);
        }

        /// <summary>The model builder: every call adds one part.</summary>
        class K
        {
            public readonly Geo g = new Geo();
            public int parts;
            public Vector3 muzzle = new Vector3(0, 0, 0.5f);
            public Material body, wd, blade, tube, skin;
            const float T = 0.2f;

            public void Box(Material m, float x, float y, float z, float sx, float sy, float sz, float rx = 0, float ry = 0, float rz = 0)
            { g.Box(m, new Vector3(x, y, z), new Vector3(sx, sy, sz), Quaternion.Euler(rx, ry, rz), T); parts++; }

            /// <summary>Cylinder along +Z starting at z.</summary>
            public void CZ(Material m, float x, float y, float z, float r, float len, float rTop = -1, int seg = 14)
            { g.Cyl(m, new Vector3(x, y, z), r, len, seg, rTop, true, T, Quaternion.Euler(90, 0, 0)); parts++; }

            /// <summary>Cylinder along +X centred at (x,y,z).</summary>
            public void CX(Material m, float x, float y, float z, float r, float len, int seg = 10)
            { g.Cyl(m, new Vector3(x - len / 2, y, z), r, len, seg, -1, true, T, Quaternion.Euler(0, 0, -90)); parts++; }

            /// <summary>Vertical cylinder standing on (x,y,z).</summary>
            public void CY(Material m, float x, float y, float z, float r, float h, int seg = 12, float rTop = -1)
            { g.Cyl(m, new Vector3(x, y, z), r, h, seg, rTop, true, T); parts++; }

            public void Ball(Material m, float x, float y, float z, float sx, float sy, float sz)
            { g.Mesh(m, Prim.MeshOf(PrimitiveType.Sphere), Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.identity, new Vector3(sx, sy, sz))); parts++; }

            public void Ext(Material m, Vector2[] poly, float depth, Vector3 pos, Quaternion rot, float uv = 6)
            { g.Extrude(m, poly, depth, Matrix4x4.TRS(pos, rot, Vector3.one), uv); parts++; }

            // ------------------------------------------------ detail kits

            /// <summary>Picatinny rail: base + a ridge every 1 cm.</summary>
            public void Rail(float y, float z0, float z1, float w = 0.022f)
            {
                Box(dark, 0, y, (z0 + z1) / 2, w, 0.008f, z1 - z0);
                for (float z = z0 + 0.006f; z < z1 - 0.004f; z += 0.01f) Box(dark, 0, y + 0.006f, z, w + 0.004f, 0.005f, 0.005f);
            }

            /// <summary>Screw / rivet heads on both sides of a part.</summary>
            public void Rivets(float halfW, float y, float z0, float z1, int n, float r = 0.0045f, Material m = null)
            {
                for (int i = 0; i < n; i++)
                {
                    float z = n == 1 ? (z0 + z1) / 2 : Mathf.Lerp(z0, z1, i / (float)(n - 1));
                    CX(m ?? metal, halfW + 0.0015f, y, z, r, 0.004f, 8);
                    CX(m ?? metal, -halfW - 0.0015f, y, z, r, 0.004f, 8);
                }
            }

            /// <summary>Slots cut in a handguard / slide (dark inserts on both sides).</summary>
            public void Slots(float halfW, float y, float z0, float z1, int n, float h = 0.012f, float len = 0.008f, float tilt = 0)
            {
                for (int i = 0; i < n; i++)
                {
                    float z = Mathf.Lerp(z0, z1, (i + 0.5f) / n);
                    Box(dark, halfW + 0.001f, y, z, 0.004f, h, len, tilt);
                    Box(dark, -halfW - 0.001f, y, z, 0.004f, h, len, tilt);
                }
            }

            /// <summary>Grip with finger ridges and a screw.</summary>
            public void Grip(Material m, float y, float z, float tilt, float h = 0.12f, float w = 0.04f, float d = 0.055f, int ridges = 6)
            {
                Box(m, 0, y, z, w, h, d, tilt);
                var q = Quaternion.Euler(tilt, 0, 0);
                for (int i = 0; i < ridges; i++)
                {
                    var p = new Vector3(0, y, z) + q * new Vector3(0, -h / 2 + h * (i + 0.5f) / ridges, d / 2 + 0.002f);
                    Box(rubber, p.x, p.y, p.z, w * 0.9f, h / ridges * 0.45f, 0.006f, tilt);
                }
                var sp = new Vector3(0, y, z);
                CX(metal, sp.x, sp.y, sp.z, 0.006f, w + 0.006f, 8);
            }

            /// <summary>Trigger with guard (3 bars) and pin.</summary>
            public void Trigger(float y, float z, float len = 0.06f)
            {
                Box(dark, 0, y - 0.03f, z, 0.012f, 0.006f, len);
                Box(dark, 0, y - 0.015f, z + len / 2, 0.012f, 0.03f, 0.006f);
                Box(dark, 0, y - 0.015f, z - len / 2, 0.012f, 0.03f, 0.006f, 10);
                Box(metal, 0, y - 0.012f, z + 0.005f, 0.006f, 0.024f, 0.006f, -15);
                CX(metal, 0, y, z + 0.012f, 0.003f, 0.03f, 6);
            }

            /// <summary>Box magazine with ribs, base plate and follower; curve tilts each segment.</summary>
            public void Mag(Material m, float y, float z, float len, float w, float d, int segs, float curve, int ribs)
            {
                var p = new Vector3(0, y, z);
                float seg = len / segs;
                for (int i = 0; i < segs; i++)
                {
                    float a = curve * i;
                    var q = Quaternion.Euler(-a, 0, 0);
                    var c = p + q * new Vector3(0, -seg / 2, 0);
                    Box(m, c.x, c.y, c.z, w, seg * 1.05f, d, -a);
                    if (i % Mathf.Max(1, segs / ribs) == 0)
                    {
                        Box(dark, w / 2 + 0.001f, c.y, c.z, 0.003f, seg * 0.7f, d * 0.5f, -a);
                        Box(dark, -w / 2 - 0.001f, c.y, c.z, 0.003f, seg * 0.7f, d * 0.5f, -a);
                    }
                    p += q * new Vector3(0, -seg, 0);
                }
                var qe = Quaternion.Euler(-curve * segs, 0, 0);
                Box(dark, p.x, p.y - 0.004f, p.z, w + 0.008f, 0.01f, d + 0.012f, -curve * segs);
                var lip = new Vector3(0, y + 0.004f, z);
                Box(metal, lip.x, lip.y, lip.z, w * 0.7f, 0.006f, d * 0.8f);
                Box(brass, 0, y + 0.01f, z, w * 0.4f, 0.008f, d * 0.6f);
            }

            /// <summary>Muzzle brake with ports, ends at z+len.</summary>
            public void Brake(float y, float z, float r, float len, int ports)
            {
                CZ(dark, 0, y, z, r, len);
                CZ(metal, 0, y, z, r * 1.08f, 0.008f);
                CZ(metal, 0, y, z + len - 0.008f, r * 1.08f, 0.008f);
                for (int i = 0; i < ports; i++)
                {
                    float a = i * 360f / ports;
                    var o = Quaternion.Euler(0, 0, a) * new Vector3(r * 0.9f, 0, 0);
                    Box(rubber, o.x, y + o.y, z + len * 0.5f, 0.006f, 0.006f, len * 0.55f, 0, 0, a);
                }
            }

            /// <summary>Barrel with flutes.</summary>
            public void Barrel(float y, float z, float r, float len, int flutes)
            {
                CZ(metal, 0, y, z, r, len);
                for (int i = 0; i < flutes; i++)
                {
                    float a = i * 360f / flutes;
                    var o = Quaternion.Euler(0, 0, a) * new Vector3(r * 0.95f, 0, 0);
                    Box(dark, o.x, y + o.y, z + len * 0.5f, 0.003f, 0.003f, len * 0.6f, 0, 0, a);
                }
            }

            /// <summary>Telescopic sight: tube, bells, lenses, rings with screws, turrets with clicks.</summary>
            public void Scope(float y, float z, float len, float r, int rings = 2)
            {
                CZ(dark, 0, y, z - len / 2, r, len);
                CZ(dark, 0, y, z + len / 2 - 0.02f, r * 1.5f, 0.07f, r * 1.15f);
                CZ(dark, 0, y, z - len / 2 - 0.05f, r * 1.3f, 0.06f);
                CZ(glass, 0, y, z + len / 2 + 0.048f, r * 1.4f, 0.004f);
                CZ(glass, 0, y, z - len / 2 - 0.054f, r * 1.2f, 0.004f);
                CZ(rubber, 0, y, z - len / 2 - 0.07f, r * 1.32f, 0.02f);
                for (int i = 0; i < rings; i++)
                {
                    float rz = z + (i == 0 ? -len * 0.28f : len * 0.28f);
                    CZ(metal, 0, y, rz - 0.009f, r * 1.25f, 0.018f);
                    Box(metal, 0, y - r * 1.4f, rz, r * 1.1f, r * 1.2f, 0.018f);
                    for (int s = -1; s <= 1; s += 2) { CX(metal, s * r * 0.75f, y - r * 1.1f, rz, 0.004f, 0.01f, 6); CX(metal, s * r * 0.75f, y + r * 0.2f, rz, 0.004f, 0.01f, 6); }
                }
                CY(metal, 0, y + r, z, r * 0.55f, r * 0.9f);
                CX(metal, r + r * 0.4f, y, z, r * 0.55f, r * 0.9f);
                for (int i = 0; i < 8; i++)
                {
                    float a = i * 45 * Mathf.Deg2Rad;
                    Box(dark, Mathf.Cos(a) * r * 0.56f, y + r + r * 0.6f, z + Mathf.Sin(a) * r * 0.56f, 0.003f, r * 0.5f, 0.003f, 0, -i * 45);
                    Box(dark, r + r * 0.9f, y + Mathf.Cos(a) * r * 0.56f, z + Mathf.Sin(a) * r * 0.56f, r * 0.5f, 0.003f, 0.003f);
                }
                CY(metal, -r * 1.3f, y - 0.004f, z + 0.02f, r * 0.35f, r * 0.6f);
            }

            /// <summary>Red-dot sight on a mount.</summary>
            public void RedDot(float y, float z)
            {
                Box(dark, 0, y + 0.01f, z, 0.03f, 0.012f, 0.05f);
                Box(dark, 0.017f, y + 0.035f, z, 0.006f, 0.045f, 0.045f);
                Box(dark, -0.017f, y + 0.035f, z, 0.006f, 0.045f, 0.045f);
                Box(dark, 0, y + 0.06f, z, 0.04f, 0.006f, 0.045f);
                Box(glass, 0, y + 0.035f, z + 0.01f, 0.028f, 0.04f, 0.003f);
                Ball(red, 0, y + 0.035f, z + 0.012f, 0.004f, 0.004f, 0.004f);
                CY(metal, 0.022f, y + 0.03f, z - 0.01f, 0.007f, 0.012f);
                for (int i = 0; i < 4; i++) CX(metal, 0.021f, y + 0.008f, z - 0.018f + i * 0.012f, 0.003f, 0.004f, 6);
            }

            /// <summary>Folding bipod legs.</summary>
            public void Bipod(float y, float z, float len)
            {
                Box(dark, 0, y, z, 0.04f, 0.02f, 0.03f);
                for (int s = -1; s <= 1; s += 2)
                {
                    g.Rod(dark, new Vector3(s * 0.015f, y - 0.01f, z), new Vector3(s * 0.08f, y - len, z + 0.04f), 0.006f, 8); parts++;
                    g.Rod(rubber, new Vector3(s * 0.08f, y - len, z + 0.04f), new Vector3(s * 0.085f, y - len - 0.02f, z + 0.045f), 0.01f, 8); parts++;
                    CX(metal, s * 0.022f, y - 0.005f, z, 0.005f, 0.006f, 6);
                }
            }

            /// <summary>Serrated edge: little teeth along the spine of a blade (in blade space: x = length).</summary>
            public void Teeth(Material m, Quaternion rot, Vector3 at, float x0, float x1, float y, int n, float size = 0.006f)
            {
                for (int i = 0; i < n; i++)
                {
                    float x = Mathf.Lerp(x0, x1, (i + 0.5f) / n);
                    var tri = new[] { new Vector2(x - size * 0.5f, y), new Vector2(x + size * 0.5f, y), new Vector2(x, y + size) };
                    Ext(m, tri, 0.003f, at, rot, 8);
                }
            }

            /// <summary>Engraved markings: thin grooves on both sides along Z.</summary>
            public void Engrave(float halfW, float y, float z0, float z1, int n, float h = 0.003f)
            {
                for (int i = 0; i < n; i++)
                {
                    float z = Mathf.Lerp(z0, z1, (i + 0.5f) / n), len = (z1 - z0) / n * (0.4f + 0.5f * ((i * 7) % 3) / 2f);
                    Box(dark, halfW + 0.0008f, y + ((i % 2) - 0.5f) * h * 2, z, 0.0015f, h, len);
                    Box(dark, -halfW - 0.0008f, y + ((i % 2) - 0.5f) * h * 2, z, 0.0015f, h, len);
                }
            }

            /// <summary>Wrapped handle: rings / diamonds along an axis (along -Z from z0).</summary>
            public void Wrap(Material m, float y, float z0, float len, float r, int n, bool diamonds)
            {
                for (int i = 0; i < n; i++)
                {
                    float z = z0 - len * (i + 0.5f) / n;
                    if (diamonds) { Box(m, 0, y + r * 0.9f, z, r * 1.1f, 0.004f, len / n * 0.7f, 0, 45); Box(m, 0, y - r * 0.9f, z, r * 1.1f, 0.004f, len / n * 0.7f, 0, 45); }
                    else CZ(m, 0, y, z - len / n * 0.25f, r * 1.08f, len / n * 0.5f);
                }
            }
        }

        static readonly Quaternion BladeRot = Quaternion.Euler(0, -90, 0);   // shape x → +Z

        public static GameObject Build(string id, string skin, Transform parent, int layer)
        {
            Init();
            var S = skin != null && skin != "std" ? Tex.SkinMat(skin) : null;
            var k = new K { skin = S, body = S ?? metal, wd = S ?? wood, blade = S ?? steel, tube = S ?? olive };
            switch (id)
            {
                case "ak74": AK(k); break;
                case "m4a1": M4(k); break;
                case "mp5": MP5(k); break;
                case "pkm": PKM(k); break;
                case "shotgun": Shotgun(k, false); break;
                case "saiga": Shotgun(k, true); break;
                case "svd": SVD(k); break;
                case "mosin": Mosin(k); break;
                case "awp": AWP(k); break;
                case "rpg7": RPG(k); break;
                case "tt": Pistol(k, 0); break;
                case "glock": Pistol(k, 1); break;
                case "colt": Pistol(k, 2); break;
                case "deagle": Pistol(k, 3); break;
                case "uzi": Uzi(k); break;
                case "nagant": Revolver(k); break;
                case "kabar": Knife(k, 0); break;
                case "butterfly": Knife(k, 1); break;
                case "karambit": Karambit(k); break;
                case "machete": Knife(k, 2); break;
                case "katana": Katana(k); break;
                case "shuriken": Shuriken(k); break;
                case "kunai": Kunai(k); break;
                case "axe": Axe(k); break;
                case "f1": Grenade(k, false); break;
                case "rgd5": Grenade(k, true); break;
            }
            var root = new GameObject(id);
            root.transform.SetParent(parent, false);
            k.g.Build("Mesh", root.transform, true, layer);
            var mz = new GameObject("Muzzle").transform;
            mz.SetParent(root.transform, false);
            mz.localPosition = k.muzzle;
            root.AddComponent<WeaponInfo>().Parts = k.parts;
            Prim.SetLayer(root, layer);
            return root;
        }

        // ================================================================== rifles

        static void AK(K k)
        {
            k.Box(k.body, 0, 0, 0, 0.07f, 0.1f, 0.42f);
            k.Box(k.body, 0, 0.065f, -0.02f, 0.066f, 0.035f, 0.36f);
            for (int i = 0; i < 9; i++) k.Box(k.body, 0, 0.083f, -0.17f + i * 0.035f, 0.068f, 0.004f, 0.006f);   // ribbed dust cover
            k.Rivets(0.035f, -0.02f, -0.17f, 0.17f, 6);
            k.Rivets(0.035f, 0.03f, -0.12f, 0.12f, 3);
            k.Box(k.body, 0.036f, 0.0f, 0.06f, 0.006f, 0.02f, 0.12f, 0, 0, 0);                                     // selector lever
            k.Box(k.body, 0.038f, -0.012f, 0.0f, 0.004f, 0.012f, 0.03f);
            k.Box(k.body, 0.04f, 0.04f, 0.1f, 0.012f, 0.012f, 0.03f);                                                // charging handle
            k.CX(k.body, 0.05f, 0.04f, 0.11f, 0.008f, 0.012f);
            k.Box(k.body, 0, 0.075f, 0.2f, 0.05f, 0.03f, 0.05f);                                                      // rear sight block
            k.Box(k.body, 0, 0.095f, 0.2f, 0.03f, 0.008f, 0.06f, 4);
            k.Box(dark, 0, 0.1f, 0.18f, 0.012f, 0.008f, 0.01f);
            k.Barrel(0.025f, 0.2f, 0.012f, 0.42f, 4);
            k.Brake(0.025f, 0.62f, 0.02f, 0.07f, 6);
            k.Box(k.body, 0, 0.035f, 0.57f, 0.025f, 0.04f, 0.04f);                                                    // front sight base
            k.Box(dark, 0, 0.065f, 0.57f, 0.004f, 0.03f, 0.004f);
            k.Box(k.body, 0.012f, 0.06f, 0.57f, 0.004f, 0.025f, 0.012f); k.Box(k.body, -0.012f, 0.06f, 0.57f, 0.004f, 0.025f, 0.012f);
            k.Box(k.body, 0, 0.05f, 0.42f, 0.03f, 0.03f, 0.04f);                                                       // gas block
            k.CZ(k.wd, 0, 0.062f, 0.21f, 0.015f, 0.21f);                                                              // gas tube cover
            for (int i = 0; i < 4; i++) k.CX(rubber, 0, 0.066f, 0.26f + i * 0.035f, 0.004f, 0.034f, 6);
            k.Box(k.wd, 0, 0, 0.31f, 0.078f, 0.078f, 0.22f);                                                          // handguard
            k.Slots(0.039f, -0.01f, 0.22f, 0.4f, 5, 0.03f, 0.006f);
            k.Box(k.body, 0, 0, 0.205f, 0.082f, 0.082f, 0.012f); k.Box(k.body, 0, 0, 0.415f, 0.082f, 0.082f, 0.012f);   // retainers
            k.CZ(metal, 0, -0.03f, 0.2f, 0.004f, 0.42f);                                                              // cleaning rod
            k.Box(metal, 0, -0.025f, 0.58f, 0.012f, 0.02f, 0.03f);                                                    // bayonet lug
            k.Mag(k.body, -0.05f, 0.09f, 0.26f, 0.05f, 0.085f, 8, 4.5f, 8);
            k.Box(k.body, 0, -0.055f, 0.04f, 0.02f, 0.015f, 0.02f);                                                  // mag release
            k.Trigger(-0.05f, -0.02f);
            k.Grip(k.wd, -0.11f, -0.13f, 20, 0.13f, 0.045f, 0.055f, 7);
            k.Box(k.wd, 0, -0.03f, -0.36f, 0.055f, 0.1f, 0.32f, -4);                                                  // stock
            k.Box(k.body, 0, -0.045f, -0.525f, 0.06f, 0.13f, 0.012f, -4);                                             // butt plate
            k.Rivets(0.03f, -0.045f, -0.52f, -0.52f, 1);
            k.CX(metal, 0, -0.08f, -0.28f, 0.01f, 0.06f); k.Box(metal, 0, -0.095f, -0.28f, 0.02f, 0.012f, 0.012f);   // sling swivel
            k.CX(metal, 0, 0.0f, 0.45f, 0.008f, 0.05f);
            k.muzzle = new Vector3(0, 0.025f, 0.7f);
        }

        static void M4(K k)
        {
            k.Box(k.body, 0, 0.02f, 0.02f, 0.06f, 0.06f, 0.26f);                                                      // upper
            k.Box(k.body, 0, -0.03f, 0.0f, 0.056f, 0.05f, 0.22f);                                                     // lower
            k.Rail(0.055f, -0.1f, 0.14f);
            k.Box(k.body, 0, 0.0f, 0.3f, 0.064f, 0.064f, 0.26f);                                                     // handguard
            k.Slots(0.032f, 0.0f, 0.18f, 0.42f, 7, 0.022f, 0.012f);
            k.Rail(0.034f, 0.18f, 0.42f, 0.02f);
            k.Barrel(0.0f, 0.43f, 0.011f, 0.17f, 4);
            k.Brake(0.0f, 0.6f, 0.016f, 0.06f, 6);
            k.Box(k.body, 0, 0.04f, 0.47f, 0.02f, 0.05f, 0.025f);                                                     // front sight
            k.Box(dark, 0, 0.07f, 0.47f, 0.004f, 0.02f, 0.004f);
            k.RedDot(0.06f, 0.02f);
            k.Box(k.body, 0.03f, 0.03f, 0.03f, 0.012f, 0.03f, 0.03f);                                                 // ejection port cover / forward assist
            k.CZ(k.body, 0.03f, 0.04f, -0.06f, 0.008f, 0.03f);
            k.Box(k.body, 0, 0.04f, -0.14f, 0.03f, 0.015f, 0.03f);                                                    // charging handle
            k.Mag(k.body, -0.05f, 0.06f, 0.2f, 0.028f, 0.07f, 6, 2.5f, 6);
            k.Trigger(-0.055f, -0.03f);
            k.Grip(grip, -0.1f, -0.1f, 22, 0.12f, 0.035f, 0.05f, 6);
            k.CZ(k.body, 0, 0.0f, -0.32f, 0.016f, 0.22f);                                                             // buffer tube
            for (int i = 0; i < 6; i++) k.Box(dark, 0, -0.016f, -0.3f + i * 0.03f, 0.012f, 0.006f, 0.008f);
            k.Box(k.wd, 0, -0.02f, -0.38f, 0.045f, 0.09f, 0.16f);                                                     // collapsible stock
            k.Box(rubber, 0, -0.02f, -0.465f, 0.047f, 0.1f, 0.012f);
            for (int i = 0; i < 5; i++) k.Box(rubber, 0, -0.06f + i * 0.02f, -0.473f, 0.045f, 0.005f, 0.004f);
            k.Rivets(0.03f, -0.03f, -0.08f, 0.08f, 4);
            k.Rivets(0.023f, -0.01f, -0.42f, -0.34f, 2);
            k.muzzle = new Vector3(0, 0, 0.67f);
        }

        static void MP5(K k)
        {
            k.CZ(k.body, 0, 0.02f, -0.14f, 0.032f, 0.34f);                                                            // round receiver
            for (int i = 0; i < 10; i++) k.Box(dark, 0, 0.053f, -0.12f + i * 0.03f, 0.03f, 0.004f, 0.004f);
            k.Box(k.wd, 0, -0.005f, 0.24f, 0.06f, 0.06f, 0.14f);                                                      // handguard
            k.Slots(0.03f, -0.005f, 0.18f, 0.3f, 6, 0.018f, 0.008f);
            k.Barrel(0.02f, 0.2f, 0.01f, 0.17f, 4);
            k.CZ(dark, 0, 0.02f, 0.35f, 0.016f, 0.04f);
            for (int i = 0; i < 3; i++) k.CZ(metal, 0, 0.02f, 0.355f + i * 0.012f, 0.018f, 0.004f);
            k.CY(k.body, 0, 0.05f, 0.29f, 0.012f, 0.03f);                                                             // front sight hood
            k.Box(k.body, 0, 0.065f, -0.12f, 0.04f, 0.025f, 0.03f);                                                   // drum rear sight
            k.CX(dark, 0, 0.08f, -0.12f, 0.012f, 0.03f);
            k.Box(k.body, 0.03f, 0.04f, 0.12f, 0.01f, 0.012f, 0.04f);                                                 // cocking handle
            k.Mag(k.body, -0.03f, 0.05f, 0.2f, 0.026f, 0.05f, 7, 3.5f, 7);
            k.Box(k.body, 0, -0.035f, -0.04f, 0.045f, 0.04f, 0.18f);                                                  // trigger group
            k.Trigger(-0.06f, -0.06f);
            k.Grip(grip, -0.11f, -0.1f, 15, 0.11f, 0.04f, 0.05f, 6);
            k.Box(k.body, 0.025f, -0.03f, -0.04f, 0.006f, 0.015f, 0.03f);                                             // selector
            for (int s = -1; s <= 1; s += 2) k.Box(k.body, s * 0.025f, 0.0f, -0.36f, 0.006f, 0.03f, 0.22f);         // retractable stock arms
            k.Box(rubber, 0, -0.01f, -0.47f, 0.06f, 0.09f, 0.015f);
            k.Rivets(0.032f, 0.0f, -0.1f, 0.15f, 6);
            k.Rivets(0.023f, -0.035f, -0.12f, 0.03f, 4);
            k.Engrave(0.032f, 0.03f, -0.12f, 0.12f, 5);
            k.muzzle = new Vector3(0, 0.02f, 0.4f);
        }

        static void PKM(K k)
        {
            k.Box(k.body, 0, 0.02f, 0, 0.075f, 0.1f, 0.4f);
            k.Box(k.body, 0, 0.08f, -0.02f, 0.07f, 0.03f, 0.3f);                                                      // feed cover
            for (int i = 0; i < 10; i++) k.Box(k.body, 0, 0.097f, -0.15f + i * 0.03f, 0.072f, 0.004f, 0.006f);
            k.Rivets(0.0375f, 0.0f, -0.17f, 0.17f, 7);
            k.Barrel(0.03f, 0.2f, 0.014f, 0.58f, 6);
            k.Brake(0.03f, 0.78f, 0.022f, 0.08f, 6);
            k.CZ(k.body, 0, -0.01f, 0.2f, 0.012f, 0.32f);                                                             // gas tube
            k.Box(k.body, 0, 0.0f, 0.52f, 0.03f, 0.05f, 0.04f);
            k.Box(k.body, 0, 0.11f, 0.25f, 0.02f, 0.06f, 0.012f);                                                     // carry handle post
            k.Box(k.wd, 0, 0.15f, 0.2f, 0.022f, 0.018f, 0.12f);
            k.Box(k.body, 0, 0.06f, 0.75f, 0.02f, 0.05f, 0.02f);                                                      // front sight
            k.Bipod(-0.02f, 0.55f, 0.24f);
            k.Box(k.body, -0.08f, -0.05f, 0.02f, 0.09f, 0.12f, 0.12f);                                                // ammo box
            k.Box(dark, -0.08f, 0.012f, 0.02f, 0.092f, 0.006f, 0.122f);
            k.CX(metal, -0.08f, 0.02f, 0.02f, 0.006f, 0.05f);
            for (int i = 0; i < 12; i++)                                                                                 // belt: 12 rounds with links
            {
                float t = i / 11f;
                var p = Vector3.Lerp(new Vector3(-0.04f, 0.04f, 0.02f), new Vector3(-0.08f, 0.0f, 0.02f), t);
                k.CX(brass, p.x - 0.01f, p.y, p.z - 0.04f + 0.0f, 0.005f, 0.04f, 6);
                k.Box(dark, p.x, p.y + 0.006f, p.z - 0.04f, 0.012f, 0.004f, 0.008f);
            }
            k.Box(metal, -0.035f, 0.05f, 0.02f, 0.03f, 0.008f, 0.06f); k.Box(metal, 0.04f, 0.05f, 0.02f, 0.012f, 0.008f, 0.05f);   // feed tray
            k.Trigger(-0.05f, -0.06f);
            k.Grip(k.wd, -0.11f, -0.1f, 18, 0.12f, 0.042f, 0.055f, 7);
            k.Box(k.wd, 0, -0.01f, -0.35f, 0.045f, 0.11f, 0.3f, -3);                                                  // skeleton stock
            k.Box(dark, 0, -0.01f, -0.33f, 0.047f, 0.05f, 0.16f);
            k.Box(rubber, 0, -0.015f, -0.505f, 0.05f, 0.12f, 0.012f);
            k.CX(metal, 0, -0.07f, -0.3f, 0.01f, 0.05f);
            k.muzzle = new Vector3(0, 0.03f, 0.86f);
        }

        static void Shotgun(K k, bool saiga)
        {
            if (!saiga)
            {
                k.Box(k.body, 0, 0.0f, 0.0f, 0.055f, 0.08f, 0.26f);
                k.Rivets(0.0275f, 0.0f, -0.1f, 0.1f, 4);
                k.Barrel(0.03f, 0.12f, 0.016f, 0.55f, 0);
                k.Rail(0.05f, 0.14f, 0.64f, 0.012f);                                                                   // vent rib
                k.Ball(brass, 0, 0.057f, 0.66f, 0.008f, 0.008f, 0.008f);
                k.CZ(k.body, 0, -0.012f, 0.12f, 0.014f, 0.48f);                                                        // tube magazine
                k.CZ(k.body, 0, -0.012f, 0.6f, 0.017f, 0.02f);
                k.CZ(k.wd, 0, -0.012f, 0.22f, 0.026f, 0.16f);                                                          // pump
                for (int i = 0; i < 9; i++) k.CZ(rubber, 0, -0.012f, 0.23f + i * 0.016f, 0.0275f, 0.006f);
                k.Box(k.body, 0.0285f, 0.0f, 0.04f, 0.004f, 0.025f, 0.06f);                                            // ejection port
                for (int i = 0; i < 6; i++)                                                                              // side saddle shells
                {
                    k.CY(Mats.Lit(new Color(0.75f, 0.1f, 0.1f), 0.4f), 0.034f, -0.03f, -0.07f + i * 0.022f, 0.009f, 0.045f, 8);
                    k.CY(brass, 0.034f, -0.035f, -0.07f + i * 0.022f, 0.0095f, 0.008f, 8);
                }
                k.Box(dark, 0.03f, -0.005f, -0.016f, 0.006f, 0.06f, 0.14f);
                k.Trigger(-0.04f, -0.06f);
                k.Box(k.wd, 0, -0.04f, -0.3f, 0.05f, 0.1f, 0.34f, -8);                                                 // stock
                k.Box(k.wd, 0, -0.07f, -0.14f, 0.045f, 0.08f, 0.08f, 25);
                k.Box(rubber, 0, -0.07f, -0.47f, 0.052f, 0.13f, 0.02f, -8);
                for (int i = 0; i < 6; i++) k.Box(dark, 0, -0.12f + i * 0.02f, -0.482f, 0.05f, 0.005f, 0.004f, -8);
                k.CX(metal, 0, -0.1f, -0.33f, 0.01f, 0.06f);
                k.muzzle = new Vector3(0, 0.03f, 0.68f);
            }
            else
            {
                k.Box(k.body, 0, 0, 0, 0.07f, 0.1f, 0.4f);
                k.Box(k.body, 0, 0.065f, -0.02f, 0.066f, 0.035f, 0.34f);
                for (int i = 0; i < 9; i++) k.Box(k.body, 0, 0.083f, -0.16f + i * 0.035f, 0.068f, 0.004f, 0.006f);
                k.Rivets(0.035f, -0.02f, -0.16f, 0.16f, 6);
                k.Rail(0.09f, -0.1f, 0.12f);
                k.Barrel(0.03f, 0.2f, 0.017f, 0.38f, 6);
                k.Brake(0.03f, 0.58f, 0.024f, 0.08f, 8);
                k.Box(k.wd, 0, 0.0f, 0.3f, 0.08f, 0.08f, 0.2f);
                k.Slots(0.04f, 0.0f, 0.22f, 0.38f, 5, 0.025f, 0.008f);
                k.Mag(k.body, -0.05f, 0.09f, 0.16f, 0.07f, 0.1f, 6, 1.5f, 6);
                k.Trigger(-0.05f, -0.02f);
                k.Grip(grip, -0.11f, -0.12f, 20, 0.13f, 0.045f, 0.055f, 7);
                k.Box(k.body, 0, -0.01f, -0.33f, 0.03f, 0.03f, 0.26f);                                                 // folding stock frame
                k.Box(k.body, 0, -0.08f, -0.33f, 0.03f, 0.02f, 0.24f, 8);
                k.Box(rubber, 0, -0.04f, -0.47f, 0.05f, 0.13f, 0.02f);
                k.muzzle = new Vector3(0, 0.03f, 0.67f);
            }
        }

        static void SVD(K k)
        {
            k.Box(k.body, 0, 0, 0, 0.055f, 0.085f, 0.36f);
            k.Box(k.body, 0, 0.055f, -0.02f, 0.052f, 0.03f, 0.3f);
            for (int i = 0; i < 8; i++) k.Box(k.body, 0, 0.071f, -0.14f + i * 0.035f, 0.054f, 0.004f, 0.006f);
            k.Rivets(0.0275f, -0.02f, -0.15f, 0.15f, 5);
            k.Barrel(0, 0.18f, 0.011f, 0.64f, 6);
            k.Brake(0, 0.8f, 0.016f, 0.08f, 6);
            k.Box(k.wd, 0, 0, 0.3f, 0.065f, 0.068f, 0.26f);
            k.Slots(0.0325f, 0.0f, 0.2f, 0.42f, 6, 0.03f, 0.006f);
            k.Box(k.body, 0, 0.04f, 0.5f, 0.02f, 0.04f, 0.03f);
            k.Scope(0.11f, -0.04f, 0.32f, 0.021f);
            k.Box(dark, -0.025f, 0.07f, -0.04f, 0.012f, 0.05f, 0.12f);                                                // side mount
            k.Rivets(0.031f, 0.07f, -0.08f, 0.0f, 2);
            k.Mag(k.body, -0.045f, 0.05f, 0.11f, 0.045f, 0.075f, 5, 2f, 5);
            k.Trigger(-0.045f, -0.05f);
            // thumbhole skeleton stock
            k.Box(k.wd, 0, 0.0f, -0.33f, 0.04f, 0.05f, 0.3f);
            k.Box(k.wd, 0, -0.075f, -0.38f, 0.04f, 0.06f, 0.2f);
            k.Box(k.wd, 0, -0.09f, -0.12f, 0.04f, 0.11f, 0.05f, 18);
            k.Box(k.wd, 0, -0.04f, -0.25f, 0.04f, 0.08f, 0.04f, 30);
            k.Box(rubber, 0, -0.035f, -0.485f, 0.044f, 0.13f, 0.012f);
            k.Box(k.wd, 0, 0.035f, -0.33f, 0.042f, 0.02f, 0.12f);                                                     // cheek rest
            k.CX(metal, 0, -0.06f, -0.3f, 0.01f, 0.05f);
            k.muzzle = new Vector3(0, 0, 0.88f);
        }

        static void Mosin(K k)
        {
            k.Box(k.wd, 0, -0.01f, 0.15f, 0.055f, 0.075f, 0.95f);
            k.Box(k.wd, 0, -0.05f, -0.45f, 0.05f, 0.13f, 0.26f, -7);
            k.Box(metal, 0, -0.05f, -0.585f, 0.052f, 0.13f, 0.01f, -7);
            k.Barrel(0.03f, -0.1f, 0.012f, 0.95f, 0);
            k.CZ(metal, 0, 0.03f, -0.1f, 0.022f, 0.22f);
            for (int i = 0; i < 2; i++) { k.CZ(metal, 0, 0.0f, 0.3f + i * 0.28f, 0.04f, 0.015f); k.Rivets(0.04f, 0.0f, 0.307f + i * 0.28f, 0.307f + i * 0.28f, 1); }   // barrel bands
            k.Box(metal, 0.04f, 0.03f, -0.15f, 0.07f, 0.012f, 0.012f);
            k.Ball(metal, 0.075f, 0.03f, -0.15f, 0.022f, 0.022f, 0.022f);
            k.Scope(0.09f, -0.12f, 0.26f, 0.017f);
            k.Box(dark, 0, 0.055f, 0.82f, 0.016f, 0.03f, 0.03f);
            k.Box(dark, 0, 0.075f, 0.82f, 0.004f, 0.015f, 0.004f);
            k.Trigger(-0.05f, -0.1f);
            k.Box(metal, 0, -0.06f, -0.02f, 0.03f, 0.03f, 0.1f);                                                      // magazine floorplate
            for (int i = 0; i < 26; i++) k.Box(dark, 0, -0.03f, -0.42f + i * 0.009f, 0.057f, 0.003f, 0.004f);           // grip checkering
            k.Rivets(0.0275f, -0.01f, -0.25f, 0.6f, 8);
            k.CX(metal, 0, -0.05f, 0.4f, 0.01f, 0.06f);
            k.CX(metal, 0, -0.09f, -0.4f, 0.01f, 0.06f);
            k.CZ(metal, 0, -0.03f, 0.3f, 0.004f, 0.55f);                                                              // cleaning rod
            if (k.skin != null) k.Box(k.skin, 0, 0.012f, 0.15f, 0.057f, 0.03f, 0.9f);
            k.muzzle = new Vector3(0, 0.03f, 0.86f);
        }

        static void AWP(K k)
        {
            k.Box(k.body, 0, 0.01f, 0.05f, 0.075f, 0.1f, 0.5f);                                                       // chassis
            k.Slots(0.0375f, 0.0f, 0.12f, 0.29f, 6, 0.04f, 0.012f);
            k.CZ(k.body, 0, 0.05f, -0.15f, 0.022f, 0.24f);                                                            // receiver
            k.Box(metal, 0.04f, 0.05f, -0.1f, 0.06f, 0.012f, 0.012f);
            k.Ball(metal, 0.075f, 0.04f, -0.1f, 0.02f, 0.02f, 0.02f);
            k.Barrel(0.05f, 0.3f, 0.013f, 0.6f, 8);
            k.Brake(0.05f, 0.9f, 0.02f, 0.09f, 8);
            k.Scope(0.12f, -0.05f, 0.36f, 0.025f);
            k.Rail(0.075f, -0.25f, 0.15f);
            k.Mag(k.body, -0.04f, -0.05f, 0.08f, 0.05f, 0.08f, 4, 0, 4);
            k.Trigger(-0.04f, -0.13f);
            k.Grip(k.body, -0.08f, -0.2f, 10, 0.12f, 0.045f, 0.06f, 7);
            k.Box(k.body, 0, -0.01f, -0.42f, 0.06f, 0.12f, 0.26f, -3);                                                // stock
            k.Box(k.body, 0, 0.06f, -0.4f, 0.04f, 0.03f, 0.16f);                                                      // cheek riser
            for (int i = 0; i < 3; i++) k.CX(metal, 0, 0.045f, -0.45f + i * 0.05f, 0.005f, 0.065f, 6);
            k.Box(rubber, 0, -0.01f, -0.555f, 0.062f, 0.14f, 0.02f);
            k.Box(k.body, 0, -0.085f, -0.47f, 0.02f, 0.05f, 0.04f);                                                   // monopod
            k.Bipod(-0.04f, 0.28f, 0.22f);
            k.Rivets(0.0375f, 0.03f, -0.05f, 0.27f, 5);
            k.muzzle = new Vector3(0, 0.05f, 1.0f);
        }

        static void RPG(K k)
        {
            k.CZ(k.tube, 0, 0, -0.5f, 0.038f, 0.95f);
            for (int i = 0; i < 12; i++) k.CZ(dark, 0, 0, -0.44f + i * 0.075f, 0.04f, 0.008f);                        // tube rings
            k.CZ(dark, 0, 0, -0.62f, 0.055f, 0.13f, 0.038f);                                                          // rear flare
            for (int i = 0; i < 8; i++) { float a = i * 45; var o = Quaternion.Euler(0, 0, a) * new Vector3(0.05f, 0, 0); k.Box(metal, o.x, o.y, -0.58f, 0.004f, 0.004f, 0.09f, 0, 0, a); }
            k.CZ(rocketMat, 0, 0, 0.45f, 0.042f, 0.12f);
            k.CZ(rocketMat, 0, 0, 0.57f, 0.07f, 0.12f, 0.045f);
            k.CZ(rocketMat, 0, 0, 0.69f, 0.07f, 0.16f, 0.005f);
            for (int i = 0; i < 6; i++) { float a = i * 60; var o = Quaternion.Euler(0, 0, a) * new Vector3(0.06f, 0, 0); k.Box(dark, o.x, o.y, 0.62f, 0.003f, 0.012f, 0.1f, 0, 0, a); }
            k.Box(k.wd, 0, -0.06f, 0.0f, 0.06f, 0.012f, 0.24f);                                                       // heat shield
            k.Rivets(0.03f, -0.06f, -0.1f, 0.1f, 4);
            k.Grip(k.wd, -0.09f, -0.05f, 15, 0.1f, 0.03f, 0.04f, 5);
            k.Grip(k.wd, -0.09f, 0.2f, 0, 0.1f, 0.03f, 0.04f, 5);
            k.Trigger(-0.05f, -0.03f);
            k.Box(metal, 0.04f, 0.05f, 0.1f, 0.03f, 0.05f, 0.08f);                                                    // optic PGO-7
            k.CZ(dark, 0.04f, 0.075f, 0.05f, 0.016f, 0.12f);
            k.CZ(glass, 0.04f, 0.075f, 0.17f, 0.017f, 0.004f);
            k.CZ(rubber, 0.04f, 0.075f, 0.0f, 0.02f, 0.05f);
            k.Box(metal, 0, 0.05f, 0.25f, 0.012f, 0.04f, 0.012f);
            k.Box(metal, 0, 0.05f, -0.2f, 0.012f, 0.04f, 0.012f);
            k.CX(metal, 0, -0.04f, -0.35f, 0.01f, 0.08f);
            k.CX(metal, 0, -0.04f, 0.35f, 0.01f, 0.08f);
            k.Rivets(0.039f, 0.0f, -0.4f, 0.4f, 16, 0.004f, dark);
            k.Engrave(0.039f, 0.02f, -0.3f, 0.3f, 6);
            k.muzzle = new Vector3(0, 0, 0.55f);
        }

        // ================================================================== pistols

        static void Pistol(K k, int kind)
        {
            bool gl = kind == 1, de = kind == 3;
            float sw = de ? 0.04f : gl ? 0.034f : 0.032f, sh = de ? 0.05f : 0.042f, sl = de ? 0.26f : 0.19f;
            var gr = kind == 2 ? k.wd : kind == 0 ? (k.skin ?? grip) : k.body;
            k.Box(k.body, 0, 0.03f, 0.03f, sw, sh, sl);                                                              // slide
            for (int i = 0; i < 9; i++) k.Box(dark, 0, 0.03f, -0.05f + i * 0.007f, sw + 0.002f, sh * 0.8f, 0.0025f);  // rear serrations
            if (!de) for (int i = 0; i < 6; i++) k.Box(dark, 0, 0.03f, 0.08f + i * 0.007f, sw + 0.002f, sh * 0.7f, 0.0025f);
            k.Box(dark, sw / 2 + 0.001f, 0.035f, 0.04f, 0.003f, 0.016f, 0.04f);                                     // ejection port
            k.Box(k.body, 0, -0.003f, 0.02f, sw - 0.002f, 0.03f, sl * 0.85f);                                       // frame
            if (gl || de) k.Rail(-0.02f, 0.03f, 0.1f, 0.018f);
            k.CZ(metal, 0, 0.03f, sl / 2 + 0.03f, 0.009f, 0.012f);
            k.CZ(rubber, 0, 0.03f, sl / 2 + 0.041f, 0.005f, 0.002f);
            k.Box(dark, 0, 0.054f + (de ? 0.004f : 0), sl / 2 + 0.02f, 0.004f, 0.008f, 0.006f);                       // front sight
            k.Box(dark, 0, 0.054f + (de ? 0.004f : 0), -0.055f, 0.022f, 0.008f, 0.008f);                             // rear sight
            k.Ball(Mats.Glow(new Color(0.3f, 1.2f, 0.4f)), 0, 0.06f, sl / 2 + 0.02f, 0.003f, 0.003f, 0.003f);
            k.Ball(Mats.Glow(new Color(0.3f, 1.2f, 0.4f)), 0.007f, 0.06f, -0.055f, 0.003f, 0.003f, 0.003f);
            k.Ball(Mats.Glow(new Color(0.3f, 1.2f, 0.4f)), -0.007f, 0.06f, -0.055f, 0.003f, 0.003f, 0.003f);
            k.Grip(gr, -0.07f, -0.06f, 15, 0.13f, 0.034f, 0.055f, 10);
            for (int i = 0; i < 12; i++) k.Box(dark, sw / 2, -0.04f - (i / 4) * 0.025f, -0.07f + (i % 4) * 0.012f, 0.002f, 0.006f, 0.006f, 15);   // grip texture
            k.Trigger(-0.003f, 0.03f, 0.05f);
            k.Box(metal, 0, -0.14f, -0.08f, 0.032f, 0.012f, 0.05f, 15);                                               // mag base
            if (!gl) { k.Box(metal, 0, 0.045f, -0.065f, 0.008f, 0.02f, 0.012f, -25); k.CX(metal, 0, 0.04f, -0.06f, 0.004f, 0.02f, 6); }   // hammer
            k.Box(metal, sw / 2 + 0.002f, 0.005f, -0.02f, 0.004f, 0.008f, 0.02f);                                    // safety / slide stop
            k.Box(metal, sw / 2 + 0.002f, 0.01f, 0.02f, 0.004f, 0.006f, 0.03f);
            k.Rivets(sw / 2, 0.005f, -0.03f, 0.08f, 4, 0.003f);
            k.Rivets(0.017f, -0.06f, -0.075f, -0.045f, 2, 0.004f);
            k.Engrave(sw / 2, 0.038f, -0.02f, sl / 2 - 0.01f, 8);                                                    // markings on the slide
            for (int i = 0; i < 10; i++) k.Box(dark, 0, 0.03f + sh / 2 + 0.001f, -0.04f + i * 0.008f, sw * 0.5f, 0.0015f, 0.003f);   // top serrations
            k.Box(metal, sw / 2 + 0.001f, 0.042f, 0.05f, 0.003f, 0.006f, 0.02f);                                     // extractor
            k.CZ(metal, 0, 0.005f, sl / 2 - 0.02f, 0.005f, 0.04f);                                                   // recoil spring rod
            k.g.Cyl(metal, new Vector3(-0.01f, -0.15f, -0.1f), 0.008f, 0.003f, 12, 0.008f, false, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;   // lanyard loop
            k.Box(metal, 0, -0.145f, -0.095f, 0.01f, 0.01f, 0.006f);
            k.Rivets(0.016f, -0.143f, -0.09f, -0.07f, 2, 0.0025f);
            if (de) { k.Box(k.body, 0, 0.06f, 0.03f, 0.02f, 0.008f, 0.22f); for (int i = 0; i < 10; i++) k.Box(dark, 0, 0.065f, -0.07f + i * 0.02f, 0.022f, 0.003f, 0.004f); }
            k.muzzle = new Vector3(0, 0.03f, sl / 2 + 0.05f);
        }

        static void Uzi(K k)
        {
            k.Box(k.body, 0, 0.02f, 0.03f, 0.05f, 0.06f, 0.24f);
            for (int i = 0; i < 12; i++) k.Box(dark, 0, 0.051f, -0.07f + i * 0.018f, 0.052f, 0.003f, 0.005f);       // top ribs
            k.Slots(0.025f, 0.02f, -0.06f, 0.14f, 8, 0.03f, 0.006f);
            k.Barrel(0.02f, 0.15f, 0.01f, 0.06f, 4);
            k.CZ(dark, 0, 0.02f, 0.2f, 0.015f, 0.012f);
            k.Box(k.body, 0.026f, 0.055f, 0.02f, 0.01f, 0.012f, 0.03f);                                               // cocking knob
            k.Box(k.body, 0, 0.06f, 0.13f, 0.012f, 0.02f, 0.012f);
            k.Box(k.body, 0, 0.06f, -0.08f, 0.02f, 0.02f, 0.012f);
            k.Mag(k.body, -0.02f, -0.02f, 0.22f, 0.028f, 0.04f, 6, 0, 6);                                             // mag in the grip
            k.Grip(grip, -0.07f, -0.02f, 0, 0.11f, 0.04f, 0.05f, 8);
            k.Trigger(-0.01f, 0.05f, 0.05f);
            k.Box(metal, 0, -0.04f, 0.04f, 0.008f, 0.03f, 0.008f);
            k.Rivets(0.025f, 0.0f, -0.08f, 0.13f, 5, 0.003f);
            for (int s = -1; s <= 1; s += 2) k.Box(k.body, s * 0.03f, 0.03f, -0.15f, 0.005f, 0.012f, 0.14f);        // folded stock
            k.Box(k.body, 0, 0.03f, -0.22f, 0.065f, 0.03f, 0.01f);
            k.Engrave(0.025f, 0.04f, -0.06f, 0.14f, 8);
            k.muzzle = new Vector3(0, 0.02f, 0.22f);
        }

        static void Revolver(K k)
        {
            k.Box(k.body, 0, 0.02f, 0.0f, 0.03f, 0.06f, 0.08f);                                                      // frame
            k.CZ(k.body, 0, 0.025f, -0.03f, 0.022f, 0.055f);                                                          // cylinder
            for (int i = 0; i < 7; i++)
            {
                float a = i * 360f / 7;
                var o = Quaternion.Euler(0, 0, a) * new Vector3(0.013f, 0, 0);
                k.CZ(brass, o.x, 0.025f + o.y, -0.032f, 0.004f, 0.004f);                                                // cartridges
                k.Box(dark, o.x * 1.65f, 0.025f + o.y * 1.65f, -0.003f, 0.003f, 0.003f, 0.04f, 0, 0, a);                 // flutes
            }
            k.Barrel(0.04f, 0.04f, 0.009f, 0.14f, 0);
            k.CZ(k.body, 0, 0.027f, 0.04f, 0.006f, 0.12f);                                                            // ejector rod
            k.Box(dark, 0, 0.053f, 0.17f, 0.004f, 0.012f, 0.008f);
            k.Box(metal, 0, 0.05f, -0.05f, 0.01f, 0.02f, 0.014f, -25);
            k.Grip(k.wd, -0.06f, -0.05f, 20, 0.11f, 0.03f, 0.045f, 10);
            for (int i = 0; i < 16; i++) k.Box(dark, 0.0155f, -0.03f - (i / 4) * 0.02f, -0.065f + (i % 4) * 0.009f, 0.002f, 0.005f, 0.005f, 20);
            k.Trigger(0.0f, 0.005f, 0.04f);
            k.Rivets(0.015f, 0.02f, -0.02f, 0.03f, 3, 0.003f);
            k.CX(metal, 0, -0.12f, -0.09f, 0.006f, 0.02f);
            for (int i = 0; i < 14; i++) { float a = i * 360f / 14; var o = Quaternion.Euler(0, 0, a) * new Vector3(0.02f, 0, 0); k.Box(metal, o.x, 0.025f + o.y, -0.031f, 0.004f, 0.004f, 0.004f, 0, 0, a); }   // ratchet
            for (int i = 0; i < 7; i++) { float a = i * 360f / 7; var o = Quaternion.Euler(0, 0, a) * new Vector3(0.013f, 0, 0); k.CZ(metal, o.x, 0.025f + o.y, -0.034f, 0.0048f, 0.002f); }   // rims
            k.Engrave(0.015f, 0.03f, -0.035f, 0.035f, 8, 0.0025f);
            k.g.Cyl(metal, new Vector3(-0.006f, -0.125f, -0.1f), 0.007f, 0.003f, 12, 0.007f, false, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;
            for (int i = 0; i < 4; i++) k.Box(dark, 0, 0.062f, -0.058f + i * 0.004f, 0.009f, 0.002f, 0.002f, -25);   // hammer spur checkering
            k.muzzle = new Vector3(0, 0.04f, 0.19f);
        }

        // ================================================================== blades

        static void Knife(K k, int kind)
        {
            // kind 0 KA-BAR, 1 butterfly, 2 machete
            float L = kind == 2 ? 0.42f : kind == 1 ? 0.17f : 0.215f, H = kind == 2 ? 0.05f : kind == 1 ? 0.026f : 0.034f;
            Vector2[] poly = kind == 2
                ? new[] { new Vector2(0, -H / 2), new Vector2(L * 0.85f, -H / 2), new Vector2(L, H * 0.1f), new Vector2(L * 0.9f, H * 0.5f), new Vector2(0, H * 0.4f) }
                : new[] { new Vector2(0, -H / 2), new Vector2(L * 0.7f, -H / 2), new Vector2(L * 0.93f, -H * 0.25f), new Vector2(L, H * 0.35f), new Vector2(L * 0.78f, H * 0.18f), new Vector2(L * 0.7f, H / 2), new Vector2(0, H / 2) };
            k.Ext(k.blade, poly, 0.006f, Vector3.zero, BladeRot);
            k.Ext(steel, new[] { new Vector2(0.02f, -H / 2 - 0.0005f), new Vector2(L * 0.68f, -H / 2 - 0.0005f), new Vector2(L * 0.68f, -H / 2 + 0.004f), new Vector2(0.02f, -H / 2 + 0.004f) }, 0.007f, Vector3.zero, BladeRot);   // honed edge
            k.Ext(dark, new[] { new Vector2(0.02f, -0.002f), new Vector2(L * 0.6f, -0.002f), new Vector2(L * 0.6f, 0.002f), new Vector2(0.02f, 0.002f) }, 0.0068f, Vector3.zero, BladeRot);   // fuller
            k.Teeth(k.blade, BladeRot, Vector3.zero, 0.03f, L * 0.45f, H / 2, kind == 2 ? 40 : 34, kind == 2 ? 0.007f : 0.004f);
            for (int i = 0; i < 6; i++) k.Ext(dark, new[] { new Vector2(0.03f + i * 0.012f, -0.006f), new Vector2(0.036f + i * 0.012f, -0.006f), new Vector2(0.036f + i * 0.012f, 0.004f), new Vector2(0.03f + i * 0.012f, 0.004f) }, 0.0066f, Vector3.zero, BladeRot);   // etched mark
            if (kind == 1)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    k.Box(k.body, s * 0.012f, 0, -0.065f, 0.01f, 0.028f, 0.13f);
                    for (int i = 0; i < 16; i++) k.Box(dark, s * 0.0175f, 0, -0.01f - i * 0.0075f, 0.002f, 0.012f, 0.004f);   // window cut-outs
                }
                k.Rivets(0.017f, 0.008f, -0.005f, -0.125f, 6, 0.0025f);
                for (int s = -1; s <= 1; s += 2)
                {
                }
                k.CX(metal, -0.02f, 0, -0.005f, 0.006f, 0.04f);
                k.CX(metal, -0.02f, 0, -0.125f, 0.004f, 0.04f);
                k.Box(metal, 0, 0.01f, -0.13f, 0.03f, 0.008f, 0.01f);
                k.Box(metal, 0, 0.016f, -0.135f, 0.008f, 0.004f, 0.012f);
                for (int i = 0; i < 10; i++) k.Box(rubber, 0, 0.016f, -0.02f - i * 0.0105f, 0.022f, 0.002f, 0.004f);   // handle ribs
                k.muzzle = new Vector3(0, 0, 0.17f);
                return;
            }
            k.Box(metal, 0, 0, -0.006f, 0.03f, H + 0.025f, 0.012f);                                                   // guard
            k.Ball(metal, 0, (H + 0.025f) / 2, -0.006f, 0.012f, 0.012f, 0.012f); k.Ball(metal, 0, -(H + 0.025f) / 2, -0.006f, 0.012f, 0.012f, 0.012f);
            float hl = kind == 2 ? 0.14f : 0.11f;
            k.CZ(grip, 0, 0, -0.012f - hl, 0.015f, hl);
            k.Wrap(k.wd, 0, -0.016f, hl - 0.01f, 0.015f, 40, false);                                                // leather washers
            k.Rivets(0.015f, 0, -0.006f, -0.006f, 2, 0.003f);
            k.Box(metal, 0, 0, -0.018f - hl, 0.026f, 0.032f, 0.02f);                                                  // pommel
            k.CZ(metal, 0, 0, -0.03f - hl, 0.008f, 0.004f); k.Rivets(0.013f, 0, -0.018f - hl, -0.018f - hl, 1, 0.003f);
            k.CX(dark, 0, 0.006f, -0.02f - hl, 0.004f, 0.03f, 8);                                                     // lanyard hole
            for (int i = 0; i < 4; i++) k.CX(brass, 0, 0, -0.03f - i * hl / 4, 0.003f, 0.034f, 6);                   // rivets through the handle
            k.muzzle = new Vector3(0, 0, L);
        }

        static void Karambit(K k)
        {
            var poly = new List<Vector2>();
            for (int i = 0; i <= 10; i++) { float a = Mathf.Lerp(-10, 80, i / 10f) * Mathf.Deg2Rad; poly.Add(new Vector2(Mathf.Cos(a) * 0.1f, Mathf.Sin(a) * 0.1f - 0.1f)); }
            for (int i = 10; i >= 0; i--) { float a = Mathf.Lerp(-10, 80, i / 10f) * Mathf.Deg2Rad; poly.Add(new Vector2(Mathf.Cos(a) * 0.075f, Mathf.Sin(a) * 0.075f - 0.1f + 0.008f)); }
            k.Ext(k.blade, poly.ToArray(), 0.006f, new Vector3(0, 0.06f, 0.0f), Quaternion.Euler(0, -90, 180));
            k.Teeth(k.blade, Quaternion.Euler(0, -90, 0), new Vector3(0, 0, 0.0f), 0.0f, 0.04f, 0.008f, 24, 0.003f);
            k.CZ(k.body, 0, 0, -0.1f, 0.016f, 0.1f);
            k.Wrap(dark, 0, -0.005f, 0.09f, 0.016f, 34, true);
            k.g.Cyl(metal, new Vector3(0, -0.02f, -0.13f), 0.022f, 0.008f, 16, -1, true, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;   // finger ring
            k.g.Cyl(dark, new Vector3(0, -0.02f, -0.13f), 0.015f, 0.009f, 16, -1, true, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;
            for (int i = 0; i < 5; i++) k.CX(brass, 0, 0, -0.02f - i * 0.018f, 0.003f, 0.036f, 6);
            k.muzzle = new Vector3(0, 0, 0.1f);
        }

        static void Katana(K k)
        {
            float L = 0.7f;
            var poly = new List<Vector2>();
            for (int i = 0; i <= 12; i++) { float t = i / 12f; poly.Add(new Vector2(t * L, -0.016f + t * t * 0.03f)); }
            poly.Add(new Vector2(L + 0.03f, 0.02f));
            for (int i = 12; i >= 0; i--) { float t = i / 12f; poly.Add(new Vector2(t * L, 0.014f + t * t * 0.03f)); }
            k.Ext(k.blade, poly.ToArray(), 0.006f, Vector3.zero, BladeRot);
            k.Ext(steel, new[] { new Vector2(0.05f, -0.0165f), new Vector2(L * 0.95f, -0.0165f + 0.027f), new Vector2(L * 0.95f, -0.012f + 0.027f), new Vector2(0.05f, -0.012f) }, 0.0065f, Vector3.zero, BladeRot);   // hamon line
            k.Box(brass, 0, 0, 0.012f, 0.012f, 0.036f, 0.025f);                                                      // habaki
            k.CZ(metal, 0, 0, -0.008f, 0.042f, 0.008f, -1, 20);                                                       // tsuba
            for (int i = 0; i < 8; i++) { float a = i * 45; var o = Quaternion.Euler(0, 0, a) * new Vector3(0.032f, 0, 0); k.CZ(brass, o.x, o.y, -0.009f, 0.004f, 0.01f); }
            k.CZ(k.wd, 0, 0, -0.27f, 0.016f, 0.26f);                                                                  // tsuka
            k.Wrap(dark, 0, -0.012f, 0.25f, 0.016f, 40, true);
            for (int i = 0; i < 6; i++) k.Box(Mats.Lit(new Color(0.5f, 0.05f, 0.08f), 0.3f), 0, -0.022f - i * 0.006f, -0.03f - i * 0.004f, 0.006f, 0.006f, 0.006f);   // sageo cord
            k.CZ(metal, 0, 0, -0.285f, 0.018f, 0.016f);                                                               // kashira
            k.Ball(brass, 0, 0.017f, -0.14f, 0.008f, 0.004f, 0.016f); k.Ball(brass, 0, -0.017f, -0.14f, 0.008f, 0.004f, 0.016f);   // menuki
            k.CX(dark, 0, 0, -0.04f, 0.003f, 0.036f, 6);
            k.muzzle = new Vector3(0, 0.03f, L);
        }

        // ================================================================== throwing

        static void Shuriken(K k)
        {
            var poly = new List<Vector2>();
            for (int i = 0; i < 8; i++) { float a = i * Mathf.PI / 4, rr = i % 2 == 0 ? 0.075f : 0.022f; poly.Add(new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr)); }
            var flat = Quaternion.Euler(90, 0, 0);
            k.Ext(k.blade, poly.ToArray(), 0.006f, Vector3.zero, flat);
            for (int p = 0; p < 4; p++)                                                                                  // bevelled edges + engraved lines per point
            {
                float a = p * 90;
                var q = Quaternion.Euler(0, -a, 0);
                for (int e = 0; e < 6; e++)
                {
                    var c = q * new Vector3(0.028f + e * 0.008f, 0.0035f, 0);
                    k.Box(steel, c.x, c.y, c.z, 0.006f, 0.0015f, 0.03f - e * 0.0045f, 0, -a, 0);
                    k.Box(dark, c.x, -c.y, c.z, 0.004f, 0.0015f, 0.002f, 0, -a, 0);
                }
                k.Box(dark, (q * new Vector3(0.05f, 0.0036f, 0)).x, 0.0036f, (q * new Vector3(0.05f, 0, 0)).z, 0.04f, 0.0008f, 0.0015f, 0, -a, 0);
            }
            k.CY(dark, 0, -0.005f, 0, 0.013f, 0.01f, 16);
            k.CY(metal, 0, -0.0055f, 0, 0.008f, 0.011f, 16);
            for (int i = 0; i < 8; i++) { float a = i * 45 * Mathf.Deg2Rad; k.CY(brass, Mathf.Cos(a) * 0.017f, -0.004f, Mathf.Sin(a) * 0.017f, 0.0025f, 0.008f, 6); }
            for (int i = 0; i < 24; i++) { float a = i * 15f * Mathf.Deg2Rad; k.Box(steel, Mathf.Cos(a) * 0.024f, -0.0032f, Mathf.Sin(a) * 0.024f, 0.003f, 0.0015f, 0.0015f, 0, -i * 15f, 0); }
            for (int i = 0; i < 16; i++) { float a = i * 22.5f * Mathf.Deg2Rad; k.Box(dark, Mathf.Cos(a) * 0.011f, 0.0032f, Mathf.Sin(a) * 0.011f, 0.003f, 0.0015f, 0.0015f, 0, -i * 22.5f, 0); }
            k.muzzle = Vector3.zero;
        }

        static void Kunai(K k)
        {
            var poly = new[] { new Vector2(0, -0.012f), new Vector2(0.09f, -0.022f), new Vector2(0.14f, 0), new Vector2(0.09f, 0.022f), new Vector2(0, 0.012f) };
            k.Ext(k.blade, poly, 0.006f, Vector3.zero, BladeRot);
            k.Ext(steel, new[] { new Vector2(0.005f, -0.001f), new Vector2(0.13f, -0.0005f), new Vector2(0.13f, 0.0005f), new Vector2(0.005f, 0.001f) }, 0.0075f, Vector3.zero, BladeRot);   // ridge
            k.Teeth(k.blade, BladeRot, Vector3.zero, 0.01f, 0.08f, 0.015f, 20, 0.003f);
            k.Teeth(k.blade, BladeRot * Quaternion.Euler(180, 0, 0), Vector3.zero, 0.01f, 0.08f, 0.015f, 20, 0.003f);
            k.CZ(grip, 0, 0, -0.09f, 0.009f, 0.09f);
            k.Wrap(Mats.Lit(new Color(0.6f, 0.1f, 0.1f), 0.3f), 0, -0.002f, 0.085f, 0.009f, 20, true);
            k.g.Cyl(metal, new Vector3(-0.002f, 0, -0.11f), 0.018f, 0.004f, 18, -1, true, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;
            k.g.Cyl(dark, new Vector3(-0.0025f, 0, -0.11f), 0.012f, 0.005f, 18, -1, true, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;
            for (int i = 0; i < 10; i++) { float a = i * 36 * Mathf.Deg2Rad; k.CX(brass, 0, Mathf.Sin(a) * 0.015f, -0.11f + Mathf.Cos(a) * 0.015f, 0.0022f, 0.006f, 6); }
            for (int i = 0; i < 8; i++) k.CZ(k.wd, 0, 0, -0.005f - i * 0.011f, 0.0105f, 0.003f);
            k.muzzle = Vector3.zero;
        }

        static void Axe(K k)
        {
            k.CZ(k.wd, 0, 0, -0.2f, 0.014f, 0.38f);
            for (int i = 0; i < 40; i++) k.CZ(dark, 0, 0, -0.19f + i * 0.0045f, 0.0148f, 0.002f);                  // cord wrap
            for (int i = 0; i < 20; i++) k.CZ(grip, 0, 0, 0.0f + i * 0.006f, 0.0145f, 0.002f);
            var poly = new[] { new Vector2(-0.03f, -0.02f), new Vector2(0.03f, -0.02f), new Vector2(0.03f, 0.04f), new Vector2(0.07f, 0.08f), new Vector2(0.075f, 0.13f), new Vector2(-0.065f, 0.13f), new Vector2(-0.055f, 0.08f), new Vector2(-0.03f, 0.04f) };
            k.Ext(k.blade, poly, 0.012f, new Vector3(0, 0, 0.15f), BladeRot);
            k.Ext(steel, new[] { new Vector2(0.075f, 0.12f), new Vector2(-0.065f, 0.12f), new Vector2(-0.065f, 0.132f), new Vector2(0.075f, 0.132f) }, 0.013f, new Vector3(0, 0, 0.15f), BladeRot);   // edge
            k.Teeth(k.blade, BladeRot, new Vector3(0, 0, 0.15f), -0.03f, 0.03f, -0.02f, 20, 0.003f);
            for (int i = 0; i < 12; i++) k.CX(brass, 0, 0.0f + (i % 6) * 0.015f, 0.15f + (i / 6) * 0.03f - 0.015f, 0.003f, 0.016f, 6);
            for (int i = 0; i < 6; i++) k.Ext(dark, new[] { new Vector2(-0.04f + i * 0.015f, 0.09f), new Vector2(-0.035f + i * 0.015f, 0.09f), new Vector2(-0.035f + i * 0.015f, 0.11f), new Vector2(-0.04f + i * 0.015f, 0.11f) }, 0.0125f, new Vector3(0, 0, 0.15f), BladeRot);
            k.CZ(metal, 0, 0, 0.17f, 0.018f, 0.012f);
            k.CZ(metal, 0, 0, -0.205f, 0.018f, 0.015f);
            k.muzzle = Vector3.zero;
        }

        static void Grenade(K k, bool rgd)
        {
            var bodyM = k.skin ?? olive;
            if (!rgd)
            {
                k.Ball(bodyM, 0, 0, 0, 0.08f, 0.1f, 0.08f);
                for (int row = 0; row < 8; row++)                                                                         // pineapple segments
                    for (int col = 0; col < 10; col++)
                    {
                        float v = (row + 0.5f) / 8f, lat = Mathf.Lerp(-62, 62, v) * Mathf.Deg2Rad, lon = col * Mathf.PI / 5 + (row % 2) * Mathf.PI / 10;
                        var n = new Vector3(Mathf.Cos(lat) * Mathf.Cos(lon), Mathf.Sin(lat) * 1.25f, Mathf.Cos(lat) * Mathf.Sin(lon));
                        k.Box(bodyM, n.x * 0.04f, n.y * 0.04f, n.z * 0.04f, 0.016f, 0.014f, 0.016f, 0, -lon * Mathf.Rad2Deg, 0);
                    }
            }
            else
            {
                k.Ball(bodyM, 0, 0, 0, 0.075f, 0.09f, 0.075f);
                for (int i = 0; i < 24; i++) { float a = i * 15 * Mathf.Deg2Rad; k.Box(dark, Mathf.Cos(a) * 0.0375f, 0.0f, Mathf.Sin(a) * 0.0375f, 0.004f, 0.006f, 0.004f, 0, -i * 15, 0); }
                for (int i = 0; i < 24; i++) { float a = i * 15 * Mathf.Deg2Rad; k.Box(dark, Mathf.Cos(a) * 0.036f, 0.014f, Mathf.Sin(a) * 0.036f, 0.003f, 0.004f, 0.003f, 0, -i * 15, 0); }
                for (int i = 0; i < 24; i++) { float a = i * 15 * Mathf.Deg2Rad; k.Box(dark, Mathf.Cos(a) * 0.03f, 0.028f, Mathf.Sin(a) * 0.03f, 0.003f, 0.004f, 0.003f, 0, -i * 15, 0); }
                for (int i = 0; i < 24; i++) { float a = i * 15 * Mathf.Deg2Rad; k.Box(dark, Mathf.Cos(a) * 0.03f, -0.028f, Mathf.Sin(a) * 0.03f, 0.003f, 0.004f, 0.003f, 0, -i * 15, 0); }
            }
            k.CY(metal, 0, 0.045f, 0, 0.012f, 0.035f);                                                                // fuse
            for (int i = 0; i < 5; i++) k.CY(dark, 0, 0.05f + i * 0.006f, 0, 0.0128f, 0.002f);
            k.Box(metal, 0.022f, 0.045f, 0, 0.01f, 0.07f, 0.014f, 0, 0, -15);                                          // lever
            for (int i = 0; i < 6; i++) k.Box(dark, 0.028f, 0.02f + i * 0.01f, 0, 0.002f, 0.003f, 0.012f, 0, 0, -15);
            k.g.Cyl(metal, new Vector3(-0.022f, 0.075f, 0), 0.014f, 0.004f, 16, 0.014f, false, 0.2f, Quaternion.Euler(0, 0, 90)); k.parts++;   // pull ring
            k.CX(metal, -0.012f, 0.07f, 0, 0.003f, 0.02f, 6);
            k.Ball(brass, 0, 0.083f, 0, 0.014f, 0.01f, 0.014f);
            for (int i = 0; i < 6; i++) k.Box(dark, Mathf.Cos(i * 1.047f) * 0.0125f, 0.06f, Mathf.Sin(i * 1.047f) * 0.0125f, 0.002f, 0.02f, 0.002f, 0, -i * 60, 0);
            k.muzzle = Vector3.zero;
        }
    }
}
