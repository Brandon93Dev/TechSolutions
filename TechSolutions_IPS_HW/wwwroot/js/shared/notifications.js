$(function () {
  
    var $badge = $('#notificationBadge');
    var $bell = $('#notificationBell');

    if (!$bell.length) return;

    // Fetch unread count on page load
    function refreshBadge() {
        $.getJSON('/api/notifications/unread-count', function (data) {
            if (data.count > 0) {
                $badge.text(data.count).removeClass('d-none');
            } else {
                $badge.addClass('d-none');
            }
        });
    }

    refreshBadge();

    // Load notifications when modal is shown
    $('#notificationsModal').on('show.bs.modal', function () {
        var $loading = $('#notificationsLoading');
        var $empty = $('#notificationsEmpty');
        var $table = $('#notificationsTable');
        var $body = $('#notificationsBody');

        $loading.removeClass('d-none');
        $empty.addClass('d-none');
        $table.addClass('d-none');
        $body.empty();

        $.getJSON('/api/notifications/unread', function (data) {
            $loading.addClass('d-none');

            if (!data || data.length === 0) {
                $empty.removeClass('d-none');
                return;
            }

            $table.removeClass('d-none');

            for (var i = 0; i < data.length; i++) {
                var n = data[i];
                var date = new Date(n.createdAt).toLocaleString();
                var row = '<tr id="notif-row-' + n.id + '">' +
                    '<td>' + escapeHtml(n.subjectUserEmail || '') + '</td>' +
                    '<td>' + escapeHtml(n.message) + '</td>' +
                    '<td>' + date + '</td>' +
                    '<td class="text-nowrap">' +
                    '<button class="btn btn-sm btn-success me-1 btn-approve" data-user-id="' + n.subjectUserId + '" data-notif-id="' + n.id + '">Approve</button>' +
                    '<button class="btn btn-sm btn-danger btn-deny" data-user-id="' + n.subjectUserId + '" data-notif-id="' + n.id + '">Deny</button>' +
                    '</td></tr>';
                $body.append(row);
            }
        }).fail(function () {
            $loading.addClass('d-none');
            $empty.removeClass('d-none').find('p').text('Failed to load notifications.');
        });
    });

    // Approve employee registration from UI
    $(document).on('click', '.btn-approve', function () {
        var $btn = $(this);
        var userId = $btn.data('user-id');
        var notifId = $btn.data('notif-id');
        performAction('/admin/employees/approve-ajax', userId, notifId, $btn);
    });

    // Deny employee registration from UI
    $(document).on('click', '.btn-deny', function () {
        var $btn = $(this);
        var userId = $btn.data('user-id');
        var notifId = $btn.data('notif-id');
        performAction('/admin/employees/deny-ajax', userId, notifId, $btn);
    });

    function performAction(url, userId, notifId, $btn) {
        var $row = $('#notif-row-' + notifId);
        $row.find('button').prop('disabled', true);

        $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId }),
            success: function (resp) {
                $row.fadeOut(300, function () {
                    $(this).remove();
                    // If table is now empty, show empty state
                    if ($('#notificationsBody tr').length === 0) {
                        $('#notificationsTable').addClass('d-none');
                        $('#notificationsEmpty').removeClass('d-none');
                    }
                });
                refreshBadge();
            },
            error: function () {
                alert('Action failed. Please try again.');
                $row.find('button').prop('disabled', false);
            }
        });
    }
});
