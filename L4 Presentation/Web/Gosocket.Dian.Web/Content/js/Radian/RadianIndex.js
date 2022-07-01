
(document.querySelectorAll('img.RadianimgSvg').forEach(function (img) {
    var imgID = img.id;
    var imgClass = img.className;
    var imgURL = img.src;

    fetch(imgURL).then(function (response) {
        return response.text();
    }).then(function (text) {

        var parser = new DOMParser();
        var xmlDoc = parser.parseFromString(text, "text/xml");
        var svg = xmlDoc.getElementsByTagName('svg')[0];
        if (typeof imgID !== 'undefined') {
            svg.setAttribute('id', imgID);
        }
        if (typeof imgClass !== 'undefined') {
            svg.setAttribute('class', imgClass + ' replaced-svg');
        }
        svg.removeAttribute('xmlns:a');
        if (!svg.getAttribute('viewBox') && svg.getAttribute('height') && svg.getAttribute('width')) {
            svg.setAttribute('viewBox', '0 0 ' + svg.getAttribute('height') + ' ' + svg.getAttribute('width'))
        }
        img.parentNode.replaceChild(svg, img);
    });

}));


function CallExecution(callMethod, url, jsonvalue, method, showMessage, cancelFunction) {
    $.ajax({
        url: url,
        type: callMethod,
        data: jsonvalue,
        success: function (data) {
            hideLoading('#panel-form');
            if (showMessage) {
                if (data.MessageType === "alert") {
                    showConfirmation(data.Message, AlertExec(cancelFunction));
                }
                if (data.MessageType === "confirm") {
                    showConfirmation(data.Message, ConfirmExec(method, jsonvalue, cancelFunction));
                }
                if (data.MessageType === "redirect") {
                    operationClick = false;
                    window.location.href = data.RedirectTo;
                }
            }
            else {
                if (data.Code == "500" && data.MessageType === "alert") {
                    showConfirmation(data.Message, AlertExec(cancelFunction));
                }
                else {
                    method(jsonvalue);
                }
                
            }

        }
    });
}

function CallExecutionWithData(callMethod, url, jsonvalue, method, showMessage, cancelFunction) {
    $.ajax({
        url: url,
        type: callMethod,
        data: jsonvalue,
        success: function (data) {
            if (showMessage) {
                if (data.MessageType === "alert") {
                    showConfirmation(data.Message, AlertExec(cancelFunction));
                }
                if (data.MessageType === "confirm") {
                    showConfirmation(data.Message, ConfirmExec(method, jsonvalue, cancelFunction));
                }
                if (data.MessageType === "redirect") {
                    operationClick = false;
                    window.location.href = data.RedirectTo;
                }
            }
            else {
                if (data.Code == "500" && data.MessageType === "alert") {
                    showConfirmation(data.Message, AlertExec(cancelFunction));
                }
                else {
                    method(jsonvalue, data);
                }

            }

        }
    });
}


function showConfirmation(confirmMessage, buttons, className, operationCancel) {
    if (!buttons) {
        SuccessDialogV2Callback(confirmMessage, "", operationCancel);
        return;
    }

    if (Object.keys(buttons).length <= 1) {
        ErrorDialogV2Callback("", confirmMessage, operationCancel);
        return;
    }
    
    ConfirmDialogV2(confirmMessage, " ",
        { confirm: buttons.del.label, cancel: buttons.del1.label },
        buttons.del.callback, buttons.del1.callback);

    //bootbox.dialog({
    //    className: className && className,
    //    message: "<div class='media'><div class='media-body'>" + "<h4 class='text-thin'>" + confirmMessage + "</h4></div></div>",
    //    buttons: buttons,
    //    onEscape: () => {
    //        operationCancel ? operationCancel() : window.location.reload();
    //    }
    //});

}

function showConfirmationComplement(confirmMessage, buttons, className, operationCancel, complement) {
    showConfirmation(`${confirmMessage} <br> ${complement}`, buttons, className, operationCancel);

    //bootbox.dialog({
    //    className: className && className,
    //    message: "<div class='media'><div class='media-body'>" + "<h4 class='text-thin'>" + confirmMessage + "</h4> <p class='text-center subtitle-modal'>" + complement + "</p></div></div>",
    //    buttons: buttons,
    //    onEscape: () => {
    //        operationCancel ? operationCancel() : window.location.reload();
    //    }
    //});
}

function ConfirmExec(operation, param, operationCancel) {
    return {
        del: {
            label: "Aceptar",
            className: "btn-radian-default",
            callback: function () {
                param ? operation(param) : operation();
                operationClick = false;
            }
        },
        del1: {
            label: "Cancelar",
            className: "btn-radian-default btn-radian-success",
            callback: function () {
                operationCancel != null && operationCancel();
                operationClick = false;
            }
        }
    }
}

function AlertExec(operation) {
    return {
        del: {
            label: "Aceptar",
            className: "btn-radian-default",
            callback: function () {
                operation != null && operation();
                operationClick = false;
            }
        }
    }
}

