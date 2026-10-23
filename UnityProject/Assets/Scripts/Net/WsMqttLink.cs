using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Arena
{
    /// <summary>
    /// The internet relay for the browser version: the same MQTT 3.1.1 protocol and the same public
    /// brokers and topics as <see cref="MqttLink"/>, but over a secure WebSocket (a web page can't
    /// open raw TCP). Phone and browser players therefore meet in the same rooms. Everything runs
    /// on the main thread: the socket lives in JavaScript (HayWeb.jslib) and is polled every frame.
    /// </summary>
    public class WsMqttLink : ILink
    {
        // Secure WebSocket endpoints of the public brokers (the page itself is served over https).
        static string Url(string broker) => broker switch
        {
            "broker.hivemq.com" => "wss://broker.hivemq.com:8884/mqtt",
            _ => "wss://" + broker + ":8084/mqtt",
        };

        readonly string broker, subTopic, pubTopic;
        readonly int sock;
        int state;              // 0 connecting, 1 ready, 2 failed
        bool sentConnect;
        readonly byte[] buf = new byte[65536];
        int len;
        readonly float startedAt;
        float lastPing;

        public string Name => "Интернет (" + broker + ")";
        public bool Ready => state == 1;
        public bool Failed => state == 2;

        public WsMqttLink(string broker, string subTopic, string pubTopic)
        {
            this.broker = broker;
            this.subTopic = subTopic;
            this.pubTopic = pubTopic;
            startedAt = lastPing = Time.unscaledTime;
            sock = WebBridge.WsOpen(Url(broker));
            if (sock <= 0) state = 2;
        }

        void Write(byte[] data)
        {
            if (!WebBridge.WsSend(sock, data)) state = 2;
        }

        public void Send(string msg)
        {
            if (state != 1) return;
            var body = new List<byte>(MqttLink.Str(pubTopic));
            body.AddRange(Encoding.UTF8.GetBytes(msg));
            Write(MqttLink.Packet(0x30, body));
        }

        public void Poll(List<NetIncoming> into)
        {
            if (state == 2) return;
            int ws = WebBridge.WsState(sock);
            if (ws == 2) { state = 2; return; }
            if (state == 0 && Time.unscaledTime - startedAt > 10f) { Close(); return; }
            if (ws != 1) return;
            try
            {
                if (!sentConnect)
                {
                    sentConnect = true;
                    var clientId = "na" + Guid.NewGuid().ToString("N").Substring(0, 12);
                    var vh = new List<byte> { 0, 4, (byte)'M', (byte)'Q', (byte)'T', (byte)'T', 4, 0x02, 0, 30 }; // clean session, keepalive 30 s
                    vh.AddRange(MqttLink.Str(clientId));
                    Write(MqttLink.Packet(0x10, vh));
                }
                if (state == 1 && Time.unscaledTime - lastPing > 20f)
                {
                    lastPing = Time.unscaledTime;
                    Write(new byte[] { 0xC0, 0x00 });
                }
                len += WebBridge.WsRecv(sock, buf, len);
                len = MqttLink.Parse(buf, len, this, into, out int connAck);
                if (connAck > 0) { Close(); return; }
                if (connAck == 0 && state == 0)
                {
                    var sub = new List<byte> { 0, 1 };
                    sub.AddRange(MqttLink.Str(subTopic));
                    sub.Add(0);
                    Write(MqttLink.Packet(0x82, sub));
                    state = 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Net] websocket relay failed: " + e.Message);
                Close();
            }
        }

        public void Close()
        {
            if (state == 1) WebBridge.WsSend(sock, new byte[] { 0xE0, 0x00 });
            state = 2;
            WebBridge.WsClose(sock);
        }
    }

    /// <summary>Calls into the page's JavaScript (Assets/Plugins/WebGL/GadWeb.jslib); no-ops elsewhere.</summary>
    public static class WebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern int GadWsOpen(string url);
        [DllImport("__Internal")] static extern int GadWsState(int id);
        [DllImport("__Internal")] static extern int GadWsSend(int id, byte[] data, int length);
        [DllImport("__Internal")] static extern int GadWsRecv(int id, byte[] into, int offset, int max);
        [DllImport("__Internal")] static extern void GadWsClose(int id);
        [DllImport("__Internal")] static extern void GadSyncFs();

        public static int WsOpen(string url) => GadWsOpen(url);
        public static int WsState(int id) => GadWsState(id);
        public static bool WsSend(int id, byte[] data) => GadWsSend(id, data, data.Length) == 1;
        public static int WsRecv(int id, byte[] into, int offset) => Math.Max(0, GadWsRecv(id, into, offset, into.Length - offset));
        public static void WsClose(int id) => GadWsClose(id);
        /// <summary>Flush the save files to the browser's storage (IndexedDB) right away.</summary>
        public static void SyncFiles() => GadSyncFs();
#else
        public static int WsOpen(string url) => 0;
        public static int WsState(int id) => 2;
        public static bool WsSend(int id, byte[] data) => false;
        public static int WsRecv(int id, byte[] into, int offset) => 0;
        public static void WsClose(int id) { }
        public static void SyncFiles() { }
#endif
        public static bool IsWeb => Application.platform == RuntimePlatform.WebGLPlayer;
    }
}
