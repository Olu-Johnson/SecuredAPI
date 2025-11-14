using MyPortal.Application.DTOs;
using MyPortal.Core.Entities;

namespace MyPortal.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto> RegisterAsync(RegisterRequestDto request);
    Task<bool> ValidateTokenAsync(string token);
    Task<ResendTokenResponseDto> ResendTokenAsync(ResendTokenRequestDto request);
    Task<PasswordResetResponseDto> SendForgotPasswordEmailAsync(ForgotPasswordRequestDto request);
    Task<PasswordResetResponseDto> SetPasswordAsync(SetPasswordRequestDto request);
    string GenerateJwtToken(UserDto user);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<PaginatedUserResponseDto> GetAllUsersAsync(GetAllUsersDto request);
    Task<UserDto> UpdateUserAsync(UpdateUserDto request);
    Task<bool> DeleteUserAsync(int id);
}

public interface ICampaignService
{
    Task<CampaignDetailsDto> CreateCampaignAsync(CreateCampaignDto request, int userId);
    Task<CampaignDetailsDto> UpdateCampaignAsync(int id, CreateCampaignDto request);
    Task<CampaignDetailsDto?> GetCampaignByIdAsync(int id);
    Task<PaginatedCampaignResponseDto> GetAllCampaignsAsync(GetAllCampaignsDto request, int userId);
    Task<bool> DeleteCampaignAsync(int id);
    Task<string> ProcessCampaignClickAsync(string promotionLink, string parameters, string ipAddress, string userAgent);
    Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync(object request);
    Task<PaginatedClicksReportResponseDto> GetAllClicksReportsAsync(GetAllClicksReportDto request);
    Task<ServiceResponse<string>> FilterClicksAsync(int networkId, string ipAddress, string browserInfo, string host, Dictionary<string, string> queryParams);
}

public interface IIPValidationService
{
    Task<IPValidationResult> ValidateIPAsync(string ipAddress);
}

public interface IContactService
{
    Task<ContactDto> CreateContactAsync(CreateContactDto request, int userId);
    Task<ContactDto> UpdateContactAsync(UpdateContactDto request, int userId);
    Task<ContactDto?> GetContactByIdAsync(int id);
    Task<PaginatedContactResponseDto> GetAllContactsAsync(GetAllContactsDto request, int userId);
    Task<UploadResultDto> UploadContactsAsync(UploadContactDto request, int userId);
    Task<bool> DeleteContactAsync(int id, int userId);
    Task<ContactDto> BlockContactAsync(int id, int userId);
}

public interface IEmailService
{
    Task<EmailDto> CreateEmailAsync(CreateEmailDto request, int userId);
    Task<EmailDto> UpdateEmailAsync(UpdateEmailDto request, int userId);
    Task<EmailDto?> GetEmailByIdAsync(int id);
    Task<PaginatedEmailResponseDto> GetAllEmailsAsync(GetAllEmailsDto request);
    Task<bool> SaveOfferEmailAsync(SaveOfferEmailDto request, int userId);
    Task<bool> SaveCampaignEmailAsync(SaveCampaignEmailDto request, int userId);
    Task<bool> SendEmailAsync(EmailCampaignDto email);
    Task<bool> SendBulkEmailsAsync(EmailCampaignDto email, List<ContactDto> contacts);
    Task SendSmtpEmailAsync(SmtpSetup smtpSetup, string to, string subject, string htmlBody, string? from = null);
}

public interface INetworkService
{
    Task<NetworkDto> CreateNetworkAsync(CreateNetworkDto request);
    Task<NetworkDto> UpdateNetworkAsync(UpdateNetworkDto request);
    Task<NetworkDto?> GetNetworkByIdAsync(int id);
    Task<PaginatedNetworkResponseDto> GetAllNetworksAsync(GetAllNetworksDto request);
    Task<bool> DeleteNetworkAsync(int id);
}

public interface IGroupService
{
    Task<GroupDto> CreateGroupAsync(CreateGroupDto request, int userId);
    Task<GroupDto> UpdateGroupAsync(UpdateGroupDto request, int userId);
    Task<GroupDto?> GetGroupByIdAsync(int id);
    Task<PaginatedGroupResponseDto> GetAllGroupsAsync(GetAllGroupsDto request, int userId);
    Task<bool> DeleteGroupAsync(int id, int userId);
}

public interface IContactGroupService
{
    Task<ContactGroupDto> CreateContactGroupAsync(CreateContactGroupDto request, int userId);
    Task<ContactGroupDto> UpdateContactGroupAsync(UpdateContactGroupDto request);
    Task<ContactGroupDto?> GetContactGroupByIdAsync(int id);
    Task<PaginatedContactGroupResponseDto> GetAllContactGroupsAsync(GetAllContactGroupsDto request, int userId);
    Task<bool> DeleteContactGroupAsync(int id);
}

public interface IUserProfileService
{
    Task<UserProfileDto2> CreateUserProfileAsync(CreateUserProfileDto request, int userId);
    Task<UserProfileDto2> UpdateUserProfileAsync(UpdateUserProfileDto request, int userId);
    Task<UserProfileDto2?> GetUserProfileByIdAsync(int id);
    Task<IEnumerable<UserProfileDto2>> GetAllUserProfilesAsync(int userId);
}

public interface ISmtpSetupService
{
    Task<SmtpSetupDto> CreateSmtpSetupAsync(CreateSmtpSetupDto request, int userId);
    Task<SmtpSetupDto> UpdateSmtpSetupAsync(UpdateSmtpSetupDto request);
    Task<SmtpSetupDto?> GetSmtpSetupByIdAsync(int id);
    Task<PaginatedSmtpSetupResponseDto> GetAllSmtpSetupsAsync(int userId);
    Task<bool> DeleteSmtpSetupAsync(int id);
    Task<SmtpSetupDto?> GetSmtpSetupByNetworkIdAsync(int networkId);
}

public interface ISecuritySetupService
{
    Task<SecuritySetupDto> CreateSecuritySetupAsync(CreateSecuritySetupDto request, int userId);
    Task<SecuritySetupDto> UpdateSecuritySetupAsync(UpdateSecuritySetupDto request);
    Task<SecuritySetupDto> UpdateIsMonitorAsync(UpdateIsMonitorDto request);
    Task<SecuritySetupDto?> GetSecuritySetupByIdAsync(int id);
    Task<SecuritySetupDto?> GetSecuritySetupByNetworkIdAsync(int networkId);
    Task<PaginatedSecuritySetupResponseDto> GetAllSecuritySetupsAsync(GetAllSecuritySetupsDto request);
}

public interface IEmailTemplateService
{
    Task<EmailTemplateDto> CreateEmailTemplateAsync(CreateEmailTemplateDto request, int userId);
    Task<EmailTemplateDto> UpdateEmailTemplateAsync(UpdateEmailTemplateDto request);
    Task<EmailTemplateDto?> GetEmailTemplateByIdAsync(int id);
    Task<PaginatedEmailTemplateResponseDto> GetAllEmailTemplatesAsync(GetAllEmailTemplatesDto request);
    Task<bool> DeleteEmailTemplateAsync(int id);
}
