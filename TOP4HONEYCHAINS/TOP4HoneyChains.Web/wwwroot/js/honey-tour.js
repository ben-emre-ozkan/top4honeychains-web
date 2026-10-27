/* D-78 P4 (D78-SPEC 7.1, D-83): the first-use tour of a role. 3-5 steps, each a one-line caption next to a menu item of the role
   (a still highlight, no scene). Shown once per user and role (browser storage), skippable at any step (Skip, Esc), re-opened from
   the user menu ([data-tour-open]). Non-modal: the page stays usable, focus moves to the card only when nothing else has it. */
(function () {
    'use strict';
    var data = document.getElementById('hs-tour');
    if (!data) return;
    var steps;
    try { steps = JSON.parse(data.textContent || '[]'); } catch (e) { return; }
    var key = 'hs-tour:' + (data.getAttribute('data-key') || 'anonymous');
    var labels = { next: data.getAttribute('data-next'), back: data.getAttribute('data-back'), skip: data.getAttribute('data-skip'), done: data.getAttribute('data-done'), title: data.getAttribute('data-title'), of: data.getAttribute('data-of') };
    var card = null, index = 0, marked = null, visible = [];

    function seen() { try { return window.localStorage.getItem(key) === '1'; } catch (e) { return true; } }
    function remember() { try { window.localStorage.setItem(key, '1'); } catch (e) { /* private mode */ } }

    function shown(el) { return !!(el && el.getClientRects().length && window.getComputedStyle(el).visibility !== 'hidden'); }

    function close() {
        remember();
        if (marked) marked.classList.remove('tour-target');
        marked = null;
        if (card) card.remove();
        card = null;
        document.removeEventListener('keydown', onKey, true);
    }

    function onKey(e) { if (e.key === 'Escape') { close(); } }

    function render() {
        var step = visible[index];
        var target = step.el;
        if (marked) marked.classList.remove('tour-target');
        marked = target && shown(target) ? target : null;
        if (marked) marked.classList.add('tour-target');
        card.querySelector('.tour-text').textContent = step.text;
        card.querySelector('.tour-count').textContent = labels.of.replace('{0}', index + 1).replace('{1}', visible.length);
        card.querySelector('[data-tour-back]').hidden = index === 0;
        card.querySelector('[data-tour-next]').textContent = index === visible.length - 1 ? labels.done : labels.next;
        // place it under the item (or centred at the top when the item is in a collapsed menu)
        if (marked) {
            var r = marked.getBoundingClientRect();
            var left = Math.max(12, Math.min(r.left + window.scrollX, window.scrollX + document.documentElement.clientWidth - card.offsetWidth - 12));
            card.style.top = (r.bottom + window.scrollY + 10) + 'px';
            card.style.left = left + 'px';
            card.classList.remove('is-floating');
        } else {
            card.style.top = (window.scrollY + 72) + 'px';
            card.style.left = '';
            card.classList.add('is-floating');
        }
    }

    function open() {
        // a step whose item is not on the page (a menu entry the organization does not have) is left out; an item hidden in a
        // collapsed phone menu keeps its step, shown as a floating card
        visible = steps.map(function (s) { return { el: document.querySelector('[data-tour="' + s.target + '"]'), text: s.text }; })
            .filter(function (s) { return !!s.el; });
        if (!visible.length) return;
        if (card) close();
        index = 0;
        card = document.createElement('div');
        card.className = 'tour-card';
        card.setAttribute('role', 'dialog');
        card.setAttribute('aria-modal', 'false');
        card.setAttribute('aria-labelledby', 'tour-title');
        card.innerHTML = '<p class="tour-head"><strong id="tour-title"></strong> <span class="tour-count"></span></p><p class="tour-text" aria-live="polite"></p>' +
            '<div class="tour-actions"><button type="button" class="tour-skip" data-tour-skip></button>' +
            '<span><button type="button" class="tour-btn tour-ghost" data-tour-back></button> <button type="button" class="tour-btn" data-tour-next></button></span></div>';
        card.querySelector('#tour-title').textContent = labels.title;
        card.querySelector('[data-tour-skip]').textContent = labels.skip;
        card.querySelector('[data-tour-back]').textContent = labels.back;
        card.addEventListener('click', function (e) {
            if (e.target.closest('[data-tour-skip]')) close();
            else if (e.target.closest('[data-tour-back]')) { index = Math.max(0, index - 1); render(); }
            else if (e.target.closest('[data-tour-next]')) { if (index >= visible.length - 1) close(); else { index++; render(); } }
        });
        document.body.appendChild(card);
        document.addEventListener('keydown', onKey, true);
        render();
        var active = document.activeElement;
        if (!active || active === document.body || !active.matches('input, select, textarea, [contenteditable]'))
            card.querySelector('[data-tour-next]').focus({ preventScroll: true });
    }

    document.addEventListener('click', function (e) {
        if (e.target.closest && e.target.closest('[data-tour-open]')) { e.preventDefault(); open(); }
    });
    window.addEventListener('resize', function () { if (card) render(); });

    // the first time a user meets the role (after the notice gate: the gate pages carry no tour)
    if (!seen()) {
        if (document.readyState === 'complete') window.setTimeout(open, 300);
        else window.addEventListener('load', function () { window.setTimeout(open, 300); });
    }
})();
