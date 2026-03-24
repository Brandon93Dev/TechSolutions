namespace TechSolutions_IPS_HW.Models.Api;

public class EncryptedLoginRequest
{
    public string Data { get; set; } = string.Empty; // base64 encrypted json for LoginRequest
}
