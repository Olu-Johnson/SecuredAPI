using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public EmailTemplateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<EmailTemplateDto> CreateEmailTemplateAsync(CreateEmailTemplateDto request, int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        
        var emailTemplate = new EmailTemplate
        {
            Name = request.Name,
            HtmlContent = request.HtmlContent,
            Description = request.Description,
            Type = request.Type,
            Module = request.Module,
            NetworkId = request.NetworkId ?? user?.NetworkId,
            StatusId = 1, // Active by default
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Repository<EmailTemplate>().AddAsync(emailTemplate);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(emailTemplate);
    }
    
    public async Task<EmailTemplateDto> UpdateEmailTemplateAsync(UpdateEmailTemplateDto request)
    {
        var emailTemplate = await _unitOfWork.Repository<EmailTemplate>().GetByIdAsync(request.Id);
        
        if (emailTemplate == null)
        {
            throw new KeyNotFoundException("Email template not found");
        }
        
        emailTemplate.Name = request.Name;
        emailTemplate.HtmlContent = request.HtmlContent;
        emailTemplate.Description = request.Description;
        emailTemplate.Type = request.Type;
        emailTemplate.Module = request.Module;
        emailTemplate.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Repository<EmailTemplate>().UpdateAsync(emailTemplate);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(emailTemplate);
    }
    
    public async Task<EmailTemplateDto?> GetEmailTemplateByIdAsync(int id)
    {
        var emailTemplate = await _unitOfWork.Repository<EmailTemplate>().GetByIdAsync(id);
        return emailTemplate == null ? null : MapToDto(emailTemplate);
    }
    
    public async Task<PaginatedEmailTemplateResponseDto> GetAllEmailTemplatesAsync(GetAllEmailTemplatesDto request)
    {
        var query = await _unitOfWork.Repository<EmailTemplate>()
            .FindAsync(e => (request.NetworkId == null || e.NetworkId == request.NetworkId) &&
                           (request.StatusId == null || e.StatusId == request.StatusId));
        
        var total = query.Count();
        
        var templates = query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();
        
        return new PaginatedEmailTemplateResponseDto
        {
            AllTemplate = templates,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<bool> DeleteEmailTemplateAsync(int id)
    {
        var emailTemplate = await _unitOfWork.Repository<EmailTemplate>().GetByIdAsync(id);
        
        if (emailTemplate == null)
        {
            return false;
        }
        
        await _unitOfWork.Repository<EmailTemplate>().DeleteAsync(emailTemplate);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    private static EmailTemplateDto MapToDto(EmailTemplate emailTemplate)
    {
        return new EmailTemplateDto
        {
            Id = emailTemplate.Id,
            Name = emailTemplate.Name,
            HtmlContent = emailTemplate.HtmlContent,
            Description = emailTemplate.Description,
            Type = emailTemplate.Type,
            Module = emailTemplate.Module,
            NetworkId = emailTemplate.NetworkId,
            StatusId = emailTemplate.StatusId,
            CreatedAt = emailTemplate.CreatedAt,
            UpdatedAt = emailTemplate.UpdatedAt
        };
    }
}
