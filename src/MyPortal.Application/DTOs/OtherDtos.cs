namespace MyPortal.Application.DTOs;

public class ContactGroupDto
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int GroupId { get; set; }
    public int StatusId { get; set; }
}

public class CreateContactGroupDto
{
    public int ContactId { get; set; }
    public int GroupId { get; set; }
}

public class UpdateContactGroupDto
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int GroupId { get; set; }
}

public class GetAllContactGroupsDto
{
    public int? GroupId { get; set; }
    public int? ContactId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedContactGroupResponseDto
{
    public List<ContactGroupDto> AllGroup { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class UserInfoDto
{
    public string Email { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
    public string? ProviderTypeId { get; set; }
    public int StatusId { get; set; }
    public int? NetworkId { get; set; }
}

public class UserProfileDto2
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Picture { get; set; }
    public string? Company { get; set; }
    public string? Country { get; set; }
    public int UserId { get; set; }
    public int StatusId { get; set; }
    public UserInfoDto? User { get; set; }
    public List<object> Guarantors { get; set; } = new();
    public List<object> Leaves { get; set; } = new();
}

public class CreateUserProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Picture { get; set; }
    public string? Company { get; set; }
    public string? Country { get; set; }
}

public class UpdateUserProfileDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class SmtpSetupDto
{
    public int Id { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public bool? IsSecure { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? Encryption { get; set; }
    public int BatchSize { get; set; }
    public int BatchIntervalMinutes { get; set; }
    public int MaxRetryAttempts { get; set; }
    public bool IsSendingEnabled { get; set; }
    public int? NetworkId { get; set; }
}

public class CreateSmtpSetupDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Encryption { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 50;
    public int BatchIntervalMinutes { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 5;
    public bool IsSendingEnabled { get; set; } = true;
}

public class UpdateSmtpSetupDto
{
    public int Id { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Encryption { get; set; } = string.Empty;
    public int BatchSize { get; set; }
    public int BatchIntervalMinutes { get; set; }
    public int MaxRetryAttempts { get; set; }
    public bool IsSendingEnabled { get; set; }
}

public class PaginatedSmtpSetupResponseDto
{
    public List<SmtpSetupDto> AllSmtpSetup { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class SecuritySetupDto
{
    public int Id { get; set; }
    public string? Parameters { get; set; }
    public string? PlatformSetup { get; set; }
    public string? ApprovedPage { get; set; }
    public string? RejectedPage { get; set; }
    public bool? IsMonitor { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
}

public class CreateSecuritySetupDto
{
    public string Parameters { get; set; } = string.Empty;
    public string ApprovedPage { get; set; } = string.Empty;
    public string RejectedPage { get; set; } = string.Empty;
    public bool IsMonitor { get; set; }
    public int NetworkId { get; set; }
    public string PlatformSetup { get; set; } = string.Empty;
  
}

public class UpdateSecuritySetupDto
{
    public int Id { get; set; }
    public string Parameters { get; set; } = string.Empty;
    public string ApprovedPage { get; set; } = string.Empty;
    public string RejectedPage { get; set; } = string.Empty;
    public string PlatformSetup { get; set; } = string.Empty;
    public bool IsMonitor { get; set; }
}

public class UpdateIsMonitorDto
{
    public int SetupId { get; set; }
    public bool IsMonitor { get; set; }
}

public class GetAllSecuritySetupsDto
{
    public int? NetworkId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int StatusId { get; set; }
}

public class PaginatedSecuritySetupResponseDto
{
    public List<SecuritySetupDto> AllSetup { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class ServiceResponse<T>
{
    public bool Status { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
