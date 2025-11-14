using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new LoginResponseDto
            {
                Status = false,
                StatusCode = 401,
                StatusMessage = "invalid credentials",
                Message = "invalid credentials",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return Ok(new LoginResponseDto
            {
                Status = false,
                StatusCode = 500,
                StatusMessage = "Internal Server Error",
                Message = "An error occurred during login",
                Data = null
            });
        }
    }
    
    [HttpPost("resendtoken")]
    [AllowAnonymous]
    public async Task<ActionResult> ResendToken([FromBody] ResendTokenRequestDto request)
    {
        try
        {
            var result = await _authService.ResendTokenAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new ResendTokenResponseDto
            {
                Status = false,
                StatusCode = 404,
                StatusMessage = "user not found",
                Message = "userId does not exist",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return Ok(new ResendTokenResponseDto
            {
                Status = false,
                StatusCode = 422,
                StatusMessage = "Unprocessable Entity",
                Message = "unable to send resend token",
                Data = null
            });
        }
    }
    
    [HttpPost("sendforgotpasswordEmail")]
    [AllowAnonymous]
    public async Task<ActionResult> SendForgotPasswordEmail([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var result = await _authService.SendForgotPasswordEmailAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new PasswordResetResponseDto
            {
                Status = false,
                StatusCode = 404,
                StatusMessage = "user not found",
                Message = "Password reset instruction has been sent to your email",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return Ok(new PasswordResetResponseDto
            {
                Status = false,
                StatusCode = 500,
                StatusMessage = "Internal Server Error",
                Message = "An error occurred",
                Data = null
            });
        }
    }
    
    [HttpPost("setPassword")]
    [AllowAnonymous]
    public async Task<ActionResult> SetPassword([FromBody] SetPasswordRequestDto request)
    {
        try
        {
            var result = await _authService.SetPasswordAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new PasswordResetResponseDto
            {
                Status = false,
                StatusCode = 404,
                StatusMessage = "invalid User",
                Message = "invalid token or token has expired",
                Data = null
            });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new PasswordResetResponseDto
            {
                Status = false,
                StatusCode = 400,
                StatusMessage = "Password not match",
                Message = "Password not match",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return Ok(new PasswordResetResponseDto
            {
                Status = false,
                StatusCode = 400,
                StatusMessage = "invalid token or token has expired",
                Message = "invalid token or token has expired",
                Data = null
            });
        }
    }
    
    [HttpGet("validate")]
    [Authorize]
    public ActionResult ValidateToken()
    {
        return Ok(new 
        { 
            status = true,
            statusCode = 200,
            statusMessage = "success",
            message = "Token is valid",
            data = new { valid = true }
        });
    }
}
