using MyPortal.Application.DTOs;

namespace MyPortal.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatisticsDto> GetNetworkStatisticsAsync(int networkId);
    Task<AllNetworksDashboardDto> GetAllNetworksStatisticsAsync();
}
