using System.ComponentModel.DataAnnotations;

namespace TechSolutions_IPS_HW.Models.Customer;

public class CustomerFormDataModel
{
    public Guid CustomerId { get; set; }
    
    [Required, StringLength(150)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, StringLength(150)]
    public string Surname { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Nationality { get; set; }
    
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Phone, StringLength(20)]
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
    
    [StringLength(20)]
    public string? IdNumber { get; set; }
    
    [StringLength(10)]
    public string? Gender { get; set; }
    
    [StringLength(200)]
    public string? DataSource { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public string SubmitAction { get; set; } = "draft";
}
