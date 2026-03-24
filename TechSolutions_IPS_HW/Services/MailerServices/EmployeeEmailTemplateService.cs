using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Models.ViewModels;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Security.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.MailerServices;

public class EmployeeEmailTemplateService : IEmployeeEmailTemplateService
{
    private readonly IEmployeeEmailTemplateRepository _repository;
    private readonly IHtmlSanitizerService _htmlSanitizer;

    public EmployeeEmailTemplateService(
        IEmployeeEmailTemplateRepository repository,
        IHtmlSanitizerService htmlSanitizer)
    {
        _repository = repository;
        _htmlSanitizer = htmlSanitizer;
    }

    // Retrieves all email templates owned by the specified user
    public Task<List<EmployeeEmailTemplate>> GetMyTemplatesAsync(string ownerUserId)
        => _repository.GetByOwnerAsync(ownerUserId);

    public Task<EmployeeEmailTemplate?> GetMyTemplateAsync(Guid id, string ownerUserId)
        => _repository.GetByIdForOwnerAsync(id, ownerUserId);


    //Stores new employee created email template ind database
    public async Task<(bool Success, string Error)> CreateAsync(string ownerUserId, EmployeeEmailTemplateFormModel model)
    {
        var name = model.Name.Trim();
        if (await _repository.ExistsNameAsync(ownerUserId, name))
            return (false, "You already have a template with this name.");

        var sanitizedBodyHtml = _htmlSanitizer.Sanitize(model.BodyHtml);
        if (string.IsNullOrWhiteSpace(sanitizedBodyHtml))
            return (false, "Template body is empty after sanitization. Please add safe HTML content.");

        var now = DateTime.UtcNow;
        await _repository.AddAsync(new EmployeeEmailTemplate
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Name = name,
            Subject = model.Subject.Trim(),
            BodyHtml = sanitizedBodyHtml,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        return (true, string.Empty);
    }


    //Updates an existing employee email template with new data
    public async Task<(bool Success, string Error)> UpdateAsync(string ownerUserId, EmployeeEmailTemplateFormModel model)
    {
        if (!model.Id.HasValue || model.Id.Value == Guid.Empty)
            return (false, "Invalid template id.");

        var entity = await _repository.GetByIdForOwnerAsync(model.Id.Value, ownerUserId);
        if (entity == null)
            return (false, "Template not found.");

        var name = model.Name.Trim();
        if (await _repository.ExistsNameAsync(ownerUserId, name, entity.Id))
            return (false, "You already have a template with this name.");

        var sanitizedBodyHtml = _htmlSanitizer.Sanitize(model.BodyHtml);
        if (string.IsNullOrWhiteSpace(sanitizedBodyHtml))
            return (false, "Template body is empty after sanitization. Please add safe HTML content.");

        entity.Name = name;
        entity.Subject = model.Subject.Trim();
        entity.BodyHtml = sanitizedBodyHtml;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(string ownerUserId, Guid id)
    {
        var entity = await _repository.GetByIdForOwnerAsync(id, ownerUserId);
        if (entity == null)
            return (false, "Template not found.");

        await _repository.DeleteAsync(entity);
        return (true, string.Empty);
    }
}
