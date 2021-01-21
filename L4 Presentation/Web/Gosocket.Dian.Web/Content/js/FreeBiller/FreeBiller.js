
function createUser(url, metod, data, actionError, actionSuccess) {
    ajaxFunction(url, metod, data, actionError, actionSuccess);
}

function initialValuesCheck(ProfileId, url) {

    var dataAjax = {
        profileId: ProfileId
    }
    var errorAction = (error) => {
        console.log(error);
    }
    var successAction = (result) => {
        result.forEach(function (id) {
            $("input#" + id).prop("checked", "checked");
        });
    }
    ajaxFunction(url, "POST", dataAjax, errorAction, successAction);
}


function showPassword(element) {
    var attrType = $(element).parent().children("input").attr("type");
    if (attrType == "password") {
        $("#Password").attr("type", "text");
    } else {
        $("#Password").attr("type","password");
    }
}

function initialValuesSwitchActive(users) {
    $("a.isActive-False").click(function (e) { e.preventDefault() });
    var listUsers = JSON.parse(users.replace(/(&quot\;)/g, "\""));
    listUsers.forEach((element) => {
        element.IsActive && $("#" + element.Id + element.NumberDoc).prop("checked", "checked");
    });
}
