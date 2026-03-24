$(function () {
    'use strict';

    var empotyGuid = '00000000-0000-0000-0000-000000000000';
    //amount of ms delay after performing email duplicate check,t this prevents constant calls while user is busy typing
    var emailRefreshDelay = 2000;

    var emailCheckTimer = null;
    var isEmailDuplicate = false;

    function normalizeValue(value) {
        if (value === undefined || value === null) {
            return '';
        }

        return value.toString().trim();
    }

    function sanitizePhoneValue(value) {
        var raw = '';
        if (value !== undefined && value !== null) {
            raw = value.toString();
        }

        var trimmed = raw.trim();
        var hasLeadingPlus = trimmed.charAt(0) === '+';
        var digitsOnly = raw.replace(/\D/g, '');

        if (hasLeadingPlus) {
            return '+' + digitsOnly;
        }

        return digitsOnly;
    }


    //client side validation
    function isEmailFormatValid(email) {
        return /^\S+@\S+\.\S+$/.test(email);
    }

    function setEmailError(message) {
        $('#Email').addClass('is-invalid');
        $('[data-field="Email"]').text(message);
    }

    function clearEmailError() {
        $('#Email').removeClass('is-invalid');
        $('[data-field="Email"]').text('');
    }

    function validatePhoneField() {
        var $phone = $('#Phone');
        var phone = normalizeValue($phone.val());

        $phone.removeClass('is-invalid');
        $('[data-field="Phone"]').text('');

        if (!phone) return true;

        if (!/^\+?\d+$/.test(phone)) {
            $phone.addClass('is-invalid');
            $('[data-field="Phone"]').text('Phone number can only contain numbers and an optional leading +.');
            return false;
        }

        return true;
    }

    function getFormState() {
        return JSON.stringify({
            firstName: normalizeValue($('#FirstName').val()),
            surname: normalizeValue($('#Surname').val()),
            nationality: normalizeValue($('#Nationality').val()),
            email: normalizeValue($('#Email').val()),
            phone: normalizeValue($('#Phone').val()),
            dateOfBirth: normalizeValue($('#DateOfBirth').val()),
            addressLine1: normalizeValue($('#AddressLine1').val()),
            addressLine2: normalizeValue($('#AddressLine2').val()),
            city: normalizeValue($('#City').val()),
            provinceState: normalizeValue($('#ProvinceState').val()),
            addressCountry: normalizeValue($('#AddressCountry').val()),
            areaCode: normalizeValue($('#AreaCode').val()),
            idNumber: normalizeValue($('#IdNumber').val()),
            gender: normalizeValue($('#Gender').val()),
            dataSource: normalizeValue($('#DataSource').val()),
            notes: normalizeValue($('#Notes').val())
        });
    }

    var initialFormState = getFormState();

    function getSelectedDialCode() {
        var selectedCountry = $('#Nationality').val();
        if (!selectedCountry) return null;

        var dialCode = $('#Nationality option:selected').attr('data-dial-code');
        return dialCode ? dialCode.trim() : null;
    }

    function tryPrefillPhoneFromCountry() {
        var $phone = $('#Phone');
        if (!$phone.length) return;

        var existingPhone = normalizeValue($phone.val());
        if (existingPhone.length > 0) return;

        var dialCode = getSelectedDialCode();
        if (!dialCode) return;

        $phone.val(dialCode + ' ');
    }

    function checkEmailDuplicate(callback) {
        var email = normalizeValue($('#Email').val());
        var customerId = $('#CustomerId').val();

        clearEmailError();
        isEmailDuplicate = false;

        if (!email || !isEmailFormatValid(email)) {
            callback(false);
            return;
        }

        $.getJSON('/Customer/CheckEmail', { email: email, customerId: customerId })
            .done(function (result) {
                isEmailDuplicate = !!(result && result.exists);
                if (isEmailDuplicate) {
                    setEmailError('A customer with this email already exists.');
                }
                callback(isEmailDuplicate);
            })
            .fail(function () {
                callback(false);
            });
    }

    function scheduleEmailDuplicateCheck() {
        if (emailCheckTimer) {
            clearTimeout(emailCheckTimer);
        }

        emailCheckTimer = setTimeout(function () {
            checkEmailDuplicate(function () { });
        }, emailRefreshDelay);
    }

    $('#Nationality').on('change', tryPrefillPhoneFromCountry);

    $('#Email').on('input', function () {
        clearEmailError();
        scheduleEmailDuplicateCheck();
    });

    $('#Email').on('blur', function () {
        if (emailCheckTimer) {
            clearTimeout(emailCheckTimer);
            emailCheckTimer = null;
        }
        checkEmailDuplicate(function () { });
    });

    $('#Phone').on('input', function () {
        var current = $(this).val();
        var sanitized = sanitizePhoneValue(current);
        if (current !== sanitized) {
            $(this).val(sanitized);
        }
        validatePhoneField();
    });

    $('#btnCancel').on('click', function (e) {
        if (getFormState() === initialFormState) return;

        //prevents user from accidentally losing changes or additonas made to the form
        var leavePage = window.confirm('You have unsaved changes. Leave this page and lose pending changes?');
        if (!leavePage) {
            e.preventDefault();
        }
    });

    // Save buttons
    $('#btnSaveDraft').on('click', function () { submitForm('draft'); });
    $('#btnSaveActive').on('click', function () { submitForm('active'); });

    function submitForm(action) {
        clearErrors();

        if (!validatePhoneField()) {
            return;
        }

        checkEmailDuplicate(function (exists) {
            if (exists || isEmailDuplicate) {
                return;
            }

            var customerId = $('#CustomerId').val();
            var isNew = customerId === empotyGuid;

            var payload = {
                customerId: customerId,
                firstName: $('#FirstName').val(),
                surname: $('#Surname').val(),
                nationality: $('#Nationality').val() || null,
                email: $('#Email').val(),
                phone: $('#Phone').val() || null,
                dateOfBirth: $('#DateOfBirth').val() || null,
                addressLine1: $('#AddressLine1').val() || null,
                addressLine2: $('#AddressLine2').val() || null,
                city: $('#City').val() || null,
                provinceState: $('#ProvinceState').val() || null,
                addressCountry: $('#AddressCountry').val() || null,
                areaCode: $('#AreaCode').val() || null,
                idNumber: $('#IdNumber').val() || null,
                gender: $('#Gender').val() || null,
                dataSource: $('#DataSource').val() || null,
                notes: $('#Notes').val() || null,
                submitAction: action
            };

            var url = isNew ? '/Customer/Create' : '/Customer/Update';

            $('#btnSaveDraft, #btnSaveActive').prop('disabled', true);


            //posting form with new or existing customer info
            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload)
            })
                .done(function (result) {
                    if (result.success) {
                        sessionStorage.setItem('customerStatusMessage', result.message);
                        window.location.href = '/Customer';
                    }
                })
                .fail(function (xhr) {
                    var resp = xhr.responseJSON;
                    if (resp && resp.errors) {
                        showErrors(resp.errors);
                    } else {
                        $('#formErrors').removeClass('d-none').text('An unexpected error occurred. Please try again.');
                    }
                })
                .always(function () {
                    $('#btnSaveDraft, #btnSaveActive').prop('disabled', false);
                });
        });
    }

    // Error display
    function clearErrors() {
        $('#formErrors').addClass('d-none').text('');
        $('#customerForm .is-invalid').removeClass('is-invalid');
        $('#customerForm .invalid-feedback').text('');
    }

    function showErrors(errors) {
        var general = [];

        $.each(errors, function (field, message) {
            var msg = Array.isArray(message) ? message.join(' ') : message;
            var $input = $('#' + field);
            var $feedback = $('[data-field="' + field + '"]');

            if ($input.length && $feedback.length) {
                $input.addClass('is-invalid');
                $feedback.text(msg);
            } else {
                general.push(msg);
            }
        });

        if (general.length) {
            $('#formErrors').removeClass('d-none').html(general.join('<br/>'));
        }
    }
        
    var pendingMessage = sessionStorage.getItem('customerStatusMessage');
    if (pendingMessage) {
        sessionStorage.removeItem('customerStatusMessage');
    }
});
