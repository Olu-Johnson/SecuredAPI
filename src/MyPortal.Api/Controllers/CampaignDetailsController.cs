using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/campaigndetails")]
[Authorize]
public class CampaignDetailsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    
    public CampaignDetailsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult<CampaignDetailsDto>> Create([FromBody] CreateCampaignDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var campaign = await _campaignService.CreateCampaignAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Campaign created successfully", data = campaign });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new { status = false, statusCode = 409, statusMessage = "conflict", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult<CampaignDetailsDto>> Update([FromBody] UpdateCampaignDto request)
    {
        try
        {
            var campaign = await _campaignService.UpdateCampaignAsync(request.Id, request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Campaign updated successfully", data = campaign });
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("getbyid")]
    public async Task<ActionResult<CampaignDetailsDto>> GetById([FromBody] GetByIdDto request)
    {
        var campaign = await _campaignService.GetCampaignByIdAsync(request.Id);
        if (campaign == null)
        {
            return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Campaign not found", data = (object?)null });
        }
        return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = campaign });
    }
    
    [HttpPost("getall")]
    public async Task<ActionResult<PaginatedCampaignResponseDto>> GetAll([FromBody] GetAllCampaignsDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var campaigns = await _campaignService.GetAllCampaignsAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = campaigns });
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }

    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] GetByIdDto request)
    {
        try
        {
            var result = await _campaignService.DeleteCampaignAsync(request.Id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Campaign not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Campaign deleted successfully", data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}

[ApiController]
[Route("cpg")]
[AllowAnonymous]
public class CampaignTrackingController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    
    public CampaignTrackingController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }
    
    [HttpGet("{promotionLink}/{parameters}")]
    public async Task<IActionResult> TrackClick(string promotionLink, string parameters)
    {
        try
        {
            // Get client IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Check for forwarded IP (behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            else if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = Request.Headers["X-Real-IP"].ToString();
            }
            
            var userAgent = Request.Headers["User-Agent"].ToString();
            
            var redirectUrl = await _campaignService.ProcessCampaignClickAsync(
                promotionLink, 
                parameters, 
                ipAddress, 
                userAgent);
            
            return Redirect(redirectUrl);
        }
        catch (Exception)
        {
            return Redirect("https://google.com");
        }
    }
}
