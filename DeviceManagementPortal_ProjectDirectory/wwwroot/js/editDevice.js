
var mappedBackends = [];
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

                loadDataFromServer();
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

function loadDataFromServer() {
    var deviceId = document.location.href.split('?')[1].replace('id=', '');
    $.ajax({
        url: '/api/devices/get/' + deviceId,
        method: 'GET',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        beforeSend: function () {
            //$('#btnEditFormSubmit').addClass('disabled');
            $('#btnModalTrigger').attr('disabled', true);
        },
        success: function (data) {
            //debugger;
            if (data != null) {
                $('#hiddenFieldDeviceId').val(data.deviceID);
                $('#spanWaitInfo').remove();
                $('#btnModalTrigger').attr('disabled', false);
                $('#textBoxDeviceImei').val(data.imei);
                $('#textBoxModel').val(data.model);
                $('#textBoxSimCardNumber').val(data.simCardNumber);
                $('#selectEnabled option[value=' + data.enabled + ']').attr('selected', true);
                $('#textBoxDeviceCreatedBy').val(data.createdBy);
                $('#textBoxDeviceCreatedOn').val(data.createdDateTime);

                if (data.deviceBackends != null && data.deviceBackends.length > 0) {
                    for (var index = 0; index < data.deviceBackends.length; index++) {
                        if (data.deviceBackends[index].backend != null) {
                            mappedBackends.push({ backendID: parseInt(data.deviceBackends[index].backend.backendID), name: data.deviceBackends[index].backend.name });
                        }
                    }

                    var pageSize = sessionStorage.getItem("PageSize");
                    renderMappingData(1, parseInt(pageSize));
                }
                bindEventHandlers();
            }
            else {
                $('#spanWaitInfo').remove();
                $('#btnModalTrigger').attr('disabled', true);
                $('#btnEditFormSubmit').addClass('disabled');
                prepareAndRenderErrorDiv('editActionErrorDiv', 'Device information not received from server.');
            }
        },
        error: function (jqXHR, exception) {
            $('#spanWaitInfo').remove();
            $('#btnModalTrigger').attr('disabled', true);
            $('#btnEditFormSubmit').removeClass('disabled');
            prepareAndRenderErrorDiv('editActionErrorDiv', 'An error occured. Please try again later. Error response from server: ' + jqXHR.responseText);
        }
    });
}

function bindEventHandlers() {
    $('#editForm').submit(function (event) {
        //debugger;
        event.preventDefault();
        resetValidations();
        var proceed = enforceValidations();
        if (proceed) {
            var formData = {
                deviceId: $('#hiddenFieldDeviceId').val(),
                model: $('#textBoxModel').val(),
                simCardNumber: $('#textBoxSimCardNumber').val(),
                enabled: $('#selectEnabled').val(),
                backends: mappedBackends
            };
            sendDataToServer(JSON.stringify(formData));
        }
    });
    $('#textBoxModel').keyup(function () {
        closeValidationDiv('#deviceModelValidationDiv');
    });
    $('#textBoxSimCardNumber').inputFilter(function (value) {
        return /^-?\d*$/.test(value);
    });
    $('#textBoxSimCardNumber').keyup(function () {
        closeValidationDiv('#deviceSimCardNumberValidationDiv');
    });
    $('#modalPopup').on('show.bs.modal', loadModalWithData); 
}

(function ($) {
    $.fn.inputFilter = function (inputFilter) {
        return this.on("input keydown keyup mousedown mouseup select contextmenu drop", function () {
            if (inputFilter(this.value)) {
                this.oldValue = this.value;
                this.oldSelectionStart = this.selectionStart;
                this.oldSelectionEnd = this.selectionEnd;
            } else if (this.hasOwnProperty("oldValue")) {
                this.value = this.oldValue;
                this.setSelectionRange(this.oldSelectionStart, this.oldSelectionEnd);
            } else {
                this.value = "";
            }
        });
    };
}(jQuery));

