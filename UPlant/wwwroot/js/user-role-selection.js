(function () {
    const operatorRadio = document.getElementById('role-operator');
    const readerRadio = document.getElementById('role-reader');
    const primaryRoleRadios = document.querySelectorAll('.js-primary-role');
    const supportTools = document.querySelectorAll('.js-support-tool');

    const toggleSupportTools = () => {
        const enableExtras = (operatorRadio && operatorRadio.checked) || (readerRadio && readerRadio.checked);

        supportTools.forEach(extra => {
            extra.disabled = !enableExtras;
            if (!enableExtras) {
                extra.checked = false;
            }
        });
    };

    primaryRoleRadios.forEach(radio => {
        radio.addEventListener('change', toggleSupportTools);
    });

    toggleSupportTools();
})();
