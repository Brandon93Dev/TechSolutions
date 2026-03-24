using System.ComponentModel.DataAnnotations;

namespace TechSolutions_IPS_HW.Models.Email;

public class EmployeeEmailTemplate
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string OwnerUserId { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(20000)]
    public string BodyHtml { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
