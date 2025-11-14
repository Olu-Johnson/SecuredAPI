using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.Interfaces;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
//[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("network/{networkId}")]
    public async Task<ActionResult> GetNetworkStatistics(int networkId)
    {
        try
        {
            var statistics = await _dashboardService.GetNetworkStatisticsAsync(networkId);
            return Ok(new 
            { 
                status = true, 
                statusCode = 200, 
                statusMessage = "Success", 
                message = "Network statistics retrieved successfully", 
                data = statistics 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new 
            { 
                status = false, 
                statusCode = 404, 
                statusMessage = "Not Found", 
                message = ex.Message, 
                data = (object?)null 
            });
        }
        catch (Exception ex)
        {
            return Ok(new 
            { 
                status = false, 
                statusCode = 500, 
                statusMessage = "Internal Server Error", 
                message = ex.Message, 
                data = (object?)null 
            });
        }
    }

    [HttpGet("all-networks")]
    public async Task<ActionResult> GetAllNetworksStatistics()
    {
        try
        {
            var statistics = await _dashboardService.GetAllNetworksStatisticsAsync();
            return Ok(new 
            { 
                status = true, 
                statusCode = 200, 
                statusMessage = "Success", 
                message = "All networks statistics retrieved successfully", 
                data = statistics 
            });
        }
        catch (Exception ex)
        {
            return Ok(new 
            { 
                status = false, 
                statusCode = 500, 
                statusMessage = "Internal Server Error", 
                message = ex.Message, 
                data = (object?)null 
            });
        }
    }
}
