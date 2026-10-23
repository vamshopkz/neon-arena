using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Arena
{
    public class Slot
    {
        public WeaponDef w;
        public string skin;
        public int ammo, reserve;
        public float next, reloadEnd;
    }

    /// <summary>The local first-person fighter: movement, look, weapons, viewmodel.</summary>
    public class PlayerCtl : MonoBehaviour
    {
        public static PlayerCtl I;
        public Camera Cam, VmCam;
        public CharacterController CC;
        public Vector2 MoveInput, LookDelta;
        public bool Frozen;
        // touch buttons
        public bool FireHeld, FireTap, JumpTap, ReloadTap, ZoomToggle, RunToggle;

        public float yaw, pitch, hp = 100;
        public bool alive = true;
        public int team;
        public float respawnAt, invulnUntil;
        public Vector3 vel;
        public readonly List<Slot> Slots = new List<Slot>();
        public int cur;
        public bool zoomed;
        public string lastKiller = "";

        Transform vmRoot, vmModel, muzzle, flash;
        float camY, bobT, kick, recoil, switchT, swingT, throwT, flashT, stepT;
        Vector2 sway;
        int lastSlot;
        bool zoomHeldTouch;
        readonly RaycastHit[] hits = new RaycastHit[32];
        readonly Collider[] overlap = new Collider[32];

        public Slot Cur => cur >= 0 && cur < Slots.Count ? Slots[cur] : null;
        public Vector3 Pos => transform.position;
        public Vector3 Eye => Cam.transform.position;
        public bool Moving => new Vector2(vel.x, vel.z).magnitude > 0.5f;

        public static PlayerCtl Create()
        {
            var go = new GameObject("Player") { layer = Layers.LocalPlayer };
            I = go.AddComponent<PlayerCtl>();
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);
            cc.stepOffset = 0.48f; cc.slopeLimit = 50; cc.skinWidth = 0.04f; cc.minMoveDistance = 0;
            I.CC = cc;

            var camGo = new GameObject("Camera") { tag = "MainCamera" };
            camGo.transform.SetParent(go.transform, false);
            camGo.transform.localPosition = new Vector3(0, 1.6f, 0);
            I.Cam = camGo.AddComponent<Camera>();
            I.Cam.nearClipPlane = 0.05f; I.Cam.farClipPlane = 300; I.Cam.fieldOfView = 72;
            I.Cam.cullingMask = ~((1 << Layers.ViewModel) | (1 << Layers.LocalPlayer) | (1 << Layers.Icon));
            camGo.AddComponent<AudioListener>();
            var data = I.Cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.renderShadows = true;

            var vmGo = new GameObject("VmCamera");
            vmGo.transform.SetParent(camGo.transform, false);
            I.VmCam = vmGo.AddComponent<Camera>();
            I.VmCam.nearClipPlane = 0.01f; I.VmCam.farClipPlane = 5; I.VmCam.fieldOfView = 60;
            I.VmCam.cullingMask = 1 << Layers.ViewModel;
            I.VmCam.clearFlags = CameraClearFlags.Depth;
            var vmData = I.VmCam.GetUniversalAdditionalCameraData();
            vmData.renderType = CameraRenderType.Overlay;
            vmData.renderShadows = false;
            data.cameraStack.Add(I.VmCam);

            I.vmRoot = new GameObject("Viewmodel").transform;
            I.vmRoot.SetParent(vmGo.transform, false);
            I.flash = Prim.Make(PrimitiveType.Quad, vmGo.transform, Vector3.zero, Vector3.one * 0.22f, Mats.Additive(new Color(1f, 0.75f, 0.3f, 0.9f), Tex.Glow()), "Flash", Layers.ViewModel).transform;
            I.flash.gameObject.SetActive(false);
            return I;
        }

        // ------------------------------------------------------------ loadout

        public void BuildLoadout(bool keepCurrent)
        {
            string prev = Cur?.w.id;
            Slots.Clear();
            foreach (var cat in Defs.Cats)
            {
                var id = Save.Slot(cat);
                if (string.IsNullOrEmpty(id) || !Save.Owns(id)) continue;
                var w = Defs.W(id);
                if (w == null) continue;
                Slots.Add(new Slot { w = w, skin = Save.SkinOf(id), ammo = w.mag, reserve = w.reserve });
            }
            int idx = keepCurrent && prev != null ? Slots.FindIndex(s => s.w.id == prev) : -1;
            Equip(idx >= 0 ? idx : 0, true);
            Hud.SlotsChanged();
        }

        public void Equip(int i, bool instant = false)
        {
            if (Slots.Count == 0) { cur = -1; if (vmModel != null) Destroy(vmModel.gameObject); return; }
            i = Mathf.Clamp(i, 0, Slots.Count - 1);
            if (i != cur) lastSlot = cur;
            cur = i;
            var s = Slots[i];
            s.reloadEnd = 0;
            switchT = instant ? 0 : 0.35f;
            if (vmModel != null) Destroy(vmModel.gameObject);
            var go = Models.Weapon(s.w.id, s.skin, vmRoot, Layers.ViewModel);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            vmModel = go.transform;
            muzzle = vmModel.Find("Muzzle");
            Hud.SlotsChanged();
        }

        public void Refill()
        {
            foreach (var s in Slots) { s.ammo = s.w.mag; s.reserve = s.w.reserve; s.reloadEnd = 0; }
        }

        // ------------------------------------------------------------ placement

        public void Teleport(Vector3 p, float yawDeg)
        {
            CC.enabled = false;
            transform.position = p;
            CC.enabled = true;
            yaw = yawDeg; pitch = 0; vel = Vector3.zero;
            camY = p.y + 1.6f;
        }

        public void Spawn(Spawn s)
        {
            var jitter = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Teleport(s.pos + jitter, s.yaw);
            hp = 100; alive = true;
            invulnUntil = Time.time + 1.5f;
            Refill();
            Equip(cur < 0 ? 0 : cur, true);
        }

        public bool InputBlocked => Frozen || UIRoot.Open != null || Hud.Blocking;

        // ------------------------------------------------------------ update

        void Update()
        {
            float dt = Time.deltaTime;
            var g = Game.I;
            bool canAct = !InputBlocked && alive && (g == null || g.CanAct);
            bool desktop = !Application.isMobilePlatform;

            // look
            if (!InputBlocked)
            {
                float sens = Save.P.sens * (zoomed && Cur != null && Cur.w.zoom > 0 ? 1f / Cur.w.zoom : 1f);
                if (desktop && Cursor.lockState == CursorLockMode.Locked)
                {
                    float mx = Input.GetAxisRaw("Mouse X"), my = Input.GetAxisRaw("Mouse Y");
                    yaw += mx * 2.2f * sens; pitch -= my * 2.2f * sens;
                    sway += new Vector2(-mx, -my) * 0.0025f;
                }
                yaw += LookDelta.x * 0.13f * sens; pitch += LookDelta.y * 0.13f * sens;
                sway += new Vector2(-LookDelta.x, LookDelta.y) * 0.0002f;
            }
            LookDelta = Vector2.zero;
            pitch = Mathf.Clamp(pitch, -85, 85);
            sway = Vector2.ClampMagnitude(sway * Mathf.Exp(-dt * 8), 0.04f);

            // move
            Vector2 mv = Vector2.zero;
            bool run = false, jump = false;
            if (canAct)
            {
                if (desktop)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) mv.y += 1;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) mv.y -= 1;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) mv.x += 1;
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) mv.x -= 1;
                    run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    jump = Input.GetKeyDown(KeyCode.Space);
                }
                mv += MoveInput;
                if (MoveInput.y > 0.85f) run = true;
                jump |= JumpTap;
            }
            JumpTap = false;
            if (mv.magnitude > 1) mv.Normalize();
            float speed = (run && mv.y > 0.3f ? 8.2f : 5.6f) * (zoomed ? 0.5f : 1f);
            var q = Quaternion.Euler(0, yaw, 0);
            var wish = q * new Vector3(mv.x, 0, mv.y) * speed;
            float acc = CC.isGrounded ? 14 : 3;
            float k = 1 - Mathf.Exp(-acc * dt);
            vel.x += (wish.x - vel.x) * k; vel.z += (wish.z - vel.z) * k;
            if (CC.isGrounded)
            {
                if (vel.y < -2) vel.y = -2;
                if (jump) { vel.y = 6.4f; AudioFx.Play("jump", 0.5f); }
            }
            vel.y -= 20 * dt;
            if (alive && CC.enabled) CC.Move(vel * dt);
            if (CC.isGrounded && vel.y < -2) vel.y = -2;
            if (transform.position.y < -20 && g != null) g.FellOut();

            // footsteps
            if (CC.isGrounded && Moving) { stepT += dt * new Vector2(vel.x, vel.z).magnitude; if (stepT > 2.4f) { stepT = 0; AudioFx.Play("step", 0.35f); } }

            // camera (smooth only when stepping up)
            float eye = transform.position.y + (alive ? 1.6f : 0.35f);
            camY = eye > camY ? Mathf.Lerp(camY, eye, 1 - Mathf.Exp(-dt * 18)) : eye;
            Cam.transform.position = new Vector3(transform.position.x, camY, transform.position.z);
            recoil *= Mathf.Exp(-dt * 9);
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Cam.transform.localRotation = Quaternion.Euler(pitch - recoil, 0, alive ? 0 : 25);

            Weapons(dt, canAct, desktop);
        }

        // ------------------------------------------------------------ weapons

        void Weapons(float dt, bool canAct, bool desktop)
        {
            var s = Cur;
            if (switchT > 0) switchT = Mathf.Max(0, switchT - dt);
            if (s == null) { zoomed = false; return; }
            var w = s.w;
            float now = Time.time;
            if (s.reloadEnd > 0 && now >= s.reloadEnd)
            {
                int n = Mathf.Min(w.mag - s.ammo, s.reserve);
                s.ammo += n; s.reserve -= n; s.reloadEnd = 0;
            }

            if (canAct)
            {
                bool clickDown = desktop && Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButtonDown(0);
                bool held = (desktop && Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButton(0)) || FireHeld;
                if ((w.type == WType.Auto || w.type == WType.Melee) && held) TryFire();
                else if (clickDown || FireTap) TryFire();

                if (desktop)
                {
                    if (Input.GetKeyDown(KeyCode.R)) Reload();
                    for (int i = 0; i < 5; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < Slots.Count) Equip(i);
                    float wheel = Input.mouseScrollDelta.y;
                    if (wheel > 0.1f) Next(-1); else if (wheel < -0.1f) Next(1);
                    if (Input.GetKeyDown(KeyCode.Q) && lastSlot >= 0 && lastSlot < Slots.Count) Equip(lastSlot);
                }
                if (ReloadTap) Reload();
                if (ZoomToggle) zoomHeldTouch = !zoomHeldTouch;
            }
            FireTap = ReloadTap = ZoomToggle = false;
            bool zoomWant = (desktop && Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButton(1)) || zoomHeldTouch;
            zoomed = canAct && zoomWant && w.zoom > 0 && s.reloadEnd == 0 && switchT == 0;
            if (w.zoom <= 0) zoomHeldTouch = false;
            float fov = zoomed ? 70f / w.zoom : 72f;
            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, fov, 1 - Mathf.Exp(-dt * 16));

            Viewmodel(dt, s, now);
        }

        public void Next(int d)
        {
            if (Slots.Count == 0) return;
            Equip((cur + d + Slots.Count) % Slots.Count);
        }

        public void Reload()
        {
            var s = Cur;
            if (s == null || !s.w.Hitscan && s.w.type != WType.Rocket) return;
            if (s.reloadEnd > 0 || s.ammo >= s.w.mag || s.reserve <= 0) return;
            s.reloadEnd = Time.time + s.w.reload;
            AudioFx.Play("reload", 0.7f);
        }

        void TryFire()
        {
            var s = Cur;
            if (s == null) return;
            var w = s.w;
            float now = Time.time;
            if (now < s.next || switchT > 0 || s.reloadEnd > 0) return;
            var g = Game.I;
            if (w.type == WType.Melee)
            {
                s.next = now + 1f / w.rate;
                swingT = 0.0001f;
                AudioFx.Play("melee", 0.8f);
                Melee(w);
                return;
            }
            if (s.ammo <= 0)
            {
                if (w.Hitscan || w.type == WType.Rocket) Reload();
                else { AudioFx.Play("empty"); s.next = now + 0.3f; }
                return;
            }
            s.ammo--;
            s.next = now + 1f / w.rate;
            kick = 0.06f + w.recoil;
            recoil += w.recoil * Mathf.Rad2Deg * 1.2f;
            var camT = Cam.transform;
            if (w.Projectile)
            {
                var dir = camT.forward;
                var origin = camT.position + dir * 0.5f + camT.right * 0.12f - camT.up * 0.1f;
                var v = dir * w.speed + (w.type == WType.Rocket ? Vector3.zero : Vector3.up * 2.5f);
                g.LaunchProjectile(w, s.skin, origin, v, Save.P.id, team, true);
                if (w.type != WType.Rocket) throwT = 0.0001f;
                if (w.type == WType.Rocket) { Flash(); AudioFx.Play("rocket"); }
                else AudioFx.Play("throw");
                if (s.ammo == 0 && w.type == WType.Rocket) Invoke(nameof(Reload), 0.3f);
                if (s.ammo == 0 && w.type != WType.Rocket) Invoke(nameof(SwitchFromEmpty), 0.5f);
                return;
            }
            // hitscan
            float spread = w.spread + (Moving ? w.moveSpread : 0) + (CC.isGrounded ? 0 : 0.05f);
            if (zoomed) spread = 0.0015f;
            var d = (camT.forward + camT.right * Random.Range(-spread, spread) + camT.up * Random.Range(-spread, spread)).normalized;
            var end = camT.position + d * w.range;
            int n = Physics.RaycastNonAlloc(camT.position, d, hits, w.range, Layers.ShotMask, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, 0, n, HitCmp.I);
            for (int i = 0; i < n; i++)
            {
                var h = hits[i];
                var ht = h.collider.GetComponent<Hittable>();
                if (ht == null) { end = h.point; g.Impact(h.point, h.normal); break; }
                if (ht.kind == 0)
                {
                    if (!g.IsEnemy(ht.id)) continue;
                    end = h.point;
                    g.DealDamage(ht.id, w.dmg * (ht.head ? 2 : 1), w.id, ht.head);
                    g.Blood(h.point);
                    break;
                }
                if (ht.kind == 1) { end = h.point; ht.target.Hit(); Hud.HitMarker(false); AudioFx.Play("hit"); break; }
                if (ht.kind == 2) { end = h.point; g.KickBall(d * 6); break; }
            }
            Flash();
            var mpos = MuzzleWorld();
            g.Tracer(mpos, end, true);
            g.SendShot(mpos, end, w.id);
            AudioFx.Play(w.sound, 0.8f);
            if (s.ammo == 0 && s.reserve > 0) Invoke(nameof(Reload), 0.25f);
        }

        void SwitchFromEmpty()
        {
            var s = Cur;
            if (s != null && s.ammo == 0 && !s.w.Hitscan && s.w.type != WType.Rocket) Equip(0);
        }

        class HitCmp : IComparer<RaycastHit>
        {
            public static readonly HitCmp I = new HitCmp();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }

        void Melee(WeaponDef w)
        {
            var g = Game.I;
            var c = Cam.transform;
            int n = Physics.OverlapSphereNonAlloc(c.position + c.forward * 1.1f, 1.3f, overlap, (1 << Layers.Hitbox) | (1 << Layers.Ball), QueryTriggerInteraction.Collide);
            Hittable best = null; float bd = 99;
            for (int i = 0; i < n; i++)
            {
                var ht = overlap[i].GetComponent<Hittable>();
                if (ht == null) continue;
                if (ht.kind == 0 && !g.IsEnemy(ht.id)) continue;
                var to = overlap[i].bounds.center - c.position;
                if (to.magnitude > w.reach + 0.6f || Vector3.Dot(to.normalized, c.forward) < 0.4f) continue;
                if (to.magnitude < bd) { bd = to.magnitude; best = ht; }
            }
            if (best == null) return;
            if (best.kind == 0) { g.DealDamage(best.id, w.dmg, w.id, false); g.Blood(best.transform.position); }
            else if (best.kind == 1) { best.target.Hit(); Hud.HitMarker(false); }
            else g.KickBall(c.forward * 8);
        }

        void Flash() { flashT = 0.05f; }

        public Vector3 MuzzleWorld()
        {
            if (muzzle == null) return Cam.transform.position + Cam.transform.forward * 0.6f;
            // viewmodel camera space → main camera space (same transform, different FOV: close enough)
            var local = VmCam.transform.InverseTransformPoint(muzzle.position);
            return Cam.transform.TransformPoint(local);
        }

        static readonly Dictionary<string, Vector3> Off = new Dictionary<string, Vector3>
        {
            { "primary", new Vector3(0.18f, -0.21f, 0.5f) }, { "pistol", new Vector3(0.16f, -0.17f, 0.4f) }, { "melee", new Vector3(0.2f, -0.18f, 0.34f) },
            { "throw", new Vector3(0.18f, -0.16f, 0.36f) }, { "grenade", new Vector3(0.18f, -0.17f, 0.34f) }, { "rpg7", new Vector3(0.21f, -0.11f, 0.12f) },
        };

        void Viewmodel(float dt, Slot s, float now)
        {
            if (vmModel == null) return;
            var w = s.w;
            var basePos = Off.TryGetValue(w.id, out var o) ? o : Off[w.cat];
            float sp = new Vector2(vel.x, vel.z).magnitude;
            bobT += dt * sp * 1.7f;
            float bob = CC.isGrounded ? Mathf.Min(1, sp / 6) : 0;
            var pos = basePos + new Vector3(Mathf.Cos(bobT) * 0.012f * bob + sway.x, Mathf.Abs(Mathf.Sin(bobT)) * 0.014f * bob + sway.y - switchT * 0.9f, -kick);
            var rot = Vector3.zero;
            if (w.cat == "melee") rot = new Vector3(-12, -18, -10);
            if (w.cat == "pistol") rot = new Vector3(2, -6, 0);
            if (w.cat == "primary" && w.id != "rpg7") rot = new Vector3(1, -4, 0);
            if (w.id == "shuriken") rot = new Vector3(-50, 0, 0);
            if (s.reloadEnd > 0)
            {
                float p = Mathf.Clamp01(1 - (s.reloadEnd - now) / w.reload);
                float kk = Mathf.Sin(Mathf.PI * p);
                pos.y -= 0.1f * kk; rot += new Vector3(30 * kk, 0, 20 * kk);
            }
            if (swingT > 0)
            {
                swingT += dt;
                float p = swingT / 0.3f;
                if (p >= 1) swingT = 0;
                else { float kk = Mathf.Sin(Mathf.PI * p); rot += new Vector3(55 * kk, -45 * kk, 0); pos += new Vector3(-0.1f * kk, 0, 0.12f * kk); }
            }
            if (throwT > 0)
            {
                throwT += dt;
                float p = throwT / 0.35f;
                if (p >= 1) throwT = 0;
                else { float kk = Mathf.Sin(Mathf.PI * p); rot += new Vector3(-40 * kk, 0, 0); pos += new Vector3(0, 0.08f * kk, 0.15f * kk); }
            }
            kick *= Mathf.Exp(-dt * 14);
            vmModel.localPosition = pos;
            vmModel.localRotation = Quaternion.Euler(rot);
            bool hasAmmo = s.ammo > 0 || w.Hitscan || w.type == WType.Melee || w.type == WType.Rocket;
            vmRoot.gameObject.SetActive(alive && !zoomed && hasAmmo);
            flashT -= dt;
            bool fl = flashT > 0 && muzzle != null && w.Hitscan || flashT > 0 && w.type == WType.Rocket;
            flash.gameObject.SetActive(fl && !zoomed);
            if (fl && muzzle != null) { flash.position = muzzle.position; flash.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360)) * VmCam.transform.rotation; }
        }

        // ------------------------------------------------------------ damage

        public void TakeDamage(float dmg, string by, string weapon)
        {
            if (!alive || Time.time < invulnUntil) return;
            hp -= dmg;
            Hud.Damage();
            AudioFx.Play("hurt", 0.8f);
            if (hp <= 0) { hp = 0; Die(by, weapon); }
        }

        public void Heal(float v) { hp = Mathf.Min(100, hp + v); }

        void Die(string by, string weapon)
        {
            alive = false;
            zoomed = false;
            respawnAt = Time.time + 3.5f;
            lastKiller = by;
            Game.I.LocalDied(by, weapon);
        }
    }
}
