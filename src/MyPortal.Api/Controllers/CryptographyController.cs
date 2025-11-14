using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/cryptography")]
[Authorize]
public class CryptographyController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public CryptographyController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpPost("encryption")]
    public ActionResult Encrypt([FromBody] EncryptionRequest request)
    {
        try
        {
            var encrypted = EncryptString(request.Plaintext);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = encrypted });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("decryption")]
    public ActionResult Decrypt([FromBody] DecryptionRequest request)
    {
        try
        {
            var decrypted = DecryptString(request.Encryptedtext);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = decrypted });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    private string EncryptString(string plainText)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!.Substring(0, 32));
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        
        return Convert.ToBase64String(ms.ToArray());
    }
    
    private string DecryptString(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!.Substring(0, 32));
        
        using var aes = Aes.Create();
        aes.Key = key;
        
        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];
        
        Array.Copy(fullCipher, 0, iv, 0, 16);
        Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
        
        aes.IV = iv;
        
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }
}

public class EncryptionRequest
{
    public string Plaintext { get; set; } = string.Empty;
}

public class DecryptionRequest
{
    public string Encryptedtext { get; set; } = string.Empty;
}
