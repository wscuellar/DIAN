function rememberPassword(htmlPartial, url) {
    $("#forgot-password").click(function (e) {
        var box = bootbox.dialog({
            message: htmlPartial,
            className: "table-data modal-radian set-test-counts",
            size: 'medium'
        })
        box.bind('shown.bs.modal', function () {
            $("#submitRememberPassword").click(function (e) {debugger
                var form = $("#forgot-pasword-form");
                if ($(form).valid()) {
                    e.preventDefault();
                    var data = {
                        email: $('#EmailRemember').val()
                    }
                    var actionError = (error) => {debugger
                        console.log(error);
                    }
                    var actionSuccess = (success) => {
                        debugger
                        console.log(success);
                    }
                    ajaxFunction(url, 'GET', data, actionError, actionSuccess);
                }
            })
            $("#btnCancel").click(function (e) {
                e.preventDefault()
                box.hideAll();
            });
        });
    })
}

