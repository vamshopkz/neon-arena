using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Arena
{
    /// <summary>
    /// Party networking over the public MQTT relays (same transport as Hay Hunter / ГАД 2-2 X): every
    /// player owns a room (its code = player id); a friend who types the code joins that room's topic.
    /// The room owner is the host (votes, bots, score). Messages are text lines "TYPE|sender|seq|args…",
    /// batched every 0.1 s. Important ones ("!" prefix) are repeated a few times and de-duplicated.
    /// </summary>
    public class Net : MonoBehaviour
    {
        public static Net I;
        static readonly string[] Brokers = { "broker.emqx.io", "broker.hivemq.com" };
        public const string Prefix = "neonarena/v1/r/";
        public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public string Room = "";
        public bool IsHost => Room == Save.P.id;
        public bool Connected => links.Exists(l => l != null && l.Ready);
        public float JoinedAt;
        public bool HostSeen;

        public class Member { public string id, name; public float heard; }
        public readonly Dictionary<string, Member> Members = new Dictionary<string, Member>();

        public Action<string, string, string[]> OnMessage;   // type, sender, args
        public Action<string> OnMemberLeft;

        readonly List<ILink> links = new List<ILink>();
        readonly float[] retryAt = new float[2];
        readonly List<NetIncoming> inbox = new List<NetIncoming>();
        readonly Dictionary<string, int> lastSeq = new Dictionary<string, int>();
        readonly HashSet<string> seen = new HashSet<string>();
        readonly Queue<string> seenOrder = new Queue<string>();
        readonly List<string> outbox = new List<string>();
        readonly List<(string line, float at)> repeats = new List<(string, float)>();
        int seq, mid;
        float flushT;
        public int Sent, Received;

        /// <summary>Test hook: when set, lines are passed here instead of the relays (two games in one process).</summary>
        public static Action<string> LoopbackOut;

        public static void Create()
        {
            var go = new GameObject("Net");
            I = go.AddComponent<Net>();
            for (int i = 0; i < Brokers.Length; i++) I.links.Add(null);
            I.Room = Save.P.id;
            I.AddMember(Save.P.id, Save.P.name);
        }

        public void AddMember(string id, string name)
        {
            if (!Members.TryGetValue(id, out var m)) { m = new Member { id = id }; Members[id] = m; }
            if (!string.IsNullOrEmpty(name)) m.name = name;
            m.heard = Time.unscaledTime;
        }

        public string NameOf(string id) => Members.TryGetValue(id, out var m) && !string.IsNullOrEmpty(m.name) ? m.name : id;

        public void JoinRoom(string code)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (code.Length == 0) code = Save.P.id;
            Room = code;
            foreach (var l in links) l?.Close();
            for (int i = 0; i < links.Count; i++) { links[i] = null; retryAt[i] = 0; }
            Members.Clear();
            AddMember(Save.P.id, Save.P.name);
            lastSeq.Clear();
            JoinedAt = Time.unscaledTime;
            HostSeen = IsHost;
            Save.P.lastRoom = IsHost ? "" : code;
            Save.Write();
            if (!IsHost) SendR("HELLO", Save.P.name);
        }

        public void LeaveRoom() => JoinRoom(Save.P.id);

        static string Clean(string s) => (s ?? "").Replace("|", "/").Replace("\n", " ").Replace(";", ",").Replace(":", ".");
        public static string Name(string s) => Clean(s);
        public static string F(float v) => v.ToString("0.##", Inv);
        public static float PF(string s) => float.TryParse(s, NumberStyles.Float, Inv, out var v) ? v : 0;
        public static int PI(string s) => int.TryParse(s, out var v) ? v : 0;

        /// <summary>Unreliable (state): newest wins.</summary>
        public void Send(string type, params string[] args)
        {
            var sb = new StringBuilder(type).Append('|').Append(Save.P.id).Append('|').Append(++seq);
            foreach (var a in args) sb.Append('|').Append(a);
            outbox.Add(sb.ToString());
        }

        /// <summary>Reliable-ish: sent now and repeated 3 more times; receivers drop duplicates.</summary>
        public void SendR(string type, params string[] args)
        {
            var sb = new StringBuilder("!").Append(type).Append('|').Append(Save.P.id).Append('|').Append(++mid).Append('_').Append(UnityEngine.Random.Range(0, 99999));
            foreach (var a in args) sb.Append('|').Append(a);
            var line = sb.ToString();
            outbox.Add(line);
            float t = Time.unscaledTime;
            repeats.Add((line, t + 0.2f)); repeats.Add((line, t + 0.6f)); repeats.Add((line, t + 1.4f));
        }

        string Topic => Prefix + Room;

        void Update()
        {
            if (LoopbackOut == null)
            {
                for (int i = 0; i < Brokers.Length; i++)
                {
                    var l = links[i];
                    if ((l == null || l.Failed) && Time.unscaledTime >= retryAt[i])
                    {
                        l?.Close();
                        links[i] = MqttLink.Create(Brokers[i], Topic, Topic);
                        retryAt[i] = Time.unscaledTime + 5f;
                    }
                }
                inbox.Clear();
                foreach (var l in links) l?.Poll(inbox);
                foreach (var m in inbox) Receive(m.text);
            }

            float now = Time.unscaledTime;
            for (int i = repeats.Count - 1; i >= 0; i--)
                if (now >= repeats[i].at) { outbox.Add(repeats[i].line); repeats.RemoveAt(i); }

            flushT += Time.unscaledDeltaTime;
            if (flushT >= 0.1f && outbox.Count > 0)
            {
                flushT = 0;
                var packet = string.Join("\n", outbox);
                outbox.Clear();
                Sent++;
                if (LoopbackOut != null) LoopbackOut(packet);
                else foreach (var l in links) if (l != null && l.Ready) l.Send(packet);
            }

            // drop silent members
            List<string> gone = null;
            foreach (var kv in Members)
                if (kv.Key != Save.P.id && now - kv.Value.heard > 9f) (gone ??= new List<string>()).Add(kv.Key);
            if (gone != null) foreach (var id in gone) { Members.Remove(id); OnMemberLeft?.Invoke(id); }
        }

        /// <summary>A packet from the relay (or the loopback test): one or more lines.</summary>
        public void Receive(string packet)
        {
            Received++;
            foreach (var raw in packet.Split('\n'))
            {
                if (raw.Length < 3) continue;
                bool reliable = raw[0] == '!';
                var p = (reliable ? raw.Substring(1) : raw).Split('|');
                if (p.Length < 3) continue;
                string type = p[0], sender = p[1];
                if (sender == Save.P.id) continue;
                if (reliable)
                {
                    string key = sender + "/" + p[2];
                    if (!seen.Add(key)) continue;
                    seenOrder.Enqueue(key);
                    if (seenOrder.Count > 3000) seen.Remove(seenOrder.Dequeue());
                }
                else
                {
                    int s = PI(p[2]);
                    string key = type + sender;
                    if (lastSeq.TryGetValue(key, out int last) && s <= last && last - s < 5000) continue;
                    lastSeq[key] = s;
                }
                if (sender == Room) HostSeen = true;
                AddMember(sender, null);
                var args = new string[p.Length - 3];
                Array.Copy(p, 3, args, 0, args.Length);
                if (type == "HELLO" && args.Length > 0) AddMember(sender, args[0]);
                try { OnMessage?.Invoke(type, sender, args); }
                catch (Exception e) { Debug.LogWarning("[Net] " + type + ": " + e); }
            }
        }

        void OnApplicationQuit() { foreach (var l in links) l?.Close(); }
    }
}
