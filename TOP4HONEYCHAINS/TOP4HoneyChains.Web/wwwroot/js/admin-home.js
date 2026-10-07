/* D-72: the admin home (Views/Dashboard/Index.cshtml; docs/brand/MOTION.md). The figures count up once, when they first
   come into view; the server renders the final numbers, so without script (or with reduced motion) they simply show. */
(function () {
    'use strict';

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var values = document.querySelectorAll('.ah-kpi-value[data-count]');
    if (reduce || !values.length || !('IntersectionObserver' in window)) return;
    var format = new Intl.NumberFormat(document.documentElement.lang || undefined);

    function countUp(el) {
        var target = parseInt(el.getAttribute('data-count'), 10);
        if (!(target > 0)) return;
        var start = performance.now(), duration = Math.min(1400, 600 + target * 12);
        el.setAttribute('aria-label', format.format(target));   // screen readers get the final number at once
        (function frame(now) {
            var t = Math.min(1, (now - start) / duration), eased = 1 - Math.pow(1 - t, 3);
            el.textContent = format.format(Math.round(target * eased));
            if (t < 1) requestAnimationFrame(frame);
        })(start);
    }

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            if (!e.isIntersecting) return;
            io.unobserve(e.target);
            countUp(e.target);
        });
    }, { threshold: 0.6 });
    Array.prototype.forEach.call(values, function (el) { el.textContent = format.format(0); io.observe(el); });
})();
