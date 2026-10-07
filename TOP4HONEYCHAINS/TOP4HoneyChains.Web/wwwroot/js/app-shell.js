/* App shell behaviour (D-66): corner toasts, the My work badge and glow, table helpers, "New" marks on My work.
   Front end only: reads GET /MyWork/Count (counts only) and the page; stores what this browser has seen in
   localStorage, per user and active membership. Works without it (the page stays usable when storage is blocked). */
(function () {
    'use strict';

    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var store = {
        get: function (key) { try { return JSON.parse(window.localStorage.getItem(key) || 'null'); } catch (e) { return null; } },
        set: function (key, value) { try { window.localStorage.setItem(key, JSON.stringify(value)); } catch (e) { /* storage blocked */ } },
        removePrefix: function (prefix) {
            try {
                for (var i = window.localStorage.length - 1; i >= 0; i--) {
                    var k = window.localStorage.key(i);
                    if (k && k.indexOf(prefix) === 0) window.localStorage.removeItem(k);
                }
            } catch (e) { /* storage blocked */ }
        }
    };
    function text(template, n) { return (template || '').replace('{0}', n); }

    // ------------------------------------------------------------------ toasts
    var toastBox = document.getElementById('app-toasts');
    function wireToast(toast) {
        var close = toast.querySelector('.app-toast-close');
        function dismiss() {
            toast.classList.add('is-leaving');
            setTimeout(function () { if (toast.parentNode) toast.parentNode.removeChild(toast); }, reduceMotion ? 0 : 220);
        }
        if (close) close.addEventListener('click', dismiss);
        if (toast.getAttribute('data-persist') === 'true') return;
        // success and info: about 8 s, paused while hovered or focused
        var left = 8000, started = Date.now(), timer = setTimeout(dismiss, left), paused = false;
        function pause() { if (paused) return; paused = true; clearTimeout(timer); left -= Date.now() - started; }
        function resume() {
            if (!paused || toast.matches(':hover') || toast.contains(document.activeElement)) return;
            paused = false; started = Date.now(); timer = setTimeout(dismiss, Math.max(1500, left));
        }
        toast.addEventListener('mouseenter', pause);
        toast.addEventListener('focusin', pause);
        toast.addEventListener('mouseleave', resume);
        toast.addEventListener('focusout', function () { setTimeout(resume, 0); });
    }
    if (toastBox) Array.prototype.forEach.call(toastBox.querySelectorAll('.app-toast'), wireToast);

    window.AppToast = {
        // kind: success | info | error; html is built by this script only (no user input)
        show: function (kind, html, persist) {
            if (!toastBox) return null;
            var t = document.createElement('div');
            t.className = 'app-toast app-toast-' + kind;
            t.setAttribute('role', kind === 'error' ? 'alert' : 'status');
            t.setAttribute('data-persist', persist ? 'true' : 'false');
            var icon = kind === 'error' ? 'mdi-alert-circle-outline' : kind === 'success' ? 'mdi-check-circle-outline' : 'mdi-bell-ring-outline';
            t.innerHTML = '<span class="app-toast-icon" aria-hidden="true"><i class="mdi ' + icon + '"></i></span><div class="app-toast-text">' + html +
                '</div><button type="button" class="app-toast-close" aria-label="' + (toastBox.getAttribute('data-close') || 'Close') + '"><i class="mdi mdi-close" aria-hidden="true"></i></button>';
            toastBox.appendChild(t);
            wireToast(t);
            return t;
        }
    };

    // ------------------------------------------------------------------ My work badge and glow
    var workLinks = document.querySelectorAll('[data-work-link][data-work-scope]');
    var scope = workLinks.length ? workLinks[0].getAttribute('data-work-scope') : null;
    var userPrefix = scope ? scope.split(':')[0] + ':' : null;
    var kindsNode = document.getElementById('work-kinds');
    var kinds = {};
    try { kinds = kindsNode ? JSON.parse(kindsNode.textContent) : {}; } catch (e) { kinds = {}; }
    var live = document.getElementById('work-live');
    var baseTitle = document.title;
    var pulseTimer = null;

    // switching membership: start again from nothing for this user
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (userPrefix && form && /\/OrgWorkspace\/Switch/i.test(form.getAttribute('action') || '')) {
            store.removePrefix('t4h.work.' + userPrefix);
            store.removePrefix('t4h.workseen.' + userPrefix);
        }
    }, true);

    function render(count, rose, newCount) {
        var badges = document.querySelectorAll('[data-work-badge]');
        var glowing = document.querySelectorAll('[data-work-link], [data-work-glow]');
        Array.prototype.forEach.call(badges, function (b) {
            b.textContent = count.total > 99 ? '99+' : String(count.total);
            b.hidden = count.total === 0;
            b.setAttribute('aria-label', text(live && live.getAttribute('data-waiting'), count.total));
        });
        Array.prototype.forEach.call(glowing, function (l) {
            l.classList.toggle('work-steady', count.total > 0);
            if (rose && !reduceMotion) {
                l.classList.remove('work-pulse');
                void l.offsetWidth;   // restart the animation
                l.classList.add('work-pulse');
            }
        });
        if (rose && !reduceMotion) {
            clearTimeout(pulseTimer);
            pulseTimer = setTimeout(function () {
                Array.prototype.forEach.call(glowing, function (l) { l.classList.remove('work-pulse'); });
            }, 1300);   // the glow rises once (D-70)
        }
        if (rose || /^\(\d+\+?\) /.test(document.title)) document.title = count.total > 0 ? '(' + count.total + ') ' + baseTitle : baseTitle;
        if (rose && live) live.textContent = text(newCount === 1 ? live.getAttribute('data-one') : live.getAttribute('data-many'), newCount);
    }

    function grownGroups(count, seen) {
        var grown = [];
        Object.keys(count.byKind || {}).forEach(function (k) {
            var before = seen && seen.byKind && seen.byKind[k] || 0;
            if (count.byKind[k] > before) grown.push({ kind: k, n: count.byKind[k] - before });
        });
        return grown;
    }

    function escapeHtml(s) { return String(s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }

    function onCount(count) {
        var key = 't4h.work.' + scope;
        var seen = store.get(key);
        var before = seen ? seen.total : 0;
        var rose = count.total > before;
        var grown = rose ? grownGroups(count, seen) : [];
        render(count, rose, count.total - before);
        if (rose && grown.length && live) {
            var links = grown.map(function (g) {
                return '<a href="/MyWork/Index#group-' + encodeURIComponent(g.kind) + '">' + escapeHtml(kinds[g.kind] || g.kind) + ' (+' + g.n + ')</a>';
            }).join(', ');
            window.AppToast.show('info', '<strong>' + escapeHtml(text(count.total - before === 1 ? live.getAttribute('data-one') : live.getAttribute('data-many'), count.total - before)) + '</strong><br>' + links, true);
        }
        store.set(key, count);
    }

    var timer = null, inFlight = false;
    function fetchCount() {
        if (inFlight || document.hidden) return;
        inFlight = true;
        fetch('/MyWork/Count', { credentials: 'same-origin', cache: 'no-store', headers: { 'Accept': 'application/json' } })
            .then(function (r) { return r.ok && /json/.test(r.headers.get('Content-Type') || '') ? r.json() : null; })
            .then(function (c) { if (c && typeof c.total === 'number') onCount(c); })
            .catch(function () { /* offline: try again later */ })
            .then(function () { inFlight = false; });
    }
    function schedule() {
        clearInterval(timer);
        timer = null;
        if (!document.hidden) timer = setInterval(fetchCount, 60000);
    }
    if (scope) {
        fetchCount();
        schedule();
        document.addEventListener('visibilitychange', function () {
            if (!document.hidden) fetchCount();
            schedule();
        });
    }

    // ------------------------------------------------------------------ My work page: items new since the last visit
    var workPage = document.getElementById('work-page');
    if (workPage && scope) {
        var seenKey = 't4h.workseen.' + scope;
        var items = workPage.querySelectorAll('.work-item[data-key]');
        var keys = Array.prototype.map.call(items, function (i) { return i.getAttribute('data-key'); });
        var previous = store.get(seenKey);
        if (previous && previous.length !== undefined) {
            var known = {};
            previous.forEach(function (k) { known[k] = true; });
            Array.prototype.forEach.call(items, function (i) {
                if (!known[i.getAttribute('data-key')]) {
                    i.classList.add('is-new');
                    var chip = i.querySelector('.work-new');
                    if (chip) chip.hidden = false;
                }
            });
        }
        store.set(seenKey, keys);
    }

    // ------------------------------------------------------------------ file pickers and copy buttons (D-67)
    Array.prototype.forEach.call(document.querySelectorAll('.app-file-input'), function (input) {
        var name = input.parentElement.querySelector('.app-file-name');
        if (!name) return;
        input.addEventListener('change', function () {
            name.textContent = input.files && input.files.length ? input.files[0].name : name.getAttribute('data-empty');
            name.classList.toggle('has-file', !!(input.files && input.files.length));
        });
    });
    Array.prototype.forEach.call(document.querySelectorAll('.app-copy[data-copy]'), function (button) {
        var label = button.querySelector('span');
        var original = label ? label.textContent : '';
        button.addEventListener('click', function () {
            var text = button.getAttribute('data-copy');
            var done = function () {
                if (label) label.textContent = button.getAttribute('data-copied') || original;
                button.classList.add('is-copied');
                setTimeout(function () { if (label) label.textContent = original; button.classList.remove('is-copied'); }, 2000);
            };
            if (navigator.clipboard && navigator.clipboard.writeText) navigator.clipboard.writeText(text).then(done, function () { });
            else {
                var area = document.createElement('textarea');
                area.value = text; document.body.appendChild(area); area.select();
                try { document.execCommand('copy'); done(); } catch (e) { /* not available */ }
                document.body.removeChild(area);
            }
        });
    });

    // ------------------------------------------------------------------ sticky offsets
    var topbar = document.getElementById('page-topbar');
    function topOffset() { if (topbar) document.documentElement.style.setProperty('--app-top-h', topbar.offsetHeight + 'px'); }
    topOffset();
    window.addEventListener('resize', topOffset, { passive: true });
    if (topbar && 'ResizeObserver' in window) new ResizeObserver(topOffset).observe(topbar);

    // ------------------------------------------------------------------ tables
    var numberCell = /^[-+]?(\d{1,3}([.,\s ]\d{3})*|\d+)([.,]\d+)?\s*(%|kg|g|°C|ppb|ppm|mg\/kg)?$/;
    Array.prototype.forEach.call(document.querySelectorAll('.page-content table.table'), function (table) {
        if (/^datatable/.test(table.id) || table.closest('.dataTables_wrapper')) return;   // DataTables manages these
        // numbers right-aligned: a column whose filled body cells are all numbers
        var rows = table.tBodies.length ? table.tBodies[0].rows : [];
        var head = table.tHead && table.tHead.rows.length ? table.tHead.rows[0].cells : [];
        var columns = rows.length ? rows[0].cells.length : 0;
        for (var c = 0; c < columns; c++) {
            var filled = 0, numeric = 0;
            for (var r = 0; r < rows.length; r++) {
                var cell = rows[r].cells[c];
                if (!cell || cell.colSpan > 1 || cell.querySelector('input, select, textarea, button, a.btn, .btn')) { filled = -1; break; }
                var t = cell.textContent.trim();
                if (!t) continue;
                filled++;
                if (numberCell.test(t)) numeric++;
            }
            if (filled > 0 && numeric === filled) {
                for (var r2 = 0; r2 < rows.length; r2++) if (rows[r2].cells[c]) rows[r2].cells[c].classList.add('num');
                if (head[c]) head[c].classList.add('num');
            }
        }
        // wide tables scroll inside their card, not the page (on desktop the header row stays sticky instead)
        var parent = table.parentElement;
        var narrow = window.innerWidth < 992 || table.scrollWidth > (parent ? parent.clientWidth : 0) + 1;
        // an existing scroll wrapper whose table fits on desktop lets the header row stick under the top bar
        if (parent && parent.classList.contains('table-responsive') && !narrow) parent.classList.add('app-table-fits');
        if (parent && narrow && !parent.classList.contains('table-responsive')) {
            var wrap = document.createElement('div');
            wrap.className = 'table-responsive app-table-scroll';
            parent.insertBefore(wrap, table);
            wrap.appendChild(table);
        }
    });
})();
