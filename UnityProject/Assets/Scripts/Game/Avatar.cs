using TMPro;
using UnityEngine;
using Arena.Real;

namespace Arena
{
    /// <summary>
    /// Another fighter as seen here (a friend or a bot): the realistic human from ГАД 2-2 X (jointed
    /// body, face, hair, clothes) in the team's colours, holding the weapon with its skin; name tag, hitboxes.
    /// </summary>
    public class Avatar : MonoBehaviour
    {
        public string Id, Name;
        public int Team = -1;
        public string Scene = "";
        public bool Alive = true;
        public float Hp = 100, LastHeard;
        public Vector3 target;
        public float tYaw, tPitch;
        public bool Smooth = true;
        Person person;
        Transform model, aimPivot, gunHolder, headBone;
        TextMeshPro nameTag;
        GameObject weapon;
        string wid, wsk;
        float deathK, yaw, pitch, speed;
        Vector3 lastPos;
        Transform flash;
        float flashT;
        Hittable body, head;

        public static Avatar Create(string id, string name, Transform parent)
        {
            var go = new GameObject("Avatar " + id);
            go.transform.SetParent(parent, false);
            var a = go.AddComponent<Avatar>();
            a.Id = id; a.Name = name;
            a.model = new GameObject("Model").transform;
            a.model.SetParent(go.transform, false);
            a.aimPivot = new GameObject("AimPivot").transform;
            a.aimPivot.SetParent(a.model, false);
            a.aimPivot.localPosition = new Vector3(0, 1.42f, 0);
            a.gunHolder = new GameObject("Gun").transform;
            a.gunHolder.SetParent(a.aimPivot, false);
            a.gunHolder.localPosition = new Vector3(0.12f, -0.16f, 0.36f);
            a.nameTag = Models.NameTag(go.transform, name, Color.white);
            a.body = Hitbox(go.transform, id, false, new Vector3(0, 0.75f, 0), 0.34f, 1.5f);
            a.head = Hitbox(go.transform, id, true, new Vector3(0, 1.64f, 0.02f), 0.16f, 0);
            a.flash = Prim.Make(PrimitiveType.Quad, go.transform, Vector3.zero, Vector3.one * 0.5f, Mats.Additive(new Color(1f, 0.75f, 0.3f, 0.9f), Tex.Glow()), "Flash").transform;
            a.flash.gameObject.SetActive(false);
            a.SetTeam(0);
            return a;
        }

        static Hittable Hitbox(Transform parent, string id, bool isHead, Vector3 pos, float r, float h)
        {
            var go = new GameObject(isHead ? "HeadBox" : "BodyBox") { layer = Layers.Hitbox };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            if (isHead) { var s = go.AddComponent<SphereCollider>(); s.radius = r; s.isTrigger = true; }
            else { var c = go.AddComponent<CapsuleCollider>(); c.radius = r; c.height = h; c.isTrigger = true; }
            var ht = go.AddComponent<Hittable>();
            ht.id = id; ht.head = isHead; ht.kind = 0;
            return ht;
        }

        /// <summary>A fighter's outfit: own face/hair/skin from the id, a hoodie + cap in the team colour.</summary>
        public static SkinSet LookFor(string id, int team)
        {
            uint h = 2166136261u;
            foreach (char ch in id) h = (h ^ ch) * 16777619u;
            int R(int n, int salt) => (int)(((h >> (salt % 24)) ^ (h * (uint)(salt * 2654435761u))) % (uint)n);
            bool female = R(100, 3) < 35;
            string[] skins = { "hs_light", "hs_peach", "hs_tan", "hs_olive", "hs_brown", "hs_dark" };
            string[] hairM = { "hh_short_black", "hh_short_brown", "hh_short_blonde", "hh_buzz_black", "hh_buzz_brown" };
            string[] hairF = { "hh_long_brown", "hh_long_black", "hh_long_blonde", "hh_ponytail_brown", "hh_bun_black", "hh_ponytail_blonde" };
            string col = team == 1 ? "blue" : team == 2 ? "orange" : "purple";
            var s = new SkinSet
            {
                hGender = female ? "hg_f" : "hg_m",
                hSkin = skins[R(skins.Length, 5)],
                hHair = female ? hairF[R(hairF.Length, 7)] : hairM[R(hairM.Length, 9)],
                hEyes = female ? "he_lashes" : (R(2, 11) == 0 ? "he_brown" : "he_blue"),
                hMouth = female ? "hm_lips" : (R(4, 13) == 0 ? "hm_beard" : "hm_neutral"),
                hFace = new[] { "hface_oval", "hface_square", "hface_long", "hface_heart" }[R(4, 15)],
                hTop = "ht_hoodie_" + col,
                hBottom = R(2, 17) == 0 ? "hb_cargo_olive" : "hb_cargo_khaki",
                hShoes = "hf_boots_black",
                hAcc = R(3, 19) == 0 ? "ha_sunglasses" : "ha_none",
            };
            if (Wardrobe.Find(s.hTop) == null) s.hTop = "ht_tee_" + col;
            if (Wardrobe.Find(s.hTop) == null) s.hTop = "ht_tee_white";
            if (Wardrobe.Find(s.hHair) == null) s.hHair = female ? "hh_long_brown" : "hh_short_brown";
            if (Wardrobe.Find(s.hBottom) == null) s.hBottom = "hb_pants_black";
            if (Wardrobe.Find(s.hShoes) == null) s.hShoes = "hf_sneakers_white";
            return s;
        }

