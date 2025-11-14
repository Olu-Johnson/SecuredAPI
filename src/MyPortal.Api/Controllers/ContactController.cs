using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/contact")]
[Authorize]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    
    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateContactDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var contact = await _contactService.CreateContactAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact is created Successful", data = contact });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new { status = false, statusCode = 409, statusMessage = "email already exist", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("upload")]
    public async Task<ActionResult> Upload([FromBody] UploadContactDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _contactService.UploadContactsAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact is created Successful", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateContactDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var contact = await _contactService.UpdateContactAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact updated successfully", data = contact });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllContactsDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _contactService.GetAllContactsAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "success", data = result });
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
            var contact = await _contactService.GetContactByIdAsync(request.Id);
            if (contact == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Contact not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = contact });
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _contactService.DeleteContactAsync(id, userId);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Contact not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact deleted successfully", data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new { status = false, statusCode = 403, statusMessage = "Forbidden", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("block/{id}")]
    public async Task<ActionResult> Block(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var contact = await _contactService.BlockContactAsync(id, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact blocked successfully", data = contact });
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = ex.Message, data = (object?)null });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new { status = false, statusCode = 403, statusMessage = "Forbidden", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
