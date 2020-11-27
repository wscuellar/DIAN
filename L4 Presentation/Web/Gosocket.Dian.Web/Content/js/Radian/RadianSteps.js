
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
    });
    $(".radian-file").change(function (file) {
        var form = $("#uploadForm");
        var messages = new Object();
        var id = $(this).attr('name');
        var fileObj = file.target.files[0];
        var fileSize = Math.round(fileObj.size / 10000) / 100;
        if (fileSize > 10) {
            $(this).val("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Tamaño máximo 10 Mb."
                }
            });
        }
        else if (fileObj.type != "application/pdf") {
            $(this).val("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Solo documentos .PDF"
                }
            });
        } else {
            $(this).parent().children().html(fileObj.name + "  (" + fileSize + " Mb)");
            $(this).parents(".inputs-dinamics").children(".file-input-disabled").toggle();
            $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();
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

function RenderTable(element, data, form, urlSearch, radianId, tableRendered) {
    tableRendered && tableRendered.destroy();
    var table = $(element).DataTable({
        data: data,
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
    $("#table-customers_filter > label").hide();
    $("#table-customers_filter").append(form);
    LoadEventsToSearch(urlSearch, radianId, form, table);
}

function LoadEventsToSearch(url, radianContributorId, form, table) {
    $("#search-customers").click(function (e) {
        e.preventDefault();
        var nit = $("#NitSearch").val();
        var state = $("#RadianStateSelect").val();
        var data = {
            radianContributorId,
            code: nit,
            radianState: state,
            page: 1,
            pagesize: 10
        };
        var dataTable = [
            [
                "Tiger Nixon",
                "System Architect",
                "Edinburgh"
            ],
            [
                "Garrett Winters",
                "Director",
                "Edinburgh"
            ]
        ];
        var actionError = () => {}
        var actionSuccess = (response) => {
            RenderTable('#table-customers', dataTable, form, url, radianContributorId, table)
        }
        ajaxFunction(url, 'POST', data, actionError, actionSuccess);
       
    })
}