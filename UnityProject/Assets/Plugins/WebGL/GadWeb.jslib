// Browser glue for the WebGL build: WebSocket transport for the MQTT relay (Net/WsMqttLink.cs),
// flushing save files to IndexedDB, and reloading the page.
mergeInto(LibraryManager.library, {
  GadWsOpen: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    if (!window.gadWs) window.gadWs = { next: 1, socks: {} };
    var id = window.gadWs.next++;
    var s = { ws: null, state: 0, inbox: [] };
    window.gadWs.socks[id] = s;
    try {
      s.ws = new WebSocket(url, ['mqtt']);
      s.ws.binaryType = 'arraybuffer';
      s.ws.onopen = function () { s.state = 1; };
      s.ws.onmessage = function (e) { if (e.data instanceof ArrayBuffer) s.inbox.push(new Uint8Array(e.data)); };
      s.ws.onerror = function () { s.state = 2; };
      s.ws.onclose = function () { s.state = 2; };
    } catch (e) { s.state = 2; }
    return id;
  },
  GadWsState: function (id) {
    var s = window.gadWs && window.gadWs.socks[id];
    return s ? s.state : 2;
  },
  GadWsSend: function (id, ptr, length) {
    var s = window.gadWs && window.gadWs.socks[id];
    if (!s || s.state !== 1) return 0;
    try { s.ws.send(HEAPU8.slice(ptr, ptr + length)); return 1; } catch (e) { s.state = 2; return 0; }
  },
  // Copies queued incoming bytes into the C# buffer (starting at offset); returns how many.
  GadWsRecv: function (id, ptr, offset, max) {
    var s = window.gadWs && window.gadWs.socks[id];
    if (!s) return 0;
    var n = 0;
    while (s.inbox.length > 0 && n < max) {
      var m = s.inbox[0];
      var take = Math.min(m.length, max - n);
      HEAPU8.set(m.subarray(0, take), ptr + offset + n);
      n += take;
      if (take < m.length) { s.inbox[0] = m.subarray(take); break; }
      s.inbox.shift();
    }
    return n;
  },
  GadWsClose: function (id) {
    var s = window.gadWs && window.gadWs.socks[id];
    if (!s) return;
    try { if (s.ws) s.ws.close(); } catch (e) {}
    delete window.gadWs.socks[id];
  },
  GadSyncFs: function () {
    try { FS.syncfs(false, function (err) { if (err) console.warn('save sync failed', err); }); } catch (e) {}
  },
  GadReload: function () {
    window.location.reload();
  },
});
