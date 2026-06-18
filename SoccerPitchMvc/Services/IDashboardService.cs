using System.Threading.Tasks;
using System.Collections.Generic;

namespace SoccerPitchMvc.Services;

public interface IDashboardService
{
    Task<int> GetBookingsTodayAsync();
    Task<decimal> GetTotalRevenueAllTimeAsync();
    Task<double> GetOccupancyRateTodayAsync();
    Task<List<(string PitchName, int Count)>> GetTopPitchesAsync(int count = 3);
    Task<List<(string Label, decimal Revenue)>> GetRevenueLast7DaysAsync();
}
