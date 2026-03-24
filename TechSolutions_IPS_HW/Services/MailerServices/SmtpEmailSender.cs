using System.Net;
using System.Net.Mail;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.MailerServices;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly string _host;
    private readonly int _port;
    private readonly string? _user;
    private readonly string? _pass;
    private readonly bool _enableSsl;
    private readonly string _fromEmail;
    private readonly string _fromName;


    //Configuration for mail client expected here, pulled from appsettings.json.
    // For development, you can use a local SMTP server like Papercut or MailHog to capture outgoing emails without actually sending them.
    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
        _host = configuration["Smtp:Host"] ?? "localhost";
        _port = int.TryParse(configuration["Smtp:Port"], out var p) ? p : 25;
        _user = configuration["Smtp:User"];
        _pass = configuration["Smtp:Pass"];
        _enableSsl = bool.TryParse(configuration["Smtp:EnableSsl"], out var s) && s;
        _fromEmail = configuration["Smtp:FromEmail"] ?? "no-reply@example.com";
        _fromName = configuration["Smtp:FromName"] ?? "TechSolutions";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {            
        using var client = new SmtpClient(_host, _port)
        {
            //b3cause this is jsut dev we will not be configuring SSL on mail provider
            EnableSsl = _enableSsl
        };

        if (!string.IsNullOrEmpty(_user))
        {
            client.Credentials = new NetworkCredential(_user, _pass);
        }

        var msg = new MailMessage()
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        msg.To.Add(toEmail);

        await client.SendMailAsync(msg);
    }
}
