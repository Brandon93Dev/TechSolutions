using System.Security.Claims;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.CustomerServices;

public class CustomerAccessService : ICustomerAccessService
{
    private readonly IAuditRepository _auditRepository;

    public CustomerAccessService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    //Determine 
    public bool IsPrivilegedUser(ClaimsPrincipal user)
        => user.IsInRole("Administrator") || user.IsInRole("Management");

    //Determine if current authenticated user is an employee and if they are blocked from
    //accessing the specified customer (customers are bound to employees and cannot be
    //accessed by other employees but can be accessed by privileged users)
    public async Task<bool> IsEmployeeBlockedFromCustomerAsync(
        ClaimsPrincipal user, 
        Guid customerId)
    {
        if (IsPrivilegedUser(user) || !user.IsInRole("Employee"))
            return false;

        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return true;

        var canAccess = await _auditRepository.
            HasCreatedCustomerAsync(customerId, currentUserId);

        return !canAccess;
    }

    //Get list of customers that can be accessed by employee
    public async Task<HashSet<Guid>> GetAllowedCustomerIdsAsync(ClaimsPrincipal user)
    {
        if (IsPrivilegedUser(user) || !user.IsInRole("Employee"))
            return new HashSet<Guid>();

        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return new HashSet<Guid>();

        var ids = await _auditRepository.
            GetCreatedCustomerIdsByPerformerAsync(currentUserId);

        return ids.ToHashSet();
    }
}
