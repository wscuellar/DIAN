
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

    //var newModel = JSON.parse(model.replace(/(&quot\;)/g, "\""));
    //console.log(newModel);
}