function rememberPassword(htmlPartial, url) {
    $("#forgot-password").click(function (e) {
        var box = bootbox.dialog({
            message: htmlPartial,
            className: "table-data modal-radian set-test-counts",
            size: 'medium'
        })
        box.bind('shown.bs.modal', function () {
            $("#submitRememberPassword").click(function (e) {
                showLoading('#panel-form', 'Cargando', 'Enviando datos, por favor espere.');
                var form = $("#forgot-pasword-form");
                if ($(form).valid()) {
                    e.preventDefault();
                    var data = {
                        email: $('#EmailRemember').val()
                    }
                    var actionError = (error) => {
                        console.log(error);
                    }
                    var actionSuccess = (success) => {
                        hideLoading("#panel-form");
                        var className = success.Code == 400 ? "" : "cancel-confirmation";
                        var operationCancel = () => {
                            success.Code == 200 && window.location.reload();
                        }
                        var buttons = AlertExec();
                        showConfirmation(success.Message, buttons, className, operationCancel);
                    }
                    ajaxFunction(url, 'GET', data, actionError, actionSuccess);
                }
            })
            $("#btnCancel").click(function (e) {
                e.preventDefault();
                box.modal('hide');
            });
        });
    })
}

