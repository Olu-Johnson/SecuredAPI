using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/smtpsetup")]
[Authorize]
public class SmtpSetupController : ControllerBase
{
    private readonly ISmtpSetupService _smtpSetupService;
    
    public SmtpSetupController(ISmtpSetupService smtpSetupService)
    {
        _smtpSetupService = smtpSetupService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateSmtpSetupDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var setup = await _smtpSetupService.CreateSmtpSetupAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "SMTP setup created successfully", data = setup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateSmtpSetupDto request)
    {
        try
        {
            var setup = await _smtpSetupService.UpdateSmtpSetupAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "SMTP setup updated successfully", data = setup });
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
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var setups = await _smtpSetupService.GetAllSmtpSetupsAsync(userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = setups });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("getbyid")]
    public async Task<ActionResult> GetById([FromBody] GetByIdDto request)
    {
        try
        {
            var setup = await _smtpSetupService.GetSmtpSetupByIdAsync(request.Id);
            if (setup == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "SMTP setup not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = setup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _smtpSetupService.DeleteSmtpSetupAsync(id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "SMTP setup not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "SMTP setup deleted successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("getbynetworkid")]
    public async Task<ActionResult> GetByNetworkId([FromBody] GetByIdDto request)
    {
        try
        {
            var setup = await _smtpSetupService.GetSmtpSetupByNetworkIdAsync(request.Id);
            if (setup == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "SMTP setup not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = setup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
