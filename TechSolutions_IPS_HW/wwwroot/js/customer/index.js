$(function () {
    'use strict';

    var currentFilter = '';
    var searchTerm = '';

    var deleteModal = new bootstrap.Modal($('#deleteModal')[0]);
    var deleteTargetId = null;

    var canManageCustomers = ($('#customerPermissions').data('can-manage') + '') === 'true';
    var isPrivilegedViewer = ($('#customerPermissions').data('is-privileged') + '') === 'true';

    var emailModal = new bootstrap.Modal($('#emailModal')[0]);
    var emailTargetCustomerId = null;
    var emailTemplates = [];

    var createdBySelect = null;
    var countrySelect = null;
    var table = null;


    // generating and randomising color backgrounds for each customer, 
    // as we do not store images to represent or show customers
    var avatarColors = [
        '#4e79a7', '#f28e2b', '#e15759', '#76b7b2',
        '#59a14f', '#edc948', '#b07aa1', '#ff9da7',
        '#9c755f', '#bab0ac', '#6b5b95', '#d64161'
    ];

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

    function initFilterSelects() {
        if (!isPrivilegedViewer || typeof TomSelect === 'undefined') return;

        // Use TomSelect jquery library to load a drodown select for country and employee
        if (!createdBySelect) {
            createdBySelect = new TomSelect('#filterCreatedBy', {
                create: false,
                searchField: ['text'],
                sortField: { field: 'text', direction: 'asc' },
                maxOptions: 300,
                placeholder: 'Select employee...'
            });
            createdBySelect.on('change', function () { reloadTable(); });
        }

        if (!countrySelect) {
            countrySelect = new TomSelect('#filterCountry', {
                create: false,
                searchField: ['text'],
                sortField: { field: 'text', direction: 'asc' },
                maxOptions: 300,
                placeholder: 'Select country...'
            });
            countrySelect.on('change', function () { reloadTable(); });
        }
    }

    function applyFilterOptions(filterOptions) {
        if (!isPrivilegedViewer || !createdBySelect || !countrySelect || !filterOptions) return;

        var selectedCreatedBy = createdBySelect.getValue();
        var selectedCountry = countrySelect.getValue();

        createdBySelect.clearOptions();

        createdBySelect.addOption({ value: '', text: 'Select employee...' });
        (filterOptions.creators || filterOptions.Creators || []).forEach(function (x) {
            createdBySelect.addOption({ value: x, text: x });
        });

        countrySelect.clearOptions();

        countrySelect.addOption({ value: '', text: 'Select country...' });
        (filterOptions.countries || filterOptions.Countries || []).forEach(function (x) {
            countrySelect.addOption({ value: x, text: x });
        });

        createdBySelect.refreshOptions(false);
        countrySelect.refreshOptions(false);

        createdBySelect.setValue(selectedCreatedBy || '', true);
        countrySelect.setValue(selectedCountry || '', true);
    }

    function updateMetrics(metrics) {
        var m = metrics || {};

        var total = 0;
        if (m.total !== undefined && m.total !== null) total = m.total;
        else if (m.Total !== undefined && m.Total !== null) total = m.Total;

        var active = 0;
        if (m.active !== undefined && m.active !== null) active = m.active;
        else if (m.Active !== undefined && m.Active !== null) active = m.Active;

        var draft = 0;
        if (m.draft !== undefined && m.draft !== null) draft = m.draft;
        else if (m.Draft !== undefined && m.Draft !== null) draft = m.Draft;

        $('#metricTotal').text(total);
        $('#metricActive').text(active);
        $('#metricDraft').text(draft);
    }

    function getApiParams(data) {
        var pageSize = data.length || 10;
        var page = Math.floor((data.start || 0) / pageSize) + 1;

        return {
            page: page,
            pageSize: pageSize,
            status: currentFilter || null,
            search: searchTerm || null,
            createdBy: createdBySelect ? (createdBySelect.getValue() || null) : ($('#filterCreatedBy').val() || null),
            country: countrySelect ? (countrySelect.getValue() || null) : ($('#filterCountry').val() || null),
            createdFrom: $('#filterCreatedFrom').val() || null,
            createdTo: $('#filterCreatedTo').val() || null
        };
    }

    function buildActions(c) {
        var emailBtn =
            '<button class="btn btn-sm btn-light me-1 btn-email" title="Email" data-id="' + c.customerId + '" data-name="' + attrEncode(c.fullName) + '" data-email="' + attrEncode(c.email) + '">' +
                '<i class="fa-regular fa-envelope"></i></button>';

        var viewBtn =
            '<a href="/Customer/Details/' + c.customerId + '" class="btn btn-sm btn-light me-1" title="View">' +
                '<i class="fa-solid fa-eye"></i></a>';

        if (!canManageCustomers) return viewBtn + emailBtn;

        var editBtn =
            '<a href="/Customer/Edit/' + c.customerId + '" class="btn btn-sm btn-light me-1" title="Edit">' +
                '<i class="fa-solid fa-pen"></i></a>';

        var deleteBtn =
            '<button class="btn btn-sm btn-light text-danger btn-delete" data-id="' + c.customerId + '" data-name="' + attrEncode(c.fullName) + '" title="Delete">' +
                '<i class="fa-solid fa-trash"></i></button>';

        return viewBtn + emailBtn + editBtn + deleteBtn;
    }

    function initTable() {
        var columns = [
            {
                data: null,
                render: function (_, __, c) {
                    var initials = getInitials(c.firstName, c.surname);
                    var color = avatarColors[Math.abs(hashCode(c.fullName)) % avatarColors.length];
                    return '<div class="d-flex align-items-center gap-3">' +
                        '<div class="customer-avatar" style="background-color:' + color + '">' + htmlEncode(initials) + '</div>' +
                        '<div><a href="/Customer/Details/' + c.customerId + '" class="customer-name-link">' + htmlEncode(c.fullName) + '</a>' +
                        (c.dataSource ? '<div class="text-muted small"><i class="fa-solid fa-building me-1"></i>' + htmlEncode(c.dataSource) + '</div>' : '') +
                        '</div></div>';
                }
            },
            {
                data: null,
                render: function (_, __, c) {
                    return '<div><i class="fa-regular fa-envelope me-1 text-muted"></i>' + htmlEncode(c.email) + '</div>' +
                        (c.phone ? '<div class="small"><i class="fa-solid fa-phone me-1 text-muted"></i>' + htmlEncode(c.phone) + '</div>' : '');
                }
            },
            {
                data: 'status',
                render: function (s) {
                    var active = s === 'Active';
                    return '<span class="customer-status-pill ' + (active ? 'status-active' : 'status-draft') + '">' +
                        '<span class="customer-status-dot"></span>' + (active ? 'Active' : 'Draft') +
                        '</span>';
                }
            },
            { data: 'createdAt', className: 'text-muted' }
        ];

        if (isPrivilegedViewer) {
            columns.push({
                data: 'createdBy',
                className: 'text-muted',
                defaultContent: '—'
            });
        }

        columns.push({
            data: null,
            orderable: false,
            className: 'text-end customer-actions',
            render: function (_, __, c) {
                return buildActions(c);
            }
        });


        // populate datatable as we retrieve data from backend and do dynamic filtering and magination
        // Datatable is quite useful for that
        table = $('#customerTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            ordering: false,
            lengthMenu: [10, 25, 50],
            pageLength: 10,
            ajax: function (data, callback) {
                $.getJSON('/Customer/List', getApiParams(data))
                    .done(function (resp) {
                        var items = resp.items || resp.Items || [];
                        var totalCount = resp.totalCount || resp.TotalCount || 0;

                        updateMetrics(resp.metrics || resp.Metrics);
                        applyFilterOptions(resp.filterOptions || resp.FilterOptions);

                        callback({
                            draw: data.draw,
                            recordsTotal: totalCount,
                            recordsFiltered: totalCount,
                            data: items
                        });
                    })
                    .fail(function () {
                        callback({ draw: data.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    });
            },
            columns: columns
        });
    }

    function reloadTable() {
        if (table) table.ajax.reload();
    }

    $(document).on('click', '.customer-metric-card', function () {
        $('.customer-metric-card').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).data('status') || '';
        reloadTable();
    });

    var searchTimer;
    $('#customerSearch').on('input', function () {
        var input = $(this);
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
            searchTerm = input.val().trim();
            reloadTable();
        }, 250);
    });

    if (isPrivilegedViewer) {
        initFilterSelects();

        $('#filterCreatedFrom, #filterCreatedTo').on('change input', function () {
            reloadTable();
        });

        $('#btnClearAdvancedFilters').on('click', function () {
            if (createdBySelect) createdBySelect.setValue('', true); else $('#filterCreatedBy').val('');
            if (countrySelect) countrySelect.setValue('', true); else $('#filterCountry').val('');
            $('#filterCreatedFrom').val('');
            $('#filterCreatedTo').val('');
            reloadTable();
        });
    }

    $(document).on('click', '.btn-email', function () {
        emailTargetCustomerId = $(this).data('id');

        var name = $(this).data('name') || '';
        var email = $(this).data('email') || '';

        $('#emailTargetLine').text('Customer: ' + name + ' (' + email + ')');
        $('#emailTemplate').empty();
        $('#emailSubject').val('');
        $('#emailBodyHtml').val('');

        renderEmailPreview();

        $.ajax({
            url: '/Customer/EmailTemplates',
            type: 'GET',
            data: { customerId: emailTargetCustomerId }
        })
        .done(function (result) {
            var templates = [];
            if (result && result.templates) {
                templates = result.templates;
            }

            emailTemplates = templates;
            populateEmailTemplates(emailTemplates);
            emailModal.show();
        })
        .fail(function (xhr) {
            var messageText = 'Failed to load email templates.';
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                messageText = xhr.responseJSON.message;
            }

            showStatusModal(messageText, true);
        });
    });

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

    $('#btnSendCustomerEmail').on('click', function () {
        if (!emailTargetCustomerId) return;
        var $btn = $(this).prop('disabled', true);

        var subject = ($('#emailSubject').val() || '').trim();
        var bodyHtml = ($('#emailBodyHtml').val() || '').trim();

        if (!subject || !bodyHtml) {
            showStatusModal('Please provide both subject and email content.', true);
            $btn.prop('disabled', false);
            return;
        }

        $.ajax({
            url: '/Customer/SendEmail',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ customerId: emailTargetCustomerId, subject: subject, bodyHtml: bodyHtml })
        })
        .done(function (result) {
            emailModal.hide();
            showStatusModal(result.message || 'Email queued.');
        })
        .fail(function (xhr) {
            var messageText = 'Failed to send email.';
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                messageText = xhr.responseJSON.message;
            }

            showStatusModal(messageText, true);
        })
        .always(function () {
            $btn.prop('disabled', false);
        });
    });

    $(document).on('click', '.btn-delete', function () {
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
            deleteModal.hide();
            reloadTable();
            showStatusModal(result.message);
        })
        .fail(function () {
            deleteModal.hide();
            showStatusModal('Failed to delete customer.', true);
        })
        .always(function () {
            $btn.prop('disabled', false);
            deleteTargetId = null;
        });
    });

    $('#btnNewCustomer').on('click', function (e) {
        e.preventDefault();
        window.location.href = '/Customer/Edit/00000000-0000-0000-0000-000000000000';
    });

    function getInitials(first, last) {
        return ((first || '').charAt(0) + (last || '').charAt(0)).toUpperCase();
    }

    function hashCode(str) {
        var hash = 0;
        for (var i = 0; i < (str || '').length; i++) {
            hash = ((hash << 5) - hash) + str.charCodeAt(i);
            hash |= 0;
        }
        return hash;
    }

    function showStatusModal(message, isError) {
        var $existing = $('#statusMessageModal');
        if ($existing.length) $existing.remove();

        var cls = isError ? 'bg-danger' : 'bg-success';
        var title = isError ? 'Error' : 'Action Completed';
        var btnCls = isError ? 'danger' : 'success';

        var html =
            '<div class="modal fade" id="statusMessageModal" tabindex="-1" aria-hidden="true">' +
                '<div class="modal-dialog modal-dialog-centered"><div class="modal-content">' +
                    '<div class="modal-header ' + cls + ' text-white">' +
                        '<h5 class="modal-title">' + title + '</h5>' +
                        '<button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>' +
                    '</div>' +
                    '<div class="modal-body"><p>' + htmlEncode(message) + '</p></div>' +
                    '<div class="modal-footer">' +
                        '<button type="button" class="btn btn-' + btnCls + '" data-bs-dismiss="modal">OK</button>' +
                    '</div>' +
                '</div></div>' +
            '</div>';

        $('body').append(html);
        var modal = new bootstrap.Modal($('#statusMessageModal')[0]);
        modal.show();
        $('#statusMessageModal').on('hidden.bs.modal', function () { $(this).remove(); });
    }

    initTable();

    var pendingMsg = sessionStorage.getItem('customerStatusMessage');
    if (pendingMsg) {
        sessionStorage.removeItem('customerStatusMessage');
        showStatusModal(pendingMsg);
    }
});
