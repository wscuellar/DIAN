
//var paramsObject = {
//    element: "",
//    data: [],
//    form: "",
//    urlSearch: "",
//    radianId: 0,
//    page: 0,
//    tableRendered: "",
//    customersTotalCount: 0,
//    columns: []
//}

function RenderSteps(index) {
    $("#steps-approved").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        autoFocus: true,
        startIndex: index,
        enablePagination: false,
        enableKeyNavigation: false
    });

    $(".radian-file").click(function () {
        $(this).val("");
        $(this).parents(".custom-file").children("label").html("");
    });
    $(".radian-file").change(function (file) {
        var form = $(this).parents("form");
        var messages = new Object();
        var id = $(this).attr('name');
        var fileObj = file.target.files[0];
        var fileSize = Math.round(fileObj.size / 10000) / 100;
        if (fileSize > 10) {
            $(this).parents(".custom-file").children("label").html("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Tamaño máximo 10 Mb."
                }
            });
        }
        else if (fileObj.type != "application/pdf") {
            $(this).parents(".custom-file").children("label").html("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Solo documentos .PDF"
                }
            });
        } else {
            $(this).parent().children().html(fileObj.name + "  (" + fileSize + " Mb)");
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").toggle();
            $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();debugger
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").children("input").attr("value",fileObj.name);
            //$(this).parents(".inputs-dinamics").children(".file-input-enabled").children("input").val();
        }
        form.validate({
            messages: messages
        });
        form.valid();
    });

    $(".close").click(function () {
        $(this).parent().toggle();
        $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();
    })
}

function RenderTable(element, data, form, urlSearch, radianId, page, tableRendered, customersTotalCount, columns) {
    var totalPages = Math.round(customersTotalCount / 10) + 1 ;
    tableRendered && tableRendered.destroy();
    var table = $(element).DataTable({
        paging: false,
        info: false,
        data: data,
        columns: columns,
        language: {
            "lengthMenu": "Mostrar _MENU_ elementos por página",
            "zeroRecords": "No se encontraron datos",
            "info": "Mostrando página _PAGE_ de _PAGES_",
            "infoEmpty": "No hay datos",
            "infoFiltered": "(Filtrado de _MAX_ registros)",
            "search": "Nit Facturador",
            "paginate": {
                "next": ">",
                "previous": "<"
            }
        }
    });
    $(element+"_filter > label").hide();
    $(element + "_wrapper").append("<div><span>Mostrando 1 de " + totalPages + " páginas</span>" + TablePagination(page, customersTotalCount));
    $(element + "_filter").append(form);
    LoadEventsToSearch(urlSearch, radianId, form, page, table, customersTotalCount, columns);
    LoadEventsToPagiantion(element, data, form, urlSearch, radianId, page, table, customersTotalCount, columns);
    changeToSpanish();
}

function LoadEventsToSearch(url, radianContributorId, form, page, table, customersTotalCount, columns) {
    $("#search-customers").click(function (e) {
        e.preventDefault();
        var nit = $("#NitSearch").val();
        var state = $("#RadianStateSelect").val();
        var data = {
            radianContributorId,
            code: nit,
            radianState: state,
            page: page,
            pagesize: 10
        };
        var actionError = () => {}
        var actionSuccess = (response) => {
            RenderTable('#table-customers', response, form, url, radianContributorId, page, table, customersTotalCount, columns)
        }
        ajaxFunction(url, 'POST', data, actionError, actionSuccess); 
    })
}

function LoadEventsToPagiantion(element, data, form, urlSearch, radianId, page, table, customersTotalCount, columns) {
    $(".next-page").click(function () {
        var newPage = parseInt($("#PageTable").val()) + 1;
        $("#PageTable").val(newPage);
        var nit = $("#NitSearch").val();
        var state = $("#RadianStateSelect").val();
        var data = {
            radianContributorId: radianId,
            code: nit,
            radianState: state,
            page: newPage,
            pagesize: 10
        };
        var actionError = () => { }
        var actionSuccess = (response) => {
            RenderTable(element, response, form, urlSearch, radianId, newPage, table, customersTotalCount, columns)
        }
        ajaxFunction(urlSearch, 'POST', data, actionError, actionSuccess); 
    });
    $(".prev-page").click(function () {
        var newPage = parseInt($("#PageTable").val()) - 1;
        $("#PageTable").val(newPage);
        var nit = $("#NitSearch").val();
        var state = $("#RadianStateSelect").val();
        var data = {
            radianContributorId: radianId,
            code: nit,
            radianState: state,
            page: newPage,
            pagesize: 10
        };
        var actionError = () => { }
        var actionSuccess = (response) => {
            RenderTable(element, response, form, urlSearch, radianId, newPage, table, customersTotalCount, columns)
        }
        ajaxFunction(url, 'POST', data, actionError, actionSuccess); 
    });
}

function SearchCustomers() {
    var nit = $("#NitSearch").val();
    var state = $("#RadianStateSelect").val();
    var data = {
        radianContributorId: radianId,
        code: nit,
        radianState: state,
        page: newPage,
        pagesize: 10
    };
    var actionError = (error) => { showConfirmation(error.messages, AlertExec()); }
    var actionSuccess = (response) => {
        RenderTable(element, response, form, urlSearch, radianId, newPage, table)
    }
    ajaxFunction(url, 'POST', data, actionError, actionSuccess); 
}



function TablePagination(page, totalCount) {
    var disabledNext = (page * 10) >= totalCount ? 'disabled="disabled"' : ""; 
    var disabledPrev = page == 1 ? 'disabled="disabled"' : "";
    var html = '<div class="pagination-controls pull-right"><span class="text-muted">\
                <strong>1-1</strong >\
                </span >\
                <div class="btn-group btn-group margin-left-5" style="padding-right: 20px;">\
                <a class="btn btn-default paginate-btn prev-page" '+ disabledPrev +'>\
                        <span class="fa fa-chevron-left"></span>\
                    </a>\
                <a class="btn btn-default paginate-btn next-page" '+ disabledNext +'>\
                <span class="fa fa-chevron-right"></span>\
                    </a >\
                </div></div>'
    return html;
}

function changeToSpanish() {
    $('.input-daterange').datepicker({
        language: "es"
    });
    $(".input-daterange input").change(function(){
        $(".datepicker-dropdown").css("display","none");
    });
}