// D-87: the consumer's own state on a public Honey Story. Asked only when the consumer marker cookie is present (anonymous
// visitors make no request); the answer is private and never cached, the page HTML stays the same for everyone.
(function () {
    var box = document.getElementById('consumer-actions');
    if (!box || document.cookie.split(';').every(function (c) { return c.trim().indexOf('t4h.consumer=') !== 0; })) return;
    fetch('/Consumer/State?qr=' + encodeURIComponent(box.getAttribute('data-qr')), { credentials: 'same-origin', cache: 'no-store', headers: { 'Accept': 'application/json' } })
        .then(function (r) { return r.ok && (r.headers.get('content-type') || '').indexOf('application/json') === 0 ? r.json() : null; })
        .then(function (s) {
            if (!s || !s.signedIn) return;
            var anon = document.getElementById('consumer-anon');
            if (anon) anon.hidden = true;
            var save = document.getElementById('consumer-save');
            if (save && s.saved) {
                save.textContent = save.getAttribute('data-saved-text');
                save.setAttribute('href', '/Consumer');
                save.classList.add('is-saved');
            }
        })
        .catch(function () { });
})();
