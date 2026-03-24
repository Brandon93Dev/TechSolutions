using System.ComponentModel.DataAnnotations;

namespace TechSolutions_IPS_HW.Models.ViewModels;

public class EmployeeEmailTemplateFormModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(20000)]
    public string BodyHtml { get; set; } = string.Empty;
}
