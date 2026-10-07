/* D-72: the public Honey Story (Views/HoneyStory/Index.cshtml, wwwroot/css/honeystory.css; docs/brand/MOTION.md). Phone first and light:
   - the journey lights up step by step as it scrolls into view, the light path grows with it;
   - the step photos (decorative, never photos of this batch) load only when their step comes near;
   - lab meters move to their values and the share bars grow, once, when they come into view;
   - the verified band stamps its seal once.
   The page is armed (.hs-armed) only when this can run; with reduced motion everything is shown at rest. */
(function () {
    'use strict';

    var page = document.getElementById('honeystory');
    if (!page) return;
    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var io = 'IntersectionObserver' in window;

    // decorative step backgrounds: only when near (also with reduced motion; they do not move)
    var backgrounds = page.querySelectorAll('.hs-jbg[data-bg]');
    function load(bg) {
        if (bg.classList.contains('is-loaded')) return;
        var img = new Image();
        img.onload = function () { bg.style.backgroundImage = 'url("' + bg.getAttribute('data-bg') + '")'; bg.classList.add('is-loaded'); };
        img.src = bg.getAttribute('data-bg');
    }
    if (io) {
        var near = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) { if (e.isIntersecting) { near.unobserve(e.target); load(e.target); } });
        }, { rootMargin: '200px 0px' });
        Array.prototype.forEach.call(backgrounds, function (bg) { near.observe(bg); });
    } else {
        Array.prototype.forEach.call(backgrounds, load);
    }

    var box = page.querySelector('[data-journey]');
    var steps = box ? Array.prototype.slice.call(box.querySelectorAll('[data-step]')) : [];
    var fill = box && box.querySelector('.hs-jline-fill');
    var line = box && box.querySelector('.hs-jline');

    // the line runs from the first node to the last one
    function measure() {
        if (!line || steps.length < 2) return;
        var top = steps[0].querySelector('.hs-jnode'), bottom = steps[steps.length - 1].querySelector('.hs-jnode');
        var b = box.getBoundingClientRect(), t = top.getBoundingClientRect(), l = bottom.getBoundingClientRect();
        line.style.top = (t.top - b.top + t.height / 2) + 'px';
        line.style.height = Math.max(0, l.top - t.top) + 'px';
    }
    measure();
    window.addEventListener('resize', measure);

    if (reduce || !io) {
        steps.forEach(function (s) { s.classList.add('is-lit'); });
        return;
    }
    page.classList.add('hs-armed');

    var lit = -1;
    function light(index) {
        if (index <= lit) return;
        lit = index;
        steps.forEach(function (s, i) { if (i <= index) s.classList.add('is-lit'); });
        if (fill) fill.style.setProperty('--p', steps.length > 1 ? String(index / (steps.length - 1)) : '1');
    }
    if (fill) fill.style.setProperty('--p', '0');
    var stepWatch = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) { if (e.isIntersecting) light(steps.indexOf(e.target)); });
    }, { rootMargin: '0px 0px -30% 0px', threshold: 0.4 });
    steps.forEach(function (s) { stepWatch.observe(s); });

    // meters, share bars and the seal: once, when they come into view
    var once = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            if (!e.isIntersecting) return;
            once.unobserve(e.target);
            e.target.classList.add(e.target.hasAttribute('data-seal') ? 'is-sealed' : 'is-seen');
        });
    }, { threshold: 0.5 });
    Array.prototype.forEach.call(page.querySelectorAll('.hs-tile, .hs-legend-item, [data-seal]'), function (el) { once.observe(el); });
})();
