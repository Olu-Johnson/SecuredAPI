using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/group")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    
    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateGroupDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var group = await _groupService.CreateGroupAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Group created successfully", data = group });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateGroupDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var group = await _groupService.UpdateGroupAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Group updated successfully", data = group });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllGroupsDto? request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _groupService.GetAllGroupsAsync(request ?? new GetAllGroupsDto(), userId);
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
            var group = await _groupService.GetGroupByIdAsync(request.Id);
            if (group == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Group not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = group });
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
            var result = await _groupService.DeleteGroupAsync(id, userId);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Group not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Group deleted successfully", data = result });
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
