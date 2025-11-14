namespace MyPortal.Application.DTOs;

public class DashboardStatisticsDto
{
    public int NetworkId { get; set; }
    public string NetworkName { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int DraftCampaigns { get; set; }
    public int SentCampaigns { get; set; }
    public int TotalContacts { get; set; }
    public int ActiveContacts { get; set; }
    public int TotalGroups { get; set; }
    public int TotalEmails { get; set; }
    public int EmailsSent { get; set; }
    public int EmailsFailed { get; set; }
    public int EmailsPending { get; set; }
    public int TotalCampaignReports { get; set; }
    public int TotalSmtpSetups { get; set; }
    public int TotalGuarantors { get; set; }
    public int TotalLeaves { get; set; }
    public int PendingLeaves { get; set; }
    public int ApprovedLeaves { get; set; }
}

public class AllNetworksDashboardDto
{
    public List<DashboardStatisticsDto> NetworkStatistics { get; set; } = new();
    public DashboardStatisticsDto OverallStatistics { get; set; } = new();
}
