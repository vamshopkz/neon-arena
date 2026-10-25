using System.Collections.Generic;
using UnityEngine;

namespace Arena.Real
{
    /// <summary>
    /// A person in the world (player, other players, pedestrians, staff): the realistic human from
    /// <see cref="Human.Build"/> (smooth jointed body, face, hair, clothes) driven by
    /// <see cref="HumanAnimator"/>. Crowd people are merged into one mesh per bone so a street full of
    /// them stays cheap on phones.
    /// </summary>
    public class Person : MonoBehaviour
    {
        public SkinSet Look;
        public float Speed;
        public bool Grounded = true;
        /// <summary>0 normal, 1 working with the hands, 2 sitting, 3 waving, 4 driving, 5 typing at a desk, 6 dancing.</summary>
        public int Pose;
        public bool Full;
        public float Height = 1.78f;
        Wardrobe.Built built;
        HumanAnimator anim;
        float workT;

        public HumanRig Rig => built?.human;

        public static Person Create(Transform parent, SkinSet look, string name = "Person", bool full = false, float height = 1.78f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<Person>();
            p.Full = full;
            p.Height = height;
            p.Rebuild(look);
            return p;
        }

        public void Rebuild(SkinSet look)
        {
            Look = (look ?? new SkinSet()).Clone();
            if (built != null && built.root != null) Destroy(built.root);
            built = Human.Build(Look, Height, false);
            built.root.transform.SetParent(transform, false);
            built.root.transform.localPosition = Vector3.zero;
            built.root.transform.localRotation = Quaternion.identity;
            anim = built.humanAnim;
            foreach (var r in built.root.GetComponentsInChildren<Renderer>()) r.gameObject.layer = gameObject.layer;
            if (!Full) Merge();
        }

        /// <summary>One mesh per bone and material (fingers go with the hand, eyelids with the head).</summary>
        void Merge()
        {
            var rig = built.human;
            var joints = new HashSet<Transform>(rig.Joints);
            var groups = new Dictionary<(Transform, Material), List<CombineInstance>>();
            foreach (var mf in built.root.GetComponentsInChildren<MeshFilter>())
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mf.sharedMesh == null || !mr.enabled) continue;
                var j = mf.transform;
                while (j != null && !joints.Contains(j)) j = j.parent;
                if (j == null) continue;
                var mesh = mf.sharedMesh;
                for (int s = 0; s < mesh.subMeshCount && s < mr.sharedMaterials.Length; s++)
                {
                    var key = (j, mr.sharedMaterials[s]);
                    if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = mesh, subMeshIndex = s, transform = j.worldToLocalMatrix * mf.transform.localToWorldMatrix });
                }
                mr.enabled = false;
            }
            foreach (var kv in groups)
            {
                var m = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                m.CombineMeshes(kv.Value.ToArray(), true, true);
                var go = new GameObject("merged");
                go.layer = gameObject.layer;
                go.transform.SetParent(kv.Key.Item1, false);
                go.AddComponent<MeshFilter>().sharedMesh = m;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = kv.Key.Item2;
            }
        }

        void Update()
        {
            if (anim == null) return;
            string st;
            if (Pose == 2) st = "sit";
            else if (Pose == 4) st = "drive";
            else if (Pose == 5) st = "type";
            else if (Pose == 6) st = "dance";
            else if (!Grounded) st = "fall";
            else if (Speed > 4.5f) st = "sprint";
            else if (Speed > 0.25f) st = "walk";
            else if (Pose == 3) st = "wave";
            else if (Pose == 1)
            {
                workT -= Time.deltaTime;
                if (workT <= 0) { workT = 1.4f; anim.Play("interact-right", 0.15f, true); }
                return;
            }
            else st = "idle";
            anim.Play(st, 0.2f);
            if (st == "walk") anim.SetSpeed(Speed / 1.6f);
            else if (st == "sprint") anim.SetSpeed(Speed / 6.5f);
        }

    }
}
