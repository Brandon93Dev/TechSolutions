using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSolutions_IPS_HW.Models.Customer; 
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Models.Requests;
using TechSolutions_IPS_HW.Security.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;
using Emails = TechSolutions_IPS_HW.Services.MailerServices;

namespace TechSolutions_IPS_HW.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;
    private readonly ICustomerReferenceDataService _referenceDataService;
    private readonly ICustomerAccessService _customerAccessService;
    private readonly ICustomerEmailService _customerEmailService;
    private readonly IEmployeeEmailTemplateService _employeeEmailTemplateService;
    private readonly IHtmlSanitizerService _htmlSanitizer;

    public CustomerController(
        ICustomerService customerService,
        IAuditService auditService,
        ICustomerReferenceDataService referenceDataService,
        ICustomerAccessService customerAccessService,
        ICustomerEmailService customerEmailService,
        IEmployeeEmailTemplateService employeeEmailTemplateService,
        IHtmlSanitizerService htmlSanitizer)
    {
        _customerService = customerService;
        _auditService = auditService;
        _referenceDataService = referenceDataService;
        _customerAccessService = customerAccessService;
        _customerEmailService = customerEmailService;
        _employeeEmailTemplateService = employeeEmailTemplateService;
        _htmlSanitizer = htmlSanitizer;
    }

    public IActionResult Index() => View();

    // This method serves both the creation and editing of customers.
    // If id is empty, it's a creation, otherwise it's an edit
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!User.IsInRole("Employee"))
            return Forbid();

        ViewData["CountryDialCodes"] = await _referenceDataService.GetCountryDialCodesAsync();

        if (id == Guid.Empty)
            return View(new Customer { CustomerId = Guid.Empty });

        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, id))
            return Forbid();

        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    // This is a read-only view for customer details,
    // accessible by both Employees and Management
    public async Task<IActionResult> Details(Guid id)
    {
        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, id))
            return Forbid();

        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }


    //Retrieve list of customers based on filter criteria, with pagination and metrics
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status, [FromQuery] string? search, [FromQuery] string? createdBy,
        [FromQuery] string? country, [FromQuery] DateTime? createdFrom, [FromQuery] DateTime? createdTo,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var customers = await _customerService.GetCustomersAsync(null);

        var isPrivileged = _customerAccessService.IsPrivilegedUser(User);

        if (User.IsInRole("Employee") && !isPrivileged)
        {
            var allowedCustomerIds = await _customerAccessService.GetAllowedCustomerIdsAsync(User);
            customers = customers.Where(c => allowedCustomerIds.Contains(c.CustomerId)).ToList();
        }

        var creatorMap = isPrivileged
            ? await _customerService.GetCustomerCreatorDisplayNamesAsync(customers.Select(c => c.CustomerId))
            : new Dictionary<Guid, string>();

        var filtered = customers;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLowerInvariant();
            filtered = filtered.Where(c =>
                (c.FullName ?? string.Empty).ToLower().Contains(q) ||
                (c.Email ?? string.Empty).ToLower().Contains(q) ||
                (c.Phone ?? string.Empty).ToLower().Contains(q) ||
                (c.DataSource ?? string.Empty).ToLower().Contains(q)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            var ctry = country.Trim().ToLowerInvariant();
            filtered = filtered.Where(c =>
                ((c.AddressCountry ?? string.Empty).ToLower().Contains(ctry)) ||
                ((c.Nationality ?? string.Empty).ToLower().Contains(ctry))).ToList();
        }

        if (createdFrom.HasValue)
        {
            var from = createdFrom.Value.Date;
            filtered = filtered.Where(c => c.CreatedAt.Date >= from).ToList();
        }

        if (createdTo.HasValue)
        {
            var to = createdTo.Value.Date;
            filtered = filtered.Where(c => c.CreatedAt.Date <= to).ToList();
        }

        if (!string.IsNullOrWhiteSpace(createdBy) && isPrivileged)
        {
            var creatorQ = createdBy.Trim().ToLowerInvariant();
            filtered = filtered.Where(c =>
            {
                if (!creatorMap.TryGetValue(c.CustomerId, out var creator)) return false;
                return (creator ?? string.Empty).ToLower().Contains(creatorQ);
            }).ToList();
        }

        var metrics = new
        {
            Total = filtered.Count,
            Active = filtered.Count(c => c.Status == CustomerStatus.Active),
            Draft = filtered.Count(c => c.Status == CustomerStatus.Draft)
        };

        if (Enum.TryParse<CustomerStatus>(status, true, out var parsed))
            filtered = filtered.Where(c => c.Status == parsed).ToList();

        var totalCount = filtered.Count;
        var paged = filtered
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = paged.Select(c => new
        {
            c.CustomerId,
            c.FirstName,
            c.Surname,
            c.FullName,
            c.Email,
            Phone = c.Phone ?? "",
            DataSource = c.DataSource ?? "",
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt.ToString("dd MMM yyyy"),
            UpdatedAt = c.UpdatedAt.ToString("dd MMM yyyy HH:mm"),
            c.AddressCountry,
            c.Nationality,
            CreatedBy = isPrivileged && creatorMap.TryGetValue(c.CustomerId, out var creatorName)
                ? creatorName
                : null
        });

        var filterOptions = new
        {
            Countries = customers
                .Select(c => !string.IsNullOrWhiteSpace(c.AddressCountry) ? c.AddressCountry! : c.Nationality)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList(),
            Creators = isPrivileged
                ? creatorMap.Values
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()
                : new List<string>()
        };

        return Json(new
        {
            Items = result,
            TotalCount = totalCount,
            Metrics = metrics,
            FilterOptions = filterOptions
        });
    }


    //Gets detailed information of a specific customer by ID, with access control check
    [HttpGet]
    public async Task<IActionResult> Get(Guid id)
    {
        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, id))
            return Forbid();

        var c = await _customerService.GetByIdAsync(id);
        if (c == null) return NotFound();

        return Json(new
        {
            c.CustomerId,
            c.FirstName,
            c.Surname,
            c.FullName,
            c.Nationality,
            c.Email,
            c.Phone,
            DateOfBirth = c.DateOfBirth?.ToString("yyyy-MM-dd"),
            c.AddressLine1,
            c.AddressLine2,
            c.City,
            c.ProvinceState,
            c.AddressCountry,
            c.AreaCode,
            c.IdNumber,
            c.Gender,
            c.DataSource,
            c.Notes,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt.ToString("dd MMM yyyy HH:mm"),
            UpdatedAt = c.UpdatedAt.ToString("dd MMM yyyy HH:mm")
        });
    }

    // Checks if an email address is already associated with another customer,
    // excluding the current customer for edits
    [HttpGet]
    public async Task<IActionResult> CheckEmail(
        [FromQuery] string? email, 
        [FromQuery] Guid? customerId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { success = false, message = "Email is required." });

        var exists = await _customerService.ExistsByEmailAsync(email, customerId);
        return Json(new { exists });
    }

    // Creates a customer with the provided form data, with validation and access control checks
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerFormDataModel form)
    {
        if (!User.IsInRole("Employee"))
            return StatusCode(
                StatusCodes.Status403Forbidden, 
                new { 
                    success = false, 
                    message = "Only employees can create customers." 
                });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = GetModelErrors() });

        var isActive = string.Equals(
            form.SubmitAction, "active", 
            StringComparison.OrdinalIgnoreCase);

        if (isActive && string.IsNullOrWhiteSpace(form.IdNumber))
            return BadRequest(new { 
                success = false, 
                errors = new { 
                    IdNumber = "ID Number is required to activate a customer." 
                } 
            });

        if (await _customerService.ExistsByEmailAsync(form.Email))
            return Conflict(new { 
                success = false, 
                errors = new { 
                    Email = "A customer with this email already exists." 
                } 
            });

        if (!string.IsNullOrWhiteSpace(form.IdNumber)
            && await _customerService.ExistsByIdNumberAsync(form.IdNumber))
            return Conflict(new { 
                success = false, 
                errors = new { 
                    IdNumber = "A customer with this ID number already exists." 
                }
            });

        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FirstName = form.FirstName,
            Surname = form.Surname,
            Nationality = form.Nationality,
            Email = form.Email,
            Phone = form.Phone,
            DateOfBirth = form.DateOfBirth,
            AddressLine1 = form.AddressLine1,
            AddressLine2 = form.AddressLine2,
            City = form.City,
            ProvinceState = form.ProvinceState,
            AddressCountry = form.AddressCountry,
            AreaCode = form.AreaCode,
            IdNumber = string.IsNullOrWhiteSpace(form.IdNumber) ? null : form.IdNumber.Trim(),
            Gender = form.Gender,
            DataSource = form.DataSource,
            Notes = form.Notes,
            Status = isActive ? CustomerStatus.Active : CustomerStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _customerService.AddAsync(customer);

        var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _auditService.LogAsync(
            identityId, 
            "CustomerCreated",
            details: $"Customer '{customer.FullName}' ({customer.CustomerId}) created as {customer.Status}",
            customerId: customer.CustomerId);

        return Json(
            new { 
                success = true, 
                message = $"Customer \"{customer.FullName}\" created as {customer.Status}.", 
                customerId = customer.CustomerId 
            });
    }

    //Updates existing customer with the provided form data, with validation and access control checks
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] CustomerFormDataModel form)
    {
        if (!User.IsInRole("Employee"))
            return StatusCode(
                StatusCodes.Status403Forbidden, 
                new { 
                    success = false, 
                    message = "Only employees can update customers." 
                });

        if (form.CustomerId == Guid.Empty)
            return BadRequest(new { 
                success = false,
                errors = new { 
                    CustomerId = "Invalid customer." 
                } 
            });

        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, form.CustomerId))
            return StatusCode(
                StatusCodes.Status403Forbidden, new { 
                    success = false, 
                    message = "You are not allowed to update this customer." 
                });

        if (!ModelState.IsValid)
            return BadRequest(new { 
                success = false, 
                errors = GetModelErrors() 
            });

        var customer = await _customerService.GetByIdAsync(form.CustomerId);
        if (customer == null) return NotFound(new { success = false });

        var isActive = string.Equals(
            form.SubmitAction, 
            "active", 
            StringComparison.OrdinalIgnoreCase);

        if (isActive && string.IsNullOrWhiteSpace(form.IdNumber))
            return BadRequest(new { 
                success = false,
                errors = new { 
                    IdNumber = "ID Number is required to activate a customer." 
                } 
            });

        if (await _customerService.ExistsByEmailAsync(form.Email, form.CustomerId))
            return Conflict(new { 
                success = false, 
                errors = new { 
                    Email = "A customer with this email already exists." 
                } 
            });

        var newIdNumber = string.IsNullOrWhiteSpace(form.IdNumber) ? null : form.IdNumber.Trim();
        if (newIdNumber != null && newIdNumber != customer.IdNumber
            && await _customerService.ExistsByIdNumberAsync(newIdNumber))
            return Conflict(new { 
                success = false, 
                errors = new { 
                    IdNumber = "A customer with this ID number already exists." 
                } 
            });

        //populate customer with form data
        customer.FirstName = form.FirstName;
        customer.Surname = form.Surname;
        customer.Nationality = form.Nationality;
        customer.Email = form.Email;
        customer.Phone = form.Phone;
        customer.DateOfBirth = form.DateOfBirth;
        customer.AddressLine1 = form.AddressLine1;
        customer.AddressLine2 = form.AddressLine2;
        customer.City = form.City;
        customer.ProvinceState = form.ProvinceState;
        customer.AddressCountry = form.AddressCountry;
        customer.AreaCode = form.AreaCode;
        customer.IdNumber = newIdNumber;
        customer.Gender = form.Gender;
        customer.DataSource = form.DataSource;
        customer.Notes = form.Notes;
        customer.Status = isActive ? CustomerStatus.Active : CustomerStatus.Draft;

        await _customerService.UpdateAsync(customer);

        var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        
        await _auditService.LogAsync(
            identityId, "CustomerUpdated",
            details: $"Customer '{customer.FullName}' updated (Status: {customer.Status})",
            customerId: customer.CustomerId);

        return Json(
            new { 
                success = true, 
                message = $"Customer \"{customer.FullName}\" updated." 
            });
    }

    //Soft deletes a customer by ID, with access control checks and audit logging
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest req)
    {
        if (!User.IsInRole("Employee"))
            return StatusCode(
                StatusCodes.Status403Forbidden, 
                new { 
                    success = false, 
                    message = "Only employees can delete customers." 
                }
            );

        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, req.Id))
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new {
                    success = false,
                    message = "You are not allowed to delete this customer."
                });

        var customer = await _customerService.GetByIdAsync(req.Id);
        if (customer == null) return NotFound(new { success = false });

        await _customerService.DeleteAsync(req.Id);

        var identityId = User.
            FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";

        await _auditService.LogAsync(
            identityId, "CustomerDeleted",
            details: $"Customer '{customer.FullName}' deleted",
            customerId: customer.CustomerId
        );

        return Json(new { success = true, message = $"Customer \"{customer.FullName}\" deleted." });
    }

    // Gets customer infop ald loads templates as well as employees custom email
    // templates to send emails to a client
    [HttpGet]
    [Authorize(Roles = "Employee,Management")]
    public async Task<IActionResult> EmailTemplates(Guid emailTargetCustomerId)
    {
        var customer = await _customerService.GetByIdAsync(emailTargetCustomerId);
        if (customer == null)
            return NotFound(new { success = false, message = "Customer not found." });

        if (await _customerAccessService
            .IsEmployeeBlockedFromCustomerAsync(User, emailTargetCustomerId))
            return StatusCode(StatusCodes.Status403Forbidden, new { 
                success = false,
                message = "You are not allowed to email this customer." 
            });

        var senderName = User.Identity?.Name;
        var templates = Emails.EmailTemplates.CustomerTemplates(customer.FullName, senderName)
            .Select(t => new
            {
                t.Key,
                t.Name,
                t.Subject,
                t.BodyHtml
            })
            .ToList();              

        //load the emplpyee's custom created templates
        if (User.IsInRole("Employee"))
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var employeeTemplates = await _employeeEmailTemplateService
                    .GetMyTemplatesAsync(userId);
                templates.AddRange(employeeTemplates.Select(t => new
                {
                    Key = $"employee-{t.Id}",
                    Name = $"My Template: {t.Name}",
                    t.Subject,
                    t.BodyHtml
                }));
            }
        }

        return Json(new { success = true, templates });
    }

    //Sends email to client from employee or manager
    [HttpPost]
    [Authorize(Roles = "Employee,Management")]
    public async Task<IActionResult> SendEmail([FromBody] SendCustomerEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = GetModelErrors() });

        var customer = await _customerService.GetByIdAsync(request.CustomerId);
        if (customer == null)
            return NotFound(new { success = false, message = "Customer not found." });

        if (await _customerAccessService.IsEmployeeBlockedFromCustomerAsync(User, request.CustomerId))
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "You are not allowed to email this customer." });

        var sanitizedBodyHtml = _htmlSanitizer.Sanitize(request.BodyHtml);
        if (string.IsNullOrWhiteSpace(sanitizedBodyHtml))
            return BadRequest(new { success = false, message = "Email content is empty after sanitization. Please revise your HTML." });

        var result = await _customerEmailService.SendCustomerEmailAsync(customer, request.Subject.Trim(), sanitizedBodyHtml);

        var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _auditService.LogAsync(identityId, "CustomerEmailQueued",
            details: $"Email queued for customer '{customer.FullName}'. Effective recipient: {result.EffectiveRecipient}",
            customerId: customer.CustomerId);

        var responseMessage = result.Redirected
            ? $"Email queued in development mode. Redirected to {result.EffectiveRecipient}."
            : $"Email queued for {customer.Email}.";

        return Json(new { success = true, message = responseMessage });
    }

    private Dictionary<string, string> GetModelErrors()
    {
        return ModelState
            .Where(kvp => kvp.Value!.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.First().ErrorMessage);
    }
}
