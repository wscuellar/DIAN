
function DeleteOperationMode(url) {
    $(".delete-software").click(function () {
        showLoading('#table-modes', 'Cargando', 'Buscando datos, por favor espere.');
        var metod = 'POST';
        var data = {
            Id: $(this).attr("data-id")
        }
        var actionError = () => { }
        var actionSuccess = (response) => {
            showConfirmation(response.message, AlertExec(()=>location.reload()))
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
}

function AddOperationMode(url, SetOperationViewModel) {
        var metod = 'POST';
        var data = SetOperationViewModel;
        var actionError = (error) => {
            var message = error.Message;
            var button = AlertExec();
            showConfirmation(message, button);
        }
        var actionSuccess = (response) => {
            var message = response.Message;
            var operation = () => { location.reload() };
            var button = AlertExec(operation);
            showConfirmation(message, button);
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess);
}

function RenderAutocomplete(url, contributorId, contributorTypeId, softwareType) {

    if (softwareType == "1") {
        var metod = "POST";
        var data = {
            term: "",
            contributorId,
            contributorTypeId,
            softwareType
        };
        var actionError = () => { };
        var actionSuccess = (response) => {
            var newUrl = "/RadianApproved/SoftwareList";
            var newData = {
                radianContributorId: response[0].value
            };
            var newAction = (response) => {
                $("#SoftwareNameList").append($("<option>", { value: response[0].value, text: response[0].text }));
            }
            $("#CustomerName").val(response[0].text);
            ajaxFunction(newUrl, metod, newData, actionError, newAction);
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess);
    } else {
        $("#CustomerName").val("");
        $("#SoftwareNameList option").remove();
        $("#CustomerName").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: url,
                    datatype: "json",
                    data: {
                        term: request.term,
                        contributorId,
                        contributorTypeId,
                        softwareType
                    },
                    success: function (data) {
                            response($.map(data, function (val, item) {
                                return {
                                    label: val.text,
                                    value: val.text,
                                    customerId: val.value
                                }
                            }))
                    }
                })
            },
            select: function (event, ui) {
                if (softwareType == "1") {
                    $("#CustomerName").val(ui.item.customerId);
                    $("#CustomerID").val(ui.item.customerId);
                } else {
                    $("#CustomerID").val(ui.item.customerId);
                }
                LoadSoftwareList(ui.item.customerId);
            }
        });
    }

}

function ChangeSelected() {
    $("#OperationModeSelected").change(function (value) {
        $("#SoftwareSelectedId").val(value);
    })
}

function LoadSoftwareList(radianId) {
    var url = "/RadianApproved/SoftwareList";
    var metod = "POST";
    var data = {
        radianContributorId: radianId
    }
    var actionError = () => { };
    var actionSuccess = (response) => {
        for (var i = 0; i < response.length; i++) {
            $("#SoftwareNameList").append($("<option>", { value: response[i].value, text: response[i].text }));
        }
    }
    ajaxFunction(url, metod, data, actionError, actionSuccess);
}