        public void SetTeam(int team)
        {
            if (team == Team) return;
            Team = team;
            if (person != null) Destroy(person.gameObject);
            person = Person.Create(model, LookFor(Id, team), "Body", false, 1.8f);
            foreach (var r in person.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            headBone = person.Rig != null ? person.Rig.head : null;
            var c = Defs.TeamColor(team);
            nameTag.color = Color.Lerp(c, Color.white, 0.4f);
        }

        public void SetName(string n) { if (n != Name && !string.IsNullOrEmpty(n)) { Name = n; nameTag.text = n; } }

        public void SetWeapon(string id, string skin)
        {
            if (id == wid && skin == wsk) return;
            wid = id; wsk = skin;
            if (weapon != null) Destroy(weapon);
            if (string.IsNullOrEmpty(id) || Defs.W(id) == null) return;
            weapon = Models.Weapon(id, skin, gunHolder, 0);
            var w = Defs.W(id);
            if (id == "rpg7") weapon.transform.localPosition = new Vector3(0.02f, 0.22f, -0.2f);
            else if (w.cat == "pistol") weapon.transform.localPosition = new Vector3(-0.1f, 0.06f, 0.15f);
            else if (w.cat == "primary") weapon.transform.localPosition = new Vector3(-0.02f, 0, -0.05f);
        }

        public void SetState(Vector3 pos, float yawDeg, float pitchDeg, bool alive, float hp)
        {
            bool first = LastHeard == 0;
            target = pos; tYaw = yawDeg; tPitch = pitchDeg;
            Hp = hp;
            if (alive != Alive) { Alive = alive; if (!alive) deathK = 0; }
            if (first || (pos - transform.position).sqrMagnitude > 100) { transform.position = pos; yaw = yawDeg; }
            LastHeard = Time.unscaledTime;
        }

        public void MuzzleFlash() { flashT = 0.06f; }

        public Vector3 MuzzlePos()
        {
            var m = weapon != null ? weapon.transform.Find("Muzzle") : null;
            return m != null ? m.position : transform.position + Vector3.up * 1.3f + transform.forward * 0.6f;
        }

        public Vector3 Chest => transform.position + Vector3.up * 1.2f;

        public bool Visible
        {
            set { if (model.gameObject.activeSelf != value) { model.gameObject.SetActive(value); nameTag.gameObject.SetActive(value); body.gameObject.SetActive(value); } }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (Smooth) transform.position = Vector3.Lerp(transform.position, target, 1 - Mathf.Exp(-dt * 12));
            yaw = Mathf.LerpAngle(yaw, tYaw, 1 - Mathf.Exp(-dt * 14));
            pitch = Mathf.Lerp(pitch, tPitch, 1 - Mathf.Exp(-dt * 14));
            var mv = transform.position - lastPos; mv.y = 0;
            speed = Mathf.Lerp(speed, mv.magnitude / Mathf.Max(dt, 0.001f), 1 - Mathf.Exp(-dt * 8));
            lastPos = transform.position;
            if (person != null) { person.Speed = Alive ? Mathf.Min(speed, 8f) : 0; person.Grounded = true; }
            if (!Alive) deathK = Mathf.Min(1, deathK + dt * 3);
            else deathK = 0;
            transform.rotation = Quaternion.Euler(0, yaw, 0) * Quaternion.Euler(-90 * deathK, 0, 0);
            aimPivot.localRotation = Quaternion.Euler(Mathf.Clamp(pitch, -60, 60), 0, 0);
            bool vis = model.gameObject.activeSelf;
            head.gameObject.SetActive(Alive && vis);
            body.gameObject.SetActive(Alive && vis);
            var cam = PlayerCtl.I != null ? PlayerCtl.I.Cam : null;
            if (cam != null) nameTag.transform.rotation = Quaternion.LookRotation(nameTag.transform.position - cam.transform.position);
            nameTag.gameObject.SetActive(Alive && vis);
            flashT -= dt;
            bool f = flashT > 0 && vis;
            flash.gameObject.SetActive(f);
            if (f && cam != null) { flash.position = MuzzlePos(); flash.rotation = Quaternion.LookRotation(flash.position - cam.transform.position); }
        }

        /// <summary>After the walk/idle animation: arms raised to hold the weapon, aiming up/down with the view.</summary>
        void LateUpdate()
        {
            var rig = person != null ? person.Rig : null;
            if (rig == null || !Alive || weapon == null) return;
            float p = Mathf.Clamp(pitch, -50, 50);
            var w = Defs.W(wid);
            bool melee = w != null && (w.cat == "melee" || w.cat == "throw" || w.cat == "grenade");
            rig.curlR = 0.95f;
            if (melee)
            {
                rig.uArmR.localRotation = Quaternion.Euler(-25 + p * 0.5f, 0, 8);
                rig.fArmR.localRotation = Quaternion.Euler(-60, 0, 0);
                return;
            }
            rig.curlL = 0.8f;
            rig.uArmR.localRotation = Quaternion.Euler(-30 + p * 0.7f, -10, 6);
            rig.fArmR.localRotation = Quaternion.Euler(-72, 0, 0);
            rig.handR.localRotation = Quaternion.Euler(0, 0, 0);
            rig.uArmL.localRotation = Quaternion.Euler(-48 + p * 0.7f, 28, -4);
            rig.fArmL.localRotation = Quaternion.Euler(-52, 0, 0);
        }
    }
}
