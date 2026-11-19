/* D-78 P4 (D78-SPEC 7, MOTION.md "Explanation"): plays the explanatory scenes (<honey-scene>, css/honey-scene.css).
   - A scene is drawn in its final frame; playing it adds .is-playing for its duration (at most 1.5 s), then .is-done.
   - It never blocks: nothing is a modal, focus is never moved, a key press or a pointer press anywhere ends every scene at
     its final frame (typing ends it; focus alone does not, so a field focused by the page keeps the scene).
   - prefers-reduced-motion and the user's "Show animations" switch (html[data-scenes="off"]) keep the static final frame.
   - Triggers: now (once on load), first (first visit per user and scene, then static), session (once per browser session),
     click (the first click on the element in data-for), manual (only the replay button). The replay button always replays. */
(function () {
    'use strict';
    var root = document.documentElement;
    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var off = root.getAttribute('data-scenes') === 'off';
    var user = root.getAttribute('data-user') || 'anonymous';

    function store(kind) { try { return kind === 'session' ? window.sessionStorage : window.localStorage; } catch (e) { return null; } }
    function keyOf(name) { return 'hs-seen:' + user + ':' + name; }
    function seen(name, kind) { var s = store(kind); try { return !!(s && s.getItem(keyOf(name))); } catch (e) { return false; } }
    function remember(name, kind) { var s = store(kind); try { if (s) s.setItem(keyOf(name), '1'); } catch (e) { /* private mode */ } }

    function finish(el) {
        if (!el.classList.contains('is-playing')) return;
        window.clearTimeout(el._hsTimer);
        el.classList.remove('is-playing');
        el.classList.add('is-done');
    }

    function finishAll() {
        var playing = document.querySelectorAll('.hs.is-playing');
        for (var i = 0; i < playing.length; i++) finish(playing[i]);
    }

    function play(el) {
        if (reduce || off) return false;
        finishAll();   // one scene at a time
        el.classList.remove('is-done');
        // restart the CSS timeline on the next frame, without reading layout
        window.requestAnimationFrame(function () {
            el.classList.add('is-playing');
            el._hsTimer = window.setTimeout(function () { finish(el); }, (parseInt(el.getAttribute('data-duration'), 10) || 1500) + 50);
        });
        return true;
    }

    // 7.2: a click or a key press ends a scene at its final frame; the input itself goes on as usual. Once the user has
    // pressed something, scenes waiting to start on their own are dropped.
    var interacted = false;
    document.addEventListener('keydown', function () { interacted = true; finishAll(); }, true);
    document.addEventListener('pointerdown', function (e) {
        if (!(e.target.closest && e.target.closest('[data-scene-replay]'))) { interacted = true; finishAll(); }
    }, true);

    // MOTION.md: one brand moment at a time. A scene that starts on its own waits for the top-bar mark's intro (played once
    // per session after sign-in) and starts after the page has settled.
    function whenCalm(fn) {
        var waited = 0;
        (function check() {
            if (interacted) return;
            var intro = document.querySelector('[data-hm-intro] .is-active, [data-hm-intro].is-active');
            if (intro && waited < 2400) { waited += 150; window.setTimeout(check, 150); return; }
            fn();
        })();
    }
    function auto(el) { window.setTimeout(function () { whenCalm(function () { play(el); }); }, 450); }
    document.addEventListener('visibilitychange', function () { if (document.hidden) finishAll(); });

    function init(el) {
        var name = el.getAttribute('data-scene');
        var trigger = el.getAttribute('data-trigger');
        if (trigger === 'now') {
            auto(el);
        } else if (trigger === 'first' || trigger === 'session') {
            var kind = trigger === 'session' ? 'session' : 'local';
            if (!seen(name, kind)) {
                remember(name, kind);
                auto(el);
            }
        } else if (trigger === 'click') {
            var target = el.getAttribute('data-for') && document.querySelector(el.getAttribute('data-for'));
            if (target) {
                target.addEventListener('click', function () {
                    if (!seen(name, 'local') && play(el)) remember(name, 'local');
                });
            }
        }
    }

    document.addEventListener('click', function (e) {
        var button = e.target.closest && e.target.closest('[data-scene-replay]');
        if (!button) return;
        var el = document.getElementById(button.getAttribute('data-scene-replay'));
        if (el) play(el);
    });

    function start() {
        var scenes = document.querySelectorAll('.hs[data-scene]');
        for (var i = 0; i < scenes.length; i++) init(scenes[i]);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
    else start();

    // the gallery and tests can play a scene on demand
    window.honeyScene = { play: play, finish: finishAll };
})();

/* D-91: info buttons (TagHelpers/InfoTipTagHelper.cs). A disclosure: the button (click, tap, Enter, Space) shows or hides the
   note below it; Escape closes the note and puts the focus back on its button. No hover-only behaviour. */
(function () {
    'use strict';
    function set(button, open) {
        var note = document.getElementById(button.getAttribute('aria-controls'));
        if (!note) return;
        note.hidden = !open;
        button.setAttribute('aria-expanded', open ? 'true' : 'false');
    }
    document.addEventListener('click', function (e) {
        var button = e.target.closest && e.target.closest('[data-info-tip]');
        if (button) set(button, button.getAttribute('aria-expanded') !== 'true');
    });
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        var open = document.querySelectorAll('[data-info-tip][aria-expanded="true"]');
        if (!open.length) return;
        var focused = null;
        open.forEach(function (button) {
            var note = document.getElementById(button.getAttribute('aria-controls'));
            if (document.activeElement === button || (note && note.contains(document.activeElement))) focused = button;
            set(button, false);
        });
        if (focused) focused.focus();
    });
})();
