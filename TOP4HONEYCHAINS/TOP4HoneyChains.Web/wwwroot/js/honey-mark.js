/* The animated mark and the page loader (D-70, docs/brand/MOTION.md). Styles: wwwroot/css/honey-mark.css.
   - [data-hm-once]: plays once after load (optionally only when its media query matches), then on hover;
   - [data-hm-intro]: the top-bar mark, played once per browser session after sign-in (the sign-in page resets it);
     it also plays once on a success moment (a freshly approved status pill or a freshly sealed batch);
   - loops pause off screen, in a hidden tab and while focus is in a form field;
   - #app-loader: shown when a navigation or form post takes longer than 400 ms, at once for long actions
     (data-loader="upload|report|seal|pdf"), and around the DataTables PDF export. With reduced motion the mark is static. */
(function () {
    'use strict';

    var root = document.documentElement;
    var reduce = window.matchMedia ? window.matchMedia('(prefers-reduced-motion: reduce)') : { matches: false };

    function markOf(el) {
        return el && (el.classList.contains('honey-mark') ? el : el.querySelector('.honey-mark'));
    }
    function play(el, ms) {
        var svg = markOf(el);
        if (!svg || reduce.matches) return;
        svg.classList.add('is-active');
        clearTimeout(svg._hmTimer);
        svg._hmTimer = setTimeout(function () { svg.classList.remove('is-active'); }, ms || 1700);
    }
    window.HoneyMark = { play: play };

    function store(action, key, value) {
        try {
            if (action === 'get') return window.sessionStorage.getItem(key);
            if (action === 'set') window.sessionStorage.setItem(key, value);
            if (action === 'remove') window.sessionStorage.removeItem(key);
        } catch (e) { /* storage blocked: the intro simply plays again */ }
        return null;
    }

    // once on load
    var once = document.querySelectorAll('[data-hm-once]');
    for (var i = 0; i < once.length; i++) {
        var query = once[i].getAttribute('data-hm-once');
        if (query && window.matchMedia && !window.matchMedia(query).matches) continue;
        (function (el) { setTimeout(function () { play(el); }, 300); })(once[i]);
    }

    // top bar: once per session after sign-in, and on success moments
    if (document.body && document.body.hasAttribute('data-hm-intro-reset')) store('remove', 'hm-intro');
    var intro = document.querySelector('[data-hm-intro]');
    if (intro) {
        if (!store('get', 'hm-intro')) {
            store('set', 'hm-intro', '1');
            setTimeout(function () { play(intro); }, 350);
        } else if (document.querySelector('.st-pill.is-fresh, .seal.is-fresh')) {
            setTimeout(function () { play(intro, 1900); }, 600);
        }
    }

    // loops: pause off screen, in a hidden tab, and while someone types
    var loops = document.querySelectorAll('.honey-mark.is-looping');
    if (loops.length && 'IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) { e.target.classList.toggle('hm-paused', !e.isIntersecting); });
        });
        for (var j = 0; j < loops.length; j++) io.observe(loops[j]);
    }
    function onVisibility() { root.classList.toggle('is-hidden-tab', document.hidden); }
    document.addEventListener('visibilitychange', onVisibility);
    onVisibility();
    var fields = 'input:not([type=checkbox]):not([type=radio]):not([type=submit]):not([type=button]):not([type=file]), textarea, select, [contenteditable]';
    document.addEventListener('focusin', function (e) { if (e.target.matches && e.target.matches(fields)) root.classList.add('is-typing'); });
    document.addEventListener('focusout', function (e) { if (e.target.matches && e.target.matches(fields)) root.classList.remove('is-typing'); });

    // ------------------------------------------------------------------ loader
    var loader = document.getElementById('app-loader');
    if (!loader) return;
    var text = loader.querySelector('.hm-loader-text');
    var live = document.getElementById('app-loader-live');
    var timer = null, safety = null;

    function message(kind) {
        return loader.getAttribute('data-msg-' + (kind || 'default')) || loader.getAttribute('data-msg-default') || '';
    }
    function show(kind, now) {
        clearTimeout(timer);
        timer = setTimeout(function () {
            root.classList.remove('is-typing');
            loader.hidden = false;
            document.body.setAttribute('aria-busy', 'true');
            requestAnimationFrame(function () {
                loader.classList.add('is-on');
                text.textContent = message(kind);
                if (live) live.textContent = message(kind);   // the polite live region announces it
            });
            // a navigation that turned out to be a download never unloads the page
            clearTimeout(safety);
            if (!now) safety = setTimeout(hide, 15000);
        }, now ? 0 : 400);
    }
    function hide() {
        clearTimeout(timer);
        clearTimeout(safety);
        loader.classList.remove('is-on');
        loader.hidden = true;
        text.textContent = '';
        if (live) live.textContent = '';
        document.body.removeAttribute('aria-busy');
    }
    window.AppLoader = { show: show, hide: hide };
    window.addEventListener('pageshow', hide);   // back and forward from the page cache

    // links: same-origin page loads only
    document.addEventListener('click', function (e) {
        if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        var a = e.target.closest && e.target.closest('a[href]');
        if (!a || a.hasAttribute('download') || a.closest('[data-no-loader]') || a.matches('[data-bs-toggle], [data-toggle], .dropdown-toggle')) return;
        if (a.target && a.target !== '_self') return;
        var href = a.getAttribute('href');
        if (!href || href.charAt(0) === '#' || /^(javascript|mailto|tel):/i.test(href)) return;
        var url = new URL(a.href, window.location.href);
        if (url.origin !== window.location.origin) return;
        if (url.pathname === window.location.pathname && url.search === window.location.search && url.hash) return;
        // decide after every other handler had its say (confirm dialogs, menus)
        setTimeout(function () { if (!e.defaultPrevented) show(a.getAttribute('data-loader')); }, 0);
    });

    // forms: after client validation; long actions show the loader at once
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form.closest('[data-no-loader]') || (form.target && form.target !== '_self')) return;
        var kind = (e.submitter && e.submitter.getAttribute('data-loader')) || form.getAttribute('data-loader');
        if (kind === 'upload') {
            var file = form.querySelector('input[type=file]');
            if (!file || !file.files || !file.files.length) kind = 'save';
        }
        var long = kind === 'upload' || kind === 'report' || kind === 'seal';
        if (!kind) kind = (form.method || 'get').toLowerCase() === 'post' ? 'save' : 'default';
        setTimeout(function () { if (!e.defaultPrevented) show(kind, long); }, 0);
    });

    // DataTables PDF export runs in the page: show the loader first, then let the export run
    document.addEventListener('click', function (e) {
        var button = e.target.closest && e.target.closest('.buttons-pdf');
        if (!button || button._hmGo) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        show('pdf', true);
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                setTimeout(function () {
                    button._hmGo = true;
                    button.click();
                    button._hmGo = false;
                    setTimeout(hide, 400);
                }, 30);
            });
        });
    }, true);
})();
