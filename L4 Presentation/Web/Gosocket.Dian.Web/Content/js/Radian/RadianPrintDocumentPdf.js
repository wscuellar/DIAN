function showPdfModal(element, cufe, url, panel) {
    $(element).click(() => {
        showLoading(panel, 'Cargando', 'Procesando datos, por favor espere.');
        var data = { cufe }
        var actionSuccess = (docBase) => {
            hideLoading(panel);
            downloadPDF(docBase, cufe);
        }
        ajaxFunction(url, "POST", data, () => { }, actionSuccess);
    })
}

function showPdfModalClick(cufe, url, panel) {
    showLoading(panel, 'Cargando', 'Procesando datos, por favor espere.');
    var data = { cufe }
    var actionSuccess = (docBase) => {
        hideLoading(panel);
        downloadPDF(docBase, cufe);
    }
    ajaxFunction(url, "POST", data, () => { }, actionSuccess);
}

function showPdfModalDirect(cufe, url, panel) {
    showLoading(panel, 'Cargando', 'Procesando datos, por favor espere.');
    var data = { cufe }
    var actionSuccess = (docBase) => {
        hideLoading(panel);
        downloadPDF(docBase, cufe);
    }
    ajaxFunction(url, "POST", data, () => { }, actionSuccess);
}

function downloadPDF(pdf, name) {
    const linkSource = `data:application/pdf;base64,${pdf}`;
    const downloadLink = document.createElement("a");
    const fileName = name + ".pdf";
    downloadLink.href = linkSource;
    downloadLink.download = fileName;
    downloadLink.click();
}