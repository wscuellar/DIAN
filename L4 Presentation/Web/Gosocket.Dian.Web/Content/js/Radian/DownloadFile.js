$(document).ready(function () {
    $('.download-contributor-files').click('onClick', function () {
        downloadContributorFile(this);
    });
});



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
        window.location.href = "/Radian/DownloadContributorFile?code=" + code + "&fileName=" + fileName;
    }
}