using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Services.Interfaces;
using TechSolutions_IPS_HW.Services.MailerServices;

namespace TechSolutions_IPS_HW.Controllers
{
    [Authorize(Roles = "Administrator,Management", Policy = "CanApproveUsers")]
    [Route("admin/employees")]
    public class AdminEmployeesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly EmailQueue _emailQueue;
        private readonly IAdminNotificationService _notificationService;

        public AdminEmployeesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService,
            EmailQueue emailQueue,
            IAdminNotificationService notificationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _emailQueue = emailQueue;
            _notificationService = notificationService;
        }

        [HttpGet("")]
        public IActionResult Index() => View();

        /// <summary>
        /// Retrieves a list of employees with optional filtering by approval status.
        /// </summary>
        /// <remarks>The returned list is ordered by display name. If an employee's email, approver, or
        /// approval date is not available, a default value is provided in the result. This endpoint is accessible via
        /// an HTTP GET request to 'list'.</remarks>
        /// <param name="status">An optional status filter applied to the results. Specify "pending" to return only employees who are not
        /// approved, or "approved" to return only approved employees. If null or any other value, all employees are
        /// returned.</param>
        /// <returns>A JSON result containing a list of employees, each with their ID, display name, email, approval status,
        /// approver, and approval date.</returns>
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string? status)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var pendingEmployees = _userManager.Users.Where(a => !a.IsApproved).ToList();

            foreach (var u in pendingEmployees)           
                employees.Add(u);           

            IEnumerable<ApplicationUser> query = employees;

            if (string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => !u.IsApproved);
            else if (string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsApproved);

            var users = query
                .OrderBy(u => u.DisplayName)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayName,
                    Email = u.Email ?? string.Empty,
                    u.IsApproved,
                    ApprovedBy = u.ApprovedBy ?? "—",
                    ApprovedAt = u.ApprovedAt.HasValue
                        ? u.ApprovedAt.Value.ToString("dd MMM yyyy HH:mm")
                        : "—"
                })
                .ToList();

            return Json(users);
        }


        //Approve a new employee registration request
        [HttpGet("approve")]
        public async Task<IActionResult> Approve(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsApproved = true;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = User.Identity?.Name ?? "system";

            await _userManager.UpdateAsync(user);

            await EnsureEmployeeRoleExistsAsync();
            if (!await _userManager.IsInRoleAsync(user, "Employee"))
                await _userManager.AddToRoleAsync(user, "Employee");

            await _auditService.LogAsync(user.Id, "UserApproved", details: "Approved via admin link");

            await _notificationService.MarkAllAsReadBySubjectUserAsync(userId);

            var body = $"<p>Your account ({user.Email}) has been approved. And you can now log in to TechSolutions</p>";
            await _emailQueue.EnqueueAsync(new Models.Email.EmailMessage
            {
                To = user.Email ?? user.UserName ?? string.Empty,
                Subject = "Account approved",
                Body = body
            });

            TempData["StatusMessage"] = $"User {user.Email} has been approved. And you can now log in to TechSolutions";
            return RedirectToAction("Index", "Home");
        }


        //Regject or deny a new employee registration request
        [HttpGet("deny")]
        public async Task<IActionResult> Deny(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsApproved = false;
            user.ApprovedAt = null;
            user.ApprovedBy = User.Identity?.Name ?? "system";
            await _userManager.UpdateAsync(user);

            if (await _userManager.IsInRoleAsync(user, "Employee"))
                await _userManager.RemoveFromRoleAsync(user, "Employee");

            await _auditService.LogAsync(user.Id, "UserDenied", details: "Denied via admin link");

            await _notificationService.MarkAllAsReadBySubjectUserAsync(userId);

            var body = $"<p>Your account ({user.Email}) has been denied.</p>";
            await _emailQueue.EnqueueAsync(new Models.Email.EmailMessage
            {
                To = user.Email ?? user.UserName ?? string.Empty,
                Subject = "Account denied",
                Body = body
            });

            TempData["StatusMessage"] = $"User {user.Email} has been denied.";
            return RedirectToAction("Index", "Home");
        }


        //Ajax functionality if external login or admin approved user from email link
        [HttpPost("approve-ajax")]
        public async Task<IActionResult> ApproveAjax([FromBody] UserActionRequest request)
        {
            if (string.IsNullOrEmpty(request?.UserId)) return BadRequest(new { error = "User id required." });
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound(new { error = "User not found." });

            user.IsApproved = true;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = User.Identity?.Name ?? "system";
            await _userManager.UpdateAsync(user);

            await EnsureEmployeeRoleExistsAsync();
            if (!await _userManager.IsInRoleAsync(user, "Employee"))
                await _userManager.AddToRoleAsync(user, "Employee");

            await _auditService.LogAsync(user.Id, "UserApproved", details: "Approved via admin panel");

            await _notificationService.MarkAllAsReadBySubjectUserAsync(request.UserId);

            var body = $"<p>Your account ({user.Email}) has been approved.</p>";
            await _emailQueue.EnqueueAsync(new Models.Email.EmailMessage
            {
                To = user.Email ?? user.UserName ?? string.Empty,
                Subject = "Account approved",
                Body = body
            });

            return Ok(new { success = true, message = $"{user.Email} approved." });
        }

        //Ajax functionality if external login or admin denied user from email link
        [HttpPost("deny-ajax")]
        public async Task<IActionResult> DenyAjax([FromBody] UserActionRequest request)
        {
            if (string.IsNullOrEmpty(request?.UserId)) return BadRequest(new { error = "User id required." });
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound(new { error = "User not found." });

            user.IsApproved = false;
            user.ApprovedAt = null;
            user.ApprovedBy = User.Identity?.Name ?? "system";
            await _userManager.UpdateAsync(user);

            if (await _userManager.IsInRoleAsync(user, "Employee"))
                await _userManager.RemoveFromRoleAsync(user, "Employee");

            await _auditService.LogAsync(user.Id, "UserDenied", details: "Denied via admin panel");

            await _notificationService.MarkAllAsReadBySubjectUserAsync(request.UserId);

            var body = $"<p>Your account ({user.Email}) has been denied.</p>";
            await _emailQueue.EnqueueAsync(new Models.Email.EmailMessage
            {
                To = user.Email ?? user.UserName ?? string.Empty,
                Subject = "Account denied",
                Body = body
            });

            return Ok(new { success = true, message = $"{user.Email} denied." });
        }

        private async Task EnsureEmployeeRoleExistsAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Employee"))            
                await _roleManager.CreateAsync(new IdentityRole("Employee"));            
        }

        public class UserActionRequest
        {
            public string UserId { get; set; } = string.Empty;
        }
    }
}
