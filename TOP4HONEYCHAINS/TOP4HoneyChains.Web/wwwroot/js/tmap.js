/* D-72: the shared Türkiye map (Views/Shared/_TurkeyMap.cshtml, wwwroot/css/tmap.css; docs/brand/MOTION.md).
   - the outline draws itself (and a Honey Story map fills in) once, when the map first scrolls into view;
   - the honey dots pulse only while the map is on screen (the CSS pauses them in a hidden tab and while typing);
   - hover, tap or keyboard focus on a province with data lifts it over a warm glow and shows its name and counts;
     Tab moves through those provinces, Escape hides the tooltip. Reduced motion: everything at rest.
   - D-98: a province with districts (data-drill) opens them on click, Enter or Space: the map zooms to the province
     (data-zoom of its district group), the back button or Escape returns to Türkiye and to the province. */
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
        var world = map.querySelector('.tmap-world'), bar = map.querySelector('.tmap-bar');
        var current = null, hideTimer = 0, shownAt = 0, opened = null, openedFrom = null;

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
        // the tooltip sits above the area's centroid (through the zoom of the world group), kept inside the map
        function place(p) {
            var stage = map.querySelector('.tmap-stage').getBoundingClientRect();
            var pt = svg.createSVGPoint();
            pt.x = parseFloat(p.getAttribute('data-cx'));
            pt.y = parseFloat(p.getAttribute('data-cy'));
            var ctm = (world || svg).getScreenCTM();
            var at = ctm ? pt.matrixTransform(ctm) : { x: stage.left, y: stage.top };
            var x = at.x - stage.left, y = at.y - stage.top;
            var half = Math.min(110, Math.max(60, tip.offsetWidth / 2 || 80));
            tip.style.left = Math.max(half, Math.min(stage.width - half, x)) + 'px';
            tip.style.top = Math.max(0, y - 6) + 'px';
        }

        // D-98: open a province's districts, zoomed to fit; back to Türkiye
        function open(p) {
            var group = document.getElementById(p.getAttribute('data-drill'));
            if (!group || !world) return;
            hide();
            var z = (group.getAttribute('data-zoom') || '1 0 0').split(' ');
            world.style.transform = 'translate(' + z[1] + 'px, ' + z[2] + 'px) scale(' + z[0] + ')';
            map.style.setProperty('--z', z[0]);
            map.classList.add('is-drilled');
            group.classList.add('is-open');
            opened = group;
            openedFrom = p;
            bar.hidden = false;
            bar.querySelector('.tmap-where').textContent = group.getAttribute('data-name');
            bar.querySelector('.tmap-back').focus();
        }
        function close() {
            if (!opened) return;
            hide();
            world.style.transform = '';
            map.style.removeProperty('--z');
            map.classList.remove('is-drilled');
            opened.classList.remove('is-open');
            opened = null;
            bar.hidden = true;
            if (openedFrom) openedFrom.focus();
            openedFrom = null;
        }
        // the zoom eases in and out: place an open tooltip again once it has settled
        if (world) world.addEventListener('transitionend', function (e) { if (e.target === world && current) place(current); });
        if (bar) bar.querySelector('.tmap-back').addEventListener('click', close);
        map.addEventListener('keydown', function (e) { if (e.key === 'Escape' && opened && !current) close(); });

        var areas = map.querySelectorAll('.tmap-p.is-data, .tmap-d.is-data');
        Array.prototype.forEach.call(areas, function (p) {
            var drills = p.hasAttribute('data-drill');
            p.addEventListener('pointerenter', function (e) { if (e.pointerType !== 'touch') show(p, false); });
            p.addEventListener('pointerleave', function (e) { if (e.pointerType !== 'touch' && current === p) hide(); });
            p.addEventListener('click', function () {
                if (drills) open(p);
                else if (current === p && Date.now() - shownAt > 450) hide();
                else show(p, map.classList.contains('is-focus'));
            });
            p.addEventListener('focus', function () { var ring = true; try { ring = p.matches(':focus-visible'); } catch (e) { /* older browsers */ } show(p, ring); });
            p.addEventListener('blur', function () { if (current === p) hide(); });
            p.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') { if (current) { hide(); e.stopPropagation(); } }
                else if (drills && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); open(p); }
            });
        });
        // a tap outside an area closes the tooltip on touch screens
        document.addEventListener('pointerdown', function (e) { if (current && !map.contains(e.target)) hide(); });
    });
})();
