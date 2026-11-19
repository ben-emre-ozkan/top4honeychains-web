/* Delivery with sealed containers (D-78 P2): add and remove container rows (names Containers[i].Field are renumbered),
   show the origin-seal or the plant-seal field depending on how the container arrived (two radios: sealed / unsealed), and add up the net weights.
   The server checks everything again; the total it stores is the sum of the net weights. */
(function () {
    'use strict';
    var table = document.getElementById('containers');
    if (!table) return;
    var body = table.tBodies[0];
    var total = document.getElementById('total-kg');

    var number = function (v) { var n = parseFloat(String(v || '').trim().replace(',', '.')); return isFinite(n) ? n : 0; };

    // D-91: "arrived unsealed" is a second radio of ArrivedSealed; it carries data-name, so only "arrived sealed" has data-field
    var name = function (row, i) {
        row.querySelectorAll('[data-field], [data-name]').forEach(function (el) {
            el.name = 'Containers[' + i + '].' + (el.getAttribute('data-field') || el.getAttribute('data-name'));
        });
    };

    var renumber = function () {
        Array.prototype.forEach.call(body.rows, function (row, i) {
            row.querySelector('.row-no').textContent = String(i + 1);
            name(row, i);
            row.querySelectorAll('[id]').forEach(function (el) { el.id = el.id.replace(/-\d+$/, '-' + i); });
            row.querySelectorAll('label[for]').forEach(function (el) { el.htmlFor = el.htmlFor.replace(/-\d+$/, '-' + i); });
        });
        var sum = 0;
        body.querySelectorAll('.net-kg').forEach(function (el) { sum += number(el.value); });
        total.textContent = (Math.round(sum * 100) / 100).toLocaleString(document.documentElement.lang || undefined);
    };

    var toggle = function (row) {
        var sealed = row.querySelector('.arrived-sealed').checked;
        row.querySelector('.origin').classList.toggle('d-none', !sealed);
        row.querySelector('.plant').classList.toggle('d-none', sealed);
    };

    body.addEventListener('change', function (e) {
        if (e.target.classList.contains('arrived-sealed') || e.target.classList.contains('arrived-unsealed')) toggle(e.target.closest('tr'));
    });
    body.addEventListener('input', function (e) { if (e.target.classList.contains('net-kg')) renumber(); });
    body.addEventListener('click', function (e) {
        var btn = e.target.closest('.remove-row');
        if (!btn || body.rows.length <= 1) return;
        btn.closest('tr').remove();
        renumber();
    });

    document.getElementById('add-container').addEventListener('click', function () {
        var last = body.rows[body.rows.length - 1];
        var row = last.cloneNode(true);
        // a radio group of its own before it joins the form (the clone still has the last row's names); the next container
        // usually arrived the same way, so the arrival choice is kept
        name(row, body.rows.length);
        row.querySelectorAll('input').forEach(function (el) {
            if (el.type === 'checkbox') el.checked = el.getAttribute('data-field') === 'OriginSealIntact';
            else if (el.type !== 'radio') el.value = '';
        });
        // the next container is usually the same kind
        row.querySelector('[data-field=Kind]').value = last.querySelector('[data-field=Kind]').value;
        body.appendChild(row);
        toggle(row);
        renumber();
        row.querySelector('.net-kg').focus();
    });
    renumber();
})();
