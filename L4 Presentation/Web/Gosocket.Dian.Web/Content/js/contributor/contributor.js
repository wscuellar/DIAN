$(document).ready(function () {
    var indexContributorFile = parseInt($('#lastIndexContributorFile').val());

    $(".add-contributor-file").click(function () {
        var exist = false;
        var id = $(this).attr('data-id');
        var name = $(this).attr('data-name');

        $("#table-contributor-files").find("tbody > tr").each(function () {
            var val = $(this).attr('data-id');
            if (val === id) {
                exist = true;
            }
        });

        if (!exist) {
            var line = "<tr data-id='" + id + "'>";
            line += "<td class='text-left'>";
            line += "<input name='ContributorFiles[" + indexContributorFile + "].ContributorFileType.Id' type='hidden' value='" + id + "'/>";
            line += "<input name='ContributorFiles[" + indexContributorFile + "].FileName' type='hidden' value='" + name + "'/>";
            line += "<input name='ContributorFiles[" + indexContributorFile + "].IsNew' type='hidden' value='true'/>" + name + "</td>";
            line += "<td class='text-center' nowrap><span class='label label-info'>Agregado<span></td>";
            line += "<td class=''><a href='javascript:void(0)' class='text-center new' data-index-contributor-file='" + indexContributorFile + "' onclick='removeLine(this)'> <i class='fa fa-trash'></i></a></td></tr>";
            $('#table-contributor-files > tbody').append(line);
            indexContributorFile++;
        } else {
            showNotification('info', 'fa fa-info fa-2x', 'floating', 'Aviso', 'Archivo añadido previamente');
        }
    });
    $('.download-contributor-files').click('onClick', function () {
        downloadContributorFile(this);
    });
});

function removeLine(button) {
    if ($(button).hasClass('new'))
        $(button).parent().parent().remove();
    else {
        var index = $(button).attr("data-index-contributor-file");
        var line = "";
        line = "<input name='ContributorFiles[" + index + "].Deleted' type='hidden' value='true'/>";
        $('#table-contributor-files > tbody').append(line);
        $(button).parent().parent().hide();
    }    
}

function downloadContributorFile(span) {
    $(span).parent().parent().addClass('not-redirect');
    var code = $(span).attr('data-code');
    var fileName = $(span).attr('data-fileName');
    var id = span.id;
    if ($(span).hasClass("fa-download")) {
        $(span).removeClass("fa-download");
        $(span).addClass("fa-spinner");
        setTimeout(function () {
            $("#" + id).removeClass("fa-spinner");
            $("#" + id).addClass("fa-download");
        }, 5000);
        window.location.href = "/Contributor/DownloadContributorFile?code=" + code + "&fileName=" + fileName;
    }
}

