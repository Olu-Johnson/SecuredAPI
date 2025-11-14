using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/report")]
[Authorize]
public class ClicksReportController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    
    public ClicksReportController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }
    
    [HttpPost("getall")]
    public async Task<ActionResult> GetAll([FromBody] GetAllClicksReportDto request)
    {
        try
        {
            var result = await _campaignService.GetAllClicksReportsAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "success", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}

public class GetAllReportsDto
{
    public int? NetworkId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
