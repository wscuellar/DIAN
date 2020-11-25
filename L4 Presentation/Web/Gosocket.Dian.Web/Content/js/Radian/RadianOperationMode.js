function DeleteOperationMode(url) {
    $("#delete-software").click(function () {
        var metod = 'POST';
        var data = {
            Id: $(this).attr("data-id")
        }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
}

function AddOperationMode(url, contributorId, radianTypeId) {
    $("#save-operation-mode").click(function () {
        var metod = 'POST';
        var data = {
            ContributorId: contributorId,
            RadianTypeId: radianTypeId
        }
        var actionError = () => { }
        var actionSuccess = () => {
            location.reload();
        }
        ajaxFunction(url, metod, data, actionError, actionSuccess)
    })
}

function RenderAutocomplete(url) {
    $('.basicAutoComplete').autoComplete({
        resolverSettings: {
            url: url
        }
    });
}