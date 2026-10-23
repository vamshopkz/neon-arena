using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Arena
{
    public struct NetIncoming
    {
        public string text;
        public ILink link;
    }

    /// <summary>One way of reaching the other phone: direct Wi-Fi (UDP) or the public relay (MQTT).</summary>
    public interface ILink
    {
        string Name { get; }
        bool Ready { get; }
        bool Failed { get; }
        void Send(string msg);
        void Poll(List<NetIncoming> into);
        void Close();
    }

    // =========================================================================
    // Internet: minimal MQTT 3.1.1 client (QoS 0) talking to a free public broker.
    // The broker just forwards messages between the two phones on the room's topics.
    // =========================================================================

    public class MqttLink : ILink
    {
        readonly string broker, subTopic, pubTopic;
        TcpClient tcp;
        NetworkStream stream;
        volatile int state; // 0 connecting, 1 ready, 2 failed
        readonly byte[] buf = new byte[65536];
        int len;
        DateTime lastPing = DateTime.UtcNow;
        readonly object writeLock = new object();

        public string Name => "Интернет (" + broker + ")";
        public bool Ready => state == 1;
        public bool Failed => state == 2;

        public MqttLink(string broker, string subTopic, string pubTopic)
        {
            this.broker = broker;
            this.subTopic = subTopic;
            this.pubTopic = pubTopic;
            var t = new Thread(Connect) { IsBackground = true, Name = "mqtt-" + broker };
            t.Start();
        }

        void Connect()
        {
            try
            {
                tcp = new TcpClient();
                var ar = tcp.BeginConnect(broker, 1883, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(6000)) throw new TimeoutException();
                tcp.EndConnect(ar);
                tcp.NoDelay = true;
                stream = tcp.GetStream();
                stream.ReadTimeout = 6000;

                var clientId = "na" + Guid.NewGuid().ToString("N").Substring(0, 12);
                var vh = new List<byte> { 0, 4, (byte)'M', (byte)'Q', (byte)'T', (byte)'T', 4, 0x02, 0, 30 }; // clean session, keepalive 30 s
                vh.AddRange(Str(clientId));
                Write(Packet(0x10, vh));

                var ack = new byte[4];
                int got = 0;
                while (got < 4) { int n = stream.Read(ack, got, 4 - got); if (n <= 0) throw new Exception("closed"); got += n; }
                if ((ack[0] >> 4) != 2 || ack[3] != 0) throw new Exception("refused");

                var sub = new List<byte> { 0, 1 };
                sub.AddRange(Str(subTopic));
                sub.Add(0);
                Write(Packet(0x82, sub));
                stream.ReadTimeout = Timeout.Infinite;
                state = 1;
            }
            catch
            {
                state = 2;
                try { tcp?.Close(); } catch { }
            }
        }

        /// <summary>The right MQTT link for this platform: raw TCP on phones, WebSocket in a browser.</summary>
        public static ILink Create(string broker, string subTopic, string pubTopic)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new WsMqttLink(broker, subTopic, pubTopic);
#else
            return new MqttLink(broker, subTopic, pubTopic);
#endif
        }

        internal static byte[] Str(string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            var r = new byte[b.Length + 2];
            r[0] = (byte)(b.Length >> 8); r[1] = (byte)(b.Length & 0xFF);
            Buffer.BlockCopy(b, 0, r, 2, b.Length);
            return r;
        }

        internal static byte[] Packet(byte header, List<byte> body)
        {
            var p = new List<byte> { header };
            int x = body.Count;
            do { byte d = (byte)(x % 128); x /= 128; if (x > 0) d |= 0x80; p.Add(d); } while (x > 0);
            p.AddRange(body);
            return p.ToArray();
        }

        void Write(byte[] data)
        {
            lock (writeLock) stream.Write(data, 0, data.Length);
        }

        public void Send(string msg)
        {
            if (state != 1) return;
            try
            {
                var body = new List<byte>(Str(pubTopic));
                body.AddRange(Encoding.UTF8.GetBytes(msg));
                Write(Packet(0x30, body));
            }
            catch { state = 2; }
        }

        public void Poll(List<NetIncoming> into)
        {
            if (state != 1) return;
            try
            {
                if ((DateTime.UtcNow - lastPing).TotalSeconds > 20)
                {
                    lastPing = DateTime.UtcNow;
                    Write(new byte[] { 0xC0, 0x00 });
                }
                while (stream.DataAvailable && len < buf.Length)
                {
                    int n = stream.Read(buf, len, buf.Length - len);
                    if (n <= 0) { state = 2; return; }
                    len += n;
                }
                len = Parse(buf, len, this, into, out _);
            }
            catch { state = 2; }
        }

        /// <summary>
        /// Takes every complete MQTT packet off the front of the buffer: PUBLISH payloads go to
        /// <paramref name="into"/>, a CONNACK sets <paramref name="connAck"/> (0 = accepted, -1 = none seen).
        /// Returns the bytes left over (an incomplete packet), moved to the start of the buffer.
        /// </summary>
        internal static int Parse(byte[] buf, int len, ILink link, List<NetIncoming> into, out int connAck)
        {
            connAck = -1;
            int p = 0;
            while (true)
            {
                if (len - p < 2) break;
                byte h = buf[p];
                int mul = 1, rem = 0, i = p + 1;
                bool complete = false;
                while (i < len)
                {
                    byte d = buf[i++];
                    rem += (d & 0x7F) * mul;
                    mul *= 128;
                    if ((d & 0x80) == 0) { complete = true; break; }
                    if (mul > 128 * 128 * 128) throw new Exception("bad length");
                }
                if (!complete || len - i < rem) break;
                if ((h >> 4) == 3)
                {
                    int tl = (buf[i] << 8) | buf[i + 1];
                    int payload = i + 2 + tl;
                    if (((h >> 1) & 3) > 0) payload += 2;
                    int plen = i + rem - payload;
                    if (plen > 0) into.Add(new NetIncoming { text = Encoding.UTF8.GetString(buf, payload, plen), link = link });
                }
                else if ((h >> 4) == 2 && rem >= 2) connAck = buf[i + 1];
                p = i + rem;
            }
            if (p > 0)
            {
                Buffer.BlockCopy(buf, p, buf, 0, len - p);
                len -= p;
            }
            return len;
        }

        public void Close()
        {
            try { if (state == 1) Write(new byte[] { 0xE0, 0x00 }); } catch { }
            state = 2;
            try { tcp?.Close(); } catch { }
        }
    }
}
