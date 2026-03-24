using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Audit;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.AuditServices;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditService(
        ApplicationDbContext db, 
        IHttpContextAccessor httpContextAccessor, 
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    //When a action is performed on an entity, the initial snapshot of the entity is taken and stored, along with the information on modifications that
    //was made to that user, it is then stored in the database, this allows us to have a history of changes for each entity, and also to know who made the change and when it was made.
    public async Task LogAsync(
        string subjectUserId, 
        string action, 
        string? details = null, 
        Guid? customerId = null)
    {
        var performerIdentityId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Create or reuse actor snapshots
        var subjectActor = await GetOrCreateActorAsync(subjectUserId);
        var performerActor = performerIdentityId != null
            ? await GetOrCreateActorAsync(performerIdentityId)
            : null;

        var entry = new AuditEntry
        {
            Action = action,
            SubjectActorId = subjectActor?.Id,
            PerformerActorId = performerActor?.Id,
            CustomerId = customerId,
            Details = details,
            TimestampUtc = DateTime.UtcNow
        };

        _db.AuditEntries.Add(entry);
        await _db.SaveChangesAsync();
    }


    // This method is specifically for logging changes to customer entities,
    // it captures the old and new values of the customer in JSON format,
    // along with the type of change and the performer of the change.
    public async Task LogCustomerChangeAsync(
        Guid customerId, 
        string changeType, 
        string? oldValuesJson, 
        string? newValuesJson)
    {
        var performerIdentityId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var performerActor = await GetOrCreateActorAsync(performerIdentityId);

        var changeLog = new CustomerChangeLog
        {
            CustomerId = customerId,
            ChangeType = changeType,
            OldValues = oldValuesJson,
            NewValues = newValuesJson,
            PerformerActorId = performerActor?.Id,
            TimestampUtc = DateTime.UtcNow
        };

        _db.CustomerChangeLogs.Add(changeLog);
        await _db.SaveChangesAsync();
    }

    private async Task<AuditActor?> GetOrCreateActorAsync(string? identityUserId)
    {
        if (string.IsNullOrEmpty(identityUserId))
            return null;

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == identityUserId);

        var actor = new AuditActor
        {
            UserId = identityUserId,
            DisplayName = user?.DisplayName ?? "Unknown",
            Email = user?.Email ?? identityUserId,
            CapturedAtUtc = DateTime.UtcNow
        };

        _db.AuditActors.Add(actor);
        await _db.SaveChangesAsync();

        return actor;
    }
}
