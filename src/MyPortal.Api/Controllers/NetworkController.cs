using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using System.Security.Claims;

namespace MyPortal.Api.Controllers;

[ApiController]
[Route("api/network")]
//[Authorize]
public class NetworkController : ControllerBase
{
    private readonly INetworkService _networkService;
    
    public NetworkController(INetworkService networkService)
    {
        _networkService = networkService;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateNetworkDto request)
    {
        try
        {
            var network = await _networkService.CreateNetworkAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Network created successfully", data = network });
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
    public async Task<ActionResult> Update([FromBody] UpdateNetworkDto request)
    {
        try
        {
            var network = await _networkService.UpdateNetworkAsync(request);
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Network updated successfully", data = network });
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
    public async Task<ActionResult> GetAll([FromBody] GetAllNetworksDto? request)
    {
        try
        {
            var networks = await _networkService.GetAllNetworksAsync(request ?? new GetAllNetworksDto());
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = networks });
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
            var network = await _networkService.GetNetworkByIdAsync(request.Id);
            if (network == null)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Network not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Success", data = network });
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
            var result = await _networkService.DeleteNetworkAsync(id);
            if (!result)
            {
                return Ok(new { status = false, statusCode = 404, statusMessage = "not found", message = "Network not found", data = (object?)null });
            }
            return Ok(new { status = true, statusCode = 200, statusMessage = "Success", message = "Network deleted successfully", data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { status = false, statusCode = 500, statusMessage = "Internal Server Error", message = ex.Message, data = (object?)null });
        }
    }
}
