$(document).ready(function () {
    var indexContributorFile = parseInt($('#lastIndexContributorFile').val());

   

});


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
            line += "<input name='ContributorFiles[" + indexContributor + "].ContributorFileType.Id' type='hidden' value='" + id + "'/>";
            line += "<input name='ContributorFiles[" + indexContributor + "].FileName' type='hidden' value='" + name + "'/>";
            line += "<input name='ContributorFiles[" + indexContributor + "].IsNew' type='hidden' value='true'/>" + name + "</td>";
            line += "<td class='text-center' nowrap><span class='label label-info'>Agregado<span></td>";
            line += "<td class=''><a href='javascript:void(0)' class='text-center new' data-index-contributor-file='" + indexContributor + "' onclick='removeLine(this)'> <i class='fa fa-trash'></i></a></td></tr>";
            $('#table-contributor-files > tbody').append(line);
            indexContributor++;
        } else {
            showNotification('info', 'fa fa-info fa-2x', 'floating', 'Aviso', 'Archivo añadido previamente');
        }
    });
});

function removeLine(button) {
    if ($(button).hasClass('new'))
        $(button).parent().parent().remove();
    else {
        var index = $(button).attr("data-index-contributor-file");
        var line = "";
        line = "<input name='Contributors[" + index + "].Deleted' type='hidden' value='true'/>";
        $('#table-contributor-files > tbody').append(line);
        $(button).parent().parent().hide();
    }    
}

