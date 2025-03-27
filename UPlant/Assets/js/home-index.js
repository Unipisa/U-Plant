    var baseApiUrl = "api/ADApi/";

        $(function () {
        ClearAllTables();
            FillAllTables();
            $("#btnSearch").click(function (e) {
        PerformSearch();
            });
        });

        $(document.body).on('click', '.sub-control', function (e) {
        SubControlFired(e);
        });

        function PerformSearch() {
            var text = $("#search").val();
            if (text != "") {
        ClearSearchTeachingsTable();
                $.ajax({
        url: baseApiUrl + "Search/" + text,
                    dataType: "json"
                })
                    .done(function (data) {
        $.each(data, function (key, d) {
            subscribeBoxText = MakeSubscribeBox(d.id, d.subscription);
            teachingText = MakeTeachingText(d.valutamiID, d.name);
            //$("#SearchTeachingsTable > tbody").append("<tr><td>" + d.courseName + "</td><td>" + teachingText + "</td><td>" + d.code + "</td><td>" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
            $("#SearchTeachingsTable > tbody").append("<tr><td>" + d.courseName + "</td><td>" + teachingText + "<br />" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.code + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
        });
                        $("#SearchTeachings").show();
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
        alert("Errore di comunicazione: " + errorThrown);
                    });


            }
        }
        function SubControlFired(e) {
        e.preventDefault();
            var chk = $("#" + e.target.id);
            //var teachingId = chk.data("id");
            var subscribeId = chk.data("sub");
            if (chk.is(":checked")) {
                // trying to subscribe
                if (subscribeId != "") {
        alert("Sei gia' iscritto a questo insegnamento");
                } else {
        Subscribe(chk);
                }
            } else {
                // trying to unsubscribe
                if (chk.data("sub") == "") {
        alert("Non risulti iscritto a questo insegnamento");
                } else {
        UnSubscribe(chk);
                }
            }
        }

        function Subscribe(chk) {
            var teachingId = chk.data("teaching");
            $.ajax({
        url: baseApiUrl + "Subscribe/" + teachingId,
                method: "POST",
                dataType: "json"
            })
                .done(function (data) {
                    if (data.success) {
        chk.data("sub", data.subscribeId);
                        chk.prop("checked", true);
                    } else {
        window.alert(data.errorMessage);
                    }
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
        alert("Errore di comunicazione: " + errorThrown);
                });
        }

        function UnSubscribe(chk) {
            var subId = chk.data("sub");
            $.ajax({
        url: baseApiUrl + "UnSubscribe/" + subId,
                method: "DELETE",
                dataType: "json"
            })
                .done(function (data) {
                    if (data.success) {
        chk.data("sub", "");
                        chk.prop("checked", false);
                    } else {
        window.alert(data.errorMessage);
                    }
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
        alert("Errore di comunicazione: " + errorThrown);
                });
        }

        function FillAllTables() {
        FillCurrentCourseTable();
            RefreshOtherTeachingsTable();
        }

        function FillCurrentCourseTable() {
        $.ajax({
            url: baseApiUrl + "MyCourses",
            dataType: "json"
        })
            .done(function (data) {
                $.each(data, function (key, d) {
                    subscribeBoxText = MakeSubscribeBox(d.id, d.subscription);
                    teachingText = MakeTeachingText(d.valutamiID, d.name);
                    //$("#CurrentCourseTable > tbody").append("<tr><td>" + teachingText + "</td><td>" + d.code + "</td><td>" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
                    $("#CurrentCourseTable > tbody").append("<tr><td>" + d.courseName + "</td><td>" + teachingText + "<br />" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.code + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
                });
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert("Errore di comunicazione: " + errorThrown);
            });
        }

        function RefreshOtherTeachingsTable() {
        ClearOtherTeachingsTable();
            $.ajax({
        url: baseApiUrl + "OtherCourses",
                dataType: "json"
            })
                .done(function (data) {
                    var n = 0;
                    $.each(data, function (key, d) {
        n = 1;
                        subscribeBoxText = MakeSubscribeBox(d.id, d.subscription);
                        teachingText = MakeTeachingText(d.valutamiID, d.name);
                        //$("#OtherTeachingsTable > tbody").append("<tr><td>" + d.courseName + "</td><td>" + teachingText + "</td><td>" + d.code + "</td><td>" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
                        $("#OtherTeachingsTable > tbody").append("<tr><td>" + d.courseName + "</td><td>" + teachingText + "<br />" + d.teacherName + " " + d.teacherSurname + "</td><td>" + d.code + "</td><td>" + d.cfu + "</td><td>" + subscribeBoxText + "</td></tr>");
                    });
                    if (n > 0) {
        $(".OtherTeachings").show();
                    }
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
        alert("Errore di comunicazione: " + errorThrown);
                });
        }

        function MakeTeachingText(id, name) {
            if (id == null)
                return name;
            return "<a href='https://esami.unipi.it/programma.php?c=" + id + "' target='_blank'>" + name + "</a>";
        }


        function MakeSubscribeBox(id, subId) {
        controlId = "sub_" + id;
            chkdStatus = ((subId == null) ? "" : " checked ");
            chkdData = ' data-sub="' + ((subId == null) ? '' : subId) + '" ';
            ret = '<div class="form-check form-switch sub-control"><input class="form-check-input" type="checkbox" data-teaching="' + id + '" id="' + controlId + '"' + chkdStatus + chkdData + '></div>';
            return ret;
        }

        function ClearAllTables() {
        ClearCurrentCourseTable();
            ClearSearchTeachingsTable();
        }

        function ClearCurrentCourseTable() {
        $("#CurrentCourseTable tbody").empty();
        }
        function ClearSearchTeachingsTable() {
        $("#SearchTeachings").hide();
            $("#SearchTeachingsTable tbody").empty();
        }
        function ClearOtherTeachingsTable() {
        $(".OtherTeachings").hide();
            $("#OtherTeachingsTable tbody").empty();
        }
