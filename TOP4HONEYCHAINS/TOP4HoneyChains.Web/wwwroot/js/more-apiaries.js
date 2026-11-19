/* D-93: "Add apiary" on the registration form. Clones the template block, numbers the blocks (Apiary 2, 3, …) and keeps the
   field names MoreApiaries[0..n-1] without gaps, so the server binds every block; a block can be removed before saving. */
(function () {
    'use strict';
    var list = document.getElementById('more-apiaries');
    var add = document.getElementById('add-apiary');
    var template = document.getElementById('more-apiary-template');
    if (!list || !add || !template) return;

    var renumber = function () {
        list.querySelectorAll('.more-apiary').forEach(function (block, i) {
            block.setAttribute('data-index', i);
            block.querySelector('.more-apiary-n').textContent = String(i + 2);
            block.querySelectorAll('[name]').forEach(function (el) { el.name = el.name.replace(/MoreApiaries\[\d+\]/, 'MoreApiaries[' + i + ']'); });
            block.querySelectorAll('[id]').forEach(function (el) { el.id = el.id.replace(/MoreApiaries_\d+_/, 'MoreApiaries_' + i + '_'); });
            block.querySelectorAll('label[for]').forEach(function (el) { el.htmlFor = el.htmlFor.replace(/MoreApiaries_\d+_/, 'MoreApiaries_' + i + '_'); });
            var place = block.querySelector('.apiary-place');
            ['data-lat', 'data-lng'].forEach(function (k) { place.setAttribute(k, place.getAttribute(k).replace(/MoreApiaries_\d+_/, 'MoreApiaries_' + i + '_')); });
        });
    };

    list.addEventListener('click', function (e) {
        var button = e.target.closest('.remove-apiary');
        if (!button) return;
        button.closest('.more-apiary').remove();
        renumber();
        add.focus();
    });

    add.addEventListener('click', function () {
        var block = template.content.firstElementChild.cloneNode(true);
        list.appendChild(block);
        renumber();
        if (window.initApiaryPlace) window.initApiaryPlace(block.querySelector('.apiary-place'));
        block.querySelector('input').focus();
    });

    renumber();
})();
