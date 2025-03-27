$(document).ready(function () {
    $('#specie').select2();
    $('#regione').prop('disabled', 'disabled');
    $('#provincia').prop('disabled', 'disabled');
    $.ajax({
        type: "GET",
        url: baseUrlNazioni,
        data: "{}",
        success: function (data) {
            var s = '<option value="">Selezione Nazione</option>';
            for (var i = 0; i < data.length; i++) {
                s += '<option value="' + data[i].codicenazione + '">' + data[i].descrizionenazione + '</option>';
            }
            $("#nazione").html(s);

        }
    });
});



$('#nazione').on("change", function () {
    $('#regione').val('0');
    $('#provincia').val('0');
    var country = $('#nazione').val();
    if (country != "IT") {
        /*  $('#regione')
              .empty()
              .append('<option selected="selected" value="99"> NON DEFINITA</option>')
              ;
  */
        $('#regione').val('99');
        $('#provincia')
            .empty()
            .append('<option selected="selected" value="999"> NON DEFINITA</option>')
            ;

        $('#regione').prop('disabled', 'disabled');

        $('#provincia').prop('disabled', 'disabled');
    } else {
        $("#regione").removeAttr("disabled");

        $.ajax({
            type: "GET",
            url: baseUrlRegioni,
            data: "{}",
            success: function (data) {
                var s = '<option value="-1">Seleziona Regione</option>';
                for (var i = 0; i < data.length; i++) {
                    s += '<option value="' + data[i].codiceregione + '">' + data[i].descrizioneregione + '</option>';
                }
                $("#regione").html(s);
            }
        });

    }
});
$('#regione').on("change", function () {
    var codregionesel = $('#regione :selected').val();
    $("#provincia").removeAttr("disabled");
    $('#provincia').val('0');
    $.ajax({
        type: "GET",
        //                url: "getProvincie?codiceregione=" + codregionesel,
        url: baseUrlProvince,
        data: {
            'codiceregione': codregionesel
        },
        success: function (data) {
            var s = '<option value="-1">Selezione Provincia</option>';
            for (var i = 0; i < data.length; i++) {
                s += '<option value="' + data[i].codiceprovincia + '">' + data[i].descrizioneprovincia + '</option>';
            }
            $("#provincia").html(s);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error during process: \n' + xhr.responseText);
        }
    });

});
$('#provincia').on("change", function () {
    var codprovinciasel = $('#provincia :selected').val();

    $.ajax({
        type: "GET",
        //                url: "getProvincie?codiceregione=" + codregionesel,
        url: baseUrlProvince,
        data: {
            'codiceprovincia': codprovinciasel
        },
        success: function (data) {
            var s = '<option value="-1">Selezione Comune</option>';
            for (var i = 0; i < data.length; i++) {
                s += '<option value="' + data[i].codicecomune + '">' + data[i].descrizionecomune + '</option>';
            }
            $("#comune").html(s);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error during process: \n' + xhr.responseText);
        }
    });

});
$('#famiglia').on("change", function () {
    var codfamigliasel = $('#famiglia :selected').val();
    $('#genere').val('0');
    // $('#specie').val('0');
    $('#addlink2').show();
    if (codfamigliasel != "") {
        $.ajax({
            type: "GET",
            //                url: "getProvincie?codiceregione=" + codregionesel,
            url: baseUrlGenere,
            data: {
                'codicefamiglia': codfamigliasel
            },
            success: function (data) {
                $("#genere").disabled = false;
                var s = '<option value="-1">Selezione Genere</option>';
                for (var i = 0; i < data.length; i++) {
                    s += '<option value="' + data[i].codicegenere + '">' + data[i].descrizionegenere + '</option>';
                }
                $("#genere").removeAttr("disabled");
                $("#genere").html(s);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Error during process: \n' + xhr.responseText);
            }
        });


    } else {
        var g = '<option value="-1">Selezione Genere</option>';
        $("#genere").html(g);
        $("#genere").attr("disabled", "disabled");
        var s = '<option value="-1">Selezione Specie</option>';
        $("#specie").html(s);
        $("#specie").attr("disabled", "disabled");

    }
});
$('#genere').on("change", function () {
    $('#addGenere').hide();
    $('#addSpecie').hide();
    $('#addlink2').show();
    $('#addlink').show();
    var codgeneresel = $('#genere :selected').val();
    if (codgeneresel != "-1") {
        $('#specie').val('0');
        $('#addlink').show();
        $.ajax({
            type: "GET",
            url: baseUrlSpecie,
            data: {
                'codicegenere': codgeneresel
            },
            success: function (data) {
                $("#specie").disabled = false;
                var s = '<option value="-1">Selezione Specie</option>';
                for (var i = 0; i < data.length; i++) {
                    s += '<option value="' + data[i].codicespecie + '">' + data[i].nomescientifico + '</option>';
                }
                $("#specie").removeAttr("disabled");
                $("#specie").html(s);

            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Error during process: \n' + xhr.responseText);
            }
        });
    } else {//nel caso torno indietro e riazzero il genere devo riazzerare anche la specie
        var s = '<option value="-1">Selezione Specie</option>';
        $("#specie").html(s);
    }
});
$('#specie').on("change", function () {
    $('#addSpecie').hide();
    $('#addlink').show();
});
$('#addlink').click(function () {

    if ($('#addSpecie').css('display') == 'none') {
        //riazzera valori form
        $('#specieNavigation_nome').val("");
        $('#specieNavigation_autori').val("");
        $('#specieNavigation_nome_volgare').val("");
        $('#specieNavigation_nome_comune').val("");
        $('#specieNavigation_nome_comune_en').val("");
        $('#specieNavigation_subspecie').val("");
        $('#specieNavigation_autorisub').val("");
        $('#specieNavigation_varieta').val("");
        $('#specieNavigation_autorivar').val("");
        $('#specieNavigation_cult').val("");
        $('#specieNavigation_autoricult').val("");
        $('#regno').val("343b8a46-5dfa-4127-9ab4-29eb0d1d1ad5");
        $('#areale').val("473a0041-c4ee-406f-892e-1728997f45fa");
        $('#specieNavigation_genereNavigation_descrizione').val("");

        $('#addSpecie').show(); //div form inserimento specie
        $('#addGenere').hide();
        $('#addlink').hide();
        $('#addlink2').show();
        $('#specie').val('0');
    } else {
        $('#addSpecie').hide();

    }

});
$('#addlink2').click(function () {

    if ($('#addGenere').css('display') == 'none') {
        //riazzera valori form
        $('#specieNavigation_nome').val("");
        $('#specieNavigation_autori').val("");
        $('#specieNavigation_nome_volgare').val("");
        $('#specieNavigation_nome_comune').val("");
        $('#specieNavigation_nome_comune_en').val("");
        $('#specieNavigation_subspecie').val("");
        $('#specieNavigation_autorisub').val("");
        $('#specieNavigation_varieta').val("");
        $('#specieNavigation_autorivar').val("");
        $('#specieNavigation_cult').val("");
        $('#specieNavigation_autoricult').val("");
        $('#regno').val("343b8a46-5dfa-4127-9ab4-29eb0d1d1ad5");
        $('#areale').val("473a0041-c4ee-406f-892e-1728997f45fa");
        $('#specieNavigation_genereNavigation_descrizione').val("");
        // document.getElementById('specieNavigation_regno').value = "343b8a465dfa41279ab429eb0d1d1ad5";
        //  document.getElementById('specieNavigation_regno').value = "343b8a465dfa41279ab429eb0d1d1ad5";


        $('#addSpecie').hide();// div form inserimento specie
        $('#addGenere').show(); //div form inserimento genere
        $('#addlink2').hide();
        $('#addlink').show();
    } else {
        $('#addGenere').hide();
        $("#genere").attr("disabled", "disabled");
        $("#famiglia").attr("disabled", "disabled");
    }

});
$('#insSpecie').click(function () {
    if (document.getElementById('specieNavigation_nome').value == "") {
        alert("Campo Nome Specie Obbligatorio");
    } else {
        var nomecomune = "";
        var nomecomuneen = "";
        if ($('#specieNavigation_subspecie').val().length > 0) {

            var val4 = " subsp. " + $('#specieNavigation_subspecie').val().trim();
            if ($('#specieNavigation_autorisub').val().length > 0) {
                val4 = val4 + " " + $('#specieNavigation_autorisub').val().trim();
            }
        } else {
            var val4 = "";
        }
        if ($('#specieNavigation_varieta').val().length > 0) {

            var val6 = " var. " + $('#specieNavigation_varieta').val().trim() + " " + $('#specieNavigation_autorivar').val().trim();
        } else {
            var val6 = "";
        }
        if ($('#specieNavigation_cult').val().length > 0) {

            var val8 = " '" + $('#specieNavigation_cult').val().trim() + "' " + $('#specieNavigation_autoricult').val().trim();
        } else {
            var val8 = "";
        }
        if ($('#specieNavigation_nome_comune').val().length > 0) {
            nomecomune = $('#specieNavigation_nome_comune').val();
        } else {
            nomecomune = '';
        }
        if ($('#specieNavigation_nome_comune_en').val().length > 0) {
            nomecomuneen = $('#specieNavigation_nome_comune_en').val();
        } else {
            nomecomuneen = '';
        }
        $.ajax({
            type: "GET",
            url: baseUrlAddSpecie,
            dataType: 'json',

            data: {
                'genere': $('#genere :selected').val(),
                'nome': $('#specieNavigation_nome').val().trim(),
                'nome_scientifico': $('#genere :selected').text() + " " + $('#specieNavigation_nome').val().trim() + " " + $('#specieNavigation_autori').val().trim() + val4 + val6 + val8,//devo metterci la descrizione e non id devo correggere
                'nome_volgare': $('#specieNavigation_nome_volgare').val(),
                'nome_comune': nomecomune,
                'nome_comune_en': nomecomuneen,
                'autori': $('#specieNavigation_autori').val().trim(),
                'subspecie': $('#specieNavigation_subspecie').val().trim(),
                'autorisub': $('#specieNavigation_autorisub').val().trim(),
                'varieta': $('#specieNavigation_varieta').val().trim(),
                'autorivar': $('#specieNavigation_autorivar').val().trim(),
                'cult': $('#specieNavigation_cult').val().trim(),
                'autoricult': $('#specieNavigation_autoricult').val().trim(),
                'regno': $('#regno').val(),
                'areale': $('#areale').val()

            },

            success: function (data) {

                if (data.errore == false) {


                    var sel = document.getElementById('specie');

                    var opt = document.createElement('option');

                    opt.appendChild(document.createTextNode(data.specie_nome));

                    opt.value = data.specie_id;

                    sel.appendChild(opt);

                    $('#specie').val(data.specie_id);

                    $('#addSpecie').hide();
                    //riazzera valori form
                    $('#specieNavigation_nome').val("");
                    $('#specieNavigation_autori').val("");
                    $('#specieNavigation_nome_volgare').val("");
                    $('#specieNavigation_nome_comune').val("");
                    $('#specieNavigation_nome_comune_en').val("");
                    $('#specieNavigation_subspecie').val("");
                    $('#specieNavigation_autorisub').val("");
                    $('#specieNavigation_varieta').val("");
                    $('#specieNavigation_autorivar').val("");
                    $('#specieNavigation_cult').val("");
                    $('#specieNavigation_autoricult').val("");
                    $('#regno').val("343b8a46-5dfa-4127-9ab4-29eb0d1d1ad5");
                    $('#areale').val("473a0041-c4ee-406f-892e-1728997f45fa");
                    $('#specieNavigation_genereNavigation_descrizione').val("");
                    //$("#genere").attr("disabled", "disabled");
                    //$("#famiglia").attr("disabled", "disabled");
                    //$('#addlink').hide();
                    alert(data.message);
                    console.log("Specie Inserita con successo");
                }

                else {
                    alert(data.message);
                }

            },
            error: function () {
                alert(data.message);
                console.log('Qualcosa è andato storto!');
            }
        });
    }
});
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

