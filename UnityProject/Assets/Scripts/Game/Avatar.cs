using TMPro;
using UnityEngine;

namespace Arena
{
    /// <summary>Another fighter as seen here (a friend or a bot): model, weapon with skin, name tag, hitboxes.</summary>
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
        Models.Rig rig;
        TextMeshPro nameTag;
        GameObject weapon;
        string wid, wsk;
        float walk, deathK, yaw, pitch;
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
            a.rig = Models.Fighter(go.transform, Defs.Neutral);
            a.nameTag = Models.NameTag(go.transform, name, Color.white);
            a.body = Hitbox(go.transform, id, false, new Vector3(0, 0.75f, 0), 0.36f, 1.5f);
            a.head = Hitbox(a.rig.head, id, true, Vector3.zero, 0.24f, 0);
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

        public void SetTeam(int team)
        {
            if (team == Team) return;
            Team = team;
            var c = Defs.TeamColor(team);
            rig.team.color = c;
            nameTag.color = Color.Lerp(c, Color.white, 0.4f);
        }

        public void SetName(string n) { if (n != Name && !string.IsNullOrEmpty(n)) { Name = n; nameTag.text = n; } }

        public void SetWeapon(string id, string skin)
        {
            if (id == wid && skin == wsk) return;
            wid = id; wsk = skin;
            if (weapon != null) Destroy(weapon);
            if (string.IsNullOrEmpty(id) || Defs.W(id) == null) return;
            weapon = Models.Weapon(id, skin, rig.hand, 0);
            if (id == "rpg7") weapon.transform.localPosition = new Vector3(0.05f, 0.25f, -0.15f);
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
            set { if (rig.go.activeSelf != value) { rig.go.SetActive(value); nameTag.gameObject.SetActive(value); body.gameObject.SetActive(value); } }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (Smooth)
            {
                var p = Vector3.Lerp(transform.position, target, 1 - Mathf.Exp(-dt * 12));
                transform.position = p;
            }
            yaw = Mathf.LerpAngle(yaw, tYaw, 1 - Mathf.Exp(-dt * 14));
            pitch = Mathf.Lerp(pitch, tPitch, 1 - Mathf.Exp(-dt * 14));
            float sp = (transform.position - lastPos).magnitude / Mathf.Max(dt, 0.001f);
            lastPos = transform.position;
            walk += dt * Mathf.Min(sp, 9) * 1.9f;
            float sw = Mathf.Sin(walk) * Mathf.Min(1, sp / 4) * 35;
            rig.legL.localRotation = Quaternion.Euler(sw, 0, 0);
            rig.legR.localRotation = Quaternion.Euler(-sw, 0, 0);
            rig.upper.localRotation = Quaternion.Euler(pitch * 0.8f, 0, 0);
            if (!Alive) deathK = Mathf.Min(1, deathK + dt * 3);
            else deathK = 0;
            transform.rotation = Quaternion.Euler(0, yaw, 0) * Quaternion.Euler(-90 * deathK, 0, 0);
            head.gameObject.SetActive(Alive && rig.go.activeSelf);
            body.gameObject.SetActive(Alive && rig.go.activeSelf);
            var cam = PlayerCtl.I != null ? PlayerCtl.I.Cam : null;
            if (cam != null) nameTag.transform.rotation = Quaternion.LookRotation(nameTag.transform.position - cam.transform.position);
            nameTag.gameObject.SetActive(Alive && rig.go.activeSelf);
            flashT -= dt;
            bool f = flashT > 0 && rig.go.activeSelf;
            flash.gameObject.SetActive(f);
            if (f && cam != null) { flash.position = MuzzlePos(); flash.rotation = Quaternion.LookRotation(flash.position - cam.transform.position); }
        }
    }
}
