using System.Security.Claims;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface ICustomerAccessService
{
    bool IsPrivilegedUser(ClaimsPrincipal user);

    Task<bool> IsEmployeeBlockedFromCustomerAsync(ClaimsPrincipal user, Guid customerId);
    
    Task<HashSet<Guid>> GetAllowedCustomerIdsAsync(ClaimsPrincipal user);
}
