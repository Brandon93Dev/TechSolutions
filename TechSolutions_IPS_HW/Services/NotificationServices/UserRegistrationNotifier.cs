using Microsoft.AspNetCore.Identity;
using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;
using TechSolutions_IPS_HW.Services.MailerServices;

namespace TechSolutions_IPS_HW.Services.NotificationServices
{
    /// <summary>
    /// Handles notifying all approvers gwhen a new user registers on the system
    /// Sends approval-request emails and creates in-app <see cref="AdminNotification"/> records.
    /// </summary>
    public class UserRegistrationNotifier : IUserRegistrationNotifier
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly EmailQueue _emailQueue;
        private readonly IAdminNotificationRepository _notificationRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRegistrationNotifier> _logger;

        public UserRegistrationNotifier(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, 
            EmailQueue emailQueue, 
            IAdminNotificationRepository notificationRepo,
            IConfiguration configuration, 
            ILogger<UserRegistrationNotifier> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailQueue = emailQueue;
            _notificationRepo = notificationRepo;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends notification emails and in-app notifications to approvers for each newly 
        /// registered user requiring approval
        /// </summary>   
        public async Task NotifyApproversAsync(
            IReadOnlyList<ApplicationUser> newUsers, 
            CancellationToken ct = default)
        {
            if (newUsers == null || newUsers.Count == 0) return;

            //Getting or setting roles responsibvle for approving new user logins
            var approverRoles = _configuration.GetSection("ApproverRoles").Get<string[]>() ?? new[] { "Administrator", "Management" };
            var baseUrl = _configuration["App:BaseUrl"] ?? _configuration["Jwt:Issuer"] ?? "";

            var pendingNotifications = new List<AdminNotification>();

            foreach (var newUser in newUsers)
            {
                foreach (var role in approverRoles)
                {
                    var approvers = await _userManager.GetUsersInRoleAsync(role);
                    foreach (var approver in approvers)
                    {
                        try
                        {
                            await SendApprovalEmailAsync(approver, newUser, baseUrl);

                            pendingNotifications.Add(new AdminNotification
                            {
                                RecipientUserId = approver.Id,
                                SubjectUserId = newUser.Id,
                                SubjectUserEmail = newUser.Email ?? newUser.UserName,
                                Message = $"New user {newUser.Email} has registered and requires approval.",
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex, 
                                "Failed to queue notification for approver {ApproverId} about new user {UserId}", 
                                approver.Id, 
                                newUser.Id);
                        }
                    }
                }
            }

            if (pendingNotifications.Count > 0)
            {
                try
                {
                    await _notificationRepo.AddRangeAsync(pendingNotifications, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, 
                        "Failed to persist {Count} in-app notifications",
                        pendingNotifications.Count);
                }
            }
        }

        private async Task SendApprovalEmailAsync(
            ApplicationUser approver,
            ApplicationUser newUser, 
            string baseUrl)
        {
            var approveUrl = $"{baseUrl}/admin/employees/approve?userId={newUser.Id}";
            var denyUrl = $"{baseUrl}/admin/employees/deny?userId={newUser.Id}";
            var body = EmailTemplates.NewUserAwaitingApproval(
                approver.Email ?? approver.UserName ?? "Approver",
                newUser.Email,
                approveUrl,
                denyUrl);

            try
            {
                await _emailQueue.EnqueueAsync(new Models.Email.EmailMessage
                {
                    To = approver.Email ?? approver.UserName ?? string.Empty,
                    Subject = "New user awaiting approval",
                    Body = body
                });
            }
            catch
            {
                await _emailSender.SendEmailAsync(
                    approver.Email ?? approver.UserName ?? string.Empty,
                    "New user awaiting approval",
                    body);
            }
        }
    }
}
