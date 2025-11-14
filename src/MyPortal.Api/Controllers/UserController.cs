using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public UserController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "User registered successfully", data = user });
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
}
