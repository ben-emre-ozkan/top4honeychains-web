/* D-97: the delivery step "Containers and samples". One card per container (Views/Intake/_ContainerCard.cshtml) in three steps;
   new cards come from <template id="ci-card-template">, names Containers[i].Field are renumbered on add and remove. Fields that
   do not apply to the chosen option are hidden and disabled (so they are not sent). Every seal code is checked against the
   organization's seal pool while it is typed or scanned (a USB scanner types the code and Enter); the server checks it all again.
   The list of what the beekeeper confirms is built from the fields. Motion only when animations are on and not reduced. */
(function () {
    'use strict';
    var cards = document.getElementById('ci-cards');
    if (!cards) return;
    var template = document.getElementById('ci-card-template');
    var total = document.getElementById('total-kg');
    var form = cards.closest('form');
    var lang = document.documentElement.lang || undefined;
    var motion = document.documentElement.getAttribute('data-scenes') !== 'off'
        && !(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);
    var uid = 0;

    var number = function (v) { var n = parseFloat(String(v || '').trim().replace(',', '.')); return isFinite(n) ? n : 0; };
    var all = function () { return Array.prototype.slice.call(cards.querySelectorAll('[data-card]')); };
    var capType = function () { var c = form.querySelector('input[name=CapType]:checked'); return c ? c.value : 'SafetyButton'; };
    var closureOf = function (card) { var c = card.querySelector('[data-field=Closure]:checked'); return c ? c.value : 'SealLabel'; };
    var sealedOf = function (card) { var c = card.querySelector('[data-field=ArrivedSealed]:checked'); return !!c && c.value === 'true'; };

    // play a choice's illustration once (the drawing is its final frame; .is-playing runs the timeline)
    var play = function (el) {
        if (!motion || !el) return;
        el.classList.remove('is-playing');
        void el.offsetWidth;
        el.classList.add('is-playing');
        window.clearTimeout(el._t);
        el._t = window.setTimeout(function () { el.classList.remove('is-playing'); }, (parseInt(el.getAttribute('data-duration'), 10) || 1200) + 60);
    };

    var show = function (el, on) {
        var was = !el.hidden;
        el.hidden = !on;
        el.querySelectorAll('input, select, textarea').forEach(function (f) { f.disabled = !on; });
        if (on && !was && motion) { el.classList.remove('ci-show'); void el.offsetWidth; el.classList.add('ci-show'); }
    };

    var renumber = function () {
        all().forEach(function (card, i) {
            card.querySelector('.ci-card-no').textContent = String(i + 1);
            card.querySelectorAll('[data-field]').forEach(function (el) { el.name = 'Containers[' + i + '].' + el.getAttribute('data-field'); });
            card.querySelectorAll('[id^="c"]').forEach(function (el) { el.id = el.id.replace(/^c(\d+|__i__)-/, 'c' + i + '-'); });
            card.querySelectorAll('label[for^="c"]').forEach(function (el) { el.htmlFor = el.htmlFor.replace(/^c(\d+|__i__)-/, 'c' + i + '-'); });
        });
        var sum = 0;
        cards.querySelectorAll('.net-kg').forEach(function (el) { sum += number(el.value); });
        total.textContent = (Math.round(sum * 100) / 100).toLocaleString(lang);
        summary();
    };

    // ---------------------------------------------------------------- the card's options
    var apply = function (card) {
        var closure = closureOf(card);
        var box = closure === 'OrgContainer';
        card.querySelectorAll('[data-when=box]').forEach(function (el) { show(el, box); });
        card.querySelectorAll('[data-when=own]').forEach(function (el) { show(el, !box); });
        card.querySelectorAll('[data-when=sealed]').forEach(function (el) { show(el, sealedOf(card)); });
        card.querySelectorAll('[data-closing-text]').forEach(function (el) { el.hidden = el.getAttribute('data-closing-text') !== closure; });
        card.querySelectorAll('[data-cap-text]').forEach(function (el) { el.hidden = el.getAttribute('data-cap-text') !== capType(); });
        progress(card);
    };

    var filled = function (el) { return !el || el.disabled || (el.value.trim() !== '' && !el.classList.contains('is-bad')); };
    var progress = function (card) {
        var box = closureOf(card) === 'OrgContainer';
        var s1 = number(card.querySelector('[data-field=NetKg]').value) > 0 && (!box || filled(card.querySelector('.ci-box')) && card.querySelector('.ci-box').value.trim() !== '')
            && (!sealedOf(card) || filled(card.querySelector('[data-use=origin]')));
        var s2 = filled(card.querySelector('[data-field=JarSeal]')) && filled(card.querySelector('[data-field=CounterSeal]')) && card.querySelector('[data-field=CapIntact]').checked;
        var s3 = filled(card.querySelector('[data-field=ClosingSeal]'));
        [s1, s2, s3].forEach(function (done, k) { card.querySelector('[data-step="' + (k + 1) + '"]').classList.toggle('is-done', !!done); });
        var sum = card.querySelector('[data-sum]');
        var kg = number(card.querySelector('[data-field=NetKg]').value);
        var label = card.querySelector('[data-field=Closure]:checked');
        sum.textContent = (label ? label.closest('.ci-tile').querySelector('.ci-tile-title').textContent : '') + (kg > 0 ? ' · ' + kg.toLocaleString(lang) + ' kg' : '');
    };

    // ---------------------------------------------------------------- live checks against the seal pool
    var state = function (input) { return input.closest('.ci-code').parentNode.querySelector('[data-state]'); };
    var mark = function (input, ok, text) {
        input.classList.toggle('is-ok', ok === true);
        input.classList.toggle('is-bad', ok === false);
        var s = state(input);
        if (s) { s.textContent = text || ''; s.classList.toggle('is-ok', ok === true); s.classList.toggle('is-bad', ok === false); }
    };
    var check = function (input) {
        var card = input.closest('[data-card]');
        var code = input.value.trim();
        if (!code) { mark(input, null, ''); progress(card); summary(); return; }
        var box = input.classList.contains('ci-box');
        var url = box
            ? cards.getAttribute('data-box-url') + '?code=' + encodeURIComponent(code) + '&number=' + encodeURIComponent(cards.getAttribute('data-number') || '')
            : cards.getAttribute('data-check-url') + '?code=' + encodeURIComponent(code) + '&use=' + encodeURIComponent(input.getAttribute('data-use'))
              + '&closure=' + encodeURIComponent(closureOf(card)) + '&number=' + encodeURIComponent(cards.getAttribute('data-number') || '');
        var asked = code;
        fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (j) {
                if (!j || input.value.trim() !== asked) return;
                if (j.code && j.code !== input.value) input.value = j.code;
                var dup = j.ok && !box && all().some(function (c) {
                    return Array.prototype.some.call(c.querySelectorAll('.ci-seal'), function (o) { return o !== input && !o.disabled && o.value.trim().toUpperCase() === j.code; });
                });
                if (box && j.ok) {
                    var tare = card.querySelector('[data-tare]');
                    if (tare) tare.textContent = j.tare ? cards.getAttribute('data-tare').split('{0}').join(j.tare) : '';
                    mark(input, true, cards.getAttribute('data-box-ok').replace('{0}', j.kindText + (j.name ? ' · ' + j.name : '')));
                    var kind = card.querySelector('[data-field=Kind]');
                    if (kind) kind.value = j.kind;
                    play(card.querySelector('.hs-scan'));
                } else if (dup) mark(input, false, form.getAttribute('data-dup') || '');
                else mark(input, j.ok, j.ok ? cards.getAttribute('data-ok') : j.message);
                progress(card);
                summary();
            })
            .catch(function () { /* offline: the server checks again on saving */ });
    };
    var timers = new WeakMap();
    cards.addEventListener('input', function (e) {
        var t = e.target;
        if (t.classList.contains('ci-seal') || t.classList.contains('ci-box')) {
            mark(t, null, '');
            window.clearTimeout(timers.get(t));
            timers.set(t, window.setTimeout(function () { check(t); }, 450));
        }
        if (t.classList.contains('net-kg')) renumber();
        var card = t.closest('[data-card]');
        if (card) { progress(card); summary(); }
    });
    // a USB scanner types the code and presses Enter: check it and go to the next code field instead of submitting the form
    cards.addEventListener('keydown', function (e) {
        var t = e.target;
        if (e.key !== 'Enter' || !(t.classList.contains('ci-seal') || t.classList.contains('ci-box'))) return;
        e.preventDefault();
        window.clearTimeout(timers.get(t));
        check(t);
        var fields = Array.prototype.slice.call(form.querySelectorAll('.ci-seal:not([disabled]), .ci-box:not([disabled]), .net-kg'));
        var next = fields[fields.indexOf(t) + 1];
        if (next) next.focus();
    });
    cards.addEventListener('change', function (e) {
        var t = e.target;
        var card = t.closest('[data-card]');
        if (!card) return;
        if (t.getAttribute('data-field') === 'Closure') {
            play(t.closest('.ci-tile').querySelector('.ci-ill'));
            var closing = card.querySelector('[data-field=ClosingSeal]');
            if (closing.value.trim()) check(closing);
        }
        apply(card);
        summary();
    });
    cards.addEventListener('click', function (e) {
        var btn = e.target.closest('.ci-remove');
        if (!btn || all().length <= 1) return;
        var card = btn.closest('[data-card]');
        var done = function () { card.remove(); renumber(); };
        if (motion) { card.classList.add('ci-leave'); window.setTimeout(done, 240); } else done();
    });

    // the jar cap, once per delivery: its illustration, its info card and the cap question of every card
    form.querySelectorAll('input[name=CapType]').forEach(function (r) {
        r.addEventListener('change', function () {
            play(r.closest('.ci-tile').querySelector('.ci-ill'));
            form.querySelectorAll('[data-cap-info]').forEach(function (el) {
                var on = el.getAttribute('data-cap-info') === capType();
                el.hidden = !on;
                if (on && motion) { el.classList.remove('ci-show'); void el.offsetWidth; el.classList.add('ci-show'); }
            });
            all().forEach(apply);
            summary();
        });
    });

    document.getElementById('add-container').addEventListener('click', function () {
        var last = all()[all().length - 1];
        var html = template.innerHTML.replace(/__i__/g, String(all().length));
        var wrap = document.createElement('div');
        wrap.innerHTML = html.trim();
        var card = wrap.firstElementChild;
        // info tips of the copy get their own ids
        card.querySelectorAll('[data-info-tip]').forEach(function (b) {
            var note = card.querySelector('#' + b.getAttribute('aria-controls'));
            var id = 'itc' + (++uid);
            if (note) note.id = id;
            b.setAttribute('aria-controls', id);
        });
        // the next container is usually closed the same way and of the same kind
        if (last) {
            var closure = closureOf(last);
            card.querySelectorAll('[data-field=Closure]').forEach(function (r) { r.checked = r.value === closure; });
            var kind = last.querySelector('[data-field=Kind]');
            if (kind && closure !== 'OrgContainer') card.querySelector('[data-field=Kind]').value = kind.value;
        }
        cards.appendChild(card);
        if (motion) card.classList.add('ci-enter');
        renumber();
        apply(card);
        (card.querySelector('.ci-box:not([disabled])') || card.querySelector('.net-kg')).focus();
    });

    // ---------------------------------------------------------------- what the beekeeper confirms
    var list = document.getElementById('ci-confirm-list');
    var esc = function (s) { var d = document.createElement('span'); d.textContent = s; return d.innerHTML; };
    var summary = function () {
        if (!list) return;
        var capTile = form.querySelector('input[name=CapType]:checked');
        var capText = capTile ? capTile.closest('.ci-tile').querySelector('.ci-tile-title').textContent : '';
        var html = '<div>' + esc(list.getAttribute('data-cap-label')) + ': <strong>' + esc(capText) + '</strong></div><ol>';
        all().forEach(function (card) {
            var v = function (f) { var el = card.querySelector('[data-field=' + f + ']'); return el && !el.disabled ? el.value.trim().toUpperCase() : ''; };
            var parts = [];
            var tile = card.querySelector('[data-field=Closure]:checked');
            parts.push(esc(tile ? tile.closest('.ci-tile').querySelector('.ci-tile-title').textContent : ''));
            if (v('OrgContainerCode')) parts.push('<code>' + esc(v('OrgContainerCode')) + '</code>');
            var kg = number(card.querySelector('[data-field=NetKg]').value);
            if (kg > 0) parts.push(esc(kg.toLocaleString(lang) + ' kg'));
            if (sealedOf(card) && v('OriginSeal')) parts.push(esc(list.getAttribute('data-origin')) + ' <code>' + esc(v('OriginSeal')) + '</code>');
            if (v('JarSeal')) parts.push(esc(list.getAttribute('data-jar')) + ' <code>' + esc(v('JarSeal')) + '</code>');
            if (v('CounterSeal')) parts.push(esc(list.getAttribute('data-counter')) + ' <code>' + esc(v('CounterSeal')) + '</code>');
            if (v('ClosingSeal')) parts.push(esc(list.getAttribute('data-closing')) + ' <code>' + esc(v('ClosingSeal')) + '</code>');
            html += '<li>' + parts.join(' · ') + '</li>';
        });
        list.innerHTML = html + '</ol>';
    };

    all().forEach(apply);
    renumber();
    // codes already typed (the form came back with an error or a sent code): show their state
    cards.querySelectorAll('.ci-seal:not([disabled]), .ci-box:not([disabled])').forEach(function (el) { if (el.value.trim()) check(el); });
})();
