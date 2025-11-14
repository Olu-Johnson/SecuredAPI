using MyPortal.Application.Constants;
using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatisticsDto> GetNetworkStatisticsAsync(int networkId)
    {
        // Verify network exists
        var network = await _unitOfWork.Repository<Network>().GetByIdAsync(networkId);
        if (network == null)
        {
            throw new KeyNotFoundException($"Network with ID {networkId} not found");
        }

        var statistics = new DashboardStatisticsDto
        {
            NetworkId = networkId,
            NetworkName = network.NetworkName
        };

        // Get all users for this network
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.NetworkId == networkId);

        statistics.TotalUsers = users.Count();
        statistics.ActiveUsers = users.Count(u => u.StatusId == StatusConstants.Active);
        statistics.InactiveUsers = users.Count(u => u.StatusId == StatusConstants.Inactive);

        // Get all campaigns for this network
        var campaigns = await _unitOfWork.Repository<CampaignDetails>()
            .FindAsync(c => c.NetworkId == networkId);

        statistics.TotalCampaigns = campaigns.Count();
        statistics.ActiveCampaigns = campaigns.Count(c => c.StatusId == StatusConstants.Active);
        statistics.DraftCampaigns = campaigns.Count(c => c.StatusId == StatusConstants.Draft);
        statistics.SentCampaigns = campaigns.Count(c => c.StatusId == StatusConstants.Sent);

        // Get all contacts for this network
        var contacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => c.NetworkId == networkId);

        statistics.TotalContacts = contacts.Count();
        statistics.ActiveContacts = contacts.Count(c => c.StatusId == StatusConstants.Active);

        // Get all groups for this network
        var groups = await _unitOfWork.Repository<Group>()
            .FindAsync(g => g.NetworkId == networkId);
        statistics.TotalGroups = groups.Count();

        // Get all emails for this network
        var emails = await _unitOfWork.Repository<Email>()
            .FindAsync(e => e.NetworkId == networkId);

        statistics.TotalEmails = emails.Count();
        statistics.EmailsSent = emails.Count(e => e.StatusId == StatusConstants.EmailSent);
        statistics.EmailsFailed = emails.Count(e => e.StatusId == StatusConstants.EmailFailed);
        statistics.EmailsPending = emails.Count(e => e.StatusId == StatusConstants.Pending);

        // Get campaign reports for this network
        var campaignReports = await _unitOfWork.Repository<CampaignReport>()
            .FindAsync(cr => cr.NetworkId == networkId);
        statistics.TotalCampaignReports = campaignReports.Count();

        // Get SMTP setups for this network
        var smtpSetups = await _unitOfWork.Repository<SmtpSetup>()
            .FindAsync(s => s.NetworkId == networkId);
        statistics.TotalSmtpSetups = smtpSetups.Count();

        // Get network user IDs for related queries
        var networkUserIds = users.Select(u => u.Id).ToList();

        // Get guarantors for users in this network
        var guarantors = await _unitOfWork.Repository<Guarantor>()
            .FindAsync(g => networkUserIds.Contains(g.UserId));
        statistics.TotalGuarantors = guarantors.Count();

        // Get leaves for users in this network
        var leaves = await _unitOfWork.Repository<Leave>()
            .FindAsync(l => networkUserIds.Contains(l.UserId));

        statistics.TotalLeaves = leaves.Count();
        statistics.PendingLeaves = leaves.Count(l => l.Status != null && l.Status.ToLower() == "pending");
        statistics.ApprovedLeaves = leaves.Count(l => l.Status != null && l.Status.ToLower() == "approved");

        return statistics;
    }

    public async Task<AllNetworksDashboardDto> GetAllNetworksStatisticsAsync()
    {
        var result = new AllNetworksDashboardDto();

        // Get all active networks
        var networks = await _unitOfWork.Repository<Network>()
            .FindAsync(n => n.StatusId == StatusConstants.Active);

        // Get statistics for each network
        foreach (var network in networks)
        {
            var networkStats = await GetNetworkStatisticsAsync(network.Id);
            result.NetworkStatistics.Add(networkStats);
        }

        // Calculate overall statistics
        var overallStats = new DashboardStatisticsDto
        {
            NetworkId = 0,
            NetworkName = "All Networks",
            TotalUsers = result.NetworkStatistics.Sum(s => s.TotalUsers),
            ActiveUsers = result.NetworkStatistics.Sum(s => s.ActiveUsers),
            InactiveUsers = result.NetworkStatistics.Sum(s => s.InactiveUsers),
            TotalCampaigns = result.NetworkStatistics.Sum(s => s.TotalCampaigns),
            ActiveCampaigns = result.NetworkStatistics.Sum(s => s.ActiveCampaigns),
            DraftCampaigns = result.NetworkStatistics.Sum(s => s.DraftCampaigns),
            SentCampaigns = result.NetworkStatistics.Sum(s => s.SentCampaigns),
            TotalContacts = result.NetworkStatistics.Sum(s => s.TotalContacts),
            ActiveContacts = result.NetworkStatistics.Sum(s => s.ActiveContacts),
            TotalGroups = result.NetworkStatistics.Sum(s => s.TotalGroups),
            TotalEmails = result.NetworkStatistics.Sum(s => s.TotalEmails),
            EmailsSent = result.NetworkStatistics.Sum(s => s.EmailsSent),
            EmailsFailed = result.NetworkStatistics.Sum(s => s.EmailsFailed),
            EmailsPending = result.NetworkStatistics.Sum(s => s.EmailsPending),
            TotalCampaignReports = result.NetworkStatistics.Sum(s => s.TotalCampaignReports),
            TotalSmtpSetups = result.NetworkStatistics.Sum(s => s.TotalSmtpSetups),
            TotalGuarantors = result.NetworkStatistics.Sum(s => s.TotalGuarantors),
            TotalLeaves = result.NetworkStatistics.Sum(s => s.TotalLeaves),
            PendingLeaves = result.NetworkStatistics.Sum(s => s.PendingLeaves),
            ApprovedLeaves = result.NetworkStatistics.Sum(s => s.ApprovedLeaves)
        };

        result.OverallStatistics = overallStats;

        return result;
    }
}
