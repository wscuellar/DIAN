$(document).ready(function () {
    //var docSearch = new Bloodhound({
    //    identify: function (o) { return o.documentId; },
    //    queryTokenizer: Bloodhound.tokenizers.whitespace,
    //    datumTokenizer: Bloodhound.tokenizers.obj.whitespace("documentId"),
    //    dupDetector: function (a, b) { return a.documentId === b.documentId; },
    //    remote: {
    //        url: "/Inbox/Suggests/?term=%QUERY&length=3",
    //        wildcard: "%QUERY"
    //    }
    //});
    
    //$("#input-search*").typeahead(
    //    {
    //        hint: $(".typeahead-hint"),
    //        menu: $(".typeahead-menu"),
    //        minLength: 3,
    //        highlight: true,
    //        classNames: {
    //            open: "is-open",
    //            empty: "is-empty",
    //            cursor: "is-active",
    //            suggestion: "typeahead-suggestion",
    //            selectable: "typeahead-selectable"
    //        }
    //    },
    //    {
    //        source: docSearch,
    //        display: Handlebars.compile("{{documentTypeName}} - {{number}}"),
    //        limit: 10,
    //        templates: {
    //            header: "Documentos<hr/>",
    //            suggestion: Handlebars.compile("<div>{{documentTypeName}} - {{number}}</div>"),
    //            empty: Handlebars.compile("<div><i class=\"fa fa-search\"></i>Ver más resultados</div>")
    //        }
    //    }
    //)
    //.on("typeahead:asyncrequest", function () {
    //    $(".typeahead-spinner").show();
    //})
    //.on("typeahead:asynccancel typeahead:asyncreceive", function () {
    //    $(".typeahead-spinner").hide();
    //});
});