using System.Text.Json;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MyPortal.Application.Services;

public class IPValidationService : IIPValidationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public IPValidationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["IPDataApiKey"] ?? throw new ArgumentException("IPDataApiKey not found in configuration");
    }
    
    public async Task<IPValidationResult> ValidateIPAsync(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://api.ipdata.co/{ipAddress}?api-key={_apiKey}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new IPValidationResult
                {
                    IsValid = false,
                    Status = "Rejected",
                    Reason = "IP validation service unavailable"
                };
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var ipData = JsonSerializer.Deserialize<IPDataResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            if (ipData?.Threat == null)
            {
                return new IPValidationResult
                {
                    IsValid = true,
                    Status = "Approved",
                    Country = ipData?.CountryName
                };
            }
            
            var result = new IPValidationResult
            {
                Country = ipData.CountryName,
                IsVpn = ipData.Threat.IsVpn,
                IsProxy = ipData.Threat.IsProxy,
                IsTor = ipData.Threat.IsTor,
                IsDatacenter = ipData.Threat.IsDatacenter,
                ThreatScore = ipData.Threat.Scores?.ThreatScore ?? 0,
                TrustScore = ipData.Threat.Scores?.TrustScore ?? 100
            };
            
            // Apply the same validation rules as Node.js version
            bool rejectStatus = false;
            string reason = "";
            
            if (ipData.Threat.IsTor)
            {
                rejectStatus = true;
                reason = "Tor";
            }
            else if (ipData.Threat.IsVpn)
            {
                rejectStatus = true;
                reason = "VPN";
            }
            else if (ipData.Threat.IsIcloudRelay)
            {
                rejectStatus = true;
                reason = "Relay";
            }
            else if (ipData.Threat.IsProxy)
            {
                rejectStatus = true;
                reason = "Proxy";
            }
            else if (ipData.Threat.IsDatacenter)
            {
                rejectStatus = true;
                reason = "Bot";
            }
            else if (ipData.Threat.IsAnonymous)
            {
                rejectStatus = true;
                reason = "Proxy/VPN";
            }
            else if (ipData.Threat.IsKnownAttacker)
            {
                rejectStatus = true;
                reason = "Proxy/VPN";
            }
            else if (ipData.Threat.IsKnownAbuser)
            {
                rejectStatus = true;
                reason = "Proxy/VPN";
            }
            else if (ipData.Threat.IsThreat)
            {
                rejectStatus = true;
                reason = "Proxy/VPN";
            }
            else if (ipData.Threat.IsBogon)
            {
                rejectStatus = true;
                reason = "Proxy/VPN";
            }
            else if (ipData.Threat.Scores != null &&
                    (ipData.Threat.Scores.VpnScore == 100 || 
                     ipData.Threat.Scores.ProxyScore == 100 || 
                     ipData.Threat.Scores.ThreatScore == 100))
            {
                rejectStatus = true;
                reason = "Threat too high";
            }
            else if (ipData.Threat.Scores != null &&
                    ((ipData.Threat.Scores.VpnScore > 50 || 
                      ipData.Threat.Scores.ProxyScore > 50 || 
                      ipData.Threat.Scores.ThreatScore > 50) && 
                     ipData.Threat.Scores.TrustScore < 50))
            {
                rejectStatus = true;
                reason = "Trust too low";
            }
            
            result.IsValid = !rejectStatus;
            result.Status = rejectStatus ? "Rejected" : "Approved";
            result.Reason = reason;
            
            return result;
        }
        catch (Exception ex)
        {
            // Log error in production
            return new IPValidationResult
            {
                IsValid = false,
                Status = "Rejected",
                Reason = $"Validation error: {ex.Message}"
            };
        }
    }
}

// IPData API Response Models
public class IPDataResponse
{
    public string? CountryName { get; set; }
    public ThreatData? Threat { get; set; }
}

public class ThreatData
{
    public bool IsTor { get; set; }
    public bool IsVpn { get; set; }
    public bool IsIcloudRelay { get; set; }
    public bool IsProxy { get; set; }
    public bool IsDatacenter { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsKnownAttacker { get; set; }
    public bool IsKnownAbuser { get; set; }
    public bool IsThreat { get; set; }
    public bool IsBogon { get; set; }
    public ThreatScores? Scores { get; set; }
}

public class ThreatScores
{
    public int VpnScore { get; set; }
    public int ProxyScore { get; set; }
    public int ThreatScore { get; set; }
    public int TrustScore { get; set; }
}
