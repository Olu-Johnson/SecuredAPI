using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/emailtemplate")]
[Authorize]
public class EmailTemplateController : ControllerBase
{
    private readonly IEmailTemplateService _emailTemplateService;
    
    public EmailTemplateController(IEmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateEmailTemplateDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var template = await _emailTemplateService.CreateEmailTemplateAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Email template created successfully", data = template });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateEmailTemplateDto request)
    {
        try
        {
            var template = await _emailTemplateService.UpdateEmailTemplateAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Email template updated successfully", data = template });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllEmailTemplatesDto? request)
    {
        try
        {
            var templates = await _emailTemplateService.GetAllEmailTemplatesAsync(request ?? new GetAllEmailTemplatesDto());
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = templates });
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
            var template = await _emailTemplateService.GetEmailTemplateByIdAsync(request.Id);
            if (template == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Email template not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = template });
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
            var result = await _emailTemplateService.DeleteEmailTemplateAsync(id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Email template not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Email template deleted successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
