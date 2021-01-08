
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
        var operation = () => ajaxFunction(url, metod, data, actionError, actionSuccess);
        var message = "¿Está seguro de eliminar este ususario?"
        var cancelOperation = () => hideLoading('#table-modes');
        showConfirmation(message, ConfirmExec(operation, null, cancelOperation), null, cancelOperation);
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

    //if (true) {
        var metod = "POST";
        var data = {
            term: "",
            contributorId,
            contributorTypeId,
            softwareType
        };
        var actionError = (error) => {
            console.log(error);
        };
    var actionSuccess = (response) => {
                $("#SoftwareNameList").html("");
                $("#bussiness-name").html("");
                response.length == 0 && hideLoading('#panel-form');
                if (response.length) {
                    LoadSoftwareList(response[0].value);
                }
                for (var i = 0; i < response.length; i++) {
                    $("#bussiness-name").append($("<option>", { value: response[i].value, text: response[i].text }));
                }
            }
        ajaxFunction(url, metod, data, actionError, actionSuccess);

        $("#bussiness-name").change(function (element) {
            LoadSoftwareList(element);
        })

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
        hideLoading('#panel-form');
        $("#SoftwareNameList").html("");
        for (var i = 0; i < response.length; i++) {
            $("#SoftwareNameList").append($("<option>", { value: response[i].value, text: response[i].text }));
        }
    }
    ajaxFunction(url, metod, data, actionError, actionSuccess);
}
