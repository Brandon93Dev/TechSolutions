$(function () {
    'use strict';

    var currentFilter = 'pending';

    $('#statusFilter a').on('click', function (e) {
        e.preventDefault();
        $('#statusFilter a').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).data('status');
        loadEmployees();
    });

    function loadEmployees() {
        var url = '/admin/employees/list';
        if (currentFilter) {
            url += '?status=' + encodeURIComponent(currentFilter);
        }

        // Add loading spinner so content cant be interacted witrh while loading
        $('#employeesTableContainer').html(
            '<div class="text-center py-4">' +
                '<div class="spinner-border text-primary" role="status">' +
                    '<span class="visually-hidden">Loading...</span>' +
                '</div>' +
            '</div>'
        );

        $.getJSON(url)
            .done(function (data) {
                renderTable(data);
            })
            .fail(function () {
                //Safety fallback of no data is returnned or error occurs
                $('#employeesTableContainer').html(
                    '<div class="alert alert-danger m-3">' +
                        '<i class="fa-solid fa-triangle-exclamation me-1"></i>Failed to load employees.' +
                    '</div>'
                );
            });
    }

    function renderTable(users) {
        // no users? fallback to safe html to not render an empty table
        if (!users || users.length === 0) {
            $('#employeesTableContainer').html(
                '<div class="alert alert-info m-3">' +
                    '<i class="fa-solid fa-circle-info me-1"></i>No employees found.' +
                '</div>'
            );
            return;
        }

        var html =
            '<div class="table-responsive">' +
            '<table class="table table-striped table-hover align-middle mb-0">' +
                '<thead class="table-dark"><tr>' +
                    '<th>Display Name</th><th>Email</th><th>Status</th>' +
                    '<th>Approved By</th><th>Approved At</th><th></th>' +
                '</tr></thead><tbody>';

        $.each(users, function (_, u) {
            var badge = '';
            if (u.isApproved) {
                badge = '<span class="badge bg-success"><i class="fa-solid fa-circle-check me-1"></i>Approved</span>';
            } else {
                badge = '<span class="badge bg-warning text-dark"><i class="fa-solid fa-hourglass-half me-1"></i>Pending</span>';
            }

            var actions = '';
            if (!u.isApproved) {
                actions =
                    '<button class="btn btn-sm btn-success me-1 btn-approve" data-user-id="' + u.id + '">' +
                        '<i class="fa-solid fa-circle-check me-1"></i>Approve' +
                    '</button>' +
                    '<button class="btn btn-sm btn-danger btn-deny" data-user-id="' + u.id + '">' +
                        '<i class="fa-solid fa-circle-xmark me-1"></i>Deny' +
                    '</button>';
            }

            html +=
                '<tr data-user-id="' + u.id + '">' +
                    '<td>' + htmlEncode(u.displayName) + '</td>' +
                    '<td>' + htmlEncode(u.email) + '</td>' +
                    '<td>' + badge + '</td>' +
                    '<td>' + htmlEncode(u.approvedBy) + '</td>' +
                    '<td>' + htmlEncode(u.approvedAt) + '</td>' +
                    '<td class="text-nowrap">' + actions + '</td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        $('#employeesTableContainer').html(html);
    }

    $(document).on('click', '.btn-approve', function () {
         ('/admin/employees/approve-ajax', $(this));
    });

    $(document).on('click', '.btn-deny', function () {
        performAction('/admin/employees/deny-ajax', $(this));
    });

    function performAction(url, $btn) {
        var userId = $btn.data('user-id');
        var $row = $btn.closest('tr');
        $row.find('button').prop('disabled', true);

        $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId })
        })
        .done(function (result) {
            showStatusModal(result.message || 'Action completed.');
            loadEmployees();
        })
        .fail(function () {
            alert('Action failed. Please try again.');
            $row.find('button').prop('disabled', false);
        });
    }

    function showStatusModal(message) {
        $('#statusModalMessage').text(message);
        var modal = new bootstrap.Modal(document.getElementById('statusModal'));
        modal.show();
    }

    loadEmployees();
});
