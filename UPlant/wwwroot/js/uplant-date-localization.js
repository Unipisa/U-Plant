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

    var culture = (document.documentElement.lang || navigator.language || 'it-IT').toLowerCase();
    var isEnglish = culture.indexOf('en') === 0;

    var regionalOptions = isEnglish ? {
        closeText: 'Done',
        currentText: 'Today',
        prevText: 'Previous',
        nextText: 'Next',
        monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
        monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
        firstDay: 0
    } : {
        closeText: 'Chiudi',
        currentText: 'Oggi',
        prevText: 'Precedente',
        nextText: 'Successivo',
        monthNames: ['Gennaio', 'Febbraio', 'Marzo', 'Aprile', 'Maggio', 'Giugno', 'Luglio', 'Agosto', 'Settembre', 'Ottobre', 'Novembre', 'Dicembre'],
        monthNamesShort: ['Gen', 'Feb', 'Mar', 'Apr', 'Mag', 'Giu', 'Lug', 'Ago', 'Set', 'Ott', 'Nov', 'Dic'],
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Me', 'Gi', 'Ve', 'Sa'],
        firstDay: 1
    };

    function keepDatepickerAboveMaps(instance) {
        window.setTimeout(function () {
            instance.dpDiv.css('z-index', 3000);
        }, 0);
    }

    function getOptions($input) {
        return $.extend({}, regionalOptions, {
            dateFormat: $input.data('date-format') || window.uplantDateFormat,
            changeMonth: true,
            changeYear: true,
            showButtonPanel: true,
            yearRange: '1900:+10',
            autoclose: true,
            beforeShow: function (input, instance) {
                keepDatepickerAboveMaps(instance);
            },
            onChangeMonthYear: function (year, month, instance) {
                keepDatepickerAboveMaps(instance);
            }
        });
    }

    $(function () {
        $('.datepicker').each(function () {
            var $input = $(this);
            var options = getOptions($input);

            if ($input.data('datepicker')) {
                $input.datepicker('option', options);
                return;
            }

            $input.datepicker(options);
        });
    });
})(window.jQuery, window);
