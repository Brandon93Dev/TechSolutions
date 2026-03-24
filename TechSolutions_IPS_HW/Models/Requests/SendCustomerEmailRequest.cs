using System.ComponentModel.DataAnnotations;

namespace TechSolutions_IPS_HW.Models.Requests;

public class SendCustomerEmailRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(20000)]
    public string BodyHtml { get; set; } = string.Empty;
}
