$('#settore').on("change", function () {
    var codsettoresel = $('#settore :selected').val();
    $('#collezione').val('0');
    $("#collezione").removeAttr("disabled");
    $.ajax({
        type: "GET",
        //                url: "getProvincie?codiceregione=" + codregionesel,
        url: baseUrlCollezioni,
        data: {
            'codicesettore': codsettoresel
        },
        success: function (data) {

            var s = '';
            for (var i = 0; i < data.length; i++) {
                s += '<option value="' + data[i].codicecollezione + '">' + data[i].descrizionecollezione + '</option>';
            }
            if (i == 0) {
                alert("Il settore non ha collezioni corrispondenti verificare e correggi");
                $('#settore').focusin();

            }
            $("#collezione").html(s);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error during process: \n' + xhr.responseText);
        }
    });

});
