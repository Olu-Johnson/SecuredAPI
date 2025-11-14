using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/userprofile")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    
    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateUserProfileDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var profile = await _userProfileService.CreateUserProfileAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "User profile created successfully", data = profile });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateUserProfileDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var profile = await _userProfileService.UpdateUserProfileAsync(request, userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "User profile updated successfully", data = profile });
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
            var profiles = await _userProfileService.GetAllUserProfilesAsync(userId);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = profiles });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
    
    [HttpPost("getbyid")]
    public async Task<ActionResult> GetById()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var profile = await _userProfileService.GetUserProfileByIdAsync(userId);
            if (profile == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "User profile not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "success", data = profile });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