$('#insGenere').click(function () {

    if (document.getElementById('specieNavigation_genereNavigation_descrizione').value == "") {
        alert("Campo Descrizione Obbligatorio");
    } else {
        $.ajax({
            type: "GET",
            url: baseUrlAddGenere,
            dataType: 'json',

            data: {
                'famiglia': $('#famiglia :selected').val(),
                'descrizione': $('#specieNavigation_genereNavigation_descrizione').val()
            },

            success: function (data) {

                if (data.erroreG == false) {

                    var sel = document.getElementById('genere');
                    var opt = document.createElement('option');
                    opt.appendChild(document.createTextNode(data.genere_descrizione));
                    opt.value = data.genere_id;
                    sel.appendChild(opt);

                    $('#genere').val(data.genere_id);

                    //$('#generespan').hide();
                    //$('#labelgenere').html(data.genere_descrizione);
                    // $('#labelgenere').show();
                    $('#addGenere').hide();
                    $('#specieNavigation_genereNavigation_descrizione').val("");
                    $('#addlink').show();
                    $('#addlink2').hide();
                    var codgeneresel = data.genere_id;
                    console.log(data);
                    console.log("Send success");

                    //ricarico specie
                    $.ajax({
                        type: "GET",
                        url: baseUrlSpecie,
                        data: {
                            'codicegenere': codgeneresel
                        },
                        success: function (data) {
                            $("#specie").disabled = false;
                            var s = '<option value="-1">Selezione Specie</option>';
                            for (var i = 0; i < data.length; i++) {
                                s += '<option value="' + data[i].codicespecie + '">' + data[i].nomespecie + '</option>';
                            }
                            $("#specie").removeAttr("disabled");
                            $("#specie").html(s);
                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            alert('Error during process: \n' + xhr.responseText);
                        }
                    });
                    alert(data.messageG);
                } else {

                    alert(data.messageG);
                }

            },
            error: function (data) {
                alert(data.message);
                console.log(data);
                console.log('Qualcosa è andato storto!');
            }
        });
    }
});