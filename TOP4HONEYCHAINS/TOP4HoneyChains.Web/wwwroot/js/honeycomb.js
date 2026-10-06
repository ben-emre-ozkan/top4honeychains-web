/* Honeycomb canvas of the landing hero and the sign-in panel (D-65).
   A faint hex grid; cells light up one after another along a path through five stations
   (apiary → delivery → lab → processing → shelf), with a few drifting pollen motes.
   <canvas data-honeycomb data-theme="light|dark" data-speed="1" data-labels="a|b|c|d|e" data-path="x,y x,y …" data-path-wide="x,y …">
   (path points are fractions of the canvas; data-path-wide is used when the canvas is at least 1000 px wide)
   - draws only while the canvas is on screen and the tab is visible;
   - with prefers-reduced-motion (or data-speed="0") it draws one static frame with the whole path lit.
   Decorative only (aria-hidden); the page states the same content in text. */
(function () {
    'use strict';

    var SQ3 = Math.sqrt(3);
    var reduceQuery = window.matchMedia ? window.matchMedia('(prefers-reduced-motion: reduce)') : null;

    function themeColors(theme) {
        return theme === 'dark'
            ? { grid: 'rgba(255, 201, 92, 0.10)', fill: '255, 179, 61', station: '#F2B33D', stationInk: '#15120D', label: 'rgba(246, 238, 223, 0.92)', labelBg: 'rgba(21, 18, 13, 0.72)', mote: '255, 214, 130', glow: 18 }
            : { grid: 'rgba(217, 147, 13, 0.17)', fill: '242, 179, 61', station: '#D9930D', stationInk: '#FFFFFF', label: '#3B352C', labelBg: 'rgba(251, 248, 242, 0.92)', mote: '217, 147, 13', glow: 0 };
    }

    // pointy-top axial hex helpers
    function toPixel(q, r, size) { return { x: size * SQ3 * (q + r / 2), y: size * 1.5 * r }; }
    function toHex(x, y, size) {
        var q = (SQ3 / 3 * x - y / 3) / size, r = (2 / 3 * y) / size;
        var s = -q - r, rq = Math.round(q), rr = Math.round(r), rs = Math.round(s);
        var dq = Math.abs(rq - q), dr = Math.abs(rr - r), ds = Math.abs(rs - s);
        if (dq > dr && dq > ds) rq = -rr - rs; else if (dr > ds) rr = -rq - rs;
        return { q: rq, r: rr };
    }
    function hexPath(ctx, x, y, size) {
        ctx.beginPath();
        for (var i = 0; i < 6; i++) {
            var a = Math.PI / 180 * (60 * i - 30);
            var px = x + size * Math.cos(a), py = y + size * Math.sin(a);
            if (i === 0) ctx.moveTo(px, py); else ctx.lineTo(px, py);
        }
        ctx.closePath();
    }

    function Honeycomb(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.colors = themeColors(canvas.getAttribute('data-theme'));
        this.speed = parseFloat(canvas.getAttribute('data-speed') || '1');
        this.labels = (canvas.getAttribute('data-labels') || '').split('|').filter(function (s) { return s.length; });
        function parse(text) {
            return text.split(' ').map(function (p) { var xy = p.split(','); return { x: parseFloat(xy[0]), y: parseFloat(xy[1]) }; });
        }
        this.pathNarrow = parse(canvas.getAttribute('data-path') || '0.08,0.70 0.30,0.40 0.50,0.66 0.72,0.36 0.92,0.62');
        this.pathWide = canvas.getAttribute('data-path-wide') ? parse(canvas.getAttribute('data-path-wide')) : this.pathNarrow;
        this.visible = true;
        this.running = false;
        this.head = 0;
        this.last = 0;
        this.hold = 0;
        this.motes = [];
        this.resize();
        this.bind();
    }

    Honeycomb.prototype.staticMode = function () {
        return this.speed <= 0 || (reduceQuery && reduceQuery.matches);
    };

    Honeycomb.prototype.resize = function () {
        var rect = this.canvas.getBoundingClientRect();
        var dpr = Math.min(window.devicePixelRatio || 1, 2);
        this.w = Math.max(1, rect.width);
        this.h = Math.max(1, rect.height);
        this.canvas.width = Math.round(this.w * dpr);
        this.canvas.height = Math.round(this.h * dpr);
        this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        this.size = Math.max(16, Math.min(32, this.w / 34));
        this.points = this.w >= 1000 ? this.pathWide : this.pathNarrow;
        this.buildPath();
        this.buildGrid(dpr);
        this.buildMotes();
        this.draw(0);
    };

    // the cells under the polyline through the stations, in order
    Honeycomb.prototype.buildPath = function () {
        var self = this, cells = [], seen = {}, stations = [];
        var pts = this.points.map(function (p) { return { x: p.x * self.w, y: p.y * self.h }; });
        function add(x, y) {
            var h = toHex(x, y, self.size), key = h.q + ',' + h.r;
            if (!seen[key]) { seen[key] = cells.length; cells.push(h); }
            return seen[key];
        }
        for (var i = 0; i < pts.length; i++) {
            if (i > 0) {
                var a = pts[i - 1], b = pts[i], len = Math.hypot(b.x - a.x, b.y - a.y), steps = Math.max(1, Math.ceil(len / (self.size * 0.5)));
                for (var s = 1; s < steps; s++) add(a.x + (b.x - a.x) * s / steps, a.y + (b.y - a.y) * s / steps);
            }
            stations.push(add(pts[i].x, pts[i].y));
        }
        this.cells = cells.map(function (c) { var p = toPixel(c.q, c.r, self.size); return { x: p.x, y: p.y, lit: 0 }; });
        this.stations = stations;
    };

    // the faint grid, drawn once per size into an offscreen canvas and faded towards the edges
    Honeycomb.prototype.buildGrid = function (dpr) {
        var g = document.createElement('canvas');
        g.width = this.canvas.width; g.height = this.canvas.height;
        var c = g.getContext('2d');
        c.setTransform(dpr, 0, 0, dpr, 0, 0);
        c.strokeStyle = this.colors.grid;
        c.lineWidth = 1;
        var size = this.size, rMax = Math.ceil(this.h / (size * 1.5)) + 1, qMax = Math.ceil(this.w / (size * SQ3)) + 2;
        for (var r = -1; r <= rMax; r++) {
            for (var q = -Math.ceil(r / 2) - 1; q <= qMax - Math.floor(r / 2); q++) {
                var p = toPixel(q, r, size);
                hexPath(c, p.x, p.y, size * 0.94);
                c.stroke();
            }
        }
        c.globalCompositeOperation = 'destination-in';
        // the grid is strongest around the path
        var cx = 0, cy = 0;
        this.points.forEach(function (p) { cx += p.x / this.points.length; cy += p.y / this.points.length; }, this);
        var fade = c.createRadialGradient(this.w * cx, this.h * cy, 0, this.w * cx, this.h * cy, Math.max(this.w, this.h) * 0.75);
        fade.addColorStop(0, 'rgba(0,0,0,1)');
        fade.addColorStop(0.6, 'rgba(0,0,0,0.55)');
        fade.addColorStop(1, 'rgba(0,0,0,0)');
        c.fillStyle = fade;
        c.fillRect(0, 0, this.w, this.h);
        this.grid = g;
    };

    Honeycomb.prototype.buildMotes = function () {
        var n = Math.round(Math.min(26, this.w / 48));
        this.motes = [];
        for (var i = 0; i < n; i++) {
            this.motes.push({
                x: Math.random() * this.w, y: Math.random() * this.h, r: 1 + Math.random() * 2.2,
                vx: 4 + Math.random() * 8, vy: -(3 + Math.random() * 7), phase: Math.random() * Math.PI * 2, a: 0.25 + Math.random() * 0.5
            });
        }
    };

    Honeycomb.prototype.step = function (dt) {
        var cells = this.cells, n = cells.length, rate = 7 * this.speed;   // cells per second
        if (this.hold > 0) {
            this.hold -= dt;
            if (this.hold <= 0) { this.head = 0; }
        } else {
            this.head += rate * dt;
            if (this.head >= n + 2) { this.hold = 2.2; }
        }
        for (var i = 0; i < n; i++) {
            var c = cells[i], isStation = this.stations.indexOf(i) >= 0;
            var reached = this.head >= i;
            if (this.hold > 0 && this.hold < 1.2) c.lit = Math.max(0, c.lit - dt * 1.4);       // fade out before the next round
            else if (reached && this.head - i < 1.2) c.lit = 1;
            else if (reached) c.lit = Math.max(isStation ? 0.85 : 0.32, c.lit - dt * 0.9);  // the trail stays faintly lit
        }
        for (var m = 0; m < this.motes.length; m++) {
            var mo = this.motes[m];
            mo.phase += dt * 0.8;
            mo.x += (mo.vx + Math.sin(mo.phase) * 6) * dt * this.speed;
            mo.y += mo.vy * dt * this.speed;
            if (mo.x > this.w + 10) mo.x = -10;
            if (mo.y < -10) { mo.y = this.h + 10; mo.x = Math.random() * this.w; }
        }
    };

    Honeycomb.prototype.draw = function () {
        var ctx = this.ctx, col = this.colors, size = this.size, i;
        ctx.clearRect(0, 0, this.w, this.h);
        if (this.grid) ctx.drawImage(this.grid, 0, 0, this.w, this.h);
        var still = this.staticMode();
        for (i = 0; i < this.cells.length; i++) {
            var c = this.cells[i], lit = still ? 0.55 : c.lit;
            if (lit <= 0.01) continue;
            hexPath(ctx, c.x, c.y, size * 0.9);
            ctx.fillStyle = 'rgba(' + col.fill + ',' + (0.12 + lit * 0.55).toFixed(3) + ')';
            if (col.glow) { ctx.shadowColor = 'rgba(' + col.fill + ',0.8)'; ctx.shadowBlur = col.glow * lit; }
            ctx.fill();
            ctx.shadowBlur = 0;
        }
        var showLabels = this.labels.length === this.stations.length && this.w >= 640;
        ctx.font = '600 13px "Instrument Sans", system-ui, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        for (i = 0; i < this.stations.length; i++) {
            var st = this.cells[this.stations[i]], on = still || st.lit > 0.5;
            hexPath(ctx, st.x, st.y, size * 0.9);
            ctx.fillStyle = on ? col.station : 'rgba(' + col.fill + ',0.25)';
            if (on && col.glow) { ctx.shadowColor = col.station; ctx.shadowBlur = col.glow * 1.4; }
            ctx.fill();
            ctx.shadowBlur = 0;
            hexPath(ctx, st.x, st.y, size * 0.42);
            ctx.fillStyle = on ? col.stationInk : 'rgba(255,255,255,0.6)';
            ctx.fill();
            if (showLabels && on) {
                var text = this.labels[i], tw = ctx.measureText(text).width + 16, ty = st.y - size * 1.75;
                ctx.fillStyle = col.labelBg;
                ctx.beginPath();
                if (ctx.roundRect) ctx.roundRect(st.x - tw / 2, ty - 12, tw, 24, 12); else ctx.rect(st.x - tw / 2, ty - 12, tw, 24);
                ctx.fill();
                ctx.fillStyle = col.label;
                ctx.fillText(text, st.x, ty + 0.5);
            }
        }
        for (i = 0; i < this.motes.length; i++) {
            var mo = this.motes[i], g = ctx.createRadialGradient(mo.x, mo.y, 0, mo.x, mo.y, mo.r * 4);
            g.addColorStop(0, 'rgba(' + col.mote + ',' + mo.a + ')');
            g.addColorStop(1, 'rgba(' + col.mote + ',0)');
            ctx.fillStyle = g;
            ctx.beginPath();
            ctx.arc(mo.x, mo.y, mo.r * 4, 0, Math.PI * 2);
            ctx.fill();
        }
    };

    Honeycomb.prototype.loop = function (t) {
        if (!this.running) return;
        var dt = this.last ? Math.min(0.05, (t - this.last) / 1000) : 0;
        this.last = t;
        this.step(dt);
        this.draw();
        var self = this;
        this.frame = requestAnimationFrame(function (tt) { self.loop(tt); });
    };

    Honeycomb.prototype.update = function () {
        var should = this.visible && !document.hidden && !this.staticMode();
        if (should && !this.running) {
            this.running = true;
            this.last = 0;
            var self = this;
            this.frame = requestAnimationFrame(function (t) { self.loop(t); });
        } else if (!should && this.running) {
            this.running = false;
            cancelAnimationFrame(this.frame);
        }
        if (!should) this.draw();
    };

    Honeycomb.prototype.bind = function () {
        var self = this;
        if ('IntersectionObserver' in window) {
            new IntersectionObserver(function (entries) {
                self.visible = entries[0].isIntersecting;
                self.update();
            }).observe(this.canvas);
        }
        document.addEventListener('visibilitychange', function () { self.update(); });
        if (reduceQuery && reduceQuery.addEventListener) reduceQuery.addEventListener('change', function () { self.update(); });
        var pending;
        var onResize = function () { clearTimeout(pending); pending = setTimeout(function () { self.resize(); self.update(); }, 120); };
        if ('ResizeObserver' in window) new ResizeObserver(onResize).observe(this.canvas); else window.addEventListener('resize', onResize);
        // labels use the web font: redraw once it is ready
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(function () { self.draw(); });
        this.update();
    };

    function init() {
        var nodes = document.querySelectorAll('canvas[data-honeycomb]');
        for (var i = 0; i < nodes.length; i++) {
            if (!nodes[i].getContext) continue;
            nodes[i].honeycomb = new Honeycomb(nodes[i]);
        }
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
})();
