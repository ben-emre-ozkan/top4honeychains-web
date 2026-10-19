/* Apiary location picker (D-78), on the administrators' apiary create and edit pages only.
   Click the map or drag the pin to fill latitude and longitude; typing coordinates moves the pin; "Use my location"
   takes the device position. The coordinates are only stored; public pages show the province, never the point. */
(function () {
    'use strict';
    var box = document.getElementById('apiary-map');
    if (!box || typeof L === 'undefined') return;
    var lat = document.getElementById(box.getAttribute('data-lat'));
    var lng = document.getElementById(box.getAttribute('data-lng'));
    var status = document.getElementById('apiary-map-status');
    if (!lat || !lng) return;

    var parse = function (v) { var n = parseFloat(String(v || '').trim().replace(',', '.')); return isFinite(n) ? n : null; };
    var valid = function (a, b) { return a !== null && b !== null && a >= -90 && a <= 90 && b >= -180 && b <= 180 && !(a === 0 && b === 0); };
    var round = function (n) { return Math.round(n * 1e6) / 1e6; };

    var start = { lat: parse(lat.value), lng: parse(lng.value) };
    var has = valid(start.lat, start.lng);
    var map = L.map(box, { scrollWheelZoom: false }).setView(has ? [start.lat, start.lng] : [39.0, 35.0], has ? 14 : 6);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a>'
    }).addTo(map);
    map.on('focus', function () { map.scrollWheelZoom.enable(); });
    map.on('blur', function () { map.scrollWheelZoom.disable(); });

    var marker = null;
    var say = function (text) { if (status) status.textContent = text || ''; };
    var place = function (a, b, fromInputs) {
        if (!marker) {
            marker = L.marker([a, b], { draggable: true, keyboard: true, title: box.getAttribute('data-pin') || '' }).addTo(map);
            marker.on('dragend', function () { var p = marker.getLatLng(); write(p.lat, p.lng); });
        } else {
            marker.setLatLng([a, b]);
        }
        if (!fromInputs) map.panTo([a, b]);
    };
    var write = function (a, b) {
        lat.value = String(round(a));
        lng.value = String(round(b));
        lat.dispatchEvent(new Event('change', { bubbles: true }));
        lng.dispatchEvent(new Event('change', { bubbles: true }));
        say((box.getAttribute('data-set') || '') + ' ' + round(a) + ', ' + round(b));
    };

    if (has) place(start.lat, start.lng, true);
    map.on('click', function (e) { place(e.latlng.lat, e.latlng.lng, false); write(e.latlng.lat, e.latlng.lng); });

    var fromInputs = function () {
        var a = parse(lat.value), b = parse(lng.value);
        if (valid(a, b)) { place(a, b, true); map.setView([a, b], Math.max(map.getZoom(), 12)); }
    };
    lat.addEventListener('input', fromInputs);
    lng.addEventListener('input', fromInputs);

    var locate = document.getElementById('apiary-map-locate');
    if (locate) {
        if (!('geolocation' in navigator)) { locate.hidden = true; }
        locate.addEventListener('click', function () {
            say(box.getAttribute('data-locating'));
            navigator.geolocation.getCurrentPosition(function (pos) {
                var a = pos.coords.latitude, b = pos.coords.longitude;
                place(a, b, false); map.setView([a, b], 15); write(a, b);
            }, function () { say(box.getAttribute('data-nolocation')); }, { enableHighAccuracy: true, timeout: 15000 });
        });
    }
    // the map was drawn while its card may have been hidden or resized
    setTimeout(function () { map.invalidateSize(); }, 200);
})();
