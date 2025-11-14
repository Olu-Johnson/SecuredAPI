using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    
    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();
        
        if (user == null || !VerifyPassword(request.Password, user.Password ?? ""))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        // Get user profile
        var profiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(p => p.UserId == user.Id);
        var profile = profiles.FirstOrDefault();
        
        var userDto = MapToUserDto(user, profile);
        var token = GenerateJwtToken(userDto);
        
        return new LoginResponseDto
        {
            Status = true,
            StatusCode = 200,
            StatusMessage = "login is successful",
            Message = "login is successful",
            Data = new LoginDataDto
            {
                Token = token,
                UserTypeId = user.UserTypeId
            }
        };
    }
    
    public async Task<UserDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user exists
        var existing = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == request.Email);
        
        if (existing.Any())
        {
            throw new InvalidOperationException("User with this email already exists");
        }
        
        // Create user
        var user = new User
        {
            Email = request.Email,
            Password = HashPassword(request.Password),
            UserTypeId = request.UserTypeId,
            NetworkId = request.NetworkId,
            StatusId = 1 // Active
        };
        
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        // Create user profile
        var profile = new UserProfile
        {
            UserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PhoneNumber = request.PhoneNumber,
            Company = request.Company,
            SocialContact = request.SocialContact,
            CustomProfileDetails = request.CustomProfileDetails,
            TaxDetails = request.TaxDetails,
            AccountSecretKey = request.AccountSecretKey,
            Picture = request.Picture,
            Address = request.Address,
            Country = request.Country,
            State = request.State,
            City = request.City,
            PostalCode = request.PostalCode,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToUserDto(user, profile);
        }
        catch (Exception e)
        {
            
            throw;
        }
    }
    
    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
    
    public string GenerateJwtToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("UserTypeId", user.UserTypeId.ToString())
        };
        
        if (user.NetworkId.HasValue)
        {
            claims.Add(new Claim("NetworkId", user.NetworkId.Value.ToString()));
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60")),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    private static bool VerifyPassword(string password, string hashedPassword)
    {
        var hash = HashPassword(password);
        return hash == hashedPassword;
    }
    
    public async Task<ResendTokenResponseDto> ResendTokenAsync(ResendTokenRequestDto request)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Generate new token
        var random = new Random();
        var tokenValue = random.Next(100000, 999999);
        
        var token = new Token
        {
            Value = tokenValue,
            Purpose = request.Purpose,
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(24),
            UserId = user.Id,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<Token>().AddAsync(token);
        await _unitOfWork.SaveChangesAsync();
        
        // TODO: Send email with token
        
        return new ResendTokenResponseDto
        {
            Status = true,
            StatusCode = 200,
            StatusMessage = "success",
            Message = "Token has been sent to " + request.Email,
            Data = null
        };
    }
    
    public async Task<PasswordResetResponseDto> SendForgotPasswordEmailAsync(ForgotPasswordRequestDto request)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Generate reset token
        var random = new Random();
        var tokenValue = random.Next(100000, 999999);
        
        var token = new Token
        {
            Value = tokenValue,
            Purpose = "PASSWORD_RESET",
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(24),
            UserId = user.Id,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<Token>().AddAsync(token);
        await _unitOfWork.SaveChangesAsync();
        
        // TODO: Send email with token
        
        return new PasswordResetResponseDto
        {
            Status = true,
            StatusCode = 200,
            StatusMessage = "success",
            Message = "Token and Password reset instruction has been sent to your email, if you do not get any email kindly confirm your email is correct",
            Data = new { userId = user.Id }
        };
    }
    
    public async Task<PasswordResetResponseDto> SetPasswordAsync(SetPasswordRequestDto request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match");
        }
        
        // Verify token
        var tokens = await _unitOfWork.Repository<Token>()
            .FindAsync(t => t.Value == request.Token && 
                           t.UserId == request.UserId && 
                           !t.IsUsed && 
                           t.ExpiredAt > DateTime.UtcNow);
        var token = tokens.FirstOrDefault();
        
        if (token == null)
        {
            throw new KeyNotFoundException("Invalid or expired token");
        }
        
        // Get user
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Update password
        user.Password = HashPassword(request.Password);
        await _unitOfWork.Repository<User>().UpdateAsync(user);
        
        // Mark token as used
        token.IsUsed = true;
        await _unitOfWork.Repository<Token>().UpdateAsync(token);
        
        await _unitOfWork.SaveChangesAsync();
        
        return new PasswordResetResponseDto
        {
            Status = true,
            StatusCode = 200,
            StatusMessage = "Success",
            Message = "Password has been reset, kindly go to Login to login",
            Data = null
        };
    }
    
    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var profiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(p => p.UserId == id);
        var profile = profiles.FirstOrDefault();

        return MapToUserDto(user, profile);
    }

    public async Task<PaginatedUserResponseDto> GetAllUsersAsync(GetAllUsersDto request)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => (request.NetworkId == null || u.NetworkId == request.NetworkId) &&
                           (request.UserTypeId == null || u.UserTypeId == request.UserTypeId) &&
                           (request.StatusId == null || u.StatusId == request.StatusId));

        var userList = users.ToList();
        var pagedUsers = userList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers)
        {
            var profiles = await _unitOfWork.Repository<UserProfile>()
                .FindAsync(p => p.UserId == user.Id);
            var profile = profiles.FirstOrDefault();
            userDtos.Add(MapToUserDto(user, profile));
        }

        return new PaginatedUserResponseDto
        {
            AllUser = userDtos,
            Page = request.PageIndex,
            Total = userList.Count
        };
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserDto request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.Email = request.Email;
        user.UserTypeId = request.UserTypeId;
        user.StatusId = request.StatusId;
        user.NetworkId = request.NetworkId;

        await _unitOfWork.Repository<User>().UpdateAsync(user);

        // Update user profile
        var profiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(p => p.UserId == user.Id);
        var profile = profiles.FirstOrDefault();

        if (profile != null)
        {
            // Only update fields if they are provided
            if (request.FirstName != null) profile.FirstName = request.FirstName;
            if (request.LastName != null) profile.LastName = request.LastName;
            if (request.MiddleName != null) profile.MiddleName = request.MiddleName;
            if (request.PhoneNumber != null) profile.PhoneNumber = request.PhoneNumber;
            if (request.Company != null) profile.Company = request.Company;
            if (request.SocialContact != null) profile.SocialContact = request.SocialContact;
            if (request.CustomProfileDetails != null) profile.CustomProfileDetails = request.CustomProfileDetails;
            if (request.TaxDetails != null) profile.TaxDetails = request.TaxDetails;
            if (request.AccountSecretKey != null) profile.AccountSecretKey = request.AccountSecretKey;
            if (request.Picture != null) profile.Picture = request.Picture;
            if (request.Address != null) profile.Address = request.Address;
            if (request.Country != null) profile.Country = request.Country;
            if (request.State != null) profile.State = request.State;
            if (request.City != null) profile.City = request.City;
            if (request.PostalCode != null) profile.PostalCode = request.PostalCode;

            await _unitOfWork.Repository<UserProfile>().UpdateAsync(profile);
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user, profile);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        // Delete associated user profile
        var profiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(p => p.UserId == id);
        var profile = profiles.FirstOrDefault();
        if (profile != null)
        {
            await _unitOfWork.Repository<UserProfile>().DeleteAsync(profile);
        }

        // Delete user
        await _unitOfWork.Repository<User>().DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static UserDto MapToUserDto(User user, UserProfile? profile)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserTypeId = user.UserTypeId,
            NetworkId = user.NetworkId,
            StatusId = user.StatusId,
            Profile = profile == null ? null : new UserProfileDto
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                MiddleName = profile.MiddleName,
                PhoneNumber = profile.PhoneNumber,
                Company = profile.Company,
                SocialContact = profile.SocialContact,
                CustomProfileDetails = profile.CustomProfileDetails,
                TaxDetails = profile.TaxDetails,
                AccountSecretKey = profile.AccountSecretKey,
                Picture = profile.Picture,
                Address = profile.Address,
                Country = profile.Country,
                State = profile.State,
                City = profile.City,
                PostalCode = profile.PostalCode
            }
        };
    }
}
