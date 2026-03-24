using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TechSolutions_IPS_HW.Services.AuthenticationServices;

namespace TechSolutions_IPS_HW.Tests.Services.AuthenticationServices;

public class RsaCryptoServiceTests
{
    [Fact]
    public void Constructor_Throws_WhenPrivateKeyMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        Assert.Throws<InvalidOperationException>(
            () => new RsaCryptoService(configuration));
    }

    [Fact]
    public void Constructor_Throws_WhenPrivateKeyInvalid()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rsa:PrivateKeyPem"] = "invalid-pem"
            })
            .Build();

        Assert.Throws<InvalidOperationException>(
            () => new RsaCryptoService(configuration));
    }

    [Fact]
    public void DecryptBase64_ReturnsDecryptedValue()
    {
        using var rsa = RSA.Create(2048);
        var privatePem = rsa.ExportRSAPrivateKeyPem();
        var publicPem = rsa.ExportRSAPublicKeyPem();

        using var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(publicPem.ToCharArray());

        var plainText = "secret-value";

        var encryptedBytes = publicRsa
            .Encrypt(
                Encoding.UTF8.GetBytes(plainText), 
                RSAEncryptionPadding.OaepSHA256
            );

        var cipherText = Convert.ToBase64String(encryptedBytes);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rsa:PrivateKeyPem"] = privatePem
            })
            .Build();

        var service = new RsaCryptoService(configuration);

        var decrypted = service.DecryptBase64(cipherText);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void DecryptBase64_Throws_WhenInputEmpty()
    {
        using var rsa = RSA.Create(2048);
        var privatePem = rsa.ExportRSAPrivateKeyPem();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rsa:PrivateKeyPem"] = privatePem
            })
            .Build();

        var service = new RsaCryptoService(configuration);

        Assert.Throws<ArgumentNullException>(() => service.DecryptBase64(string.Empty));
    }
}
