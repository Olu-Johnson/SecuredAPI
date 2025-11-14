using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/email")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateEmailDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var email = await _emailService.CreateEmailAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Email created successfully", data = email });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("saveofferemail")]
    public async Task<ActionResult> SaveOfferEmail([FromBody] SaveOfferEmailDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _emailService.SaveOfferEmailAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Offer email saved successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("saveCampaignEmail")]
    public async Task<ActionResult> SaveCampaignEmail([FromBody] SaveCampaignEmailDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _emailService.SaveCampaignEmailAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Campaign email saved successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateEmailDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var email = await _emailService.UpdateEmailAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Email updated successfully", data = email });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllEmailsDto? request)
    {
        try
        {
            var emails = await _emailService.GetAllEmailsAsync(request ?? new GetAllEmailsDto());
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = emails });
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
            var email = await _emailService.GetEmailByIdAsync(request.Id);
            if (email == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Email not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = email });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
