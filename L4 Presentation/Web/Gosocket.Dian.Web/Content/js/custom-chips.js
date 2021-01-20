var list = document.getElementById('list');
var profiles = [];


$(".add-profile").click(function () {
    var option = $('[name=ProfileId] option:selected').html();
    var optionId = $('[name=ProfileId] option:selected').val();
    let val = $('[name=ProfileId]').val();
    if (val !== "0") {
        var elementFind = profiles.find(m => m.option == option);
        if (!elementFind) {
            profiles.push({ option, optionId});
            render();
            checkChecks(optionId);
        }
    } 
});

function render() {
    list.innerHTML = '';
    profiles.map((item, index) => {
        list.innerHTML += `<li><span>${item.option}</span><a href="javascript: remove(${index},${item.optionId})">X</a></li>`;
    });
}


function remove(i, optionId) {
    profiles = profiles.filter(item => profiles.indexOf(item) != i);
    unchekedPermits(profiles);
    render();
}

function checkChecks(profileId) {
    var idProfile = profileId;
    var url = '/FreeBiller/GetIdsByProfile';
    var dataAjax = {
        profileId: idProfile
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

function unchekedPermits(profiles) {
    $("input:checkbox:checked").prop("checked", false);
    profiles.forEach((element) => {
        checkChecks(element.optionId)
    });
}

window.onload = function () {
    render();
}