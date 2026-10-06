/* Landing page motion (D-65): scroll reveals, the honey line along the chain's steps, the header's shadow.
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
    window.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('resize', onScroll, { passive: true });
    draw();
    onScroll();
})();
