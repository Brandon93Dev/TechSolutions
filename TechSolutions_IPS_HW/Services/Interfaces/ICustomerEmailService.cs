using TechSolutions_IPS_HW.Models.Customer;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface ICustomerEmailService
{
    Task<(bool Redirected, string EffectiveRecipient)> SendCustomerEmailAsync(
        Customer customer, string subject, string bodyHtml);
}
