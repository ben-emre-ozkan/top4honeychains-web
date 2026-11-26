/* D-98: emptying a container into a batch (Views/Shared/Components/BatchContainers). A USB scanner types the seal's short address
   and presses Enter: Enter goes on to the "seal is intact" check instead of submitting the form before it is answered. */
(function () {
    'use strict';
    document.querySelectorAll('.emptying-form .em-seal').forEach(function (input) {
        input.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            var intact = input.form.querySelector('input[name=intact]');
            if (intact) intact.focus();
        });
    });
})();
