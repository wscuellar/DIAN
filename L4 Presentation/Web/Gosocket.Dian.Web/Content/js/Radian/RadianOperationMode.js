
function DeleteOperationMode(url) {
    $("#delete-software").click(function () {
        var metod = 'POST';
        var data = {
            Id: $(this).attr("data-id")
        }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
}

function AddOperationMode(url, contributorId, radianTypeId, softwareId) {
        var metod = 'POST';
        var data = {
            ContributorId: contributorId,
            RadianTypeId: radianTypeId,
            softwareId: softwareId
        }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess);
}

function RenderAutocomplete(url, contributorId, contributorTypeId, softwareType) {
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

function ChangeSelected() {
    $("#OperationModeSelected").change(function (value) {
        $("#SoftwareSelectedId").val(value);
    })
}

function LoadSoftwareList(radianId) {debugger
    var url = "/RadianApproved/SoftwareList";
    var metod = "POST";
    var data = {
        radianContributorId: radianId
    }
    var actionError = () => { };
    var actionSuccess = (response) => {
        debugger
        for (var i = 0; i < response.length; i++) {
            $("#SoftwareNameList").append($("<option>", { value: response[i].value, text: response[i].text }));
        }
    }
    ajaxFunction(url, metod, data, actionError, actionSuccess);
}
