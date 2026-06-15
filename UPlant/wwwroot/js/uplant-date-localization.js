(function (window) {
    'use strict';

    var culture = (document.documentElement.lang || navigator.language || 'it-IT').toLowerCase();
    var isEnglish = culture.indexOf('en') === 0;

    function pad(value) {
        value = String(value);
        return value.length === 1 ? '0' + value : value;
    }

    window.uplantDateFormat = isEnglish ? 'mm/dd/yy' : 'dd/mm/yy';

    window.uplantFormatIsoDate = function (isoDate) {
        if (!isoDate) {
            return '';
        }

        var parts = isoDate.split('T')[0].split('-');
        if (parts.length !== 3) {
            return isoDate;
        }

        var year = parts[0];
        var month = pad(parts[1]);
        var day = pad(parts[2]);
        return isEnglish ? month + '/' + day + '/' + year : day + '/' + month + '/' + year;
    };
})(window);

(function ($, window) {
    'use strict';

    if (!$ || !$.fn || !$.fn.datepicker) {
        return;
    }

    $(function () {
        $('.datepicker').each(function () {
            var $input = $(this);
            if ($input.data('datepicker')) {
                return;
            }

            $input.datepicker({
                dateFormat: $input.data('date-format') || window.uplantDateFormat,
                autoclose: true
            });
        });
    });
})(window.jQuery, window);
