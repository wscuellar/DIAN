$('#OperationModeId').change(function () {
    $('.captureFields').val('0');
    $("#Description").val("");
});

$('#TotalDocumentRequired').attr('readonly', true);
$('#TotalDocumentAcceptedRequired').attr('redonly', true);


$("#EndorsementWarrantyTotalRequired").change(function () {
    var noticeTotal = parseInt($("#EndorsementWarrantyTotalRequired").val());
    if (noticeTotal < 0) {
        showErrorMessage(0);
        $("#EndorsementWarrantyTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});

$("#EndorsementProcurationTotalRequired").change(function () {
    var noticeTotal = parseInt($("#EndorsementProcurationTotalRequired").val());
    if (noticeTotal < 0) {
        showErrorMessage(0);
        $("#EndorsementProcurationTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});

$("#ReceiptNoticeTotalRequired").change(function () {
    var noticeTotal = parseInt($("#ReceiptNoticeTotalRequired").val());
    if (noticeTotal < 0) {
        showErrorMessage(0);
        $("#ReceiptNoticeTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#ReceiptServiceTotalRequired").change(function () {
    var receiptServiceTotal = parseInt($("#ReceiptServiceTotalRequired").val());
    if (receiptServiceTotal < 0) {
        showErrorMessage(0);
        $("#ReceiptServiceTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#ExpressAcceptanceTotalRequired").change(function () {
    var expressAcceptanceTotal = parseInt($("#ExpressAcceptanceTotalRequired").val());
    if (expressAcceptanceTotal < 0) {
        showErrorMessage(0);
        $("#ExpressAcceptanceTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#AutomaticAcceptanceTotalRequired").change(function () {
    var automaticAcceptanceTotal = parseInt($("#AutomaticAcceptanceTotalRequired").val());
    if (automaticAcceptanceTotal < 0) {
        showErrorMessage(0);
        $("#AutomaticAcceptanceTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#RejectInvoiceTotalRequired").change(function () {
    var rejectInvoiceTotal = parseInt($("#RejectInvoiceTotalRequired").val());
    if (rejectInvoiceTotal < 0) {
        showErrorMessage(0);
        $("#RejectInvoiceTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#ApplicationAvailableTotalRequired").change(function () {
    var appAvailableTotal = parseInt($("#ApplicationAvailableTotalRequired").val());
    if (appAvailableTotal < 0) {
        showErrorMessage(0);
        $("#ApplicationAvailableTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#EndorsementTotalRequired").change(function () {
    var endorsementTotal = parseInt($("#EndorsementTotalRequired").val());
    if (endorsementTotal < 0) {
        showErrorMessage(0);
        $("#EndorsementTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#EndorsementCancellationTotalRequired").change(function () {
    var endorsementcancelTotal = parseInt($("#EndorsementCancellationTotalRequired").val());
    if (endorsementcancelTotal < 0) {
        showErrorMessage(0);
        $("#EndorsementCancellationTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#GuaranteeTotalRequired").change(function () {
    var garanteeTotal = parseInt($("#GuaranteeTotalRequired").val());
    if (garanteeTotal < 0) {
        showErrorMessage(0);
        $("#GuaranteeTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#EndMandateTotalRequired").change(function () {
    var endMandateTotal = parseInt($("#EndMandateTotalRequired").val());
    if (endMandateTotal < 0) {
        showErrorMessage(0);
        $("#EndMandateTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#ElectronicMandateTotalRequired").change(function () {
    var electronicMandateTotal = parseInt($("#ElectronicMandateTotalRequired").val());
    if (electronicMandateTotal < 0) {
        showErrorMessage(0);
        $("#ElectronicMandateTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#PaymentNotificationTotalRequired").change(function () {
    var paymentNotificationTotal = parseInt($("#PaymentNotificationTotalRequired").val());
    if (paymentNotificationTotal < 0) {
        showErrorMessage(0);
        $("#PaymentNotificationTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#CirculationLimitationTotalRequired").change(function () {
    var circulationNotificationTotal = parseInt($("#CirculationLimitationTotalRequired").val());
    if (circulationNotificationTotal < 0) {
        showErrorMessage(0);
        $("#CirculationLimitationTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});
$("#EndCirculationLimitationTotalRequired").change(function () {
    var endcirculationNotificationTotal = parseInt($("#EndCirculationLimitationTotalRequired").val());
    if (endcirculationNotificationTotal < 0) {
        showErrorMessage(0);
        $("#EndCirculationLimitationTotalRequired").val(0);
    }
    else {
        updateTotal();
    }
});

$('#TotalDocumentAcceptedRequired').attr('readonly', true);

$("#ReceiptNoticeTotalAcceptedRequired").change(function () {
    var noticeTotal0 = parseInt($("#ReceiptNoticeTotalAcceptedRequired").val());
    if (noticeTotal0 < 0) {
        showErrorMessage(0);
        $("#ReceiptNoticeTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#ReceiptServiceTotalAcceptedRequired").change(function () {
    var receiptServiceTotal0 = parseInt($("#ReceiptServiceTotalAcceptedRequired").val());
    if (receiptServiceTotal0 < 0) {
        showErrorMessage(0);
        $("#ReceiptServiceTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#ExpressAcceptanceTotalAcceptedRequired").change(function () {
    var expressAcceptanceTotal0 = parseInt($("#ExpressAcceptanceTotalAcceptedRequired").val());
    if (expressAcceptanceTotal0 < 0) {
        showErrorMessage(0);
        $("#ExpressAcceptanceTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#AutomaticAcceptanceTotalAcceptedRequired").change(function () {
    var automaticAcceptanceTotal0 = parseInt($("#AutomaticAcceptanceTotalAcceptedRequired").val());
    if (automaticAcceptanceTotal0 < 0) {
        showErrorMessage(0);
        $("#AutomaticAcceptanceTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#RejectInvoiceTotalAcceptedRequired").change(function () {
    var rejectInvoiceTotal0 = parseInt($("#RejectInvoiceTotalAcceptedRequired").val());
    if (rejectInvoiceTotal0 < 0) {
        showErrorMessage(0);
        $("#RejectInvoiceTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#ApplicationAvailableTotalAcceptedRequired").change(function () {
    var appAvailableTotal0 = parseInt($("#ApplicationAvailableTotalAcceptedRequired").val());
    if (appAvailableTotal0 < 0) {
        showErrorMessage(0);
        $("#ApplicationAvailableTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#EndorsementTotalAcceptedRequired").change(function () {
    var endorsementTotal0 = parseInt($("#EndorsementTotalAcceptedRequired").val());
    if (endorsementTotal0 < 0) {
        showErrorMessage(0);
        $("#EndorsementTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#EndorsementCancellationTotalAcceptedRequired").change(function () {
    var endorsementcancelTotal0 = parseInt($("#EndorsementCancellationTotalAcceptedRequired").val());
    if (endorsementcancelTotal0 < 0) {
        showErrorMessage(0);
        $("#EndorsementCancellationTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#GuaranteeTotalAcceptedRequired").change(function () {
    var garanteeTotal0 = parseInt($("#GuaranteeTotalAcceptedRequired").val());
    if (garanteeTotal0 < 0) {
        showErrorMessage(0);
        $("#GuaranteeTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#EndMandateTotalAcceptedRequired").change(function () {
    var endMandateTotal0 = parseInt($("#EndMandateTotalAcceptedRequired").val());
    if (endMandateTotal0 < 0) {
        showErrorMessage(0);
        $("#EndMandateTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#ElectronicMandateTotalAcceptedRequired").change(function () {
    var electronicMandateTotal0 = parseInt($("#ElectronicMandateTotalAcceptedRequired").val());
    if (electronicMandateTotal0 < 0) {
        showErrorMessage(0);
        $("#ElectronicMandateTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#PaymentNotificationTotalAcceptedRequired").change(function () {
    var paymentNotificationTotal0 = parseInt($("#PaymentNotificationTotalAcceptedRequired").val());
    if (paymentNotificationTotal0 < 0) {
        showErrorMessage(0);
        $("#PaymentNotificationTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#CirculationLimitationTotalAcceptedRequired").change(function () {
    var circulationNotificationTotal0 = parseInt($("#CirculationLimitationTotalAcceptedRequired").val());
    if (circulationNotificationTotal0 < 0) {
        showErrorMessage(0);
        $("#CirculationLimitationTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});
$("#EndCirculationLimitationTotalAcceptedRequired").change(function () {
    var endcirculationNotificationTotal0 = parseInt($("#EndCirculationLimitationTotalAcceptedRequired").val());
    if (endcirculationNotificationTotal0 < 0) {
        showErrorMessage(0);
        $("#EndCirculationLimitationTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});

$("#EndorsementWarrantyTotalAcceptedRequired").change(function () {
    var endcirculationNotificationTotal0 = parseInt($("#EndorsementWarrantyTotalAcceptedRequired").val());
    if (endcirculationNotificationTotal0 < 0) {
        showErrorMessage(0);
        $("#EndorsementWarrantyTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});

$("#EndorsementProcurationTotalAcceptedRequired").change(function () {
    var endcirculationNotificationTotal0 = parseInt($("#EndorsementProcurationTotalAcceptedRequired").val());
    if (endcirculationNotificationTotal0 < 0) {
        showErrorMessage(0);
        $("#EndorsementProcurationTotalAcceptedRequired").val(0);
    }
    else {
        updateTotalRequired();
    }
});


$('.btn-save').click(function () {

    var form = $('#edit-testset-form');
    if (form.valid()) {
        showLoading('#panel-form', 'Editando', 'Procesando datos, por favor espere.');
        form.submit();
    }
});

function updateTotal() {
    var noticeTotal = parseInt($("#ReceiptNoticeTotalRequired").val());
    var receiptServiceTotal = parseInt($("#ReceiptServiceTotalRequired").val());
    var expressAcceptanceTotal = parseInt($("#ExpressAcceptanceTotalRequired").val());
    var automaticAcceptanceTotal = parseInt($("#AutomaticAcceptanceTotalRequired").val());
    var rejectInvoiceTotal = parseInt($("#RejectInvoiceTotalRequired").val());
    var appAvailableTotal = parseInt($("#ApplicationAvailableTotalRequired").val());
    var endorsementTotal = parseInt($("#EndorsementTotalRequired").val());
    var endorsmentWarranty = parseInt($("#EndorsementWarrantyTotalRequired").val());
    var endorsmentProcuration = parseInt($("#EndorsementProcurationTotalRequired").val());
    var endorsementCancelTotal = parseInt($("#EndorsementCancellationTotalRequired").val());
    var garanteeTotal = parseInt($("#GuaranteeTotalRequired").val());
    var electronicMandateTotal = parseInt($("#EndMandateTotalRequired").val());
    var endMandateTotal = parseInt($("#ElectronicMandateTotalRequired").val());
    var paymentNotificationTotal = parseInt($("#PaymentNotificationTotalRequired").val());
    var circulationNotificationTotal = parseInt($("#CirculationLimitationTotalRequired").val());
    var endcirculationNotificationTotal = parseInt($("#EndCirculationLimitationTotalRequired").val());

    $("#TotalDocumentRequired").val(noticeTotal + receiptServiceTotal + expressAcceptanceTotal + automaticAcceptanceTotal
        + rejectInvoiceTotal + appAvailableTotal + endorsementTotal + endorsementCancelTotal + garanteeTotal +
        electronicMandateTotal + endMandateTotal + paymentNotificationTotal + circulationNotificationTotal +
        endcirculationNotificationTotal + endorsmentWarranty + endorsmentProcuration);
}

function updateTotalRequired() {
    var noticeTotal0 = parseInt($("#ReceiptNoticeTotalAcceptedRequired").val());
    var receiptServiceTotal0 = parseInt($("#ReceiptServiceTotalAcceptedRequired").val());
    var expressAcceptanceTotal0 = parseInt($("#ExpressAcceptanceTotalAcceptedRequired").val());
    var automaticAcceptanceTotal0 = parseInt($("#AutomaticAcceptanceTotalAcceptedRequired").val());
    var rejectInvoiceTotal0 = parseInt($("#RejectInvoiceTotalAcceptedRequired").val());
    var appAvailableTotal0 = parseInt($("#ApplicationAvailableTotalAcceptedRequired").val());
    var endorsementTotal0 = parseInt($("#EndorsementTotalAcceptedRequired").val());
    var endorsementCancelTotal0 = parseInt($("#EndorsementCancellationTotalAcceptedRequired").val());
    var endorsmentWarranty0 = parseInt($("#EndorsementWarrantyTotalAcceptedRequired").val());
    var endorsmentProcuration0 = parseInt($("#EndorsementProcurationTotalAcceptedRequired").val());
    var garanteeTotal0 = parseInt($("#GuaranteeTotalAcceptedRequired").val());
    var electronicMandateTotal0 = parseInt($("#EndMandateTotalAcceptedRequired").val());
    var endMandateTotal0 = parseInt($("#ElectronicMandateTotalAcceptedRequired").val());
    var paymentNotificationTotal0 = parseInt($("#PaymentNotificationTotalAcceptedRequired").val());
    var circulationNotificationTotal0 = parseInt($("#CirculationLimitationTotalAcceptedRequired").val());
    var endcirculationNotificationTotal0 = parseInt($("#EndCirculationLimitationTotalAcceptedRequired").val());

    $("#TotalDocumentAcceptedRequired").val(noticeTotal0 + receiptServiceTotal0 + expressAcceptanceTotal0 + automaticAcceptanceTotal0
        + rejectInvoiceTotal0 + appAvailableTotal0 + endorsementTotal0 + endorsementCancelTotal0 + garanteeTotal0 +
        electronicMandateTotal0 + endMandateTotal0 + paymentNotificationTotal0 + circulationNotificationTotal0 +
        endcirculationNotificationTotal0 + endorsmentWarranty0 + endorsmentProcuration0);
}

function showErrorMessage(total) {
    showNotification('warning', 'fa fa-check fa-2x', 'floating', 'Aviso.', 'El valor no puede ser inferior a ' + total);
}

//function showDetails(html, objectSet) {
//    debugger
//    var jsonData = JSON.parse(objectSet);
//    var html = html;
//    var softwareType = data.softwareType;
//    ShowDetailsTestSet(htmlPartial, "", "", softwareType, url);
//}

function showDetails(html, softwareName, url) {
    var data = {
        softwareType: softwareName
    }
    ShowDetailsTestSetConfig(html, data, url);
}
