
function OnBeginAjax(response) {
    showLoading("#panel-forma", "Validando datos", "Por favor esperar");
}

function OnSuccessAjax(response) {
    hideLoading("#panel-forma");
    if (response.Code == 200) {
        window.location.href = response.Message;
    } else {
        if (response.Code == 400) {
            ErrorDialogV2("", response.Message || "Ocurrió un error por favor intente nuevamente.");
            return;
        }
        SuccessDialogV2("Acceso al sistema de facturación", response.Message, false);
        //var confirmMessage = response.Message;
        //showConfirmation(confirmMessage, buttons, null, closeEvent);
        //var closeEvent = () => { }
        //var buttons = AlertExecLoggin2();
    }
}

function OnFailureAjax(response) {
    hideLoading("#panel-forma");
    var confirmMessage = response.responseJSON.message;
    ErrorDialogV2("Notificación", confirmMessage);
    //var buttons = AlertExec();
    //showConfirmation(confirmMessage, buttons);
}


function AlertExecLoggin2() {
    return {
        del: {
            label: "Aceptar",
            className: "btn-radian-default",
            callback: function () {
                if (location.href.indexOf('tlogin=1') == -1) {
                    window.location.href = window.location.href + "?tlogin=1";
                }
                else {
                    window.location.reload();
                }                   
                operationClick = false;
            }
        }
    }
}
