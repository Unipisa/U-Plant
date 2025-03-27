// ******************************************************** //
//                         Select2
//            Funzioni di formattazione elenchi
// ******************************************************** //
function formattoElenco(res, msg) {
    if (res.loading) {
        return res.text;
    }
    var Str = '';

    if (res.id) {
        Str += "<span>(" + res.id + ")</span> - <span>" + res.text + "</span>";
        if (res.disabled) {
            Str += " - <span>(" + msg + ")</span>";
        }
    }

    return Str;
}
function formattoSelezione(sel, msg = null) {

    var Str = '';

    if (sel.id) {
        Str += "<span>(" + sel.id + ")</span> - <span>" + sel.text + "</span>";
    } else {
        Str += "<span>" + sel.text + "</span>";
    }

    return Str;
}