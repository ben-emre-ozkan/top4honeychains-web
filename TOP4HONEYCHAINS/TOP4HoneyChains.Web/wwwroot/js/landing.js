/* Landing page behaviour (D-65, D-70, D-71); the hero honeycomb is wwwroot/js/hero-comb.js. Rules: docs/brand/MOTION.md.
   - scroll reveals (the chain's photos uncover through a hexagon), the honey line along the steps, the header shadow;
   - the language switcher: a honey thumb that slides to the hovered or focused language and returns to the current
     one; on phones a button opens a popover (Escape and an outside click close it);
   - counters count up once when they come into view;
   - the Honey Story phone tilts towards the pointer (mouse and pen, at most 6 degrees);
   - decorative loops pause off screen and in a hidden tab; the trust section's video plays only while on screen.
   Without JavaScript, or with prefers-reduced-motion, everything is shown at once and nothing moves. */
(function () {
    'use strict';

    var root = document.documentElement;
    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var canObserve = 'IntersectionObserver' in window;

    // ------------------------------------------------------------------ reveals
    var reveals = document.querySelectorAll('.ld-reveal');
    if (reduce || !canObserve) {
        root.classList.add('ld-static');
        for (var i = 0; i < reveals.length; i++) reveals[i].classList.add('is-in');
    } else {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) { e.target.classList.add('is-in'); io.unobserve(e.target); }
            });
        }, { rootMargin: '0px 0px -12% 0px', threshold: 0.12 });
        for (var j = 0; j < reveals.length; j++) io.observe(reveals[j]);
    }

    // ------------------------------------------------------------------ the honey line: drawn up to the middle of the viewport; a node lights once the line reaches it
    var list = document.querySelector('[data-honey-line]');
    var fill = list && list.querySelector('.ld-spine-fill');
    var steps = list ? list.querySelectorAll('.ld-step') : [];
    var ticking = false, lineVisible = true;
    var header = document.querySelector('.ld-top');
    function draw() {
        ticking = false;
        if (!list || !fill) return;
        var rect = list.getBoundingClientRect();
        var progress = reduce ? 1 : Math.max(0, Math.min(1, (window.innerHeight * 0.6 - rect.top) / rect.height));
        fill.style.transform = 'scaleY(' + progress.toFixed(4) + ')';
        var reached = rect.top + progress * rect.height;
        for (var k = 0; k < steps.length; k++) {
            var node = steps[k].querySelector('.ld-node');
            var top = node.getBoundingClientRect().top + node.offsetHeight / 2;
            steps[k].classList.toggle('is-lit', reduce || top <= reached + 1);
        }
    }
    function onScroll() {
        if (!ticking && lineVisible) { ticking = true; requestAnimationFrame(draw); }
        if (header) header.classList.toggle('is-scrolled', window.scrollY > 8);
    }
    if (list && canObserve)
        new IntersectionObserver(function (entries) { lineVisible = entries[0].isIntersecting; if (lineVisible) onScroll(); }, { rootMargin: '25% 0px' }).observe(list);
    window.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('resize', onScroll, { passive: true });

    // ------------------------------------------------------------------ language switcher
    var lang = document.querySelector('[data-lang-switch]');
    if (lang) {
        var thumb = lang.querySelector('.ld-lang-thumb');
        var options = Array.prototype.slice.call(lang.querySelectorAll('.ld-lang-opt'));
        var current = lang.querySelector('.ld-lang-opt[aria-current="true"]') || options[0];
        var place = function (el) {
            if (!el || !thumb) return;
            thumb.style.transform = 'translateX(' + el.offsetLeft + 'px)';
            options.forEach(function (o) { o.classList.toggle('is-under', o === el); });
        };
        options.forEach(function (o) {
            o.addEventListener('mouseenter', function () { place(o); });
            o.addEventListener('focus', function () { place(o); });
        });
        lang.addEventListener('mouseleave', function () { place(current); });
        lang.addEventListener('focusout', function () { setTimeout(function () { if (!lang.contains(document.activeElement)) place(current); }); });
        place(current);
        // the thumb takes over the highlight once it sits under the current language
        requestAnimationFrame(function () { lang.classList.add('is-ready'); });
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(function () { place(current); });
        window.addEventListener('resize', function () { place(current); }, { passive: true });
    }
    var mini = document.querySelector('[data-lang-mini]');
    if (mini) {
        var button = mini.querySelector('.ld-lang-btn'), pop = mini.querySelector('.ld-lang-pop');
        var setOpen = function (open, focusButton) {
            pop.hidden = !open;
            button.setAttribute('aria-expanded', String(open));
            if (open) { var first = pop.querySelector('[aria-current="true"]') || pop.querySelector('a'); if (first) first.focus(); }
            else if (focusButton) button.focus();
        };
        button.addEventListener('click', function () { setOpen(pop.hidden); });
        mini.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !pop.hidden) { e.preventDefault(); setOpen(false, true); } });
        document.addEventListener('click', function (e) { if (!pop.hidden && !mini.contains(e.target)) setOpen(false); });
        mini.addEventListener('focusout', function () { setTimeout(function () { if (!pop.hidden && !mini.contains(document.activeElement)) setOpen(false); }); });
    }

    // ------------------------------------------------------------------ counters count up once, in tabular figures (the final value is in the HTML)
    var counters = document.querySelectorAll('[data-count]');
    if (counters.length && !reduce && canObserve) {
        var format = new Intl.NumberFormat(root.lang || undefined);
        var countIo = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (!e.isIntersecting) return;
                countIo.unobserve(e.target);
                var el = e.target, target = parseInt(el.getAttribute('data-count'), 10) || 0, t0 = 0, duration = 900 + Math.min(600, target * 4);
                var tick = function (t) {
                    if (!t0) t0 = t;
                    var p = Math.min(1, (t - t0) / duration), eased = 1 - Math.pow(1 - p, 3);
                    el.textContent = format.format(Math.round(target * eased));
                    if (p < 1) requestAnimationFrame(tick);
                };
                el.textContent = format.format(0);
                requestAnimationFrame(tick);
            });
        }, { threshold: 0.6 });
        for (var c = 0; c < counters.length; c++) countIo.observe(counters[c]);
    }

    // ------------------------------------------------------------------ Honey Story phone: pointer tilt, at most 6 degrees (mouse and pen)
    var phone = document.querySelector('[data-tilt]');
    if (phone && !reduce) {
        var stage = phone.parentElement;
        stage.addEventListener('pointermove', function (e) {
            if (e.pointerType === 'touch') return;
            var r = phone.getBoundingClientRect();
            var x = Math.max(-1, Math.min(1, (e.clientX - r.left) / r.width * 2 - 1));
            var y = Math.max(-1, Math.min(1, (e.clientY - r.top) / r.height * 2 - 1));
            phone.style.setProperty('--ry', (x * 6).toFixed(2) + 'deg');
            phone.style.setProperty('--rx', (-y * 6).toFixed(2) + 'deg');
        });
        stage.addEventListener('pointerleave', function () { phone.style.setProperty('--ry', '0deg'); phone.style.setProperty('--rx', '0deg'); });
    }

    // ------------------------------------------------------------------ decorative loops pause off screen and in a hidden tab
    function onVisibility() { root.classList.toggle('is-hidden-tab', document.hidden); }
    document.addEventListener('visibilitychange', onVisibility);
    if (canObserve) {
        var offscreen = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) { e.target.classList.toggle('is-offscreen', !e.isIntersecting); });
        });
        Array.prototype.forEach.call(document.querySelectorAll('.ld-hero, .ld-night, .ld-story'), function (s) { offscreen.observe(s); });
    }

    // ------------------------------------------------------------------ the trust section's background video: plays only on screen and in a visible tab
    var video = document.querySelector('.ld-night-video');
    if (video) {
        video.muted = true;
        if (reduce || !canObserve) {
            video.preload = 'none';
        } else {
            var onScreen = false;
            var sync = function () {
                if (onScreen && !document.hidden) {
                    var p = video.play();
                    if (p && p.catch) p.catch(function () { /* autoplay refused: the poster stays */ });
                } else {
                    video.pause();
                }
            };
            new IntersectionObserver(function (entries) { onScreen = entries[0].isIntersecting; sync(); }, { threshold: 0.15 }).observe(video);
            document.addEventListener('visibilitychange', sync);
        }
    }

    draw();
    onScroll();
})();
