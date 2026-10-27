/* Apiary place fields (D-78, F5): the districts of the chosen province, the province proposed from the map pin (until the
   user picks one), and a preview of the generated title "Province – District – Locality". The server generates the title. */
(function () {
    'use strict';
    document.querySelectorAll('.apiary-place').forEach(function (box) {
        var province = box.querySelector('.ap-province');
        var district = box.querySelector('.ap-district');
        var locality = box.querySelector('.ap-locality');
        var preview = box.querySelector('.ap-preview');
        var lat = document.getElementById(box.getAttribute('data-lat'));
        var lng = document.getElementById(box.getAttribute('data-lng'));
        var chosen = box.getAttribute('data-district');
        var touched = !!province.value;

        var text = function (select) { var o = select.options[select.selectedIndex]; return o && o.value ? o.textContent.trim() : ''; };
        var show = function () {
            var parts = [text(province), text(district), (locality.value || '').trim()];
            preview.textContent = parts.every(function (p) { return p; }) ? parts.join(' – ') : '';
        };

        var load = function () {
            var code = province.value;
            var first = district.options[0];
            district.innerHTML = '';
            district.appendChild(first);
            if (!code) { show(); return Promise.resolve(); }
            return fetch(box.getAttribute('data-districts-url') + '?province=' + encodeURIComponent(code), { credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.json() : []; })
                .then(function (list) {
                    list.forEach(function (d) {
                        var o = document.createElement('option');
                        o.value = d.id;
                        o.textContent = d.name;
                        if (String(d.id) === String(chosen)) o.selected = true;
                        district.appendChild(o);
                    });
                    chosen = null;
                    show();
                });
        };

        var propose = function () {
            if (touched || !lat || !lng) return;
            var a = parseFloat(String(lat.value).replace(',', '.')), b = parseFloat(String(lng.value).replace(',', '.'));
            if (!isFinite(a) || !isFinite(b)) return;
            fetch(box.getAttribute('data-province-url') + '?lat=' + a + '&lng=' + b, { credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.json() : {}; })
                .then(function (res) {
                    if (res.code && !touched && province.value !== res.code) { province.value = res.code; load(); }
                });
        };

        province.addEventListener('change', function () { touched = true; load(); });
        district.addEventListener('change', show);
        locality.addEventListener('input', show);
        if (lat) lat.addEventListener('change', propose);
        if (lng) lng.addEventListener('change', propose);
        load().then(propose);
    });
})();
