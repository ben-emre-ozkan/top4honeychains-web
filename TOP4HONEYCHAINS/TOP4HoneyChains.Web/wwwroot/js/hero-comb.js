/* Landing hero: the live honeycomb behind the copy and the five-step chain (D-71; reference
   docs/design/landing-hero/hero-prototype.html).
   - one canvas; the base grid is drawn once to an offscreen canvas and copied each frame;
   - pointer heat (mouse and pen only): cells near the pointer turn honey and fade; a click or tap sends a ripple;
   - a bright honey thread with a glowing head travels from step to step along a smooth curve through the real
     positions of the medallions (.ld-hnode); the cells it passes stay lit; each step lights up on arrival;
     when all five are lit it holds, fades and starts again; pollen motes drift upwards;
   - cells under the hero copy ([data-comb-calm]) are only tinted, so the text keeps its contrast;
   - device pixel ratio capped at 2; pauses off screen and in a hidden tab;
   - prefers-reduced-motion: one static frame with the whole path and all steps lit.
   Decorative only: the canvas is aria-hidden and the steps are an ordered list in the page. */
(function () {
    'use strict';

    var hero = document.querySelector('[data-hero]');
    var cv = hero && hero.querySelector('[data-hero-comb]');
    if (!cv || !cv.getContext) return;
    var ctx = cv.getContext('2d');
    var reduceQuery = window.matchMedia ? window.matchMedia('(prefers-reduced-motion: reduce)') : { matches: false };
    var nodes = Array.prototype.slice.call(hero.querySelectorAll('.ld-hnode'));

    var R = 24, GAP = 2.2, HW = Math.sqrt(3) * R, RH = 1.5 * R;
    var HEAT_R = 120, CALM = 0.3, SPEED = 58;   // heat radius (px); max heat under the copy; path samples per second
    var W = 0, H = 0, dpr = 1, rows = 0, cols = 0, grid = [], cells = [], base = null;
    var path = [], segEnds = [], centers = [], motes = [], ripples = [], pointer = null;
    var head = 0, phase = 'run', phaseT = 0, segIdx = 0;

    function hexPath(c, x, y, r) {
        for (var i = 0; i < 6; i++) {
            var a = Math.PI / 180 * (60 * i - 90), px = x + r * Math.cos(a), py = y + r * Math.sin(a);
            if (i) c.lineTo(px, py); else c.moveTo(px, py);
        }
        c.closePath();
    }
    function cellAt(r, q) { var row = grid[r + 1]; return row ? row[q + 1] || null : null; }
    function nearest(x, y) {
        var r0 = Math.round(y / RH), best = null, bd = 1e12;
        for (var r = r0 - 1; r <= r0 + 1; r++) {
            var q0 = Math.round((x - (r & 1 ? HW / 2 : 0)) / HW);
            for (var q = q0 - 1; q <= q0 + 1; q++) {
                var c = cellAt(r, q);
                if (!c) continue;
                var d = (c.x - x) * (c.x - x) + (c.y - y) * (c.y - y);
                if (d < bd) { bd = d; best = c; }
            }
        }
        return best;
    }
    // every cell within radius of (x, y), visited through the grid instead of the whole list
    function around(x, y, radius, fn) {
        var r1 = Math.max(-1, Math.floor((y - radius) / RH)), r2 = Math.min(rows - 1, Math.ceil((y + radius) / RH));
        for (var r = r1; r <= r2; r++) {
            var off = r & 1 ? HW / 2 : 0;
            var q1 = Math.max(-1, Math.floor((x - radius - off) / HW)), q2 = Math.min(cols - 1, Math.ceil((x + radius - off) / HW));
            for (var q = q1; q <= q2; q++) { var c = cellAt(r, q); if (c) fn(c); }
        }
    }

    // build: layout reads and geometry in one task, the offscreen grid in the next (no long task on load)
    function build() {
        var t0 = performance.now();
        var rect = hero.getBoundingClientRect();
        W = rect.width; H = rect.height;
        if (W < 1 || H < 1) return;
        dpr = Math.min(2, window.devicePixelRatio || 1);
        cv.width = Math.round(W * dpr); cv.height = Math.round(H * dpr);
        rows = Math.ceil(H / RH) + 2; cols = Math.ceil(W / HW) + 2;
        grid = []; cells = [];
        for (var r = -1; r < rows; r++) {
            var row = [];
            for (var q = -1; q < cols; q++) {
                var c = { x: q * HW + (r & 1 ? HW / 2 : 0), y: r * RH, heat: 0, keep: 0, calm: false };
                row.push(c); cells.push(c);
            }
            grid.push(row);
        }
        // the copy stays readable: cells behind it are only tinted
        Array.prototype.forEach.call(hero.querySelectorAll('[data-comb-calm]'), function (el) {
            var b = el.getBoundingClientRect();
            var x1 = b.left - rect.left - R, x2 = b.right - rect.left + R, y1 = b.top - rect.top - R, y2 = b.bottom - rect.top + R;
            cells.forEach(function (c) { if (c.x > x1 && c.x < x2 && c.y > y1 && c.y < y2) c.calm = true; });
        });
        // medallion centres in hero coordinates and a smooth curve between them, sampled every 4 px
        centers = nodes.map(function (n) {
            var h = n.querySelector('.ld-hx').getBoundingClientRect();
            return { x: h.left - rect.left + h.width / 2, y: h.top - rect.top + h.height / 2, rad: h.width * 0.62 };
        });
        path = []; segEnds = [];
        for (var i = 0; i < centers.length - 1; i++) {
            var A = centers[i], B = centers[i + 1];
            var mx = (A.x + B.x) / 2, my = (A.y + B.y) / 2, nx = -(B.y - A.y), ny = B.x - A.x, nl = Math.hypot(nx, ny) || 1;
            var bow = (i % 2 ? 1 : -1) * Math.min(60, nl * 0.18), cx = mx + nx / nl * bow, cy = my + ny / nl * bow;
            var len = Math.hypot(B.x - A.x, B.y - A.y) * 1.15;
            for (var t = 0; t <= 1; t += 4 / len) {
                var u = 1 - t, x = u * u * A.x + 2 * u * t * cx + t * t * B.x, y = u * u * A.y + 2 * u * t * cy + t * t * B.y;
                if (Math.hypot(x - A.x, y - A.y) < A.rad || Math.hypot(x - B.x, y - B.y) < B.rad) continue;
                path.push({ x: x, y: y, cell: nearest(x, y) });
            }
            segEnds.push(path.length - 1);
        }
        motes = [];
        for (var m = 0, n = Math.round(W / 70); m < n; m++)
            motes.push({ x: Math.random() * W, y: Math.random() * H, s: 1 + Math.random() * 2.6, v: 0.08 + Math.random() * 0.22, p: Math.random() * 6.28 });
        restart();
        measure('hero-comb:geometry', t0);
        requestAnimationFrame(function () {
            var t1 = performance.now();
            var grid2 = document.createElement('canvas');
            grid2.width = cv.width; grid2.height = cv.height;
            var b = grid2.getContext('2d');
            b.scale(dpr, dpr);
            b.beginPath();
            cells.forEach(function (c) { hexPath(b, c.x, c.y, R - GAP); });
            b.strokeStyle = 'rgba(214, 168, 88, 0.22)'; b.lineWidth = 1; b.stroke();
            base = grid2;
            measure('hero-comb:grid', t1);
            if (reduceQuery.matches) drawStatic(); else start();
        });
    }
    function measure(name, from) { if (performance.measure) try { performance.measure(name, { start: from, end: performance.now() }); } catch (e) { /* older browsers */ } }

    function lightNode(i, on) { if (nodes[i]) nodes[i].classList.toggle('is-lit', on); }
    function restart() {
        head = 0; segIdx = 0; phase = 'run'; phaseT = 0;
        cells.forEach(function (c) { c.keep = 0; });
        nodes.forEach(function (n, i) { lightNode(i, i === 0); });
    }

    var lerp = function (a, b, t) { return a + (b - a) * t; };
    function honey(h) { return 'rgb(255,' + Math.round(lerp(236, 172, h)) + ',' + Math.round(lerp(176, 46, h)) + ')'; }

    function step(dt) {
        if (pointer) around(pointer.x, pointer.y, HEAT_R, function (c) {
            var d = Math.hypot(c.x - pointer.x, c.y - pointer.y);
            if (d < HEAT_R) { var v = Math.pow(1 - d / HEAT_R, 1.6) * 0.95; if (v > c.heat) c.heat = v; }
        });
        ripples = ripples.filter(function (rp) { return (rp.t += dt) < 1.4; });
        ripples.forEach(function (rp) {
            var rad = rp.t * 520, fade = 1 - rp.t / 1.4;
            around(rp.x, rp.y, rad + 22, function (c) {
                var d = Math.abs(Math.hypot(c.x - rp.x, c.y - rp.y) - rad);
                if (d < 22) { var v = (1 - d / 22) * 0.8 * fade; if (v > c.heat) c.heat = v; }
            });
        });
        if (!path.length) return;
        if (phase === 'run') {
            var from = Math.floor(head);
            head += SPEED * dt;
            for (var i = Math.max(0, from); i <= Math.min(head, path.length - 1); i++) if (path[i].cell) path[i].cell.keep = 0.64;
            var end = segEnds[segIdx];
            if (head >= end + 1) { head = end + 1; lightNode(segIdx + 1, true); phase = 'dwell'; phaseT = 0; }
        } else if (phase === 'dwell') {
            phaseT += dt;
            if (phaseT > 0.7) { segIdx++; if (segIdx >= segEnds.length) { phase = 'hold'; phaseT = 0; } else phase = 'run'; }
        } else if (phase === 'hold') {
            phaseT += dt;
            if (phaseT > 2.6) { phase = 'fade'; phaseT = 0; nodes.forEach(function (n, k) { lightNode(k, false); }); }
        } else if (phase === 'fade') {
            phaseT += dt;
            var cap = 0.64 * (1 - phaseT / 0.9);
            cells.forEach(function (c) { if (c.keep > cap) c.keep = Math.max(0, cap); });
            if (phaseT > 1) restart();
        }
        var decay = Math.pow(0.18, dt);
        for (var k = 0; k < cells.length; k++) if (cells[k].heat) { cells[k].heat *= decay; if (cells[k].heat < 0.01) cells[k].heat = 0; }
    }

    function headPos() {
        if (!path.length) return null;
        var i = Math.min(Math.floor(head), path.length - 1), j = Math.min(i + 1, path.length - 1), f = head - Math.floor(head);
        return { x: lerp(path[i].x, path[j].x, f), y: lerp(path[i].y, path[j].y, f) };
    }

    function thread(upto, hp) {
        var s = 0;
        for (var k = 0; k < segEnds.length; k++) {
            var e = Math.min(segEnds[k], Math.floor(upto));
            if (e > s) {
                ctx.beginPath(); ctx.moveTo(path[s].x, path[s].y);
                for (var i = s + 1; i <= e; i++) ctx.lineTo(path[i].x, path[i].y);
                if (k === segIdx && hp && phase === 'run') ctx.lineTo(hp.x, hp.y);
                ctx.stroke();
            }
            s = segEnds[k] + 1;
            if (s > upto) break;
        }
    }

    function draw(time) {
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.clearRect(0, 0, cv.width, cv.height);
        if (base) ctx.drawImage(base, 0, 0);
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        var hp = phase === 'run' ? headPos() : null;
        for (var k = 0; k < cells.length; k++) {
            var c = cells[k], h = c.heat > c.keep ? c.heat : c.keep;
            if (hp) { var d = Math.hypot(c.x - hp.x, c.y - hp.y); if (d < 70 && 1 - d / 70 > h) h = 1 - d / 70; }
            if (c.calm && h > CALM) h = CALM;
            if (h < 0.03) continue;
            ctx.beginPath(); hexPath(ctx, c.x, c.y, R - GAP);
            ctx.globalAlpha = Math.min(1, 0.18 + h * 0.82); ctx.fillStyle = honey(h); ctx.fill();
            ctx.globalAlpha = Math.min(1, h + 0.1); ctx.strokeStyle = 'rgba(255, 190, 70, 0.9)'; ctx.lineWidth = 1.2; ctx.stroke();
        }
        ctx.globalAlpha = 1;
        // the thread: a soft wide glow under a bright core (no shadowBlur, which is costly per frame)
        var upto = phase === 'run' ? head : (phase === 'fade' ? -1 : path.length - 1);
        if (upto > 0) {
            ctx.save(); ctx.lineCap = 'round'; ctx.lineJoin = 'round';
            ctx.strokeStyle = 'rgba(255, 176, 32, 0.28)'; ctx.lineWidth = 12; thread(upto, hp);
            ctx.strokeStyle = 'rgba(255, 168, 36, 0.95)'; ctx.lineWidth = 3.5; thread(upto, hp);
            ctx.restore();
        }
        if (hp) {
            ctx.save(); ctx.globalCompositeOperation = 'lighter';
            var g = ctx.createRadialGradient(hp.x, hp.y, 0, hp.x, hp.y, 56);
            g.addColorStop(0, 'rgba(255, 236, 170, 1)'); g.addColorStop(0.25, 'rgba(255, 196, 70, 0.85)'); g.addColorStop(1, 'rgba(255, 170, 40, 0)');
            ctx.fillStyle = g; ctx.beginPath(); ctx.arc(hp.x, hp.y, 56, 0, Math.PI * 2); ctx.fill(); ctx.restore();
        }
        ctx.fillStyle = '#FFB547';
        for (var m = 0; m < motes.length; m++) {
            var mo = motes[m];
            if (time) { mo.y -= mo.v; mo.x += Math.sin(time / 1700 + mo.p) * 0.15; if (mo.y < -6) { mo.y = H + 6; mo.x = Math.random() * W; } }
            ctx.globalAlpha = 0.35 + 0.3 * Math.sin((time || 0) / 900 + mo.p);
            ctx.beginPath(); ctx.arc(mo.x, mo.y, mo.s, 0, Math.PI * 2); ctx.fill();
        }
        ctx.globalAlpha = 1;
    }

    function drawStatic() {
        path.forEach(function (p) { if (p.cell) p.cell.keep = 0.64; });
        nodes.forEach(function (n, i) { lightNode(i, true); });
        phase = 'hold'; head = path.length - 1;
        draw(0);
    }

    var last = 0, running = false, visible = true, frameCost = 0;
    window.heroCombStats = function () { return { maxFrameMs: Math.round(frameCost * 10) / 10 }; };
    function loop(t) {
        if (!running) return;
        var dt = Math.min(0.05, (t - (last || t)) / 1000); last = t;
        var t0 = performance.now();
        step(dt); draw(t);
        frameCost = Math.max(frameCost, performance.now() - t0);
        requestAnimationFrame(loop);
    }
    function start() { if (reduceQuery.matches || running || !visible || document.hidden || !base) return; running = true; last = 0; requestAnimationFrame(loop); }
    function stop() { running = false; }

    hero.addEventListener('pointermove', function (e) {
        if (e.pointerType === 'touch') return;   // touch gets the ripple only
        var r = hero.getBoundingClientRect();
        pointer = { x: e.clientX - r.left, y: e.clientY - r.top };
    });
    hero.addEventListener('pointerleave', function () { pointer = null; });
    hero.addEventListener('pointerdown', function (e) {
        if (reduceQuery.matches || (e.target.closest && e.target.closest('a, button'))) return;
        var r = hero.getBoundingClientRect();
        ripples.push({ x: e.clientX - r.left, y: e.clientY - r.top, t: 0 });
    });
    if ('IntersectionObserver' in window)
        new IntersectionObserver(function (es) { visible = es[0].isIntersecting; if (visible) start(); else stop(); }).observe(hero);
    document.addEventListener('visibilitychange', function () { if (document.hidden) stop(); else start(); });
    // the layout changes with the viewport, the fonts and the images: rebuild once it settles
    // one build after load (fonts and images settle the layout), then again only when the hero's size changes
    var timer = 0, lastSize = '', loaded = false;
    function rebuild(force) {
        var r = hero.getBoundingClientRect(), size = Math.round(r.width) + 'x' + Math.round(r.height);
        if (size === lastSize && !force) return;
        lastSize = size; stop(); build();
    }
    function schedule(force) {
        if (!loaded) return;
        clearTimeout(timer);
        timer = setTimeout(function () { rebuild(force); }, 150);
    }
    if ('ResizeObserver' in window) new ResizeObserver(function () { schedule(false); }).observe(hero);
    else window.addEventListener('resize', function () { schedule(false); });
    if (reduceQuery.addEventListener) reduceQuery.addEventListener('change', function () { schedule(true); });
    function first() {
        loaded = true;
        var go = function () { (document.fonts && document.fonts.ready ? document.fonts.ready : Promise.resolve()).then(function () { rebuild(true); }); };
        if ('requestIdleCallback' in window) window.requestIdleCallback(go, { timeout: 600 }); else setTimeout(go, 50);
    }
    if (document.readyState === 'complete') first(); else window.addEventListener('load', first);
})();
