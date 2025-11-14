namespace MyPortal.Application.DTOs;

public class NetworkDto
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? NetworkLogo { get; set; }
    public string? NetworkUrl { get; set; }
    public string NetworkName { get; set; } = string.Empty;
    public string? NetworkSignupUrl { get; set; }
    public int StatusId { get; set; }
}

public class CreateNetworkDto
{
    // Network fields
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? NetworkUrl { get; set; }
    public string? NetworkSignupUrl { get; set; }
    public string? TaxDetails { get; set; }
    public string? NetworkLogo { get; set; }
    
    // User fields
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public int UserTypeId { get; set; } = 2; // Default to regular user
    //public int StatusId { get; set; } = 1; // Default to active
    public string ProviderId { get; set; } = ""; // Default provider
    
    // UserProfile fields
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
}

public class UpdateNetworkDto
{
    public int Id { get; set; }
    public int NetworkId { get; set; }
    public string? TaxDetails { get; set; }
    public string? NetworkLogo { get; set; }
    public string? NetworkUrl { get; set; }
    public string? NetworkSignupUrl { get; set; }
    
    // UserProfile update fields
    public string? PhoneNumber { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class GetAllNetworksDto
{
    public int? StatusId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedNetworkResponseDto
{
    public List<NetworkDto> AllNetwork { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}
