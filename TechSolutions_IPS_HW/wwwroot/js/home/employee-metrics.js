$(function () {
    'use strict';

    $.getJSON('/admin/dashboard/data')
        .done(function (data) {
            var totalCustomers = data.totalCustomers;
            var myCreated = data.customersCreatedByCurrentUser;
            var myActive = data.activeCustomers;
            var myDraft = data.draftCustomers;

            $('#homeMetricTotalCustomers').text(totalCustomers);
            $('#homeMetricMyCreated').text(myCreated);
            $('#homeMetricMyActive').text(myActive);
            $('#homeMetricMyDraft').text(myDraft);

            var last = data.lastCreatedCustomer;
            if (!last) {
                $('#homeLastCreatedCustomer').html('<span class="text-muted">No customers created by your account yet.</span>');
                return;
            }

            var statusBadge = '<span class="badge bg-warning text-dark ms-2">Draft</span>';
            if (last.status === 'Active') {
                statusBadge = '<span class="badge bg-success ms-2">Active</span>';
            }

            var html =
                '<div class="fw-semibold mb-1">Last customer you created ' + statusBadge + '</div>' +
                '<div><a class="text-decoration-none" href="/Customer/Details/' + last.customerId + '">' + htmlEncode(last.fullName) + '</a></div>' +
                '<div class="small text-muted"><i class="fa-regular fa-envelope me-1"></i>' + htmlEncode(last.email) + '</div>' +
                '<div class="small text-muted"><i class="fa-solid fa-location-dot me-1"></i>' + htmlEncode(last.location) + '</div>' +
                '<div class="small text-muted"><i class="fa-regular fa-clock me-1"></i>' + htmlEncode(last.createdAt) + '</div>';

            $('#homeLastCreatedCustomer').html(html);
        })
        .fail(function () {
            $('#homeLastCreatedCustomer').html('<span class="text-danger">Failed to load employee metrics.</span>');
        });
});
