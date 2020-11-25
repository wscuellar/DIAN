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

function RenderAutocomplete() {
    const myData = [{
        "id": 1,
        "name": 'Item 1',
        "ignore": false
    }, {
        "id": 2,
        "name": 'Item 2',
        "ignore": false
    }, {
        "id": 3,
        "name": 'Item 3',
        "ignore": false
    },
        // ...
    ]

    $('.demo').autocomplete({
        nameProperty: 'name',
        valueField: '#hidden-field',
        dataSource: myData
    });
}