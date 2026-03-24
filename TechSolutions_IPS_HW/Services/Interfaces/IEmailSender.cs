namespace TechSolutions_IPS_HW.Services.Interfaces;

/// <summary>
/// Defines method signatures for sending email messages asynchronously using an HTML-formatted body.
/// </summary>
public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}
