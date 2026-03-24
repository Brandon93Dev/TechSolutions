using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechSolutions_IPS_HW.Models.Audit;
using TechSolutions_IPS_HW.Models.Enums;

namespace TechSolutions_IPS_HW.Models.Customer;

public class Customer
{
    [Key]
    public Guid CustomerId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Surname { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Nationality { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? ProvinceState { get; set; }

    [StringLength(100)]
    public string? AddressCountry { get; set; }

    [StringLength(20)]
    public string? AreaCode { get; set; }

    [Display(Name = "ID / Passport Number")]
    [StringLength(20)]
    public string? IdNumber { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [Display(Name = "Data Source")]
    [StringLength(200)]
    public string? DataSource { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Soft-delete
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>
    /// Convenience property for display  - not saved in db
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {Surname}".Trim();

    // Navigation properties
    public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
    
    public ICollection<CustomerChangeLog> ChangeLogs { get; set; } = new List<CustomerChangeLog>();
}
