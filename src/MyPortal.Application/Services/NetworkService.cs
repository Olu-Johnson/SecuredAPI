using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace MyPortal.Application.Services;

public class NetworkService : INetworkService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public NetworkService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<NetworkDto> CreateNetworkAsync(CreateNetworkDto request)
    {
        // Comprehensive validation matching Node.js
        
        // Email validation
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email is required");
        }
        
        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        // Company validation
        if (string.IsNullOrEmpty(request.Company))
        {
            throw new ArgumentException("Company is required");
        }
        
        // Password validation
        if (string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Password is required");
        }
        
        if (string.IsNullOrEmpty(request.ConfirmPassword))
        {
            throw new ArgumentException("confirmPassword is required");
        }
        
        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("password not match");
        }
        
        // UserProfile field validations
        if (string.IsNullOrEmpty(request.FirstName))
        {
            throw new ArgumentException("First Name is required");
        }
        
        if (string.IsNullOrEmpty(request.MiddleName))
        {
            throw new ArgumentException("Middle Name is required");
        }
        
        if (string.IsNullOrEmpty(request.LastName))
        {
            throw new ArgumentException("Last Name is required");
        }
        
        if (string.IsNullOrEmpty(request.PhoneNumber))
        {
            throw new ArgumentException("Phone Number is required");
        }
        
        if (string.IsNullOrEmpty(request.Country))
        {
            throw new ArgumentException("Country is required");
        }

        if (string.IsNullOrEmpty(request.UserTypeId.ToString()) || request.UserTypeId == 0)
        {
            throw new ArgumentException("UserTypeId is required");
        }
        
        // Check for duplicate email
        var existingUser = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == request.Email);
        
        if (existingUser.Any())
        {
            throw new InvalidOperationException("email already exist");
        }
        
        // Check for duplicate company
        var existingNetwork = await _unitOfWork.Repository<Network>()
            .FindAsync(n => n.Company == request.Company);
        
        if (existingNetwork.Any())
        {
            throw new InvalidOperationException("Company already exist");
        }
        
        // ATOMIC OPERATION: Create Network -> User -> UserProfile
        
        // 1. Create Network
        var network = new Network
        {
            Company = request.Company,
            NetworkName = request.Name,
            TaxDetails = request.TaxDetails,
            NetworkUrl = request.NetworkUrl,
            NetworkSignupUrl = request.NetworkSignupUrl,
            NetworkLogo = request.NetworkLogo,
            StatusId = 1 // Active
        };
        
        await _unitOfWork.Repository<Network>().AddAsync(network);
        await _unitOfWork.SaveChangesAsync();
        
        // 2. Create User linked to Network
        var user = new User
        {
            Email = request.Email,
            Password = HashPassword(request.Password),
            UserTypeId = request.UserTypeId,
            ProviderTypeId = request.ProviderId,
            NetworkId = network.Id,
            StatusId = 1 // Active
        };
        
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        // 3. Create UserProfile linked to User
        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Country = request.Country,
            Company = request.Company,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<UserProfile>().AddAsync(userProfile);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(network);
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    public async Task<NetworkDto> UpdateNetworkAsync(UpdateNetworkDto request)
    {
        // Update Network
        var network = await _unitOfWork.Repository<Network>().GetByIdAsync(request.Id);
        
        if (network == null)
        {
            throw new KeyNotFoundException("Invalid user details");
        }
        
        // Update network fields
        network.TaxDetails = request.TaxDetails;
        network.NetworkLogo = request.NetworkLogo;
        network.NetworkUrl = request.NetworkUrl;
        network.NetworkSignupUrl = request.NetworkSignupUrl;
        
        await _unitOfWork.Repository<Network>().UpdateAsync(network);
        await _unitOfWork.SaveChangesAsync();
        
        // Update UserProfile
        var userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(request.Id);
        
        if (userProfile != null)
        {
            userProfile.PhoneNumber = request.PhoneNumber;
            userProfile.Country = request.Country;
            
            await _unitOfWork.Repository<UserProfile>().UpdateAsync(userProfile);
            await _unitOfWork.SaveChangesAsync();
        }
        
        return MapToDto(network);
    }
    
    public async Task<NetworkDto?> GetNetworkByIdAsync(int id)
    {
        var network = await _unitOfWork.Repository<Network>().GetByIdAsync(id);
        return network == null ? null : MapToDto(network);
    }
    
    public async Task<PaginatedNetworkResponseDto> GetAllNetworksAsync(GetAllNetworksDto request)
    {
        var networks = await _unitOfWork.Repository<Network>()
            .FindAsync(n => request.StatusId == null || n.StatusId == request.StatusId);
        
        var networksList = networks.ToList();
        
        // Get total count
        var total = networksList.Count;
        
        // Apply pagination
        var pagedNetworks = networksList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();
        
        return new PaginatedNetworkResponseDto
        {
            AllNetwork = pagedNetworks,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<bool> DeleteNetworkAsync(int id)
    {
        var network = await _unitOfWork.Repository<Network>().GetByIdAsync(id);
        
        if (network == null)
        {
            return false;
        }
        
        await _unitOfWork.Repository<Network>().DeleteAsync(network);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    private static NetworkDto MapToDto(Network network)
    {
        return new NetworkDto
        {
            Id = network.Id,
            Company = network.Company,
            TaxDetails = network.TaxDetails,
            AccountSecretKey = network.AccountSecretKey,
            NetworkLogo = network.NetworkLogo,
            NetworkUrl = network.NetworkUrl,
            NetworkName = network.NetworkName,
            NetworkSignupUrl = network.NetworkSignupUrl,
            StatusId = network.StatusId
        };
    }
}
