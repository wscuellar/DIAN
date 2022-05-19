$(document).ready(function () {
    
});

function hideLoading(target) {
    hideDianLoading();
    //$(target).niftyOverlay('hide');
}

function showLoading(target, title, message) {
    showDianLoading();
    //$(target).niftyOverlay({
    //    title: title + '...',
    //    desc: message
    //});
    //$(target).niftyOverlay('show');
}

function showNotification(type, icon, panel, title, message) {
    $.niftyNoty({
        type: type,
        icon: icon,
        container: panel,
        title: title,
        message: message,
        timer: 10000
    });
}

function showPageNotification(type, message) {
    $.niftyNoty({
        type: type,
        container: 'page',
        html: message,
        timer: 10000
    });
}