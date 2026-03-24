$(function(){
'use strict';

  var countryChartInstance = null;

function loadEmployeeDashboard(){
    $.getJSON('/employee/dashboard/data')
        .done(function (data) {

      $('#metricTotalCustomers').text(data.totalCustomers);
      $('#metricActiveCustomers').text(data.activeCustomers);
      $('#metricDraftCustomers').text(data.draftCustomers);
      $('#metricMyCreatedCustomers').text(data.customersCreatedByCurrentUser);

      renderLastCreatedCustomer(data.lastCreatedCustomer);
      renderCountryChart(data.customersByCountry);
    })
    .fail(function(){
      $('#lastCreatedCustomerCard').html('<div class="text-danger">Failed to load dashboard data.</div>');
    });
}

function renderLastCreatedCustomer(customer) {
  if (!customer){
    $('#lastCreatedCustomerCard').html('<div class="text-muted">No customer has been created by your account yet.</div>');
    return;
  }

  var status = customer.status;
  var fullName = customer.fullName;
  var email = customer.email;
  var location = customer.location;
  var createdAt = customer.createdAt;
  var customerId = customer.customerId;

  var statusBadge = '<span class="badge bg-warning-subtle text-warning border border-warning-subtle">Draft</span>';
  if (status === 'Active') {
    statusBadge = '<span class="badge bg-success-subtle text-success border border-success-subtle">Active</span>';
  }

  var initials = getInitials(fullName);

  var html =
    '<div class="employee-last-customer">' +
      '<div class="d-flex align-items-center gap-3 mb-3">' +
        '<div class="employee-last-avatar">' + htmlEncode(initials) + '</div>' +
        '<div class="flex-grow-1">' +
          '<div class="fw-semibold fs-5 mb-0">' + htmlEncode(fullName) + '</div>' +
          '<div class="small text-muted">Most recently created customer</div>' +
        '</div>' +
      '</div>' +
      '<div class="employee-last-meta">' +
        '<div class="small text-muted mb-2"><i class="fa-regular fa-envelope me-2"></i>' + htmlEncode(email) + '</div>' +
        '<div class="small text-muted mb-2"><i class="fa-solid fa-location-dot me-2"></i>' + htmlEncode(location) + '</div>' +
        '<div class="small text-muted mb-3"><i class="fa-regular fa-clock me-2"></i>' + htmlEncode(createdAt) + '</div>' +
      '</div>' +
      '<div class="d-flex align-items-center justify-content-between gap-2">' +
        statusBadge +
        '<a class="btn btn-sm btn-outline-primary" href="/Customer/Details/' + customerId + '">View customer</a>' +
      '</div>' +
    '</div>';

  $('#lastCreatedCustomerCard').html(html);
}

function getInitials(name){
  var parts = (name || '').trim().split(/\s+/).filter(Boolean);
  if (parts.length===0) return 'NA';
  if (parts.length===1) return parts[0].charAt(0).toUpperCase();
  return (parts[0].charAt(0) + parts[1].charAt(0)).toUpperCase();
}



function renderCountryChart(countryData){
  var canvas = $('#countryChart');
  if (!canvas || typeof Chart === 'undefined') return;

  var labels = countryData.map(function(x){ return x.country ?? x.Country; });
  var counts = countryData.map(function(x){ return x.count ?? x.Count; });

  if (countryChartInstance) countryChartInstance.destroy();


  // charrt library loaded and utilised here
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
      plugins: { legend: { display: false } }
    }
  });
}

loadEmployeeDashboard();
});