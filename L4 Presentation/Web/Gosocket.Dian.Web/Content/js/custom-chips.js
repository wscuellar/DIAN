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
            render("True");
            checkChecks(optionId);
        }
    } 
});

function render(isEdit) {
    list.innerHTML = '';
    profiles.map((item, index) => {
        list.innerHTML += isEdit == "True"
            ? `<li><a href="javascript: remove(${index},${item.optionId})">+</a><span class="margin-right-10">${item.option}</span></li>`
            : `<li><span class="margin-right-10">${item.option}</span></li>`
    });
}


function remove(i, optionId) {
    profiles = profiles.filter(item => profiles.indexOf(item) != i);
    unchekedPermits(profiles);
    render("True");
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

function getProfiles() {
    var profilesIds = [];
    profiles.forEach((profile) => {
        profilesIds.push(Number(profile.optionId));
    });
    return profilesIds;
}

function showInitialChips(profilesIds, profilesList, isEdit) {
    var objProfile; 
    var listProfile = JSON.parse(profilesList.replace(/(&quot\;)/g, "\""));
    profilesIds.forEach((profile) => {
        objProfile = listProfile.find(m => m.Value == String(profile));
        profiles.push({ option: objProfile.Text, optionId: objProfile.Value})
    })
    render(isEdit);
    unchekedPermits(profiles);
}