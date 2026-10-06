/* Client-side number rules that accept exactly what the server accepts (D-53, Services/DecimalInput.cs):
   a comma or a dot as decimal separator in every language ("17,2" and "17.2" are 17.2); when both appear the last
   one is the decimal separator and the other groups thousands ("1.234,5", "1,234.5"); a single separator is always
   decimal ("1.234" is 1.234); the same separator repeated groups thousands ("1.234.567").
   Replaces jquery-validation's number, range, min and max rules, which only understand "17.2".
   Also gives every validated field a message slot: a field without its own asp-validation-for span would otherwise
   block the form without saying why. (D-66) */
(function ($) {
    'use strict';
    if (!$ || !$.validator) return;

    function grouped(s, sep) {
        var body = s.charAt(0) === '-' || s.charAt(0) === '+' ? s.substring(1) : s;
        var parts = body.split(sep);
        if (parts.length === 1) return parts[0].length > 0 && /^\d+$/.test(parts[0]);
        if (parts[0].length < 1 || parts[0].length > 3) return false;
        for (var i = 0; i < parts.length; i++) {
            if (!/^\d*$/.test(parts[i]) || (i > 0 && parts[i].length !== 3)) return false;
        }
        return true;
    }

    // same steps as DecimalInput.TryParse; returns NaN when the server would refuse the text
    function parse(text) {
        if (text === null || text === undefined) return NaN;
        var s = String(text).replace(/[\s  ']/g, '');
        if (!s.length) return NaN;
        var lastDot = s.lastIndexOf('.'), lastComma = s.lastIndexOf(','), normalized;
        if (lastDot >= 0 && lastComma >= 0) {
            var dec = lastDot > lastComma ? '.' : ',', group = dec === '.' ? ',' : '.';
            var at = s.lastIndexOf(dec), whole = s.substring(0, at);
            if (s.indexOf(dec) !== at || !grouped(whole, group)) return NaN;
            normalized = whole.split(group).join('') + '.' + s.substring(at + 1);
        } else if (lastDot >= 0 || lastComma >= 0) {
            var sep = lastDot >= 0 ? '.' : ',', count = s.split(sep).length - 1;
            if (count === 1) normalized = s.replace(sep, '.');
            else if (grouped(s, sep)) normalized = s.split(sep).join('');
            else return NaN;
        } else {
            normalized = s;
        }
        // AllowLeadingSign | AllowDecimalPoint: digits with at most one point, ".5" and "5." included
        if (!/^[+-]?(\d+\.?\d*|\.\d+)$/.test(normalized)) return NaN;
        return parseFloat(normalized);
    }

    function num(value) { return typeof value === 'number' ? value : parse(value); }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || !isNaN(parse(value));
    };
    $.validator.methods.range = function (value, element, param) {
        var v = parse(value);
        return this.optional(element) || (!isNaN(v) && v >= num(param[0]) && v <= num(param[1]));
    };
    $.validator.methods.min = function (value, element, param) {
        var v = parse(value);
        return this.optional(element) || (!isNaN(v) && v >= num(param));
    };
    $.validator.methods.max = function (value, element, param) {
        var v = parse(value);
        return this.optional(element) || (!isNaN(v) && v <= num(param));
    };

    window.T4hDecimal = { parse: parse };

    // a message slot after each validated field that has none (unobtrusive looks the slot up when the field fails)
    function addMessageSlots(root) {
        $(root).find('input[data-val="true"], select[data-val="true"], textarea[data-val="true"]').not('[type=hidden]').each(function () {
            var name = this.name;
            if (!name || !this.form) return;
            var exists = $(this.form).find('[data-valmsg-for]').filter(function () { return this.getAttribute('data-valmsg-for') === name; }).length;
            if (exists) return;
            var slot = document.createElement('span');
            slot.className = 'field-validation-valid text-danger small d-block';
            slot.setAttribute('data-valmsg-for', name);
            slot.setAttribute('data-valmsg-replace', 'true');
            var anchor = $(this).closest('.input-group').length ? $(this).closest('.input-group')[0] : this;
            anchor.insertAdjacentElement('afterend', slot);
        });
    }
    $(function () { addMessageSlots(document); });
})(window.jQuery);
