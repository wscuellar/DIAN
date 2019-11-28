$(document).ready(function () {
    
});

function hideLoading(target) {
    $(target).niftyOverlay('hide');
}

function showLoading(target, title, message) {
    $(target).niftyOverlay({
        title: title + '...',
        desc: message
    });
    $(target).niftyOverlay('show');
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