/* D-72: the shared Türkiye map (Views/Shared/_TurkeyMap.cshtml, wwwroot/css/tmap.css; docs/brand/MOTION.md).
   - the outline draws itself (and a Honey Story map fills in) once, when the map first scrolls into view;
   - the honey dots pulse only while the map is on screen (the CSS pauses them in a hidden tab and while typing);
   - hover, tap or keyboard focus on a province with data lifts it over a warm glow and shows its name and counts;
     Tab moves through those provinces, Escape hides the tooltip. Reduced motion: everything at rest. */
(function () {
    'use strict';

    var maps = document.querySelectorAll('[data-tmap]');
    if (!maps.length) return;
    var root = document.documentElement;
    document.addEventListener('visibilitychange', function () { root.classList.toggle('is-hidden-tab', document.hidden); });

    Array.prototype.forEach.call(maps, function (map) {
        var svg = map.querySelector('.tmap-svg');
        var tip = map.querySelector('.tmap-tip');
        var glow = map.querySelector('.tmap-glow'), lift = map.querySelector('.tmap-lift'), ring = map.querySelector('.tmap-ring');
        var current = null, hideTimer = 0, shownAt = 0;

        // first view: arm (hide the outline and the fill), then draw when visible; without the observer, show at once
        if ('IntersectionObserver' in window) {
            map.classList.add('is-armed');
            var seen = false;
            new IntersectionObserver(function (entries) {
                var on = entries[0].isIntersecting;
                map.classList.toggle('is-live', on);
                if (on && !seen) {
                    seen = true;
                    requestAnimationFrame(function () { map.classList.add('is-drawn'); });
                }
            }, { threshold: 0.25 }).observe(map);
        } else {
            map.classList.add('is-live');
        }

        function show(p, focus) {
            clearTimeout(hideTimer);
            if (current !== p) {
                current = p;
                shownAt = Date.now();
                var ref = '#' + p.id;
                [glow, lift].forEach(function (u) { u.setAttribute('href', ref); });
                lift.style.transformOrigin = ring.style.transformOrigin = p.getAttribute('data-cx') + 'px ' + p.getAttribute('data-cy') + 'px';
                ring.setAttribute('d', p.getAttribute('d'));
                tip.querySelector('b').textContent = p.getAttribute('data-name');
                tip.querySelector('span').textContent = p.getAttribute('data-tip');
            }
            map.classList.add('is-hot');
            map.classList.toggle('is-focus', !!focus);
            place(p);
            tip.hidden = false;
            requestAnimationFrame(function () { tip.classList.add('is-on'); });
        }
        function hide() {
            map.classList.remove('is-hot', 'is-focus');
            tip.classList.remove('is-on');
            hideTimer = setTimeout(function () { tip.hidden = true; }, 180);
            current = null;
        }
        // the tooltip sits above the province's centroid, kept inside the map
        function place(p) {
            var box = svg.getBoundingClientRect(), stage = map.querySelector('.tmap-stage').getBoundingClientRect();
            var vb = svg.viewBox.baseVal, k = box.width / vb.width;
            var x = box.left - stage.left + parseFloat(p.getAttribute('data-cx')) * k;
            var y = box.top - stage.top + parseFloat(p.getAttribute('data-cy')) * k;
            var half = Math.min(110, Math.max(60, tip.offsetWidth / 2 || 80));
            tip.style.left = Math.max(half, Math.min(stage.width - half, x)) + 'px';
            tip.style.top = Math.max(0, y - 6) + 'px';
        }

        var provinces = map.querySelectorAll('.tmap-p.is-data');
        Array.prototype.forEach.call(provinces, function (p) {
            p.addEventListener('pointerenter', function (e) { if (e.pointerType !== 'touch') show(p, false); });
            p.addEventListener('pointerleave', function (e) { if (e.pointerType !== 'touch' && current === p) hide(); });
            p.addEventListener('click', function () { if (current === p && Date.now() - shownAt > 450) hide(); else show(p, map.classList.contains('is-focus')); });
            p.addEventListener('focus', function () { var ring = true; try { ring = p.matches(':focus-visible'); } catch (e) { /* older browsers */ } show(p, ring); });
            p.addEventListener('blur', function () { if (current === p) hide(); });
            p.addEventListener('keydown', function (e) { if (e.key === 'Escape') hide(); });
        });
        // a tap outside a province closes the tooltip on touch screens
        document.addEventListener('pointerdown', function (e) { if (current && !map.contains(e.target)) hide(); });
    });
})();
