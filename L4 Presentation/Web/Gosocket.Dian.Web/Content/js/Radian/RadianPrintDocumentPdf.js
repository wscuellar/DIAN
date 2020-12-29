function showPdfModal(element, cufe, url, panel) {
    $(element).click(() => {
        showLoading(panel, 'Cargando', 'Procesando datos, por favor espere.');
        var data = { cufe }
        var actionSuccess = (docBase) => {
            hideLoading(panel);
            bootbox.dialog({
                message: '<object data="data:application/pdf;base64,' + docBase + '" width="860" height="768" type="application/pdf"></object>',
                size: 'large',
                backdrop: false
            });
        }
        ajaxFunction(url, "POST", data, () => { }, actionSuccess);
    })

}