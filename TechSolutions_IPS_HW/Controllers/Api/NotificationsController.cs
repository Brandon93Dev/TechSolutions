using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Controllers.Api
{

    // only managers and admins (currently) have this role
    [Authorize(Policy = "CanApproveUsers")]
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            IAdminNotificationService notificationService, 
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }


        /// <summary>
        /// Count of employee approvals that has not been actioned by an admin/manager
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var activeCount = await _notificationService
                .GetUnreadActiveCountAsync(userId);

            return Ok(new { count = activeCount });
        }

        /// <summary>
        /// Retrieves list of unread notifications (un-actioned notifications)
        /// </summary>
        /// <returns></returns>
        [HttpGet("unread")]
        public async Task<IActionResult> Unread()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var active = await _notificationService
                .GetUnreadActiveAsync(userId);

            return Ok(active.Select(n => new
            {
                n.Id,
                n.SubjectUserId,
                n.SubjectUserEmail,
                n.Message,
                n.CreatedAt
            }));
        }

        /// <summary>
        /// After a notifications has been actioned (approved/denied via email or UI), this is called
        /// </summary>
        /// <param name="id">Notification Id</param>     
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notification = await _notificationService
                .GetByIdForRecipientAsync(id, userId);

            if (notification == null) return NotFound();

            await _notificationService.MarkAsReadAsync(notification);
            return Ok();
        }
    }
}
