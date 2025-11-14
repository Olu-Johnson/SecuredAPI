using System.Text;
using System.Text.Json;
using System.Web;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIPValidationService _ipValidationService;
    
    public CampaignService(IUnitOfWork unitOfWork, IIPValidationService ipValidationService)
    {
        _unitOfWork = unitOfWork;
        _ipValidationService = ipValidationService;
    }
    
    public async Task<CampaignDetailsDto> CreateCampaignAsync(CreateCampaignDto request, int userId)
    {
        // Check if campaign with same name exists for this network
        var existing = await _unitOfWork.Repository<CampaignDetails>()
            .FindAsync(c => c.Name == request.Name && c.NetworkId == request.NetworkId);
        
        if (existing.Any())
        {
            throw new InvalidOperationException("Campaign with this name already exists for this network");
        }
        
        // Create promotion link (simplified - just remove spaces from name)
        var promotionLink = request.Name.Replace(" ", "");
        
        var campaign = new CampaignDetails
        {
            Name = request.Name,
            Link = request.Link,
            PromotionLink = promotionLink,
            RejectedLink = request.RejectedLink,
            Type = request.Type,
            Content = request.Content,
            NetworkId = request.NetworkId,
            StatusId = 1 // Active status
        };
        
        await _unitOfWork.Repository<CampaignDetails>().AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(campaign);
    }
    
    public async Task<CampaignDetailsDto> UpdateCampaignAsync(int id, CreateCampaignDto request)
    {
        var campaign = await _unitOfWork.Repository<CampaignDetails>().GetByIdAsync(id);
        if (campaign == null)
        {
            throw new KeyNotFoundException("Campaign not found");
        }
        
        campaign.Name = request.Name;
        campaign.Link = request.Link;
        campaign.RejectedLink = request.RejectedLink;
        campaign.Type = request.Type;
        campaign.Content = request.Content;
        campaign.PromotionLink = request.Name.Replace(" ", "");
        
        await _unitOfWork.Repository<CampaignDetails>().UpdateAsync(campaign);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(campaign);
    }
    
    public async Task<CampaignDetailsDto?> GetCampaignByIdAsync(int id)
    {
        var campaign = await _unitOfWork.Repository<CampaignDetails>().GetByIdAsync(id);
        return campaign == null ? null : MapToDto(campaign);
    }
    
    public async Task<PaginatedCampaignResponseDto> GetAllCampaignsAsync(GetAllCampaignsDto request, int userId)
    {
        // Get user to retrieve networkId
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Build query based on networkId and optional statusId filter
        var campaigns = await _unitOfWork.Repository<CampaignDetails>()
            .FindAsync(c => c.NetworkId == user.NetworkId && 
                           (!request.StatusId.HasValue || c.StatusId == request.StatusId.Value));
        
        var campaignList = campaigns.OrderByDescending(c => c.CreatedAt).ToList();
        var totalCount = campaignList.Count;
        
        // Apply pagination
        var paginatedCampaigns = campaignList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        return new PaginatedCampaignResponseDto
        {
            AllCampaign = paginatedCampaigns.Select(MapToDto).ToList(),
            Page = request.PageIndex,
            Total = totalCount
        };
    }
    
    public async Task<bool> DeleteCampaignAsync(int id)
    {
        var campaign = await _unitOfWork.Repository<CampaignDetails>().GetByIdAsync(id);
        if (campaign == null)
        {
            return false;
        }
        
        await _unitOfWork.Repository<CampaignDetails>().DeleteAsync(campaign);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<string> ProcessCampaignClickAsync(string promotionLink, string parameters, string ipAddress, string userAgent)
    {
        try
        {
            // Find campaign by promotion link
            var campaigns = await _unitOfWork.Repository<CampaignDetails>()
                .FindAsync(c => c.PromotionLink == promotionLink);
            var campaign = campaigns.FirstOrDefault();
            
            if (campaign == null)
            {
                return "https://google.com"; // Default redirect
            }
            
            // Decode Base64 parameters: email-firstname-lastname
            string email = "", firstName = "", lastName = "";
            try
            {
                var decodedParams = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));
                var parts = decodedParams.Split('-');
                if (parts.Length >= 3)
                {
                    email = parts[0];
                    firstName = parts[1];
                    lastName = parts[2];
                }
            }
            catch
            {
                // Invalid parameters format
            }
            
            // Validate IP address
            var ipValidation = await _ipValidationService.ValidateIPAsync(ipAddress);
            
            // Parse user agent for browser info
            var browser = ParseBrowser(userAgent);
            
            // Create campaign report
            var report = new CampaignReport
            {
                ClickTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Campaign = campaign.Name,
                Status = ipValidation.Status,
                Reason = ipValidation.Reason,
                ClickIP = ipAddress,
                Country = ipValidation.Country,
                OsName = "",
                DeviceType = "",
                BrowserName = browser,
                NetworkId = campaign.NetworkId
            };
            
            await _unitOfWork.Repository<CampaignReport>().AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
            
            // Return appropriate redirect URL
            if (ipValidation.Status == "Rejected")
            {
                return campaign.RejectedLink ?? "https://google.com";
            }
            
            return campaign.Link ?? "https://google.com";
        }
        catch (Exception ex)
        {
            // Log error and return safe default
            return "https://google.com";
        }
    }
    
    private static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "unknown";
        
        userAgent = userAgent.ToLower();
        
        if (userAgent.Contains("chrome")) return "Chrome";
        if (userAgent.Contains("firefox")) return "Firefox";
        if (userAgent.Contains("safari")) return "Safari";
        if (userAgent.Contains("edge")) return "Edge";
        if (userAgent.Contains("opera")) return "Opera";
        
        return "unknown";
    }
    
    public async Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync(object request)
    {
        // Get all campaign reports
        var reports = await _unitOfWork.Repository<CampaignReport>().GetAllAsync();
        
        return reports.Select(r => new CampaignReportDto
        {
            Id = r.Id,
            ClickTime = r.ClickTime,
            Email = r.Email,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Campaign = r.Campaign,
            Status = r.Status,
            Reason = r.Reason,
            ClickIP = r.ClickIP,
            Country = r.Country,
            OsName = r.OsName,
            DeviceType = r.DeviceType,
            BrowserName = r.BrowserName,
            NetworkId = r.NetworkId
        });
    }
    
    public async Task<PaginatedClicksReportResponseDto> GetAllClicksReportsAsync(GetAllClicksReportDto request)
    {
        // Filter by networkId matching Node.js implementation
        var query = await _unitOfWork.Repository<CampaignReport>()
            .FindAsync(r => r.NetworkId == request.NetworkId);
        
        var total = query.Count();
        
        // Apply pagination
        var reports = query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ClicksReportDto
            {
                Id = r.Id,
                ClickTime = r.ClickTime,
                Publisher = r.Email ?? "", // Map Email to Publisher for Node.js compatibility
                Offer = r.Campaign ?? "", // Map Campaign to Offer for Node.js compatibility
                Status = r.Status,
                Reason = r.Reason,
                ClickIP = r.ClickIP,
                Country = r.Country,
                OsName = r.OsName ?? "",
                DeviceType = r.DeviceType ?? "",
                BroswerName = r.BrowserName ?? "", // Note: keeping Node.js typo "Broswe"
                NetworkId = r.NetworkId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToList();
        
        return new PaginatedClicksReportResponseDto
        {
            AllReport = reports,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<ServiceResponse<string>> FilterClicksAsync(int networkId, string ipAddress, string browserInfo, string host, Dictionary<string, string> queryParams)
    {
        try
        {
            // Validate IP address for fraud detection
            var ipValidation = await _ipValidationService.ValidateIPAsync(ipAddress);
            
            // Get network details
            var network = await _unitOfWork.Repository<Network>().GetByIdAsync(networkId);
            var setupDetails = await _unitOfWork.Repository<SecuritySetup>().GetByIdAsync(networkId);
            
            if (setupDetails == null)
            {
                return new ServiceResponse<string>
                {
                    Status = false,
                    StatusCode = 404,
                    StatusMessage = "not found",
                    Message = "Security setup not found for network",
                    Data = "https://google.com"
                };
            }
            
            // Parse and map query parameters based on setupDetails.Parameters
            var mappedQueryParams = new Dictionary<string, string>();
            var queryString = "";
            
            if (!string.IsNullOrEmpty(setupDetails.Parameters))
            {
                try
                {
                    // Parse the Parameters JSON mapping
                    var parameterMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(setupDetails.Parameters);
                    
                    if (parameterMapping != null)
                    {
                        // Map queryParams based on the parameter mapping
                        foreach (var mapping in parameterMapping)
                        {
                            var mappedKey = mapping.Value; // The key to look for in queryParams
                            if (queryParams.ContainsKey(mappedKey))
                            {
                                mappedQueryParams[mapping.Value] = queryParams[mappedKey];
                            }
                        }
                        
                        // Build query string from mapped parameters
                        var queryStringParts = mappedQueryParams
                            .Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}");
                        queryString = string.Join("&", queryStringParts);
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, continue without query string mapping
                }
            }
            
            // Create click report
            var report = new CampaignReport
            {
                ClickTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Email = "",
                FirstName = "",
                LastName = "",
                Campaign = "Direct Network Click",
                Status = ipValidation.Status,
                Reason = ipValidation.Reason,
                ClickIP = ipAddress,
                Country = ipValidation.Country,
                OsName = "",
                DeviceType = "",
                BrowserName = ParseBrowser(browserInfo),
                NetworkId = networkId
            };
            
            await _unitOfWork.Repository<CampaignReport>().AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
            
            // Build redirect URLs with query string
            var rejectedPage = setupDetails.RejectedPage ?? "https://google.com";
            var approvedPage = setupDetails.ApprovedPage ?? "https://google.com";
            
            // Append query string to approved page if available
            if (!string.IsNullOrEmpty(queryString))
            {
                approvedPage = approvedPage.Contains("?") 
                    ? $"{approvedPage}&{queryString}" 
                    : $"{approvedPage}?{queryString}";
            }

            // Return appropriate redirect URL based on IP validation
            string redirectUrl = ipValidation.Status == "Rejected"
                ? rejectedPage
                : approvedPage;
            
            // if (ipValidation.Status == "Rejected")
            // {
            //     // For rejected IPs, redirect to signup URL or default
            //     redirectUrl = network.NetworkSignupUrl ?? "https://google.com";
            // }
            
            return new ServiceResponse<string>
            {
                Status = true,
                StatusCode = 302,
                StatusMessage = "success",
                Message = "Redirect",
                Data = redirectUrl
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<string>
            {
                Status = false,
                StatusCode = 500,
                StatusMessage = "Internal Server Error",
                Message = ex.Message,
                Data = "https://google.com"
            };
        }
    }
    
    private static CampaignDetailsDto MapToDto(CampaignDetails campaign)
    {
        return new CampaignDetailsDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Link = campaign.Link,
            PromotionLink = campaign.PromotionLink,
            RejectedLink = campaign.RejectedLink,
            Type = campaign.Type,
            Content = campaign.Content,
            NetworkId = campaign.NetworkId,
            StatusId = campaign.StatusId
        };
    }
}
