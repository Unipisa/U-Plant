﻿// ******************************************************** //
//          Configurazione Globale di DataTables
//    Inizializzo alcune variabili/configurazioni globali
// ******************************************************** //

var DtLang = "/Assets/js/DataTables/i18n/Italian.json";
// Percorso al file di lingua
if (typeof (extDtLang) !== "undefined") {
    // ricevo la url corretta dal javascript della pagina
    DtLang = extDtLang;
}

jQuery.extend(true, $.fn.dataTable.defaults, {
    info: true,
    order: [],
    language: {
        "sEmptyTable": "Nessun dato presente nella tabella",
        "sInfo": "Vista da _START_ a _END_ di _TOTAL_ elementi",
        "sInfoEmpty": "Vista da 0 a 0 di 0 elementi",
        "sInfoFiltered": "(filtrati da _MAX_ elementi totali)",
        "sInfoPostFix": "",
        "sInfoThousands": ".",
        "sLengthMenu": "Visualizza _MENU_ elementi",
        "sLoadingRecords": "Caricamento...",
        "sProcessing": "Elaborazione...",
        "sSearch": "Cerca:",
        "sZeroRecords": "La ricerca non ha portato alcun risultato.",
        "oPaginate": {
            "sFirst": "Inizio",
            "sPrevious": "Precedente",
            "sNext": "Successivo",
            "sLast": "Fine"
        },
        "oAria": {
            "sSortAscending": ": attiva per ordinare la colonna in ordine crescente",
            "sSortDescending": ": attiva per ordinare la colonna in ordine decrescente"
        }
    }
    //pagingType: "input"
    //sPaginationType: "listbox"
});