function RenderSteps(index) {
    $("#steps-approved").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        autoFocus: true,
        startIndex: index,
        enablePagination: false,
        enableKeyNavigation: false
    });

    $(".radian-file").click(function () {
        $(this).val("");
    });
    $(".radian-file").change(function (file) {
        var form = $("#uploadForm");
        var messages = new Object();
        var id = $(this).attr('name');
        var fileObj = file.target.files[0];
        var fileSize = Math.round(fileObj.size / 10000) / 100;
        if (fileSize > 10) {
            $(this).val("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Tamaño máximo 10 Mb."
                }
            });
        }
        else {
            $(this).parent().children().html(fileObj.name + "  (" + fileSize + " Mb)");
        }
        if (fileObj.type != "application/pdf") {
            $(this).val("");
            messages = Object.assign(messages, {
                [id]: {
                    required: "Solo documentos .PDF"
                }
            });
        }
        form.validate({
            messages: messages
        });
        form.valid();
    });

    $(".close").click(function () {
        $(this).parent().toggle();
        $(this).parents(".inputs-dinamics").children(".file-input-enabled").toggle();
    })
}

function LoadEventsToSearch() {

}