using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Application.Utilities;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyPortal.Application.Constants;

namespace MyPortal.Application.Services;

public class EmailService : IEmailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TemplateRenderer _templateRenderer;
    
    public EmailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _templateRenderer = new TemplateRenderer();
    }
    
    public async Task<EmailDto> CreateEmailAsync(CreateEmailDto request, int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        
        var email = new Email
        {
            From = request.From,
            To = request.To,
            Subject = request.Subject,
            Message = request.Message,
            RetryCount = 0,
            StatusId = 1,
            NetworkId = request.NetworkId ?? user?.NetworkId
        };
        
        await _unitOfWork.Repository<Email>().AddAsync(email);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(email);
    }
    
    public async Task<EmailDto> UpdateEmailAsync(UpdateEmailDto request, int userId)
    {
        var email = await _unitOfWork.Repository<Email>().GetByIdAsync(request.Id);
        
        if (email == null)
        {
            throw new KeyNotFoundException("Email not found");
        }
        
        email.From = request.From;
        email.To = request.To;
        email.Subject = request.Subject;
        email.Message = request.Message;
        
        await _unitOfWork.Repository<Email>().UpdateAsync(email);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(email);
    }
    
    public async Task<EmailDto?> GetEmailByIdAsync(int id)
    {
        var email = await _unitOfWork.Repository<Email>().GetByIdAsync(id);
        return email == null ? null : MapToDto(email);
    }
    
    public async Task<PaginatedEmailResponseDto> GetAllEmailsAsync(GetAllEmailsDto request)
    {
        var emails = await _unitOfWork.Repository<Email>()
            .FindAsync(e => (request.NetworkId == null || e.NetworkId == request.NetworkId) &&
                           (request.StatusId == null || e.StatusId == request.StatusId));
        
        var emailList = emails.ToList();
        var pagedEmails = emailList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        return new PaginatedEmailResponseDto
        {
            AllEmail = pagedEmails.Select(MapToDto).ToList(),
            Page = request.PageIndex,
            Total = emailList.Count
        };
    }
    
    public async Task<bool> SaveOfferEmailAsync(SaveOfferEmailDto request, int userId)
    {
        try
        {
            // Get UserProfile with User details
            var userProfiles = await _unitOfWork.Repository<UserProfile>()
                .FindAsync(up => up.UserId == userId);
            var userProfile = userProfiles.FirstOrDefault();
            
            if (userProfile == null)
            {
                throw new KeyNotFoundException("User profile not found");
            }
            
            // Get User to access network
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            
            // Get Network details
            var network = user.NetworkId.HasValue 
                ? await _unitOfWork.Repository<Network>().GetByIdAsync(user.NetworkId.Value)
                : null;
            
            if (network == null)
            {
                throw new KeyNotFoundException("Network not found");
            }
            
            // Get Email Template by name
            var templates = await _unitOfWork.Repository<EmailTemplate>()
                .FindAsync(t => t.Name == request.TemplateName && t.StatusId == 1); // Active templates only
            var template = templates.FirstOrDefault();
            
            if (template == null)
            {
                throw new KeyNotFoundException($"Email template '{request.TemplateName}' not found");
            }
            
            // Get Group with Contacts
            var group = await _unitOfWork.Repository<Group>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new KeyNotFoundException("No contacts found for this group");
            }
            
            // Get contacts in this group
            var contactGroups = await _unitOfWork.Repository<ContactGroup>()
                .FindAsync(cg => cg.GroupId == request.GroupId);
            
            var contactIds = contactGroups.Select(cg => cg.ContactId ?? 0).Where(id => id != 0).ToList();
            var contacts = await _unitOfWork.Repository<Contact>()
                .FindAsync(c => contactIds.Contains(c.Id));
            
            var contactsList = contacts.ToList();
            
            if (!contactsList.Any())
            {
                throw new KeyNotFoundException("No contacts found for this group");
            }
            
            // Parse offers JSON once - deserialize to ExpandoObject for dynamic property access
            List<dynamic> offersList;
            try
            {
                var offersDict = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(request.Offers) ?? new List<Dictionary<string, JsonElement>>();
                
                // Convert dictionaries to ExpandoObjects for dynamic property access in Razor
                offersList = offersDict.Select(dict =>
                {
                    var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                    foreach (var kvp in dict)
                    {
                        expando[kvp.Key] = kvp.Value.ValueKind == JsonValueKind.String 
                            ? kvp.Value.GetString() 
                            : kvp.Value.ToString();
                    }
                    return (dynamic)expando;
                }).ToList();
            }
            catch
            {
                offersList = new List<dynamic>();
            }
            
            // Create email records for each contact
            foreach (var contact in contactsList)
            {
                // Build model for template rendering
                dynamic model = new System.Dynamic.ExpandoObject();
                model.offers = offersList;
                model.note = request.Note ?? "";
                model.firstname = contact.FirstName ?? "";
                model.networklogo = network.NetworkLogo ?? "#";
                model.networkurl = network.NetworkUrl ?? "#";
                model.networkname = network.Company ?? "";
                model.networksignupurl = network.NetworkSignupUrl ?? "#";
                model.address = userProfile.Address ?? "";
                
                // Render template with contact data
                var renderedMessage = await _templateRenderer.RenderAsync(template.HtmlContent, model);
                
                var email = new Email
                {
                    From = $"\"{network.Company}\" <{user.Email}>",
                    To = contact.Email ?? "",
                    Subject = request.Subject,
                    Message = renderedMessage,
                    RetryCount = 0,
                    StatusId = StatusConstants.NotSent, // Pending status (3rd record in Statuses table)
                    NetworkId = user.NetworkId
                };
                
                await _unitOfWork.Repository<Email>().AddAsync(email);
            }
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    
    public async Task<bool> SaveCampaignEmailAsync(SaveCampaignEmailDto request, int userId)
    {
        // Get campaign details
        var campaign = await _unitOfWork.Repository<CampaignDetails>().GetByIdAsync(request.CampaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException("campaign detail not found");
        }
        
        // Get UserProfile with User details
        var userProfiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(up => up.UserId == userId);
        var userProfile = userProfiles.FirstOrDefault();
        
        if (userProfile == null)
        {
            throw new KeyNotFoundException("User profile not found");
        }
        
        // Get User to access network
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Get Network details
        var network = user.NetworkId.HasValue 
            ? await _unitOfWork.Repository<Network>().GetByIdAsync(user.NetworkId.Value)
            : null;
        
        if (network == null)
        {
            throw new KeyNotFoundException("Network not found");
        }
        
        // Get SMTP Setup
        var smtpSetups = await _unitOfWork.Repository<SmtpSetup>()
            .FindAsync(s => s.NetworkId == user.NetworkId);
        var smtpSetup = smtpSetups.FirstOrDefault();
        
        // Get Group with Contacts
        // if (!int.TryParse(request.GroupId, out var groupId))
        // {
        //     throw new ArgumentException("Invalid group ID");
        // }
        
        var group = await _unitOfWork.Repository<Group>().GetByIdAsync(request.GroupId);
        if (group == null)
        {
            throw new KeyNotFoundException("No contacts found for this group");
        }
        
        // Get contacts in this group
        var contactGroups = await _unitOfWork.Repository<ContactGroup>()
            .FindAsync(cg => cg.GroupId == request.GroupId);
        
        var contactIds = contactGroups.Select(cg => cg.ContactId ?? 0).Where(id => id != 0).ToList();
        var contacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => contactIds.Contains(c.Id));
        
        var contactsList = contacts.ToList();
        
        if (!contactsList.Any())
        {
            throw new KeyNotFoundException("No contacts found for this group");
        }
        
        // Validate campaign has content
        if (string.IsNullOrEmpty(campaign.Content))
        {
            throw new InvalidOperationException("Campaign content is empty");
        }

        // Create email records for each contact with personalized campaign link
        foreach (var contact in contactsList)
        {
            // Create personalized campaign link with Base64 encoded parameters
            // Format: /cpg/{promotionLink}/{Base64(email-firstname)}
            var parameters = $"{contact.Email}-{contact.FirstName}";
            var base64Parameters = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(parameters));
            var personalizedLink = $"{request.Protocol ?? "https"}://{request.Host ?? "localhost"}/cpg/{campaign.PromotionLink}/{base64Parameters}";
            
            // Replace placeholders in campaign content with actual values
            var message = campaign.Content
                .Replace("{{firstName}}", contact.FirstName ?? "")
                .Replace("{{lastName}}", contact.LastName ?? "")
                .Replace("{{link}}", personalizedLink)
                .Replace("{{company}}", network.Company ?? "")
                .Replace("{{networkName}}", network.NetworkName ?? "");
            
            var fromEmail = smtpSetup?.Username ?? user.Email;
            
            var email = new Email
            {
                From = $"\"{network.Company}\" <{fromEmail}>",
                To = contact.Email ?? "",
                Subject = request.Subject,
                Message = message,
                RetryCount = 0,
                StatusId = StatusConstants.NotSent, // Pending status (3rd record in Statuses table)
                NetworkId = user.NetworkId
            };
            
            await _unitOfWork.Repository<Email>().AddAsync(email);
        }
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> SendEmailAsync(EmailCampaignDto email)
    {
        // TODO: Implement actual SMTP sending using SmtpSetup
        // This is a placeholder implementation
        return await Task.FromResult(true);
    }
    
    public async Task<bool> SendBulkEmailsAsync(EmailCampaignDto email, List<ContactDto> contacts)
    {
        // TODO: Implement bulk email sending
        // This is a placeholder implementation
        return await Task.FromResult(true);
    }
    
    private static EmailDto MapToDto(Email email)
    {
        return new EmailDto
        {
            Id = email.Id,
            From = email.From ?? "",
            To = email.To ?? "",
            Subject = email.Subject ?? "",
            Message = email.Message ?? "",
            RetryCount = email.RetryCount ?? 0,
            StatusId = email.StatusId,
            NetworkId = email.NetworkId ?? 0
        };
    }

    public async Task SendSmtpEmailAsync(SmtpSetup smtpSetup, string to, string subject, string htmlBody, string? from = null)
    {
        var message = new MimeMessage();
        
        // Set from address
        var fromAddress = from ?? smtpSetup.FromEmail ?? smtpSetup.Username ?? "noreply@example.com";
        var fromName = smtpSetup.FromName ?? "No Reply";
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        
        // Set to address
        message.To.Add(MailboxAddress.Parse(to));
        
        // Set subject
        message.Subject = subject;
        
        // Create message body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();
        
        // Send email
        using var client = new SmtpClient();
        
        try
        {
            // Determine security options based on encryption setting
            var secureSocketOptions = smtpSetup.Encryption?.ToLower() switch
            {
                "ssl" => SecureSocketOptions.SslOnConnect,
                "tls" => SecureSocketOptions.StartTls,
                "starttls" => SecureSocketOptions.StartTls,
                "none" => SecureSocketOptions.None,
                _ => smtpSetup.IsSecure ? SecureSocketOptions.StartTls : SecureSocketOptions.None
            };
            
            await client.ConnectAsync(
                smtpSetup.Host ?? "localhost",
                smtpSetup.Port ?? 587,
                secureSocketOptions);
            
            // Authenticate if credentials provided
            if (!string.IsNullOrEmpty(smtpSetup.Username) && !string.IsNullOrEmpty(smtpSetup.Password))
            {
                await client.AuthenticateAsync(smtpSetup.Username, smtpSetup.Password);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }
}
