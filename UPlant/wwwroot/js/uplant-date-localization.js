(function (window) {
    'use strict';

    var culture = (document.documentElement.lang || navigator.language || 'it-IT').toLowerCase();
    var isEnglish = culture.indexOf('en') === 0;

    function pad(value) {
        value = String(value);
        return value.length === 1 ? '0' + value : value;
    }

    window.uplantDateFormat = isEnglish ? 'mm/dd/yy' : 'dd/mm/yy';
    window.uplantDatepickerLanguage = isEnglish ? 'en' : 'it';
    window.uplantDatepickerTexts = isEnglish ? {
        closeText: 'Done',
        currentText: 'Today',
        monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
        monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
        dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
        dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
        weekHeader: 'Wk',
        firstDay: 0
    } : {
        closeText: 'Chiudi',
        currentText: 'Oggi',
        monthNames: ['gennaio', 'febbraio', 'marzo', 'aprile', 'maggio', 'giugno', 'luglio', 'agosto', 'settembre', 'ottobre', 'novembre', 'dicembre'],
        monthNamesShort: ['gen', 'feb', 'mar', 'apr', 'mag', 'giu', 'lug', 'ago', 'set', 'ott', 'nov', 'dic'],
        dayNames: ['domenica', 'lunedì', 'martedì', 'mercoledì', 'giovedì', 'venerdì', 'sabato'],
        dayNamesShort: ['dom', 'lun', 'mar', 'mer', 'gio', 'ven', 'sab'],
        dayNamesMin: ['do', 'lu', 'ma', 'me', 'gi', 've', 'sa'],
        weekHeader: 'Sm',
        firstDay: 1
    };

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

    if ($.datepicker && $.datepicker.setDefaults) {
        $.datepicker.setDefaults($.extend({}, window.uplantDatepickerTexts || {}, {
            dateFormat: window.uplantDateFormat
        }));
    }

    if ($.fn.datepicker.defaults) {
        $.extend($.fn.datepicker.defaults, {
            language: window.uplantDatepickerLanguage,
            format: window.uplantDateFormat.replace('yy', 'yyyy'),
            autoclose: true,
            todayHighlight: true,
            showOn: 'both',
            buttonText: window.uplantDatepickerLanguage === 'en' ? 'Open calendar' : 'Apri calendario'
        });
    }

    function getTimeSuffix(value) {
        var match = String(value || '').match(/\s+(\d{1,2}:\d{2}(?:\s*[AP]M)?)\s*$/i);
        return match ? match[1] : '';
    }

    function buildOptions($input) {
        var dateFormat = $input.data('date-format') || window.uplantDateFormat;
        var texts = window.uplantDatepickerTexts || {};

        return $.extend({}, texts, {
            dateFormat: dateFormat,
            format: dateFormat.replace('yy', 'yyyy'),
            language: window.uplantDatepickerLanguage,
            autoclose: true,
            todayHighlight: true,
            showOn: 'both',
            buttonText: window.uplantDatepickerLanguage === 'en' ? 'Open calendar' : 'Apri calendario',
            onSelect: function (dateText) {
                if (!$input.data('preserve-time')) {
                    return;
                }

                var time = getTimeSuffix($input.val());
                $input.val(time ? dateText + ' ' + time : dateText);
            }
        });
    }

    $(function () {
        $('.datepicker, .datetimepicker').each(function () {
            var $input = $(this);
            $input.datepicker(buildOptions($input));
        });
    });
})(window.jQuery, window);
