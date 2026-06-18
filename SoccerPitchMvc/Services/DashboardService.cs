using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public DashboardService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<int> GetBookingsTodayAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        return await context.Bookings
            .CountAsync(b => b.BookingDate == today && b.Status != 3);
    }

    public async Task<decimal> GetTotalRevenueAllTimeAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Where(b => b.Status == 2)
            .SumAsync(b => b.TotalPrice);
    }

    public async Task<double> GetOccupancyRateTodayAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int bookingsToday = await context.Bookings
            .CountAsync(b => b.BookingDate == today && b.Status != 3);

        int totalActiveSlots = await context.TimeSlots
            .CountAsync(t => context.Pitches.Any(p => p.Id == t.PitchId && p.Status != 2));

        if (totalActiveSlots > 0)
        {
            return Math.Round((double)bookingsToday / totalActiveSlots * 100, 1);
        }
        return 0;
    }

    public async Task<List<(string PitchName, int Count)>> GetTopPitchesAsync(int count = 3)
    {
        using var context = _dbFactory.CreateDbContext();
        var data = await context.Bookings
            .Where(b => b.Status != 3)
            .GroupBy(b => b.Pitch.Name)
            .Select(g => new { PitchName = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(count)
            .ToListAsync();

        return data.Select(x => (x.PitchName, x.Count)).ToList();
    }

    public async Task<List<(string Label, decimal Revenue)>> GetRevenueLast7DaysAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        var datesRange = Enumerable.Range(-6, 7)
            .Select(i => DateOnly.FromDateTime(DateTime.Today.AddDays(i)))
            .ToList();

        var bookingsInLast7Days = await context.Bookings
            .Where(b => b.Status == 2 && b.BookingDate >= datesRange.First() && b.BookingDate <= datesRange.Last())
            .GroupBy(b => b.BookingDate)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
            .ToListAsync();

        return datesRange.Select(d => (
            Label: d.ToString("dd/MM"),
            Revenue: bookingsInLast7Days.FirstOrDefault(b => b.Date == d)?.Revenue ?? 0
        )).ToList();
    }
}
