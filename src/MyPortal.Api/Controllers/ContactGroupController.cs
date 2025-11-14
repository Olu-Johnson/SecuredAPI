using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/contactgroup")]
[Authorize]
public class ContactGroupController : ControllerBase
{
    private readonly IContactGroupService _contactGroupService;
    
    public ContactGroupController(IContactGroupService contactGroupService)
    {
        _contactGroupService = contactGroupService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateContactGroupDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var contactGroup = await _contactGroupService.CreateContactGroupAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact group created successfully", data = contactGroup });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateContactGroupDto request)
    {
        try
        {
            var contactGroup = await _contactGroupService.UpdateContactGroupAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact group updated successfully", data = contactGroup });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllContactGroupsDto? request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var contactGroups = await _contactGroupService.GetAllContactGroupsAsync(request ?? new GetAllContactGroupsDto(), userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = contactGroups });
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
            var contactGroup = await _contactGroupService.GetContactGroupByIdAsync(request.Id);
            if (contactGroup == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Contact group not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = contactGroup });
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
            var result = await _contactGroupService.DeleteContactGroupAsync(id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Contact group not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Contact group deleted successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
