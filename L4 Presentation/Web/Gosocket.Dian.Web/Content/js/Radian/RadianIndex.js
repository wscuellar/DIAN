﻿(document.querySelectorAll('img.RadianimgSvg').forEach(function (img) {
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


function CallExecution(callMethod, url, jsonvalue, method, showMessage) {
    $.ajax({
        url: url,
        type: callMethod,
        data: jsonvalue,
        success: function (data) {
            if (showMessage) {
                if (data.MessageType === "alert") {
                    showConfirmation(data.Message, AlertExec());
                }
                if (data.MessageType === "confirm") {
                    showConfirmation(data.Message, ConfirmExec(method, jsonvalue));
                }
                if (data.MessageType === "redirect") {
                    operationClick = false;
                    window.location.href = data.RedirectTo;
                }
            }
            else {
                method(jsonvalue);
            }

        }
    });
}

function showConfirmation(confirmMessage, buttons) {
    bootbox.dialog({
        message: "<div class='media'><div class='media-body'>" + "<h4 class='text-thin'>" + confirmMessage + "</h4></div></div>",
        buttons: buttons
    });
}

function ConfirmExec(operation, param, operationCancel) {
    return {
        del: {
            label: "Aceptar",
            className: "btn-radian-default btn-radian-success",
            callback: function () {
                operation(param);
                operationClick = false;
            }
        },
        del1: {
            label: "Cancelar",
            className: "btn-radian-default",
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


function ajaxFunction(url,metod,data,actionError,actionSuccess) {
    $.ajax({
        url: url,
        type: metod,
        data: data,
        error: actionError,
        success: actionSuccess
    });
}

function DeleteOperationMode(url) {
    $("#delete-software").click(function () {
        var metod = 'POST';
        var data = {
                Id:  $(this).attr("data-id")
            }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
}

function AddOperationMode(url, contributorId) {
    $("#save-operation-mode").click(function () {debugger
        var metod = 'POST';
        var data = {
            ContributorId: contributorId
        }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
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
    debugger
    $('input#' + fileId)[0].value;
}