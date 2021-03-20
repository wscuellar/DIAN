
function OnBeginAjax(response) {
    showLoading("#panel-forma", "Validando datos", "Por favor esperar");
}

function OnSuccessAjax(response) {
    debugger
    if (response.Code == 200) {
        window.location.href = response.Message;
    } else { 
        hideLoading("#panel-forma");
        var confirmMessage = response.Message;
        var closeEvent = () => { }
        var buttons = response.Message.indexOf('Captcha') > 0 ? AlertExecLogin() : AlertExec();
        showConfirmation(confirmMessage, buttons, null, closeEvent);
    }
}

function OnFailureAjax(response) {
    var confirmMessage = response.responseJSON.message;
    var buttons = AlertExec();
    showConfirmation(confirmMessage, buttons);
}
