function rememberPassword(htmlPartial) {
    $("#forgot-password").click(function (e) {
        var box = bootbox.dialog({
            message: htmlPartial,
            className: "table-data modal-radian set-test-counts",
            size: 'medium'
        })
        box.bind('shown.bs.modal', function () {
            $("#submitRememberPassword").click(function (e) {
                var form = $("#forgot-pasword-form");
                if ($(form).valid()) {

                    ajaxFunction(url, metod, data, actionError, actionSuccess);
                }
            })
            $("#btnCancel").click(function (e) {
                e.preventDefault()
                box.hideAll();
            });
        });
    })
}

