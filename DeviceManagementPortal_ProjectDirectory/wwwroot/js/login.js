$(document).ready(function () {
    //debugger;
    if (document.location.href.split('?').length > 1) {
        var redirectUrl = document.location.href.split('?')[1].replace('redirectUrl=', '');
        if (redirectUrl !== '') {
            $('#hiddenFieldRedirectUrl').val(redirectUrl);
        }
    }
    bindEventHandlers();
});

function reset() {
    $('#textBoxUserName').val('');
    $('#textBoxPassword').val('');
}

function bindEventHandlers() {
    //debugger;
    $('#loginForm').submit(function (event) {
        //debugger;
        event.preventDefault();
        resetValidations();
        var proceed = enforceValidations();
        if (proceed) {
            var formData = {
                userName: $('#textBoxUserName').val(),
                password: $('#textBoxPassword').val(),
                redirectUrl: $('#hiddenFieldRedirectUrl').val(),
            };
            sendDataToServer(JSON.stringify(formData));
        }
    });
    $('#btnCancel').click(reset);
}

function resetValidations() {
    resetValidationDiv('#userNameValidationDiv');
    resetValidationDiv('#passwordValidationDiv');
}

function resetValidationDiv(divId) {
    $(divId).empty();
    if (!($(divId).hasClass('hidden')))
        $(divId).addClass('hidden');
}

function enforceValidations() {
    //debugger;
    var proceed = false;
    var statusUserName = enforceTextValidation('textBoxUserName', 'userNameValidationDiv', 5, 'User Name cannot be blank.', 'User Name cannot be less than 5 characters long.');
    var statusPassword = enforceTextValidation('textBoxPassword', 'passwordValidationDiv', 6, 'Password cannot be blank.', 'Password cannot be less than 6 characters long.');

    proceed = (statusUserName && statusPassword);
    return proceed;
}

function enforceTextValidation(inputControlId, validationDivId, minCharLimit, blankValidationMessage, upperLimitExceededValidtionMessage) {
    //debugger;
    var status = true;
    var valueProvidedByUser = $('#' + inputControlId).val();
    if (valueProvidedByUser === '') {
        status = false;
        prepareAndRenderErrorDiv(validationDivId, blankValidationMessage);
    }
    else if (valueProvidedByUser !== '' && valueProvidedByUser.length < minCharLimit) {
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
        url: '/api/accounts/token',
        type: 'POST',
        contentType: 'application/json',
        data: serializedFormData,
        beforeSend: function () {
            resetValidationDiv('#loginActionErrorDiv');
            $('#btnCancel').after('<span id="spanWaitInfo" class="text-info"><i>&nbsp;&nbsp;Please wait...</i></span>');
        },
        success: function (data) {
            //debugger;
            $('#spanWaitInfo').remove();
            if (data != null && data.token !== '') {
                sessionStorage.setItem('JWT_Token', data.token);
                window.location.href = "Index.html";
            }
            else {
                prepareAndRenderErrorDiv('loginActionErrorDiv', 'Error in authentication.');
            }
        },
        error: function (jqXHR, exception) {
            //debugger;
            $('#spanWaitInfo').remove();
            if (jqXHR.status === 401) {
                prepareAndRenderErrorDiv('loginActionErrorDiv', 'Authentication failed. Please provide proper credentials.');
            }
            else {
                prepareAndRenderErrorDiv('loginActionErrorDiv', 'Error occured, Error Message: ' + jqXHR.statusText);
            }
        }
    });
}