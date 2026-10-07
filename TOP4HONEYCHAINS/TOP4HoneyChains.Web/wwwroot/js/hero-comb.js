/* Landing hero: the live honeycomb behind the copy and the five-step chain (D-71; reference
   docs/design/landing-hero/hero-prototype.html).
   - one canvas; the static grid is drawn once (in slices) to an offscreen canvas and copied each frame;
   - pointer heat (mouse and pen only): cells near the pointer turn honey and fade; a click or tap sends a ripple;
   - a bright honey thread with a glowing head travels from step to step along a gentle curve that leaves one
     medallion (.ld-hnode) exactly at its hexagon frame and ends in an arrowhead at the next; it eases in and out on
     each leg; the cells it passes warm up and stay lit, fading smoothly; each step lights up on arrival with a soft
     ripple; when all five are lit it holds, the steps dim one after another, and it starts again; pollen drifts upwards;
   - cells under the hero copy ([data-comb-calm]) are only tinted, so the text keeps its contrast;
   - device pixel ratio capped at 2; pauses off screen and in a hidden tab;
   - prefers-reduced-motion: one static frame with the whole path and all steps lit.
   Decorative only: the canvas is aria-hidden and the steps are an ordered list in the page.
   D-72: every [data-hero] gets its own engine; data-comb-r (cell radius, default 24) scales it, data-comb-node and
   data-comb-hex name the step elements and their hexagon frames (default .ld-hnode and .ld-hx). */
