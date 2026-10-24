using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Arena
{
    /// <summary>Procedural weapon models (barrel along +Z, muzzle child) with skins, and the low-poly fighters.</summary>
    public static class Models
    {
        static Material metal, dark, wood, grip, steel, olive, rocketMat;

        static void Init()
        {
            if (metal != null) return;
            metal = Mats.Lit(new Color(0.17f, 0.18f, 0.2f), 0.55f, 0.85f);
            dark = Mats.Lit(new Color(0.07f, 0.075f, 0.085f), 0.4f, 0.6f);
            wood = Mats.Tex(Tex.Planks("8a4f22"), Color.white, 0.35f);
            grip = Mats.Lit(new Color(0.09f, 0.09f, 0.09f), 0.15f);
            steel = Mats.Lit(new Color(0.82f, 0.84f, 0.87f), 0.85f, 1f);
            olive = Mats.Lit(new Color(0.29f, 0.33f, 0.19f), 0.25f);
            rocketMat = Mats.Lit(new Color(0.3f, 0.36f, 0.22f), 0.3f);
        }

        static readonly Dictionary<string, Mesh> cache = new Dictionary<string, Mesh>();

        public static GameObject Weapon(string id, string skin, Transform parent, int layer) => WeaponModels.Build(id, skin, parent, layer);

        /// <summary>First-aid kit for the shop preview: the bigger the heal, the bigger the box.</summary>
        public static GameObject Medkit(string id, Transform parent, int layer)
        {
            var md = Defs.Med(id);
            float s = md.heal >= 100 ? 1.3f : md.heal >= 50 ? 1f : 0.7f;
            var g = new Geo();
            var body = Mats.Lit(md.color, 0.45f);
            var white = Mats.Lit(Color.white, 0.5f);
            g.Box(body, Vector3.zero, new Vector3(0.36f, 0.24f, 0.12f) * s, Quaternion.identity, 0.5f);
            g.Box(white, new Vector3(0, 0, -0.062f * s), new Vector3(0.2f, 0.06f, 0.01f) * s, Quaternion.identity, 0.5f);
            g.Box(white, new Vector3(0, 0, -0.062f * s), new Vector3(0.06f, 0.2f, 0.01f) * s, Quaternion.identity, 0.5f);
            g.Box(Mats.Lit(new Color(0.15f, 0.15f, 0.15f), 0.3f), new Vector3(0, 0.14f * s, 0), new Vector3(0.14f, 0.04f, 0.04f) * s, Quaternion.identity, 0.5f);
            var root = new GameObject(id);
            root.transform.SetParent(parent, false);
            g.Build("Mesh", root.transform, false, layer);
            Prim.SetLayer(root, layer);
            return root;
        }

        /// <summary>Flying rocket (RPG warhead).</summary>
        public static GameObject Rocket(Transform parent)
        {
            Init();
            var g = new Geo();
            var Z = Quaternion.Euler(90, 0, 0);
            g.Cyl(rocketMat, new Vector3(0, 0, -0.2f), 0.04f, 0.2f, 10, -1, true, 0.2f, Z);
            g.Cyl(rocketMat, new Vector3(0, 0, 0), 0.07f, 0.1f, 10, 0.045f, true, 0.2f, Z);
            g.Cyl(rocketMat, new Vector3(0, 0, 0.1f), 0.07f, 0.14f, 10, 0.005f, true, 0.2f, Z);
            for (int i = 0; i < 4; i++) g.Box(dark, new Vector3(0, 0, -0.2f), new Vector3(0.005f, 0.14f, 0.08f), Quaternion.Euler(0, 0, i * 45), 0.2f);
            var root = new GameObject("Rocket");
            root.transform.SetParent(parent, false);
            g.Build("Mesh", root.transform, false, 0);
            return root;
        }

        // ------------------------------------------------------------------

        /// <summary>A fighter: legs, torso, head with helmet and visor, arms holding the weapon.</summary>
        public class Rig
        {
            public GameObject go;
            public Transform legL, legR, upper, hand, head;
            public Material team;
            public Renderer[] renderers;
        }

        public static Rig Fighter(Transform parent, Color teamColor)
        {
            Init();
            var r = new Rig { go = new GameObject("Fighter") };
            r.go.transform.SetParent(parent, false);
            r.team = new Material(Mats.Lit(teamColor, 0.35f));
            var pants = Mats.Lit(new Color(0.14f, 0.15f, 0.18f), 0.2f);
            var skin = Mats.Lit(new Color(0.88f, 0.69f, 0.55f), 0.3f);
            var boots = Mats.Lit(new Color(0.08f, 0.08f, 0.09f), 0.2f);
            var visor = Mats.Lit(new Color(0.05f, 0.08f, 0.12f), 0.95f, 0.5f);
            Transform T(string n, Transform p, Vector3 pos) { var t = new GameObject(n).transform; t.SetParent(p, false); t.localPosition = pos; return t; }
            var root = r.go.transform;
            r.legL = T("LegL", root, new Vector3(-0.13f, 0.85f, 0));
            r.legR = T("LegR", root, new Vector3(0.13f, 0.85f, 0));
            foreach (var leg in new[] { r.legL, r.legR })
            {
                Prim.Make(PrimitiveType.Cube, leg, new Vector3(0, -0.38f, 0), new Vector3(0.19f, 0.78f, 0.21f), pants);
                Prim.Make(PrimitiveType.Cube, leg, new Vector3(0, -0.8f, 0.04f), new Vector3(0.21f, 0.12f, 0.3f), boots);
            }
            Prim.Make(PrimitiveType.Cube, root, new Vector3(0, 0.9f, 0), new Vector3(0.46f, 0.16f, 0.26f), pants);
            Prim.Make(PrimitiveType.Cube, root, new Vector3(0, 1.17f, 0), new Vector3(0.5f, 0.42f, 0.28f), r.team);
            r.upper = T("Upper", root, new Vector3(0, 1.36f, 0));
            Prim.Make(PrimitiveType.Cube, r.upper, new Vector3(0, 0.02f, 0), new Vector3(0.54f, 0.16f, 0.3f), r.team);
            Prim.Make(PrimitiveType.Cube, r.upper, new Vector3(0, -0.06f, 0.15f), new Vector3(0.36f, 0.26f, 0.06f), Mats.Lit(new Color(0.15f, 0.17f, 0.15f), 0.2f));
            r.head = T("Head", r.upper, new Vector3(0, 0.27f, 0));
            Prim.Make(PrimitiveType.Sphere, r.head, Vector3.zero, new Vector3(0.36f, 0.4f, 0.36f), skin);
            Prim.Make(PrimitiveType.Sphere, r.head, new Vector3(0, 0.07f, -0.01f), new Vector3(0.41f, 0.32f, 0.42f), r.team);
            Prim.Make(PrimitiveType.Cube, r.head, new Vector3(0, 0.03f, 0.17f), new Vector3(0.3f, 0.09f, 0.08f), visor);
            // right arm reaches forward to the weapon, left arm supports it
            var armR = T("ArmR", r.upper, new Vector3(0.28f, 0f, 0));
            Prim.Make(PrimitiveType.Cube, armR, new Vector3(-0.02f, -0.08f, 0.2f), new Vector3(0.13f, 0.13f, 0.46f), r.team).transform.localRotation = Quaternion.Euler(12, -12, 0);
            Prim.Make(PrimitiveType.Sphere, armR, new Vector3(-0.08f, -0.15f, 0.43f), Vector3.one * 0.1f, skin);
            var armL = T("ArmL", r.upper, new Vector3(-0.28f, 0f, 0));
            Prim.Make(PrimitiveType.Cube, armL, new Vector3(0.12f, -0.07f, 0.27f), new Vector3(0.12f, 0.12f, 0.5f), r.team).transform.localRotation = Quaternion.Euler(10, 30, 0);
            Prim.Make(PrimitiveType.Sphere, armL, new Vector3(0.24f, -0.12f, 0.5f), Vector3.one * 0.1f, skin);
            r.hand = T("Hand", r.upper, new Vector3(0.18f, -0.15f, 0.42f));
            r.renderers = r.go.GetComponentsInChildren<Renderer>();
            foreach (var rr in r.renderers) rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return r;
        }

        /// <summary>Name tag floating over a fighter.</summary>
        public static TextMeshPro NameTag(Transform parent, string name, Color c)
        {
            var t = Prim.Text(parent, name, new Vector3(0, 2.2f, 0), 2.6f, c);
            return t;
        }
    }
}
