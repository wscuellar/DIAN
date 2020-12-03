function showPdfModal(element, cufe, url) {
    $(element).click(() => {
        var data = { cufe }
        var actionSuccess = (docBase) => {
            bootbox.dialog({
                message: '<object data="data:application/pdf;base64,' + docBase + '" width="1024" height="768" type="application/pdf"></object>',
                size: 'large',
                backdrop: false
            });
        }
        ajaxFunction(url, "POST", data, () => { }, actionSuccess);
    })

}