using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Services.Interfaces;
using TechSolutions_IPS_HW.Services.MailerServices;

namespace TechSolutions_IPS_HW.Services.CustomerServices;

public class CustomerEmailService : ICustomerEmailService
{
    private const string DevCustomerRedirectAddress = "info@synsoft.co.za";

    private readonly EmailQueue _emailQueue;
    private readonly IWebHostEnvironment _environment;

    public CustomerEmailService(EmailQueue emailQueue, IWebHostEnvironment environment)
    {
        _emailQueue = emailQueue;
        _environment = environment;
    }

    //Function that sends an email to a specified customer, as a safety measure, if the environment is development,
    //it redirects the email to a predefined address and adds a notice to the email body.
    public async Task<(bool Redirected, string EffectiveRecipient)>
        SendCustomerEmailAsync(
        Customer customer,
        string subject,
        string bodyHtml)
    {
        var redirected = _environment.IsDevelopment();
        var toAddress = redirected ? DevCustomerRedirectAddress : customer.Email;

        var finalBody = redirected
            ? EmailTemplates.AddDevRedirectNotice(bodyHtml, customer.Email)
            : bodyHtml;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = toAddress,
            Subject = subject,
            Body = finalBody
        });

        return (redirected, toAddress);
    }
}
