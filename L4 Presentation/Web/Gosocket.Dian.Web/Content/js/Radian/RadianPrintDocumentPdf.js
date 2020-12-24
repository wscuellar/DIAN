function showPdfModal(element, cufe, url) {
    $(element).click(() => {
        showLoading('#panel-form', 'Cargando', 'Procesando datos, por favor espere.');
        var data = { cufe }
        var actionSuccess = (docBase) => {
            hideLoading('#panel-form');
            bootbox.dialog({
                message: '<object data="data:application/pdf;base64,' + docBase + '" width="860" height="768" type="application/pdf"></object>',
                size: 'large',
                backdrop: false
            });
        }
        ajaxFunction(url, "POST", data, () => { }, actionSuccess);
    })

}