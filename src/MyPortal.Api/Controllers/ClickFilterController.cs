using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.Interfaces;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("/")]
public class ClickFilterController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly IIPValidationService _ipValidationService;
    private readonly string _environment;
    
    public ClickFilterController(ICampaignService campaignService, IIPValidationService ipValidationService, IConfiguration configuration)
    {
        _campaignService = campaignService;
        _ipValidationService = ipValidationService;
        _environment = configuration["environment"] ?? "production";
    }

    // private readonly string _apiKey;
    
    // public IPValidationService(HttpClient httpClient, IConfiguration configuration)
    // {
    //     _httpClient = httpClient;
    //     _apiKey = configuration["IPDataApiKey"] ?? throw new ArgumentException("IPDataApiKey not found in configuration");
    // }
    
    [HttpGet("{networkId}")]
    public async Task<ActionResult> FilterClicks(int networkId)
    {
        try
        {
            var browserInfo = Request.Headers["User-Agent"].ToString();
            var host = Request.Headers["Host"].ToString();
            var clientIp = _environment == "development" ? "142.167.8.154" : HttpContext.Connection.RemoteIpAddress?.ToString();
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            
            var result = await _campaignService.FilterClicksAsync(
                networkId, 
                clientIp, 
                browserInfo, 
                host, 
                queryParams);
            
            if (result.Status)
            {
                // Redirect to the destination URL
                return Redirect(result.Data);
            }
            
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = "An error occurred", error = ex.Message });
        }
    }
}