function resetValidations() {
    resetValidationDiv('#deviceModelValidationDiv');
    resetValidationDiv('#deviceSimCardNumberValidationDiv');
    resetValidationDiv('#editActionErrorDiv');
}

function resetValidationDiv(divId) {
    $(divId).empty();
    if (!($(divId).hasClass('hidden')))
        $(divId).addClass('hidden');
}

function enforceValidations() {
    //debugger;
    var proceed = false;
    var statusModel = enforceTextValidation('textBoxModel', 'deviceModelValidationDiv', 50, 'Model cannot be blank.', 'Model cannot be more than 50 characters long.');
    var statusSimCardNumber = enforceTextValidation('textBoxSimCardNumber', 'deviceSimCardNumberValidationDiv', 20, 'SIM card number cannot be blank.', 'SIM card number cannot be more than 20 characters long.');
    proceed = (statusModel && statusSimCardNumber);
    return proceed;
}

function enforceTextValidation(inputControlId, validationDivId, maxCharLimit, blankValidationMessage, upperLimitExceededValidtionMessage) {
    //debugger;
    var status = true;
    var valueProvidedByUser = $('#' + inputControlId).val();
    if (valueProvidedByUser === '') {
        status = false;
        prepareAndRenderErrorDiv(validationDivId, blankValidationMessage);
    }
    else if (valueProvidedByUser !== '' && valueProvidedByUser.length > maxCharLimit) {
        status = false;
        prepareAndRenderErrorDiv(validationDivId, upperLimitExceededValidtionMessage);
    }
    return status;
}

function prepareAndRenderErrorDiv(validationDivId, validationMessage) {
    //debugger;
    $('#' + validationDivId).removeClass('hidden');
    var innerDivId = validationDivId + 'ErrorDivInner';
    var innerDiv = $('<div id="' + innerDivId + '" class="border border-danger p-2"><button type="button" class="close"><span aria-hidden="true" onClick="closeValidationDiv(' + validationDivId + ');">&times;</span</button></div>');
    $('#' + validationDivId).append(innerDiv);
    $('#' + innerDivId).append('<div class="text-danger">' + validationMessage + '</div>');
}

function closeValidationDiv(validationDivId) {
    $(validationDivId).empty();
    $(validationDivId).addClass('hidden');
}

function sendDataToServer(serializedFormData) {
    $.ajax({
        url: '/api/devices/edit',
        type: 'POST',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        contentType: 'application/json',
        data: serializedFormData,
        beforeSend: function () {
            $('#btnEditFormSubmit').addClass('disabled');
            $('#btnCancel').after('<span id="spanWaitInfo" class="text-info"><i>&nbsp;&nbsp;Please wait, saving data...</i></span>');
        },
        success: function () {
            $('#spanWaitInfo').remove();
            $('#btnEditFormSubmit').removeClass('disabled');
            window.location.href = "Index.html";
        },
        error: function (jqXHR, exception) {
            $('#spanWaitInfo').remove();
            $('#btnEditFormSubmit').removeClass('disabled');
            prepareAndRenderErrorDiv('editActionErrorDiv', 'An error occured. Please try again later. Error response from server: ' + jqXHR.responseText);
        }
    });
}

