using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/adminuser")]
//[Authorize]
public class AdminUserController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AdminUserController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] RegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Admin user created successfully", data = user });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new { status = false, statusCode = 409, statusMessage = "conflict", message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }

    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] UpdateUserDto request)
    {
        try
        {
            var user = await _authService.UpdateUserAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "User updated successfully", data = user });
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new { status = false, statusCode = 404, statusMessage = "Not Found", message = ex.Message, data = (object?)null });
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
            var user = await _authService.GetUserByIdAsync(request.Id);
            if (user == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "Not Found", message = "User not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = user });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }

    [HttpPost("getall")]
    public async Task<ActionResult> GetAll([FromBody] GetAllUsersDto request)
    {
        try
        {
            var users = await _authService.GetAllUsersAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = users });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }

    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] GetByIdDto request)
    {
        try
        {
            var result = await _authService.DeleteUserAsync(request.Id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "Not Found", message = "User not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "User deleted successfully", data = (object?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
