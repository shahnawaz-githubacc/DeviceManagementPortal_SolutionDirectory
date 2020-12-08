$(document).ready(function () {
    invokeServerToGetLoggedInUserData();
});

function invokeServerToGetLoggedInUserData() {
    //debugger;
    $.ajax({
        url: '/api/accounts/user',
        method: 'GET',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        success: function (data) {
            if (data != null) {
                var userInfo = '<span id="userInfo" class="text-info text-light">User: <b>' + data + '</b></span>';
                $('.navbar-brand').after(userInfo);

                var logoutButton = '<input type="button" id="logoutBtn" onclick="return logout();" value="Logout" class="btn btn-danger">';
                $('#userInfo').after(logoutButton);

                invokeServerToGetData(1);
            }
        },
        error: function (jqXHR) {
            window.location.href = "Login.html";
        }
    });
}

function logout() {
    sessionStorage.removeItem("JWT_Token");
    window.location.href = "Login.html";
}

function invokeServerToGetData(page) {
    //debugger;
    $.ajax({
        url: '/api/devices/all?page=' + page,
        method: 'GET',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        beforeSend: function () {
            $('#errorDiv').empty();
            if (!($('#errorDiv').hasClass('hidden')))
                $('#errorDiv').addClass('hidden');

            if (!($('#paginationDiv').hasClass('hidden')))
                $('#paginationDiv').addClass('hidden');

            sessionStorage.removeItem("PageSize");
            sessionStorage.setItem("PageSize", 5);    // Default
        },
        success: function (data) {
            //debugger;
            $('#mainDiv').removeClass('hidden');
            $('#tableBody').empty();
            $.each(data.devices, function (index, value) {
                var row = $('<tr class="text-center">'
                    + '<td>' + value.imei + '</td>'
                    + '<td class=" text-wrap">' + value.model + '</td>'
                    + '<td>' + value.simCardNumber + '</td>'
                    + '<td>' + renderSwitch(value.enabled, value.deviceID) + '</td>'
                    + '<td>' + value.createdDateTime + '</td>'
                    + '<td>' + value.createdBy + '</td>'
                    + '<td><a class="btn disabled" href=Details/' + value.deviceID + '>Details</a></td>'
                    + '<td><a class="btn btn-link" href="EditDevice.html?id=' + value.deviceID + '">Edit</a></td>'
                    + '<td><button type="button" class="btn btn-link p-0" onclick="deleteDevice(' + value.deviceID + ')">Delete</td>'
                    + '</tr>');
                $('#tableData').append(row);
            });
            $('#paginationDiv').removeClass('hidden');
            $('#paginationUl').empty();
            var index = 0;
            sessionStorage.removeItem("PageSize"); 
            sessionStorage.setItem("PageSize", parseInt(data.paginationInfo.pageSize));
            for (index = 1; index <= data.paginationInfo.totalPages; index++) {
                var liClass = '';
                if (index === data.paginationInfo.currentPage)
                    liClass = 'page-item active';
                else
                    liClass = 'page-item';
                var li = $('<li class="' + liClass + '"><a class="page-link" href="javascript:void(0);" onclick="invokeServerToGetData(' + index + ');">' + index + '</a></li>');
                $('#paginationUl').append(li);
            }
        },
        error: function (jqXHR, exception) {
            //debugger;
            if (jqXHR.status === 401) {
                window.location.href = "Login.html";
            }
            else {
                prepareAndRenderErrorDiv(jqXHR.statusText);
            }
        },
        complete: function () {
            $('#spanLoadingInfo').remove();
        }
    });
}

function prepareAndRenderErrorDiv(errorResponse) {
    $('#errorDiv').empty();
    $('#errorDiv').addClass('hidden');
    $('#errorDiv').removeClass('hidden');
    var innerDiv = $('<div id="errorDivInner" class="border-danger m-4 p-2"><button type="button" class="close"><span aria-hidden="true" onClick="closeValidationDiv();">&times;</span</button></div>');
    $('#errorDiv').append(innerDiv);
    $('#errorDivInner').append('<div class="border m-6 p-2">' + JSON.stringify(errorResponse) + '</div>');
}

function closeValidationDiv() {
    $('#errorDiv').empty();
    $('#errorDiv').addClass('hidden');
}

function deleteDevice(deviceId) {
    $.ajax({
        url: '/api/devices/delete/' + deviceId,
        method: 'DELETE',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        success: function () {
            window.location.href = "Index.html";
        },
        error: function (error) {
            prepareAndRenderErrorDiv(error);
        }
    });
}

function renderSwitch(status, deviceId) {
    var innerHtml = '';
    if (status === true) {
        innerHtml = '<div class="custom-control custom-switch">'
            + '<input type="checkbox" onclick="onSliderChange(this);" class="custom-control-input" checked id="customSwitch' + deviceId + '">'
            + '<label class="custom-control-label" id="customSwitchLabel' + deviceId + '" for="customSwitch' + deviceId + '">Enabled</label></div >';
    }
    else {
        innerHtml = '<div class="custom-control custom-switch">'
            + '<input type="checkbox" onclick="onSliderChange(this);" class="custom-control-input" id="customSwitch' + deviceId + '">'
            + '<label class="custom-control-label" id="customSwitchLabel' + deviceId + '" for="customSwitch' + deviceId + '">Disabled</label></div >';
    }
    return innerHtml;
}

function onSliderChange(checkBox) {
    //debugger;
    var newStatus = '';
    var revertStatus = '';

    var checkBoxId = $(checkBox).attr('id');
    var deviceId = checkBoxId.replace('customSwitch', '');

    var currentStatusText = $('#customSwitchLabel' + deviceId).text();
    var newStatusText = '';
    if (currentStatusText.toLowerCase() === "enabled")
        newStatusText = "Disabled";
    else if (currentStatusText.toLowerCase() === "disabled")
        newStatusText = "Enabled";

    if (!checkBox.checked) {
        newStatus = 'false';
        revertStatus = 'true';
    }
    else {
        $('#' + checkBoxId).prop('checked', true);
        newStatus = 'true';
        revertStatus = 'false';
    }

    var object = [{ op: "replace", path: "enabled", value: newStatus }];
    $.ajax({
        url: '/api/devices/updateStatus/' + deviceId,
        method: 'PATCH',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        contentType: 'application/json',
        processData: false,
        data: JSON.stringify(object),
        beforeSend: function () {
            $('#' + checkBoxId).attr('disabled', true);
        },
        complete: function () {
            $('#' + checkBoxId).attr('disabled', false);
        },
        success: function () {
            $('#customSwitchLabel' + deviceId).text(newStatusText);
        },
        error: function (error) {
            prepareAndRenderErrorDiv(error);
            $('#' + checkBoxId).removeProp('checked');
        }
    });
}
