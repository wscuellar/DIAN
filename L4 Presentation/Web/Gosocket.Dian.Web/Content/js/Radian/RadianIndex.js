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

function DeleteOperationMode(url) {debugger
    $("#delete-software").click(function () {debugger
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


function SetIconsList() {
    var myOptions = [
        ['ct', 'aproved.png', 'Catalonia'],
        ['es', 'es.png', 'Spain'],
        ['gb', 'gb.png', 'Great Britain'],
        ['de', 'de.png', 'Germany'],
        ['it', 'it.png', 'Italy'],
        ['fi', 'fi.png', 'Finland'],
        ['fr', 'fr.png', 'France']
    ];
    var myTemplate = "<div class='jqcs_option' data-select-value='$0' style='background-image:url(../../Content/images/$1);'>$2</div>";
    $.customSelect({
        selector: '#RadianFileStatus',
        placeholder: '',
        options: myOptions,
        template: myTemplate
    });
    $('input#RadianFileStatus')[0].value;
    //var numberOptions = $(".list-change-status option").length;
    //var actualHtml = "";
    //var newHtml = "";
    //for (var i = 1; i <= numberOptions; i++) {
    //    actualHtml = $($(".list-change-status option")[i]).html();
    //    switch (i) {
    //        case 1:
    //            newHtml = "<i class='fa fa-exclamation-circle'></i>";
    //            break;
    //        case 2:
    //            newHtml = "<img src='../../Content/images/Svg/Loaded.svg'>";
    //            break;
    //        case 3:
    //            newHtml = "<i class='fa fa-check-circle'></i>";
    //            break;
    //        case 4:
    //            newHtml = "<i class='fa fa-times-circle'></i>";
    //            break;
    //        case 5:
    //            newHtml = "<i class='fa fa-info-circle'></i>";
    //            break;
    //        default:
    //            newHtml = "<i class='fa fa-exclamation-circle'></i>";
    //    }
    //    debugger
    //    $($($(".list-change-status option")[i])[0]).html(newHtml + " " + actualHtml);
}