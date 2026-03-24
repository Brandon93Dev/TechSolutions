namespace TechSolutions_IPS_HW.Security.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}
