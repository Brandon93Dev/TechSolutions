$(function () {
    'use strict';

    var $deleteModalElement = $('#deleteModal');
    var deleteModal = $deleteModalElement.length ? new bootstrap.Modal($deleteModalElement[0]) : null;
    var deleteTargetId = null;

    var $emailModalElement = $('#emailModal');
    var emailModal = $emailModalElement.length ? new bootstrap.Modal($emailModalElement[0]) : null;
    var emailTemplates = [];

    function renderEmailPreview() {
        renderHtmlPreview('#emailPreview', $('#emailBodyHtml').val() || '');
    }

    function populateEmailTemplates(templates) {
        var $template = $('#emailTemplate');
        $template.empty();

        templates.forEach(function (item, index) {
            $template.append($('<option/>', {
                value: item.key,
                text: item.name,
                selected: index === 0
            }));
        });

        if (templates.length === 0) {
            $('#emailSubject').val('');
            $('#emailBodyHtml').val('');
            renderEmailPreview();
            return;
        }

        var firstTemplate = templates[0];
        $('#emailSubject').val(firstTemplate.subject || '');
        $('#emailBodyHtml').val(firstTemplate.bodyHtml || '');
        renderEmailPreview();
    }

    $('#emailTemplate').on('change', function () {
        var selectedKey = $(this).val();
        var selected = null;

        for (var i = 0; i < emailTemplates.length; i++) {
            if (emailTemplates[i].key === selectedKey) {
                selected = emailTemplates[i];
                break;
            }
        }

        if (!selected) {
            return;
        }

        $('#emailSubject').val(selected.subject || '');
        $('#emailBodyHtml').val(selected.bodyHtml || '');
        renderEmailPreview();
    });

    $('#emailBodyHtml').on('input', renderEmailPreview);

    $('#btnEmailCustomer').on('click', function () {
        var name = $(this).data('name') || '';
        var email = $(this).data('email') || '';
        var customerId = $(this).data('id');

        $('#emailTargetLine').text('Customer: ' + name + ' (' + email + ')');
        $('#emailTemplate').empty();
        $('#emailSubject').val('');
        $('#emailBodyHtml').val('');
        renderEmailPreview();

        $.ajax({
            url: '/Customer/GetEmailTemplates',
            type: 'GET',
            data: { customerId: customerId }
        })
        .done(function (result) {
            var templates = [];
            if (result && result.templates) {
                templates = result.templates;
            }

            emailTemplates = templates;
            populateEmailTemplates(emailTemplates);
            if (emailModal) emailModal.show();
        })
        .fail(function (xhr) {
            var messageText = 'Failed to load email templates. Please try again.';
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                messageText = xhr.responseJSON.message;
            }

            alert(messageText);
        });
    });

    $('#btnSendCustomerEmail').on('click', function () {
        var $btn = $(this).prop('disabled', true);
        var customerId = $('#btnEmailCustomer').data('id');
        var subject = ($('#emailSubject').val() || '').trim();
        var bodyHtml = ($('#emailBodyHtml').val() || '').trim();

        if (!subject || !bodyHtml) {
            alert('Please provide both subject and email content.');
            $btn.prop('disabled', false);
            return;
        }

        $.ajax({
            url: '/Customer/SendEmail',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                customerId: customerId,
                subject: subject,
                bodyHtml: bodyHtml
            })
        })
        .done(function (result) {
            if (emailModal) emailModal.hide();
            alert(result.message || 'Email queued successfully.');
        })
        .fail(function (xhr) {
            var messageText = 'Failed to send email. Please try again.';
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                messageText = xhr.responseJSON.message;
            }

            alert(messageText);
        })
        .always(function () {
            $btn.prop('disabled', false);
        });
    });

    $('#btnDelete').on('click', function () {
        if (!deleteModal) return;

        deleteTargetId = $(this).data('id');
        $('#deleteCustomerName').text($(this).data('name'));
        deleteModal.show();
    });

    $('#btnConfirmDelete').on('click', function () {
        if (!deleteTargetId) return;
        var $btn = $(this).prop('disabled', true);

        $.ajax({
            url: '/Customer/Delete',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: deleteTargetId })
        })
        .done(function (result) {
            if (deleteModal) deleteModal.hide();
            sessionStorage.setItem('customerStatusMessage', result.message);
            window.location.href = '/Customer';
        })
        .fail(function () {
            if (deleteModal) deleteModal.hide();
            alert('Failed to delete customer. Please try again.');
        })
        .always(function () {
            $btn.prop('disabled', false);
            deleteTargetId = null;
        });
    });
});
