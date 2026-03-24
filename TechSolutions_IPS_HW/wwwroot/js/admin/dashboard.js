$(function () {
    'use strict';

    // set chart instnace to null for reloads so it does not pesrist old data, 
    // and reloads on command
    var countryChartInstance = null;

    function loadAdminDashboard() {
        $.getJSON('/admin/dashboard/data')
            .done(function (data) {
                var totalEmployees = data.totalEmployees;
                var approvedCount = data.approvedCount;
                var pendingCount = data.pendingCount;
                var employees = data.employees;              

                // now that data is loaded, show the elemetns that are previously hidden
                $('#employeeOverviewRow').removeClass('d-none');
                $('#employeeTableSection').removeClass('d-none');

                // use vars set text values
                $('#metricTotal').text(totalEmployees);
                $('#metricApproved').text(approvedCount);
                $('#metricPending').text(pendingCount);
                $('#employeeCount').text(totalEmployees);
                $('#metricMyApprovedEmployees').text(data.approvedByCurrentUserCount);

                renderCountryChart(data.customersByCountry);
                renderEmployeeTable(employees);
            })
            .fail(
                //safe backoff if something happened to data or db items were not populated or not retrieved
                function () {
                $('#employeeTableContainer').html(
                    '<div class="alert alert-danger m-3">' +
                        '<i class="fa-solid fa-triangle-exclamation me-1"></i>Failed to load dashboard data.' +
                    '</div>'
                );
            });
    }

    function renderCountryChart(countryData) {
        var canvas = $('#countryChart');
        if (!canvas || typeof Chart === 'undefined') return;

        var labels = [];
        var counts = [];

        for (var i = 0; i < countryData.length; i++) {
            var item = countryData[i];
            labels.push(item.country != null ? item.country : item.Country);
            counts.push(item.count != null ? item.count : item.Count);
        }

        if (countryChartInstance) {
            countryChartInstance.destroy();
        }

        // load char lib and use built in chart functionality
        countryChartInstance = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Customers',
                    data: counts,
                    backgroundColor: 'rgba(13,110,253,0.55)',
                    borderColor: 'rgba(13,110,253,1)',
                    borderWidth: 1,
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    }
                },
                plugins: {
                    // we only show customer data, no legend need
                    legend: { display: false }
                }
            }
        });
    }

    function renderEmployeeTable(employees) {
        if (!employees || employees.length === 0) {
            //Fallbak iof no employee data loaded
            $('#employeeTableContainer').html(
                '<div class="alert alert-info m-3">' +
                    '<i class="fa-solid fa-circle-info me-1"></i>No employees found yet.' +
                '</div>'
            );
            return;
        }

        var html =
            '<div class="table-responsive">' +
            '<table class="table table-striped table-hover align-middle mb-0">' +
            '<thead class="table-dark"><tr>' +
            '<th>Display Name</th><th>Email</th><th>Status</th>' +
            '<th>Approved By</th><th>Approved At</th>' +
            '</tr></thead><tbody>';

        $.each(employees, function (_, emp) {
            var isApproved = false;
            // approved and non approved to determine what hbadge to show
            if (emp.isApproved !== undefined) isApproved = emp.isApproved;
            else if (emp.IsApproved !== undefined) isApproved = emp.IsApproved;

            var displayName = emp.displayName;
            if (displayName === undefined || displayName === null) displayName = emp.DisplayName;

            var email = emp.email;
            if (email === undefined || email === null) email = emp.Email;

            var approvedBy = emp.approvedBy;
            if (approvedBy === undefined || approvedBy === null) approvedBy = emp.ApprovedBy;

            var approvedAt = emp.approvedAt;
            if (approvedAt === undefined || approvedAt === null) approvedAt = emp.ApprovedAt;

            var badge = isApproved
                ? '<span class="badge bg-success"><i class="fa-solid fa-circle-check me-1"></i>Approved</span>'
                : '<span class="badge bg-warning text-dark"><i class="fa-solid fa-hourglass-half me-1"></i>Pending</span>';

            html +=
                '<tr>' +
                '<td>' + htmlEncode(displayName) + '</td>' +
                '<td>' + htmlEncode(email) + '</td>' +
                '<td>' + badge + '</td>' +
                '<td>' + htmlEncode(approvedBy) + '</td>' +
                '<td>' + htmlEncode(approvedAt) + '</td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        $('#employeeTableContainer').html(html);
    }

    loadAdminDashboard();
});
