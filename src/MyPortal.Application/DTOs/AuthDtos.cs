namespace MyPortal.Application.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Status { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public LoginDataDto? Data { get; set; }
}

public class LoginDataDto
{
    public string Token { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
}

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
    public int? NetworkId { get; set; }
    
    // Optional UserProfile fields
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? SocialContact { get; set; }
    public string? CustomProfileDetails { get; set; }
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? Picture { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class ResendTokenRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

public class ResendTokenResponseDto
{
    public bool Status { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class PasswordResetResponseDto
{
    public bool Status { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class SetPasswordRequestDto
{
    public int Token { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    public UserProfileDto? Profile { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? SocialContact { get; set; }
    public string? CustomProfileDetails { get; set; }
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? Picture { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class UpdateUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
    public int StatusId { get; set; }
    public int? NetworkId { get; set; }
    
    // Optional UserProfile fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? SocialContact { get; set; }
    public string? CustomProfileDetails { get; set; }
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? Picture { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class GetAllUsersDto
{
    public int? NetworkId { get; set; }
    public int? UserTypeId { get; set; }
    public int? StatusId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedUserResponseDto
{
    public List<UserDto> AllUser { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}
