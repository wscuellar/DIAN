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

function showConfirmation(confirmMessage, buttons) {
    bootbox.dialog({
        title: "Advertencia",
        message: "<div class='media'><div class='media-body'>" + "<h4 class='text-thin'>" + confirmMessage + "</h4></div></div>",
        buttons: buttons
    });
}

function ConfirmExec(operation) {
    return {
        del: {
            label: "Aceptar",
            className: "btn-gosocket",
            callback: function () {
                operation();
            }
        },
        del1: {
            label: "Cancelar",
            className: "btn-gosocket",
        }
    }
}

function AlertExec() {
    return {
        del: {
            label: "Aceptar",
            className: "btn-gosocket",
        }
    }
}