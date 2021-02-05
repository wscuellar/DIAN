function OnBegin(response) {

}

function OnSuccess(response) {
    if (response.Code == 200) {
        window.location.href = response.Message;
    }
}

function OnFailure(response) {

}
