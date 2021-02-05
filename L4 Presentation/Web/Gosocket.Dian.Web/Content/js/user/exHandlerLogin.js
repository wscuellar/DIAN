function OnBeginAjax(response) {
    showLoading("#panel-form", "Validando datos", "Por favor esperar");
}

function OnSuccessAjax(response) {
    hideLoading("#panel-form");
    if (response.Code == 200) {
        setTimeout(() => showLoading("#panel-form", "Redireccionando...", "Por favor esperar"), 500);
        window.location.href = response.Message;
    } else {
        var confirmMessage = response.Message;
        var buttons = AlertExec();
        showConfirmation(confirmMessage, buttons);
    }
}

function OnFailureAjax(response) {
    var confirmMessage = response.responseJSON.message;
    var buttons = AlertExec();
    showConfirmation(confirmMessage, buttons);
}
