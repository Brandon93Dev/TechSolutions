using System.ComponentModel.DataAnnotations;

namespace TechSolutions_IPS_HW.Models.Reference;

public class CountryDialCode
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string CountryName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string DialCode { get; set; } = string.Empty;
}