function loadModalWithData() {
    $.ajax({
        url: '/api/backends/all',
        type: 'GET',
        headers: {
            "Authorization": "Bearer " + sessionStorage.getItem('JWT_Token')
        },
        success: function (data) {
            //debugger;
            $('#modalInnerBody').empty();
            if (data != null && data.length > 0) {
                $('#modalInnerBody').empty();
                var dropdownItemCount = 0;
                var dropdown = '<div class="form-group"><label><b>Backends</b></label><select name="backends" id="selectBackends" class="form-control">';
                for (var indexOuter = 0; indexOuter < data.length; indexOuter++) {
                    if (mappedBackends.length > 0) {
                        var moveToNextItem = false;
                        for (var indexInner = 0; indexInner < mappedBackends.length; indexInner++) {
                            if (mappedBackends[indexInner].backendID === data[indexOuter].backendID) {
                                moveToNextItem = true;
                                break;
                            }
                        }
                        if (moveToNextItem === true) {
                            continue;
                        }
                        dropdown += '<option value="' + data[indexOuter].backendID + '">' + data[indexOuter].name + '</option>';
                        dropdownItemCount++;
                    }
                    else {
                        dropdown += '<option value="' + data[indexOuter].backendID + '">' + data[indexOuter].name + '</option>';
                        dropdownItemCount++;
                    }
                }
                dropdown += '</select></div>';
                if (dropdownItemCount === 0) {
                    $('#btnModalMapAndClose').remove();
                }
                $('#modalInnerBody').append(dropdown);
            }
            else {
                $('#modalInnerBody').html('<p>No backend(s) found.</p>');
                $('#btnModalMapAndClose').remove();
            }
        },
        error: function (jqXHR, exception) {
            $('#modalInnerBody').empty();
            $('#modalInnerBody').text('Error occurred. Cannot load data.');
            $('#btnModalMapAndClose').remove();
        }
    });
}

function mapAndCloseModal() {
    //debugger;
    var selectedBackend = $('#selectBackends');
    $('#modalPopup').modal('hide');

    mappedBackends.push({ backendID: parseInt(selectedBackend.val()), name: selectedBackend.find(':selected').text() });
    var pageSize = sessionStorage.getItem("PageSize");
    renderMappingData(1, parseInt(pageSize));
}

function renderMappingData(currentPageNumber, pageSize) {
    //debugger;
    $('#tableBody').empty();
    $('#tablePaging').empty();
    if (mappedBackends.length > 0) {
        var totalPageCount = calculateTotalPagesRequired(mappedBackends.length, pageSize);
        var startIndex = (currentPageNumber * pageSize) - pageSize;
        var imeiTextBoxValue = $('#textBoxDeviceImei').val();
        var rowCounter = 0;
        var positionTracker = startIndex;
        for (var index = startIndex; index <= ((currentPageNumber * pageSize) - 1); index++) {
            if (positionTracker < mappedBackends.length) {
                var backendName = mappedBackends[index].name;
                var backendId = mappedBackends[index].backendID;
                var row = $('<tr class="text-center"><td>' + imeiTextBoxValue + '</td><td>' + backendName + '</td><td><a class="btn btn-link p-0" onclick="updateMappedBackends(' + backendId + ');return false;" href="">Delete</a></td></tr>');
                $('#tableBody').append(row);
                positionTracker++;
                rowCounter++;
            }
        }
        if (mappedBackends.length > rowCounter) {
            var pagingHtml = '<ul class="pagination">';
            for (var index = 1; index <= totalPageCount; index++) {
                var liClass = '';
                if (index === currentPageNumber)
                    liClass = 'page-item active';
                else
                    liClass = 'page-item';
                pagingHtml += '<li class="' + liClass + '"><a class="page-link" href="javascript:void(0);" onclick="renderMappingData(' + index + ', ' + pageSize + ');">' + index + '</a></li>';
            }
            pagingHtml += '</ul>';
            $('#tablePaging').html(pagingHtml);
        }
    }
    else {
        $('#tableBody').append('<tr><td colspan="3" class="text-center text-info"><i>No mapping(s)</i></td></tr>');
    }
}

function calculateTotalPagesRequired(totalItems, pageSize) {
    return Math.ceil(totalItems / pageSize);
}

function updateMappedBackends(backendIDToRemove) {
    //debugger;
    var mappedBackendsLocal = [];
    for (var index = 0; index < mappedBackends.length; index++) {
        if (mappedBackends[index].backendID !== backendIDToRemove) {
            mappedBackendsLocal.push(mappedBackends[index]);
        }
    }
    mappedBackends = mappedBackendsLocal;
    var pageSize = sessionStorage.getItem("PageSize");
    renderMappingData(1, parseInt(pageSize));
}