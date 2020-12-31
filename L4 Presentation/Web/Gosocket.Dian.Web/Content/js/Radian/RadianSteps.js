
//var paramsObject = {
//    element: "",
//    data: [],
//    form: "",
//    urlSearch: "",
//    radianId: 0,
//    page: 0,
//    tableRendered: "",
//    customersTotalCount: 0,
//    columns: [],
//    ajaxData: {}
//}

function RenderSteps(index) {
    var validator;
    $("#steps-approved").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        autoFocus: true,
        startIndex: index,
        enablePagination: false,
        enableKeyNavigation: false,
        onInit: () => {
            var tooltip = $('.add-tooltip');
            if (tooltip.length) tooltip.tooltip();
        }
    });
    //hideLoading();
    $(".radian-file").click(function () {
        $(this).val("");
        $(this).parents(".custom-file").children("label").html("");
    });
    $(".radian-file").change(function (file) {
        validator && validator.destroy();
        var isValid = true;
        var id = $(this).attr('name');
        var form = $(this).parents("form");
        var files = form[0];
        var messages = new Object();
        var actualFileObj = file.target.files[0];
        var actualFileSize = Math.round(actualFileObj.size / 10000) / 100;
        var fileObj = "";
        var fileSize = "";
        for (var i = 0; i < files.length; i++) {
            if (files[i].type == "file" && files[i].id == "") {
                fileObj = files[i].files[0];
                fileSize = fileObj ? Math.round(fileObj.size / 10000) / 100 : 0;
                if (fileObj && fileSize > 10) {
                    $($("input[name='" + files[i].name + "']")[0]).val("");
                    $($("input[name='" + files[i].name + "']")[0]).parents(".custom-file").children("label").html("");
                    if (id == files[i].name)
                        isValid = false;

                    messages = Object.assign(messages, {
                        [files[i].name]: {
                            required: "Tamaño máximo 10 Mb."
                        }
                    });
                }
                else if (fileObj && fileObj.type != "application/pdf") {
                    $($("input[name='" + files[i].name + "']")[0]).val("");
                    $($("input[name='" + files[i].name + "']")[0]).parents(".custom-file").children("label").html("");
                    if (id == files[i].name)
                        isValid = false;

                    messages = Object.assign(messages, {
                        [files[i].name]: {
                            required: "Solo documentos .PDF"
                        }
                    });
                } else {
                    messages = Object.assign(messages, {
                        [files[i].name]: {
                            required: "Archivo requerido"
                        }
                    });
                }
            }
        }
        if (isValid) {
            $(this).parent().children().html(actualFileObj.name + "  (" + actualFileSize + " Mb)");
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").toggle();
            $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").children("input").attr("value", actualFileObj.name);
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").children(".file-size").html("(" + actualFileSize + " Mb)");
        }

        validator = form.validate({
            messages: messages
        });
        $(form).valid();
    });

    $(".close").click(function () {
        $(this).parent().toggle();
        $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();
        $(this).parents(".inputs-dinamics").children(".file-input-enabled").children(".custom-file").children("input").val("");
        $(this).parents(".inputs-dinamics").children(".file-input-enabled").children(".custom-file").children("label").html("");

    })
}

function RenderTable(paramsObject) {
    var totalPages = Math.trunc(paramsObject.customersTotalCount / 10) + 1;
    paramsObject.tableRendered && paramsObject.tableRendered.destroy();
    paramsObject.tableRendered = $(paramsObject.element).DataTable({
        paging: false,
        info: false,
        data: paramsObject.data,
        columns: paramsObject.columns,
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
    $(paramsObject.element + "_filter > label").hide();
    paramsObject.ajaxData.Page && $(paramsObject.element + "_wrapper").append("<div><span class='counter-data'>Mostrando " + paramsObject.ajaxData.Page + " de " + totalPages + " páginas</span>" + TablePagination(paramsObject.ajaxData.Page, paramsObject.customersTotalCount, paramsObject.data.length));
    $(paramsObject.element + "_filter").append(paramsObject.form);
    LoadEventsToSearch(paramsObject);
    LoadEventsToPagiantion(paramsObject);
    changeToSpanish();
}

function LoadEventsToSearch(paramsObject) {
    $(".search-data").click(function (e) {
        e.preventDefault();
        SearchData(paramsObject);
    })
}

function SearchData(paramsObject) {
    const cloneAjaxData = Object.assign({}, paramsObject.ajaxData);
    var arrayToMap = Object.entries(paramsObject.ajaxData);
    arrayToMap.forEach((element) => {
        if (typeof element[1] === 'string' && element[1].includes("#")) {
            paramsObject.ajaxData[element[0]] = $(element[1]).val();
        }
    });
    var actionError = () => { }
    var actionSuccess = (response) => {
        paramsObject.data = response.Customers;
        paramsObject.ajaxData = cloneAjaxData;
        RenderTable(paramsObject);
    }
    ajaxFunction(paramsObject.urlSearch, 'POST', paramsObject.ajaxData, actionError, actionSuccess);
}

function LoadEventsToPagiantion(paramsObject) {
    $(".next-page").click(function () {
        paramsObject.page = paramsObject.page + 1;
        paramsObject.ajaxData.Page = paramsObject.page
        SearchData(paramsObject);
    });
    $(".prev-page").click(function () {
        paramsObject.page = paramsObject.page - 1;
        paramsObject.ajaxData.Page = paramsObject.page
        SearchData(paramsObject);
    });
}

function TablePagination(page, totalCount, countPage) {
    var disabledNext = (page * 10) >= totalCount ? 'disabled="disabled"' : "";
    var disabledPrev = page == 1 ? 'disabled="disabled"' : "";
    var min = (page - 1) * 10 + 1;
    var max = 10 > countPage ? (page - 1) * 10 + countPage : (page) * 10;
    var html = String(min) != 'NaN' ? '<div class="pagination-controls pull-right"><span class="text-muted">\
                <strong>'+ min + '-' + max + '</strong >\
                </span >\
                <div class="btn-group btn-group margin-left-5" style="padding-right: 20px;">\
                <a class="btn btn-default paginate-btn prev-page" '+ disabledPrev + '>\
                        <span class="fa fa-chevron-left"></span>\
                    </a>\
                <a class="btn btn-default paginate-btn next-page" '+ disabledNext + '>\
                <span class="fa fa-chevron-right"></span>\
                    </a >\
                </div></div>' : "";
    return html;
}

function changeToSpanish() {
    $('.input-daterange').datepicker({
        language: "es"
    });
    $(".input-daterange input").change(function () {
        $(".datepicker-dropdown").css("display", "none");
    });
}

function cancelRegister(cancelData) {
    var url = cancelData.url;
    var confirmationMessage = bootboxMessage.CONFIRMATION_MESSAGE;
    var successAction = () => {
        var message = bootboxMessage.CANCEL_RESPONSE_CORRECT;
        var operation = () => window.location.href = cancelData.href
        showConfirmation(message, AlertExec(operation), "cancel-confirmation", operation);
    };
    var dataAjax = {
        id: cancelData.id,
        radianContributorTypeId: cancelData.type,
        radianState: cancelData.state
    };
    var label = bootboxMessage.CANCEL_DESCRIPTION;
    CancelRegister(url, dataAjax, confirmationMessage, successAction, label);
}
