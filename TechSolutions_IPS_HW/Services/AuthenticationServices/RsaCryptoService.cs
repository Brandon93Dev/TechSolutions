using System.Security.Cryptography;
using System.Text;

namespace TechSolutions_IPS_HW.Services.AuthenticationServices;

public class RsaCryptoService
{
    private readonly RSA _privateRsa;


    //Added in functionality to allow users to authenticate wsith RSA encrypted credentials,
    //the private key is loaded from configuration and used to decrypt the incoming username and poasswords
    // saw some ugly incidents where these were expoosed before, a real nasty jar of dirt haha
    public RsaCryptoService(IConfiguration configuration)
    {
        var pem = configuration["Rsa:PrivateKeyPem"];
        if (string.IsNullOrEmpty(pem))           
            throw new InvalidOperationException("RSA private key not configured. Set Rsa:PrivateKeyPem in configuration.");
        
        _privateRsa = RSA.Create();
        try
        {
            // ImportFromPem expects the PEM to include the header/footer
            _privateRsa.ImportFromPem(pem.ToCharArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to import RSA private key PEM.", ex);
        }
    }

    public string DecryptBase64(string base64Encrypted)
    {
        if (string.IsNullOrEmpty(base64Encrypted))
            throw new ArgumentNullException(nameof(base64Encrypted));

        var encrypted = Convert.FromBase64String(base64Encrypted);
        var decrypted = _privateRsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decrypted);
    }
}