(function () {
    'use strict';

    // one engine per [data-hero] element: the landing hero, and (D-72) the admin home's chain strip
    function mount(hero) {
        var cv = hero.querySelector('[data-hero-comb]');
        if (!cv || !cv.getContext) return;
        var ctx = cv.getContext('2d');
        var PIXEL_BUDGET = 2400000;   // device pixels; a very tall hero gets a slightly lower ratio (never above 2)
        var reduceQuery = window.matchMedia ? window.matchMedia('(prefers-reduced-motion: reduce)') : { matches: false };
        var nodes = Array.prototype.slice.call(hero.querySelectorAll(hero.getAttribute('data-comb-node') || '.ld-hnode'));
        var HEX = hero.getAttribute('data-comb-hex') || '.ld-hx';

        // D-72: data-comb-r scales the whole engine (24 on the landing hero, smaller in the admin home's chain strip)
        var R = parseFloat(hero.getAttribute('data-comb-r')) || 24, S = R / 24, LW = Math.max(0.7, S);
        var GAP = 2.2 * LW, HW = Math.sqrt(3) * R, RH = 1.5 * R;
        var HEAT_R = 120 * S, CALM = 0.3, PX_PER_S = 240 * LW, KEEP = 0.64, HEAD_R = 84 * S;   // heat radius; max heat under the copy; travel speed; lit level; head glow radius
        var W = 0, H = 0, dpr = 1, rows = 0, cols = 0, grid = [], cells = [], base = null;
        var path = [], segs = [], centers = [], motes = [], ripples = [], pointer = null;
        var head = 0, phase = 'run', phaseT = 0, segIdx = 0, segT = 0;
        var buildToken = 0;

        function hexPath(c, x, y, r) {
            for (var i = 0; i < 6; i++) {
                var a = Math.PI / 180 * (60 * i - 90), px = x + r * Math.cos(a), py = y + r * Math.sin(a);
                if (i) c.lineTo(px, py); else c.moveTo(px, py);
            }
            c.closePath();
        }
        // pointy-top hexagon test: |dx| within the half width, |dy| within the half height minus the slanted corners
        function insideHex(x, y, C) {
            var dx = Math.abs(x - C.x), dy = Math.abs(y - C.y);
            return dx <= C.hw && dy <= C.hh - dx * (C.hh / 2) / C.hw;
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
            dpr = Math.min(2, window.devicePixelRatio || 1, Math.sqrt(PIXEL_BUDGET / (W * H)));
            cv.width = Math.round(W * dpr); cv.height = Math.round(H * dpr);
            rows = Math.ceil(H / RH) + 2; cols = Math.ceil(W / HW) + 2;
            grid = []; cells = [];
            for (var r = -1; r < rows; r++) {
                var row = [];
                for (var q = -1; q < cols; q++) {
                    var c = { x: q * HW + (r & 1 ? HW / 2 : 0), y: r * RH, heat: 0, keep: 0, target: 0, calm: false };
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
            // medallion centres and hexagon half-sizes (the lit scale included), from the layout box so a lit, scaled
            // medallion does not change the geometry
            centers = nodes.map(function (n) {
                var hx = n.querySelector(HEX), b = hx.getBoundingClientRect();
                var w = hx.offsetWidth * 1.05, h = hx.offsetHeight * 1.05;
                return { x: b.left - rect.left + b.width / 2, y: b.top - rect.top + b.height / 2, hw: w / 2, hh: h / 2 };
            });
            // a gentle curve between consecutive medallions, sampled every 3 px, kept only outside both hexagons: the
            // thread leaves one frame and reaches the next exactly at the edge
            path = []; segs = [];
            for (var i = 0; i < centers.length - 1; i++) {
                var A = centers[i], B = centers[i + 1];
                var mx = (A.x + B.x) / 2, my = (A.y + B.y) / 2, nx = -(B.y - A.y), ny = B.x - A.x, nl = Math.hypot(nx, ny) || 1;
                var bow = (i % 2 ? 1 : -1) * Math.min(28 * S, nl * 0.08), cx = mx + nx / nl * bow, cy = my + ny / nl * bow;
                var steps = Math.max(20, Math.ceil(nl * 1.05 / 3)), segFirst = path.length;
                for (var k2 = 0; k2 <= steps; k2++) {
                    var t = k2 / steps, u = 1 - t, x = u * u * A.x + 2 * u * t * cx + t * t * B.x, y = u * u * A.y + 2 * u * t * cy + t * t * B.y;
                    if (insideHex(x, y, A) || insideHex(x, y, B)) continue;
                    path.push({ x: x, y: y, cell: nearest(x, y) });
                }
                var end = path.length - 1, len = 0;
                for (var j2 = segFirst + 1; j2 <= end; j2++) len += Math.hypot(path[j2].x - path[j2 - 1].x, path[j2].y - path[j2 - 1].y);
                segs.push({ start: segFirst, end: end, dur: Math.max(0.9, len / PX_PER_S) });
            }
            motes = [];
            for (var m = 0, n = Math.round(W / 70); m < n; m++)
                motes.push({ x: Math.random() * W, y: Math.random() * H, s: 1 + Math.random() * 2.6, v: 0.08 + Math.random() * 0.22, p: Math.random() * 6.28 });
            restart();
            measure('hero-comb:geometry', t0);
            // the static grid in three slices, one per frame, so no slice becomes a long task
            var gridCanvas = document.createElement('canvas');
            gridCanvas.width = cv.width; gridCanvas.height = cv.height;
            var g2 = gridCanvas.getContext('2d');
            g2.setTransform(dpr, 0, 0, dpr, 0, 0);
            g2.strokeStyle = 'rgba(214, 168, 88, 0.22)'; g2.lineWidth = 1;
            var slice = Math.ceil(cells.length / 3), from = 0, token = ++buildToken;
            (function strokeSlice() {
                requestAnimationFrame(function () {
                    if (token !== buildToken) return;   // a newer build took over
                    var t1 = performance.now();
                    g2.beginPath();
                    for (var k = from; k < Math.min(cells.length, from + slice); k++) hexPath(g2, cells[k].x, cells[k].y, R - GAP);
                    g2.stroke();
                    from += slice;
                    measure('hero-comb:grid', t1);
                    if (from < cells.length) { strokeSlice(); return; }
                    base = gridCanvas;
                    if (reduceQuery.matches) drawStatic(); else start();
                });
            })();
        }
        function measure(name, from) { if (performance.measure) try { performance.measure(name, { start: from, end: performance.now() }); } catch (e) { /* older browsers */ } }

        function lightNode(i, on) { if (nodes[i]) nodes[i].classList.toggle('is-lit', on); }
        var dimTimers = [];
        function restart() {
            dimTimers.forEach(clearTimeout); dimTimers = [];
            head = segs.length ? segs[0].start : 0; segIdx = 0; segT = 0; phase = 'run'; phaseT = 0;
            cells.forEach(function (c) { c.keep = 0; c.target = 0; });
            nodes.forEach(function (n, i) { lightNode(i, i === 0); });
        }
        var ease = function (t) { return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2; };   // ease in and out
        var smooth = function (e0, e1, x) { var t = Math.max(0, Math.min(1, (x - e0) / (e1 - e0))); return t * t * (3 - 2 * t); };

        var lerp = function (a, b, t) { return a + (b - a) * t; };
        function honey(h) { return 'rgb(255,' + Math.round(lerp(236, 172, h)) + ',' + Math.round(lerp(176, 46, h)) + ')'; }

        function step(dt) {
            if (pointer) around(pointer.x, pointer.y, HEAT_R, function (c) {
                var d = Math.hypot(c.x - pointer.x, c.y - pointer.y);
                if (d < HEAT_R) { var v = Math.pow(1 - d / HEAT_R, 1.6) * 0.95; if (v > c.heat) c.heat = v; }
            });
            ripples = ripples.filter(function (rp) { return (rp.t += dt) < rp.life; });
            ripples.forEach(function (rp) {
                var rad = rp.t * rp.speed, fade = 1 - rp.t / rp.life;
                around(rp.x, rp.y, rad + 26 * S, function (c) {
                    var d = Math.abs(Math.hypot(c.x - rp.x, c.y - rp.y) - rad);
                    if (d < 26 * S) { var v = smooth(26 * S, 0, d) * rp.str * fade; if (v > c.heat) c.heat = v; }
                });
            });
            if (!segs.length) return;
            if (phase === 'run') {
                var sg = segs[segIdx], from = Math.floor(head);
                segT += dt;
                var u = Math.min(1, segT / sg.dur);
                head = sg.start + ease(u) * (sg.end - sg.start);
                for (var i = Math.max(sg.start, from); i <= Math.min(Math.floor(head), sg.end); i++) if (path[i].cell) path[i].cell.target = KEEP;
                if (u >= 1) {
                    var C = centers[segIdx + 1];
                    lightNode(segIdx + 1, true);
                    ripples.push({ x: C.x, y: C.y, t: 0, speed: 210 * S, life: 1.2, str: 0.5 });
                    phase = 'dwell'; phaseT = 0;
                }
            } else if (phase === 'dwell') {
                phaseT += dt;
                if (phaseT > 0.55) { segIdx++; segT = 0; if (segIdx >= segs.length) { phase = 'hold'; phaseT = 0; } else { phase = 'run'; head = segs[segIdx].start; } }
            } else if (phase === 'hold') {
                phaseT += dt;
                if (phaseT > 2.4) {
                    phase = 'fade'; phaseT = 0;
                    cells.forEach(function (c) { c.target = 0; });
                    nodes.forEach(function (n, k) { dimTimers.push(setTimeout(function () { lightNode(k, false); }, k * 120)); });
                }
            } else if (phase === 'fade') {
                phaseT += dt;
                if (phaseT > 1.5) restart();
            }
            // lit cells approach their level smoothly (in while the thread passes, out when the chain fades)
            var rate = 1 - Math.exp(-dt * (phase === 'fade' ? 3.2 : 5));
            var decay = Math.pow(0.18, dt);
            for (var k = 0; k < cells.length; k++) {
                var c = cells[k];
                if (c.keep !== c.target) { c.keep += (c.target - c.keep) * rate; if (Math.abs(c.target - c.keep) < 0.004) c.keep = c.target; }
                if (c.heat) { c.heat *= decay; if (c.heat < 0.01) c.heat = 0; }
            }
        }

        function headPos() {
            if (!path.length) return null;
            var i = Math.min(Math.floor(head), path.length - 1), j = Math.min(i + 1, path.length - 1), f = head - Math.floor(head);
            return { x: lerp(path[i].x, path[j].x, f), y: lerp(path[i].y, path[j].y, f) };
        }

        // the drawn part of the thread: finished legs in full, the current leg up to the head
        function thread(hp) {
            for (var k = 0; k < segs.length; k++) {
                var sg = segs[k], done = k < segIdx || (k === segIdx && phase !== 'run'), current = k === segIdx && phase === 'run';
                if (!done && !current) break;
                var e = done ? sg.end : Math.floor(head);
                if (e <= sg.start && !current) continue;
                ctx.beginPath(); ctx.moveTo(path[sg.start].x, path[sg.start].y);
                for (var i = sg.start + 1; i <= e; i++) ctx.lineTo(path[i].x, path[i].y);
                if (current && hp) ctx.lineTo(hp.x, hp.y);
                ctx.stroke();
            }
        }
        // an arrowhead where a finished leg meets the next medallion
        function arrows(alpha) {
            ctx.fillStyle = 'rgba(240, 150, 30, ' + alpha + ')';
            for (var k = 0; k < segs.length; k++) {
                var sg = segs[k];
                if (!(k < segIdx || (k === segIdx && phase !== 'run'))) break;
                var p = path[sg.end], q = path[Math.max(sg.start, sg.end - 4)], a = Math.atan2(p.y - q.y, p.x - q.x);
                ctx.save(); ctx.translate(p.x, p.y); ctx.rotate(a);
                ctx.scale(LW, LW); ctx.beginPath(); ctx.moveTo(2, 0); ctx.lineTo(-9, -6); ctx.lineTo(-6, 0); ctx.lineTo(-9, 6); ctx.closePath(); ctx.fill();
                ctx.restore();
            }
        }
        // a short bright tail behind the head, fading towards the back
        function tail(hp) {
            var i0 = Math.floor(head), from = Math.max(segs[segIdx].start, i0 - 26);
            ctx.lineCap = 'round';
            for (var i = from; i < i0; i++) {
                var f = (i - from + 1) / (i0 - from + 1);
                ctx.strokeStyle = 'rgba(255, 244, 205, ' + (0.85 * f * f).toFixed(3) + ')'; ctx.lineWidth = (2 + 3 * f) * LW;
                ctx.beginPath(); ctx.moveTo(path[i].x, path[i].y); ctx.lineTo(i + 1 <= i0 ? path[i + 1].x : hp.x, i + 1 <= i0 ? path[i + 1].y : hp.y); ctx.stroke();
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
                if (hp) { var d = Math.hypot(c.x - hp.x, c.y - hp.y); if (d < HEAD_R) { var v = smooth(HEAD_R, 0, d) * 0.95; if (v > h) h = v; } }
                if (c.calm && h > CALM) h = CALM;
                if (h < 0.03) continue;
                ctx.beginPath(); hexPath(ctx, c.x, c.y, R - GAP);
                ctx.globalAlpha = Math.min(1, 0.18 + h * 0.82); ctx.fillStyle = honey(h); ctx.fill();
                ctx.globalAlpha = Math.min(1, h + 0.1); ctx.strokeStyle = 'rgba(255, 190, 70, 0.9)'; ctx.lineWidth = 1.2; ctx.stroke();
            }
            ctx.globalAlpha = 1;
            // the thread: a soft wide glow under a bright core (no shadowBlur, which is costly per frame); it fades with the chain
            var threadAlpha = phase === 'fade' ? Math.max(0, 1 - phaseT / 0.8) : 1;
            if (threadAlpha > 0 && segs.length) {
                ctx.save(); ctx.globalAlpha = threadAlpha; ctx.lineCap = 'round'; ctx.lineJoin = 'round';
                ctx.strokeStyle = 'rgba(255, 176, 32, 0.26)'; ctx.lineWidth = 12 * LW; thread(hp);
                ctx.strokeStyle = 'rgba(255, 168, 36, 0.95)'; ctx.lineWidth = 3.5 * LW; thread(hp);
                arrows(0.95);
                if (hp) tail(hp);
                ctx.restore();
            }
            if (hp) {
                ctx.save(); ctx.globalCompositeOperation = 'lighter';
                var g = ctx.createRadialGradient(hp.x, hp.y, 0, hp.x, hp.y, 52 * S);
                g.addColorStop(0, 'rgba(255, 240, 190, 1)'); g.addColorStop(0.22, 'rgba(255, 200, 80, 0.85)'); g.addColorStop(1, 'rgba(255, 170, 40, 0)');
                ctx.fillStyle = g; ctx.beginPath(); ctx.arc(hp.x, hp.y, 52 * S, 0, Math.PI * 2); ctx.fill(); ctx.restore();
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
            path.forEach(function (p) { if (p.cell) { p.cell.keep = KEEP; p.cell.target = KEEP; } });
            nodes.forEach(function (n, i) { lightNode(i, true); });
            phase = 'hold'; segIdx = segs.length; head = path.length - 1;
            draw(0);
        }

        var last = 0, running = false, visible = true, frameCost = 0;
        if (!window.heroCombStats) window.heroCombStats = function () { return { maxFrameMs: Math.round(frameCost * 10) / 10 }; };
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
            ripples.push({ x: e.clientX - r.left, y: e.clientY - r.top, t: 0, speed: 520 * S, life: 1.4, str: 0.8 });
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
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-hero]'), mount);
})();