function AlertExecLogin(operation) {
    return {
        del: {
            label: "Aceptar",
            className: "btn-radian-default",
            callback: function () {
                operation != null && operation();
                location.reload();
                window.location.href = window.location.href;
                operationClick = false;
            }
        }
    }
}


function ajaxFunction(url,metod,data,actionError,actionSuccess) {
    $.ajax({
        url: url,
        type: metod,
        data: data,
        error: actionError,
        success: actionSuccess
    });
}

function SetIconsList(fileId) {
    var myOptions = [
        ['0', 'exclamation-circle.png', 'Pendiente'],
        ['1', 'Loaded.png', 'Cargado y en revisión'],
        ['2', 'aproved.png', 'Aprobado'],
        ['3', 'reject.png', 'Rechazado'],
        ['4', 'observations.png', 'Observaciones']
    ];
    var myTemplate = "<div class='jqcs_option' data-select-value='$0' style='background-image:url(../../Content/images/$1);'>$2</div>";
    $.customSelect({
        selector: '#'+fileId,
        placeholder: '',
        options: myOptions,
        template: myTemplate
    });
    $('input#' + fileId)[0].value;
}

function CancelRegister(url, dataAjax, confirmMessage, successAction, label, errorAction) {
        var metod = 'POST';
        var operation = (description) => ajaxFunction(url, metod, { ...dataAjax, description }, errorAction, successAction);
        ShowPromptCancel(confirmMessage, operation, label, errorAction, bootboxMessage.CANCEL_REGISTER);
}

function ShowPromptCancel(title, event, label, operationCancel, buttonAceptText) {
    var bootboxPrompt = bootbox.prompt({
        className: "prompt-comment",
        title: title,
        inputType: 'textarea',
        message: label,
        placeholder: "Escriba aquí...",
        inputOptions: {
            text: "text",
            value: "value"
        },
        buttons: {
            confirm: {
                label: buttonAceptText ? buttonAceptText : bootboxMessage.ACEPTAR,
                className: "btn-radian-success",
            },
            cancel: {
                label: "Cancelar",
                className: "btn-radian-default",
            }
        },
        callback: function (result) {
            if (!result && result != "") {
                operationCancel && operationCancel();
            } else {
                event(result);
            }
        }
    });

    bootboxPrompt.init(function () {
        $(".bootbox-form").prepend($("<label>", { text: title == "Cancelar registro" ? bootboxMessage.LABEL_PROMPT : bootboxMessage.LABEL_STATUS }));
    });

}

function ShowDetailsTestSet(htmlPartial, id, softwareId, type, url) {
    var data = {
        code: id,
        softwareId: softwareId,
        type: type
    }
    customDialog(htmlPartial, data, url);
}

function ShowDetailsTestSetConfig(htmlPartial, data, url) {
    customDialog(htmlPartial, data, url);
}

function customDialog(htmlPartial, data, url) {
    var actionError = (error) => {
        console.log(success);
    }
    var actionSuccess = (success) => {
        var html = "";
        var columns = 0;
        success.forEach((element, index) => {
            html += '<li>\
            <div class="set-details"><span>' + element.EventName + '</span><div><span><a class="badge custom-badget-blue">' + element.Counter1 + '</a></span><span> <a class="badge custom-badget-green">' + element.Counter2 + '</a></span><span> <a class="badge custom-badget-red">' + element.Counter3 + '</a></span></div></div>\
            </li >';
            if ((index + 1) % 8 == 0) {
                $(".list-unstyled-" + columns).append(html);
                columns++;
                html = "";
            }
        });
        $(".list-unstyled-" + columns).append(html);

    }
    bootbox.dialog({
        message: htmlPartial,
        className: "table-data modal-radian set-test-counts",
        size: 'large'
    }).init(() => {
        ajaxFunction(url, "POST", data, actionError, actionSuccess);
    });
}


function DeleteOptions(operationMode, radianState) {
    if (operationMode == 1 || radianState == 'Habilitado') {
        $("#RadianApprovalState option[value='0']").remove();
    }
    if (radianState == 'En pruebas') {
        $("#RadianApprovalState option[value='1']").remove();
    }
}

function SetCapcthatoken(configuration) {
        //grecaptcha.ready(function () {
        //    grecaptcha.execute(configuration, { action: 'submit' }).then(function (token) {debugger
        //        $(".downloadPDFUrl").attr("href", $(".downloadPDFUrl").attr("href") + "&recaptchaToken=" + token);
        //    });
        //});
}

function RestartCounters(url, testResult, softwareId, operationId) {
    console.log(testResult);
    var confirmMessage = bootboxMessage.CONFIRMATION_RESTART;
    var complement = bootboxMessage.CONFIRMATION_RESTART_COMPLEMENT;
    var operation = () => {
        var actionError = (error) => {
            window.location.reload();
        }
        var actionSuccess = (result) => {
            window.location.reload();
        }
        var data = {
            result: testResult,
            softwareId,
            operationId
        }
        ajaxFunction(url, "POST", data, actionError, actionSuccess);
    }
    var buttons = ConfirmExec(operation)
    showConfirmationComplement(confirmMessage, buttons,null, null, complement);
}