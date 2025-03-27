﻿$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: baseUrlGetAccNoAuth,
        //data: "{}",
        success: function (data) {
            var t = data.recordsTotal;
            var a = data.recordAcc;
            var i = data.recordInd;
            var iok = data.recordIndOk;

            $("#avvisitotali").text(a);
            if (t > 0) {
                $("#avvisitotali").removeClass('label-default').addClass('label-warning')
            }
            $("#avvisitotali2").text(t);
            $("#avvisiacc").text(a);
            $("#avvisiind").text(i);
            $("#avvisiindok").text(iok);
           
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error during process: \n' + xhr.responseText);
        }
    });

});
