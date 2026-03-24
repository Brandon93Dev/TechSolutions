namespace TechSolutions_IPS_HW.Services.MailerServices;

public sealed record CustomerEmailTemplateDefinition(string Key, string Name, string Subject, string BodyHtml);

public static class EmailTemplates
{
    private const string CustomerTemplateFolder = "EmailTemplates/Customer";

    //Seriously nasty way of doing this, had to store the template strings somewhere though, id prefer to do this in a folder 
    //that holds and manages all mail templates, but this is a quick and dirty way to get it working for now
    //TODO: check to change user name to actual name, but persist functionaltiy for user to use email to log in
    public static string NewUserAwaitingApproval(string approverName, string userEmail, string approveUrl, string denyUrl)
    {
        return
            $"<p>Hi {approverName},</p>" +
            $"<p>A new user has registered with email <strong>{userEmail}</strong> and requires approval.</p>" +
            $"<p><a href=\"{approveUrl}\">Approve user</a> | <a href=\"{denyUrl}\">Deny user</a></p>" +
            "<p>Or visit the admin users page to manage registrations.</p>";
    }

    public static IReadOnlyList<CustomerEmailTemplateDefinition> CustomerTemplates(string customerName, string? sentBy)
    {
        var encodedCustomerName = System.Net.WebUtility.HtmlEncode(customerName);
        var senderLine = string.IsNullOrWhiteSpace(sentBy)
            ? string.Empty
            : $"<p style=\"margin-top:18px;\">Sent by: <strong>{System.Net.WebUtility.HtmlEncode(sentBy)}</strong></p>";

        return
        [
            new CustomerEmailTemplateDefinition(
                "general-update",
                "General Update",
                "Quick update from TechSolutions",
                RenderCustomerTemplate("general-update.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "document-request",
                "Document Request",
                "Additional information required",
                RenderCustomerTemplate("document-request.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "welcome",
                "Welcome Message",
                "Welcome to TechSolutions",
                RenderCustomerTemplate("welcome.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "limited-time-offer",
                "Limited-Time Offer",
                "Exclusive offer just for you",
                RenderCustomerTemplate("limited-time-offer.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "new-feature-announcement",
                "New Feature Announcement",
                "New features now available",
                RenderCustomerTemplate("new-feature-announcement.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "referral-program",
                "Referral Program",
                "Invite a colleague and unlock benefits",
                RenderCustomerTemplate("referral-program.html", encodedCustomerName, senderLine)
            ),
            new CustomerEmailTemplateDefinition(
                "seasonal-campaign",
                "Seasonal Campaign",
                "Seasonal highlights from TechSolutions",
                RenderCustomerTemplate("seasonal-campaign.html", encodedCustomerName, senderLine)
            )
        ];
    }

    public static string AddDevRedirectNotice(string bodyHtml, string redirectedOriginalRecipient)
    {
        var redirectNotice = $"<p style=\"color:#856404;background:#fff3cd;padding:10px;border-radius:6px;\"><strong>DEV REDIRECT:</strong> Intended recipient was {System.Net.WebUtility.HtmlEncode(redirectedOriginalRecipient)}.</p>";
        return redirectNotice + bodyHtml;
    }

    private static string RenderCustomerTemplate(string fileName, string encodedCustomerName, string senderLine)
    {
        var path = Path.Combine(AppContext.BaseDirectory, CustomerTemplateFolder, fileName);

        if (!File.Exists(path))
        {
            return "<div style=\"font-family:Segoe UI,Arial,sans-serif;line-height:1.5;color:#212529;\">" +
                   "<p>Email template could not be loaded.</p>" +
                   "</div>";
        }

        var html = File.ReadAllText(path);
        return html
            .Replace("{{CustomerName}}", encodedCustomerName, StringComparison.Ordinal)
            .Replace("{{SenderLine}}", senderLine, StringComparison.Ordinal);
    }
}
