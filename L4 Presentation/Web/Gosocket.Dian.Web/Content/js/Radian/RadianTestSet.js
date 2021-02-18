$('#OperationModeId').change(function () {
    $('.captureFields').val('0');
    $("#Description").val("");
});

$('#TotalDocumentRequired').attr('readonly', true);
$('#TotalDocumentAcceptedRequired').attr('redonly', true);


$('.btn-save').click(function () {

    var form = $('#edit-testset-form');
    if (form.valid()) {
        showLoading('#panel-form', 'Editando', 'Procesando datos, por favor espere.');
        form.submit();
    }
});

function updateTotalInputRequired() {
    $('.docRequired Div input:not(#TotalDocumentRequired)').change(function () {
        updateTotal();
    });
}

function updateTotalInputAceptedRequired() {
    $('.docAcepted Div input:not(#TotalDocumentAcceptedRequired)').change(function () {
        updateTotalRequired();
    });
}


function updateTotal() {
    var summary = 0;
    $('.docRequired Div input:not(#TotalDocumentRequired)').each(function () {
        summary += parseInt($(this).val());
    });
    $("#TotalDocumentRequired").val(summary);
}

function updateTotalRequired() {
    var summary = 0;
    $('.docAcepted Div input:not(#TotalDocumentAcceptedRequired)').each(function () {
        summary += parseInt($(this).val());
    });
    $("#TotalDocumentAcceptedRequired").val(summary);
}

function showErrorMessage(total) {
    showNotification('warning', 'fa fa-check fa-2x', 'floating', 'Aviso.', 'El valor no puede ser inferior a ' + total);
}


function showDetails(html, softwareName, url) {
    var data = {
        softwareType: softwareName
    }
    ShowDetailsTestSetConfig(html, data, url);
}

$(document).ready(function () {
    updateTotalInputRequired();
    updateTotalInputAceptedRequired();
})