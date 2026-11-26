/* D-98: "Other" in a honey type list (or the "Other" box of the beekeeper's honey types) shows the name and class of a new
   honey type; any other choice hides them. No animation. Included once per block; binds each control only once. */
(function () {
    'use strict';
    document.querySelectorAll('select.honey-type-select[data-new], input.honey-type-other-check[data-new]').forEach(function (control) {
        if (control.dataset.bound) return;
        var block = document.getElementById(control.dataset.new);
        if (!block) return;
        control.dataset.bound = '1';
        var update = function () {
            var on = control.tagName === 'SELECT' ? control.value === '-1' : control.checked;
            block.hidden = !on;
            if (on && control.tagName === 'SELECT') {
                var name = block.querySelector('input[type=text]');
                if (name && document.activeElement === control) name.focus();
            }
        };
        control.addEventListener('change', update);
        update();
    });
})();
