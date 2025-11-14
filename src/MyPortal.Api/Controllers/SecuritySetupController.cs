using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/setup")]
[Authorize]
public class SecuritySetupController : ControllerBase
{
    private readonly ISecuritySetupService _securitySetupService;
    
    public SecuritySetupController(ISecuritySetupService securitySetupService)
    {
        _securitySetupService = securitySetupService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateSecuritySetupDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var setup = await _securitySetupService.CreateSecuritySetupAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Security setup created successfully", data = setup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateSecuritySetupDto request)
    {
        try
        {
            var setup = await _securitySetupService.UpdateSecuritySetupAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Security setup updated successfully", data = setup });
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
    
    [HttpPost("getall")]
    public async Task<ActionResult> GetAll([FromBody] GetAllSecuritySetupsDto request)
    {
        try
        {
            var result = await _securitySetupService.GetAllSecuritySetupsAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "success", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("updateismonitor")]
    public async Task<ActionResult> UpdateIsMonitor([FromBody] UpdateIsMonitorDto request)
    {
        try
        {
            var setup = await _securitySetupService.UpdateIsMonitorAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "IsMonitor updated successfully", data = setup });
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
    
    [HttpPost("getbynetworkid")]
    public async Task<ActionResult> GetByNetworkId([FromBody] GetByNetworkIdDto request)
    {
        try
        {
            var setup = await _securitySetupService.GetSecuritySetupByNetworkIdAsync(request.NetworkId);
            if (setup == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Security setup not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = setup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
