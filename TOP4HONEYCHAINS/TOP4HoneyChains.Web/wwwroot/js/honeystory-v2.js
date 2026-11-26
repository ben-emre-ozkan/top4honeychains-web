/* D-108: the public Honey Story (Views/HoneyStory/Index.cshtml, wwwroot/css/honeystory-v2.css).
   - source chips: show the lab values of one source;
   - the value sheet: tapping a lab value opens a bottom sheet (a modal <dialog>: focus stays inside, Escape closes, focus returns
     to the value) with the value, its limit bar, the plain-language explanation and the parameter's video, played inline
     (playsinline, with controls; muted autoplay only when motion is allowed: no reduced motion and "Show animations" not off).
     Without the dialog element the link opens the parameter page as before. */
(function () {
    'use strict';

    var page = document.getElementById('honeystory');
    if (!page) return;

    // source chips
    Array.prototype.forEach.call(page.querySelectorAll('.hv-chip'), function (chip) {
        chip.addEventListener('click', function () {
            var src = chip.getAttribute('data-src');
            Array.prototype.forEach.call(page.querySelectorAll('.hv-chip'), function (c) {
                var on = c.getAttribute('data-src') === src;
                c.classList.toggle('is-on', on);
                c.setAttribute('aria-pressed', on ? 'true' : 'false');
            });
            Array.prototype.forEach.call(page.querySelectorAll('.hv-panel'), function (p) { p.classList.toggle('is-on', p.getAttribute('data-src') === src); });
        });
    });

    var sheet = document.getElementById('hv-sheet');
    if (!sheet || typeof sheet.showModal !== 'function') return;

    var head = sheet.querySelector('.hv-sheet-head');
    var measure = sheet.querySelector('.hv-sheet-measure');
    var text = sheet.querySelector('.hv-sheet-text');
    var more = sheet.querySelector('.hv-sheet-more');
    var videoBox = sheet.querySelector('.hv-sheet-video');
    var video = videoBox.querySelector('video');
    var note = videoBox.querySelector('.hv-sheet-vnote');
    var opener = null;
    var reduce = window.matchMedia ? window.matchMedia('(prefers-reduced-motion: reduce)') : { matches: false };

    function still() {
        return reduce.matches || document.documentElement.getAttribute('data-scenes') === 'off';
    }

    function stopVideo() {
        try { video.pause(); } catch (e) { }
        video.removeAttribute('src');
        try { video.load(); } catch (e) { }
    }

    function open(link) {
        var tpl = document.getElementById('hv-p-' + link.getAttribute('data-sheet'));
        if (!tpl) return false;
        opener = link;
        head.textContent = tpl.getAttribute('data-title') || '';
        measure.innerHTML = '';
        var m = link.querySelector('.hv-measure');
        if (m) measure.appendChild(m.cloneNode(true));
        text.innerHTML = '';
        text.appendChild(tpl.content.cloneNode(true));
        more.setAttribute('href', link.getAttribute('href'));

        var src = tpl.getAttribute('data-video');
        stopVideo();
        if (src) {
            videoBox.hidden = false;
            note.textContent = tpl.getAttribute('data-video-note') || '';
            note.hidden = !note.textContent;
            video.setAttribute('src', src);
            video.muted = true;
            video.setAttribute('preload', still() ? 'metadata' : 'auto');
        } else {
            videoBox.hidden = true;
        }

        sheet.classList.toggle('is-still', still());
        sheet.showModal();
        sheet.scrollTop = 0;
        if (src && !still()) {
            var played = video.play();
            if (played && played.catch) played.catch(function () { });   // autoplay refused: the controls stay
        }
        return true;
    }

    function close() {
        if (!sheet.open) return;
        stopVideo();
        if (still()) {
            sheet.close();
            return;
        }
        sheet.classList.add('is-closing');
        setTimeout(function () { sheet.classList.remove('is-closing'); sheet.close(); }, 180);
    }

    Array.prototype.forEach.call(page.querySelectorAll('.hv-val[data-sheet]'), function (link) {
        link.setAttribute('aria-haspopup', 'dialog');
        link.addEventListener('click', function (e) {
            if (e.ctrlKey || e.metaKey || e.shiftKey || e.button === 1) return;   // a new tab opens the parameter page
            if (open(link)) e.preventDefault();
        });
    });

    sheet.querySelector('[data-close]').addEventListener('click', close);
    // Escape: the dialog's cancel event; closed with our transition
    sheet.addEventListener('cancel', function (e) { e.preventDefault(); close(); });
    // a tap on the backdrop (outside the panel) closes
    sheet.addEventListener('click', function (e) { if (e.target === sheet) close(); });
    sheet.addEventListener('close', function () {
        stopVideo();
        if (opener) { opener.focus(); opener = null; }
    });
})();
