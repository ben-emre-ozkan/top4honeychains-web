/* Landing page motion (D-65, D-70): scroll reveals, the honey line along the chain's steps, the header's shadow,
   pausing the decorative loops off screen, the trust section's background video.
   Without JavaScript, or with prefers-reduced-motion, everything is shown at once and the line is fully drawn. */
(function () {
    'use strict';

    var root = document.documentElement;
    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // reveals: .ld-reveal fades in once it enters the viewport (hidden only when html.js is set and motion is allowed)
    var reveals = document.querySelectorAll('.ld-reveal');
    if (reduce || !('IntersectionObserver' in window)) {
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

    // the honey line: drawn up to the middle of the viewport; a step's node lights up once the line reaches it
    var list = document.querySelector('[data-honey-line]');
    var fill = list && list.querySelector('.ld-spine-fill');
    var nodes = list ? list.querySelectorAll('.ld-step') : [];
    var ticking = false, lineVisible = true;
    function draw() {
        ticking = false;
        if (!list || !fill) return;
        var rect = list.getBoundingClientRect();
        var progress = reduce ? 1 : Math.max(0, Math.min(1, (window.innerHeight * 0.6 - rect.top) / rect.height));
        fill.style.transform = 'scaleY(' + progress.toFixed(4) + ')';
        var reached = rect.top + progress * rect.height;
        for (var k = 0; k < nodes.length; k++) {
            var node = nodes[k].querySelector('.ld-node');
            var top = node.getBoundingClientRect().top + node.offsetHeight / 2;
            nodes[k].classList.toggle('is-lit', reduce || top <= reached + 1);
        }
    }
    function onScroll() {
        if (!ticking && lineVisible) { ticking = true; requestAnimationFrame(draw); }
        header && header.classList.toggle('is-scrolled', window.scrollY > 8);
    }
    var header = document.querySelector('.ld-top');
    if (list && 'IntersectionObserver' in window) {
        // compute only while the steps are near the screen
        new IntersectionObserver(function (entries) { lineVisible = entries[0].isIntersecting; if (lineVisible) onScroll(); }, { rootMargin: '25% 0px' }).observe(list);
    }
    // D-70: decorative loops pause off screen and in a hidden tab
    function onVisibility() { root.classList.toggle('is-hidden-tab', document.hidden); }
    document.addEventListener('visibilitychange', onVisibility);
    var sections = document.querySelectorAll('.ld-hero, .ld-night');
    if ('IntersectionObserver' in window) {
        var offscreen = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) { e.target.classList.toggle('is-offscreen', !e.isIntersecting); });
        });
        for (var s = 0; s < sections.length; s++) offscreen.observe(sections[s]);
    }

    // D-70: the trust section's background video: muted, inline, plays only while on screen and the tab is visible;
    // with reduced motion (or without IntersectionObserver) only the poster is shown
    var video = document.querySelector('.ld-night-video');
    if (video) {
        video.muted = true;
        if (reduce || !('IntersectionObserver' in window)) {
            video.preload = 'none';
            video.removeAttribute('autoplay');
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

    window.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('resize', onScroll, { passive: true });
    draw();
    onScroll();
})();
